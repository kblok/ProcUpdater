# ProcUpdater
Stored Procedure updater (in case you hate waiting for Visual Studio Schema Compare).

This console app grabs all the stored procedures store in a directory and performs a `CREATE OR ALTER` in the database.
When in watch mode ProcUpdater will keep your store procedures up-to-date so you won't waste several minutes waiting for the Visual Studio Schema update to perform the compare/update process.

If you're a Visual Studio SQL Project user who codes lots of Stored Procedures, this app will save you hours!

# Usage

```
ProcUpdater.exe [options]
```

# Options

| Option        | appsetttings.json| Command Argument  |
| ------------- |-------------| -----|
| Connection String      | ProcUpdater:ConnectionString | -conn |
| Stored Procedures Path      | ProcUpdater:StoredProceduresPath      | -path |
| Stay Alive | ProcUpdater:StayAlive      |  -alive |
| Verbose | ProcUpdater:Verbose      |  -verbose |
| FileWatch | ProcUpdater:FileWatch      |  -watch |

# Download
| Platform        | Link|
| ------------- |-------------|
| Windows 10 x64 | https://www.dropbox.com/s/qknpsbso650cn1b/ProcUpdater-0.1.Win10-x64.zip?dl=0 |
| MacOS 10.11 x64      | https://www.dropbox.com/s/udsk5nhy90ulbok/ProcUpdater-0.1-MacOS.zip?dl=0 |
