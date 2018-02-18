# What is this?
If you do not need the complexity of a backup program and want to use a sync tool to create backups with
the option to keep a maximum number of files.

# Usage:
To use it you need to call the application using comamnd line arguments:
-p "Z:\Backup\" <-- path to the backup base folder.
-m 2 <-- maximum amount of backups to have at any case

Best case is to run this tool in front of any backup or directly after a backup. If you run it after the
backup keep in mind to set your backup maximum count to one more because the software is made to be
called before any backup starts. Reason is, that the tool is checking the amount of backups and moves
or deletes them to fullfill the maximum amount of backups before a new one starts. So if you configured
3 backups the software is only keeping the last two backups and creating a new current folder to prepare
a new sync backup.

# How it works:
At first run the program creates only the current folder and is not doing anything else. After the first
run it is always checking using a database which is created in the root folder. Inside this file (SQLite)
all information about the existing backup folders are stored. So do not delete this file as the software
would not delete old folders in that case (only touches known folders).

# Properties:
You can configure the backup folder names inside the BackupSyncFolder.exe.conf file in the application
folder for all backups. This does not effekt old backups but the current backup folder. So the current
folder name should not be changed after it ran the first time.

# File sync.sqlite:
This file is needed because it holds all backup information. If the current folder is moved this file is
copied into the moved folder to make sure we have a backup of it too.