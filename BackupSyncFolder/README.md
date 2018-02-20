# Command line parameters:
To use it you need to call the application using comamnd line arguments:
* -p 
  * Example: "Z:\Backup\"
  * path to the backup base folder.
* -m
  * Example: "2"
  * maximum amount of backups to have at any case

#And what these parameters do:
-p
This parameter specifies the base backup directory. Inside this directory the tool is creating its
structure like the current folder and the duplicate folders. It also creates the database file there
to make sure this tool can be used for multiple backup folders.

-m
You have to specify the maximum amount of backup duplicates to have. This setting is a little bit
tricky on the first view. If you e.g. set it to 3 it will delete old folders to have a maximum duplicate
count of 2. But why is it doing that? The parameter is called "maximum amount of backups". And because
this tool is meant to be called directly before a new backup is created it has to make sure the maximum
backups are correct. And in this case the new backup directly after the tool is called is the 3rd one.

This is very importent if your backup is very huge and you have limited backup space. With this you can
calculate if your backup could be stored or not. But you can let this tool run after your backup too. In
this case you have to keep in mind to increase your backup count by 1 to keep the number of backups you
want. But also keep an eye on the size because during the backup you have one more till the tool deletes
the no longer needed ones.

# The database file 'syncdb.sqlite':
First of all the tool creates for each folder an own database file to make sure it can be used for as
much folders as you need without the need to copy the whole tool.
The file itselfe is the most importent one for the backup procedure. The main reason is, that inside this
database the software keeps the information about the folders it created. Having this information the
tool is only accessing/modifying or deleting known folders to avoid damage to unknown files. By every run
of this tool a backup of the database is also created in the new duplicates folder. Directly after that
a small cleanup job on the database is made which is e.g. deleting all log entries which are older then
30 days.

# Structure:
* FOLDERS
  * FOLDERNAME: incremental path to all known folders
  * ISACTIVE: 1=is an existing backup folder, 0=no longer active and got deleted
  * CREATIONDATE: of the database entry - so can be different to folder creation date
  * REMOVEDATE: date when the folder got deleted by this tool
* LOGGING
  * Timestamp: exactly that
  * FOLDERNAME: the folder from FOLDERS
  * message: log information like success or error messages

# BackupSyncFolder.exe.conf:
Inside this file you can specify globale parameters which affect ALL backup folders. The parameters are:
* BackupSubfolderName
  * specifies the name of the target backup folder
  * This should not be changed if you already used this tool. But if you do make sure to rename all current folders!
  * default 'current'
* DuplicateDateNaming
  * how duplicate folders should be named using the datetime format
  * Format: https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
  * Can be changed at any cases because the database knows the old folders too.
  * default: 'yyyyMMdd HHmmss'
* DbName
  * Name of the database file which is used inside the folders
  * This should not be changed if you already used this tool. But if you do make sure to rename all db files!
  * default: 'syncdb.sqlite'
