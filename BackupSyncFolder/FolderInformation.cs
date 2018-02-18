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
		private DateTime? _creationDate = null;

		public FolderInformation(string path)
		{
			_directory = new DirectoryInfo(path);
		}

		public FolderInformation(string path, DateTime creationDate)
		{
			_directory = new DirectoryInfo(path);
			_creationDate = creationDate;
		}

		public FolderInformation(DirectoryInfo di)
		{
			_directory = di;
		}

		public FolderInformation(DirectoryInfo di, DateTime creationDate)
		{
			_directory = di;
			_creationDate = creationDate;
		}


		public DirectoryInfo Directory
		{
			get
			{
				return _directory;
			}
		}

		/// <summary>
		/// returns the creation date
		/// </summary>
		public DateTime CreationDate
		{
			get
			{
				if(_creationDate == null)
				{
					return _directory.CreationTime;
				}
				else
				{
					return (DateTime)_creationDate;
				}
			}
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
		/// checks if the directory exists
		/// </summary>
		public bool Exists
		{
			get
			{
				return Directory.Exists;
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
			if(!Exists)
			{
				_directory.Create();
			}
		}


		public void Delete()
		{
			if(Exists)
			{
				_directory.Delete(true);
			}
		}

		/// <summary>
		/// renames the current folder (move)
		/// </summary>
		/// <param name="newName"></param>
		public string RenameDirectory(string newName)
		{
			string newPath = System.IO.Path.Combine(_directory.Parent.FullName, newName);
			Directory.MoveTo(newPath);

			// reset some information to be up2date
			_directory = new DirectoryInfo(newPath);
			_files = null;

			return newPath;
		}
	}
}
