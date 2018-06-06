using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Steam_Library_Manager.Definitions
{
    public class OriginAppInfo
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public Library Library { get; set; }
        public string AppName { get; set; } // gameTitle
        public int AppID { get; set; } // contentID
        public string[] Locales { get; set; } // pt_BR,en_US,de_DE,es_ES,fr_FR,it_IT,es_MX,nl_NL,pl_PL,ru_RU,ar_SA,cs_CZ,da_DK,no_NO,pt_PT,zh_TW,sv_SE,tr_TR
        public string InstalledLocale { get; set; }
        public DirectoryInfo InstallationDirectory; // D:\Oyunlar\Origin Games\FIFA 17\
        public FileInfo TouchupFile { get; set; }
        public string InstallationParameter { get; set; }
        public string UpdateParameter { get; set; }
        public string RepairParameter { get; set; }
        public Version AppVersion { get; set; }
        public long SizeOnDisk => Functions.FileSystem.GetDirectorySize(InstallationDirectory, true);
        public string PrettyGameSize => Functions.FileSystem.FormatBytes(SizeOnDisk);
        public DateTime LastUpdated => InstallationDirectory.LastWriteTimeUtc;

        public OriginAppInfo(Library _Library, string _AppName, int _AppID, DirectoryInfo _InstallationDirectory, Version _AppVersion, string[] _Locales, string _InstalledLocale, string _TouchupFile, string _InstallationParameter, string _UpdateParameter = null, string _RepairParameter = null)
        {
            Library = _Library;
            AppName = _AppName;
            AppID = _AppID;
            Locales = _Locales;
            InstalledLocale = _InstalledLocale;
            InstallationDirectory = _InstallationDirectory;
            TouchupFile = new FileInfo(_InstallationDirectory.FullName + _TouchupFile);
            InstallationParameter = _InstallationParameter;
            UpdateParameter = _UpdateParameter;
            RepairParameter = _RepairParameter;
            AppVersion = _AppVersion;
        }

        //-----
        public Framework.AsyncObservableCollection<FrameworkElement> ContextMenuItems
        {
            get
            {
                Framework.AsyncObservableCollection<FrameworkElement> rightClickMenu = new Framework.AsyncObservableCollection<FrameworkElement>();
                try
                {
                    foreach (ContextMenuItem cItem in List.AppCMenuItems.Where(x => x.IsActive && x.LibraryType == Enums.LibraryType.Origin))
                    {
                        if (!cItem.ShowToNormal)
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
                                Header = string.Format(cItem.Header, AppName, AppID)
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
                    case "disk":
                        InstallationDirectory.Refresh();

                        if (InstallationDirectory.Exists)
                        {
                            Process.Start(InstallationDirectory.FullName);
                        }

                        break;

                    case "install":

                        await InstallAsync();

                        break;

                    case "repair":

                        await InstallAsync(true);

                        break;

                    case "deleteappfiles":
                        await Task.Run(() => DeleteFiles());

                        Library.Origin.Apps.Remove(this);
                        if (SLM.CurrentSelectedLibrary == Library)
                            Functions.App.UpdateAppPanel(Library);

                        break;

                    case "deleteappfilestm":
                        Framework.TaskManager.AddTask(new List.TaskInfo
                        {
                            OriginApp = this,
                            TaskType = Enums.TaskType.Delete,
                        });
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public List<FileInfo> GetFileList()
        {
            try
            {
                InstallationDirectory?.Refresh();

                return InstallationDirectory?.GetFiles("*", SearchOption.AllDirectories)?.ToList();
            }
            catch { return null; }
        }

        public async void CopyFilesAsync(List.TaskInfo CurrentTask, CancellationToken cancellationToken)
        {
            LogToTM($"[{AppName}] Populating file list, please wait");
            logger.Info("Populating file list for: {0}", AppName);

            ConcurrentBag<string> CopiedFiles = new ConcurrentBag<string>();
            ConcurrentBag<string> CreatedDirectories = new ConcurrentBag<string>();
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

                POptions.MaxDegreeOfParallelism = 1;

                if (CurrentTask.TargetLibrary.Type == Enums.LibraryType.SLM && CurrentTask.TargetLibrary.Origin == null)
                {
                    Directory.CreateDirectory(Path.Combine(CurrentTask.TargetLibrary.DirectoryInfo.FullName, "Origin"));

                    CurrentTask.TargetLibrary.Origin = new OriginLibrary(Path.Combine(CurrentTask.TargetLibrary.DirectoryInfo.FullName, "Origin"));
                }

                Parallel.ForEach(AppFiles.Where(x => (x).Length > Properties.Settings.Default.ParallelAfterSize * 1000000).OrderByDescending(x => (x).Length), POptions, CurrentFile =>
                {
                    try
                    {
                        CurrentTask.mre.WaitOne();

                        FileInfo NewFile = new FileInfo(CurrentFile.FullName.Replace(Library.Origin.FullPath, CurrentTask.TargetLibrary.Origin.FullPath));

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

                POptions.MaxDegreeOfParallelism = Environment.ProcessorCount;

                Parallel.ForEach(AppFiles.Where(x => (x).Length <= Properties.Settings.Default.ParallelAfterSize * 1000000).OrderByDescending(x => (x).Length), POptions, CurrentFile =>
                {
                    try
                    {
                        CurrentTask.mre.WaitOne();

                        FileInfo NewFile = new FileInfo(CurrentFile.FullName.Replace(Library.Origin.FullPath, CurrentTask.TargetLibrary.Origin.FullPath));

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

        public void DeleteFiles(List.TaskInfo CurrentTask = null)
        {
            try
            {
                var Files = GetFileList();

                Parallel.ForEach(Files, currentFile =>
                {
                    try
                    {
                        CurrentTask.mre.WaitOne();

                        currentFile.Refresh();
                        if (currentFile.Exists)
                        {
                            if (CurrentTask != null)
                            {
                                CurrentTask.mre.WaitOne();

                                CurrentTask.TaskStatusInfo = $"Deleting: {currentFile.Name} ({Functions.FileSystem.FormatBytes(currentFile.Length)})";
                                Main.FormAccessor.TaskManager_Logs.Add($"[{DateTime.Now}] [{CurrentTask.OriginApp.AppName}] Deleting file: {currentFile.FullName}");
                            }

                            File.SetAttributes(currentFile.FullName, FileAttributes.Normal);
                            currentFile.Delete();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Fatal(ex);
                    }
                });

                InstallationDirectory.Refresh();
                if (InstallationDirectory.Exists)
                {
                    InstallationDirectory.Delete(true);
                }
            }
            catch { }
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

        public async Task InstallAsync(bool Repair = false)
        {
            try
            {
                TouchupFile.Refresh();

                if (TouchupFile.Exists && !string.IsNullOrEmpty(InstallationParameter))
                {
                    if (Repair && string.IsNullOrEmpty(RepairParameter))
                    {
                        return;
                    }

                    await Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                    {   
                        var ProgressInformationMessage = await Main.FormAccessor.ShowProgressAsync("Please wait...", $"Asking Origin to install {AppName} as you have requested.");
                        ProgressInformationMessage.SetIndeterminate();

                        var process = Process.Start(TouchupFile.FullName, ((Repair) ? RepairParameter : InstallationParameter).Replace("{locale}", InstalledLocale).Replace("{installLocation}", InstallationDirectory.FullName));

                        Debug.WriteLine(InstallationParameter.Replace("{locale}", InstalledLocale).Replace("{installLocation}", InstallationDirectory.FullName));

                        ProgressInformationMessage.SetMessage($"Asking Origin to install {AppName} as you have requested.\n\n\nWaiting for Origin to complete the installation...");

                        while (!process.HasExited)
                        {
                            await Task.Delay(100);
                        }

                        await ProgressInformationMessage.CloseAsync();

                        var installLog = File.ReadAllLines(Path.Combine(InstallationDirectory.FullName, "__Installer", "InstallLog.txt")).Reverse();
                        if (installLog.Any(x => x.IndexOf("Installer finished with exit code:", StringComparison.OrdinalIgnoreCase) != -1))
                        {
                            var test = installLog.FirstOrDefault(x => x.IndexOf("Installer finished with exit code:", StringComparison.OrdinalIgnoreCase) != -1);

                            await Main.FormAccessor.ShowMessageAsync("Origin App Installation", "Origin app installation completed.\n\n\nResult message:\n\n" + test);
                        }
                    });

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                await SLM.RavenClient.CaptureAsync(new SharpRaven.Data.SentryEvent(ex));
            }
        }
    }
}