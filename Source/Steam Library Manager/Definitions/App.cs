using Alphaleonis.Win32.Filesystem;
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
using DirectoryInfo = Alphaleonis.Win32.Filesystem.DirectoryInfo;
using File = Alphaleonis.Win32.Filesystem.File;
using FileInfo = Alphaleonis.Win32.Filesystem.FileInfo;
using Path = Alphaleonis.Win32.Filesystem.Path;

namespace Steam_Library_Manager.Definitions
{
    public class App
    {
        protected readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public Library Library { get; set; }
        public string AppName { get; set; }
        public int AppId { get; set; }
        public DirectoryInfo InstallationDirectory { get; set; }
        protected readonly List<(DirectoryInfo directoryInfo, string searchPattern, SearchOption searchOption)> AdditionalDirectories = new List<(DirectoryInfo directoryInfo, string searchPattern, SearchOption searchOption)>();
        protected readonly List<FileInfo> AdditionalFiles = new List<FileInfo>();
        public long SizeOnDisk { get; set; }
        public string PrettyGameSize => Functions.FileSystem.FormatBytes(SizeOnDisk);
        public string GameHeaderImage { get; set; }

        public DateTime LastUpdated { get; set; }
        public DateTime LastPlayed { get; set; }
        public bool IsCompacted { get; set; }
        public bool IsCompressed { get; set; }
        public FileInfo CompressedArchivePath { get; set; }

        public List<FrameworkElement> ContextMenuElements => _contextMenuElements ?? (_contextMenuElements = GenerateCMenuItems());

        private List<FrameworkElement> GenerateCMenuItems()
        {
            var rightClickMenu = new List<FrameworkElement>();
            try
            {
                foreach (var cItem in List.AppCMenuItems.Where(x => x.IsActive && x.AllowedLibraryTypes.Contains(Library.Type)))
                {
                    if (!cItem.ShowToNormal)
                    {
                        continue;
                    }

                    if (this is SteamAppInfo appInfo && appInfo.IsSteamBackup && !cItem.ShowToSteamBackup)
                    {
                        continue;
                    }

                    if (!cItem.ShowToCompressed && IsCompressed)
                    {
                        continue;
                    }

                    if (cItem.IsSeparator)
                    {
                        rightClickMenu.Add(new Separator());
                    }
                    else
                    {
                        var slmItem = new MenuItem()
                        {
                            Header = Framework.StringFormat.Format(cItem.Header, new { AppName, AppId, SizeOnDisk = Functions.FileSystem.FormatBytes(SizeOnDisk) }),
                            Tag = cItem.Action,
                            Icon = cItem.Icon,
                            HorizontalContentAlignment = HorizontalAlignment.Left,
                            VerticalContentAlignment = VerticalAlignment.Center
                        };

                        slmItem.Click += Main.FormAccessor.AppCMenuItem_Click;

                        rightClickMenu.Add(slmItem);
                    }
                }

                return rightClickMenu;
            }
            catch (FormatException ex)
            {
                MessageBox.Show(string.Format(Functions.SLM.Translate(nameof(Properties.Resources.AppInfoFormatException)), ex));

                return rightClickMenu;
            }
        }

        private List<FrameworkElement> _contextMenuElements;

        public virtual async void ParseMenuItemActionAsync(string action)
        {
            try
            {
                switch (action.ToLowerInvariant())
                {
                    default:
                        if (string.IsNullOrEmpty(Properties.Settings.Default.SteamID64))
                        {
                            return;
                        }

                        Process.Start(string.Format(action, AppId, Properties.Settings.Default.SteamID64));
                        break;

                    case "verify":
                        {
                            Process.Start($@"steam://validate/{AppId}");
                        }
                        break;

                    case "google":
                        {
                            Process.Start($@"https://www.google.com/search?q={Uri.EscapeUriString(AppName)}+pc");
                        }
                        break;

                    case "youtube":
                        {
                            Process.Start($@"https://www.youtube.com/results?search_query={Uri.EscapeUriString(AppName)}+pc");
                        }
                        break;

                    case "compress":
                        if (Functions.TaskManager.TaskList.Count(x => x.App == this && x.TargetLibrary == Library && x.TaskType == Enums.TaskType.Compress && !x.Completed) == 0)
                        {
                            Functions.TaskManager.AddTask(new List.TaskInfo
                            {
                                App = this,
                                TargetLibrary = Library,
                                Compress = !IsCompressed,
                                TaskType = Enums.TaskType.Compress
                            });
                        }
                        break;

                    case "compact":
                        if (Functions.TaskManager.TaskList.Count(x => x.App == this && x.TargetLibrary == Library && x.TaskType == Enums.TaskType.Compact && !x.Completed) == 0)
                        {
                            Functions.TaskManager.AddTask(new List.TaskInfo
                            {
                                App = this,
                                TargetLibrary = Library,
                                TaskType = Enums.TaskType.Compact
                            });
                        }
                        break;

                    case "disk":
                        InstallationDirectory.Refresh();

                        if (InstallationDirectory.Exists)
                        {
                            Process.Start(InstallationDirectory.FullName);
                        }

                        break;

                    case "deleteappfiles":
                        await Task.Run(async () => await DeleteFilesAsync()).ConfigureAwait(true);

                        Library.Apps.Remove(this);

                        Functions.SLM.Library.UpdateLibraryVisual();

                        if (SLM.CurrentSelectedLibrary == Library)
                            Functions.App.UpdateAppPanel(Library);
                        break;

                    case "deleteappfilestm":
                        Functions.TaskManager.AddTask(new List.TaskInfo
                        {
                            App = this,
                            TargetLibrary = Library,
                            TaskType = Enums.TaskType.Delete
                        });
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }

        public List<FileInfo> GetFileList()
        {
            try
            {
                var fileList = new List<FileInfo>();

                if (IsCompressed)
                {
                    fileList.Add(CompressedArchivePath);
                }
                else
                {
                    InstallationDirectory?.Refresh();

                    if (InstallationDirectory.Exists)
                    {
                        fileList.AddRange(InstallationDirectory?.GetFiles("*", SearchOption.AllDirectories));
                    }

                    foreach (var (directoryInfo, searchPattern, searchOption) in AdditionalDirectories)
                    {
                        directoryInfo.Refresh();

                        if (directoryInfo.Exists)
                        {
                            fileList.AddRange(directoryInfo.GetFiles(searchPattern, searchOption));
                        }
                    }

                    foreach (var additionalFile in AdditionalFiles.ToList())
                    {
                        additionalFile.Refresh();

                        if (additionalFile.Exists)
                        {
                            fileList.Add(additionalFile);
                        }
                    }
                }

                return fileList;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return null;
            }
        }

        public async Task<bool> CompactStatus()
        {
            if (!Properties.Settings.Default.CompactDetection)
            {
                return false;
            }

            try
            {
                if (!InstallationDirectory.Exists)
                    return false;

                var result = await CliWrap.Cli.Wrap("compact")
                    .SetArguments($"{((Properties.Settings.Default.AdvancedCompactSizeDetection) ? "/s" : "")} /q")
                    .SetWorkingDirectory(InstallationDirectory.FullName)
                    .ExecuteAsync().ConfigureAwait(false);

                if (Properties.Settings.Default.AdvancedCompactSizeDetection)
                {
                    var output = result.StandardOutput.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    var noutput = output[output.Length - 2].Split(new[] { " total bytes of data are stored in " },
                        StringSplitOptions.RemoveEmptyEntries);

                    var sizeBeforeCompact = noutput[0];
                    var sizeAfterCompact = Convert.ToInt64(noutput[1].Replace(" bytes.", "").Replace(".", "").Replace(",", ""));

                    if (sizeAfterCompact != 0)
                    {
                        SizeOnDisk = sizeAfterCompact;
                    }
                }

                // May not be the best approach, to be improved if needed to.
                return !result.StandardOutput.Contains("0 are compressed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Logger.Fatal(ex);

                return false;
            }
        }

        public async Task CompactTask(List.TaskInfo currentTask, CancellationToken cancellationToken)
        {
            try
            {
                var appFiles = GetFileList();
                currentTask.TotalFileCount = appFiles.Count;
                long totalFileSize = 0;

                Parallel.ForEach(appFiles, file => Interlocked.Add(ref totalFileSize, file.Length));
                currentTask.TotalFileSize = totalFileSize;

                ReportToTaskManager($"Current status of {AppName} is {(IsCompacted ? "compacted" : "not compacted")} and the task is set to {(currentTask.Compact ? "compact" : "un-compact")} the app.");

                currentTask.ElapsedTime.Start();

                foreach (var file in appFiles)
                {
                    currentTask.mre.WaitOne();

                    if (!file.Directory.Exists)
                    {
                        ReportToTaskManager(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.CompactDirectoryNotExists)), new { DirectoryFullName = file.Directory.FullName }));
                    }

                    await CliWrap.Cli.Wrap("compact")
                        .SetArguments($"{(currentTask.Compact ? "/c" : "/u")} /i /q {(currentTask.ForceCompact ? "/f" : "")} /EXE:{currentTask.CompactLevel} {file.Name}")
                        .SetWorkingDirectory(file.Directory.FullName)
                        .SetCancellationToken(cancellationToken)
                        .SetStandardOutputCallback(OnCompactFolderProgress)
                        .SetStandardErrorCallback(OnCompactFolderProgress)
                        .EnableStandardErrorValidation()
                        .ExecuteAsync().ConfigureAwait(false);

                    Functions.TaskManager.ActiveTask.TaskStatusInfo = Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.Compact_CompressingFile)), new { fileName = file.FullName.Replace(InstallationDirectory.FullName, "") });
                    currentTask.MovedFileSize += file.Length;
                }

                if (InstallationDirectory.Exists)
                {
                    var result = await CliWrap.Cli.Wrap("compact")
                        .SetArguments($"/s /q")
                        .SetWorkingDirectory(InstallationDirectory.FullName)
                        .SetCancellationToken(cancellationToken)
                        .EnableStandardErrorValidation()
                        .ExecuteAsync().ConfigureAwait(false);

                    var output = result.StandardOutput.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var resultText in output.Skip(output.Length - 3))
                    {
                        ReportToTaskManager(resultText);
                    }
                }

                currentTask.ElapsedTime.Stop();

                ReportToTaskManager(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.Compact_TaskCompletedIn)), new { AppName, TimeElapsed = currentTask.ElapsedTime.Elapsed }));

                IsCompacted = await CompactStatus().ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                if (!currentTask.ErrorHappened)
                {
                    currentTask.ErrorHappened = true;

                    if (!Properties.Settings.Default.TaskManager_ContinueOnError)
                        Functions.TaskManager.Stop();

                    currentTask.Active = false;
                    currentTask.Completed = true;

                    ReportToTaskManager(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskCancelled_ElapsedTime)), new { AppName, ElapsedTime = currentTask.ElapsedTime.Elapsed }));
                    Logger.Info(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskCancelled_ElapsedTime)), new { AppName, ElapsedTime = currentTask.ElapsedTime.Elapsed }));
                }
            }
            catch (Exception ex)
            {
                ReportToTaskManager(ex.ToString());
                Debug.WriteLine(ex);
            }
        }

        private void OnCompactFolderProgress(string progress)
        {
            if (progress?.Length == 0 || !Functions.TaskManager.ActiveTask.ReportFileMovement) return;

            Functions.TaskManager.ActiveTask.mre.WaitOne();
            ReportToTaskManager(progress);
        }

        public async Task CopyFilesAsync(List.TaskInfo currentTask, CancellationToken cancellationToken)
        {
            var listLock = new object();
            ReportToTaskManager(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.PopulatingFileList)), new { AppName }));
            Logger.Info(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.PopulatingFileList)), new { AppName }));

            var copiedFiles = new List<string>();
            var createdDirectories = new List<string>();
            var appFiles = GetFileList();
            currentTask.TotalFileCount = appFiles.Count;
            long totalFileSize = 0;

            try
            {
                var pOptions = new ParallelOptions()
                {
                    CancellationToken = cancellationToken
                };

                Parallel.ForEach(appFiles, pOptions, file => Interlocked.Add(ref totalFileSize, file.Length));

                currentTask.TotalFileSize = totalFileSize;
                currentTask.ElapsedTime.Start();

                ReportToTaskManager(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FileListPopulated)), new { AppName, FileCount = appFiles.Count, TotalFileSize = Functions.FileSystem.FormatBytes(totalFileSize) }), true);

                // If the game is not compressed and user would like to compress it
                if (!IsCompressed && (currentTask.Compress || currentTask.TaskType == Enums.TaskType.Compress))
                {
                    switch (Library.Type)
                    {
                        case Enums.LibraryType.Steam:
                        case Enums.LibraryType.SLM:
                            CompressedArchivePath = new FileInfo(Path.Combine(currentTask.TargetLibrary.DirectoryList["SteamApps"].FullName, AppId + ".zip"));
                            break;

                        case Enums.LibraryType.Origin:
                            CompressedArchivePath = new FileInfo(Path.Combine(currentTask.TargetLibrary.FullPath, AppId + ".zip"));
                            break;

                        case Enums.LibraryType.Uplay:
                            CompressedArchivePath = new FileInfo(Path.Combine(currentTask.TargetLibrary.FullPath, AppName + ".zip"));
                            break;
                    }

                    CompressedArchivePath.Refresh();

                    if (CompressedArchivePath.Exists)
                    {
                        while (CompressedArchivePath.IsFileLocked())
                        {
                            if (currentTask.ReportFileMovement)
                            {
                                Logger.Info(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.CompressedArchiveExistsAndInUse)), new { ArchiveFullPath = CompressedArchivePath.FullName }));
                            }

                            await Task.Delay(1500, cancellationToken).ConfigureAwait(true);
                        }

                        CompressedArchivePath.Delete();
                    }

                    using (var archive = ZipFile.Open(CompressedArchivePath.FullName, ZipArchiveMode.Create))
                    {
                        try
                        {
                            copiedFiles.Add(CompressedArchivePath.FullName);

                            foreach (var currentFile in appFiles)
                            {
                                currentTask.mre.WaitOne();

                                var fileNameInArchive = "";

                                switch (Library.Type)
                                {
                                    case Enums.LibraryType.Steam:
                                    case Enums.LibraryType.SLM:
                                        fileNameInArchive = currentFile.FullName.Substring(Library.DirectoryList["SteamApps"].FullName.Length + 1);
                                        break;

                                    case Enums.LibraryType.Origin:
                                    case Enums.LibraryType.Uplay:
                                        fileNameInArchive = currentFile.FullName.Substring(Library.FullPath.Length);
                                        break;
                                }

                                currentTask.TaskStatusInfo = Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskStatus_CompressingFile)), new { CurrentFileName = currentFile.Name, FileSize = Functions.FileSystem.FormatBytes(currentFile.Length) });

                                archive.CreateEntryFromFile(currentFile.FullName, fileNameInArchive, Properties.Settings.Default.CompressionLevel.ParseEnum<CompressionLevel>());
                                currentTask.MovedFileSize += currentFile.Length;

                                if (currentTask.ReportFileMovement)
                                {
                                    ReportToTaskManager(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FileCompressed)), new { AppName, FileNameInArchive = fileNameInArchive }), true);
                                }

                                if (cancellationToken.IsCancellationRequested)
                                {
                                    throw new OperationCanceledException(cancellationToken);
                                }
                            }
                        }
                        catch (FileNotFoundException ex)
                        {
                            currentTask.ErrorHappened = true;

                            if (!Properties.Settings.Default.TaskManager_ContinueOnError)
                                Functions.TaskManager.Stop();

                            currentTask.Active = false;
                            currentTask.Completed = true;

                            await Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                             {
                                 if (await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.RemoveMovedFiles)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.CompressArchive_FileNotFoundEx)), new { AppName, ExceptionMessage = ex.Message }), MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(true) == MessageDialogResult.Affirmative)
                                 {
                                     await Functions.FileSystem.RemoveGivenFilesAsync(copiedFiles, createdDirectories, currentTask);
                                 }
                             }, System.Windows.Threading.DispatcherPriority.Normal).ConfigureAwait(true);

                            Main.FormAccessor.TmLogs.Report(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.CompressArchive_FileNotFoundEx)), new { AppName, ExceptionMessage = ex.Message }));
                            Logger.Fatal(ex);
                        }
                    }
                }
                // If the game is compressed and user would like to decompress it
                else if (IsCompressed && (!currentTask.Compress || currentTask.TaskType == Enums.TaskType.Compress))
                {
                    using (var zipFileReader = ZipFile.OpenRead(CompressedArchivePath.FullName))
                    {
                        foreach (var currentFile in zipFileReader.Entries)
                        {
                            currentTask.mre.WaitOne();

                            FileInfo newFile = null;

                            switch (Library.Type)
                            {
                                case Enums.LibraryType.Steam:
                                case Enums.LibraryType.SLM:
                                    newFile = new FileInfo(Path.Combine(currentTask.TaskType == Enums.TaskType.Compress ? Library.DirectoryList["SteamApps"].FullName : currentTask.TargetLibrary.DirectoryList["SteamApps"].FullName, currentFile.FullName));
                                    break;

                                case Enums.LibraryType.Origin:
                                case Enums.LibraryType.Uplay:
                                    newFile = new FileInfo(Path.Combine(currentTask.TaskType == Enums.TaskType.Compress ? Library.FullPath : currentTask.TargetLibrary.FullPath, currentFile.FullName));
                                    break;
                            }

                            if (!newFile.Directory.Exists)
                            {
                                newFile.Directory.Create();

                                lock (listLock)
                                {
                                    createdDirectories.Add(newFile.Directory.FullName);
                                }
                            }

                            currentTask.TaskStatusInfo = Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskStatus_Decompress)), new { NewFileName = newFile.FullName, NewFileSize = Functions.FileSystem.FormatBytes(currentFile.Length) });

                            currentFile.ExtractToFile(newFile.FullName, true);

                            copiedFiles.Add(newFile.FullName);
                            currentTask.MovedFileSize += currentFile.Length;

                            if (currentTask.ReportFileMovement)
                            {
                                ReportToTaskManager(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FileDecompressed)), new { AppName, NewFileFullPath = newFile.FullName }), true);
                            }

                            if (cancellationToken.IsCancellationRequested)
                            {
                                throw new OperationCanceledException(cancellationToken);
                            }
                        }
                    }
                }
                // Everything else
                else
                {
                    // Create directories
                    Parallel.ForEach(appFiles, pOptions, currentFile =>
                    {
                        var newFile = new FileInfo(currentFile.FullName.Replace(Library.DirectoryInfo.FullName, currentTask.TargetLibrary.DirectoryInfo.FullName));

                        if (!newFile.Directory.Exists)
                        {
                            newFile.Directory.Create();

                            lock (listLock)
                            {
                                createdDirectories.Add(newFile.Directory.FullName);
                            }
                        }
                    });

                    var copyProgressCallback = new CopyMoveProgressRoutine(OnFileProgress);
                    pOptions.MaxDegreeOfParallelism = 1;

                    Parallel.ForEach(appFiles.Where(x => x.Length > Properties.Settings.Default.ParallelAfterSize * 1000000).OrderBy(x => x.DirectoryName).ThenByDescending(x => x.Length), pOptions, currentFile =>
                    {
                        try
                        {
                            var newFile = new FileInfo(currentFile.FullName.Replace(Library.DirectoryInfo.FullName, currentTask.TargetLibrary.DirectoryInfo.FullName));

                            if (!newFile.Exists || (newFile.Length != currentFile.Length || newFile.LastWriteTime != currentFile.LastWriteTime))
                            {
                                File.Copy(currentFile.FullName, newFile.FullName, CopyOptions.None, true, copyProgressCallback, currentTask);
                                currentTask.MovedFileSize += currentFile.Length;
                            }
                            else
                            {
                                currentTask.MovedFileSize += newFile.Length;
                            }

                            lock (listLock)
                            {
                                copiedFiles.Add(newFile.FullName);
                            }

                            if (currentTask.ReportFileMovement)
                            {
                                ReportToTaskManager(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FileMoved)), new { AppName, NewFileName = newFile.FullName }), true);
                            }
                        }
                        catch (IOException ex)
                        {
                            currentTask.ErrorHappened = true;

                            if (!Properties.Settings.Default.TaskManager_ContinueOnError)
                                Functions.TaskManager.Stop();

                            currentTask.Active = false;
                            currentTask.Completed = true;

                            Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                            {
                                if (await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.RemoveMovedFiles)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FileSystemRelatedError_DeleteMovedFiles)), new { AppName, ExceptionMessage = ex.Message }), MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(true) == MessageDialogResult.Affirmative)
                                {
                                    await Functions.FileSystem.RemoveGivenFilesAsync(copiedFiles, createdDirectories, currentTask);
                                }
                            }, System.Windows.Threading.DispatcherPriority.Normal);

                            Main.FormAccessor.TmLogs.Report(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FileSystemRelatedError)), new { AppName, ExceptionMessage = ex.Message }));
                            Logger.Fatal(ex);
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                              {
                                  if (await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.RemoveMovedFiles)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FilePermissionRelatedError_DeleteFiles)), new { AppName, ExceptionMessage = ex.Message }), MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(true) == MessageDialogResult.Affirmative)
                                  {
                                      await Functions.FileSystem.RemoveGivenFilesAsync(copiedFiles, createdDirectories, currentTask);
                                  }
                              }, System.Windows.Threading.DispatcherPriority.Normal);
                        }
                    });

                    pOptions.MaxDegreeOfParallelism = Environment.ProcessorCount;

                    Parallel.ForEach(appFiles.Where(x => (x).Length <= Properties.Settings.Default.ParallelAfterSize * 1000000).OrderBy(x => x.DirectoryName).ThenByDescending(x => x.Length), pOptions, currentFile =>
                    {
                        try
                        {
                            var newFile = new FileInfo(currentFile.FullName.Replace(Library.DirectoryInfo.FullName, currentTask.TargetLibrary.DirectoryInfo.FullName));

                            if (!newFile.Exists || (newFile.Length != currentFile.Length || newFile.LastWriteTime != currentFile.LastWriteTime))
                            {
                                File.Copy(currentFile.FullName, newFile.FullName, CopyOptions.None, true, copyProgressCallback, currentTask);
                                currentTask.MovedFileSize += currentFile.Length;
                            }
                            else
                            {
                                currentTask.MovedFileSize += newFile.Length;
                            }

                            lock (listLock)
                            {
                                copiedFiles.Add(newFile.FullName);
                            }

                            if (currentTask.ReportFileMovement)
                            {
                                ReportToTaskManager(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FileMoved)), new { AppName, NewFileName = newFile.FullName }), true);
                            }
                        }
                        catch (IOException ex)
                        {
                            currentTask.ErrorHappened = true;

                            if (!Properties.Settings.Default.TaskManager_ContinueOnError)
                                Functions.TaskManager.Stop();

                            currentTask.Active = false;
                            currentTask.Completed = true;

                            Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                            {
                                if (await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.RemoveMovedFiles)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FileSystemRelatedError_DeleteMovedFiles)), new { AppName, ExceptionMessage = ex.Message }), MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(true) == MessageDialogResult.Affirmative)
                                {
                                    await Functions.FileSystem.RemoveGivenFilesAsync(copiedFiles, createdDirectories, currentTask);
                                }
                            }, System.Windows.Threading.DispatcherPriority.Normal);

                            Main.FormAccessor.TmLogs.Report(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FileSystemRelatedError)), new { AppName, ExceptionMessage = ex.Message }));
                            Logger.Fatal(ex);
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                            {
                                if (await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.RemoveMovedFiles)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FilePermissionRelatedError_DeleteFiles)), new { AppName, ExceptionMessage = ex.Message }), MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(true) == MessageDialogResult.Affirmative)
                                {
                                    await Functions.FileSystem.RemoveGivenFilesAsync(copiedFiles, createdDirectories, currentTask);
                                }
                            }, System.Windows.Threading.DispatcherPriority.Normal);
                        }
                    });
                }

                currentTask.ElapsedTime.Stop();
                currentTask.MovedFileSize = totalFileSize;

                ReportToTaskManager(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskCompleted)), new { AppName, ElapsedTime = currentTask.ElapsedTime.Elapsed, AverageSpeed = GetElapsedTimeAverage(totalFileSize, currentTask.ElapsedTime.Elapsed.TotalSeconds), AverageFileSize = Functions.FileSystem.FormatBytes(totalFileSize / currentTask.TotalFileCount) }), true);
            }
            catch (OperationCanceledException)
            {
                if (!currentTask.ErrorHappened)
                {
                    currentTask.ErrorHappened = true;

                    if (!Properties.Settings.Default.TaskManager_ContinueOnError)
                        Functions.TaskManager.Stop();

                    currentTask.Active = false;
                    currentTask.Completed = true;

                    await Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                    {
                        if (await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.RemoveMovedFiles)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskCancelled_RemoveFiles)), new { AppName }), MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(true) == MessageDialogResult.Affirmative)
                        {
                            await Functions.FileSystem.RemoveGivenFilesAsync(copiedFiles, createdDirectories, currentTask);
                        }
                    }, System.Windows.Threading.DispatcherPriority.Normal).ConfigureAwait(true);

                    ReportToTaskManager(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskCancelled_ElapsedTime)), new { AppName, ElapsedTime = currentTask.ElapsedTime.Elapsed }), true);
                }
            }
            catch (Exception ex)
            {
                currentTask.ErrorHappened = true;

                if (!Properties.Settings.Default.TaskManager_ContinueOnError)
                    Functions.TaskManager.Stop();

                currentTask.Active = false;
                currentTask.Completed = true;

                await Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                 {
                     if (await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.RemoveMovedFiles)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.AnyException_RemoveFiles)), new { AppName, ExceptionMessage = ex.Message }), MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(true) == MessageDialogResult.Affirmative)
                     {
                         await Functions.FileSystem.RemoveGivenFilesAsync(copiedFiles, createdDirectories, currentTask);
                     }
                 }, System.Windows.Threading.DispatcherPriority.Normal).ConfigureAwait(true);

                Main.FormAccessor.TmLogs.Report(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.AnyError_ElapsedTime)), new { AppName, ElapsedTime = currentTask.ElapsedTime.Elapsed }));
                Logger.Fatal(ex);
            }
        }

        private CopyMoveProgressResult OnFileProgress(long totalFileSize, long totalBytesTransferred, long streamSize,
            long streamBytesTransferred, int streamNumber, CopyMoveProgressCallbackReason callbackReason, object userData)
        {
            if (callbackReason == CopyMoveProgressCallbackReason.StreamSwitch)
            {
                return CopyMoveProgressResult.Continue;
            }

            ((List.TaskInfo)userData).mre.WaitOne();

            if (Functions.TaskManager.CancellationToken.IsCancellationRequested)
                throw (new OperationCanceledException(Functions.TaskManager.CancellationToken.Token));

            var percentage = Convert.ToDouble(totalBytesTransferred, System.Globalization.CultureInfo.InvariantCulture) / totalFileSize * 100;

            ((List.TaskInfo)userData).TaskStatusInfo = Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskStatus_CopyingFile)), new { Percentage = percentage, TransferredBytes = totalBytesTransferred, TotalBytes = totalFileSize });

            return CopyMoveProgressResult.Continue;
        }

        private void ReportToTaskManager(string message, bool log = false)
        {
            try
            {
                Main.FormAccessor.TmLogs.Report($"[{DateTime.Now}] {message}");

                if (log)
                {
                    Logger.Info(message);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public async Task<bool> DeleteFilesAsync(List.TaskInfo currentTask = null)
        {
            try
            {
                Parallel.ForEach(GetFileList(), currentFile =>
                {
                    try
                    {
                        currentTask?.mre.WaitOne();

                        currentFile.Refresh();
                        if (!currentFile.Exists) return;

                        if (currentTask != null)
                        {
                            currentTask.mre.WaitOne();

                            currentTask.TaskStatusInfo = Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskStatus_DeletingFile)), new { FileName = currentFile.Name, FormattedFileSize = Functions.FileSystem.FormatBytes(currentFile.Length) });

                            if (currentTask.ReportFileMovement)
                            {
                                ReportToTaskManager($"[{AppName}] {Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskStatus_DeletingFile)), new { FileName = currentFile.Name, FormattedFileSize = Functions.FileSystem.FormatBytes(currentFile.Length) })}", true);
                            }
                        }

                        File.SetAttributes(currentFile.FullName, FileAttributes.Normal);
                        currentFile.Delete();
                    }
                    catch (Exception ex)
                    {
                        Logger.Fatal(ex);
                    }
                });

                InstallationDirectory.Refresh();
                if (InstallationDirectory.Exists && !IsCompressed && InstallationDirectory.FullName.StartsWith(Library.FullPath, StringComparison.InvariantCultureIgnoreCase))
                {
                    await Task.Run(() => InstallationDirectory.Delete(true));
                }

                foreach (var dupItem in List.DupeItems.ToList().Where(x =>
                    x.App1 == this || x.App2 == this))
                {
                    List.DupeItemsRemove.Report(dupItem);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

        private static double GetElapsedTimeAverage(long fileSize, double elapsedTime)
        {
            try
            {
                return Math.Round(fileSize / 1024f / 1024f / elapsedTime, 3);
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}