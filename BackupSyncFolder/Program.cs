using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackupSyncFolder
{
	class Program
	{
		static void Main(string[] args)
		{
			CommandLineParser.CommandLineParser parser = new CommandLineParser.CommandLineParser();
			parser.AcceptEqualSignSyntaxForValueArguments = true;
			parser.IgnoreCase = true;
			parser.AcceptSlash = true;
			parser.AllowShortSwitchGrouping = false;
			parser.ExtractArgumentAttributes(AppArguments.CurArgs);

			try
			{
				parser.ParseCommandLine(args);
				parser.ShowParsedArguments();
			}
			catch (CommandLineParser.Exceptions.CommandLineArgumentException cle)
			{
				Console.WriteLine(string.Format("Invalid command line parameters: {0}", cle.Message));
				parser.ShowUsage();
				Environment.ExitCode = 1;
				return;
			}
			catch (CommandLineParser.Validation.ArgumentConflictException ace)
			{
				Console.WriteLine(string.Format("Invalid command line parameters: {0}", ace.Message));
				parser.ShowUsage();
				Environment.ExitCode = 1;
				return;
			}

			if (!parser.ParsingSucceeded)
			{
				Environment.ExitCode = 1;
				return;
			}  


		}
	}
}
