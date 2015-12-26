using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Steam_Library_Manager.Functions
{
    class Games
    {
        public async Task<List<string>> getFileList(Definitions.List.GamesList Game, bool includeDownloads = true, bool includeWorkshop = true)
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

        async Task<IEnumerable<string>> getCommonFiles(Definitions.List.GamesList Game) => await Task.Run(() => Directory.EnumerateFiles(Game.commonPath, "*", SearchOption.AllDirectories).ToList());

        async Task<IEnumerable<string>> getDownloadFiles(Definitions.List.GamesList Game) => await Task.Run(() => Directory.EnumerateFiles(Game.downloadPath, "*", SearchOption.AllDirectories).ToList());

        async Task<IEnumerable<string>> getPatchFiles(Definitions.List.GamesList Game) => await Task.Run(() => Directory.EnumerateFiles(Game.Library.downloadPath, $"*{Game.appID}*.patch", SearchOption.TopDirectoryOnly).ToList());

        async Task<IEnumerable<string>> getWorkshopFiles(Definitions.List.GamesList Game) => await Task.Run(() => Directory.EnumerateFiles(Game.workShopPath, "*", SearchOption.AllDirectories).ToList());

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
                currentForm.Log(ex.ToString());

                return false;
            }

            return true;
        }

        public async Task<bool> decompressArchive(Forms.moveGame currentForm, string newCommonPath, string currentZipNameNpath, Definitions.List.GamesList Game, Definitions.List.LibraryList targetLibrary)
        {
            try
            {
                // If directory exists at target game path
                if (Directory.Exists(newCommonPath))
                {
                    // Remove the directory
                    await Task.Run(() => Directory.Delete(newCommonPath, true));

                    await Task.Run(() => File.Delete(Path.Combine(targetLibrary.steamAppsPath, Game.acfName)));
                }

                await Task.Run(() => ZipFile.ExtractToDirectory(currentZipNameNpath, targetLibrary.steamAppsPath));
            }
            catch (Exception ex)
            {
                currentForm.Log(ex.ToString());

                return false;
            }

            return true;
        }

        public async Task<bool> copyGameFiles(Forms.moveGame currentForm, List<string> gameFiles, string newCommonPath, Definitions.List.GamesList Game, Definitions.List.LibraryList targetLibrary, bool Validate)
        {
            string newFileName;
            try
            {
                foreach (string currentFile in gameFiles)
                {
                    using (FileStream currentFileStream = File.OpenRead(currentFile))
                    {
                        newFileName = currentFile.Replace(Game.Library.steamAppsPath, targetLibrary.steamAppsPath);

                        if (!Directory.Exists(Path.GetDirectoryName(newFileName)))
                            Directory.CreateDirectory(Path.GetDirectoryName(newFileName));

                        // Create a new file
                        using (FileStream newFileStream = File.Create(newFileName))
                        {
                            // Copy the file to target library asynchronously
                            await currentFileStream.CopyToAsync(newFileStream);

                            // Perform step
                            currentForm.progressBar_CopyStatus.PerformStep();

                            // Log to user
                            currentForm.Log(string.Format("[{0}/{1}] Copied: {2}", gameFiles.IndexOf(currentFile) + 1, gameFiles.Count, newFileName));
                        }

                        if (Validate)
                        {
                            // Compare the hashes, if any of them not equals
                            if (BitConverter.ToString(FileSystem.GetFileMD5(currentFile)) != BitConverter.ToString(FileSystem.GetFileMD5(newFileName)))
                            {
                                // Log it
                                currentForm.Log(string.Format("[{0}/{1}] File couldn't verified: {2}", gameFiles.IndexOf(currentFile) + 1, gameFiles.Count, newFileName));

                                // and cancel the process
                                return false;
                            }
                        }
                    }

                    // Copy .ACF file
                    await Task.Run(() => File.Copy(Game.acfPath, Path.Combine(targetLibrary.steamAppsPath, Game.acfName), true));

                    if (File.Exists(Game.workShopAcfName))
                        await Task.Run(() => File.Copy(Game.workShopAcfName, Game.workShopAcfName.Replace(Game.Library.steamAppsPath, targetLibrary.steamAppsPath), true));
                }
            }
            catch (Exception ex)
            {
                currentForm.Log(ex.ToString());

                return false;
            }

            return true;
        }

        public async Task<bool> compressGameFiles(Forms.moveGame currentForm, List<string> gameFiles, string newZipNameNpath, Definitions.List.GamesList Game, Definitions.List.LibraryList targetLibrary)
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
                        currentForm.Log(string.Format("[{0}/{1}] Compressed: {2}", gameFiles.IndexOf(currentFile), gameFiles.Count, newFileName));
                    }

                    // Add .ACF file to archive
                    await Task.Run(() => gameBackup.CreateEntryFromFile(Game.acfPath, Game.acfName, CompressionLevel.Optimal));
                }
            }
            catch (Exception ex)
            {
                currentForm.Log(ex.ToString());

                return false;
            }

            return true;
        }

        public async Task<bool> deleteGameFiles(Definitions.List.GamesList Game, List<string> gameFiles = null)
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

        public static int GetGameCountFromLibrary(Definitions.List.LibraryList Library)
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

        public static void AddNewGame(string acfPath, int appID, string appName, string installationPath, Definitions.List.LibraryList Library, long sizeOnDisk, bool isCompressed)
        {
            try
            {
                // Make a new definition for game
                Definitions.List.GamesList Game = new Definitions.List.GamesList();

                // Set game appID
                Game.appID = appID;

                // Define it is an archive
                Game.Compressed = isCompressed;

                // Set game name
                Game.appName = appName;

                // Set acf name, appmanifest_107410.acf as example
                Game.acfName = string.Format("appmanifest_{0}.acf", appID);

                // Set game acf path
                Game.acfPath = acfPath;

                // Set workshop acf name
                Game.workShopAcfName = string.Format("appworkshop_{0}.acf", appID);

                if (!string.IsNullOrEmpty(Library.workshopPath))
                    // Set path for acf file
                    Game.workShopAcfPath = Path.Combine(Library.workshopPath, Game.workShopAcfName);

                // Set installation path
                Game.installationPath = installationPath;

                // Set game library
                Game.Library = Library;

                // If game has a folder in "common" dir, define it as exactInstallPath
                if (Directory.Exists(Path.Combine(Library.commonPath, installationPath)))
                    Game.commonPath = Path.Combine(Library.commonPath, installationPath) + Path.DirectorySeparatorChar.ToString();

                // If game has a folder in "downloading" dir, define it as downloadPath
                if (Directory.Exists(Path.Combine(Library.downloadPath, installationPath)))
                    Game.downloadPath = Path.Combine(Library.downloadPath, installationPath) + Path.DirectorySeparatorChar.ToString();

                // If game has a folder in "workshop" dir, define it as workShopPath
                if (Directory.Exists(Path.Combine(Library.workshopPath, "content", appID.ToString())))
                    Game.workShopPath = Path.Combine(Library.workshopPath, "content", appID.ToString()) + Path.DirectorySeparatorChar.ToString();

                // If game do not have a folder in "common" directory and "downloading" directory then skip this game
                if (string.IsNullOrEmpty(Game.commonPath) && string.IsNullOrEmpty(Game.downloadPath) && !Game.Compressed)
                    return; // Do not add pre-loads to list

                // If SizeOnDisk value from .ACF file is not equals to 0
                if (sizeOnDisk != 0 && Properties.Settings.Default.GameSizeCalculationMethod != "ACF" && !isCompressed)
                {
                    // If game has "common" folder
                    if (!string.IsNullOrEmpty(Game.commonPath))
                    {
                        // Calculate game size on disk
                        Game.sizeOnDisk += FileSystem.GetDirectorySize(Game.commonPath, true);
                    }

                    // If game has downloading files
                    if (!string.IsNullOrEmpty(Game.downloadPath))
                    {
                        // Calculate "downloading" folder size
                        Game.sizeOnDisk += FileSystem.GetDirectorySize(Game.downloadPath, true);
                    }

                    // If game has "workshop" files
                    if (!string.IsNullOrEmpty(Game.workShopPath))
                    {
                        // Calculate "workshop" files size
                        Game.sizeOnDisk += FileSystem.GetDirectorySize(Game.workShopPath, true);
                    }
                }
                else if (sizeOnDisk != 0 && isCompressed)
                {
                    // If user want us to get archive size from real uncompressed size
                    if (Properties.Settings.Default.ArchiveSizeCalculationMethod.StartsWith("Uncompressed"))
                    {
                        // Open archive to read
                        using (ZipArchive zip = ZipFile.OpenRead(Path.Combine(Game.Library.steamAppsPath, Game.appID + ".zip")))
                        {
                            // For each file in archive
                            foreach (ZipArchiveEntry entry in zip.Entries)
                            {
                                // Add file size to sizeOnDisk
                                Game.sizeOnDisk += entry.Length;
                            }
                        }
                    }
                    else
                    {
                        // Use FileInfo to get our archive details
                        FileInfo zip = new FileInfo(Path.Combine(Game.Library.steamAppsPath, Game.appID + ".zip"));

                        // And set archive size as game size
                        Game.sizeOnDisk = zip.Length;
                    }
                }
                else
                    // Else set game size to size in acf
                    Game.sizeOnDisk = sizeOnDisk;

                // Add our game details to global list
                Definitions.List.Game.Add(Game);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public static void UpdateGameList(Definitions.List.LibraryList Library)
        {
            try
            {
                // If our list is not empty
                if (Definitions.List.Game.Count != 0)
                {
                    if (Library == null)
                        Definitions.List.Game.Clear();
                    else
                        Definitions.List.Game.RemoveAll(x => x.Library == Library);
                }

                if (!Directory.Exists(Library.steamAppsPath))
                    return;

                // Foreach *.acf file found in library
                foreach (string game in Directory.EnumerateFiles(Library.steamAppsPath, "*.acf", SearchOption.TopDirectoryOnly))
                {
                    // Define a new value and call KeyValue
                    Framework.KeyValue Key = new Framework.KeyValue();

                    // Read the *.acf file as text
                    Key.ReadFileAsText(game);

                    // If key doesn't contains a child (value in acf file)
                    if (Key.Children.Count == 0)
                        // Skip this file (game)
                        continue;

                    AddNewGame(game, Convert.ToInt32(Key["appID"].Value), !string.IsNullOrEmpty(Key["name"].Value) ? Key["name"].Value : Key["UserConfig"]["name"].Value, Key["installdir"].Value, Library, Convert.ToInt64(Key["SizeOnDisk"].Value), false);
                }

                // If library is backup library
                if (Library.Backup)
                {
                    // Do a loop for each *.zip file in library
                    foreach (string gameArchive in Directory.EnumerateFiles(Library.steamAppsPath, "*.zip", SearchOption.TopDirectoryOnly))
                    {
                        // Open archive for read
                        using (ZipArchive compressedArchive = ZipFile.OpenRead(gameArchive))
                        {
                            // For each file in opened archive
                            foreach (ZipArchiveEntry file in compressedArchive.Entries.Where(x => x.Name.Contains(".acf")))
                            {
                                // If it contains
                                // Define a KeyValue reader
                                Framework.KeyValue Key = new Framework.KeyValue();

                                // Open .acf file from archive as text
                                Key.ReadAsText(file.Open());

                                // If acf file has no children, skip this archive
                                if (Key.Children.Count == 0)
                                    return;

                                AddNewGame(file.FullName, Convert.ToInt32(Key["appID"].Value), !string.IsNullOrEmpty(Key["name"].Value) ? Key["name"].Value : Key["UserConfig"]["name"].Value, Key["installdir"].Value, Library, Convert.ToInt64(Key["SizeOnDisk"].Value), true);

                                // we found what we are looking for, return
                                return;
                            }
                        }
                    }
                }

                if (Definitions.SLM.LatestSelectedLibrary == Library)
                    UpdateMainForm(null, null, Library);
            }
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log
                    Log.ErrorsToFile("UpdateGameList", ex.ToString());

                // Show a messagebox to user
                MessageBox.Show("An error happened while updating game list!\n\n\n" + ex.ToString());
            }
        }

        public static void UpdateMainForm(Func<Definitions.List.GamesList, object> Sort, string Search, Definitions.List.LibraryList Library)
        {
            try
            {
                // If our panel for game list not empty
                if (Definitions.Accessors.MainForm.panel_GameList.Controls.Count != 0)
                    // Then clean panel
                    Definitions.Accessors.MainForm.panel_GameList.Controls.Clear();

                // Define our sorting method
                switch (Properties.Settings.Default.SortGamesBy)
                {
                    default:
                    case "appName":
                        Sort = x => x.appName;
                        break;
                    case "appID":
                        Sort = x => x.appID;
                        break;
                    case "sizeOnDisk":
                        Sort = x => x.sizeOnDisk;
                        break;
                }

                // Do a loop for each game in library
                foreach (Definitions.List.GamesList Game in ((string.IsNullOrEmpty(Search)) ? Definitions.List.Game.Where(x => x.Library == Library).OrderBy(Sort) : Definitions.List.Game.Where(x => x.Library == Library).Where(
                    y => y.appName.ToLowerInvariant().Contains(Search.ToLowerInvariant()) // Search by appName
                    || y.appID.ToString().Contains(Search) // Search by app ID
                    ).OrderBy(Sort)
                    ))
                {
                    // Define a new pictureBox for game
                    Framework.PictureBoxWithCaching gameDetailBox = new Framework.PictureBoxWithCaching();

                    // Set picture mode of pictureBox
                    gameDetailBox.SizeMode = PictureBoxSizeMode.StretchImage;

                    // Set game image size
                    gameDetailBox.Size = Properties.Settings.Default.GamePictureBoxSize;

                    // Load game header image asynchronously
                    gameDetailBox.LoadAsync(string.Format("https://steamcdn-a.akamaihd.net/steam/apps/{0}/header.jpg", Game.appID));

                    // Set error image in case of couldn't load game header image
                    gameDetailBox.ErrorImage = Properties.Resources.no_image_available;

                    // Space between pictureBoxes for better looking
                    gameDetailBox.Margin = new Padding(20);

                    // Set our game details as Tag to pictureBox
                    gameDetailBox.Tag = Game;

                    // On we click to pictureBox (drag & drop event)
                    gameDetailBox.MouseDown += gameDetailBox_MouseDown;

                    // If game is compressed
                    if (Game.Compressed)
                    {
                        // Make a new picturebox
                        PictureBox compressedIcon = new PictureBox();

                        // Set picture box image to compressedLibraryIcon
                        compressedIcon.Image = Properties.Resources.compressedLibraryIcon;

                        // Put picturebox to right corner of game image
                        compressedIcon.Left = Properties.Settings.Default.GamePictureBoxSize.Width - 20;
                        compressedIcon.Top = 5;

                        // Add icon to game picture
                        gameDetailBox.Controls.Add(compressedIcon);
                    }

                    // Set our context menu to pictureBox
                    gameDetailBox.ContextMenuStrip = Content.Games.generateRightClickMenu(Game);

                    // Add our new game pictureBox to panel
                    Definitions.Accessors.MainForm.panel_GameList.Controls.Add(gameDetailBox);
                }
            }
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log errors to DirectoryRemoval.txt
                    Log.ErrorsToFile("Games", ex.ToString());
            }
        }

        static void gameDetailBox_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                // If clicked button is left (so it will not conflict with context menu)
                if (e.Button == MouseButtons.Left)
                {
                    // Define our picturebox from sender
                    PictureBox img = sender as PictureBox;

                    // Do drag & drop with our pictureBox
                    img.DoDragDrop(img, DragDropEffects.Move);
                }
            }
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log errors to DirectoryRemoval.txt
                    Log.ErrorsToFile("Games", ex.ToString());
            }
        }

    }
}
