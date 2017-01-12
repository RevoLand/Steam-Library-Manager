using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Steam_Library_Manager.Definitions
{
    public class Game
    {
        public int AppID { get; set; }
        public string AppName { get; set; }
        public DirectoryInfo InstallationPath;
        public long SizeOnDisk { get; set; }
        public bool IsCompressed { get; set; }
        public bool IsSteamBackup { get; set; }
        public Library InstalledLibrary { get; set; }

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
            get => new DirectoryInfo(Path.Combine(InstalledLibrary.CommonFolder.FullName, InstallationPath.Name));
        }

        public DirectoryInfo DownloadFolder
        {
            get => new DirectoryInfo(Path.Combine(InstalledLibrary.DownloadFolder.FullName, InstallationPath.Name));
        }

        public DirectoryInfo WorkShopPath
        {
            get => new DirectoryInfo(Path.Combine(InstalledLibrary.WorkshopFolder.FullName, "content", AppID.ToString()));
        }

        public FileInfo CompressedArchiveName
        {
            get => new FileInfo(Path.Combine(InstalledLibrary.SteamAppsFolder.FullName, AppID + ".zip"));
        }

        public FileInfo FullAcfPath
        {
            get => new FileInfo(Path.Combine(InstalledLibrary.SteamAppsFolder.FullName, AcfName));
        }

        public FileInfo WorkShopAcfPath
        {
            get => new FileInfo(Path.Combine(InstalledLibrary.WorkshopFolder.FullName, WorkShopAcfName));
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
                foreach (ContextMenuItem cItem in List.GameCMenuItems.Where(x => x.IsActive))
                {
                    if (IsSteamBackup && cItem.ShowToSteamBackup == Enums.MenuVisibility.NotVisible)
                        continue;
                    else if (InstalledLibrary.IsBackup && cItem.ShowToSLMBackup == Enums.MenuVisibility.NotVisible)
                        continue;
                    else if (IsCompressed && cItem.ShowToCompressed == Enums.MenuVisibility.NotVisible)
                        continue;
                    else if (cItem.ShowToNormal == Enums.MenuVisibility.NotVisible)
                        continue;

                    if (cItem.IsSeparator)
                        rightClickMenu.Add(new Separator());
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

                return rightClickMenu;
            }
        }

        public void ParseMenuItemAction(string Action)
        {
            switch (Action.ToLowerInvariant())
            {
                default:
                    if (string.IsNullOrEmpty(SLM.userSteamID64))
                        return;

                    System.Diagnostics.Process.Start(string.Format(Action, AppID, SLM.userSteamID64));
                    break;
                case "disk":
                    if (CommonFolder.Exists)
                        System.Diagnostics.Process.Start(CommonFolder.FullName);
                    break;
                case "acffile":
                    System.Diagnostics.Process.Start(FullAcfPath.FullName);
                    break;
                case "deletegamefilesslm":

                    DeleteFiles();
                    RemoveFromLibrary();

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
            }
            return FileList;
        }

        public List<FileSystemInfo> GetCommonFiles()
        {
            return CommonFolder.GetFileSystemInfos("*", SearchOption.AllDirectories).Where(x => !x.Attributes.HasFlag(FileAttributes.Directory)).ToList();
        }

        public List<FileSystemInfo> GetDownloadFiles()
        {
            return DownloadFolder.GetFileSystemInfos("*", SearchOption.AllDirectories).Where(x => !x.Attributes.HasFlag(FileAttributes.Directory)).ToList();
        }

        public List<FileSystemInfo> GetPatchFiles()
        {
            return InstalledLibrary.DownloadFolder.GetFileSystemInfos("*", SearchOption.AllDirectories).Where(x => !x.Attributes.HasFlag(FileAttributes.Directory)).ToList();
        }

        public List<FileSystemInfo> GetWorkshopFiles()
        {
            return WorkShopPath.GetFileSystemInfos("*", SearchOption.AllDirectories).Where(x => !x.Attributes.HasFlag(FileAttributes.Directory)).ToList();
        }

        public void CopyGameFiles(List.TaskList currentTask, CancellationToken cancellationToken)
        {
            LogtoTaskManager($"[{AppName}] Populating file list, please wait");

            ConcurrentBag<string> copiedFiles = new ConcurrentBag<string>();
            ConcurrentBag<string> createdDirectories = new ConcurrentBag<string>();
            List<FileSystemInfo> gameFiles = GetFileList();
            currentTask.ProgressBarMax = gameFiles.Count;
            System.Diagnostics.Stopwatch timeElapsed = new System.Diagnostics.Stopwatch();
            timeElapsed.Start();

            try
            {
                long totalFileSize = 0;
                ParallelOptions parallelOptions = new ParallelOptions()
                {
                    CancellationToken = cancellationToken
                };

                Parallel.ForEach(gameFiles, parallelOptions, file =>
                {
                    Interlocked.Add(ref totalFileSize, (file as FileInfo).Length);
                });

                LogtoTaskManager($"[{AppName}] File list populated, total files to move: {gameFiles.Count} - total size to move: {Functions.FileSystem.FormatBytes(totalFileSize)}");

                if (!IsCompressed && currentTask.Compress)
                {
                    FileInfo compressedArchive = new FileInfo(CompressedArchiveName.FullName.Replace(InstalledLibrary.SteamAppsFolder.FullName, currentTask.TargetLibrary.SteamAppsFolder.FullName));

                    if (compressedArchive.Exists)
                        compressedArchive.Delete();

                    using (ZipArchive compressed = ZipFile.Open(compressedArchive.FullName, ZipArchiveMode.Create))
                    {
                        copiedFiles.Add(compressedArchive.FullName);
                        currentTask.ProgressBar = copiedFiles.Count;

                        foreach (FileSystemInfo currentFile in gameFiles)
                        {
                            string newFileName = currentFile.FullName.Substring(InstalledLibrary.SteamAppsFolder.FullName.Length + 1);

                            compressed.CreateEntryFromFile(currentFile.FullName, newFileName, CompressionLevel.Optimal);

                            if (currentTask.ReportFileMovement)
                                LogtoTaskManager($"[{AppName}][{copiedFiles.Count}/{currentTask.ProgressBarMax}] Moven file: {newFileName}");

                            if (cancellationToken.IsCancellationRequested)
                                throw new OperationCanceledException(cancellationToken);
                        }

                        compressed.CreateEntryFromFile(FullAcfPath.FullName, AcfName);
                    }
                }
                else if (IsCompressed && !currentTask.Compress)
                {
                    foreach (ZipArchiveEntry currentFile in ZipFile.OpenRead(CompressedArchiveName.FullName).Entries)
                    {
                        FileInfo newFile = new FileInfo(Path.Combine(currentTask.TargetLibrary.SteamAppsFolder.FullName, currentFile.FullName));

                        if (!newFile.Directory.Exists)
                        {
                            newFile.Directory.Create();
                            createdDirectories.Add(newFile.Directory.FullName);
                        }

                        currentFile.ExtractToFile(newFile.FullName, true);

                        copiedFiles.Add(newFile.FullName);
                        currentTask.ProgressBar = copiedFiles.Count;

                        if (currentTask.ReportFileMovement)
                            LogtoTaskManager($"[{AppName}][{copiedFiles.Count}/{currentTask.ProgressBarMax}] Moven file: {newFile.FullName}");

                        if (cancellationToken.IsCancellationRequested)
                            throw new OperationCanceledException(cancellationToken);
                    }
                }
                else
                {
                    Parallel.ForEach(gameFiles.Where(x => (x as FileInfo).Length <= Properties.Settings.Default.ParallelAfterSize), parallelOptions, currentFile =>
                    {
                        FileInfo newFile = new FileInfo(currentFile.FullName.Replace(InstalledLibrary.SteamAppsFolder.FullName, currentTask.TargetLibrary.SteamAppsFolder.FullName));

                        if (!newFile.Exists || (newFile.Length != (currentFile as FileInfo).Length || newFile.LastWriteTime != (currentFile as FileInfo).LastWriteTime))
                        {
                            if (!newFile.Directory.Exists)
                            {
                                newFile.Directory.Create();
                                createdDirectories.Add(newFile.Directory.FullName);
                            }

                            (currentFile as FileInfo).CopyTo(newFile.FullName, true);
                        }

                        copiedFiles.Add(newFile.FullName);
                        currentTask.ProgressBar = copiedFiles.Count;

                        if (currentTask.ReportFileMovement)
                            LogtoTaskManager($"[{AppName}][{copiedFiles.Count}/{currentTask.ProgressBarMax}] Moven file: {newFile.FullName}");
                    });

                    parallelOptions.MaxDegreeOfParallelism = 1;

                    Parallel.ForEach(gameFiles.Where(x => (x as FileInfo).Length > Properties.Settings.Default.ParallelAfterSize), parallelOptions, currentFile =>
                    {
                        FileInfo newFile = new FileInfo(currentFile.FullName.Replace(InstalledLibrary.SteamAppsFolder.FullName, currentTask.TargetLibrary.SteamAppsFolder.FullName));

                        if (!newFile.Exists || (newFile.Length != (currentFile as FileInfo).Length || newFile.LastWriteTime != (currentFile as FileInfo).LastWriteTime))
                        {
                            if (!newFile.Directory.Exists)
                            {
                                newFile.Directory.Create();
                                createdDirectories.Add(newFile.Directory.FullName);
                            }

                            (currentFile as FileInfo).CopyTo(newFile.FullName, true);
                        }

                        copiedFiles.Add(newFile.FullName);
                        currentTask.ProgressBar = copiedFiles.Count;

                        if (currentTask.ReportFileMovement)
                            LogtoTaskManager($"[{AppName}][{copiedFiles.Count}/{currentTask.ProgressBarMax}] Moven file: {newFile.FullName}");
                    });

                    if (!IsCompressed)
                    {
                        // Copy .ACF file
                        if (FullAcfPath.Exists)
                        {
                            FullAcfPath.CopyTo(Path.Combine(currentTask.TargetLibrary.SteamAppsFolder.FullName, AcfName), true);

                            copiedFiles.Add(Path.Combine(currentTask.TargetLibrary.SteamAppsFolder.FullName, AcfName));
                            currentTask.ProgressBar = copiedFiles.Count;

                            if (currentTask.ReportFileMovement)
                                LogtoTaskManager($"[{AppName}][{copiedFiles.Count}/{currentTask.ProgressBarMax}] Moven file: {Path.Combine(currentTask.TargetLibrary.SteamAppsFolder.FullName, AcfName)}");
                        }

                        if (WorkShopAcfPath.Exists)
                        {
                            FileInfo newACFPath = new FileInfo(WorkShopAcfPath.FullName.Replace(InstalledLibrary.SteamAppsFolder.FullName, currentTask.TargetLibrary.SteamAppsFolder.FullName));

                            if (!newACFPath.Directory.Exists)
                            {
                                newACFPath.Directory.Create();
                                createdDirectories.Add(newACFPath.Directory.FullName);
                            }

                            WorkShopAcfPath.CopyTo(newACFPath.FullName, true);

                            copiedFiles.Add(newACFPath.FullName);
                            currentTask.ProgressBar = copiedFiles.Count;

                            if (currentTask.ReportFileMovement)
                                LogtoTaskManager($"[{AppName}][{copiedFiles.Count}/{currentTask.ProgressBarMax}] Moven file: {newACFPath.FullName}");
                        }
                    }
                }

                timeElapsed.Stop();
                currentTask.ProgressBar = currentTask.ProgressBarMax;

                LogtoTaskManager($"[{AppName}] Time elapsed: {timeElapsed.Elapsed} - Average: {Math.Round(((totalFileSize / 1024f) / 1024f) / timeElapsed.Elapsed.TotalSeconds, 3)} MB/sec");
            }
            catch (OperationCanceledException)
            {
                Framework.TaskManager.Stop();
                currentTask.Moving = false;
                currentTask.Completed = true;

                MessageBoxResult removeMovenFiles = MessageBox.Show($"[{AppName}] Game movement cancelled. Would you like to remove files that already moven?", "Remove moven files?", MessageBoxButton.YesNo);

                if (removeMovenFiles == MessageBoxResult.Yes)
                    Functions.FileSystem.RemoveGivenFiles(copiedFiles, createdDirectories);

                LogtoTaskManager($"[{AppName}] Operation cancelled by user. Time Elapsed: {timeElapsed.Elapsed}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                MessageBoxResult removeMovenFiles = MessageBox.Show($"[{AppName}] An error happened while moving game files. Would you like to remove files that already moven?", "Remove moven files?", MessageBoxButton.YesNo);

                if (removeMovenFiles == MessageBoxResult.Yes)
                    Functions.FileSystem.RemoveGivenFiles(copiedFiles, createdDirectories);

                MainWindow.Accessor.TaskManager_Logs.Add($"[{AppName}] An error happened while moving game files. Time Elapsed: {timeElapsed.Elapsed}");
            }
        }

        public void LogtoTaskManager(string TextToLog)
        {
            MainWindow.Accessor.TaskManager_Logs.Add(TextToLog);
        }

        public bool DeleteFiles()
        {
            try
            {
                if (IsCompressed)
                {
                    CompressedArchiveName.Delete();
                }
                else if (IsSteamBackup)
                {
                    if (InstallationPath.Exists)
                        InstallationPath.Delete(true);
                }
                else
                {
                    List<FileSystemInfo> gameFiles = GetFileList();

                    Parallel.ForEach(gameFiles, currentFile =>
                    {
                        if (currentFile.Exists)
                            currentFile.Delete();
                    }
                    );

                    // common folder, if exists
                    if (CommonFolder.Exists)
                        CommonFolder.Delete(true);

                    // downloading folder, if exists
                    if (DownloadFolder.Exists)
                        DownloadFolder.Delete(true);

                    // workshop folder, if exists
                    if (WorkShopPath.Exists)
                        WorkShopPath.Delete(true);

                    // game .acf file
                    if (FullAcfPath.Exists)
                        FullAcfPath.Delete();

                    // workshop .acf file
                    if (WorkShopAcfPath.Exists)
                        WorkShopAcfPath.Delete();
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());

                return false;
            }
        }

        public void RemoveFromLibrary()
        {
            InstalledLibrary.Games.Remove(this);

            InstalledLibrary.UpdateLibraryVisual();

            if (SLM.selectedLibrary == InstalledLibrary)
                Functions.Games.UpdateMainForm(InstalledLibrary);
        }
    }
}
