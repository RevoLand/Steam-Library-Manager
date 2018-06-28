using MahApps.Metro.Controls.Dialogs;
using System;
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
    public class SteamAppInfo
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public Library Library { get; set; }

        public int AppID { get; set; }
        public string AppName { get; set; }
        public DirectoryInfo InstallationDirectory;
        public long SizeOnDisk { get; set; }
        public bool IsCompressed { get; set; }
        public bool IsSteamBackup { get; set; }
        public DateTime LastUpdated { get; set; }

        public string GameHeaderImage => $"http://cdn.akamai.steamstatic.com/steam/apps/{AppID}/header.jpg";

        public string PrettyGameSize => Functions.FileSystem.FormatBytes(SizeOnDisk);

        public DirectoryInfo CommonFolder => new DirectoryInfo(Path.Combine(Library.Steam.CommonFolder.FullName, InstallationDirectory.Name));

        public DirectoryInfo DownloadFolder => new DirectoryInfo(Path.Combine(Library.Steam.DownloadFolder.FullName, InstallationDirectory.Name));

        public DirectoryInfo WorkShopPath => new DirectoryInfo(Path.Combine(Library.Steam.WorkshopFolder.FullName, "content", AppID.ToString()));

        public FileInfo CompressedArchiveName => new FileInfo(Path.Combine(Library.Steam.SteamAppsFolder.FullName, AppID + ".zip"));

        public FileInfo FullAcfPath => new FileInfo(Path.Combine(Library.Steam.SteamAppsFolder.FullName, AcfName));

        public FileInfo WorkShopAcfPath => new FileInfo(Path.Combine(Library.Steam.WorkshopFolder.FullName, WorkShopAcfName));

        public string AcfName => $"appmanifest_{AppID}.acf";

        public string WorkShopAcfName => $"appworkshop_{AppID}.acf";

        public List<FrameworkElement> ContextMenuItems
        {
            get
            {
                var rightClickMenu = new List<FrameworkElement>();
                try
                {
                    foreach (ContextMenuItem cItem in List.AppCMenuItems.Where(x => x.IsActive && x.LibraryType == Enums.LibraryType.Steam))
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
                            MenuItem SLMItem = new MenuItem()
                            {
                                Tag = this,
                                Header = string.Format(cItem.Header, AppName, AppID, Functions.FileSystem.FormatBytes(SizeOnDisk))
                            };
                            SLMItem.Tag = cItem.Action;
                            SLMItem.Icon = Functions.FAwesome.GetAwesomeIcon(cItem.Icon, cItem.IconColor);
                            SLMItem.HorizontalContentAlignment = HorizontalAlignment.Left;
                            SLMItem.VerticalContentAlignment = VerticalAlignment.Center;
                            SLMItem.Click += Main.FormAccessor.AppCMenuItem_Click;

                            rightClickMenu.Add(SLMItem);
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
        }

        public async void ParseMenuItemActionAsync(string Action)
        {
            try
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
                        CommonFolder.Refresh();

                        if (CommonFolder.Exists)
                        {
                            Process.Start(CommonFolder.FullName);
                        }

                        break;

                    case "acffile":
                        FullAcfPath.Refresh();

                        if (FullAcfPath.Exists)
                            Process.Start(FullAcfPath.FullName);
                        break;

                    case "deleteappfiles":
                        await Task.Run(() => DeleteFilesAsync());

                        Library.Steam.Apps.Remove(this);
                        Functions.SLM.Library.UpdateLibraryVisual();

                        if (SLM.CurrentSelectedLibrary == Library)
                            Functions.App.UpdateAppPanel(Library);
                        break;

                    case "deleteappfilestm":
                        Framework.TaskManager.AddTask(new List.TaskInfo
                        {
                            SteamApp = this,
                            TargetLibrary = Library,
                            TaskType = Enums.TaskType.Delete
                        });
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
            }
        }

        public List<FileInfo> GetFileList(bool includeDownloads = true, bool includeWorkshop = true)
        {
            try
            {
                List<FileInfo> FileList = new List<FileInfo>();

                if (IsCompressed)
                {
                    FileList.Add(CompressedArchiveName);
                }
                else
                {
                    CommonFolder.Refresh();

                    if (CommonFolder.Exists)
                    {
                        FileList.AddRange(GetCommonFiles());
                    }

                    DownloadFolder.Refresh();
                    if (includeDownloads && DownloadFolder.Exists)
                    {
                        FileList.AddRange(GetDownloadFiles());
                        FileList.AddRange(GetPatchFiles());
                    }

                    WorkShopPath.Refresh();
                    if (includeWorkshop && WorkShopPath.Exists)
                    {
                        FileList.AddRange(GetWorkshopFiles());
                    }

                    FullAcfPath.Refresh();
                    if (FullAcfPath.Exists)
                    {
                        FileList.Add(FullAcfPath);
                    }

                    WorkShopPath.Refresh();
                    if (WorkShopAcfPath.Exists)
                    {
                        FileList.Add(WorkShopAcfPath);
                    }
                }

                return FileList;
            }
            catch (Exception ex)
            {
                SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
                return null;
            }
        }

        public List<FileInfo> GetCommonFiles()
        {
            try
            {
                CommonFolder.Refresh();
                return CommonFolder.EnumerateFiles("*", SearchOption.AllDirectories).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error happened while populating files at common directory for game: {AppName} - Directory:\n{CommonFolder.FullName}\nError:\n{ex.Message}");
                return null;
            }
        }

        public List<FileInfo> GetDownloadFiles() => DownloadFolder.EnumerateFiles("*", SearchOption.AllDirectories).ToList();

        public List<FileInfo> GetPatchFiles() => Library.Steam.DownloadFolder.EnumerateFiles($"*{AppID}*.patch", SearchOption.TopDirectoryOnly).ToList();

        public List<FileInfo> GetWorkshopFiles() => WorkShopPath.EnumerateFiles("*", SearchOption.AllDirectories).ToList();

        public async Task CopyFilesAsync(List.TaskInfo CurrentTask, CancellationToken cancellationToken)
        {
            LogToTM($"[{AppName}] Populating file list, please wait");
            logger.Info("Populating file list for: {0}", AppName);

            List<string> CopiedFiles = new List<string>();
            List<string> CreatedDirectories = new List<string>();
            List<FileInfo> AppFiles = GetFileList();
            CurrentTask.TotalFileCount = AppFiles.Count;
            long TotalFileSize = 0;

            try
            {
                ParallelOptions POptions = new ParallelOptions()
                {
                    CancellationToken = cancellationToken
                };

                Parallel.ForEach(AppFiles, POptions, file => Interlocked.Add(ref TotalFileSize, file.Length));

                CurrentTask.TotalFileSize = TotalFileSize;
                CurrentTask.ElapsedTime.Start();

                LogToTM($"[{AppName}] File list populated, total files to move: {AppFiles.Count} - total size to move: {Functions.FileSystem.FormatBytes(TotalFileSize)}");
                logger.Info("File list populated, total files to move: {0} - total size to move: {1}", AppFiles.Count, Functions.FileSystem.FormatBytes(TotalFileSize));

                // If the game is not compressed and user would like to compress it
                if (!IsCompressed && CurrentTask.Compress)
                {
                    FileInfo CompressedArchive = new FileInfo(CompressedArchiveName.FullName.Replace(Library.Steam.SteamAppsFolder.FullName, CurrentTask.TargetLibrary.Steam.SteamAppsFolder.FullName));

                    if (CompressedArchive.Exists)
                    {
                        CompressedArchive.Delete();
                    }

                    using (ZipArchive Archive = ZipFile.Open(CompressedArchive.FullName, ZipArchiveMode.Create))
                    {
                        CopiedFiles.Add(CompressedArchive.FullName);

                        foreach (FileSystemInfo currentFile in AppFiles)
                        {
                            CurrentTask.mre.WaitOne();

                            string FileNameInArchive = currentFile.FullName.Substring(Library.Steam.SteamAppsFolder.FullName.Length + 1);

                            CurrentTask.TaskStatusInfo = $"Compressing: {currentFile.Name} ({Functions.FileSystem.FormatBytes(((FileInfo)currentFile).Length)})";
                            Archive.CreateEntryFromFile(currentFile.FullName, FileNameInArchive, Properties.Settings.Default.CompressionLevel.ParseEnum<CompressionLevel>());
                            CurrentTask.MovedFileSize += ((FileInfo)currentFile).Length;

                            if (CurrentTask.ReportFileMovement)
                            {
                                LogToTM($"[{AppName}] Compressed file: {FileNameInArchive}");
                            }

                            logger.Info("Compressed file: {0}", FileNameInArchive);

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
                    foreach (ZipArchiveEntry CurrentFile in ZipFile.OpenRead(CompressedArchiveName.FullName).Entries)
                    {
                        CurrentTask.mre.WaitOne();

                        FileInfo NewFile = new FileInfo(Path.Combine(CurrentTask.TargetLibrary.Steam.SteamAppsFolder.FullName, CurrentFile.FullName));

                        if (!NewFile.Directory.Exists)
                        {
                            NewFile.Directory.Create();
                            CreatedDirectories.Add(NewFile.Directory.FullName);
                        }

                        CurrentTask.TaskStatusInfo = $"Decompressing: {NewFile.Name} ({Functions.FileSystem.FormatBytes(CurrentFile.Length)})";
                        CurrentFile.ExtractToFile(NewFile.FullName, true);

                        CopiedFiles.Add(NewFile.FullName);
                        CurrentTask.MovedFileSize += CurrentFile.Length;

                        if (CurrentTask.ReportFileMovement)
                        {
                            LogToTM($"[{AppName}] Decompressed file: {NewFile.FullName}");
                        }

                        logger.Info("Decompressed file: {0}", NewFile.FullName);

                        if (cancellationToken.IsCancellationRequested)
                        {
                            throw new OperationCanceledException(cancellationToken);
                        }
                    }
                }
                // Everything else
                else
                {
                    POptions.MaxDegreeOfParallelism = 1;

                    Parallel.ForEach(AppFiles.Where(x => (x).Length > Properties.Settings.Default.ParallelAfterSize * 1000000).OrderByDescending(x => (x).Length), POptions, CurrentFile =>
                    {
                        try
                        {
                            var NewFile = new FileInfo(CurrentFile.FullName.Replace(Library.Steam.SteamAppsFolder.FullName, CurrentTask.TargetLibrary.Steam.SteamAppsFolder.FullName));

                            if (!NewFile.Exists || (NewFile.Length != CurrentFile.Length || NewFile.LastWriteTime != CurrentFile.LastWriteTime))
                            {
                                if (!NewFile.Directory.Exists)
                                {
                                    NewFile.Directory.Create();
                                    CreatedDirectories.Add(NewFile.Directory.FullName);
                                }

                                int currentBlockSize = 0;
                                byte[] FSBuffer = new byte[4096];

                                using (FileStream CurrentFileContent = CurrentFile.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                {
                                    using (FileStream NewFileContent = NewFile.OpenWrite())
                                    {
                                        CurrentTask.TaskStatusInfo = $"Copying: {CurrentFile.Name} ({Functions.FileSystem.FormatBytes(CurrentFile.Length)})";

                                        while ((currentBlockSize = CurrentFileContent.Read(FSBuffer, 0, FSBuffer.Length)) > 0)
                                        {
                                            if (cancellationToken.IsCancellationRequested)
                                                throw (new OperationCanceledException(cancellationToken));

                                            CurrentTask.mre.WaitOne();

                                            NewFileContent.Write(FSBuffer, 0, currentBlockSize);

                                            CurrentTask.MovedFileSize += currentBlockSize;
                                        }
                                    }

                                    NewFile.LastWriteTime = CurrentFile.LastWriteTime;
                                    NewFile.LastAccessTime = CurrentFile.LastAccessTime;
                                    NewFile.CreationTime = CurrentFile.CreationTime;
                                }
                            }
                            else
                            {
                                CurrentTask.MovedFileSize += NewFile.Length;
                            }

                            CopiedFiles.Add(NewFile.FullName);

                            if (CurrentTask.ReportFileMovement)
                            {
                                LogToTM($"[{AppName}] File moved: {NewFile.FullName}");
                            }

                            logger.Info("File moved: {0}", NewFile.FullName);
                        }
                        catch (IOException ex)
                        {
                            CurrentTask.ErrorHappened = true;
                            Framework.TaskManager.Stop();
                            CurrentTask.Active = false;
                            CurrentTask.Completed = true;

                            Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                            {
                                if (await Main.FormAccessor.ShowMessageAsync("Remove moved files?", $"[{AppName}] An error releated to file system is happened while moving files.\n\nError: {ex.Message}.\n\nWould you like to remove files that already moved from target library?", MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Affirmative)
                                {
                                    Functions.FileSystem.RemoveGivenFiles(CopiedFiles, CreatedDirectories, CurrentTask);
                                }
                            }, System.Windows.Threading.DispatcherPriority.Normal);

                            Main.FormAccessor.TaskManager_Logs.Add($"[{AppName}] An error releated to file system is happened while moving files. Error: {ex.Message}.");
                            logger.Fatal(ex);

                            SLM.RavenClient.CaptureAsync(new SharpRaven.Data.SentryEvent(ex));
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                            {
                                if (await Main.FormAccessor.ShowMessageAsync("Remove moved files?", $"[{AppName}] An error releated to file permissions happened during file movement. Running SLM as Administrator might help.\n\nError: {ex.Message}.\n\nWould you like to remove files that already moved from target library?", MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Affirmative)
                                {
                                    Functions.FileSystem.RemoveGivenFiles(CopiedFiles, CreatedDirectories, CurrentTask);
                                }
                            }, System.Windows.Threading.DispatcherPriority.Normal);
                        }
                    });

                    POptions.MaxDegreeOfParallelism = Environment.ProcessorCount;

                    Parallel.ForEach(AppFiles.Where(x => (x).Length <= Properties.Settings.Default.ParallelAfterSize * 1000000).OrderByDescending(x => (x).Length), POptions, CurrentFile =>
                    {
                        try
                        {
                            if (cancellationToken.IsCancellationRequested)
                                throw (new OperationCanceledException(cancellationToken));

                            CurrentTask.mre.WaitOne();

                            var NewFile = new FileInfo(CurrentFile.FullName.Replace(Library.Steam.SteamAppsFolder.FullName, CurrentTask.TargetLibrary.Steam.SteamAppsFolder.FullName));

                            if (!NewFile.Exists || (NewFile.Length != CurrentFile.Length || NewFile.LastWriteTime != CurrentFile.LastWriteTime))
                            {
                                CurrentTask.TaskStatusInfo = $"Copying: {CurrentFile.Name} ({Functions.FileSystem.FormatBytes(CurrentFile.Length)})";

                                if (!NewFile.Directory.Exists)
                                {
                                    NewFile.Directory.Create();
                                    CreatedDirectories.Add(NewFile.Directory.FullName);
                                }

                                int currentBlockSize = 0;
                                byte[] FSBuffer = new byte[4096];
                                using (FileStream CurrentFileContent = CurrentFile.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                {
                                    using (FileStream NewFileContent = NewFile.Create())
                                    {
                                        while ((currentBlockSize = CurrentFileContent.Read(FSBuffer, 0, FSBuffer.Length)) > 0)
                                        {
                                            NewFileContent.Write(FSBuffer, 0, currentBlockSize);

                                            CurrentTask.MovedFileSize += currentBlockSize;
                                        }
                                    }

                                    NewFile.LastWriteTime = CurrentFile.LastWriteTime;
                                    NewFile.LastAccessTime = CurrentFile.LastAccessTime;
                                    NewFile.CreationTime = CurrentFile.CreationTime;
                                }
                            }
                            else
                            {
                                CurrentTask.MovedFileSize += NewFile.Length;
                            }

                            CopiedFiles.Add(NewFile.FullName);

                            if (CurrentTask.ReportFileMovement)
                            {
                                LogToTM($"[{AppName}] File moved: {NewFile.FullName}");
                            }

                            logger.Info("File moved: {0}", NewFile.FullName);
                        }
                        catch (IOException ex)
                        {
                            CurrentTask.ErrorHappened = true;
                            Framework.TaskManager.Stop();
                            CurrentTask.Active = false;
                            CurrentTask.Completed = true;

                            Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                            {
                                if (await Main.FormAccessor.ShowMessageAsync("Remove moved files?", $"[{AppName}] An error releated to file system is happened while moving files.\n\nError: {ex.Message}.\n\nWould you like to remove files that already moved from target library?", MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Affirmative)
                                {
                                    Functions.FileSystem.RemoveGivenFiles(CopiedFiles, CreatedDirectories, CurrentTask);
                                }
                            }, System.Windows.Threading.DispatcherPriority.Normal);

                            Main.FormAccessor.TaskManager_Logs.Add($"[{AppName}] An error releated to file system is happened while moving files. Error: {ex.Message}.");
                            logger.Fatal(ex);

                            SLM.RavenClient.CaptureAsync(new SharpRaven.Data.SentryEvent(ex));
                        }
                        catch (UnauthorizedAccessException uaex)
                        {
                            Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                            {
                                if (await Main.FormAccessor.ShowMessageAsync("Remove moved files?", $"[{AppName}] An error releated to file permissions happened during file movement. Running SLM as Administrator might help.\n\nError: {uaex.Message}.\n\nWould you like to remove files that already moved from target library?", MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Affirmative)
                                {
                                    Functions.FileSystem.RemoveGivenFiles(CopiedFiles, CreatedDirectories, CurrentTask);
                                }
                            }, System.Windows.Threading.DispatcherPriority.Normal);
                        }
                    });
                }

                CurrentTask.ElapsedTime.Stop();
                CurrentTask.MovedFileSize = TotalFileSize;

                LogToTM($"[{AppName}] Time elapsed: {CurrentTask.ElapsedTime.Elapsed} - Average speed: {Math.Round(((TotalFileSize / 1024f) / 1024f) / CurrentTask.ElapsedTime.Elapsed.TotalSeconds, 3)} MB/sec - Average file size: {Functions.FileSystem.FormatBytes(TotalFileSize / (long)CurrentTask.TotalFileCount)}");
                logger.Info("Movement completed in {0} with Average Speed of {1} MB/sec - Average file size: {2}", CurrentTask.ElapsedTime.Elapsed, Math.Round(((TotalFileSize / 1024f) / 1024f) / CurrentTask.ElapsedTime.Elapsed.TotalSeconds, 3), Functions.FileSystem.FormatBytes(TotalFileSize / (long)CurrentTask.TotalFileCount));
            }
            catch (OperationCanceledException)
            {
                if (!CurrentTask.ErrorHappened)
                {
                    CurrentTask.ErrorHappened = true;
                    Framework.TaskManager.Stop();
                    CurrentTask.Active = false;
                    CurrentTask.Completed = true;

                    await Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                    {
                        if (await Main.FormAccessor.ShowMessageAsync("Remove moved files?", $"[{AppName}] Game movement cancelled. Would you like to remove files that already moved from target library?", MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Affirmative)
                        {
                            Functions.FileSystem.RemoveGivenFiles(CopiedFiles, CreatedDirectories, CurrentTask);
                        }
                    }, System.Windows.Threading.DispatcherPriority.Normal);

                    LogToTM($"[{AppName}] Operation cancelled by user. Time Elapsed: {CurrentTask.ElapsedTime.Elapsed}");
                    logger.Info("Operation cancelled by used. Elapsed time: {0}", CurrentTask.ElapsedTime.Elapsed);
                }
            }
            catch (Exception ex)
            {
                CurrentTask.ErrorHappened = true;
                Framework.TaskManager.Stop();
                CurrentTask.Active = false;
                CurrentTask.Completed = true;

                await Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                 {
                     if (await Main.FormAccessor.ShowMessageAsync("Remove moved files?", $"[{AppName}] An error happened while moving game files. Would you like to remove files that already moved from target library?", MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Affirmative)
                     {
                         Functions.FileSystem.RemoveGivenFiles(CopiedFiles, CreatedDirectories, CurrentTask);
                     }
                 }, System.Windows.Threading.DispatcherPriority.Normal);

                Main.FormAccessor.TaskManager_Logs.Add($"[{AppName}] An error happened while moving game files. Time Elapsed: {CurrentTask.ElapsedTime.Elapsed}");
                logger.Fatal(ex);
                await SLM.RavenClient.CaptureAsync(new SharpRaven.Data.SentryEvent(ex));
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
                logger.Error(ex);
            }
        }

        public async Task<bool> DeleteFilesAsync(List.TaskInfo CurrentTask = null)
        {
            try
            {
                if (IsCompressed)
                {
                    CompressedArchiveName.Refresh();

                    if (CompressedArchiveName.Exists)
                        await Task.Run(() => CompressedArchiveName.Delete());
                }
                else
                {
                    List<FileInfo> FileList = GetFileList();
                    if (FileList.Count > 0)
                    {
                        Parallel.ForEach(FileList, currentFile =>
                        {
                            try
                            {
                                CurrentTask?.mre.WaitOne();

                                currentFile.Refresh();

                                if (currentFile.Exists)
                                {
                                    if (CurrentTask != null)
                                    {
                                        CurrentTask.mre.WaitOne();

                                        CurrentTask.TaskStatusInfo = $"Deleting: {currentFile.Name} ({Functions.FileSystem.FormatBytes(currentFile.Length)})";
                                        Main.FormAccessor.TaskManager_Logs.Add($"[{DateTime.Now}] [{CurrentTask.SteamApp.AppName}] Deleting file: {currentFile.FullName}");
                                    }

                                    File.SetAttributes(currentFile.FullName, FileAttributes.Normal);
                                    currentFile.Delete();
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Fatal(ex);
                            }
                        }
                        );
                    }

                    CommonFolder.Refresh();
                    // common folder, if exists
                    if (CommonFolder.Exists)
                    {
                        await Task.Run(() => CommonFolder.Delete(true));
                    }

                    DownloadFolder.Refresh();
                    // downloading folder, if exists
                    if (DownloadFolder.Exists)
                    {
                        await Task.Run(() => DownloadFolder.Delete(true));
                    }

                    WorkShopPath.Refresh();
                    // workshop folder, if exists
                    if (WorkShopPath.Exists)
                    {
                        await Task.Run(() => WorkShopPath.Delete(true));
                    }

                    FullAcfPath.Refresh();
                    // game .acf file
                    if (FullAcfPath.Exists)
                    {
                        File.SetAttributes(FullAcfPath.FullName, FileAttributes.Normal);
                        FullAcfPath.Delete();
                    }

                    WorkShopAcfPath.Refresh();
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
            catch (FileNotFoundException ex)
            {
                logger.Error(ex);
                return true;
            }
            catch (DirectoryNotFoundException ex)
            {
                logger.Error(ex);
                return true;
            }
            catch (IOException ex)
            {
                logger.Error(ex);
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Error(ex);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                logger.Fatal(ex);
                await SLM.RavenClient.CaptureAsync(new SharpRaven.Data.SentryEvent(ex));

                return false;
            }
        }
    }
}