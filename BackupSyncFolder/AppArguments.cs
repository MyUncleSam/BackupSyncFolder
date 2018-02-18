using CommandLineParser.Arguments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackupSyncFolder
{
	public class AppArguments
	{
		public static AppArguments CurArgs = new AppArguments();
		
		[DirectoryArgument('p', AllowMultiple = false, Description = "Path to your base backup folder - make sure it is empty!", DirectoryMustExist = true, Optional = false)]
		public System.IO.DirectoryInfo BackupPath;

		[ValueArgument(typeof(int), 'm', AllowMultiple = false, DefaultValue = 2, Description = "Amount of backups to keep if current folder is filled (deletes oldest backups to fullfill these count).", Optional = true, ValueOptional = false)]
		public int MaxBackups;
	}
}