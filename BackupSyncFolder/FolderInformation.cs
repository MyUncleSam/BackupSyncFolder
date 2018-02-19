using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BackupSyncFolder
{
	public class FolderInformation
	{
		private DirectoryInfo _baseDirectory = null;
		private List<FileInfo> _files = null;
		private List<DirectoryInfo> _directories = null;
		private DateTime? _creationDate = null;

		public FolderInformation(string path)
		{
			_baseDirectory = new DirectoryInfo(path);
		}

		public FolderInformation(string path, DateTime creationDate)
		{
			_baseDirectory = new DirectoryInfo(path);
			_creationDate = creationDate;
		}

		public FolderInformation(DirectoryInfo di)
		{
			_baseDirectory = di;
		}

		public FolderInformation(DirectoryInfo di, DateTime creationDate)
		{
			_baseDirectory = di;
			_creationDate = creationDate;
		}


		public DirectoryInfo BaseDirectory
		{
			get
			{
				return _baseDirectory;
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
					return _baseDirectory.CreationTime;
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
				return _baseDirectory.Name.Equals(Properties.Settings.Default.BackupSubfolderName, StringComparison.OrdinalIgnoreCase);
			}
		}

		/// <summary>
		/// checks if the directory exists
		/// </summary>
		public bool Exists
		{
			get
			{
				return BaseDirectory.Exists;
			}
		}

		/// <summary>
		/// get all files under the given directory (cached)
		/// </summary>
		public List<FileInfo> Files
		{
			get
			{
				if(_files == null)
				{
					_files = _baseDirectory.GetFiles("*", SearchOption.AllDirectories).ToList();
				}
				return _files;
			}
		}

		/// <summary>
		/// get all directories under the given directory (cached)
		/// </summary>
		public List<DirectoryInfo> Directories
		{
			get
			{
				if(_directories == null)
				{
					_directories = _baseDirectory.GetDirectories("*", SearchOption.TopDirectoryOnly).ToList();
				}
				return _directories;
			}
		}

		/// <summary>
		/// get the directory path
		/// </summary>
		public string Path
		{
			get
			{
				return _baseDirectory.FullName;
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
		/// checks if the current folder has sub elements (noncached)
		/// </summary>
		/// <returns></returns>
		public bool HasElements()
		{
			return BaseDirectory.GetDirectories("*", SearchOption.TopDirectoryOnly).Any()
				|| BaseDirectory.GetFiles("*", SearchOption.TopDirectoryOnly).Any();
		}

		/// <summary>
		/// Creates the directory if it is not existing
		/// </summary>
		public void CreateIfNotExist()
		{
			if(!Exists)
			{
				_baseDirectory.Create();
			}
		}


		public void Delete()
		{
			if(Exists)
			{
				_baseDirectory.Delete(true);
			}
		}

		/// <summary>
		/// renames the current folder (move)
		/// </summary>
		/// <param name="newName"></param>
		public string RenameDirectory(string newName)
		{
			string newPath = System.IO.Path.Combine(_baseDirectory.Parent.FullName, newName);
			BaseDirectory.MoveTo(newPath);

			// reset some information to be up2date
			_baseDirectory = new DirectoryInfo(newPath);
			_files = null;

			return newPath;
		}

		/// <summary>
		/// creates the sub folder at the root level 
		/// </summary>
		/// <param name="newName"></param>
		public void MoveAllSubElements(DirectoryInfo newPath)
		{
			// move all files
			foreach (FileInfo subFile in BaseDirectory.GetFiles("*", SearchOption.TopDirectoryOnly))
			{
				subFile.MoveTo(System.IO.Path.Combine(newPath.FullName, subFile.Name));
			}

			// move all folders
			foreach (DirectoryInfo subDir in BaseDirectory.GetDirectories("*", SearchOption.TopDirectoryOnly))
			{
				subDir.MoveTo(System.IO.Path.Combine(newPath.FullName, subDir.Name));
			}
		}
	}
}
