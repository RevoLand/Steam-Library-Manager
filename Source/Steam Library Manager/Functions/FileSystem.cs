using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Steam_Library_Manager.Functions
{
    class FileSystem
    {
        public class Game
        {
            public static int GetGameCountFromLibrary(Definitions.List.Library Library)
            {
                try
                {
                    // Define an int for total game count
                    int gameCount = 0;

                    // Get *.acf file count from library path
                    gameCount += Directory.GetFiles(Library.steamAppsPath, "*.acf", SearchOption.TopDirectoryOnly).Length;

                    // If library is a backup library
                    if (Library.Backup)
                        // Also get *.zip file count from backup library path
                        gameCount += Directory.GetFiles(Library.steamAppsPath, "*.zip", SearchOption.TopDirectoryOnly).Length;

                    // return total game count we have found
                    return gameCount;
                }
                catch { return 0; }
            }

            public async Task<int> copyGameFiles(Forms.moveGame currentForm, List<string> gameFiles, Definitions.List.Game Game, Definitions.List.Library targetLibrary, bool Validate, CancellationToken token)
            {
                List<string> movedFiles = new List<string>();
                try
                {
                    foreach (string currentFile in gameFiles)
                    {
                        if (token.IsCancellationRequested)
                        {
                            DialogResult askUserToRemoveMovedFiles = MessageBox.Show(Languages.Games.message_canceledProcess, Languages.Games.messageTitle_canceledProcess, MessageBoxButtons.YesNo);

                            if (askUserToRemoveMovedFiles == DialogResult.Yes)
                            {
                                FileSystem.Game.removeGivenFiles(movedFiles, Game, targetLibrary);
                                currentForm.logToFormAsync(Languages.Games.message_filesRemoved);
                            }

                            return -1;
                        }

                        string newFileName = currentFile.Replace(Game.Library.steamAppsPath, targetLibrary.steamAppsPath);

                        if (!Directory.Exists(Path.GetDirectoryName(newFileName)))
                            Directory.CreateDirectory(Path.GetDirectoryName(newFileName));

                        // Copy the file to target library asynchronously
                        await Task.Run(() => File.Copy(currentFile, newFileName, true));

                        // Perform step on progress bar
                        currentForm.progressBar_CopyStatus.PerformStep();

                        // Log to textbox
                        currentForm.logToFormAsync(string.Format(Languages.Games.message_processStatus, gameFiles.IndexOf(currentFile) + 1, gameFiles.Count, newFileName));

                        // add moved file path to list 
                        movedFiles.Add(newFileName);

                        if (Validate)
                        {
                            // Compare the hashes, if any of them not equals
                            if (BitConverter.ToString(GetFileMD5(currentFile)) != BitConverter.ToString(GetFileMD5(newFileName)))
                            {
                                // Log it
                                currentForm.logToFormAsync(string.Format(Languages.Games.messageError_fileNotVerified, gameFiles.IndexOf(currentFile) + 1, gameFiles.Count, newFileName));

                                // and cancel the process
                                return 0;
                            }
                        }
                    }

                    // Copy .ACF file
                    await Task.Run(() => File.Copy(Game.acfPath, Path.Combine(targetLibrary.steamAppsPath, Game.acfName), true));

                    if (File.Exists(Game.workShopAcfName))
                        await Task.Run(() => File.Copy(Game.workShopAcfName, Game.workShopAcfName.Replace(Game.Library.steamAppsPath, targetLibrary.steamAppsPath), true));
                }
                catch (Exception ex)
                {
                    currentForm.logToFormAsync(ex.ToString());

                    return 0;
                }

                return 1;
            }

            public async Task<int> copyGameFilesNew(Forms.moveGame currentForm, List<string> gameFiles, Definitions.List.Game Game, Definitions.List.Library targetLibrary, bool Validate, CancellationToken token)
            {
                List<string> movedFiles = new List<string>();
                try
                {
                    long totalSize = 0, movenSize = 0;
                    int currentBlockSize = 0;
                    byte[] buffer = new byte[1024 * 1024];

                    foreach (string file in gameFiles)
                        totalSize += FileSystem.getFileSize(file);

                    foreach (string currentFile in gameFiles)
                    {
                        if (token.IsCancellationRequested)
                        {
                            DialogResult askUserToRemoveMovedFiles = MessageBox.Show(Languages.Games.message_canceledProcess, Languages.Games.messageTitle_canceledProcess, MessageBoxButtons.YesNo);

                            if (askUserToRemoveMovedFiles == DialogResult.Yes)
                            {
                                FileSystem.Game.removeGivenFiles(movedFiles, Game, targetLibrary);
                                currentForm.logToFormAsync(Languages.Games.message_filesRemoved);
                            }

                            return -1;
                        }

                        using (FileStream currentFileContent = File.Open(currentFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            string newFileName = currentFile.Replace(Game.Library.steamAppsPath, targetLibrary.steamAppsPath);

                            if (File.Exists(newFileName))
                                File.Delete(newFileName);

                            if (!Directory.Exists(Path.GetDirectoryName(newFileName)))
                                Directory.CreateDirectory(Path.GetDirectoryName(newFileName));

                            using (FileStream newFileContent = new FileStream(newFileName, FileMode.Create, FileAccess.Write))
                            {
                                while ((currentBlockSize = await currentFileContent.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                {
                                    await newFileContent.WriteAsync(buffer, 0, currentBlockSize);

                                    movenSize += currentBlockSize;
                                    currentForm.label_movedFileSize.Text = string.Format(Languages.Games.label_movedFileSize, FormatBytes(movenSize), FormatBytes(totalSize), FormatBytes(totalSize - movenSize));
                                }
                                
                                // Perform step on progress bar
                                currentForm.progressBar_CopyStatus.PerformStep();

                                // Log to textbox
                                currentForm.logToFormAsync(string.Format(Languages.Games.message_processStatus, gameFiles.IndexOf(currentFile) + 1, gameFiles.Count, newFileName));

                                // add moved file path to list 
                                movedFiles.Add(newFileName);

                                if (Validate)
                                {
                                    // Compare the hashes, if any of them not equals
                                    if (BitConverter.ToString(GetFileMD5(currentFile)) != BitConverter.ToString(GetFileMD5(newFileName)))
                                    {
                                        // Log it
                                        currentForm.logToFormAsync(string.Format(Languages.Games.messageError_fileNotVerified, gameFiles.IndexOf(currentFile) + 1, gameFiles.Count, newFileName));

                                        // and cancel the process
                                        return 0;
                                    }
                                }
                            } // using, new file stream
                        } // using, current file stream
                    }//forEach ends

                    // Copy .ACF file
                    await Task.Run(() => File.Copy(Game.acfPath, Path.Combine(targetLibrary.steamAppsPath, Game.acfName), true));

                    if (File.Exists(Game.workShopAcfName))
                        await Task.Run(() => File.Copy(Game.workShopAcfName, Game.workShopAcfName.Replace(Game.Library.steamAppsPath, targetLibrary.steamAppsPath), true));
                }
                catch (Exception ex)
                {
                    currentForm.logToFormAsync(ex.ToString());

                    return 0;
                }

                return 1;
            }

            public async Task<bool> deleteGameFiles(Definitions.List.Game Game, List<string> gameFiles = null)
            {
                try
                {
                    if (Game.Compressed)
                    {
                        string currentZipNameNpath = Path.Combine(Game.Library.steamAppsPath, $"{Game.appID}.zip");

                        if (File.Exists(currentZipNameNpath))
                            await Task.Run(() => File.Delete(currentZipNameNpath));
                    }
                    else
                    {
                        if (gameFiles == null || gameFiles.Count == 0)
                            gameFiles = await getFileList(Game);

                        foreach (string currentFile in gameFiles)
                        {
                            if (File.Exists(currentFile))
                                await Task.Run(() => File.Delete(currentFile));
                        }

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
                            await Task.Run(() => File.Delete(Game.acfPath));

                        // workshop .acf file
                        if (File.Exists(Game.workShopAcfPath))
                            await Task.Run(() => File.Delete(Game.workShopAcfPath));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());

                    return false;
                }

                return true;
            }

            public static async void removeGivenFiles(List<string> fileList, Definitions.List.Game Game = null, Definitions.List.Library targetLibrary = null, bool removeAcfFile = true)
            {
                try
                {
                    foreach (string currentFile in fileList)
                    {
                        if (File.Exists(currentFile))
                            await Task.Run(() => File.Delete(currentFile));
                    }

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

            public async Task<List<string>> getFileList(Definitions.List.Game Game, bool includeDownloads = true, bool includeWorkshop = true)
            {
                List<string> FileList = new List<string>();

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

            async Task<IEnumerable<string>> getCommonFiles(Definitions.List.Game Game) => await Task.Run(() => Directory.EnumerateFiles(Game.commonPath, "*", SearchOption.AllDirectories).ToList());

            async Task<IEnumerable<string>> getDownloadFiles(Definitions.List.Game Game) => await Task.Run(() => Directory.EnumerateFiles(Game.downloadPath, "*", SearchOption.AllDirectories).ToList());

            async Task<IEnumerable<string>> getPatchFiles(Definitions.List.Game Game) => await Task.Run(() => Directory.EnumerateFiles(Game.Library.downloadPath, $"*{Game.appID}*.patch", SearchOption.TopDirectoryOnly).ToList());

            async Task<IEnumerable<string>> getWorkshopFiles(Definitions.List.Game Game) => await Task.Run(() => Directory.EnumerateFiles(Game.workShopPath, "*", SearchOption.AllDirectories).ToList());

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
        }

        // Get directory size from path, with or without sub directories
        public static long GetDirectorySize(string directoryPath, bool includeSub)
        {
            try
            {
                // Define a "long" for directory size
                long directorySize = 0;

                // For each file in the given directory
                foreach (string currentFile in Directory.EnumerateFiles(directoryPath, "*", (includeSub) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                {
                    FileInfo file = new FileInfo(currentFile);
                    // add current file size to directory size
                    directorySize += file.Length;
                }

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
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
        }
    }
}
