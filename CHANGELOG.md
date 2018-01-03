# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/).

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