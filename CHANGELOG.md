# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/).

## [1.6.0.4] - 2020-12-7

### Changed

* Updated nuget packages

### Fixed

* The specified executable is not a valid application for this OS platform ([#88](https://github.com/RevoLand/Steam-Library-Manager/issues/88))

## [1.6.0.3] - 2019-11-3

### Fixed

* Versioning.

## [1.6.0.2] - 2019-10-29

### Added

* [AlphaFS](https://github.com/alphaleonis/AlphaFS) . NET library to replace System. IO namespace

### Changed

* Updated nuget packages
* Replaced every System. IO method with AlphaFS, also replaced the file copy method with AlphaFS too.
* Renamed "Remove Original Files" as "Move"
* ContextMenu (aka right click menu) borders are back.

### Fixed

* "System. IndexOutOfRangeException" when copying apps.
* Enabling/Disabling a library not updating the library panel
* Double clicking to open installation directory for Uplay games not working.

### Removed

* [FileCopyLib](https://www.nuget.org/packages/FileCopyLib/) nuget package
* Home panel
* Donate & Discord buttons.

## [1.6.0.1] - 2019-09-10

### Changed

* Updated translations
* Updated nuget packages

### Fixed

* Creating more than one SLM library fails

## [1.6.0.0] - 2019-08-11

### Added

* Support for more library types: Uplay (#23, #65, #71)
* Ability to continue on error for task manager tasks ([#68](https://github.com/RevoLand/Steam-Library-Manager/issues/68))
* Ability to skip restart warning for Steam library related tasks ([#69](https://github.com/RevoLand/Steam-Library-Manager/issues/69))
* Icons! [(Have a sneak-peek)](https://dl.dropboxusercontent.com/s/e9ruwj4f11yg5pn/21-Sunday-vB67mD7A1020.gif)
* Ability to Compress Origin games
* Ability to 'Auto Installation' for Origin related tasks
* HamburgerMenu Addition to Library Panel for library type switching ([#71](https://github.com/RevoLand/Steam-Library-Manager/issues/71))
* Duplicate Game Finder/Cleaner for Steam libraries ([#73](https://github.com/RevoLand/Steam-Library-Manager/issues/73))
* Ability to Enable/Disable Library Support (Steam, Origin, Uplay) - ([#63](https://github.com/RevoLand/Steam-Library-Manager/issues/63))
* Library loading indicator ([#63](https://github.com/RevoLand/Steam-Library-Manager/issues/63))
* Installation Wizard ([#63](https://github.com/RevoLand/Steam-Library-Manager/issues/63))
* Check for backup updates: Origin & Uplay

### Changed

* Library Creation dialog is replaced with a flyout panel which clears the path for supporting more library types. (#63 #65)
* Unified Task Manager's List View for Steam & Origin games for easier editing in future.
* Tweaked Task Manager UI a little bit
* Library Cleaner UI improved
* Settings UI improved

### Fixed

* "Remove from SLM" Context Menu Item not working with created Origin libraries
* Checking for library existence for libraries not working as intended.
* Showing Steam failover images for Origin games in List View if the image is not loaded correctly.
* Disk Space with Mounted Volumes ([#72](https://github.com/RevoLand/Steam-Library-Manager/issues/72))

### Removed

* Unused libraries (NumericUpDownLib & FontAwesome)
* Ability to disable parallel file transfers per-task. (#64)

## [1.5.1.10] - 2019-07-17

### Fixed

* Threading error with Origin installations (InvalidOperationException)
* "Remove from SLM" Context Menu Item not working with SLM libraries
* Compact tasks getting deleted at the end if "Remove Files at Source directory" option enabled in global task options
* Compact size detection for some locales

## [1.5.1.9] - 2019-07-09

### Added

* Ability to disable parallel file transfers per-task. (#64)

### Fixed

* A crucial bug which could potentially lead to losing game files with compact task.

## [1.5.1.8] - 2019-06-27

### Added

* Auto clear completed tasks
* Ability to Ignore Junks (in Library Cleaner)
* Compact Status Detection On/Off switch (default off for better performance)

### Changed

* UI Improvements:
  + Tooltips (Translation keys for tooltips will be added later.)
  + Library Cleaner: Reason why junk
  + Library type logo in library panel
  + Replace "Create Library" button with "Create Library / Remove Library / Refresh Library" buttons
* Loading of Origin header images (Load from/save as "appid".jpg rather than a complicated store page id or whatsoever)
* Updated translations ([Crowdin](https://crowdin.com/project/steam-library-manager) for translation)

## [1.5.1.7] - 2019-05-31

### Fixed

* 'The given key was not present in the dictionary' on compress tasks. (Issue: #50)
* Task Manager UI for compression related tasks.
* Can't task an item for compact if the item is already tasked for compression (and vice-versa)

## [1.5.1.6] - 2019-05-31

### Added

* Size detection for compressed games with compact *(Huge performance impact on libraries with lots of games, Needs to be enabled from Settings - Disabled by default)*

### Fixed

* Compact tasks are not pausing properly.
* Offline Origin libraries not showing correctly.
* Offline Origin libraries are not becoming online when it should be.
* NotSupportedException on adding/creating a new library.

## [1.5.1.5] - 2019-05-28

### Changed

* Compact task (compression) improvements such as compressing all files, reporting task status, handling task cancellation etc.

### Fixed

* SLM finds an update of the current version upon manual check (Issue #44)

## [1.5.1.4] - 2019-05-21

### Added

* New Task Type: Compact - Windows compact function (Beta, please report errors you encounter)
  + Compress/Uncompress
  + Show compact status
* Russian language (Credits to MrDubstep863, thank you!)
* [CliWrap](https://github.com/Tyrrrz/CliWrap)

### Changed

* Task Manager UI tweaked

## [1.5.1.3] - 2019-04-27

### Fixed

* No button to decompress a game after it was compressed. (Issue: #41)
* Incorrect task status message on game compressing.

## [1.5.1.2] - 2019-04-24

### Fixed

* System. NotSupportedException: This type of CollectionView does not support changes to its SourceCollection from a thread different from the Dispatcher thread.
  + Steam_Library_Manager. Functions. App. AddSteamApp
  + Steam_Library_Manager. Definitions. SteamLibrary. UpdateJunks() (Issue: #40)

## [1.5.1.1] - 2019-04-19

### Added

* Task Manager Logs Auto Scroll on/off switch

### Fixed

* (Task Manager) Tasked item crashes with error: System. IndexOutOfRangeException: Index was outside the bounds of the array.
* RuntimeBinderException

## [1.5.1.0] - 2019-04-06

### Added

* Localization support
  + Supported Languages: English, Turkish (Help us [Translate!](https://crowdin.com/project/steam-library-manager))
* Language selector
* Steam UserID selector (to fetch Last Play time)
* Auto Scrolling for logs in the Task Manager tab *(Use with caution on games with lots of small files)*
* Order by Last Play time for Steam games
* Include Supporters list
* Number of "Pending, Completed, Total" tasks in Task Manager
* Ability to change task options at once for multiple tasks in Task Manager (Compress / Remove Original Files)

### Changed

* Target framework version changed to 4.6.2 from 4.5 ([NET Framework Web Installer](https://www.microsoft.com/en-us/download/details.aspx?id=53345))
* Minor tweaks on Task Manager UI for both Grid & List view

### Fixed

* Broken Task Manager UI on Deleting Origin games with Task Manager option

### Removed

* Suggestion form button, use Discord if needed.
* Unused networking code
* Custom theming support
* [SharpRaven](https://github.com/getsentry/raven-csharp)
* [ColorPickerLib](https://github.com/Dirkster99/ColorPickerLib)

## [1.5.0.15] - 2019-01-03

### Fixed

* Issue #3 : Startup Error on Origin Games with manifest version v4.0

## [1.5.0.14] - 2018-12-27

### Added

* [AutoUpdater. NET](https://github.com/ravibpatel/AutoUpdater.NET) to use for updating.
* Locale selection for Origin game installation from available locales.

### Removed

* Self coded auto-updater mechanism.

## [1.5.0.13] - 2018-12-25

### Added

* [FileCopyLib](https://www.nuget.org/packages/FileCopyLib/) to use for file copying.

### Fixed

* [Issue #20: Transfer speed over a 10Gig network caps out at 600mb/s](https://github.com/RevoLand/Steam-Library-Manager/issues/20)

## [1.5.0.12] - 2018-08-12

### Added

* Compress/Decompress within the same library.
* Support for Origin manifest file version 3.0

## [1.5.0.11] - 2018-06-29

### Added

* CompressionLevel setting for Compressing apps.
* Suggestion Form button to get feedback for future of SLM.

### Changed

* Handling of PathTooLongException in CopyFilesAsync (*The specified path, file name, or both are too long. The fully qualified file name must be less than 260 characters, and the directory name must be less than 248 characters.*)
* Handling of UnauthorizedAccessException in SteamLibrary. AddNew (*Отказано в доступе по пути "H:\Program Files (x86)\Steam.dll".*)
* Handling of FileNotFoundException in CopyFilesAsync while compressing files. (*Could not find file 'J:\SteamLibrary\SteamApps\common\Commandos 3 Destination Berlin\Legacy\Data2. PCK'.*)

### Fixed

* InvalidOperationException in LibraryGrid_Drop
* KeyNotFoundException (#22 - *The given key was not present in the dictionary.*)
* FileNotFoundException in AddSteamApp (*Could not find file 'S:\Game\Steam\SteamBackup\SteamApps\586200.zip'.*)
* IOException in CopyFilesAsync (*The process cannot access the file 'E:\SteamArchive\SteamApps\378540.zip' because it is being used by another process.*)
* InvalidOperationException in UpdateAppList (*Collection was modified; enumeration operation may not execute.*)
* DivideByZeroException in CopyFilesAsync function. (*Attempted to divide by zero.*)
* ArgumentNullException in SteamAppInfo. GetFileList function (*Value cannot be null. Parameter name: collection*)
* Custom Theme options also changes the current theme.
* IOException (*The process cannot access the file 'C:\Users\revoland\Downloads\.slmcache\CustomTheme.xaml' because it is being used by another process.*)
* NullReferenceException in TaskManager_ContextMenu_Click (*Object reference not set to an instance of an object.*)

## [1.5.0.10] - 2018-06-03

### Added

* Library Creation Button (Thank you [Catalin Chelariu](http://www.softpedia.com/editors/browse/catalin-chelariu) for suggestion)
* Settings tab to tabcontrol

### Changed

* Steam restart function updated
* File movement order

### Fixed

* Reflection of disk size changes
* "If you click check for backup updates while a backup is being performed, the game you are backing up will reappear" - Thank you Mobeeuz, like always.
* "Remove from List" origin context menu item appearing for Steam / SLM libraries.
* SLM Libraries being duplicated on save.

### Removed

* Settings button from top right corner.

## [1.5.0.9] - 2018-05-29

### Added

* NLog (<https://github.com/NLog/NLog)>

### Changed

* Buffer size for file movement
* SaveWindowPlacement is set to True
* Parsed form parts into user controls for easier access & edit.

### Removed

* Custom file logger
* ConfigureAwait calls

## [1.5.0.8] - 2018-05-26

### Added

* Discord button
* Patreon button

### Changed

* Handling of DirectoryNotFoundException in Steam Library - UpdateAppList

### Fixed

* Proper pausing on Task Manager
* Same goes for the Origin releated tasks

## [1.5.0.7] - 2018-05-25

### Fixed

* Fix for memory overflow happens on task manager when task is paused.

## [1.5.0.6] - 2018-01-13

### Added

* Custom styling support

### Fixed

* FileNotFoundException happens on getting version info.
* InvalidOperationException happens on getting junk files.
* IOException happens on library cleaner.
* DirectoryNotFoundException and IOException on GetCommonFiles.
* ArgumentNullException caused by IOException on DeleteFilesAsync method.
* ArgumentOutOfRangeException happens on generating Steam library list.

## [1.5.0.5] - 2018-01-08

### Fixed

* FileNotFoundException happens on file removal which caused by cached file properties.
* InvalidOperationException happens on updating junk list.
* Workshop files for tasked items are being detected by junk cleaner.
* ArgumentException happens on getting disk details for mapped network locations.(Haven't tried mapped location yet, not sure if it works or not)
* ArgumentOutOfRangeException on generating SLM library list.
* Handling of UnauthorizedAccessException on CopyFilesAsync/Steam method.
* InvalidOperationException happens on Updating application list for Steam.

## [1.5.0.4] - 2018-01-06

### Added

* Handled UnauthorizedAccessException on Steam. CopyFiles method

### Fixed

* FileNotFoundException happens on file removal which caused by cached file properties.
* IOException happens on getting directory info in case the device is not ready.
* DriveNotFoundException happens with offline libraries.
* ArgumentException in AddNew library function(?)

## [1.5.0.3] - 2018-01-03

### Changed

* Task Manager UI
  + Changed showing of current file info and file movement info.

### Fixed

* Check for backup updates function
* Steam generated backups are not visible if there is a SLM generated backup in the same library.

## [1.5.0.2] - 2018-01-03

### Changed

* . Net Framework target version to 4.5 from 4.6
* ACF file detection - using the manual method now.

### Fixed

* Win32Exception caused by context menu actions.
* DirectoryNotFoundException caused by application file list generation.
* ArgumentException caused by getting libraries' drive info.
* A possible memory leak happens during file movement.
* File attributes which was broken since v1.5
* Check for Backup Updates function

### Removed

* TaskbarItemInfo which was used to current task's progress in taskbar.
* FileSystemWatcher which was used to detect . ACF file changes on libraries.
* [Resourcer. Fody](https://github.com/Fody/Resourcer) as it is not being used currently and not supported in . Net Framework 4.5
