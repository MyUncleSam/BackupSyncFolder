using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BackupSyncFolder
{
	class Program
	{
		private static List<FolderInformation> FolderInfos = new List<FolderInformation>();
		private static List<FolderInformation> DBFolderInfos = null;

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

			try
			{
				// get all sub folders
				List<string> dil = Directory.GetDirectories(AppArguments.CurArgs.BackupPath.FullName, "*", SearchOption.TopDirectoryOnly).ToList();

				// generate list of directory information
				foreach (string di in dil)
				{
					FolderInfos.Add(new FolderInformation(di));
				}

				// get known folders from db
				DBFolderInfos = SQLiteDB.GetInstance(AppArguments.CurArgs.BackupPath.FullName).GetActiveFolders();

				// check if it is the first run
				if (!FolderInfos.Any(a => a.isBackupFolder))
				{
					// current folder for backups does not exist - seems to be the first run
					FolderInfos.Add(CreateCurrent());

					Console.WriteLine("This is the first run - do nothing then creating the main backup folder!");
					Environment.ExitCode = 1;
					return;
				}

				FolderInformation fiCurrent = FolderInfos.First(f => f.isBackupFolder);

				// check if there are files in the folder
				if (fiCurrent.FileCount <= 0)
				{
					Console.WriteLine("No files in current folder found - do nothing!");
					Environment.ExitCode = 2;
				}

				// move current if needed
				if (fiCurrent.FileCount > 0)
				{
					// there are entries insied the current folder - move it and recreate it again
					Console.WriteLine("Moving current folder to a backup folder");
					string newName = DateTime.Now.ToString(Properties.Settings.Default.DuplicateDateNaming);
					string newPath = fiCurrent.RenameDirectory(newName);
					string dbFile = SQLiteDB.GetInstance(AppArguments.CurArgs.BackupPath.FullName).DbPath;

					try
					{
						SQLiteDB.GetInstance(AppArguments.CurArgs.BackupPath.FullName).BackupDatabase(System.IO.Path.Combine(newPath, System.IO.Path.GetFileName(dbFile)));
					}
					catch { }

					CreateCurrent();

					// add folder to db entry
					SQLiteDB.GetInstance(AppArguments.CurArgs.BackupPath.FullName).AddFolder(newPath);
					DBFolderInfos.Add(new FolderInformation(newPath)); // adding new entry to the database list
				}
				else
				{
					Console.WriteLine("Current folder empty - do not move it.");
				}

				// checking if max folders reached
				int curBackups = DBFolderInfos.Count + 1; // +1 because we prepare for the next backup - that why we add one
				if (curBackups > AppArguments.CurArgs.MaxBackups)
				{
					// we need to delete some backups now
					foreach (FolderInformation fi in DBFolderInfos.OrderByDescending(o => o.CreationDate).Skip(AppArguments.CurArgs.MaxBackups - 1))
					{
						// removing directory
						Console.WriteLine(string.Format("DELETING: {0}", fi.Directory.FullName));
						fi.Delete();
						SQLiteDB.GetInstance(AppArguments.CurArgs.BackupPath.FullName).RemFolder(fi.Directory.Name);
						SQLiteDB.GetInstance(AppArguments.CurArgs.BackupPath.FullName).WriteLog(fi.Directory.Name, "Folder successfully deleted.");
					}
				}
			}
			catch (Exception ex)
			{
				try
				{
					SQLiteDB.GetInstance(AppArguments.CurArgs.BackupPath.FullName).WriteLog("", string.Format("Unknown error:{0}{1}", Environment.NewLine, ex.ToString()));
				}
				catch { }
				Console.WriteLine(string.Format("Unknown error:{0}{1}", Environment.NewLine, ex.ToString()));
				Environment.ExitCode = 999;
				return;
			}
			
		}

		public static FolderInformation CreateCurrent()
		{
			int waitCount = 0;
			int maxWaitCount = 60;
			int sleepTime = 1000;
			string newPath = Path.Combine(AppArguments.CurArgs.BackupPath.FullName, Properties.Settings.Default.BackupSubfolderName);
			while (System.IO.Directory.Exists(newPath))
			{
				if(waitCount > maxWaitCount)
				{
					throw new Exception(string.Format("Waited for {0} seconds but move of current folder has not finished - aborting procedure.", sleepTime * maxWaitCount / 1000));
				}
				if(waitCount == 0)
				{
					Console.WriteLine("Need to wait for move complete ...");
				}

				waitCount++;
				System.Threading.Thread.Sleep(sleepTime);
			}

			FolderInformation newCurrent = new FolderInformation(newPath);
			newCurrent.CreateIfNotExist();
			return newCurrent;
		}
	}
}
