using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Steam_Library_Manager.Functions
{
    class fileSystem
    {

        public class Game
        {
            public void copyGameFiles(Forms.MoveGameForm currentForm, List<FileSystemInfo> gameFiles, Definitions.List.Game Game, Definitions.List.Library targetLibrary, CancellationTokenSource cancellationToken)
            {
                ConcurrentBag<string> movedFiles = new ConcurrentBag<string>();
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                try
                {
                    int fileIndex = 0;
                    long totalSize = 0, movenSize = 0;
                    ParallelOptions options = new ParallelOptions();
                    options.CancellationToken = cancellationToken.Token;

                    Parallel.ForEach(gameFiles, options, file =>
                    {
                        Interlocked.Add(ref totalSize, (file as FileInfo).Length);
                    });

                    Parallel.ForEach(gameFiles.Where(x => (x as FileInfo).Length <= Properties.Settings.Default.ParallelAfterSize), options, currentFile =>
                    {
                        FileInfo newFile = new FileInfo(currentFile.FullName.Replace(Game.Library.steamAppsPath, targetLibrary.steamAppsPath));

                        if (!newFile.Exists || (newFile.Length != (currentFile as FileInfo).Length || newFile.LastWriteTime != (currentFile as FileInfo).LastWriteTime))
                        {
                            if (!newFile.Directory.Exists)
                                newFile.Directory.Create();

                            (currentFile as FileInfo).CopyTo(newFile.FullName, true);
                        }

                        Interlocked.Increment(ref fileIndex);
                        Interlocked.Add(ref movenSize, (currentFile as FileInfo).Length);
                        movedFiles.Add(newFile.FullName);

                        Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Action)delegate
                        {
                            currentForm.formLogs.Add(string.Format("[{0}/{1}] {2}\n", fileIndex, gameFiles.Count, newFile.FullName));

                            currentForm.progressReportLabel.Content = $"{fileSystem.FormatBytes(totalSize - movenSize)} left - {FormatBytes(movenSize)} / {FormatBytes(totalSize)}";
                            currentForm.progressReport.Value = ((int)Math.Round((double)(100 * movenSize) / totalSize));
                        });
                    });

                    options.MaxDegreeOfParallelism = 1;

                    Parallel.ForEach(gameFiles.Where(x => (x as FileInfo).Length > Properties.Settings.Default.ParallelAfterSize), options, currentFile =>
                    {
                        FileInfo newFile = new FileInfo(currentFile.FullName.Replace(Game.Library.steamAppsPath, targetLibrary.steamAppsPath));

                        if (!newFile.Exists || (newFile.Length != (currentFile as FileInfo).Length || newFile.LastWriteTime != (currentFile as FileInfo).LastWriteTime))
                        {
                            if (!newFile.Directory.Exists)
                                newFile.Directory.Create();

                            (currentFile as FileInfo).CopyTo(newFile.FullName, true);
                        }

                        Interlocked.Increment(ref fileIndex);
                        Interlocked.Add(ref movenSize, (currentFile as FileInfo).Length);
                        movedFiles.Add(newFile.FullName);

                        Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (System.Action)delegate
                        {
                            currentForm.formLogs.Add(string.Format("[{0}/{1}] {2}\n", fileIndex, gameFiles.Count, newFile.FullName));

                            currentForm.progressReportLabel.Content = $"{fileSystem.FormatBytes(totalSize - movenSize)} left - {FormatBytes(movenSize)} / {FormatBytes(totalSize)}";
                            currentForm.progressReport.Value = ((int)Math.Round((double)(100 * movenSize) / totalSize));
                        });
                    });

                    // Copy .ACF file
                    File.Copy(Game.acfPath, Path.Combine(targetLibrary.steamAppsPath, Game.acfName), true);

                    if (File.Exists(Game.workShopAcfName))
                        File.Copy(Game.workShopAcfName, Game.workShopAcfName.Replace(Game.Library.steamAppsPath, targetLibrary.steamAppsPath), true);

                    sw.Stop();
                    Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate
                    {
                        currentForm.formLogs.Add($"Time elapsed: {sw.Elapsed}");
                        currentForm.button.Content = "Close";
                    });
                }
                catch (OperationCanceledException)
                {
                    fileSystem.Game.removeGivenFiles(movedFiles);
                    MessageBox.Show("Cancelled by user");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

            public async Task<bool> deleteGameFiles(Definitions.List.Game Game, List<FileSystemInfo> gameFiles = null)
            {
                try
                {
                    if (Game.Compressed)
                    {
                        string currentZipNameNpath = Path.Combine(Game.Library.steamAppsPath, $"{Game.appID}.zip");

                        if (File.Exists(currentZipNameNpath))
                            await Task.Run(() => File.Delete(currentZipNameNpath));
                    }
                    else if(Game.SteamBackup)
                    {
                        if (Directory.Exists(Game.installationPath))
                            Directory.Delete(Game.installationPath, true);
                    }
                    else
                    {
                        if (gameFiles == null || gameFiles.Count == 0)
                            gameFiles = await getFileList(Game);

                        Parallel.ForEach(gameFiles, currentFile =>
                        {
                            if (currentFile.Exists)
                                currentFile.Delete();
                        }
                        );

                        // common folder, if exists
                        if (Directory.Exists(Game.commonPath))
                            await Task.Run(() => Directory.Delete(Game.commonPath, true));

                        // downloading folder, if exists
                        if (Directory.Exists(Game.downloadPath))
                            await Task.Run(() => Directory.Delete(Game.downloadPath, true));

                        // workshop folder, if exists
                        if (Directory.Exists(Game.workShopPath))
                            await Task.Run(() => Directory.Delete(Game.workShopPath, true));

                        // game .acf file
                        if (File.Exists(Game.acfPath))
                            File.Delete(Game.acfPath);

                        // workshop .acf file
                        if (File.Exists(Game.workShopAcfPath))
                            File.Delete(Game.workShopAcfPath);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());

                    return false;
                }

                return true;
            }

            public static void removeGivenFiles(ConcurrentBag<string> fileList, Definitions.List.Game Game = null, Definitions.List.Library targetLibrary = null, bool removeAcfFile = true)
            {
                try
                {
                    Parallel.ForEach(fileList, currentFile =>
                    {
                        FileInfo file = new FileInfo(currentFile);

                        if (file.Exists)
                            file.Delete();
                    });

                    if (Game != null && targetLibrary != null)
                    {
                        string combinedInstallationPath = Path.Combine(targetLibrary.commonPath, Game.installationPath);

                        if (Directory.Exists(combinedInstallationPath))
                            Directory.Delete(combinedInstallationPath, true);

                        if (removeAcfFile)
                        {
                            string acfPath = Game.acfPath.Replace(Game.Library.steamAppsPath, targetLibrary.steamAppsPath);
                            if (File.Exists(acfPath))
                                File.Delete(acfPath);
                        }
                    }
                }
                catch { }
            }

            public async Task<List<FileSystemInfo>> getFileList(Definitions.List.Game Game, bool includeDownloads = true, bool includeWorkshop = true)
            {
                List<FileSystemInfo> FileList = new List<FileSystemInfo>();

                if (!string.IsNullOrEmpty(Game.commonPath) && Directory.Exists(Game.commonPath))
                {
                    FileList.AddRange(await getCommonFiles(Game));
                }

                if (includeDownloads && !string.IsNullOrEmpty(Game.downloadPath) && Directory.Exists(Game.downloadPath))
                {
                    FileList.AddRange(await getDownloadFiles(Game));
                    FileList.AddRange(await getPatchFiles(Game));
                }

                if (includeWorkshop && !string.IsNullOrEmpty(Game.workShopAcfPath) && Directory.Exists(Game.workShopPath))
                {
                    FileList.AddRange(await getWorkshopFiles(Game));
                }

                return FileList;
            }

            async Task<List<FileSystemInfo>> getCommonFiles(Definitions.List.Game Game) => await Task.Run(() => new DirectoryInfo(Game.commonPath).GetFileSystemInfos("*", SearchOption.AllDirectories).Where(x => !x.Attributes.HasFlag(FileAttributes.Directory)).ToList());

            async Task<List<FileSystemInfo>> getDownloadFiles(Definitions.List.Game Game) => await Task.Run(() => new DirectoryInfo(Game.downloadPath).GetFileSystemInfos("*", SearchOption.AllDirectories).Where(x => !x.Attributes.HasFlag(FileAttributes.Directory)).ToList());

            async Task<List<FileSystemInfo>> getPatchFiles(Definitions.List.Game Game) => await Task.Run(() => new DirectoryInfo(Game.Library.downloadPath).GetFileSystemInfos("*", SearchOption.AllDirectories).Where(x => !x.Attributes.HasFlag(FileAttributes.Directory)).ToList());

            async Task<List<FileSystemInfo>> getWorkshopFiles(Definitions.List.Game Game) => await Task.Run(() => new DirectoryInfo(Game.workShopPath).GetFileSystemInfos("*", SearchOption.AllDirectories).Where(x => !x.Attributes.HasFlag(FileAttributes.Directory)).ToList());

            /*
            public async Task<bool> copyGameArchive(Forms.moveGame currentForm, string currentZipNameNpath, string newZipNameNpath)
            {
                try
                {
                    // If archive already exists in the target library
                    if (File.Exists(newZipNameNpath))
                    {
                        // And file size doesn't equals
                        if (FileSystem.getFileSize(currentZipNameNpath) != FileSystem.getFileSize(newZipNameNpath))
                            // Remove the compressed archive
                            await Task.Run(() => File.Delete(newZipNameNpath));
                    }
                    else
                        await Task.Run(() => File.Copy(currentZipNameNpath, newZipNameNpath));
                }
                catch (Exception ex)
                {
                    currentForm.logToFormAsync(ex.ToString());

                    return false;
                }

                return true;
            }
            */

            /*
            public async Task<bool> compressGameFiles(Forms.moveGame currentForm, List<string> gameFiles, string newZipNameNpath, Definitions.List.Game Game, Definitions.List.Library targetLibrary)
            {
                string newFileName;
                try
                {
                    if (Directory.Exists(Path.GetDirectoryName(newZipNameNpath)))
                    {
                        // If compressed archive already exists
                        if (File.Exists(newZipNameNpath))
                            // Remove the compressed archive
                            File.Delete(newZipNameNpath);
                    }
                    else
                        Directory.CreateDirectory(Path.GetDirectoryName(newZipNameNpath));

                    // Create a new compressed archive at target library
                    using (ZipArchive gameBackup = ZipFile.Open(newZipNameNpath, ZipArchiveMode.Create))
                    {
                        // For each file in common folder of game
                        foreach (string currentFile in gameFiles)
                        {
                            // Define a string for better looking
                            newFileName = currentFile.Substring(Game.Library.steamAppsPath.Length + 1);

                            // Add file to archive
                            await Task.Run(() => gameBackup.CreateEntryFromFile(currentFile, newFileName, CompressionLevel.Optimal));

                            // Perform step on progressBar
                            currentForm.progressBar_CopyStatus.PerformStep();

                            // Log details about process
                            currentForm.logToFormAsync(string.Format(Languages.Games.message_compressStatus, gameFiles.IndexOf(currentFile), gameFiles.Count, newFileName));
                        }

                        // Add .ACF file to archive
                        await Task.Run(() => gameBackup.CreateEntryFromFile(Game.acfPath, Game.acfName, CompressionLevel.Optimal));
                    }
                }
                catch (Exception ex)
                {
                    currentForm.logToFormAsync(ex.ToString());

                    return false;
                }

                return true;
            }

            public async Task<bool> decompressArchive(Forms.moveGame currentForm, string currentZipNameNpath, Definitions.List.Game Game, Definitions.List.Library targetLibrary)
            {
                try
                {
                    string newCommonPath = Path.Combine(targetLibrary.commonPath, Game.installationPath);
                    // If directory exists at target game path
                    if (Directory.Exists(newCommonPath))
                    {
                        // Remove the directory
                        await Task.Run(() => Directory.Delete(newCommonPath, true));

                        if (File.Exists(Path.Combine(targetLibrary.steamAppsPath, Game.acfName)))
                            await Task.Run(() => File.Delete(Path.Combine(targetLibrary.steamAppsPath, Game.acfName)));
                    }

                    await Task.Run(() => ZipFile.ExtractToDirectory(currentZipNameNpath, targetLibrary.steamAppsPath));
                }
                catch (Exception ex)
                {
                    currentForm.logToFormAsync(ex.ToString());

                    return false;
                }

                return true;
            }

            */
        }

        // Get directory size from path, with or without sub directories
        public static long GetDirectorySize(string directoryPath, bool includeSub)
        {
            try
            {
                // Define a "long" for directory size
                long directorySize = 0;

                Parallel.ForEach(new DirectoryInfo(directoryPath).GetFileSystemInfos("*", SearchOption.AllDirectories), currentFile =>
                {
                    Interlocked.Add(ref directorySize, (currentFile as FileInfo).Length);
                });

                // and return directory size
                return directorySize;
            }
            // on error, return 0
            catch { return 0; }
        }

        public static long getFileSize(string filePath) => new FileInfo(filePath).Length;

        public static byte[] GetFileMD5(string filePath)
        {
            // Create a new md5 function and using it
            using (var MD5 = System.Security.Cryptography.MD5.Create())
            {
                // Compute md5 hash of given file and return the hash value
                return MD5.ComputeHash(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            }
        }

        // Source: http://stackoverflow.com/a/2082893
        public static string FormatBytes(long bytes)
        {
            // definition of file size suffixes
            string[] Suffix = { "B", "KB", "MB", "GB", "TB" };
            int current;
            double dblSByte = bytes;

            for (current = 0; current < Suffix.Length && bytes >= 1024; current++, bytes /= 1024)
            {
                dblSByte = bytes / 1024.0;
            }

            // Format the string
            return string.Format("{0:0.##} {1}", dblSByte, Suffix[current]);
        }

        public static long getAvailableFreeSpace(string TargetFolder)
        {
            try
            {
                // Define a drive info
                DriveInfo Disk = new DriveInfo(Path.GetPathRoot(TargetFolder));

                // And return available free space from defined drive info
                return Disk.AvailableFreeSpace;
            }
            catch { return 0; }
        }

        public static long getUsedSpace(string TargetFolder)
        {
            try
            {
                // Define a drive info
                DriveInfo Disk = new DriveInfo(Path.GetPathRoot(TargetFolder));

                // And return available free space from defined drive info
                return Disk.TotalSize;
            }
            catch { return 0; }
        }

        public async static void deleteOldLibrary(Definitions.List.Library Library)
        {
            try
            {
                await Task.Run(() =>
                {
                    if (Directory.Exists(Library.steamAppsPath))
                        Directory.Delete(Library.steamAppsPath, true);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
