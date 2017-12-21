using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;

namespace Steam_Library_Manager.Functions
{
    class App
    {
        public static void AddSteamApp(int AppID, string AppName, string InstallationPath, Definitions.Library Library, long SizeOnDisk, long LastUpdated, bool IsCompressed, bool IsSteamBackup = false)
        {
            try
            {
                // Make a new definition for app
                Definitions.AppInfo App = new Definitions.AppInfo()
                {
                    // Set game appID
                    AppID = AppID,

                    // Set game name
                    AppName = AppName,

                    Library = Library,
                    InstallationPath = new DirectoryInfo(InstallationPath),

                    // Define it is an archive
                    IsCompressed = IsCompressed,
                    LastUpdated = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(LastUpdated)
                };

                // If app doesn't have a folder in "common" directory and "downloading" directory then skip
                if (!App.CommonFolder.Exists && !App.DownloadFolder.Exists && !App.IsCompressed)
                {
                    Definitions.List.LCItems.Add(new Definitions.List.JunkInfo
                    {
                        FSInfo = new FileInfo(App.FullAcfPath.FullName),
                        Size = App.FullAcfPath.Length,
                        Library = Library
                    });

                    return; // Do not add pre-loads to list
                }

                if (IsCompressed)
                {
                    // If user want us to get archive size from real uncompressed size
                    if (Properties.Settings.Default.archiveSizeCalculationMethod.StartsWith("Uncompressed"))
                    {
                        // Open archive to read
                        using (ZipArchive Archive = ZipFile.OpenRead(App.CompressedArchiveName.FullName))
                        {
                            // For each file in archive
                            foreach (ZipArchiveEntry Entry in Archive.Entries)
                            {
                                // Add file size to sizeOnDisk
                                App.SizeOnDisk += Entry.Length;
                            }
                        }
                    }
                    else
                    {
                        // And set archive size as game size
                        App.SizeOnDisk = FileSystem.GetFileSize(Path.Combine(App.Library.Steam.SteamAppsFolder.FullName, App.AppID + ".zip"));
                    }
                }
                else
                {
                    // If SizeOnDisk value from .ACF file is not equals to 0
                    if (Properties.Settings.Default.gameSizeCalculationMethod != "ACF")
                    {
                        List<FileSystemInfo> GameFiles = App.GetFileList();

                        System.Threading.Tasks.Parallel.ForEach(GameFiles, File =>
                        {
                            App.SizeOnDisk += (File as FileInfo).Length;
                        });

                    }
                    else
                    {
                        // Else set game size to size in acf
                        App.SizeOnDisk = SizeOnDisk;
                    }
                }

                // Add our game details to global list
                Library.Steam.Apps.Add(App);

                if (Definitions.SLM.CurrentSelectedLibrary == Library)
                {
                    UpdateAppPanel(Library);
                }
            }
            catch (Exception ex)
            {
                Logger.LogToFile(Logger.LogType.Library, ex.ToString());
                MessageBox.Show(ex.ToString());
            }
        }

        public static void ReadDetailsFromZip(string ZipPath, Definitions.Library targetLibrary)
        {
            try
            {
                // Open archive for read
                using (ZipArchive Archive = ZipFile.OpenRead(ZipPath))
                {
                    // For each file in opened archive
                    foreach (ZipArchiveEntry AcfEntry in Archive.Entries.Where(x => x.Name.Contains("appmanifest_")))
                    {
                        // If it contains
                        // Define a KeyValue reader
                        Framework.KeyValue KeyValReader = new Framework.KeyValue();

                        // Open .acf file from archive as text
                        KeyValReader.ReadAsText(AcfEntry.Open());

                        // If acf file has no children, skip this archive
                        if (KeyValReader.Children.Count == 0)
                        {
                            continue;
                        }

                        AddSteamApp(Convert.ToInt32(KeyValReader["appID"].Value), !string.IsNullOrEmpty(KeyValReader["name"].Value) ? KeyValReader["name"].Value : KeyValReader["UserConfig"]["name"].Value, KeyValReader["installdir"].Value, targetLibrary, Convert.ToInt64(KeyValReader["SizeOnDisk"].Value), AcfEntry.LastWriteTime.ToUnixTimeSeconds(), true);
                    }
                }
            }
            catch (IOException)
            {
                ReadDetailsFromZip(ZipPath, targetLibrary);
            }
            catch (InvalidDataException IEx)
            {
                MessageBoxResult RemoveBuggenArchive = MessageBox.Show($"An error happened while parsing zip file ({ZipPath})\n\nIt is still suggested to check the archive file manually to see if it is really corrupted or not!\n\nWould you like to remove the given archive file?", "An error happened while parsing zip file", MessageBoxButton.YesNo);

                if (RemoveBuggenArchive == MessageBoxResult.Yes)
                {
                    new FileInfo(ZipPath).Delete();
                }

                System.Diagnostics.Debug.WriteLine(IEx);
                Logger.LogToFile(Logger.LogType.Library, IEx.ToString());
            }
        }

        public static void UpdateAppPanel(Definitions.Library Library)
        {
            try
            {
                string Search = (Properties.Settings.Default.includeSearchResults) ? Properties.Settings.Default.SearchText : null;

                if (Main.FormAccessor.AppPanel.Dispatcher.CheckAccess())
                {
                    if (Definitions.List.Libraries.Count(x => x == Library) == 0 || !Library.DirectoryInfo.Exists)
                    {
                        Main.FormAccessor.AppPanel.ItemsSource = null;
                        return;
                    }

                    Func<Definitions.AppInfo, object> Sort = SLM.Settings.GetSortingMethod();

                    switch (Library.Type)
                    {
                        case Definitions.Enums.LibraryType.Steam:
                            Main.FormAccessor.AppPanel.ItemsSource = (Properties.Settings.Default.defaultGameSortingMethod == "sizeOnDisk" || Properties.Settings.Default.defaultGameSortingMethod == "LastUpdated") ?
                                (((string.IsNullOrEmpty(Search)) ?
                                Library.Steam.Apps.OrderByDescending(Sort).ToList() : Library.Steam.Apps.Where(
                                    y => y.AppName.ToLowerInvariant().Contains(Search.ToLowerInvariant()) // Search by appName
                                    || y.AppID.ToString().Contains(Search) // Search by app ID
                                ).OrderByDescending(Sort).ToList()
                                )) :
                                ((string.IsNullOrEmpty(Search)) ? Library.Steam.Apps.OrderBy(Sort).ToList() : Library.Steam.Apps.Where(
                                y => y.AppName.ToLowerInvariant().Contains(Search.ToLowerInvariant()) // Search by appName
                                || y.AppID.ToString().Contains(Search) // Search by app ID
                                ).OrderBy(Sort).ToList()
                                );
                            break;
                        case Definitions.Enums.LibraryType.SLM:
                            List<Definitions.AppInfo> Applist = (((string.IsNullOrEmpty(Search)) ?
                                Library.Steam.Apps.OrderByDescending(Sort).ToList() : Library.Steam.Apps.Where(
                                    y => y.AppName.ToLowerInvariant().Contains(Search.ToLowerInvariant()) // Search by appName
                                    || y.AppID.ToString().Contains(Search) // Search by app ID
                                ).ToList()
                                ));

                            // Origin

                            // Uplay

                            Main.FormAccessor.AppPanel.ItemsSource = (Properties.Settings.Default.defaultGameSortingMethod == "sizeOnDisk" || Properties.Settings.Default.defaultGameSortingMethod == "LastUpdated") ? 
                                Applist.OrderByDescending(Sort) : Applist.OrderBy(Sort);
                            break;
                    }
                }
                else
                {
                    Main.FormAccessor.AppPanel.Dispatcher.Invoke(delegate
                    {
                        UpdateAppPanel(Library);
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
