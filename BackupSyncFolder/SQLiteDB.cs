using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace BackupSyncFolder
{
	public class SQLiteDB
	{
		private SQLiteConnection SQLCon { get; set; }
		public string BasePath { get; private set; }
		public string DbPath { get; private set; }
		private static Dictionary<string, SQLiteDB> _Instances = new Dictionary<string, SQLiteDB>();

		public static SQLiteDB GetInstance(string basePath)
		{
			if(!_Instances.Any(a => a.Key.Equals(basePath, StringComparison.OrdinalIgnoreCase)))
			{
				_Instances.Add(basePath, new SQLiteDB(System.IO.Path.Combine(basePath, "syncdb.sqlite")));
			}

			return _Instances.First(f => f.Key.Equals(basePath, StringComparison.OrdinalIgnoreCase)).Value;
		}

		private SQLiteDB(string file)
		{
			if (!System.IO.File.Exists(file))
			{
				// create a new database
				SQLiteConnection.CreateFile(file);
			}

			BasePath = (new System.IO.FileInfo(file)).DirectoryName;
			DbPath = file;
			SQLCon = new SQLiteConnection(string.Format("DATA Source={0};Compress=True;UTF8Encoding=True;", file));

			// create default table structures (if not exist
			// create properties
			SQLiteCommand createCmds = new SQLiteCommand("CREATE TABLE IF NOT EXISTS FOLDERS (FOLDERNAME VARCHAR(8000) NOT NULL COLLATE NOCASE, ISACTIVE INTEGER NOT NULL DEFAULT 1, CREATIONDATE DATETIME DEFAULT CURRENT_TIMESTAMP, REMOVEDATE DATETIME DEFAULT NULL);", SQLCon);

			// create matter logging
			SQLiteCommand logCmds = new SQLiteCommand("CREATE TABLE IF NOT EXISTS LOGGING (Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP, FOLDERNAME varchar(8000) COLLATE NOCASE, message varchar(8000))", SQLCon);
			
			SQLCon.Open();
			createCmds.ExecuteNonQuery();
			logCmds.ExecuteNonQuery();
			SQLCon.Close();

			createCmds.Dispose();
			logCmds.Dispose();
		}

		~SQLiteDB()
		{
			try
			{
				if (SQLCon.State != System.Data.ConnectionState.Closed)
				{
					SQLCon.Close();
				}
			}
			catch { }
		}

		/// <summary>
		/// writes some log information (e.g. if there are any errors
		/// </summary>
		/// <param name="path">folder or foldername</param>
		/// <param name="message"></param>
		public void WriteLog(string path, string message)
		{
			if (message.Length > 8000)
			{
				message = message.Substring(0, 8000);
			}

			using (SQLiteCommand logCmd = new SQLiteCommand(SQLCon))
			{
				logCmd.CommandType = System.Data.CommandType.Text;
				logCmd.CommandText = "INSERT INTO LOGGING (foldername, message) VALUES (@f, @m)";
				logCmd.Parameters.AddWithValue("@f", (new System.IO.DirectoryInfo(path)).Name);
				logCmd.Parameters.AddWithValue("@m", message);
				SQLCon.Open();
				logCmd.ExecuteNonQuery();
				SQLCon.Close();
			}
		}

		/// <summary>
		/// adds a folder to the list fo current folders
		/// </summary>
		/// <param name="path">foldername or path</param>
		public void AddFolder(string path)
		{
			using (SQLiteCommand fAdd = new SQLiteCommand(SQLCon))
			{
				fAdd.CommandType = System.Data.CommandType.Text;
				fAdd.CommandText = "INSERT INTO FOLDERS (FOLDERNAME, CREATIONDATE) VALUES(@f, @c)";
				fAdd.Parameters.AddWithValue("@f", (new System.IO.DirectoryInfo(path)).Name);
				fAdd.Parameters.AddWithValue("@c", System.IO.Directory.GetCreationTime(path));
				SQLCon.Open();
				fAdd.ExecuteNonQuery();
				SQLCon.Close();
			}

			WriteLog(path, "New folder added.");
		}

		/// <summary>
		/// removes a folder from active list
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public int RemFolder(string path)
		{
			int entries = 0;
			using (SQLiteCommand fRem = new SQLiteCommand(SQLCon))
			{
				fRem.CommandType = System.Data.CommandType.Text;
				fRem.CommandText = "UPDATE FOLDERS SET ISACTIVE = 0 AND REMDATE = @r WHERE FOLDERNAME = @p AND ISACTIVE = 1";
				fRem.Parameters.AddWithValue("@p", (new System.IO.DirectoryInfo(path)).Name);
				fRem.Parameters.AddWithValue("@r", DateTime.Now);
				SQLCon.Open();
				entries = fRem.ExecuteNonQuery();
				SQLCon.Close();
			}

			WriteLog((new System.IO.DirectoryInfo(path)).Name, string.Format("Switched {0} entries to inactive.", entries));
			return entries;
		}

		/// <summary>
		/// get a list of all active folders
		/// </summary>
		/// <returns></returns>
		public List<FolderInformation> GetActiveFolders()
		{
			List<FolderInformation> ret = new List<FolderInformation>();
			using (SQLiteCommand getActive = new SQLiteCommand(SQLCon))
			{
				getActive.CommandType = System.Data.CommandType.Text;
				getActive.CommandText = "SELECT FOLDERNAME, CREATIONDATE FROM FOLDERS WHERE ISACTIVE = 1";
				SQLCon.Open();
				using (SQLiteDataReader rdr = getActive.ExecuteReader())
				{
					while(rdr.Read())
					{
						string FolderName = (string)rdr.GetValue(rdr.GetOrdinal("FOLDERNAME"));
						DateTime CreationDate = (DateTime)rdr.GetValue(rdr.GetOrdinal("CREATIONDATE"));
						ret.Add(new FolderInformation(System.IO.Path.Combine(BasePath, FolderName), CreationDate));
					}
				}
				SQLCon.Close();
			}

			return ret;
		}
		
		public void BackupDatabase(string dest)
		{
			using (SQLiteConnection backupDb = new SQLiteConnection(string.Format("Data Source={0};Compress=True;UTF8Encoding=True;", dest)))
			{
				backupDb.Open();
				SQLCon.Open();
				SQLCon.BackupDatabase(backupDb, backupDb.Database, SQLCon.Database, -1, null, -1);
				SQLCon.Close();
				backupDb.Close();
			}


			// delete old logs - we have them in the backup which got created
			using (SQLiteCommand delCmd = new SQLiteCommand("DELETE FROM LOGGING WHERE Timestamp <= date('now', '-30 day'); VACUUM;", SQLCon))
			{
				SQLCon.Open();
				delCmd.ExecuteNonQuery();
				SQLCon.Close();
			}
		}
	}
}
