Imports System
Imports System.Diagnostics
Imports System.Reflection

Imports libBAUtilCore
Imports libBAUtilCore.ConsoleUtil
Imports libBAUtilCore.StringUtil
Imports libBAUtilCore.Utils.CmdArgs

Module Program

   Public Const COMPANY_NAME As String = "BasicAware"

   ' Parameters
   Public Const PARAM_MIN = 2 ' # of mandatory parameters

   Sub Main(args As String())

      ' Application intro
      ConHeadline(Assembly.GetEntryAssembly())
      ConCopyright()

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
            sSize = Left(sTemp, sTemp.Length - (InStr(sTemp, "kb") - 1))
            lTimes = 1024
         Case "mb"
            sSize = Left(sTemp, sTemp.Length - (InStr(sTemp, "kb") - 1))
            lTimes = CType(1024 ^ 2, UInt64)
         Case "gb"
            sSize = Left(sTemp, sTemp.Length - (InStr(sTemp, "kb") - 1))
            lTimes = CType(1024 ^ 3, UInt64)
         Case Else
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

      ' gqudSize = qudTimes * qudOrgSize









      'For Each o As KeyValue In cmd.KeyValues
      '   WriteIndent(String.Format("- {0}", o.ToString), 2)
      '   WriteIndent(String.Format("- KeyShort: {0}, KeyLong: {1}", o.KeyShort, o.KeyLong), 2)
      '   WriteIndent(String.Format("- Value: {0}", o.Value.ToString), 3)
      'Next





      BlankLine(addSeparatingLine:=True)

      'Try
      '   For i As Int32 = 5 To 255
      '      Console.WriteLine("#{0}: {1}", i.ToString, Chr(i))
      '   Next
      'Catch ex As Exception
      'End Try

      AnyKey()



   End Sub

   Public Sub ShowHelp()

      BlankLine()
      Console.WriteLine("DummyFile lets you easily create a number of dummy files for various testing scenarios.")
      Console.WriteLine("The files are generated from random (plain text aka 'human readable') characters.")
      Console.WriteLine("Per default, the file's contents is generated as one 'big blob'.")
      Console.WriteLine("However, you can specify to add line feeds (actually line feed plus carriage return)")
      Console.WriteLine("to simulate 'proper text files'.")

      BlankLine()
      Console.WriteLine("Usage:")
      Console.WriteLine("DummyFile /n=<No. of files> /s=<file size> [/f=<folder to create files in>] [/lf] [/ll=<No. of characters per line>] [/fe=<file extension>]")
      Console.WriteLine("")
      Console.WriteLine("     i.e.: DummyFile /n=10 /s=12MB")
      Console.WriteLine("              - Create 10 files (in the current folder) with a size of 12MB each, do not add line feed(s).")
      Console.WriteLine("           DummyFile /n=10 /s=12MB /f=c:\temp")
      Console.WriteLine("              - Create 10 files in the folder c:\temp with a size of 12MB each, do not add line feed(s).")
      Console.WriteLine("           DummyFile /n=10 /s=12MB /f=c:\temp /lf")
      Console.WriteLine("              - Create 10 files in the folder c:\temp with a size of 12MB each, add line feed(s). Line length defaults to 80 characters.")
      Console.WriteLine("           DummyFile /n=10 /s=12MB /f=c:\temp /lf /ll=72")
      Console.WriteLine("              - Create 10 files in the folder c:\temp with a size of 12MB each, add line feed(s). Line length should be 72 characters.")
      Console.WriteLine("           DummyFile /n=10 /s=12MB /f=c:\temp /lf /ll=72 /fe=txt")
      Console.WriteLine("              - Create 10 files with the file extension 'txt' in the folder c:\temp with a size of 12MB each, add line feed(s).")
      Console.WriteLine("                Line length should be 72 characters.")

      BlankLine()
      Console.WriteLine("Parameters")
      Console.WriteLine("----------")
      Console.WriteLine("/n  - Number of files to create (mandatory).")
      Console.WriteLine("/s  - File size of each file (mandatory).")
      Console.WriteLine("/f  - Folder in which the file(s) should be created.")
      Console.WriteLine("/lf - Create files with line feeds (CrLf).")
      Console.WriteLine("/ll - Line length (number of characters). Can only be used in conjunction with /lf.")
      Console.WriteLine("/fe - File extension. Defaults to 'tmp'.")

      BlankLine()
      Console.WriteLine("Allowed file size units for parameter /s are:")
      Console.WriteLine("<empty> = Byte      i.e. DummyFile /n=1, /s=100")
      Console.WriteLine("     kb = Kilobyte  i.e. DummyFile /n=1, /s=100kb")
      Console.WriteLine("     mb = Megabyte  i.e. DummyFile /n=1, /s=100mb")
      Console.WriteLine("     gb = Gigabyte  i.e. DummyFile /n=1, /s=100gb")

      BlankLine()
      Console.WriteLine("Please note: 1 KB = 1024 byte, 1 MB = 1024 KB etc.")


   End Sub


End Module
