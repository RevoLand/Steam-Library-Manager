using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Steam_Library_Manager.Definitions
{
    public class AppInfo
    {
        public Library Library { get; set; }
        public Enums.GameType GameType { get; set; }

        public int AppID { get; set; }
        public string AppName { get; set; }
        public DirectoryInfo InstallationPath;
        public long SizeOnDisk { get; set; }
        public bool IsCompressed { get; set; }
        public DateTime LastUpdated { get; set; }

        public string GameHeaderImage
        {
            get => $"http://cdn.akamai.steamstatic.com/steam/apps/{AppID}/header.jpg";
        }

        public string PrettyGameSize
        {
            get => Functions.FileSystem.FormatBytes(SizeOnDisk);
        }

        public DirectoryInfo CommonFolder
        {
            get => new DirectoryInfo(Path.Combine(Library.Steam.CommonFolder.FullName, InstallationPath.Name));
        }

        public DirectoryInfo DownloadFolder
        {
            get => new DirectoryInfo(Path.Combine(Library.Steam.DownloadFolder.FullName, InstallationPath.Name));
        }

        public DirectoryInfo WorkShopPath
        {
            get => new DirectoryInfo(Path.Combine(Library.Steam.WorkshopFolder.FullName, "content", AppID.ToString()));
        }

        public FileInfo CompressedArchiveName
        {
            get => new FileInfo(Path.Combine(Library.Steam.SteamAppsFolder.FullName, AppID + ".zip"));
        }

        public FileInfo FullAcfPath
        {
            get => new FileInfo(Path.Combine(Library.Steam.SteamAppsFolder.FullName, AcfName));
        }

        public FileInfo WorkShopAcfPath
        {
            get => new FileInfo(Path.Combine(Library.Steam.WorkshopFolder.FullName, WorkShopAcfName));
        }

        public string AcfName
        {
            get => $"appmanifest_{AppID}.acf";
        }

        public string WorkShopAcfName
        {
            get => $"appworkshop_{AppID}.acf";
        }

        public Framework.AsyncObservableCollection<FrameworkElement> ContextMenuItems
        {
            get => GenerateRightClickMenuItems();
        }

        public Framework.AsyncObservableCollection<FrameworkElement> GenerateRightClickMenuItems()
        {
            Framework.AsyncObservableCollection<FrameworkElement> rightClickMenu = new Framework.AsyncObservableCollection<FrameworkElement>();
            try
            {
                foreach (ContextMenuItem cItem in List.AppCMenuItems.Where(x => x.IsActive))
                {
                    if (IsCompressed && !cItem.ShowToCompressed)
                    {
                        continue;
                    }
                    else if (!cItem.ShowToNormal)
                    {
                        continue;
                    }

                    if (cItem.IsSeparator)
                    {
                        rightClickMenu.Add(new Separator());
                    }
                    else
                    {
                        MenuItem slmItem = new MenuItem()
                        {
                            Tag = this,
                            Header = string.Format(cItem.Header, AppName, AppID, Functions.FileSystem.FormatBytes(SizeOnDisk))
                        };
                        slmItem.Tag = cItem.Action;
                        slmItem.Icon = Functions.FAwesome.GetAwesomeIcon(cItem.Icon, cItem.IconColor);
                        slmItem.HorizontalContentAlignment = HorizontalAlignment.Left;
                        slmItem.VerticalContentAlignment = VerticalAlignment.Center;

                        rightClickMenu.Add(slmItem);
                    }
                }

                return rightClickMenu;
            }
            catch (FormatException ex)
            {
                MessageBox.Show($"An error happened while parsing context menu, most likely happened duo typo on color name.\n\n{ex}");
                Functions.Logger.LogToFile(Functions.Logger.LogType.SLM, $"[{AppName}][{AppID}][{AcfName}] {ex}");

                return rightClickMenu;
            }
        }

        public async void ParseMenuItemActionAsync(string Action)
        {
            switch (Action.ToLowerInvariant())
            {
                default:
                    if (string.IsNullOrEmpty(SLM.UserSteamID64))
                    {
                        return;
                    }

                    Process.Start(string.Format(Action, AppID, SLM.UserSteamID64));
                    break;
                case "disk":
                    if (CommonFolder.Exists)
                    {
                        Process.Start(CommonFolder.FullName);
                    }

                    break;
                case "acffile":
                    Process.Start(FullAcfPath.FullName);
                    break;
                case "deleteappfiles":
                    await Task.Run(() => DeleteFiles());
                    break;
                case "deleteappfilestm":
                    Framework.TaskManager.AddTask(new List.TaskInfo
                    {
                        App = this,
                        TaskType = Enums.TaskType.Delete
                    });
                    break;
            }
        }

        public List<FileSystemInfo> GetFileList(bool includeDownloads = true, bool includeWorkshop = true)
        {
            List<FileSystemInfo> FileList = new List<FileSystemInfo>();

            if (IsCompressed)
            {
                FileList.Add(CompressedArchiveName);
            }
            else
            {
                if (CommonFolder.Exists)
                {
                    FileList.AddRange(GetCommonFiles());
                }

                if (includeDownloads && DownloadFolder.Exists)
                {
                    FileList.AddRange(GetDownloadFiles());
                    FileList.AddRange(GetPatchFiles());
                }

                if (includeWorkshop && WorkShopPath.Exists)
                {
                    FileList.AddRange(GetWorkshopFiles());
                }

                if (FullAcfPath.Exists)
                {
                    FileList.Add(FullAcfPath);
                }

                if (WorkShopAcfPath.Exists)
                {
                    FileList.Add(WorkShopAcfPath);
                }
            }

            return FileList;
        }

        public List<FileSystemInfo> GetCommonFiles()
        {
            return CommonFolder.EnumerateFileSystemInfos("*", SearchOption.AllDirectories).Where(x => x is FileInfo).ToList();
        }

        public List<FileSystemInfo> GetDownloadFiles()
        {
            return DownloadFolder.EnumerateFileSystemInfos("*", SearchOption.AllDirectories).Where(x => x is FileInfo).ToList();
        }

        public List<FileSystemInfo> GetPatchFiles()
        {
            return Library.Steam.DownloadFolder.EnumerateFileSystemInfos($"*{AppID}*.patch", SearchOption.TopDirectoryOnly).Where(x => x is FileInfo).ToList();
        }

        public List<FileSystemInfo> GetWorkshopFiles()
        {
            return WorkShopPath.EnumerateFileSystemInfos("*", SearchOption.AllDirectories).Where(x => x is FileInfo).ToList();
        }

        public void CopyFiles(List.TaskInfo CurrentTask, CancellationToken cancellationToken)
        {
            LogToTM($"[{AppName}] Populating file list, please wait");
            Functions.Logger.LogToFile(Functions.Logger.LogType.App, "Populating file list", this);

            ConcurrentBag<string> CopiedFiles = new ConcurrentBag<string>();
            ConcurrentBag<string> CreatedDirectories = new ConcurrentBag<string>();
            List<FileSystemInfo> AppFiles = GetFileList();
            CurrentTask.TotalFileCount = AppFiles.Count;

            try
            {
                long TotalFileSize = 0;
                ParallelOptions parallelOptions = new ParallelOptions()
                {
                    CancellationToken = cancellationToken
                };

                Parallel.ForEach(AppFiles, parallelOptions, file =>
                {
                    Interlocked.Add(ref TotalFileSize, (file as FileInfo).Length);
                });

                CurrentTask.TotalFileSize = TotalFileSize;
                CurrentTask.ElapsedTime.Start();

                LogToTM($"[{AppName}] File list populated, total files to move: {AppFiles.Count} - total size to move: {Functions.FileSystem.FormatBytes(TotalFileSize)}");
                Functions.Logger.LogToFile(Functions.Logger.LogType.App, $"File list populated, total files to move: {AppFiles.Count} - total size to move: {Functions.FileSystem.FormatBytes(TotalFileSize)}", this);

                // If the game is not compressed and user would like to compress it
                if (!IsCompressed && CurrentTask.Compress)
                {
                    FileInfo compressedArchive = new FileInfo(CompressedArchiveName.FullName.Replace(Library.Steam.SteamAppsFolder.FullName, CurrentTask.TargetLibrary.Steam.SteamAppsFolder.FullName));

                    if (compressedArchive.Exists)
                    {
                        compressedArchive.Delete();
                    }

                    using (ZipArchive compressed = ZipFile.Open(compressedArchive.FullName, ZipArchiveMode.Create))
                    {
                        CopiedFiles.Add(compressedArchive.FullName);

                        foreach (FileSystemInfo currentFile in AppFiles)
                        {
                            string newFileName = currentFile.FullName.Substring(Library.Steam.SteamAppsFolder.FullName.Length + 1);

                            CurrentTask.TaskStatusInfo = $"Compressing: {currentFile.Name} ({Functions.FileSystem.FormatBytes((currentFile as FileInfo).Length)})";
                            compressed.CreateEntryFromFile(currentFile.FullName, newFileName, CompressionLevel.Optimal);
                            CurrentTask.MovedFileSize += (currentFile as FileInfo).Length;

                            if (CurrentTask.ReportFileMovement)
                            {
                                LogToTM($"[{AppName}] Moved file: {newFileName}");
                            }

                            Functions.Logger.LogToFile(Functions.Logger.LogType.App, $"Moved file: {newFileName}", this);

                            if (cancellationToken.IsCancellationRequested)
                            {
                                throw new OperationCanceledException(cancellationToken);
                            }
                        }
                    }
                }
                // If the game is compressed and user would like to decompress it
                else if (IsCompressed && !CurrentTask.Compress)
                {
                    foreach (ZipArchiveEntry currentFile in ZipFile.OpenRead(CompressedArchiveName.FullName).Entries)
                    {
                        FileInfo newFile = new FileInfo(Path.Combine(CurrentTask.TargetLibrary.Steam.SteamAppsFolder.FullName, currentFile.FullName));

                        if (!newFile.Directory.Exists)
                        {
                            newFile.Directory.Create();
                            CreatedDirectories.Add(newFile.Directory.FullName);
                        }

                        CurrentTask.TaskStatusInfo = $"Decompressing: {newFile.Name} ({Functions.FileSystem.FormatBytes(currentFile.Length)})";
                        currentFile.ExtractToFile(newFile.FullName, true);

                        CopiedFiles.Add(newFile.FullName);
                        CurrentTask.MovedFileSize += currentFile.Length;

                        if (CurrentTask.ReportFileMovement)
                        {
                            LogToTM($"[{AppName}] Moved file: {newFile.FullName}");
                        }

                        Functions.Logger.LogToFile(Functions.Logger.LogType.App, $"Moved file: {newFile.FullName}", this);

                        if (cancellationToken.IsCancellationRequested)
                        {
                            throw new OperationCanceledException(cancellationToken);
                        }
                    }
                }
                // Everything else
                else
                {
                    parallelOptions.MaxDegreeOfParallelism = 1;

                    Parallel.ForEach(AppFiles.Where(x => (x as FileInfo).Length > Properties.Settings.Default.ParallelAfterSize * 1000000).OrderByDescending(x => (x as FileInfo).Length), parallelOptions, currentFile =>
                    {
                        FileInfo newFile = new FileInfo(currentFile.FullName.Replace(Library.Steam.SteamAppsFolder.FullName, CurrentTask.TargetLibrary.Steam.SteamAppsFolder.FullName));

                        if (!newFile.Exists || (newFile.Length != (currentFile as FileInfo).Length || newFile.LastWriteTime != (currentFile as FileInfo).LastWriteTime))
                        {
                            if (!newFile.Directory.Exists)
                            {
                                newFile.Directory.Create();
                                CreatedDirectories.Add(newFile.Directory.FullName);
                            }

                            CurrentTask.TaskStatusInfo = $"Copying: {currentFile.Name} ({Functions.FileSystem.FormatBytes((currentFile as FileInfo).Length)})";
                            (currentFile as FileInfo).CopyTo(newFile.FullName, true);
                        }

                        CopiedFiles.Add(newFile.FullName);
                        CurrentTask.MovedFileSize += (currentFile as FileInfo).Length;

                        if (CurrentTask.ReportFileMovement)
                        {
                            LogToTM($"[{AppName}] Moved file: {newFile.FullName}");
                        }

                        Functions.Logger.LogToFile(Functions.Logger.LogType.App, $"Moved file: {newFile.FullName}", this);
                    });

                    parallelOptions.MaxDegreeOfParallelism = -1;

                    Parallel.ForEach(AppFiles.Where(x => (x as FileInfo).Length <= Properties.Settings.Default.ParallelAfterSize * 1000000).OrderByDescending(x => (x as FileInfo).Length), parallelOptions, currentFile =>
                    {
                        FileInfo newFile = new FileInfo(currentFile.FullName.Replace(Library.Steam.SteamAppsFolder.FullName, CurrentTask.TargetLibrary.Steam.SteamAppsFolder.FullName));

                        if (!newFile.Exists || (newFile.Length != (currentFile as FileInfo).Length || newFile.LastWriteTime != (currentFile as FileInfo).LastWriteTime))
                        {
                            if (!newFile.Directory.Exists)
                            {
                                newFile.Directory.Create();
                                CreatedDirectories.Add(newFile.Directory.FullName);
                            }

                            CurrentTask.TaskStatusInfo = $"Copying: {currentFile.Name} ({Functions.FileSystem.FormatBytes((currentFile as FileInfo).Length)})";
                            (currentFile as FileInfo).CopyTo(newFile.FullName, true);
                        }

                        CopiedFiles.Add(newFile.FullName);
                        CurrentTask.MovedFileSize += (currentFile as FileInfo).Length;

                        if (CurrentTask.ReportFileMovement)
                        {
                            LogToTM($"[{AppName}] Moved file: {newFile.FullName}");
                        }

                        Functions.Logger.LogToFile(Functions.Logger.LogType.App, $"Moved file: {newFile.FullName}", this);
                    });

                }

                CurrentTask.ElapsedTime.Stop();
                CurrentTask.MovedFileSize = TotalFileSize;

                LogToTM($"[{AppName}] Time elapsed: {CurrentTask.ElapsedTime.Elapsed} - Average speed: {Math.Round(((TotalFileSize / 1024f) / 1024f) / CurrentTask.ElapsedTime.Elapsed.TotalSeconds, 3)} MB/sec - Average file size: {Functions.FileSystem.FormatBytes(TotalFileSize / (long)CurrentTask.TotalFileCount)}");
                Functions.Logger.LogToFile(Functions.Logger.LogType.App, $"Movement completed in {CurrentTask.ElapsedTime.Elapsed} with Average Speed of {Math.Round(((TotalFileSize / 1024f) / 1024f) / CurrentTask.ElapsedTime.Elapsed.TotalSeconds, 3)} MB/sec - Average file size: {Functions.FileSystem.FormatBytes(TotalFileSize / (long)CurrentTask.TotalFileCount)}", this);
            }
            catch (OperationCanceledException)
            {
                CurrentTask.ErrorHappened = true;
                Framework.TaskManager.Stop();
                CurrentTask.Active = false;
                CurrentTask.Completed = true;

                MessageBoxResult RemoveMovedFiles = MessageBox.Show($"[{AppName}] Game movement cancelled. Would you like to remove files that already moved from target library?", "Remove moved files?", MessageBoxButton.YesNo);

                if (RemoveMovedFiles == MessageBoxResult.Yes)
                {
                    Functions.FileSystem.RemoveGivenFiles(CopiedFiles, CreatedDirectories);
                }

                LogToTM($"[{AppName}] Operation cancelled by user. Time Elapsed: {CurrentTask.ElapsedTime.Elapsed}");
                Functions.Logger.LogToFile(Functions.Logger.LogType.App, $"Operation cancelled by user. Time Elapsed: {CurrentTask.ElapsedTime.Elapsed}", this);
            }
            catch (Exception ex)
            {
                CurrentTask.ErrorHappened = true;
                Framework.TaskManager.Stop();
                CurrentTask.Active = false;
                CurrentTask.Completed = true;

                MessageBox.Show(ex.ToString());
                MessageBoxResult RemoveMovedFiles = MessageBox.Show($"[{AppName}] An error happened while moving game files. Would you like to remove files that already moved from target library?", "Remove moved files?", MessageBoxButton.YesNo);

                if (RemoveMovedFiles == MessageBoxResult.Yes)
                {
                    Functions.FileSystem.RemoveGivenFiles(CopiedFiles, CreatedDirectories);
                }

                Main.FormAccessor.TaskManager_Logs.Add($"[{AppName}] An error happened while moving game files. Time Elapsed: {CurrentTask.ElapsedTime.Elapsed}");
                Functions.Logger.LogToFile(Functions.Logger.LogType.SLM, $"[{AppName}][{AppID}][{AcfName}] {ex}");
            }
        }

        public void LogToTM(string TextToLog)
        {
            try
            {
                Main.FormAccessor.TaskManager_Logs.Add($"[{DateTime.Now}] {TextToLog}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Functions.Logger.LogToFile(Functions.Logger.LogType.SLM, $"[{AppName}][{AppID}][{AcfName}] {ex}");
            }
        }

        public bool DeleteFiles(List.TaskInfo Task = null)
        {
            try
            {
                if (IsCompressed)
                {
                    CompressedArchiveName.Delete();
                }
                else
                {
                    List<FileSystemInfo> gameFiles = GetFileList();

                    Parallel.ForEach(gameFiles, currentFile =>
                    {
                        if (currentFile.Exists)
                        {
                            if (Task != null)
                            {
                                Task.TaskStatusInfo = $"Deleting: {currentFile.Name} ({Functions.FileSystem.FormatBytes((currentFile as FileInfo).Length)})";
                                Main.FormAccessor.TaskManager_Logs.Add($"[{DateTime.Now}] [{Task.App.AppName}] Deleting file: {currentFile.FullName}");
                            }

                            File.SetAttributes(currentFile.FullName, FileAttributes.Normal);
                            currentFile.Delete();
                        }
                    }
                    );

                    // common folder, if exists
                    if (CommonFolder.Exists)
                    {
                        CommonFolder.Delete(true);
                    }

                    // downloading folder, if exists
                    if (DownloadFolder.Exists)
                    {
                        DownloadFolder.Delete(true);
                    }

                    // workshop folder, if exists
                    if (WorkShopPath.Exists)
                    {
                        WorkShopPath.Delete(true);
                    }

                    // game .acf file
                    if (FullAcfPath.Exists)
                    {
                        File.SetAttributes(FullAcfPath.FullName, FileAttributes.Normal);
                        FullAcfPath.Delete();
                    }

                    // workshop .acf file
                    if (WorkShopAcfPath.Exists)
                    {
                        File.SetAttributes(WorkShopAcfPath.FullName, FileAttributes.Normal);
                        WorkShopAcfPath.Delete();
                    }

                    if (Task != null)
                    {
                        Task.TaskStatusInfo = "";
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Functions.Logger.LogToFile(Functions.Logger.LogType.SLM, $"[{AppName}][{AppID}][{AcfName}] {ex}");

                return false;
            }
        }
    }

}
