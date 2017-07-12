using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;

namespace Steam_Library_Manager.Functions
{
    class Games
    {
        public static void AddNewGame(int AppID, string AppName, string InstallationPath, Definitions.Library Library, long SizeOnDisk, long LastUpdated, bool IsCompressed, bool IsSteamBackup = false)
        {
            try
            {
                // Make a new definition for game
                Definitions.Game Game = new Definitions.Game()
                {
                    // Set game appID
                    AppID = AppID,

                    // Set game name
                    AppName = AppName,

                    InstalledLibrary = Library,
                    InstallationPath = new DirectoryInfo(InstallationPath),

                    // Define it is an archive
                    IsCompressed = IsCompressed,
                    IsSteamBackup = IsSteamBackup,
                    LastUpdated = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(LastUpdated)
            };



                // If game do not have a folder in "common" directory and "downloading" directory then skip this game
                if (!Game.CommonFolder.Exists && !Game.DownloadFolder.Exists && !Game.IsCompressed && !Game.IsSteamBackup)
                {
                    Definitions.List.JunkStuff.Add(new Definitions.List.JunkInfo
                    {
                        FileSystemInfo = new FileInfo(Game.FullAcfPath.FullName),
                        FolderSize = Game.FullAcfPath.Length,
                        Library = Library
                    });

                    Game = null;
                    return; // Do not add pre-loads to list
                }

                if (IsCompressed)
                {
                    // If user want us to get archive size from real uncompressed size
                    if (Properties.Settings.Default.archiveSizeCalculationMethod.StartsWith("Uncompressed"))
                    {
                        // Open archive to read
                        using (ZipArchive zip = ZipFile.OpenRead(Game.CompressedArchiveName.FullName))
                        {
                            // For each file in archive
                            foreach (ZipArchiveEntry entry in zip.Entries)
                            {
                                // Add file size to sizeOnDisk
                                Game.SizeOnDisk += entry.Length;
                            }
                        }
                    }
                    else
                    {
                        // And set archive size as game size
                        Game.SizeOnDisk = FileSystem.GetFileSize(Path.Combine(Game.InstalledLibrary.SteamAppsFolder.FullName, Game.AppID + ".zip"));
                    }
                }
                else
                {
                    // If SizeOnDisk value from .ACF file is not equals to 0
                    if (Properties.Settings.Default.gameSizeCalculationMethod != "ACF")
                    {
                        System.Collections.Generic.List<FileSystemInfo> gameFiles = Game.GetFileList();

                        System.Threading.Tasks.Parallel.ForEach(gameFiles, file =>
                        {
                            Game.SizeOnDisk += (file as FileInfo).Length;
                        });

                    }
                    else
                        // Else set game size to size in acf
                        Game.SizeOnDisk = SizeOnDisk;
                }

                // Add our game details to global list
                Library.Games.Add(Game);
            }
            catch (Exception ex)
            {
                Logger.LogToFile(Logger.LogType.Library, ex.ToString());
                MessageBox.Show(ex.ToString());
            }
        }

        public static void ReadGameDetailsFromZip(string zipPath, Definitions.Library targetLibrary)
        {
            try
            {
                // Open archive for read
                using (ZipArchive compressedArchive = ZipFile.OpenRead(zipPath))
                {
                    // For each file in opened archive
                    foreach (ZipArchiveEntry acfFilePath in compressedArchive.Entries.Where(x => x.Name.Contains("appmanifest_")))
                    {
                        // If it contains
                        // Define a KeyValue reader
                        Framework.KeyValue Key = new Framework.KeyValue();

                        // Open .acf file from archive as text
                        Key.ReadAsText(acfFilePath.Open());

                        // If acf file has no children, skip this archive
                        if (Key.Children.Count == 0)
                            continue;

                        AddNewGame(Convert.ToInt32(Key["appID"].Value), !string.IsNullOrEmpty(Key["name"].Value) ? Key["name"].Value : Key["UserConfig"]["name"].Value, Key["installdir"].Value, targetLibrary, Convert.ToInt64(Key["SizeOnDisk"].Value), acfFilePath.LastWriteTime.ToUnixTimeSeconds(), true);

                        /*
                        if (Key["SLM"]["name"] != null)
                            MessageBox.Show(Key["SLM"]["name"].Value);
                        */
                    }
                }
            }
            catch (InvalidDataException iEx)
            {
                MessageBoxResult removeTheBuggenArchive = MessageBox.Show($"An error happened while parsing zip file ({zipPath})\n\nIt is still suggested to check the archive file manually to see if it is really corrupted or not!\n\nWould you like to remove the given archive file?", "An error happened while parsing zip file", MessageBoxButton.YesNo);

                if (removeTheBuggenArchive == MessageBoxResult.Yes)
                    new FileInfo(zipPath).Delete();

                System.Diagnostics.Debug.WriteLine(iEx);
                Logger.LogToFile(Logger.LogType.Library, iEx.ToString());
            }
        }

        public static void UpdateMainForm(Definitions.Library Library, string Search = null)
        {
            try
            {
                if (Main.Accessor.gamePanel.Dispatcher.CheckAccess())
                {
                    if (Definitions.List.Libraries.Count(x => x == Library) == 0)
                    {
                        Main.Accessor.gamePanel.ItemsSource = null;
                        return;
                    }

                    Func<Definitions.Game, object> Sort = SLM.Settings.GetSortingMethod();

                    Main.Accessor.gamePanel.ItemsSource = (Properties.Settings.Default.defaultGameSortingMethod == "sizeOnDisk" || Properties.Settings.Default.defaultGameSortingMethod == "LastUpdated") ? 
                        (((string.IsNullOrEmpty(Search)) ? Library.Games.OrderByDescending(Sort).ToList() : Library.Games.Where(
                            y => y.AppName.ToLowerInvariant().Contains(Search.ToLowerInvariant()) // Search by appName
                            || y.AppID.ToString().Contains(Search) // Search by app ID
                        ).OrderByDescending(Sort).ToList()
                        )) :
                        ((string.IsNullOrEmpty(Search)) ? Library.Games.OrderBy(Sort).ToList() : Library.Games.Where(
                        y => y.AppName.ToLowerInvariant().Contains(Search.ToLowerInvariant()) // Search by appName
                        || y.AppID.ToString().Contains(Search) // Search by app ID
                        ).OrderBy(Sort).ToList()
                        );
                }
                else
                {
                    Main.Accessor.gamePanel.Dispatcher.Invoke(delegate
                    {
                        UpdateMainForm(Library, Search);
                    }, System.Windows.Threading.DispatcherPriority.Normal);
                }

            }
            catch (Exception ex)
            {
                Logger.LogToFile(Logger.LogType.SLM, ex.ToString());
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
