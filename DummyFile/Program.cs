using System;
using System.CommandLine;  // https://github.com/dotnet/command-line-api
using System.CommandLine.Invocation;
using System.IO;

// using CommandLine;   // https://github.com/commandlineparser/commandline

using static BasicAware.Utils.ConsoleApps.ConsoleUtil;

namespace DummyFile
{
   class Program
   {
      static int Main(string[] args)
      {
         ConIntro("DummyFile");
         ConCopyright();

         // 
         var rootCommand = new RootCommand
         {
            new Option<int>(
            "--int-option",
            getDefaultValue: () => 42,
            description: "An option whose argument is parsed as an int"),
        new Option<bool>(
            "--bool-option",
            "An option whose argument is parsed as a bool"),
        new Option<FileInfo>(
            "--file-option",
            "An option whose argument is parsed as a FileInfo")
         };

         rootCommand.Description = "My sample app";

         // Note that the parameters of the handler method are matched according to the names of the options
         rootCommand.Handler = CommandHandler.Create<int, bool, FileInfo>((intOption, boolOption, fileOption) =>
         {
            Console.WriteLine($"The value for --int-option is: {intOption}");
            Console.WriteLine($"The value for --bool-option is: {boolOption}");
            Console.WriteLine($"The value for --file-option is: {fileOption?.FullName ?? "null"}");
         });

         // Parse the incoming args and invoke the handler
         return rootCommand.InvokeAsync(args).Result;

         // AnyKey();
      }
   }  // Program
}
