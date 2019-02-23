using FileCopyLib;
using MahApps.Metro.Controls.Dialogs;
using System;
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
        public string AppName { get; set; }
        public int AppID { get; set; }
        public string[] Locales { get; set; }
        public string InstalledLocale { get; set; }
        public DirectoryInfo InstallationDirectory;
        public FileInfo TouchupFile { get; set; }
        public string InstallationParameter { get; set; }
        public string UpdateParameter { get; set; }
        public string RepairParameter { get; set; }
        public Version AppVersion { get; set; }
        public long SizeOnDisk => Functions.FileSystem.GetDirectorySize(InstallationDirectory, true);
        public string PrettyGameSize => Functions.FileSystem.FormatBytes(SizeOnDisk);
        public DateTime LastUpdated => InstallationDirectory.LastWriteTimeUtc;
        public string GameHeaderImage { get; set; }

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

        public List<FrameworkElement> ContextMenuItems
        {
            get
            {
                var rightClickMenu = new List<FrameworkElement>();
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
                                Header = Framework.StringFormat.Format(cItem.Header, new { AppName, AppID, SizeOnDisk = Functions.FileSystem.FormatBytes(SizeOnDisk) })
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
                    MessageBox.Show(string.Format(Functions.SLM.Translate(nameof(Properties.Resources.OriginAppInfo_FormatException)), ex));

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
            LogToTM(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.PopulatingFileList)), new { AppName }));
            logger.Info(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.PopulatingFileList)), new { AppName }));

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

                LogToTM(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FileListPopulated)), new { AppName, FileCount = AppFiles.Count, TotalFileSize = Functions.FileSystem.FormatBytes(TotalFileSize) }));
                logger.Info(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FileListPopulated)), new { AppName, FileCount = AppFiles.Count, TotalFileSize = Functions.FileSystem.FormatBytes(TotalFileSize) }));

                if (CurrentTask.TargetLibrary.Type == Enums.LibraryType.SLM && CurrentTask.TargetLibrary.Origin == null)
                {
                    Directory.CreateDirectory(Path.Combine(CurrentTask.TargetLibrary.DirectoryInfo.FullName, "Origin"));

                    CurrentTask.TargetLibrary.Origin = new OriginLibrary(Path.Combine(CurrentTask.TargetLibrary.DirectoryInfo.FullName, "Origin"), library: CurrentTask.TargetLibrary);
                }

                // Create directories
                Parallel.ForEach(AppFiles, POptions, CurrentFile =>
                {
                    FileInfo NewFile = new FileInfo(CurrentFile.FullName.Replace(Library.Origin.FullPath, CurrentTask.TargetLibrary.Origin.FullPath));

                    if (!NewFile.Directory.Exists)
                    {
                        NewFile.Directory.Create();
                        CreatedDirectories.Add(NewFile.Directory.FullName);
                    }
                });

                void CopyProgressCallback(FileProgress s) => OnFileProgress(s);
                POptions.MaxDegreeOfParallelism = 1;

                Parallel.ForEach(AppFiles.Where(x => (x).Length > Properties.Settings.Default.ParallelAfterSize * 1000000).OrderBy(x => x.DirectoryName).ThenByDescending(x => x.Length), POptions, CurrentFile =>
                {
                    try
                    {
                        FileInfo NewFile = new FileInfo(CurrentFile.FullName.Replace(Library.Origin.FullPath, CurrentTask.TargetLibrary.Origin.FullPath));

                        if (!NewFile.Exists || (NewFile.Length != CurrentFile.Length || NewFile.LastWriteTime != CurrentFile.LastWriteTime))
                        {
                            FileCopier.CopyWithProgress(CurrentFile.FullName, NewFile.FullName, CopyProgressCallback);
                            CurrentTask.MovedFileSize += CurrentFile.Length;
                            NewFile.LastWriteTime = CurrentFile.LastWriteTime;
                            NewFile.LastAccessTime = CurrentFile.LastAccessTime;
                            NewFile.CreationTime = CurrentFile.CreationTime;
                        }
                        else
                        {
                            CurrentTask.MovedFileSize += NewFile.Length;
                        }

                        CopiedFiles.Add(NewFile.FullName);

                        if (CurrentTask.ReportFileMovement)
                        {
                            LogToTM(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FileMoved)), new { AppName, NewFileName = NewFile.FullName }));
                        }

                        logger.Info(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FileMoved)), new { AppName, NewFileName = NewFile.FullName }));
                    }
                    catch (System.ComponentModel.Win32Exception)
                    {
                        throw new OperationCanceledException(cancellationToken);
                    }
                    catch (PathTooLongException ex)
                    {
                        CurrentTask.ErrorHappened = true;
                        Framework.TaskManager.Stop();
                        CurrentTask.Active = false;
                        CurrentTask.Completed = true;

                        Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                        {
                            if (await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.RemoveMovedFiles)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.PathTooLongException)), new { AppName, ExceptionMessage = ex.Message }), MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Affirmative)
                            {
                                Functions.FileSystem.RemoveGivenFiles(CopiedFiles, CreatedDirectories, CurrentTask);
                            }
                        }, System.Windows.Threading.DispatcherPriority.Normal);

                        Main.FormAccessor.TaskManager_Logs.Add(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FileSystemRelatedError)), new { AppName, ExceptionMessage = ex.Message }));
                        logger.Fatal(ex);
                    }
                    catch (IOException ex)
                    {
                        CurrentTask.ErrorHappened = true;
                        Framework.TaskManager.Stop();
                        CurrentTask.Active = false;
                        CurrentTask.Completed = true;

                        Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                        {
                            if (await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.RemoveMovedFiles)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FileSystemRelatedError_DeleteMovedFiles)), new { AppName, ExceptionMessage = ex.Message }), MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Affirmative)
                            {
                                Functions.FileSystem.RemoveGivenFiles(CopiedFiles, CreatedDirectories, CurrentTask);
                            }
                        }, System.Windows.Threading.DispatcherPriority.Normal);

                        Main.FormAccessor.TaskManager_Logs.Add(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FileSystemRelatedError)), new { AppName, ExceptionMessage = ex.Message }));
                        logger.Fatal(ex);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                        {
                            if (await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.RemoveMovedFiles)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FilePermissionRelatedError_DeleteFiles)), new { AppName, ExceptionMessage = ex.Message }), MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Affirmative)
                            {
                                Functions.FileSystem.RemoveGivenFiles(CopiedFiles, CreatedDirectories, CurrentTask);
                            }
                        }, System.Windows.Threading.DispatcherPriority.Normal);
                    }
                });

                POptions.MaxDegreeOfParallelism = Environment.ProcessorCount;

                Parallel.ForEach(AppFiles.Where(x => (x).Length <= Properties.Settings.Default.ParallelAfterSize * 1000000).OrderBy(x => x.DirectoryName).ThenByDescending(x => x.Length), POptions, CurrentFile =>
                {
                    try
                    {
                        if (cancellationToken.IsCancellationRequested)
                            throw (new OperationCanceledException(cancellationToken));

                        CurrentTask.mre.WaitOne();

                        FileInfo NewFile = new FileInfo(CurrentFile.FullName.Replace(Library.Origin.FullPath, CurrentTask.TargetLibrary.Origin.FullPath));

                        if (!NewFile.Exists || (NewFile.Length != CurrentFile.Length || NewFile.LastWriteTime != CurrentFile.LastWriteTime))
                        {
                            FileCopier.CopyWithProgress(CurrentFile.FullName, NewFile.FullName, CopyProgressCallback);
                            CurrentTask.MovedFileSize += CurrentFile.Length;

                            NewFile.LastWriteTime = CurrentFile.LastWriteTime;
                            NewFile.LastAccessTime = CurrentFile.LastAccessTime;
                            NewFile.CreationTime = CurrentFile.CreationTime;
                        }
                        else
                        {
                            CurrentTask.MovedFileSize += NewFile.Length;
                        }

                        CopiedFiles.Add(NewFile.FullName);

                        if (CurrentTask.ReportFileMovement)
                        {
                            LogToTM(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FileMoved)), new { AppName, NewFileName = NewFile.FullName }));
                        }

                        logger.Info(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FileMoved)), new { AppName, NewFileName = NewFile.FullName }));
                    }
                    catch (System.ComponentModel.Win32Exception)
                    {
                        throw new OperationCanceledException(cancellationToken);
                    }
                    catch (PathTooLongException ex)
                    {
                        CurrentTask.ErrorHappened = true;
                        Framework.TaskManager.Stop();
                        CurrentTask.Active = false;
                        CurrentTask.Completed = true;

                        Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                        {
                            if (await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.RemoveMovedFiles)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.PathTooLongException)), new { AppName, ExceptionMessage = ex.Message }), MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Affirmative)
                            {
                                Functions.FileSystem.RemoveGivenFiles(CopiedFiles, CreatedDirectories, CurrentTask);
                            }
                        }, System.Windows.Threading.DispatcherPriority.Normal);

                        Main.FormAccessor.TaskManager_Logs.Add(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FileSystemRelatedError)), new { AppName, ExceptionMessage = ex.Message }));
                        logger.Fatal(ex);
                    }
                    catch (IOException ex)
                    {
                        CurrentTask.ErrorHappened = true;
                        Framework.TaskManager.Stop();
                        CurrentTask.Active = false;
                        CurrentTask.Completed = true;

                        Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                        {
                            if (await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.RemoveMovedFiles)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FileSystemRelatedError_DeleteMovedFiles)), new { AppName, ExceptionMessage = ex.Message }), MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Affirmative)
                            {
                                Functions.FileSystem.RemoveGivenFiles(CopiedFiles, CreatedDirectories, CurrentTask);
                            }
                        }, System.Windows.Threading.DispatcherPriority.Normal);

                        Main.FormAccessor.TaskManager_Logs.Add(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FileSystemRelatedError)), new { AppName, ExceptionMessage = ex.Message }));
                        logger.Fatal(ex);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                        {
                            if (await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.RemoveMovedFiles)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FilePermissionRelatedError_DeleteFiles)), new { AppName, ExceptionMessage = ex.Message }), MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Affirmative)
                            {
                                Functions.FileSystem.RemoveGivenFiles(CopiedFiles, CreatedDirectories, CurrentTask);
                            }
                        }, System.Windows.Threading.DispatcherPriority.Normal);
                    }
                });

                CurrentTask.ElapsedTime.Stop();
                CurrentTask.MovedFileSize = TotalFileSize;

                LogToTM(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskCompleted)), new { AppName, ElapsedTime = CurrentTask.ElapsedTime.Elapsed, AverageSpeed = GetElapsedTimeAverage(TotalFileSize, CurrentTask.ElapsedTime.Elapsed.TotalSeconds), AverageFileSize = Functions.FileSystem.FormatBytes(TotalFileSize / (long)CurrentTask.TotalFileCount) }));
                logger.Info(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskCompleted)), new { AppName, ElapsedTime = CurrentTask.ElapsedTime.Elapsed, AverageSpeed = GetElapsedTimeAverage(TotalFileSize, CurrentTask.ElapsedTime.Elapsed.TotalSeconds), AverageFileSize = Functions.FileSystem.FormatBytes(TotalFileSize / (long)CurrentTask.TotalFileCount) }));
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
                        if (await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.RemoveMovedFiles)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskCancelled_RemoveFiles)), new { AppName }), MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Affirmative)
                        {
                            Functions.FileSystem.RemoveGivenFiles(CopiedFiles, CreatedDirectories, CurrentTask);
                        }
                    }, System.Windows.Threading.DispatcherPriority.Normal);

                    LogToTM(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskCancelled_ElapsedTime)), new { AppName, ElapsedTime = CurrentTask.ElapsedTime.Elapsed }));
                    logger.Info(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskCancelled_ElapsedTime)), new { AppName, ElapsedTime = CurrentTask.ElapsedTime.Elapsed }));
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
                    if (await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.RemoveMovedFiles)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.AnyException_RemoveFiles)), new { AppName, ExceptionMessage = ex.Message }), MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Affirmative)
                    {
                        Functions.FileSystem.RemoveGivenFiles(CopiedFiles, CreatedDirectories, CurrentTask);
                    }
                }, System.Windows.Threading.DispatcherPriority.Normal);

                Main.FormAccessor.TaskManager_Logs.Add(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.AnyError_ElapsedTime)), new { AppName, ElapsedTime = CurrentTask.ElapsedTime.Elapsed }));
                logger.Fatal(ex);
            }
        }

        private double GetElapsedTimeAverage(long FileSize, double ElapsedTime)
        {
            try
            {
                return Math.Round(FileSize / 1024f / 1024f / ElapsedTime, 3);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private void OnFileProgress(FileProgress s)
        {
            Framework.TaskManager.ActiveTask.mre.WaitOne();

            if (Framework.TaskManager.CancellationToken.IsCancellationRequested)
                throw (new OperationCanceledException(Framework.TaskManager.CancellationToken.Token));

            Framework.TaskManager.ActiveTask.TaskStatusInfo = Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskStatus_CopyingFile)), new { Percentage = s.Percentage.ToString("0.00"), TransferredBytes = s.Transferred, TotalBytes = s.Total });
        }

        public void DeleteFiles(List.TaskInfo CurrentTask = null)
        {
            try
            {
                Parallel.ForEach(GetFileList(), currentFile =>
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

                                CurrentTask.TaskStatusInfo = Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskStatus_DeletingFile)), new { FileName = currentFile.Name, FormattedFileSize = Functions.FileSystem.FormatBytes(currentFile.Length) });
                                Main.FormAccessor.TaskManager_Logs.Add($"[{DateTime.Now}] [{AppName}] {Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskStatus_DeletingFile)), new { FileName = currentFile.Name, FormattedFileSize = Functions.FileSystem.FormatBytes(currentFile.Length) })}");
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
                        var ProgressInformationMessage = await Main.FormAccessor.ShowProgressAsync(Functions.SLM.Translate(nameof(Properties.Resources.PleaseWait)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.OriginInstallation_Start)), new { AppName }));
                        ProgressInformationMessage.SetIndeterminate();

                        var process = Process.Start(TouchupFile.FullName, ((Repair) ? RepairParameter : InstallationParameter).Replace("{locale}", InstalledLocale).Replace("{installLocation}", InstallationDirectory.FullName));

                        Debug.WriteLine(InstallationParameter.Replace("{locale}", InstalledLocale).Replace("{installLocation}", InstallationDirectory.FullName));

                        ProgressInformationMessage.SetMessage(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.OriginInstallation_Ongoing)), new { AppName }));

                        while (!process.HasExited)
                        {
                            await Task.Delay(100);
                        }

                        await ProgressInformationMessage.CloseAsync();

                        var installLog = File.ReadAllLines(Path.Combine(InstallationDirectory.FullName, "__Installer", "InstallLog.txt")).Reverse();
                        if (installLog.Any(x => x.IndexOf("Installer finished with exit code:", StringComparison.OrdinalIgnoreCase) != -1))
                        {
                            var installerResult = installLog.FirstOrDefault(x => x.IndexOf("Installer finished with exit code:", StringComparison.OrdinalIgnoreCase) != -1);

                            await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.OriginInstallation)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.OriginInstallation_Completed)), new { installerResult }));
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
    }
}