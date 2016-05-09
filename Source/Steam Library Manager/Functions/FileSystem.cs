using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
            public void copyGameFiles(Forms.MoveGameForm currentForm, List<FileSystemInfo> gameFiles, Definitions.Game Game, Definitions.Library targetLibrary, CancellationTokenSource cancellationToken, bool compressGame = false)
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

                    if (!Game.IsCompressed && compressGame)
                    {
                        FileInfo compressedArchive = new FileInfo(Game.compressedName.FullName.Replace(Game.installedLibrary.steamAppsPath.FullName, targetLibrary.steamAppsPath.FullName));

                        if (compressedArchive.Exists)
                            compressedArchive.Delete();

                        using (ZipArchive compressed = ZipFile.Open(compressedArchive.FullName, ZipArchiveMode.Create))
                        {
                            foreach(FileSystemInfo currentFile in gameFiles)
                            {
                                fileIndex++;

                                string newFileName = currentFile.FullName.Substring(Game.installedLibrary.steamAppsPath.FullName.Length + 1);

                                compressed.CreateEntryFromFile(currentFile.FullName, newFileName, CompressionLevel.Optimal);

                                currentForm.formLogs.Add(string.Format("[{0}/{1}] {2}\n", fileIndex, gameFiles.Count, newFileName));
                            }

                            compressed.CreateEntryFromFile(Game.fullAcfPath.FullName, Game.acfName);
                        }
                    }
                    else if (Game.IsCompressed && !compressGame)
                    {
                        foreach(ZipArchiveEntry currentFile in ZipFile.OpenRead(Game.compressedName.FullName).Entries)
                        {
                            fileIndex++;

                            FileInfo fileName = new FileInfo(Path.Combine(targetLibrary.steamAppsPath.FullName, currentFile.FullName));
                            if (!fileName.Directory.Exists)
                                fileName.Directory.Create();

                            currentFile.ExtractToFile(fileName.FullName, true);

                            currentForm.formLogs.Add(string.Format("[{0}/{1}] {2}\n", fileIndex, gameFiles.Count, fileName.FullName));
                        }
                    }
                    else
                    {
                        Parallel.ForEach(gameFiles.Where(x => (x as FileInfo).Length <= Properties.Settings.Default.ParallelAfterSize), options, currentFile =>
                        {
                            FileInfo newFile = new FileInfo(currentFile.FullName.Replace(Game.installedLibrary.steamAppsPath.FullName, targetLibrary.steamAppsPath.FullName));

                            if (!newFile.Exists || (newFile.Length != (currentFile as FileInfo).Length || newFile.LastWriteTime != (currentFile as FileInfo).LastWriteTime))
                            {
                                if (!newFile.Directory.Exists)
                                    newFile.Directory.Create();

                                (currentFile as FileInfo).CopyTo(newFile.FullName, true);
                            }

                            Interlocked.Increment(ref fileIndex);
                            Interlocked.Add(ref movenSize, (currentFile as FileInfo).Length);
                            movedFiles.Add(newFile.FullName);

                            currentForm.formLogs.Add(string.Format("[{0}/{1}] {2}\n", fileIndex, gameFiles.Count, newFile.FullName));

                            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate
                            {
                                currentForm.progressReportLabel.Content = $"{fileSystem.FormatBytes(totalSize - movenSize)} left - {FormatBytes(movenSize)} / {FormatBytes(totalSize)}";
                                currentForm.progressReport.Value = ((int)Math.Round((double)(100 * movenSize) / totalSize));
                            });
                        });

                        options.MaxDegreeOfParallelism = 1;

                        Parallel.ForEach(gameFiles.Where(x => (x as FileInfo).Length > Properties.Settings.Default.ParallelAfterSize), options, currentFile =>
                        {
                            FileInfo newFile = new FileInfo(currentFile.FullName.Replace(Game.installedLibrary.steamAppsPath.FullName, targetLibrary.steamAppsPath.FullName));

                            if (!newFile.Exists || (newFile.Length != (currentFile as FileInfo).Length || newFile.LastWriteTime != (currentFile as FileInfo).LastWriteTime))
                            {
                                if (!newFile.Directory.Exists)
                                    newFile.Directory.Create();

                                (currentFile as FileInfo).CopyTo(newFile.FullName, true);
                            }

                            Interlocked.Increment(ref fileIndex);
                            Interlocked.Add(ref movenSize, (currentFile as FileInfo).Length);
                            movedFiles.Add(newFile.FullName);

                            currentForm.formLogs.Add(string.Format("[{0}/{1}] {2}\n", fileIndex, gameFiles.Count, newFile.FullName));

                            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate
                            {
                                currentForm.progressReportLabel.Content = $"{fileSystem.FormatBytes(totalSize - movenSize)} left - {FormatBytes(movenSize)} / {FormatBytes(totalSize)}";
                                currentForm.progressReport.Value = ((int)Math.Round((double)(100 * movenSize) / totalSize));
                            });
                        });

                        if (!Game.IsCompressed)
                        {
                            // Copy .ACF file
                            if (Game.fullAcfPath.Exists)
                                Game.fullAcfPath.CopyTo(Path.Combine(targetLibrary.steamAppsPath.FullName, Game.acfName), true);

                            if (Game.workShopAcfPath.Exists)
                            {
                                FileInfo newACFPath = new FileInfo(Game.workShopAcfPath.FullName.Replace(Game.installedLibrary.steamAppsPath.FullName, targetLibrary.steamAppsPath.FullName));

                                if (!newACFPath.Directory.Exists)
                                    newACFPath.Directory.Create();

                                Game.workShopAcfPath.CopyTo(newACFPath.FullName, true);
                            }
                        }
                    }

                    sw.Stop();
                    currentForm.formLogs.Add($"Time elapsed: {sw.Elapsed}");
                    Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)delegate
                    {
                        currentForm.button.Content = "Close";
                    });
                }
                catch (OperationCanceledException)
                {
                    MessageBoxResult moveGamesBeforeDeletion = MessageBox.Show("Game movement cancelled. Would you like to remove files that already moven?", "Remove moven files?", MessageBoxButton.YesNo);

                    if (moveGamesBeforeDeletion == MessageBoxResult.Yes)
                        removeGivenFiles(movedFiles);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

            public static void removeGivenFiles(ConcurrentBag<string> fileList)
            {
                try
                {
                    Parallel.ForEach(fileList, currentFile =>
                    {
                        FileInfo file = new FileInfo(currentFile);

                        if (file.Exists)
                            file.Delete();
                    });
                }
                catch { }
            }
        }

        // Get directory size from path, with or without sub directories
        public static long GetDirectorySize(string directoryPath, bool includeSub)
        {
            try
            {
                // Define a "long" for directory size
                long directorySize = 0;

                foreach (FileInfo currentFile in new DirectoryInfo(directoryPath).GetFileSystemInfos("*", (includeSub) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                {
                    directorySize += currentFile.Length;
                }
                // and return directory size
                return directorySize;
            }
            // on error, return 0
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return 0;
            }
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

        public static void deleteOldLibrary(Definitions.Library Library)
        {
            try
            {
                if (Library.steamAppsPath.Exists)
                    Library.steamAppsPath.Delete(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
