using System;
// using System.CommandLine;  // https://github.com/dotnet/command-line-api
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

using libBAUtilCoreCS;
using static libBAUtilCoreCS.ConsoleHelper;
using static libBAUtilCoreCS.FilesystemHelper;
using static libBAUtilCoreCS.StringHelper;
using libBAUtilCoreCS.Utils.Args;

namespace DummyFile
{

  class Program
  {

    // Parameters
    public const Int32 PARAM_MIN = 2; // # of mandatory parameters

    /// <summary>
    /// Application exit codes (%ERRORLEVEL%)
    /// </summary>
    enum AppResult
    {
      /// <summary>
      /// Operation successful
      /// </summary>
      OKSuccess = 0,
      /// <summary>
      /// Less than minimum # of parameters supplied
      /// </summary>
      TooFewParameters = 1,
      /// <summary>
      /// At least 1 mandatory parameter is missing
      /// </summary>
      MissingMandatoryParameter = 2,
      /// <summary>
      /// A supplied value is invalid
      /// </summary>
      InvalidParameterValue = 3,
      /// <summary>
      /// Folder doesn't exist
      /// </summary>
      FolderDoesNotExist = 4,
      /// <summary>
      /// The creation of the dummy file's content failed.
      /// </summary>
      ContentCreationFailed = 5
    }

    // C# doesn't support method level static vars like VB
    // this is for  CreateRandomStringOfLength()
    static Random r;

    struct FilePropertyTYPE
    {
      public string FilePath;
      public string FileExtension;
      public string FileContents;
      public string FilePrefix;
    }
    static int Main(string[] args)
    {

      // Application intro
      AppIntro("DummyFile");
      AppCopyright();


      // ** Parse the command line parameters **

      // All valid parameters
      // /n  - Number of files to create (mandatory).
      // /s  - File size of each file (mandatory).
      // /f  - Folder in which the file(s) should be created.
      // /lf - Create files with line feeds (CrLf).
      // /ll - Line length (number of characters). Can only be used in conjunction with /lf.
      // /fe - File extension. Defaults to 'tmp'.
      // /fp - File (name) prefix.


      List<string> paramListAll = new List<string>() { "n", "s", "f", "fe", "fp", "lf", "ll" };
      List<string> paramListMandatory = new List<string>() { "n", "s" };

      CmdArgs cmd = new CmdArgs(paramListAll);
      cmd.Initialize();


      // * Validate what we have
      // Too few parameters?
      if (cmd.ParametersCount < PARAM_MIN)
      {
        WriteIndent(String.Format("!!! Too few parameters. Mandatory parameters: {0}, parameters supplied: {1}", PARAM_MIN, cmd.ParametersCount.ToString()), 2);
        ShowHelp();
        return (Int32)AppResult.TooFewParameters;
      }

      // Missing mandatory parameter?
      if (!cmd.HasParameter(paramListMandatory))
      {
        WriteIndent("!!! Mandatory parameter(s) missing.", 2);
        ShowHelp();
        return (Int32)AppResult.MissingMandatoryParameter;
      }

      // Invalid parameter(s)?
      // ToDo: Implement method CmdArgs.ValidateParameters. Determines if each parameter passed is a valid parameter

      // * Let's get the actual parameter values
      // # of files to create
      UInt32 lFiles = 0;

      try
      {
        lFiles = Convert.ToUInt32(cmd.GetValueByName("n"));
      }
      catch (Exception)
      {
        WriteIndent(String.Format("!!! Invalid value for parameter 'n': {0}", cmd.GetValueByName("n").ToString()), 2);
        ShowHelp();
        return (Int32)AppResult.InvalidParameterValue;
      }

      // Size of each file, sSize = actual size, sUnit = size of lUnit, e.g. "kb", lTimes = unit multiplier, e.g. "kb" = 1024
      String sTemp = String.Empty;
      String sSize = String.Empty;
      String sUnit = String.Empty;
      UInt64 lTimes = 0;

      try
      {
        sTemp = (String)cmd.GetValueByName("s");
      }
      catch (Exception)
      {
        WriteIndent(String.Format("!!! Invalid value for parameter 's': {0}", cmd.GetValueByName("s").ToString()), 2);
        ShowHelp();
        return (Int32)AppResult.InvalidParameterValue;
      }

      sUnit = Right(sTemp, 2).ToLower();
      switch (sUnit)
      {
        case "kb":
          sSize = sTemp.Replace(sUnit, String.Empty);
          lTimes = 1024;
          break;
        case "mb":
          sSize = sTemp.Replace(sUnit, String.Empty);
          lTimes = (UInt64)(1024 ^ 2);
          break;
        case "gb":
          sSize = sTemp.Replace(sUnit, String.Empty);
          lTimes = (UInt64)(1024 ^ 3);
          break;
        default:
          sSize = GetNumericPart(sTemp);
          lTimes = 1;
          sUnit = "byte";
          break;
      }

      // Remember the original value for size
      UInt64 lOriginalSize = 0;

      try
      {
        lOriginalSize = Convert.ToUInt64(sSize);
      }
      catch (OverflowException)
      {
        WriteIndent(String.Format("!!! {0} is outside the range of the UInt64 type: ", sSize), 2);
        ShowHelp();
        return (Int32)AppResult.InvalidParameterValue;
      }
      catch (FormatException)
      {
        WriteIndent(String.Format("!!! The {0} value '{1}' is not in a recognizable format.", sSize.GetType().Name, sSize), 2);
        ShowHelp();
        return (Int32)AppResult.InvalidParameterValue;
      }

      // Actual file size in bytes
      Int32 lRealSize = (Int32)(lOriginalSize * lTimes);

      // Do the values of the mandatory parameters make sense?
      if (lFiles < 1 || lRealSize < 1)
      {
        WriteIndent(String.Format("!!! Invalid value for at least one parameter: {0}, {1}",
                                  cmd.GetParameterByName("n").OriginalParameter, cmd.GetParameterByName("s").OriginalParameter), 2);
        ShowHelp();
        return (Int32)AppResult.InvalidParameterValue;
      }


      // *** Optional parameters
      // ** Location of file creation
      if (cmd.HasParameter("f"))
      {
        sTemp = cmd.GetValueByName("f").ToString().Trim().Replace(vbQuote(), String.Empty);
      }
      else
      {
        // Current folder is the default location
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        { sTemp = @".\"; }
        else
        { sTemp = "./"; }
      }

      if (!FolderExists(sTemp))
      {
        WriteIndent(String.Format("!!! Folder doesn't exist: {0}", cmd.GetValueByName("f").ToString()), 2);
        ShowHelp();
        return (Int32)AppResult.FolderDoesNotExist;
      }

      string sPath = sTemp;

      // ** Create a text file with line breaks?
      bool bolAddLineBreaks = cmd.HasParameter("lf");


      // ** Line length?
      UInt32 lLineLength = 0;
      if (cmd.HasParameter("ll"))
      {
        sTemp = cmd.GetValueByName("ll").ToString();

        try
        {
          lLineLength = Convert.ToUInt32(sTemp);
        }
        catch (OverflowException)
        {
          WriteIndent(String.Format("!!! Invalid line length: {0} is outside the range of the UInt32 type: ", sTemp), 2);
          ShowHelp();
          return (Int32)AppResult.InvalidParameterValue;
        }
        catch (FormatException)
        {
          WriteIndent(String.Format("!!!  Invalid line length: the {0} value '{1}' is not in a recognizable format.", sTemp.GetType().Name, sTemp), 2);
          ShowHelp();
          return (Int32)AppResult.InvalidParameterValue;
        }
      }

      // ** File extension?
      // 'tmp' is the default extension
      string sFileExt = "tmp";
      if (cmd.HasParameter("fe"))
      {
        sTemp = cmd.GetValueByName("fe").ToString().Trim().Replace(vbQuote(), String.Empty);
        if (sTemp.Length > 0)
        { sFileExt = sTemp; }
      }

      // Determine if it's a relative or absolute path, i.e. .\MyFolder or C:\MyFolder and/ or a UNC share
      string sPathFull = sPath;

      try
      {
        sPathFull = Path.GetFullPath(sPathFull);
      }
      catch (Exception)
      {
        // We checked that the folder exists, so in theory this shouldn't happen
        WriteIndent(String.Format("!!!  Unable to resolve full path for {0}", sPath), 2);
      }

      // ** File name prefix?
      String sFilePrefix = "";
      if (cmd.HasParameter("fp"))
      {
        sTemp = cmd.GetValueByName("fp").ToString().Trim().Replace(vbQuote(), String.Empty);
        if (sTemp.Length > 0)
        { sFilePrefix = sTemp; }
      }



      // *** Echo the CLI parameters
      Console.WriteLine("# of files    : {0}", lFiles.ToString());
      Console.WriteLine("File size     : {0} {1}", lOriginalSize.ToString(), sUnit.ToUpper());
      Console.WriteLine("File extension: {0}", sFileExt);
      Console.WriteLine("File prefix   : {0}", sFilePrefix);
      Console.Write("Folder        : {0}", sPath);
      // If path is a relative path, display the full path also
      if (NormalizePath(sPath).ToLower() == NormalizePath(sPathFull).ToLower())
      { Console.WriteLine(""); }
      else
      { Console.WriteLine(" ({0})", sPathFull); }
      Console.WriteLine("Add line feed : {0}", bolAddLineBreaks.ToString());
      if (lLineLength > 0)
      { Console.WriteLine("Line length   : {0}", lLineLength.ToString()); }


      // *** Prepare the file content
      string sContent = String.Empty;

      BlankLine();
      Console.Write("Preparing file contents ... ");

      Stopwatch Execution_Start = new Stopwatch();
      Execution_Start.Start();

      try
      {
        sContent = CreateRandomFileContent(lRealSize, bolAddLineBreaks, lLineLength);
      }
      catch (ArgumentOutOfRangeException)
      {
        WriteIndent("!!! Can't generate a file of this size, aborting ...", 2);
        return (Int32)AppResult.ContentCreationFailed;
      }
      catch (OverflowException)
      {
        WriteIndent("!!! Can't generate a file of this size, aborting ...", 2);
        return (Int32)AppResult.ContentCreationFailed;
      }
      catch (Exception)
      {
        WriteIndent("!!! Can't generate a file of this size, aborting ...", 2);
        return (Int32)AppResult.ContentCreationFailed;
      }

      Console.WriteLine(String.Format("done (in {0} sec.).", Execution_Start.Elapsed.TotalSeconds.ToString()));


      // *** Create the temp. files
      FilePropertyTYPE o = new FilePropertyTYPE();

      o.FilePath = sPathFull;
      o.FileExtension = sFileExt;
      o.FileContents = sContent;
      o.FilePrefix = sFilePrefix;

      Console.Write("Creating {0} file(s) ... ", lFiles);
      Execution_Start.Start();

      Thread thd = null;
      for (UInt32 i = 1; i <= lFiles; i++)
      {
        thd = new Thread(ThdCreateFile);
        thd.Start(o);
      }

      if (thd != null)
      {
        if (thd.ThreadState != System.Threading.ThreadState.Stopped)
        {
          thd.Join();
        }
      }

      Thread.Sleep(1000);
      Console.WriteLine(String.Format("done (in {0} sec.).", Execution_Start.Elapsed.TotalSeconds.ToString()));

      return (Int32)AppResult.OKSuccess;


    } // class Main

    static string GetNumericPart(string text)
    {
      string s = String.Empty;

      for (Int32 i = 0; i <= text.Length - 1; i++)
      {
        if (Char.IsNumber(System.Convert.ToChar(Mid(text, i, 1))))
        { s += Mid(text, i, 1); }
      }
      return s;
    }

    /// <summary>
    /// Create random plain text-like content
    /// </summary>
    /// <param name="lRealSize">Size in bytes</param>
    /// <param name="bolAddLineBreaks">Artificially add line breaks</param>
    /// <param name="lLineLength">Line length, if <paramref name="bolAddLineBreaks"/> is <see langword="true"/>.</param>
    /// <returns>Plan text (file)-like string</returns>
    static string CreateRandomFileContent(Int32 lRealSize, bool bolAddLineBreaks, UInt32 lLineLength)
    {

      Random r = new Random(DateTime.UtcNow.Millisecond);
      UInt32 lRows = 1;

      // Figure out how to format the output
      if (bolAddLineBreaks && lLineLength > 0)
      {
        // Creating a file with line breaks only makes sense if a length is specified
        lRows = (UInt32)MathUtil.IntDiv(lRealSize, lLineLength);
        lRealSize += (Int32)(MathUtil.IntDiv(lRealSize, lLineLength) * vbNewLine().Length);
      }

      // Add additional space for line breaks
      String sLine = String.Empty;
      if (lRows > 1)
      { sLine = Space(lLineLength); }
      else
      {
        sLine = Space(lRealSize);
        lLineLength = (UInt32)lRealSize;
      }

      String sContent = String.Empty;

      Int32 seed = -1;
      for (Int32 i = 1; i <= lRows; i++)
      {
        sLine = CreateRandomStringOfLength(lLineLength, ref seed, bolAddLineBreaks);
        sContent += sLine;
      }

      // Any remainder we need to add?
      if (bolAddLineBreaks && lLineLength > 0)
      {
        if ((sContent.Length + (lRows * vbNewLine().Length)) < (lRows * (lLineLength + vbNewLine().Length)))
        {

          UInt32 lRemainder = (UInt32)(lRows * (lLineLength + vbNewLine().Length) - sContent.Length + (lRows * vbNewLine().Length));
          sContent += CreateRandomStringOfLength(lRemainder, ref seed, bolAddLineBreaks);
        }
      }
      else
      {
        if (sContent.Length < lLineLength)
        {
          sContent += CreateRandomStringOfLength((UInt32)(lLineLength - sContent.Length), ref seed, bolAddLineBreaks);
        }
      }
      return sContent;
    }

    static string CreateRandomStringOfLength(UInt32 stringLength, ref Int32 seed, bool addLinebreak = false)
    {

      if (seed == -1)
      {
        seed = DateTime.UtcNow.Millisecond;
        r = new Random(seed);
      }

      string sContent = Space(stringLength);
      Char[] chars = sContent.ToCharArray();

      for (Int32 i = 0; i <= stringLength - 1; i++)
      {
        Int32 thisChar = r.Next(48, 122);
        chars[i] = Convert.ToChar(Chr(thisChar));
      }

      sContent = new String(chars);
      if (addLinebreak)
      { sContent += vbNewLine(); }

      return sContent;

    } // string CreateRandomStringOfLength

    /// <summary>
    /// Shows DummyFile's usage and syntax help.
    /// </summary>
    static void ShowHelp()
    {

      string s = String.Empty;

      CmdArgs o = new CmdArgs();
      s = o.DelimiterArgs;

      BlankLine();
      Console.WriteLine("DummyFile lets you easily create a number of dummy files for various testing scenarios.");
      Console.WriteLine("The files are generated from random plain text (aka 'human readable') characters.");
      Console.WriteLine("Per default, the file's contents is generated as one 'big blob'.");
      Console.WriteLine("However, you can specify to add line feeds (actually line feed plus carriage return)");
      Console.WriteLine("to simulate 'proper text files'.");

      BlankLine();
      Console.WriteLine("Usage:");
      Console.WriteLine("DummyFile {0}n=<No. of files> {0}s=<file size> [{0}f=<folder to create files in>] [{0}lf] [{0}ll=<No. of characters per line>] [{0}fe=<file extension>]", s);
      Console.WriteLine("");
      Console.WriteLine("     e.g.: DummyFile {0}n=10 {0}s=12MB", s);
      Console.WriteLine("              - Create 10 files (in the current folder) with a size of 12MB each, do not add line feed(s).");
      Console.WriteLine(@"           DummyFile {0}n=10 {0}s=12MB {0}f=c:\temp", s);
      Console.WriteLine(@"              - Create 10 files in the folder c:\temp with a size of 12MB each, do not add line feed(s).");
      Console.WriteLine(@"           DummyFile {0}n=10 {0}s=12MB {0}f=c:\temp {0}lf", s);
      Console.WriteLine(@"              - Create 10 files in the folder c:\temp with a size of 12MB each, add line feed(s).");
      Console.WriteLine("                Line length defaults to 80 characters.");
      Console.WriteLine(@"           DummyFile {0}n=10 {0}s=12MB {0}f=c:\temp {0}lf {0}ll=72", s);
      Console.WriteLine(@"              - Create 10 files in the folder c:\temp with a size of 12MB each, add line feed(s).");
      Console.WriteLine("                Line length should be 72 characters.");
      Console.WriteLine(@"           DummyFile {0}n=10 {0}s=12MB {0}f=c:\temp {0}lf {0}ll=72 {0}fe=txt", s);
      Console.WriteLine(@"              - Create 10 files with the file extension 'txt' in the folder c:\temp with a size of 12MB each,");
      Console.WriteLine("                add line feed(s). Line length should be 72 characters.");
      Console.WriteLine(@"           DummyFile {0}n=10 {0}s=12MB {0}f=c:\temp {0}lf {0}ll=72 {0}fe=txt {0}fp=tmp_", s);
      Console.WriteLine(@"              - Create 10 files with a fixed prefix of 'tmp_' and the file extension 'txt' in the folder c:\temp");
      Console.WriteLine("                 with a size of 12MB each, add line feed(s). Line length should be 72 characters.");

      BlankLine();
      Console.WriteLine("Parameters");
      Console.WriteLine("----------");
      Console.WriteLine("{0}n  - Number of files to create (mandatory).", s);
      Console.WriteLine("{0}s  - File size of each file (mandatory).", s);
      Console.WriteLine("{0}f  - Folder in which the file(s) should be created.", s);
      Console.WriteLine("{0}lf - Create files with line feeds (CrLf).", s);
      Console.WriteLine("{0}ll - Line length (number of characters). Can only be used in conjunction with {0}lf.", s);
      Console.WriteLine("{0}fe - File extension. Defaults to 'tmp'.", s);
      Console.WriteLine("{0}fp - Fixed file name prefix.", s);

      BlankLine();
      Console.WriteLine("Allowed file size units for parameter {0}s are:", s);
      Console.WriteLine("<empty> = Byte      e.g. {0}s=100", s);
      Console.WriteLine("     kb = Kilobyte  e.g. {0}s=100kb", s);
      Console.WriteLine("     mb = Megabyte  e.g. {0}s=100mb", s);
      Console.WriteLine("     gb = Gigabyte  e.g. {0}s=100gb", s);

      BlankLine();
      Console.WriteLine("Please note: 1 KB = 1024 byte, 1 MB = 1024 KB etc.");

    }

    /// <summary>
    /// Create a file with a random file name and fill it with the previously created random (text) content.
    /// </summary>
    /// <param name="data">Random file creation location</param>
    public static async void ThdCreateFile(Object data)
    {

      FilePropertyTYPE o;
      string sFile = String.Empty;

      o = (FilePropertyTYPE)data;

      do
      {
        // Make sure the file doesn't already exist
        sFile = o.FilePrefix + Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
        sFile = NormalizePath(o.FilePath) + sFile + "." + o.FileExtension;
      }
      while (FileExists(sFile));


      using (System.IO.StreamWriter txtStream = new System.IO.StreamWriter(sFile))
      {
        await txtStream.WriteAsync(o.FileContents);
        await txtStream.FlushAsync();
        txtStream.Close();
      }
    } // void ThdCreateFile

  }  // Program

}

