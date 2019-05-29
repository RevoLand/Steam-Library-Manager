using CliWrap;
using FileCopyLib;
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
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public Library Library { get; set; }

        public int AppID { get; set; }
        public string AppName { get; set; }
        public DirectoryInfo InstallationDirectory;

        public long SizeOnDisk { get; set; }
        public bool IsCompressed { get; set; }
        public bool IsSteamBackup { get; set; }
        public bool IsCompacted { get; set; } = false;
        public DateTime LastUpdated { get; set; }
        public DateTime LastPlayed { get; set; }

        public string GameHeaderImage { get; }

        public string PrettyGameSize => Functions.FileSystem.FormatBytes(SizeOnDisk);

        public DirectoryInfo CommonFolder => new DirectoryInfo(Path.Combine(Library.Steam.CommonFolder.FullName, InstallationDirectory.Name));

        public DirectoryInfo DownloadFolder => new DirectoryInfo(Path.Combine(Library.Steam.DownloadFolder.FullName, InstallationDirectory.Name));

        public DirectoryInfo WorkShopPath => new DirectoryInfo(Path.Combine(Library.Steam.WorkshopFolder.FullName, "content", AppID.ToString()));

        public FileInfo CompressedArchiveName => new FileInfo(Path.Combine(Library.Steam.SteamAppsFolder.FullName, AppID + ".zip"));

        public FileInfo FullAcfPath => new FileInfo(Path.Combine(Library.Steam.SteamAppsFolder.FullName, AcfName));

        public FileInfo WorkShopAcfPath => new FileInfo(Path.Combine(Library.Steam.WorkshopFolder.FullName, WorkShopAcfName));

        public string AcfName { get; }

        public string WorkShopAcfName { get; }

        public SteamAppInfo(int appId, Library library, DirectoryInfo installationDirectory)
        {
            AppID = appId;
            Library = library;
            InstallationDirectory = installationDirectory;
            GameHeaderImage = $"http://cdn.akamai.steamstatic.com/steam/apps/{AppID}/header.jpg";
            AcfName = $"appmanifest_{AppID}.acf";
            WorkShopAcfName = $"appworkshop_{AppID}.acf";
        }

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

                        if (IsSteamBackup && !cItem.ShowToSteamBackup)
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
                                Header = Framework.StringFormat.Format(cItem.Header, new { AppName, AppID, SizeOnDisk = Functions.FileSystem.FormatBytes(SizeOnDisk) }),
                                Tag = cItem.Action,
                                Icon = Functions.FAwesome.GetAwesomeIcon(cItem.Icon, cItem.IconColor),
                                HorizontalContentAlignment = HorizontalAlignment.Left,
                                VerticalContentAlignment = VerticalAlignment.Center
                            };

                            SLMItem.Click += Main.FormAccessor.AppCMenuItem_Click;

                            rightClickMenu.Add(SLMItem);
                        }
                    }

                    return rightClickMenu;
                }
                catch (FormatException ex)
                {
                    MessageBox.Show(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.SteamAppInfo_FormatException)), new { ExceptionMessage = ex.Message }));

                    return rightClickMenu;
                }
            }
        }

        public async void ParseMenuItemActionAsync(string action)
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

                        Process.Start(string.Format(action, AppID, Properties.Settings.Default.SteamID64));
                        break;

                    case "compress":
                        if (Functions.TaskManager.TaskList.Count(x => x.SteamApp == this && x.TargetLibrary == Library) == 0)
                        {
                            Functions.TaskManager.AddTask(new List.TaskInfo
                            {
                                SteamApp = this,
                                TargetLibrary = Library,
                                Compress = !IsCompressed,
                                TaskType = Enums.TaskType.Compress
                            });
                        }
                        break;

                    case "compact":
                        if (Functions.TaskManager.TaskList.Count(x => x.SteamApp == this && x.TargetLibrary == Library && x.TaskType == Enums.TaskType.Compact) == 0)
                        {
                            Functions.TaskManager.AddTask(new List.TaskInfo
                            {
                                SteamApp = this,
                                TargetLibrary = Library,
                                TaskType = Enums.TaskType.Compact
                            });
                        }
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
                        await Task.Run(() => DeleteFilesAsync()).ConfigureAwait(false);

                        Library.Steam.Apps.Remove(this);
                        Functions.SLM.Library.UpdateLibraryVisual();

                        if (SLM.CurrentSelectedLibrary == Library)
                            Functions.App.UpdateAppPanel(Library);
                        break;

                    case "deleteappfilestm":
                        Functions.TaskManager.AddTask(new List.TaskInfo
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
                Logger.Fatal(ex);
            }
        }

        public List<FileInfo> GetFileList(bool includeDownloads = true, bool includeWorkshop = true)
        {
            try
            {
                var FileList = new List<FileInfo>();

                if (IsCompressed)
                {
                    FileList.Add(CompressedArchiveName);
                }
                else
                {
                    CommonFolder.Refresh();

                    if (CommonFolder.Exists)
                    {
                        var commonFiles = GetCommonFiles();

                        if (commonFiles != null)
                        {
                            FileList.AddRange(commonFiles);
                        }
                    }

                    DownloadFolder.Refresh();
                    if (includeDownloads && DownloadFolder.Exists)
                    {
                        var downloadFiles = GetDownloadFiles();
                        var patchFiles = GetPatchFiles();

                        if (downloadFiles != null)
                        {
                            FileList.AddRange(downloadFiles);
                        }

                        if (patchFiles != null)
                        {
                            FileList.AddRange(patchFiles);
                        }
                    }

                    WorkShopPath.Refresh();
                    if (includeWorkshop && WorkShopPath.Exists)
                    {
                        var workshopPath = GetWorkshopFiles();
                        FileList.AddRange(workshopPath);
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
                Logger.Fatal(ex);
                return null;
            }
        }

        private IEnumerable<FileInfo> GetCommonFiles()
        {
            try
            {
                CommonFolder.Refresh();
                return CommonFolder.EnumerateFiles("*", SearchOption.AllDirectories);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.SteamApp_GetCommonFilesError)), new { AppName, CommonFolderFullPath = CommonFolder.FullName, ExceptionMessage = ex.Message }));
                return null;
            }
        }

        private IEnumerable<FileInfo> GetDownloadFiles() => DownloadFolder.EnumerateFiles("*", SearchOption.AllDirectories);

        private IEnumerable<FileInfo> GetPatchFiles() => Library.Steam.DownloadFolder.EnumerateFiles($"*{AppID}*.patch", SearchOption.TopDirectoryOnly);

        private IEnumerable<FileInfo> GetWorkshopFiles() => WorkShopPath.EnumerateFiles("*", SearchOption.AllDirectories);

        public async Task<bool> CompactStatus()
        {
            try
            {
                if (!CommonFolder.Exists)
                    return false;

                var result = await Cli.Wrap("compact")
                    .SetArguments($"{((Properties.Settings.Default.AdvancedCompactSizeDetection) ? "/s" : "")} /q")
                    .SetWorkingDirectory(CommonFolder.FullName)
                    .ExecuteAsync().ConfigureAwait(false);

                if (Properties.Settings.Default.AdvancedCompactSizeDetection)
                {
                    var output = result.StandardOutput.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    var noutput = output[output.Length - 2].Split(new[] { " total bytes of data are stored in " },
                        StringSplitOptions.RemoveEmptyEntries);

                    var sizeBeforeCompact = noutput[0];
                    var sizeAfterCompact = Convert.ToInt64(noutput[1].Replace(" bytes.", "").Replace(".", ""));

                    if (sizeAfterCompact != 0)
                    {
                        SizeOnDisk = sizeAfterCompact;
                        Debug.WriteLine($"SizeOnDisk updated for game: {AppName} - new size: {SizeOnDisk}");
                    }
                }

                // May not be the best approach, to be improved if needed to.
                return !result.StandardOutput.Contains("0 are compressed");
            }
            catch (Exception ex)
            {
                LogToTM(ex.ToString());
                Debug.WriteLine(ex);

                return false;
            }
        }

        public async Task CompactTask(List.TaskInfo currentTask, CancellationToken cancellationToken)
        {
            try
            {
                /*
                    Syntax
                    compact [{/c|/u}] [/s[:dir]] [/a] [/i] [/f] [/q] [FileName[...]]

                    Parameters

                    /c : Compresses the specified directory or file.
                    /u : Uncompresses the specified directory or file.
                    /s : dir : Specifies that the requested action (compress or uncompress) be applied to all subdirectories of the specified directory, or of the current directory if none is specified.
                    /a : Displays hidden or system files.
                    /i : Ignores errors.
                    /f : Forces compression or uncompression of the specified directory or file. This is used in the case of a file that was partly compressed when the operation was interrupted by a system crash. To force the file to be compressed in its entirety, use the /c and /f parameters and specify the partially compressed file.
                    /q : Reports only the most essential information.
                    FileName : Specifies the file or directory. You can use multiple file names and wildcard characters (* and ?).
                    /? : Displays help at the command prompt.
                 */

                var AppFiles = GetFileList();
                currentTask.TotalFileCount = AppFiles.Count;
                long TotalFileSize = 0;

                Parallel.ForEach(AppFiles, file => Interlocked.Add(ref TotalFileSize, file.Length));
                currentTask.TotalFileSize = TotalFileSize;

                LogToTM($"Current status of {AppName} is {(IsCompacted ? "compressed" : "not compressed")} and the task is set to {(currentTask.Compact ? "compress" : "uncompress")} the app.");
                currentTask.ElapsedTime.Start();

                foreach (var file in AppFiles)
                {
                    currentTask.mre.WaitOne();

                    if (!file.Directory.Exists)
                    {
                        LogToTM($"Directory doesn't exists !? - {file.Directory.FullName}");
                    }

                    await Cli.Wrap("compact")
                        .SetArguments($"{(currentTask.Compact ? "/c" : "/u")} /i /q {(currentTask.ForceCompact ? "/f" : "")} /EXE:{currentTask.CompactLevel} {file.Name}")
                        .SetWorkingDirectory(file.Directory.FullName)
                        .SetCancellationToken(cancellationToken)
                        .SetStandardOutputCallback(OnCompactFolderProgress)
                        .SetStandardErrorCallback(OnCompactFolderProgress)
                        .EnableStandardErrorValidation()
                        .ExecuteAsync().ConfigureAwait(false);

                    Functions.TaskManager.ActiveTask.TaskStatusInfo = $"Compressing file: {file.FullName.Replace(Library.Steam.SteamAppsFolder.FullName, "")}";
                    currentTask.MovedFileSize += file.Length;
                }

                if (CommonFolder.Exists)
                {
                    var result = await Cli.Wrap("compact")
                        .SetArguments($"/s /q")
                        .SetWorkingDirectory(CommonFolder.FullName)
                        .SetCancellationToken(cancellationToken)
                        .EnableStandardErrorValidation()
                        .ExecuteAsync().ConfigureAwait(false);

                    var output = result.StandardOutput.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var resultText in output.Skip(output.Length - 3))
                    {
                        LogToTM(resultText);
                    }
                }

                currentTask.ElapsedTime.Stop();

                LogToTM($"[{AppName}] Compact task completed in {currentTask.ElapsedTime.Elapsed}");

                IsCompacted = await CompactStatus().ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                if (!currentTask.ErrorHappened)
                {
                    currentTask.ErrorHappened = true;
                    Functions.TaskManager.Stop();
                    currentTask.Active = false;
                    currentTask.Completed = true;

                    LogToTM(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskCancelled_ElapsedTime)), new { AppName, ElapsedTime = currentTask.ElapsedTime.Elapsed }));
                    Logger.Info(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskCancelled_ElapsedTime)), new { AppName, ElapsedTime = currentTask.ElapsedTime.Elapsed }));
                }
            }
            catch (Exception ex)
            {
                LogToTM(ex.ToString());
                Debug.WriteLine(ex);
            }
        }

        private void OnCompactFolderProgress(string progress)
        {
            if (progress?.Length == 0 || !Functions.TaskManager.ActiveTask.ReportFileMovement) return;

            Functions.TaskManager.ActiveTask.mre.WaitOne();
            LogToTM(progress);
        }

        public async Task CopyFilesAsync(List.TaskInfo CurrentTask, CancellationToken cancellationToken)
        {
            LogToTM(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.PopulatingFileList)), new { AppName }));
            Logger.Info(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.PopulatingFileList)), new { AppName }));

            var CopiedFiles = new List<string>();
            var CreatedDirectories = new List<string>();
            var AppFiles = GetFileList();
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
                Logger.Info(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FileListPopulated)), new { AppName, FileCount = AppFiles.Count, TotalFileSize = Functions.FileSystem.FormatBytes(TotalFileSize) }));

                // If the game is not compressed and user would like to compress it
                if (!IsCompressed && (CurrentTask.Compress || CurrentTask.TaskType == Enums.TaskType.Compress))
                {
                    FileInfo CompressedArchive = (CurrentTask.TaskType == Enums.TaskType.Compress) ? CompressedArchiveName : new FileInfo(CompressedArchiveName.FullName.Replace(Library.Steam.SteamAppsFolder.FullName, CurrentTask.TargetLibrary.Steam.SteamAppsFolder.FullName));

                    if (CompressedArchive.Exists)
                    {
                        while (CompressedArchive.IsFileLocked())
                        {
                            Logger.Info(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.SteamAppInfo_CompressedArchiveExistsAndInUse)), new { ArchiveFullPath = CompressedArchive.FullName }));
                            await Task.Delay(1000).ConfigureAwait(false);
                        }

                        CompressedArchive.Delete();
                    }

                    using (ZipArchive Archive = ZipFile.Open(CompressedArchive.FullName, ZipArchiveMode.Create))
                    {
                        try
                        {
                            CopiedFiles.Add(CompressedArchive.FullName);

                            foreach (FileSystemInfo currentFile in AppFiles)
                            {
                                CurrentTask.mre.WaitOne();

                                string FileNameInArchive = currentFile.FullName.Substring(Library.Steam.SteamAppsFolder.FullName.Length + 1);

                                CurrentTask.TaskStatusInfo = Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskStatus_CompressingFile)), new { FileName = currentFile.Name, FormattedFileSize = Functions.FileSystem.FormatBytes(((FileInfo)currentFile).Length) });

                                Archive.CreateEntryFromFile(currentFile.FullName, FileNameInArchive, Properties.Settings.Default.CompressionLevel.ParseEnum<CompressionLevel>());
                                CurrentTask.MovedFileSize += ((FileInfo)currentFile).Length;

                                if (CurrentTask.ReportFileMovement)
                                {
                                    LogToTM(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.SteamAppInfo_FileCompressed)), new { AppName, FileNameInArchive }));
                                }

                                Logger.Info(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.SteamAppInfo_FileCompressed)), new { AppName, FileNameInArchive }));

                                if (cancellationToken.IsCancellationRequested)
                                {
                                    throw new OperationCanceledException(cancellationToken);
                                }
                            }
                        }
                        catch (FileNotFoundException ex)
                        {
                            CurrentTask.ErrorHappened = true;
                            Functions.TaskManager.Stop();
                            CurrentTask.Active = false;
                            CurrentTask.Completed = true;

                            await Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                             {
                                 if (await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.RemoveMovedFiles)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.CompressArchive_FileNotFoundEx)), new { AppName, ExceptionMessage = ex.Message }), MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(false) == MessageDialogResult.Affirmative)
                                 {
                                     Functions.FileSystem.RemoveGivenFiles(CopiedFiles, CreatedDirectories, CurrentTask);
                                 }
                             }, System.Windows.Threading.DispatcherPriority.Normal).ConfigureAwait(false);

                            Main.FormAccessor.TmLogs.Report(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.CompressArchive_FileNotFoundEx)), new { AppName, ExceptionMessage = ex.Message }));
                            Logger.Fatal(ex);
                        }
                    }
                }
                // If the game is compressed and user would like to decompress it
                else if (IsCompressed && (!CurrentTask.Compress || CurrentTask.TaskType == Enums.TaskType.Compress))
                {
                    foreach (ZipArchiveEntry CurrentFile in ZipFile.OpenRead(CompressedArchiveName.FullName).Entries)
                    {
                        CurrentTask.mre.WaitOne();

                        FileInfo NewFile = new FileInfo(Path.Combine((CurrentTask.TaskType == Enums.TaskType.Compress) ? CurrentTask.SteamApp.Library.Steam.SteamAppsFolder.FullName : CurrentTask.TargetLibrary.Steam.SteamAppsFolder.FullName, CurrentFile.FullName));

                        if (!NewFile.Directory.Exists)
                        {
                            NewFile.Directory.Create();
                            CreatedDirectories.Add(NewFile.Directory.FullName);
                        }

                        CurrentTask.TaskStatusInfo = Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskStatus_Decompress)), new { NewFileName = NewFile.FullName, NewFileSize = Functions.FileSystem.FormatBytes(CurrentFile.Length) });

                        CurrentFile.ExtractToFile(NewFile.FullName, true);

                        CopiedFiles.Add(NewFile.FullName);
                        CurrentTask.MovedFileSize += CurrentFile.Length;

                        if (CurrentTask.ReportFileMovement)
                        {
                            LogToTM(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.SteamAppInfo_FileDecompressed)), new { AppName, NewFileFullPath = NewFile.FullName }));
                        }

                        Logger.Info(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.SteamAppInfo_FileDecompressed)), new { AppName, NewFileFullPath = NewFile.FullName }));

                        if (cancellationToken.IsCancellationRequested)
                        {
                            throw new OperationCanceledException(cancellationToken);
                        }
                    }
                }
                // Everything else
                else
                {
                    // Create directories
                    Parallel.ForEach(AppFiles, POptions, CurrentFile =>
                    {
                        var NewFile = new FileInfo(CurrentFile.FullName.Replace(Library.Steam.SteamAppsFolder.FullName, CurrentTask.TargetLibrary.Steam.SteamAppsFolder.FullName));

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
                            var NewFile = new FileInfo(CurrentFile.FullName.Replace(Library.Steam.SteamAppsFolder.FullName, CurrentTask.TargetLibrary.Steam.SteamAppsFolder.FullName));

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

                            Logger.Info(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FileMoved)), new { AppName, NewFileName = NewFile.FullName }));
                        }
                        catch (System.ComponentModel.Win32Exception)
                        {
                            throw new OperationCanceledException(cancellationToken);
                        }
                        catch (PathTooLongException ex)
                        {
                            CurrentTask.ErrorHappened = true;
                            Functions.TaskManager.Stop();
                            CurrentTask.Active = false;
                            CurrentTask.Completed = true;

                            Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                            {
                                if (await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.RemoveMovedFiles)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.PathTooLongException)), new { AppName, ExceptionMessage = ex.Message }), MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(false) == MessageDialogResult.Affirmative)
                                {
                                    Functions.FileSystem.RemoveGivenFiles(CopiedFiles, CreatedDirectories, CurrentTask);
                                }
                            }, System.Windows.Threading.DispatcherPriority.Normal);

                            Main.FormAccessor.TmLogs.Report(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FileSystemRelatedError)), new { AppName, ExceptionMessage = ex.Message }));
                            Logger.Fatal(ex);
                        }
                        catch (IOException ex)
                        {
                            CurrentTask.ErrorHappened = true;
                            Functions.TaskManager.Stop();
                            CurrentTask.Active = false;
                            CurrentTask.Completed = true;

                            Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                            {
                                if (await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.RemoveMovedFiles)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FileSystemRelatedError_DeleteMovedFiles)), new { AppName, ExceptionMessage = ex.Message }), MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(false) == MessageDialogResult.Affirmative)
                                {
                                    Functions.FileSystem.RemoveGivenFiles(CopiedFiles, CreatedDirectories, CurrentTask);
                                }
                            }, System.Windows.Threading.DispatcherPriority.Normal);

                            Main.FormAccessor.TmLogs.Report(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FileSystemRelatedError)), new { AppName, ExceptionMessage = ex.Message }));
                            Logger.Fatal(ex);
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                            {
                                if (await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.RemoveMovedFiles)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FilePermissionRelatedError_DeleteFiles)), new { AppName, ExceptionMessage = ex.Message }), MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(false) == MessageDialogResult.Affirmative)
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

                            var NewFile = new FileInfo(CurrentFile.FullName.Replace(Library.Steam.SteamAppsFolder.FullName, CurrentTask.TargetLibrary.Steam.SteamAppsFolder.FullName));

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

                            Logger.Info(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FileMoved)), new { AppName, NewFileName = NewFile.FullName }));
                        }
                        catch (System.ComponentModel.Win32Exception)
                        {
                            throw new OperationCanceledException(cancellationToken);
                        }
                        catch (PathTooLongException ex)
                        {
                            CurrentTask.ErrorHappened = true;
                            Functions.TaskManager.Stop();
                            CurrentTask.Active = false;
                            CurrentTask.Completed = true;

                            Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                            {
                                if (await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.RemoveMovedFiles)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.PathTooLongException)), new { AppName, ExceptionMessage = ex.Message }), MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(false) == MessageDialogResult.Affirmative)
                                {
                                    Functions.FileSystem.RemoveGivenFiles(CopiedFiles, CreatedDirectories, CurrentTask);
                                }
                            }, System.Windows.Threading.DispatcherPriority.Normal);

                            Main.FormAccessor.TmLogs.Report(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FileSystemRelatedError)), new { AppName, ExceptionMessage = ex.Message }));
                            Logger.Fatal(ex);
                        }
                        catch (IOException ex)
                        {
                            CurrentTask.ErrorHappened = true;
                            Functions.TaskManager.Stop();
                            CurrentTask.Active = false;
                            CurrentTask.Completed = true;

                            Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                            {
                                if (await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.RemoveMovedFiles)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FileSystemRelatedError_DeleteMovedFiles)), new { AppName, ExceptionMessage = ex.Message }), MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(false) == MessageDialogResult.Affirmative)
                                {
                                    Functions.FileSystem.RemoveGivenFiles(CopiedFiles, CreatedDirectories, CurrentTask);
                                }
                            }, System.Windows.Threading.DispatcherPriority.Normal);

                            Main.FormAccessor.TmLogs.Report(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FileSystemRelatedError)), new { AppName, ExceptionMessage = ex.Message }));
                            Logger.Fatal(ex);
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                            {
                                if (await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.RemoveMovedFiles)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FilePermissionRelatedError_DeleteFiles)), new { AppName, ExceptionMessage = ex.Message }), MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(false) == MessageDialogResult.Affirmative)
                                {
                                    Functions.FileSystem.RemoveGivenFiles(CopiedFiles, CreatedDirectories, CurrentTask);
                                }
                            }, System.Windows.Threading.DispatcherPriority.Normal);
                        }
                    });
                }

                CurrentTask.ElapsedTime.Stop();
                CurrentTask.MovedFileSize = TotalFileSize;

                LogToTM(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskCompleted)), new { AppName, ElapsedTime = CurrentTask.ElapsedTime.Elapsed, AverageSpeed = GetElapsedTimeAverage(TotalFileSize, CurrentTask.ElapsedTime.Elapsed.TotalSeconds), AverageFileSize = Functions.FileSystem.FormatBytes(TotalFileSize / (long)CurrentTask.TotalFileCount) }));
                Logger.Info(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskCompleted)), new { AppName, ElapsedTime = CurrentTask.ElapsedTime.Elapsed, AverageSpeed = GetElapsedTimeAverage(TotalFileSize, CurrentTask.ElapsedTime.Elapsed.TotalSeconds), AverageFileSize = Functions.FileSystem.FormatBytes(TotalFileSize / (long)CurrentTask.TotalFileCount) }));
            }
            catch (OperationCanceledException)
            {
                if (!CurrentTask.ErrorHappened)
                {
                    CurrentTask.ErrorHappened = true;
                    Functions.TaskManager.Stop();
                    CurrentTask.Active = false;
                    CurrentTask.Completed = true;

                    await Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                    {
                        if (await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.RemoveMovedFiles)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskCancelled_RemoveFiles)), new { AppName }), MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(false) == MessageDialogResult.Affirmative)
                        {
                            Functions.FileSystem.RemoveGivenFiles(CopiedFiles, CreatedDirectories, CurrentTask);
                        }
                    }, System.Windows.Threading.DispatcherPriority.Normal).ConfigureAwait(false);

                    LogToTM(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskCancelled_ElapsedTime)), new { AppName, ElapsedTime = CurrentTask.ElapsedTime.Elapsed }));
                    Logger.Info(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskCancelled_ElapsedTime)), new { AppName, ElapsedTime = CurrentTask.ElapsedTime.Elapsed }));
                }
            }
            catch (Exception ex)
            {
                CurrentTask.ErrorHappened = true;
                Functions.TaskManager.Stop();
                CurrentTask.Active = false;
                CurrentTask.Completed = true;

                await Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                 {
                     if (await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.RemoveMovedFiles)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.AnyException_RemoveFiles)), new { AppName, ExceptionMessage = ex.Message }), MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(false) == MessageDialogResult.Affirmative)
                     {
                         Functions.FileSystem.RemoveGivenFiles(CopiedFiles, CreatedDirectories, CurrentTask);
                     }
                 }, System.Windows.Threading.DispatcherPriority.Normal).ConfigureAwait(false);

                Main.FormAccessor.TmLogs.Report(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.AnyError_ElapsedTime)), new { AppName, ElapsedTime = CurrentTask.ElapsedTime.Elapsed }));
                Logger.Fatal(ex);
            }
        }

        private void OnFileProgress(FileProgress s)
        {
            Functions.TaskManager.ActiveTask.mre.WaitOne();

            if (Functions.TaskManager.CancellationToken.IsCancellationRequested)
                throw (new OperationCanceledException(Functions.TaskManager.CancellationToken.Token));

            Functions.TaskManager.ActiveTask.TaskStatusInfo = Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskStatus_CopyingFile)), new { Percentage = s.Percentage.ToString("0.00"), TransferredBytes = s.Transferred, TotalBytes = s.Total });
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

        private void LogToTM(string TextToLog)
        {
            try
            {
                Main.FormAccessor.TmLogs.Report($"[{DateTime.Now}] {TextToLog}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Logger.Error(ex);
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
                        await Task.Run(() => CompressedArchiveName.Delete()).ConfigureAwait(false);
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

                                        CurrentTask.TaskStatusInfo = Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskStatus_DeletingFile)), new { FileName = currentFile.Name, FormattedFileSize = Functions.FileSystem.FormatBytes(currentFile.Length) });

                                        if (CurrentTask.ReportFileMovement)
                                        {
                                            Main.FormAccessor.TmLogs.Report($"[{DateTime.Now}] [{AppName}] {Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskStatus_DeletingFile)), new { FileName = currentFile.Name, FormattedFileSize = Functions.FileSystem.FormatBytes(currentFile.Length) })}");
                                        }
                                    }

                                    File.SetAttributes(currentFile.FullName, FileAttributes.Normal);
                                    currentFile.Delete();
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Fatal(ex);
                            }
                        }
                        );
                    }

                    CommonFolder.Refresh();
                    // common folder, if exists
                    if (CommonFolder.Exists)
                    {
                        await Task.Run(() => CommonFolder.Delete(true)).ConfigureAwait(false);
                    }

                    DownloadFolder.Refresh();
                    // downloading folder, if exists
                    if (DownloadFolder.Exists)
                    {
                        await Task.Run(() => DownloadFolder.Delete(true)).ConfigureAwait(false);
                    }

                    WorkShopPath.Refresh();
                    // workshop folder, if exists
                    if (WorkShopPath.Exists)
                    {
                        await Task.Run(() => WorkShopPath.Delete(true)).ConfigureAwait(false);
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
                Logger.Error(ex);
                return true;
            }
            catch (DirectoryNotFoundException ex)
            {
                Logger.Error(ex);
                return true;
            }
            catch (IOException ex)
            {
                Logger.Error(ex);
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Error(ex);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Logger.Fatal(ex);

                return false;
            }
        }
    }
}