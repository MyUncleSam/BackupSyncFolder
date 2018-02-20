# What is 'BackupSyncFolder'?
This tool gives you the possibility to keep X numbers of backups of a given folder.

# Why?
There are some applications which are able to be backed up but have no backup control. In most cases you only need to make sure that you have an given amount of backups. If you use e.g. Microsoft SQL Server you can specify a job backing up all files to a folder managed by this tool. The tool itselfe is moving the files to the needed location as soon as it is called.

# Example folder structure:
![folder structure](https://github.com/MyUncleSam/BackupSyncFolder/blob/master/BackupSyncFolder/Screenshots/FolderStructure.png?raw=true "folder structure")
1. SQlite Database which keeps all information about the folder structure to make sure that only folders created and managed by this tool are monitored.
2. The "current" folder is the target for your backup procedure. The files in it is copied into the backup duplicate structure (see 3).
3. List of Backup duplicates which are managed by the tool. If the max amount of backups are hit the oldest entries are deleted to make sure to fullfill the max backup parameter. If a folder is created and all files from the current folder got moved a backup of the sqlite database is also made into this folder.


# Why I did this
Till now I used the free backup solution Cobian Backup which was really great. But as this software stopped to continue and there is no free solution to replace it I wanted to have something like that. In my case I use it to backup a SQL server and keep the last 14 copies of it.
