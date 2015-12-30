using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Steam_Library_Manager.Functions
{
    class Games
    {
        public static async void AddNewGame(string acfPath, int appID, string appName, string installationPath, Definitions.List.LibraryList Library, long sizeOnDisk, bool isCompressed)
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

                FileSystem.Game gameFunctions = new FileSystem.Game();

                // If SizeOnDisk value from .ACF file is not equals to 0
                if (Properties.Settings.Default.GameSizeCalculationMethod != "ACF" && !isCompressed)
                {
                    List<string> gameFiles = await gameFunctions.getFileList(Game);

                    foreach (string file in gameFiles)
                    {
                        Game.sizeOnDisk += await Task.Run(() => new FileInfo(file).Length);
                    }
                }
                else if (isCompressed)
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
                        // And set archive size as game size
                        Game.sizeOnDisk = FileSystem.getFileSize(Path.Combine(Game.Library.steamAppsPath, Game.appID + ".zip"));
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

        public static async void UpdateGameList(Definitions.List.LibraryList Library)
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
                        continue;

                    await Task.Run(() => AddNewGame(game, Convert.ToInt32(Key["appID"].Value), !string.IsNullOrEmpty(Key["name"].Value) ? Key["name"].Value : Key["UserConfig"]["name"].Value, Key["installdir"].Value, Library, Convert.ToInt64(Key["SizeOnDisk"].Value), false));
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

                                await Task.Run(() => AddNewGame(file.FullName, Convert.ToInt32(Key["appID"].Value), !string.IsNullOrEmpty(Key["name"].Value) ? Key["name"].Value : Key["UserConfig"]["name"].Value, Key["installdir"].Value, Library, Convert.ToInt64(Key["SizeOnDisk"].Value), true));

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
                    Log.ErrorsToFile(Languages.Games.source_updateGameList, ex.ToString());

                // Show a messagebox to user
                MessageBox.Show(string.Format(Languages.Games.messageError_unknownErrorWhileUpdatingGames, ex, Environment.NewLine));
            }
        }

        public static async void UpdateMainForm(Func<Definitions.List.GamesList, object> Sort, string Search, Definitions.List.LibraryList Library)
        {
            try
            {
                Sort = Settings.getSortingMethod();

                // If our panel for game list not empty
                if (Definitions.Accessors.MainForm.panel_GameList.Controls.Count != 0)
                    // Then clean panel
                    Definitions.Accessors.MainForm.panel_GameList.Controls.Clear();

                // Do a loop for each game in library
                foreach (Definitions.List.GamesList Game in ((string.IsNullOrEmpty(Search)) ? Definitions.List.Game.Where(x => x.Library == Library).OrderBy(Sort) : Definitions.List.Game.Where(x => x.Library == Library).Where(
                    y => y.appName.ToLowerInvariant().Contains(Search.ToLowerInvariant()) // Search by appName
                    || y.appID.ToString().Contains(Search) // Search by app ID
                    ).OrderBy(Sort)
                    ))
                {

                    // Add our new game pictureBox to panel
                    Definitions.Accessors.MainForm.panel_GameList.Controls.Add(await Content.Games.generateGameBox(Game));
                }
            }
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log errors to DirectoryRemoval.txt
                    Log.ErrorsToFile(Languages.Games.source_Games, ex.ToString());
            }
        }

    }
}
