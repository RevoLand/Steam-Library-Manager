using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Steam_Library_Manager.Functions
{
    class Games
    {

        public static async void AddNewGame(string acfPath, int appID, string appName, string installationPath, Definitions.List.Library Library, long sizeOnDisk, bool isCompressed)
        {
            try
            {
                // Make a new definition for game
                Definitions.List.Game Game = new Definitions.List.Game();

                // Set game appID
                Game.appID = appID;

                // Define it is an archive
                Game.Compressed = isCompressed;

                // Set game name
                Game.appName = appName;

                Game.gameHeaderImage = $"http://cdn.akamai.steamstatic.com/steam/apps/{appID}/header.jpg";

                // Set acf name, appmanifest_107410.acf as example
                Game.acfName = $"appmanifest_{appID}.acf";

                // Set game acf path
                Game.acfPath = acfPath;

                // Set workshop acf name
                Game.workShopAcfName = $"appworkshop_{appID}.acf";

                if (!string.IsNullOrEmpty(Library.workshopPath))
                    // Set path for acf file
                    Game.workShopAcfPath = Path.Combine(Library.workshopPath, Game.workShopAcfName);

                // Set installation path
                Game.installationPath = installationPath;

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

                fileSystem.Game gameFunctions = new fileSystem.Game();

                // If SizeOnDisk value from .ACF file is not equals to 0
                if (Properties.Settings.Default.gameSizeCalculationMethod != "ACF" && !isCompressed)
                {
                    List<FileSystemInfo> gameFiles = await gameFunctions.getFileList(Game);

                    Parallel.ForEach(gameFiles, file =>
                    {
                        Game.sizeOnDisk += (file as FileInfo).Length;
                    });
                }
                else if (isCompressed)
                {
                    // If user want us to get archive size from real uncompressed size
                    if (Properties.Settings.Default.archiveSizeCalculationMethod.StartsWith("Uncompressed"))
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
                        Game.sizeOnDisk = fileSystem.getFileSize(Path.Combine(Game.Library.steamAppsPath, Game.appID + ".zip"));
                    }
                }
                else
                    // Else set game size to size in acf
                    Game.sizeOnDisk = sizeOnDisk;

                Game.prettyGameSize = fileSystem.FormatBytes(Game.sizeOnDisk);

                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    Game.contextMenu = Content.Games.generateRightClickMenu(Game);
                });
                // Add our game details to global list
                Library.Games.Add(Game);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public static void UpdateGameList(Definitions.List.Library Library)
        {
            try
            {
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
                                    continue;

                                AddNewGame(file.FullName, Convert.ToInt32(Key["appID"].Value), !string.IsNullOrEmpty(Key["name"].Value) ? Key["name"].Value : Key["UserConfig"]["name"].Value, Key["installdir"].Value, Library, Convert.ToInt64(Key["SizeOnDisk"].Value), true);

                                // we found what we are looking for, continue the loop
                                continue;
                            }

                            compressedArchive.Dispose();
                        }
                    }
                }

                if (Definitions.SLM.selectedLibrary == Library)
                    UpdateMainForm(null, null, Library);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public static void UpdateMainForm(Func<Definitions.List.Game, object> Sort, string Search, Definitions.List.Library Library)
        {
            try
            {
                Sort = SLM.Settings.getSortingMethod();

                MainWindow.Accessor.gamePanel.ItemsSource = ((string.IsNullOrEmpty(Search)) ? Library.Games.Where(x => x.Library == Library).OrderBy(Sort) : Library.Games.Where(x => x.Library == Library).Where(
                    y => y.appName.ToLowerInvariant().Contains(Search.ToLowerInvariant()) // Search by appName
                    || y.appID.ToString().Contains(Search) // Search by app ID
                    ).OrderBy(Sort)
                    );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

    }
}
