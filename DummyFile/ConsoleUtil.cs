using System;
using System.Collections.Generic;
using System.Text;

namespace BasicAware.Utils.ConsoleApps
{
   /// <summary>
   /// General purpose console application helpers.
   /// </summary>
   class ConsoleUtil
   {

      #region "Declarations"
      // Copyright notice values
      private const string COPY_COMPANYNAME = "BasicAware";
      private const string COPY_AUTHOR = "Knuth Konrad";
      // Console defaults
      private const string CON_SEPARATOR = "---";
      #endregion

      #region "ConIntro"
      /// <summary>
      /// Display an application intro
      /// </summary>
      /// <param name="appName">Name of the application</param>
      /// <param name="versionMajor">Major version</param>
      /// <param name="versionMinor">Minor version</param>
      /// <param name="versionRevision">Revision</param>
      /// <param name="versionBuild">Build</param>
      public static void ConIntro(string appName, Int32 versionMajor, 
         Int32 versionMinor = 0, Int32 versionRevision = 0, Int32 versionBuild = 0,
         Boolean leadingBlankLine = true)
      {
         if (leadingBlankLine == true)
         {
            Console.WriteLine();
         }

         System.Console.ForegroundColor = ConsoleColor.White;
         System.Console.WriteLine("* " + appName + " v" +
                           versionMajor.ToString() + "." +
                           versionMinor.ToString() + "." +
                           versionRevision.ToString() + "." +
                           versionBuild.ToString() +
                           " *");
         System.Console.ForegroundColor = ConsoleColor.Gray;
      }

      /// <summary>
      /// Display an application intro
      /// </summary>
      /// <param name="appName">Name of the application</param>
      public static void ConIntro(string appName, Boolean leadingBlankLine = true)
      {
         if (leadingBlankLine == true)
         {
            Console.WriteLine();
         }

         Console.ForegroundColor = ConsoleColor.White;
         Console.WriteLine("* " + appName + " *");
         Console.ForegroundColor = ConsoleColor.Gray;
      }

      /// <summary>
      /// Display an application intro
      /// </summary>
      /// <param name="appName">Name of the application</param>
      /// <param name="versionMajor">Major version</param>
      public static void ConIntro(string appName, Int32 versionMajor, Boolean leadingBlankLine = true)
      {

         if (leadingBlankLine == true)
         {
            Console.WriteLine();
         }

         Console.ForegroundColor = ConsoleColor.White;
         Console.WriteLine("* " + appName + " v" + versionMajor.ToString() + ".0 *");
         Console.ForegroundColor = ConsoleColor.Gray;
      }
      #endregion

      #region "ConCopyright"
      /// <summary>
      /// Display a copyright notice.
      /// </summary>
      public static void ConCopyright(Boolean trailingBlankLine = true)
      {
         ConCopyright(DateTime.Now.Year.ToString(), COPY_COMPANYNAME, trailingBlankLine);
      }

      /// <summary>
      /// Display a copyright notice.
      /// </summary>
      /// <param name="companyName">Copyright owner</param>
      public static void ConCopyright(string companyName, Boolean trailingBlankLine = true)
      {
         ConCopyright(DateTime.Now.Year.ToString(), companyName, trailingBlankLine);
      }

      /// <summary>
      /// Display a copyright notice.
      /// </summary>
      /// <param name="year">Copyrighted in year</param>
      /// <param name="companyName">Copyright owner</param>
      public static void ConCopyright(string year, string companyName, Boolean trailingBlankLine = true)
      {
         Console.WriteLine(String.Format("Copyright {0} {1} by {2}. All rights reserved.", Convert.ToChar(169), year, companyName));
         Console.WriteLine("Written by " + COPY_AUTHOR);
         if (trailingBlankLine == true)
         {
            Console.WriteLine("");
         }
      }
      #endregion

      #region "AnyKey()"
      /// <summary>
      /// Pauses the program execution and waits for a key press
      /// </summary>
      /// <param name="waitMessage">Pause message</param>
      /// <param name="blankLinesBefore">Number of blank lines before the message</param>
      /// <param name="blankLinesAfter">Number of blank lines after the message</param>
      public static void AnyKey(string waitMessage= "-- Press ENTER to continue --", Int32 blankLinesBefore = 0,
         Int32 blankLinesAfter  = 0)
      {
         BlankLine(blankLinesBefore);
         Console.WriteLine(waitMessage);
         BlankLine(blankLinesAfter);
         Console.ReadLine();
      }
      #endregion

      #region "BlankLine"

   /// <summary>
   /// Insert a blank line at the current position.
   /// </summary>
   /// <param name="blankLines">Number of blank lines to insert.</param>
   /// <param name="addSeparatingLine"><see langword="true"/>: Add a visual separation indicator before the blank line(s)</param>
   public static void BlankLine(Int32 blankLines = 1, Boolean addSeparatingLine = false)
   { 

      // Safe guard
      if (blankLines < 1)
      {
            blankLines = 1;
      }

      if (addSeparatingLine == true)
      {
            Console.WriteLine(CON_SEPARATOR);
      }

      for (Int32 i = 0; i <= blankLines - 1; i++)
      {
            Console.WriteLine("");
      }

   }
      #endregion

      #region "WriteIndent"
      /// <summary>
      /// Output text indented by (<paramref name="indentBy"/>) spaces
      /// </summary>
      /// <param name="text">Output text</param>
      /// <param name="indentBy">Number of leading spaces</param>
      /// <param name="addNewLine">Add a new line after <paramref name="text"/>?</param>
      public static void WriteIndent(string text, Int32 indentBy, Boolean addNewLine = true)
      { 
         if (addNewLine == true)
         {
            Console.WriteLine(String.Concat(new String(" "), indentBy) + text);
         }
         else
         {
            Console.Write(String.Concat(new String(" "), indentBy) + text);
         }
      }
      #endregion

   }  // class ConsoleUtil
}
