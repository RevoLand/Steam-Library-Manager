using MahApps.Metro.Controls.Dialogs;
using Steam_Library_Manager.Definitions.Enums;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using DirectoryInfo = Alphaleonis.Win32.Filesystem.DirectoryInfo;
using File = Alphaleonis.Win32.Filesystem.File;
using FileInfo = Alphaleonis.Win32.Filesystem.FileInfo;
using Path = Alphaleonis.Win32.Filesystem.Path;

namespace Steam_Library_Manager.Functions
{
    internal static class App
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static async System.Threading.Tasks.Task AddSteamAppAsync(int AppID, string AppName, string InstallationPath, int StateFlag, Definitions.Library Library, long SizeOnDisk, long LastUpdated, bool IsCompressed, bool IsSteamBackup = false)
        {
            try
            {
                // Make a new definition for app
                var appInfo = new Definitions.SteamAppInfo(AppID, Library, new DirectoryInfo(Path.Combine(Library.DirectoryList["Common"].FullName, InstallationPath)))
                {
                    // Set game name
                    AppName = AppName,

                    // Define it is an archive
                    IsCompressed = IsCompressed,
                    IsSteamBackup = IsSteamBackup,
                    LastUpdated = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(LastUpdated)
                };

                if (Definitions.List.SteamAppsLastPlayedDic.ContainsKey(AppID))
                {
                    appInfo.LastPlayed = Definitions.List.SteamAppsLastPlayedDic[AppID];
                }

                // If app doesn't have a folder in "common" directory and "downloading" directory then skip
                if (!appInfo.InstallationDirectory.Exists && StateFlag == 4 && !appInfo.IsCompressed && !appInfo.IsSteamBackup)
                {
                    var acfFile = new FileInfo(Path.Combine(Library.DirectoryList["SteamApps"].FullName, $"appmanifest_{appInfo.AppId}.acf"));

                    if (Definitions.List.IgnoredJunkItems.Contains(acfFile.FullName))
                    {
                        return;
                    }

                    Definitions.List.LcProgress.Report(new Definitions.List.JunkInfo
                    {
                        FSInfo = acfFile,
                        Size = FileSystem.FormatBytes(acfFile.Length),
                        Library = Library,
                        Tag = JunkType.HeadlessDataFile
                    });

                    return; // Do not add pre-loads to list
                }

                if (IsCompressed)
                {
                    // If user want us to get archive size from real uncompressed size
                    if (Properties.Settings.Default.archiveSizeCalculationMethod.StartsWith("Uncompressed"))
                    {
                        // Open archive to read
                        using (var archive = ZipFile.OpenRead(appInfo.CompressedArchivePath.FullName))
                        {
                            // For each file in archive
                            foreach (var entry in archive.Entries)
                            {
                                // Add file size to sizeOnDisk
                                appInfo.SizeOnDisk += entry.Length;
                            }
                        }
                    }
                    else
                    {
                        appInfo.CompressedArchivePath.Refresh();

                        // And set archive size as game size
                        appInfo.SizeOnDisk = appInfo.CompressedArchivePath?.Length ?? 0;
                    }
                }
                else
                {
                    // If SizeOnDisk value from .ACF file is not equals to 0
                    if (Properties.Settings.Default.gameSizeCalculationMethod != "ACF")
                    {
                        var gameFiles = appInfo.GetFileList();
                        long gameSize = 0;

                        System.Threading.Tasks.Parallel.ForEach(gameFiles, file => Interlocked.Add(ref gameSize, file.Length));

                        appInfo.SizeOnDisk = gameSize;
                    }
                    else
                    {
                        // Else set game size to size in acf
                        appInfo.SizeOnDisk = SizeOnDisk;
                    }
                }

                appInfo.IsCompacted = await appInfo.CompactStatus().ConfigureAwait(false);

                // Add our game details to global list
                Library.Apps.Add(appInfo);

                if (Definitions.SLM.CurrentSelectedLibrary == Library)
                {
                    UpdateAppPanel(Library);
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }

        public static async void ReadDetailsFromZip(string zipPath, Definitions.Library targetLibrary)
        {
            try
            {
                // Open archive for read
                using (var archive = ZipFile.OpenRead(zipPath))
                {
                    if (archive.Entries.Count <= 0) return;

                    // For each file in opened archive
                    foreach (var acfEntry in archive.Entries.Where(x => x.Name.Contains("appmanifest_")))
                    {
                        // If it contains
                        // Define a KeyValue reader
                        var keyValReader = new Framework.KeyValue();

                        // Open .acf file from archive as text
                        keyValReader.ReadAsText(acfEntry.Open());

                        // If acf file has no children, skip this archive
                        if (keyValReader.Children.Count == 0)
                        {
                            continue;
                        }

                        await AddSteamAppAsync(Convert.ToInt32(keyValReader["appID"].Value), !string.IsNullOrEmpty(keyValReader["name"].Value) ? keyValReader["name"].Value : keyValReader["UserConfig"]["name"].Value, keyValReader["installdir"].Value, Convert.ToInt32(keyValReader["StateFlags"].Value), targetLibrary, Convert.ToInt64(keyValReader["SizeOnDisk"].Value), Convert.ToInt64(keyValReader["LastUpdated"].Value), true).ConfigureAwait(true);
                    }
                }
            }
            catch (IOException IEx)
            {
                await Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                {
                    if (await Main.FormAccessor.ShowMessageAsync(SLM.Translate(nameof(Properties.Resources.ReadZip_IOException)), Framework.StringFormat.Format(SLM.Translate(nameof(Properties.Resources.ReadZip_IOExceptionMessage)), new { ZipPath = zipPath }), MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
                    {
                        NegativeButtonText = SLM.Translate(nameof(Properties.Resources.ReadZip_DontDelete))
                    }).ConfigureAwait(true) == MessageDialogResult.Affirmative)
                    {
                        File.Delete(zipPath);
                    }
                }).ConfigureAwait(true);

                System.Diagnostics.Debug.WriteLine(IEx);
                Logger.Fatal(IEx);
            }
            catch (InvalidDataException IEx)
            {
                await Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                {
                    if (await Main.FormAccessor.ShowMessageAsync(SLM.Translate(nameof(Properties.Resources.ReadZip_InvalidDataException)), Framework.StringFormat.Format(SLM.Translate(nameof(Properties.Resources.ReadZip_InvalidDataExceptionMessage)), new { ZipPath = zipPath }), MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
                    {
                        NegativeButtonText = SLM.Translate(nameof(Properties.Resources.ReadZip_DontDelete))
                    }).ConfigureAwait(true) == MessageDialogResult.Affirmative)
                    {
                        File.Delete(zipPath);
                    }
                }).ConfigureAwait(true);

                System.Diagnostics.Debug.WriteLine(IEx);
                Logger.Fatal(IEx);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }

        public static void UpdateAppPanel(Definitions.Library library) => Main.FormAccessor.LibraryChange.Report(library);
    }
}