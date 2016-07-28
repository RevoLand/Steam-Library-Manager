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
        public int appID { get; set; }
        public string appName { get; set; }
        public string gameHeaderImage { get; set; }
        public string prettyGameSize { get; set; }
        public DirectoryInfo installationPath, commonPath, downloadPath, workShopPath;
        public FileInfo fullAcfPath, workShopAcfPath, compressedName;
        public string acfName, workShopAcfName;
        public long sizeOnDisk { get; set; }
        public Framework.AsyncObservableCollection<FrameworkElement> contextMenuItems { get; set; }
        public bool IsCompressed { get; set; }
        public bool IsSteamBackup { get; set; }
        public Library installedLibrary { get; set; }

        public Framework.AsyncObservableCollection<FrameworkElement> generateRightClickMenuItems()
        {
            Framework.AsyncObservableCollection<FrameworkElement> rightClickMenu = new Framework.AsyncObservableCollection<FrameworkElement>();
            try
            {
                foreach (List.contextMenu cItem in List.gameContextMenuItems.Where(x => x.IsActive))
                {
                    if (IsSteamBackup && cItem.showToSteamBackup == SLM.Settings.menuVisibility.NotVisible)
                        continue;
                    else if (installedLibrary.Backup && cItem.showToSLMBackup == SLM.Settings.menuVisibility.NotVisible)
                        continue;
                    else if (IsCompressed && cItem.showToCompressed == SLM.Settings.menuVisibility.NotVisible)
                        continue;
                    else if (cItem.showToNormal == SLM.Settings.menuVisibility.NotVisible)
                        continue;

                    if (cItem.IsSeparator)
                        rightClickMenu.Add(new Separator());
                    else
                    {
                        MenuItem slmItem = new MenuItem();

                        slmItem.Tag = this;
                        slmItem.Header = string.Format(cItem.Header, appName, appID, Functions.fileSystem.FormatBytes(sizeOnDisk));
                        slmItem.Tag = cItem.Action;
                        slmItem.Icon = Functions.fAwesome.getAwesomeIcon(cItem.Icon, cItem.IconColor);

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

        public void parseMenuItemAction(string Action)
        {
            switch (Action.ToLowerInvariant())
            {
                default:
                    System.Diagnostics.Process.Start(string.Format(Action, appID, Definitions.SLM.userSteamID64));
                    break;
                case "disk":
                    if (commonPath.Exists)
                        System.Diagnostics.Process.Start(commonPath.FullName);
                    break;
                case "acffile":
                    System.Diagnostics.Process.Start(fullAcfPath.FullName);
                    break;
                case "deletegamefilesslm":

                    deleteFiles();
                    RemoveFromLibrary();

                    break;
            }
        }

        public List<FileSystemInfo> getFileList(bool includeDownloads = true, bool includeWorkshop = true)
        {
            List<FileSystemInfo> FileList = new List<FileSystemInfo>();

            if (IsCompressed)
            {
                FileList.Add(compressedName);
            }
            else
            {
                if (commonPath.Exists)
                {
                    FileList.AddRange(getCommonFiles());
                }

                if (includeDownloads && downloadPath.Exists)
                {
                    FileList.AddRange(getDownloadFiles());
                    FileList.AddRange(getPatchFiles());
                }

                if (includeWorkshop && workShopPath.Exists)
                {
                    FileList.AddRange(getWorkshopFiles());
                }
            }
            return FileList;
        }

        public List<FileSystemInfo> getCommonFiles() => commonPath.GetFileSystemInfos("*", SearchOption.AllDirectories).Where(x => !x.Attributes.HasFlag(FileAttributes.Directory)).ToList();

        public List<FileSystemInfo> getDownloadFiles() => downloadPath.GetFileSystemInfos("*", SearchOption.AllDirectories).Where(x => !x.Attributes.HasFlag(FileAttributes.Directory)).ToList();

        public List<FileSystemInfo> getPatchFiles() => installedLibrary.downloadPath.GetFileSystemInfos("*", SearchOption.AllDirectories).Where(x => !x.Attributes.HasFlag(FileAttributes.Directory)).ToList();

        public List<FileSystemInfo> getWorkshopFiles() => workShopPath.GetFileSystemInfos("*", SearchOption.AllDirectories).Where(x => !x.Attributes.HasFlag(FileAttributes.Directory)).ToList();

        public void copyGameFiles(Forms.MoveGameForm currentForm, Library targetLibrary, CancellationTokenSource cancellationToken, bool compressGame = false)
        {
            ConcurrentBag<string> copiedFiles = new ConcurrentBag<string>();
            ConcurrentBag<string> createdDirectories = new ConcurrentBag<string>();
            List<FileSystemInfo> gameFiles = getFileList();
            System.Diagnostics.Stopwatch timeElapsed = new System.Diagnostics.Stopwatch();
            timeElapsed.Start();

            try
            {
                int totalMovenFileCount = 0;
                long totalFileSize = 0, movenFileSize = 0;
                ParallelOptions parallelOptions = new ParallelOptions()
                {
                    CancellationToken = cancellationToken.Token
                };

                Parallel.ForEach(gameFiles, parallelOptions, file =>
                {
                    Interlocked.Add(ref totalFileSize, (file as FileInfo).Length);
                });

                if (!IsCompressed && compressGame)
                {
                    FileInfo compressedArchive = new FileInfo(compressedName.FullName.Replace(installedLibrary.steamAppsPath.FullName, targetLibrary.steamAppsPath.FullName));

                    if (compressedArchive.Exists)
                        compressedArchive.Delete();

                    using (ZipArchive compressed = ZipFile.Open(compressedArchive.FullName, ZipArchiveMode.Create))
                    {
                        copiedFiles.Add(compressedArchive.FullName);

                        foreach (FileSystemInfo currentFile in gameFiles)
                        {
                            totalMovenFileCount++;
                            movenFileSize += (currentFile as FileInfo).Length;

                            string newFileName = currentFile.FullName.Substring(installedLibrary.steamAppsPath.FullName.Length + 1);

                            compressed.CreateEntryFromFile(currentFile.FullName, newFileName, CompressionLevel.Optimal);

                            currentForm.reportFileMovement(newFileName, totalMovenFileCount, gameFiles.Count, movenFileSize, totalFileSize);

                            if (cancellationToken.IsCancellationRequested)
                                throw new OperationCanceledException(cancellationToken.Token);
                        }

                        compressed.CreateEntryFromFile(fullAcfPath.FullName, acfName);
                    }
                }
                else if (IsCompressed && !compressGame)
                {
                    foreach (ZipArchiveEntry currentFile in ZipFile.OpenRead(compressedName.FullName).Entries)
                    {
                        totalMovenFileCount++;
                        movenFileSize += currentFile.Length;

                        FileInfo newFile = new FileInfo(Path.Combine(installedLibrary.steamAppsPath.FullName, currentFile.FullName));

                        if (!newFile.Directory.Exists)
                        {
                            newFile.Directory.Create();
                            createdDirectories.Add(newFile.Directory.FullName);
                        }

                        currentFile.ExtractToFile(newFile.FullName, true);
                        copiedFiles.Add(newFile.FullName);

                        currentForm.reportFileMovement(newFile.FullName, totalMovenFileCount, gameFiles.Count, movenFileSize, totalFileSize);

                        if (cancellationToken.IsCancellationRequested)
                            throw new OperationCanceledException(cancellationToken.Token);
                    }
                }
                else
                {
                    Parallel.ForEach(gameFiles.Where(x => (x as FileInfo).Length <= Properties.Settings.Default.ParallelAfterSize), parallelOptions, currentFile =>
                    {
                        FileInfo newFile = new FileInfo(currentFile.FullName.Replace(installedLibrary.steamAppsPath.FullName, targetLibrary.steamAppsPath.FullName));

                        if (!newFile.Exists || (newFile.Length != (currentFile as FileInfo).Length || newFile.LastWriteTime != (currentFile as FileInfo).LastWriteTime))
                        {
                            if (!newFile.Directory.Exists)
                            {
                                newFile.Directory.Create();
                                createdDirectories.Add(newFile.Directory.FullName);
                            }

                            (currentFile as FileInfo).CopyTo(newFile.FullName, true);
                        }

                        Interlocked.Increment(ref totalMovenFileCount);
                        Interlocked.Add(ref movenFileSize, (currentFile as FileInfo).Length);
                        copiedFiles.Add(newFile.FullName);

                        currentForm.reportFileMovement(newFile.FullName, totalMovenFileCount, gameFiles.Count, movenFileSize, totalFileSize);
                    });

                    parallelOptions.MaxDegreeOfParallelism = 1;

                    Parallel.ForEach(gameFiles.Where(x => (x as FileInfo).Length > Properties.Settings.Default.ParallelAfterSize), parallelOptions, currentFile =>
                    {
                        FileInfo newFile = new FileInfo(currentFile.FullName.Replace(installedLibrary.steamAppsPath.FullName, targetLibrary.steamAppsPath.FullName));

                        if (!newFile.Exists || (newFile.Length != (currentFile as FileInfo).Length || newFile.LastWriteTime != (currentFile as FileInfo).LastWriteTime))
                        {
                            if (!newFile.Directory.Exists)
                            {
                                newFile.Directory.Create();
                                createdDirectories.Add(newFile.Directory.FullName);
                            }

                            (currentFile as FileInfo).CopyTo(newFile.FullName, true);
                        }

                        Interlocked.Increment(ref totalMovenFileCount);
                        Interlocked.Add(ref movenFileSize, (currentFile as FileInfo).Length);
                        copiedFiles.Add(newFile.FullName);

                        currentForm.reportFileMovement(newFile.FullName, totalMovenFileCount, gameFiles.Count, movenFileSize, totalFileSize);
                    });

                    if (!IsCompressed)
                    {
                        // Copy .ACF file
                        if (fullAcfPath.Exists)
                            fullAcfPath.CopyTo(Path.Combine(targetLibrary.steamAppsPath.FullName, acfName), true);

                        if (workShopAcfPath.Exists)
                        {
                            FileInfo newACFPath = new FileInfo(workShopAcfPath.FullName.Replace(installedLibrary.steamAppsPath.FullName, targetLibrary.steamAppsPath.FullName));

                            if (!newACFPath.Directory.Exists)
                            {
                                newACFPath.Directory.Create();
                                createdDirectories.Add(newACFPath.Directory.FullName);
                            }

                            workShopAcfPath.CopyTo(newACFPath.FullName, true);
                        }
                    }
                }

                timeElapsed.Stop();
                currentForm.formLogs.Add($"Time elapsed: {timeElapsed.Elapsed}");
                Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate
                {
                    currentForm.button.Content = "Close";
                });
            }
            catch (OperationCanceledException)
            {
                MessageBoxResult moveGamesBeforeDeletion = MessageBox.Show("Game movement cancelled. Would you like to remove files that already moven?", "Remove moven files?", MessageBoxButton.YesNo);

                if (moveGamesBeforeDeletion == MessageBoxResult.Yes)
                    Functions.fileSystem.removeGivenFiles(copiedFiles, createdDirectories);

                currentForm.formLogs.Add($"Operation cancelled by user. Time Elapsed: {timeElapsed.Elapsed}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                MessageBoxResult moveGamesBeforeDeletion = MessageBox.Show("An error happened while moving game files. Would you like to remove files that already moven?", "Remove moven files?", MessageBoxButton.YesNo);

                if (moveGamesBeforeDeletion == MessageBoxResult.Yes)
                    Functions.fileSystem.removeGivenFiles(copiedFiles, createdDirectories);

                currentForm.formLogs.Add($"An error happened while moving game files. Time Elapsed: {timeElapsed.Elapsed}");
            }
        }

        public bool deleteFiles()
        {
            try
            {
                if (IsCompressed)
                {
                    compressedName.Delete();
                }
                else if (IsSteamBackup)
                {
                    if (installationPath.Exists)
                        installationPath.Delete(true);
                }
                else
                {
                    List<FileSystemInfo> gameFiles = getFileList();

                    Parallel.ForEach(gameFiles, currentFile =>
                    {
                        if (currentFile.Exists)
                            currentFile.Delete();
                    }
                    );

                    // common folder, if exists
                    if (commonPath.Exists)
                        commonPath.Delete(true);

                    // downloading folder, if exists
                    if (downloadPath.Exists)
                        downloadPath.Delete(true);

                    // workshop folder, if exists
                    if (workShopPath.Exists)
                        workShopPath.Delete(true);

                    // game .acf file
                    if (fullAcfPath.Exists)
                        fullAcfPath.Delete();

                    // workshop .acf file
                    if (workShopAcfPath.Exists)
                        workShopAcfPath.Delete();
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
            installedLibrary.Games.Remove(this);

            installedLibrary.updateLibraryVisual();

            if (SLM.selectedLibrary == installedLibrary)
                Functions.Games.UpdateMainForm(installedLibrary);
        }
    }
}
