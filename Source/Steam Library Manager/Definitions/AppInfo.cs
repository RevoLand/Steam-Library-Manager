using MahApps.Metro.Controls.Dialogs;
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

        public string GameHeaderImage => $"http://cdn.akamai.steamstatic.com/steam/apps/{AppID}/header.jpg";

        public string PrettyGameSize => Functions.FileSystem.FormatBytes(SizeOnDisk);

        public DirectoryInfo CommonFolder => new DirectoryInfo(Path.Combine(Library.Steam.CommonFolder.FullName, InstallationPath.Name));

        public DirectoryInfo DownloadFolder => new DirectoryInfo(Path.Combine(Library.Steam.DownloadFolder.FullName, InstallationPath.Name));

        public DirectoryInfo WorkShopPath => new DirectoryInfo(Path.Combine(Library.Steam.WorkshopFolder.FullName, "content", AppID.ToString()));

        public FileInfo CompressedArchiveName => new FileInfo(Path.Combine(Library.Steam.SteamAppsFolder.FullName, AppID + ".zip"));

        public FileInfo FullAcfPath => new FileInfo(Path.Combine(Library.Steam.SteamAppsFolder.FullName, AcfName));

        public FileInfo WorkShopAcfPath => new FileInfo(Path.Combine(Library.Steam.WorkshopFolder.FullName, WorkShopAcfName));

        public string AcfName => $"appmanifest_{AppID}.acf";

        public string WorkShopAcfName => $"appworkshop_{AppID}.acf";

        public Framework.AsyncObservableCollection<FrameworkElement> ContextMenuItems => GenerateRightClickMenuItems();

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
                    await Task.Run(() => DeleteFilesAsync());
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

        public List<FileSystemInfo> GetCommonFiles() => CommonFolder.EnumerateFileSystemInfos("*", SearchOption.AllDirectories).Where(x => x is FileInfo).ToList();

        public List<FileSystemInfo> GetDownloadFiles() => DownloadFolder.EnumerateFileSystemInfos("*", SearchOption.AllDirectories).Where(x => x is FileInfo).ToList();

        public List<FileSystemInfo> GetPatchFiles() => Library.Steam.DownloadFolder.EnumerateFileSystemInfos($"*{AppID}*.patch", SearchOption.TopDirectoryOnly).Where(x => x is FileInfo).ToList();

        public List<FileSystemInfo> GetWorkshopFiles() => WorkShopPath.EnumerateFileSystemInfos("*", SearchOption.AllDirectories).Where(x => x is FileInfo).ToList();

        public async void CopyFilesAsync(List.TaskInfo CurrentTask, CancellationToken cancellationToken)
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
                            while (Framework.TaskManager.Paused)
                            {
                                await Task.Delay(100);
                            }

                            string newFileName = currentFile.FullName.Substring(Library.Steam.SteamAppsFolder.FullName.Length + 1);

                            CurrentTask.TaskStatusInfo = $"Compressing: {currentFile.Name} ({Functions.FileSystem.FormatBytes(((FileInfo)currentFile).Length)})";
                            compressed.CreateEntryFromFile(currentFile.FullName, newFileName, CompressionLevel.Optimal);
                            CurrentTask.MovedFileSize += ((FileInfo)currentFile).Length;

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
                        while (Framework.TaskManager.Paused)
                        {
                            await Task.Delay(100);
                        }

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

                    Parallel.ForEach(AppFiles.Where(x => (x as FileInfo).Length > Properties.Settings.Default.ParallelAfterSize * 1000000).OrderByDescending(x => (x as FileInfo).Length), parallelOptions, CurrentFile =>
                    {
                        FileInfo NewFile = new FileInfo(CurrentFile.FullName.Replace(Library.Steam.SteamAppsFolder.FullName, CurrentTask.TargetLibrary.Steam.SteamAppsFolder.FullName));

                        if (!NewFile.Exists || (NewFile.Length != ((FileInfo)CurrentFile).Length || NewFile.LastWriteTime != ((FileInfo)CurrentFile).LastWriteTime))
                        {
                            if (!NewFile.Directory.Exists)
                            {
                                NewFile.Directory.Create();
                                CreatedDirectories.Add(NewFile.Directory.FullName);
                            }

                            int currentBlockSize = 0;
                            byte[] FSBuffer = new byte[1024 * 1024];

                            using (FileStream CurrentFileContent = ((FileInfo)CurrentFile).Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                using (FileStream NewFileContent = NewFile.OpenWrite())
                                {
                                    while ((currentBlockSize = CurrentFileContent.Read(FSBuffer, 0, FSBuffer.Length)) > 0)
                                    {
                                        if (cancellationToken.IsCancellationRequested)
                                            throw (new OperationCanceledException(cancellationToken));

                                        while (Framework.TaskManager.Paused)
                                        {
                                            Task.Delay(100);
                                        }

                                        NewFileContent.Write(FSBuffer, 0, currentBlockSize);

                                        CurrentTask.MovedFileSize += currentBlockSize;
                                        CurrentTask.TaskStatusInfo = $"Copying: {CurrentFile.Name} ({NewFileContent.Length}/{((FileInfo)CurrentFile).Length})";
                                    }
                                }
                            }
                        }
                        else
                            CurrentTask.MovedFileSize += NewFile.Length;

                        CopiedFiles.Add(NewFile.FullName);

                        if (CurrentTask.ReportFileMovement)
                        {
                            LogToTM($"[{AppName}] File moved: {NewFile.FullName}");
                        }

                        Functions.Logger.LogToFile(Functions.Logger.LogType.App, $"File moved: {NewFile.FullName}", this);
                    });

                    parallelOptions.MaxDegreeOfParallelism = -1;

                    Parallel.ForEach(AppFiles.Where(x => (x as FileInfo).Length <= Properties.Settings.Default.ParallelAfterSize * 1000000).OrderByDescending(x => (x as FileInfo).Length), parallelOptions, CurrentFile =>
                    {
                        FileInfo NewFile = new FileInfo(CurrentFile.FullName.Replace(Library.Steam.SteamAppsFolder.FullName, CurrentTask.TargetLibrary.Steam.SteamAppsFolder.FullName));

                        if (!NewFile.Exists || (NewFile.Length != ((FileInfo)CurrentFile).Length || NewFile.LastWriteTime != ((FileInfo)CurrentFile).LastWriteTime))
                        {
                            if (!NewFile.Directory.Exists)
                            {
                                NewFile.Directory.Create();
                                CreatedDirectories.Add(NewFile.Directory.FullName);
                            }

                            int currentBlockSize = 0;
                            byte[] FSBuffer = new byte[1024 * 1024];

                            using (FileStream CurrentFileContent = ((FileInfo)CurrentFile).Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                using (FileStream NewFileContent = NewFile.OpenWrite())
                                {
                                    while ((currentBlockSize = CurrentFileContent.Read(FSBuffer, 0, FSBuffer.Length)) > 0)
                                    {
                                        if (cancellationToken.IsCancellationRequested)
                                            throw (new OperationCanceledException(cancellationToken));

                                        while (Framework.TaskManager.Paused)
                                        {
                                            Task.Delay(100);
                                        }

                                        NewFileContent.Write(FSBuffer, 0, currentBlockSize);

                                        CurrentTask.MovedFileSize += currentBlockSize;
                                        CurrentTask.TaskStatusInfo = $"Copying: {CurrentFile.Name} ({NewFileContent.Length}/{((FileInfo)CurrentFile).Length})";
                                    }
                                }
                            }
                        }
                        else
                            CurrentTask.MovedFileSize += NewFile.Length;

                        CopiedFiles.Add(NewFile.FullName);

                        if (CurrentTask.ReportFileMovement)
                        {
                            LogToTM($"[{AppName}] File moved: {NewFile.FullName}");
                        }

                        Functions.Logger.LogToFile(Functions.Logger.LogType.App, $"File moved: {NewFile.FullName}", this);
                    });

                }

                CurrentTask.ElapsedTime.Stop();
                CurrentTask.MovedFileSize = TotalFileSize;

                LogToTM($"[{AppName}] Time elapsed: {CurrentTask.ElapsedTime.Elapsed} - Average speed: {Math.Round(((TotalFileSize / 1024f) / 1024f) / CurrentTask.ElapsedTime.Elapsed.TotalSeconds, 3)} MB/sec - Average file size: {Functions.FileSystem.FormatBytes(TotalFileSize / (long)CurrentTask.TotalFileCount)}");
                Functions.Logger.LogToFile(Functions.Logger.LogType.App, $"Movement completed in {CurrentTask.ElapsedTime.Elapsed} with Average Speed of {Math.Round(((TotalFileSize / 1024f) / 1024f) / CurrentTask.ElapsedTime.Elapsed.TotalSeconds, 3)} MB/sec - Average file size: {Functions.FileSystem.FormatBytes(TotalFileSize / (long)CurrentTask.TotalFileCount)}", this);
            }
            catch (OperationCanceledException oex)
            {
                if (!CurrentTask.ErrorHappened)
                {
                    CurrentTask.ErrorHappened = true;
                    Framework.TaskManager.Stop();
                    CurrentTask.Active = false;
                    CurrentTask.Completed = true;

                    await Main.FormAccessor.AppPanel.Dispatcher.Invoke(async delegate
                    {
                        if (await Main.FormAccessor.ShowMessageAsync("Remove moved files?", $"[{AppName}] Game movement cancelled. Would you like to remove files that already moved from target library?", MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Affirmative)
                        {
                            Functions.FileSystem.RemoveGivenFiles(CopiedFiles, CreatedDirectories, CurrentTask);
                        }
                    }, System.Windows.Threading.DispatcherPriority.Normal);

                    LogToTM($"[{AppName}] Operation cancelled by user. Time Elapsed: {CurrentTask.ElapsedTime.Elapsed}");
                    Functions.Logger.LogToFile(Functions.Logger.LogType.App, $"Operation cancelled by user. Time Elapsed: {CurrentTask.ElapsedTime.Elapsed}", this);
                }
            }
            catch (Exception ex)
            {
                CurrentTask.ErrorHappened = true;
                Framework.TaskManager.Stop();
                CurrentTask.Active = false;
                CurrentTask.Completed = true;

                await Main.FormAccessor.AppPanel.Dispatcher.Invoke(async delegate
                 {
                     if (await Main.FormAccessor.ShowMessageAsync("Remove moved files?", $"[{AppName}] An error happened while moving game files. Would you like to remove files that already moved from target library?", MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Affirmative)
                     {
                         Functions.FileSystem.RemoveGivenFiles(CopiedFiles, CreatedDirectories, CurrentTask);
                     }
                 }, System.Windows.Threading.DispatcherPriority.Normal);


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

        public async Task<bool> DeleteFilesAsync(List.TaskInfo CurrentTask = null)
        {
            try
            {
                if (IsCompressed)
                {
                    await Task.Run(() => CompressedArchiveName.Delete());
                }
                else
                {
                    List<FileSystemInfo> gameFiles = GetFileList();

                    Parallel.ForEach(gameFiles, currentFile =>
                    {
                        if (currentFile.Exists)
                        {
                            if (CurrentTask != null)
                            {
                                while (Framework.TaskManager.Paused)
                                {
                                    Task.Delay(100);
                                }

                                CurrentTask.TaskStatusInfo = $"Deleting: {currentFile.Name} ({Functions.FileSystem.FormatBytes(((FileInfo)currentFile).Length)})";
                                Main.FormAccessor.TaskManager_Logs.Add($"[{DateTime.Now}] [{CurrentTask.App.AppName}] Deleting file: {currentFile.FullName}");
                            }

                            File.SetAttributes(currentFile.FullName, FileAttributes.Normal);
                            currentFile.Delete();
                        }
                    }
                    );

                    // common folder, if exists
                    if (CommonFolder.Exists)
                    {
                        await Task.Run(() => CommonFolder.Delete(true));
                    }

                    // downloading folder, if exists
                    if (DownloadFolder.Exists)
                    {
                        await Task.Run(() => DownloadFolder.Delete(true));
                    }

                    // workshop folder, if exists
                    if (WorkShopPath.Exists)
                    {
                        await Task.Run(() => WorkShopPath.Delete(true));
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

                    if (CurrentTask != null)
                    {
                        CurrentTask.TaskStatusInfo = "";
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
