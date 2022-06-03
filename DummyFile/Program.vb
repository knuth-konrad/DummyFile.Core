Imports System
Imports System.IO
Imports System.IO.Path
Imports System.Reflection
Imports System.Threading

Imports libBAUtilCore
Imports libBAUtilCore.ConsoleHelper
Imports libBAUtilCore.FilesystemHelper
Imports libBAUtilCore.StringHelper
Imports libBAUtilCore.Utils.Args

Module Program

   Public Const COMPANY_NAME As String = "BasicAware"

   ' Parameters
   Public Const PARAM_MIN = 2 ' # of mandatory parameters

   Structure FilePropertyTYPEE
      Public FilePath As String
      Public FileExtension As String
      Public FileContents As String
   End Structure

   Sub Main(args As String())

      ' Application intro
      ConsoleHelper.AppIntro(Assembly.GetEntryAssembly())
      ConsoleHelper.AppCopyright()

      ' ** Parse the command line parameters **

      ' All valid parameters
      ' /n  - Number of files to create (mandatory).
      ' /s  - File size of each file (mandatory).
      ' /f  - Folder in which the file(s) should be created.
      ' /lf - Create files with line feeds (CrLf).
      ' /ll - Line length (number of characters). Can only be used in conjunction with /lf.
      ' /fe - File extension. Defaults to 'tmp'.

      Dim paramListAll As New List(Of String)({"n", "s", "f", "fe", "lf", "ll"})
      Dim paramListMandatory As New List(Of String)({"n", "s"})

      Dim cmd As CmdArgs = New CmdArgs(paramListAll)
      cmd.Initialize()

      ' * Validate what we have
      ' Too few parameters?
      If cmd.ParametersCount < PARAM_MIN Then
         WriteIndent(String.Format("!!! Too few parameters. Mandatory parameters: {0}, parameters supplied: {1}", PARAM_MIN, cmd.ParametersCount.ToString), 2)
         ShowHelp()
         Exit Sub
      End If

      ' Missing mandatory parameter?
      If cmd.HasParameter(paramListMandatory) = False Then
         WriteIndent("!!! Mandatory parameter(s) missing.", 2)
         ShowHelp()
         Exit Sub
      End If

      ' Invalid parameter(s)?
      ' ToDo: Implement method CmdArgs.ValidateParameters. Determines if each parameter passed is a valid parameter

      ' * Let's get the actual parameter values
      ' # of files to create
      Dim lFiles As UInt32

      Try
         lFiles = CType(cmd.GetValueByName("n"), UInt32)
      Catch ex As Exception
         WriteIndent(String.Format("!!! Invalid value for parameter 'n': {0}", cmd.GetValueByName("n").ToString), 2)
         ShowHelp()
         Exit Sub
      End Try

      ' Size of each file, sSize = actual size, sUnit = size of lUnit, e.g. "kb", lTimes = unit multiplier, e.g. "kb" = 1024
      Dim sTemp As String = String.Empty
      Dim sSize As String = String.Empty, sUnit As String = String.Empty
      Dim lTimes As UInt64 = 0

      Try
         sTemp = CType(cmd.GetValueByName("s"), String)
      Catch ex As Exception
         WriteIndent(String.Format("!!! Invalid value for parameter 's': {0}", cmd.GetValueByName("s").ToString), 2)
         ShowHelp()
         Exit Sub
      End Try

      sUnit = Right(sTemp, 2).ToLower

      Select Case sUnit
         Case "kb"
            'sSize = Left(sTemp, sTemp.Length - (InStr(sTemp, "kb") - 1))
            sSize = sTemp.Replace(sUnit, String.Empty)
            lTimes = 1024
         Case "mb"
            'sSize = Left(sTemp, sTemp.Length - (InStr(sTemp, "mb") - 1))
            sSize = sTemp.Replace(sUnit, String.Empty)
            lTimes = CType(1024 ^ 2, UInt64)
         Case "gb"
            'sSize = Left(sTemp, sTemp.Length - (InStr(sTemp, "gb") - 1))
            sSize = sTemp.Replace(sUnit, String.Empty)
            lTimes = CType(1024 ^ 3, UInt64)
         Case Else
            sSize = GetNumericPart(sTemp)
            lTimes = 1
            sUnit = "byte"
      End Select

      ' Remember the original value for size
      Dim lOriginalSize As UInt64 = 0

      Try
         lOriginalSize = Convert.ToUInt64(sSize)
      Catch ex As OverflowException
         WriteIndent(String.Format("!!! {0} is outside the range of the UInt64 type: ", sSize), 2)
         ShowHelp()
         Exit Sub
      Catch e As FormatException
         WriteIndent(String.Format("!!! The {0} value '{1}' is not in a recognizable format.", sSize.GetType().Name, sSize), 2)
         ShowHelp()
         Exit Sub
      End Try

      ' Console.WriteLine("sSize: {0}, sUnit: {1}", sSize, sUnit)

      ' Actual file size in bytes
      Dim lRealSize As Int32 = CType(lOriginalSize * lTimes, Int32)

      ' Do the values of the mandatory parameters make sense?
      If lFiles < 1 OrElse lRealSize < 1 Then
         WriteIndent(String.Format("!!! Invalid value for at least one parameter: {0}, {1}",
                                   cmd.GetParameterByName("n").OriginalParameter, cmd.GetParameterByName("s").OriginalParameter), 2)
         ShowHelp()
         Exit Sub
      End If


      ' *** Optional parameters
      ' ** Location of file creation
      If cmd.HasParameter("f") = True Then
         sTemp = cmd.GetValueByName("f").ToString.Trim.Replace(vbQuote, String.Empty)
      Else
         ' Current folder is the default location
         If System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(Runtime.InteropServices.OSPlatform.Windows) Then
            sTemp = ".\"
         Else
            sTemp = "./"
         End If
      End If

      If FolderExists(sTemp) = False Then
         WriteIndent(String.Format("!!! Folder doesn't exist: {0}", cmd.GetValueByName("f").ToString), 2)
         ShowHelp()
         Exit Sub
      End If

      Dim sPath As String = sTemp


      ' ** Create a text file with line breaks?
      Dim bolAddLineBreaks As Boolean = cmd.HasParameter("lf")


      ' ** Line length?
      Dim lLineLength As UInt32 = 0
      If cmd.HasParameter("ll") = True Then
         sTemp = cmd.GetValueByName("ll").ToString

         Try
            lLineLength = Convert.ToUInt32(sTemp)
         Catch ex As OverflowException
            WriteIndent(String.Format("!!! Invalid line length: {0} is outside the range of the UInt32 type: ", sTemp), 2)
            ShowHelp()
            Exit Sub
         Catch e As FormatException
            WriteIndent(String.Format("!!!  Invalid line length: the {0} value '{1}' is not in a recognizable format.", sTemp.GetType().Name, sTemp), 2)
            ShowHelp()
            Exit Sub
         End Try
      End If

      ' ** File extension?
      ' 'tmp' is the default extension
      Dim sFileExt As String = "tmp"
      If cmd.HasParameter("fe") = True Then
         sTemp = cmd.GetValueByName("fe").ToString.Trim.Replace(vbQuote, String.Empty)
         If sTemp.Length > 0 Then
            sFileExt = sTemp
         End If
      End If

      ' Determine if it's a relative or absolute path, i.e. .\MyFolder or C:\MyFolder and/or a UNC share
      Dim sPathFull As String = sPath

      Try
         sPathFull = GetFullPath(sPathFull)
      Catch ex As Exception
         ' We checked that the folder exists, so in theory this shouldn't happen
         WriteIndent(String.Format("!!!  Unable to resolve full path for {0}", sPath), 2)
      End Try

      ' Console.WriteLine("Full path: {0}", sPathFull)

      ' *** Echo the CLI parameters
      Console.WriteLine("# of files    : {0}", lFiles.ToString)
      Console.WriteLine("File size     : {0} {1}", lOriginalSize.ToString, sUnit.ToUpper)
      Console.WriteLine("File extension: {0}", sFileExt)
      Console.Write("Folder        : {0}", sPath)
      ' If path is a relative path, display the full path also
      If (NormalizePath(sPath)).ToLower = NormalizePath(sPathFull).ToLower Then
         Console.WriteLine("")
      Else
         Console.WriteLine(" ({0})", sPathFull)
      End If
      Console.WriteLine("Add line feed : {0}", bolAddLineBreaks.ToString)
      If lLineLength > 0 Then
         Console.WriteLine("Line length   : {0}", lLineLength.ToString)
      End If


      ' *** Prepare the file content
      Dim sContent As String = String.Empty

      BlankLine()
      Console.Write("Preparing file contents ... ")

      Dim Execution_Start As New Stopwatch
      Execution_Start.Start()

      Try
         sContent = CreateRandomFileContent(lRealSize, bolAddLineBreaks, lLineLength)
      Catch ex As ArgumentOutOfRangeException
         WriteIndent("!!! Can't generate a file of this size, aborting ...", 2)
         Exit Sub
      Catch ex As OverflowException
         WriteIndent("!!! Can't generate a file of this size, aborting ...", 2)
         Exit Sub
      Catch ex As Exception
         WriteIndent("!!! Can't generate a file of this size, aborting ...", 2)
         Exit Sub
      End Try

      Console.WriteLine(String.Format("done (in {0} sec.).", Execution_Start.Elapsed.TotalSeconds.ToString))

      Dim o As New FilePropertyTYPEE

      With o
         .FilePath = sPathFull
         .FileExtension = sFileExt
         .FileContents = sContent
      End With

      Console.Write("Creating {0} file(s) ... ", lFiles)
      Execution_Start.Start()

      Dim thd As Thread
      For i As UInt32 = 1 To lFiles
         thd = New Thread(AddressOf ThdCreateFile)
         thd.Start(o)
      Next

      If Not thd Is Nothing Then
         If thd.ThreadState <> ThreadState.Stopped Then
            thd.Join()
         End If
      End If

      Thread.Sleep(1000)

      Console.WriteLine(String.Format("done (in {0} sec.).", Execution_Start.Elapsed.TotalSeconds.ToString))

      ' AnyKey()


   End Sub

   ''' <summary>
   ''' Create a file with a random file name and fill it with the previously created random (text) content.
   ''' </summary>
   ''' <param name="data">Random file creation location</param>
   Public Async Sub ThdCreateFile(ByVal data As Object)

      Dim o As FilePropertyTYPEE
      Dim sFile As String = String.Empty

      o = CType(data, FilePropertyTYPEE)

      Do
         ' Make sure the file doesn't already exist
         sFile = Path.GetFileNameWithoutExtension(Path.GetRandomFileName())
         sFile = NormalizePath(o.FilePath) & sFile & "." & o.FileExtension
         ' Console.WriteLine("File name: {0}", sFile)
      Loop Until FileExists(sFile) = False

      Using txtStream As New System.IO.StreamWriter(sFile)
         With txtStream
            Await .WriteAsync(o.FileContents)
            Await txtStream.FlushAsync()
            .Close()
         End With
      End Using

   End Sub

   ''' <summary>
   ''' Create random plain text-like content
   ''' </summary>
   ''' <param name="lRealSize">Size in bytes</param>
   ''' <param name="bolAddLineBreaks">Artificially add line breaks</param>
   ''' <param name="lLineLength">Line length, if <paramref name="bolAddLineBreaks"/> is <see langword="true"/>.</param>
   ''' <returns>Plan text (file)-like string</returns>
   Public Function CreateRandomFileContent(ByVal lRealSize As Int32, ByVal bolAddLineBreaks As Boolean,
                                           ByVal lLineLength As UInt32) As String

      Dim r As Random
      r = New Random(DateTime.UtcNow.Millisecond)

      Dim lRows As UInt32 = 1

      ' Figure out how to format the output
      If bolAddLineBreaks = True AndAlso lLineLength > 0 Then
         ' Creating a file with line breaks only makes sense if a length is specified
         lRows = CType(lRealSize \ lLineLength, UInt32)
         lRealSize += CType(CType(lRealSize \ lLineLength, UInt32) * vbNewLine.Length, Int32)
      End If

      ' Add additional space for line breaks
      Dim sLine As String = String.Empty
      If lRows > 1 Then
         sLine = Space(lLineLength)
      Else
         sLine = Space(lRealSize)
         lLineLength = CType(lRealSize, UInt32)
      End If

      Dim sContent As String = String.Empty

      Dim seed As Int32 = -1
      For i As Int32 = 1 To CType(lRows, Int32)
         sLine = CreateRandomStringOfLength(lLineLength, seed, bolAddLineBreaks)
         sContent &= sLine
      Next

      ' Any remainder we need to add?
      If bolAddLineBreaks = True AndAlso lLineLength > 0 Then
         If (sContent.Length + (lRows * vbNewLine.Length)) < (lRows * (lLineLength + vbNewLine.Length)) Then
            Dim lRemainder As UInt32 = CType(lRows * (lLineLength + vbNewLine.Length) - sContent.Length + (lRows * vbNewLine.Length), UInt32)
            sContent &= CreateRandomStringOfLength(lRemainder, seed, bolAddLineBreaks)
         End If
      Else
         If sContent.Length < lLineLength Then
            sContent &= CreateRandomStringOfLength(CType(lLineLength - sContent.Length, UInt32), seed, bolAddLineBreaks)
         End If

      End If

      Return sContent

   End Function

   ''' <summary>
   ''' Shows DummyFile's usage and syntax help.
   ''' </summary>
   Public Sub ShowHelp()

      Dim s As String = String.Empty

      Using o As New CmdArgs
         s = o.DelimiterArgs
      End Using

      BlankLine()
      Console.WriteLine("DummyFile lets you easily create a number of dummy files for various testing scenarios.")
      Console.WriteLine("The files are generated from random (plain text aka 'human readable') characters.")
      Console.WriteLine("Per default, the file's contents is generated as one 'big blob'.")
      Console.WriteLine("However, you can specify to add line feeds (actually line feed plus carriage return)")
      Console.WriteLine("to simulate 'proper text files'.")

      BlankLine()
      Console.WriteLine("Usage:")
      Console.WriteLine("DummyFile {0}n=<No. of files> {0}s=<file size> [{0}f=<folder to create files in>] [{0}lf] [{0}ll=<No. of characters per line>] [{0}fe=<file extension>]", s)
      Console.WriteLine("")
      Console.WriteLine("     i.e.: DummyFile {0}n=10 {0}s=12MB", s)
      Console.WriteLine("              - Create 10 files (in the current folder) with a size of 12MB each, do not add line feed(s).")
      Console.WriteLine("           DummyFile {0}n=10 {0}s=12MB {0}f=c:\temp", s)
      Console.WriteLine("              - Create 10 files in the folder c:\temp with a size of 12MB each, do not add line feed(s).")
      Console.WriteLine("           DummyFile {0}n=10 {0}s=12MB {0}f=c:\temp {0}lf", s)
      Console.WriteLine("              - Create 10 files in the folder c:\temp with a size of 12MB each, add line feed(s).")
      Console.WriteLine("                Line length defaults to 80 characters.")
      Console.WriteLine("           DummyFile {0}n=10 {0}s=12MB {0}f=c:\temp {0}lf {0}ll=72", s)
      Console.WriteLine("              - Create 10 files in the folder c:\temp with a size of 12MB each, add line feed(s).")
      Console.WriteLine("                Line length should be 72 characters.")
      Console.WriteLine("           DummyFile {0}n=10 {0}s=12MB {0}f=c: \temp {0}lf {0}ll=72 {0}fe=txt", s)
      Console.WriteLine("              - Create 10 files with the file extension 'txt' in the folder c:\temp with a size of 12MB each,")
      Console.WriteLine("                add line feed(s). Line length should be 72 characters.")

      BlankLine()
      Console.WriteLine("Parameters")
      Console.WriteLine("----------")
      Console.WriteLine("{0}n  - Number of files to create (mandatory).", s)
      Console.WriteLine("{0}s  - File size of each file (mandatory).", s)
      Console.WriteLine("{0}f  - Folder in which the file(s) should be created.", s)
      Console.WriteLine("{0}lf - Create files with line feeds (CrLf).", s)
      Console.WriteLine("{0}ll - Line length (number of characters). Can only be used in conjunction with {0}lf.", s)
      Console.WriteLine("{0}fe - File extension. Defaults to 'tmp'.", s)

      BlankLine()
      Console.WriteLine("Allowed file size units for parameter {0}s are:", s)
      Console.WriteLine("<empty> = Byte      e.g. {0}s=100", s)
      Console.WriteLine("     kb = Kilobyte  e.g. {0}s=100kb", s)
      Console.WriteLine("     mb = Megabyte  e.g. {0}s=100mb", s)
      Console.WriteLine("     gb = Gigabyte  e.g. {0}s=100gb", s)

      BlankLine()
      Console.WriteLine("Please note: 1 KB = 1024 byte, 1 MB = 1024 KB etc.")


   End Sub

   Function CreateRandomStringOfLength(ByVal stringLength As UInt32, Optional ByRef seed As Int32 = -1,
                                       Optional ByVal addLinebreak As Boolean = False) As String

      Static r As Random

      If seed = -1 Then
         seed = DateTime.UtcNow.Millisecond
         r = New Random(seed)
      End If

      Dim sContent As String = Space(stringLength)
      Dim chars() As Char = sContent.ToCharArray

      For i As Int32 = 0 To CType(stringLength - 1, Int32)
         Dim thisChar As Integer = r.Next(48, 122)
         chars(i) = CType(Chr(thisChar), Char)
      Next

      sContent = New String(chars)
      If addLinebreak = True Then
         sContent &= vbNewLine()
      End If

      Return sContent

   End Function

   Function GetNumericPart(ByVal text As String) As String

      Dim s As String = String.Empty

      For i As Int32 = 0 To text.Length
         If System.Char.IsNumber(CType(Mid(text, i, 1), Char)) Then
            s &= Mid(text, i, 1)
         End If
      Next

      Return s

   End Function

End Module
