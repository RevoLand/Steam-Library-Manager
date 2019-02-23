# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/).

## [Unreleased]

### Added

- Localization support
  - Supported Languages: English, Turkish (Help us [Translate!](https://crowdin.com/project/steam-library-manager))
- Language selector
- Steam UserID selector
- Auto Scrolling for logs in the Task Manager tab
- Order by Last Play time for Steam games.
- Supporters tab.

### Changed

- Target framework version changed to 4.6.2 from 4.5 ([NET Framework Web Installer](https://www.microsoft.com/en-us/download/details.aspx?id=53345))

### Deprecated

- Custom theming support

### Removed

- Suggestion form button, use Discord if needed.
- Unused networking code
- [SharpRaven](https://github.com/getsentry/raven-csharp)

## [1.5.0.15] - 2019-01-03

### Fixed

- Issue #3 : Startup Error on Origin Games with manifest version v4.0

## [1.5.0.14] - 2018-12-27

### Added

- [AutoUpdater.NET](https://github.com/ravibpatel/AutoUpdater.NET) to use for updating.
- Locale selection for Origin game installation from available locales.

### Removed

- Self coded auto-updater mechanism.

## [1.5.0.13] - 2018-12-25

### Added

-  [FileCopyLib](https://www.nuget.org/packages/FileCopyLib/) to use for file copying.

### Fixed

- [Issue #20: Transfer speed over a 10Gig network caps out at 600mb/s](https://github.com/RevoLand/Steam-Library-Manager/issues/20)

## [1.5.0.12] - 2018-08-12

### Added

- Compress/Decompress within the same library.
- Support for Origin manifest file version 3.0

## [1.5.0.11] - 2018-06-29

### Added

- CompressionLevel setting for Compressing apps.
- Suggestion Form button to get feedback for future of SLM.

### Changed

- Handling of PathTooLongException in CopyFilesAsync (*The specified path, file name, or both are too long. The fully qualified file name must be less than 260 characters, and the directory name must be less than 248 characters.*)
- Handling of UnauthorizedAccessException in SteamLibrary.AddNew (*Отказано в доступе по пути "H:\Program Files (x86)\Steam.dll".*)
- Handling of FileNotFoundException in CopyFilesAsync while compressing files. (*Could not find file 'J:\SteamLibrary\SteamApps\common\Commandos 3 Destination Berlin\Legacy\Data2.PCK'.*)

### Fixed

- InvalidOperationException in LibraryGrid_Drop
- KeyNotFoundException (#22 - *The given key was not present in the dictionary.*)
- FileNotFoundException in AddSteamApp (*Could not find file 'S:\Game\Steam\SteamBackup\SteamApps\586200.zip'.*)
- IOException in CopyFilesAsync (*The process cannot access the file 'E:\SteamArchive\SteamApps\378540.zip' because it is being used by another process.*)
- InvalidOperationException in UpdateAppList (*Collection was modified; enumeration operation may not execute.*)
- DivideByZeroException in CopyFilesAsync function. (*Attempted to divide by zero.*)
- ArgumentNullException in SteamAppInfo.GetFileList function (*Value cannot be null. Parameter name: collection*)
- Custom Theme options also changes the current theme.
- IOException (*The process cannot access the file 'C:\Users\revoland\Downloads\.slmcache\CustomTheme.xaml' because it is being used by another process.*)
- NullReferenceException in TaskManager_ContextMenu_Click (*Object reference not set to an instance of an object.*)

## [1.5.0.10] - 2018-06-03

### Added

- Library Creation Button (Thank you [Catalin Chelariu](http://www.softpedia.com/editors/browse/catalin-chelariu) for suggestion)
- Settings tab to tabcontrol

### Changed

- Steam restart function updated
- File movement order

### Fixed

- Reflection of disk size changes
- "If you click check for backup updates while a backup is being performed, the game you are backing up will reappear" - Thank you Mobeeuz, like always.
- "Remove from List" origin context menu item appearing for Steam / SLM libraries.
- SLM Libraries being duplicated on save.

### Removed

- Settings button from top right corner.

## [1.5.0.9] - 2018-05-29

### Added

- NLog (https://github.com/NLog/NLog)

### Changed

- Buffer size for file movement
- SaveWindowPlacement is set to True
- Parsed form parts into user controls for easier access & edit.

### Removed

- Custom file logger
- ConfigureAwait calls

## [1.5.0.8] - 2018-05-26

### Added

- Discord button
- Patreon button

### Changed

- Handling of DirectoryNotFoundException in Steam Library - UpdateAppList

### Fixed

- Proper pausing on Task Manager
- Same goes for the Origin releated tasks

## [1.5.0.7] - 2018-05-25

### Fixed

- Fix for memory overflow happens on task manager when task is paused.

## [1.5.0.6] - 2018-01-13

### Added

- Custom styling support

### Fixed

- FileNotFoundException happens on getting version info.
- InvalidOperationException happens on getting junk files.
- IOException happens on library cleaner.
- DirectoryNotFoundException and IOException on GetCommonFiles.
- ArgumentNullException caused by IOException on DeleteFilesAsync method.
- ArgumentOutOfRangeException happens on generating Steam library list.

## [1.5.0.5] - 2018-01-08

### Fixed

- FileNotFoundException happens on file removal which caused by cached file properties.
- InvalidOperationException happens on updating junk list.
- Workshop files for tasked items are being detected by junk cleaner.
- ArgumentException happens on getting disk details for mapped network locations.(Haven't tried mapped location yet, not sure if it works or not)
- ArgumentOutOfRangeException on generating SLM library list.
- Handling of UnauthorizedAccessException on CopyFilesAsync/Steam method.
- InvalidOperationException happens on Updating application list for Steam.

## [1.5.0.4] - 2018-01-06

### Added

- Handled UnauthorizedAccessException on Steam.CopyFiles method

### Fixed

- FileNotFoundException happens on file removal which caused by cached file properties.
- IOException happens on getting directory info in case the device is not ready.
- DriveNotFoundException happens with offline libraries.
- ArgumentException in AddNew library function(?)

## [1.5.0.3] - 2018-01-03

### Changed

- Task Manager UI
  - Changed showing of current file info and file movement info.

### Fixed

- Check for backup updates function
- Steam generated backups are not visible if there is a SLM generated backup in the same library.

## [1.5.0.2] - 2018-01-03

### Changed

- .Net Framework target version to 4.5 from 4.6
- ACF file detection - using the manual method now.

### Fixed

- Win32Exception caused by context menu actions.
- DirectoryNotFoundException caused by application file list generation.
- ArgumentException caused by getting libraries' drive info.
- A possible memory leak happens during file movement.
- File attributes which was broken since v1.5
- Check for Backup Updates function

### Removed

- TaskbarItemInfo which was used to current task's progress in taskbar.
- FileSystemWatcher which was used to detect .ACF file changes on libraries.
- [Resourcer.Fody](https://github.com/Fody/Resourcer) as it is not being used currently and not supported in .Net Framework 4.5