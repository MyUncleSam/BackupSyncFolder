using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BackupSyncFolder
{
	public class FolderInformation
	{
		private DirectoryInfo _directory = null;
		private List<FileInfo> _files = null;

		public FolderInformation(string path)
		{
			_directory = new DirectoryInfo(path);
		}

		public FolderInformation(DirectoryInfo di)
		{
			_directory = di;
		}

		/// <summary>
		/// check if the current folder is the main backup folder
		/// </summary>
		public bool isBackupFolder
		{
			get
			{
				return _directory.Name.Equals(Properties.Settings.Default.BackupSubfolderName, StringComparison.OrdinalIgnoreCase);
			}
		}

		/// <summary>
		/// get all files under the given directory
		/// </summary>
		public List<FileInfo> Files
		{
			get
			{
				if(_files == null)
				{
					_files = _directory.GetFiles("*", SearchOption.AllDirectories).ToList();
				}
				return _files;
			}
		}

		/// <summary>
		/// get the directory path
		/// </summary>
		public string Path
		{
			get
			{
				return _directory.FullName;
			}
		}

		/// <summary>
		/// get the number of sub files (all subdirectories)
		/// </summary>
		public int FileCount
		{
			get
			{
				return Files.Count;
			}
		}

		/// <summary>
		/// Creates the directory if it is not existing
		/// </summary>
		public void CreateIfNotExist()
		{
			if(!_directory.Exists)
			{
				_directory.Create();
			}
		}

		/// <summary>
		/// renames the current folder (move)
		/// </summary>
		/// <param name="newName"></param>
		public void RenameDirectory(string newName)
		{
			Directory.Move(_directory.FullName, System.IO.Path.Combine(_directory.Parent.FullName, newName));
		}
	}
}
