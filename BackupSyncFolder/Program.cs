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
            Console.WriteLine("Starting...");
            try
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
            catch(Exception ex)
            {
                try
                {
                    SQLiteDB.GetInstance(AppArguments.CurArgs.BackupPath.FullName).WriteLog("", string.Format("Unknown commandline parsing error:{0}{1}", Environment.NewLine, ex.ToString()));
                }
                catch { }
                Console.WriteLine(string.Format("Unknown commandline parsing error:{0}{1}", Environment.NewLine, ex.ToString()));
                Environment.ExitCode = 998;
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
				if (!fiCurrent.HasElements())
				{
					if(AppArguments.CurArgs.IgnoreEmptyTarget)
					{
						Console.WriteLine("No files or folders in current directory found - ignore was set so do nothing!");
						return;
					}

					Console.WriteLine("No files or folders in current directory found - cancel operation because nothing has to be done!");
					Environment.ExitCode = 2;
				}
				else
				{
					// there are entries insied the current folder - move it and recreate it again
					Console.WriteLine("Moving current folder to a backup folder");
					string newName = DateTime.Now.ToString(Properties.Settings.Default.DuplicateDateNaming);

					// create new location
					DirectoryInfo newDi = AppArguments.CurArgs.BackupPath.CreateSubdirectory(newName);

					// move all folders from current location to new one
					fiCurrent.MoveAllSubElements(newDi);

					string dbFile = SQLiteDB.GetInstance(AppArguments.CurArgs.BackupPath.FullName).DbPath;

					try
					{
						SQLiteDB.GetInstance(AppArguments.CurArgs.BackupPath.FullName).BackupDatabase(System.IO.Path.Combine(newDi.FullName, System.IO.Path.GetFileName(dbFile)));
					}
					catch { }
					
					// add folder to db entry
					SQLiteDB.GetInstance(AppArguments.CurArgs.BackupPath.FullName).AddFolder(newDi.FullName);
					DBFolderInfos.Add(new FolderInformation(newDi)); // adding new entry to the database list
				}

				// checking if max folders reached
				int curBackups = DBFolderInfos.Count + 1; // +1 because we prepare for the next backup - that why we add one
				if (curBackups > AppArguments.CurArgs.MaxBackups)
				{
					// we need to delete some backups now
					foreach (FolderInformation fi in DBFolderInfos.OrderByDescending(o => o.CreationDate).Skip(AppArguments.CurArgs.MaxBackups - 1))
					{
						// removing directory
						Console.WriteLine(string.Format("DELETING: {0}", fi.BaseDirectory.FullName));
						fi.Delete();
						SQLiteDB.GetInstance(AppArguments.CurArgs.BackupPath.FullName).RemFolder(fi.BaseDirectory.Name);
						SQLiteDB.GetInstance(AppArguments.CurArgs.BackupPath.FullName).WriteLog(fi.BaseDirectory.Name, "Folder successfully deleted.");
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
