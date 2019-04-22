using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace Steam_Library_Manager.Functions
{
    internal static class App
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void AddSteamApp(int AppID, string AppName, string InstallationPath, Definitions.Library Library, long SizeOnDisk, long LastUpdated, bool IsCompressed, bool IsSteamBackup = false)
        {
            try
            {
                // Make a new definition for app
                Definitions.SteamAppInfo App = new Definitions.SteamAppInfo()
                {
                    // Set game appID
                    AppID = AppID,

                    // Set game name
                    AppName = AppName,

                    Library = Library,
                    InstallationDirectory = new DirectoryInfo(InstallationPath),

                    // Define it is an archive
                    IsCompressed = IsCompressed,
                    IsSteamBackup = IsSteamBackup,
                    LastUpdated = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(LastUpdated)
                };

                if (Definitions.List.SteamApps_LastPlayedDic.ContainsKey(AppID))
                {
                    App.LastPlayed = Definitions.List.SteamApps_LastPlayedDic[AppID];
                }

                // If app doesn't have a folder in "common" directory and "downloading" directory then skip
                if (!App.CommonFolder.Exists && !App.DownloadFolder.Exists && !App.IsCompressed && !App.IsSteamBackup)
                {
                    Definitions.List.LCProgress.Report(new Definitions.List.JunkInfo
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
                        App.CompressedArchiveName.Refresh();

                        // And set archive size as game size
                        App.SizeOnDisk = App.CompressedArchiveName?.Length ?? 0;
                    }
                }
                else
                {
                    // If SizeOnDisk value from .ACF file is not equals to 0
                    if (Properties.Settings.Default.gameSizeCalculationMethod != "ACF")
                    {
                        List<FileInfo> GameFiles = App.GetFileList();
                        long GameSize = 0;

                        System.Threading.Tasks.Parallel.ForEach(GameFiles, File => Interlocked.Add(ref GameSize, File.Length));

                        App.SizeOnDisk = GameSize;
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
                logger.Fatal(ex);
            }
        }

        public static async void ReadDetailsFromZip(string ZipPath, Definitions.Library targetLibrary)
        {
            try
            {
                // Open archive for read
                using (ZipArchive Archive = ZipFile.OpenRead(ZipPath))
                {
                    if (Archive.Entries.Count > 0)
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

                            AddSteamApp(Convert.ToInt32(KeyValReader["appID"].Value), !string.IsNullOrEmpty(KeyValReader["name"].Value) ? KeyValReader["name"].Value : KeyValReader["UserConfig"]["name"].Value, KeyValReader["installdir"].Value, targetLibrary, Convert.ToInt64(KeyValReader["SizeOnDisk"].Value), Convert.ToInt64(KeyValReader["LastUpdated"].Value), true);
                        }
                    }
                }
            }
            catch (IOException IEx)
            {
                await Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                {
                    if (await Main.FormAccessor.ShowMessageAsync(SLM.Translate(nameof(Properties.Resources.ReadZip_IOException)), Framework.StringFormat.Format(SLM.Translate(nameof(Properties.Resources.ReadZip_IOExceptionMessage)), new { ZipPath }), MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
                    {
                        NegativeButtonText = SLM.Translate(Properties.Resources.ReadZip_DontDelete)
                    }) == MessageDialogResult.Affirmative)
                    {
                        File.Delete(ZipPath);
                    }
                });

                System.Diagnostics.Debug.WriteLine(IEx);
                logger.Fatal(IEx);
            }
            catch (InvalidDataException IEx)
            {
                await Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                {
                    if (await Main.FormAccessor.ShowMessageAsync(SLM.Translate(nameof(Properties.Resources.ReadZip_InvalidDataException)), Framework.StringFormat.Format(SLM.Translate(nameof(Properties.Resources.ReadZip_InvalidDataExceptionMessage)), new { ZipPath }), MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
                    {
                        NegativeButtonText = SLM.Translate(Properties.Resources.ReadZip_DontDelete)
                    }) == MessageDialogResult.Affirmative)
                    {
                        File.Delete(ZipPath);
                    }
                });

                System.Diagnostics.Debug.WriteLine(IEx);
                logger.Fatal(IEx);
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
            }
        }

        public static void UpdateAppPanel(Definitions.Library Library) => Main.FormAccessor.LibraryChange.Report(Library);
    }
}