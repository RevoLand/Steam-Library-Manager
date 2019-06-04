using MahApps.Metro.Controls.Dialogs;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Steam_Library_Manager.Definitions
{
    public class SteamLibrary : LibraryBase
    {
        public DirectoryInfo SteamAppsFolder => new DirectoryInfo(Path.Combine(FullPath, "SteamApps"));
        public DirectoryInfo SteamBackupsFolder => new DirectoryInfo(Path.Combine(FullPath, "SteamBackups"));
        public DirectoryInfo CommonFolder => new DirectoryInfo(Path.Combine(SteamAppsFolder.FullName, "common"));
        public DirectoryInfo DownloadFolder => new DirectoryInfo(Path.Combine(SteamAppsFolder.FullName, "downloading"));
        public DirectoryInfo WorkshopFolder => new DirectoryInfo(Path.Combine(SteamAppsFolder.FullName, "workshop"));

        public SteamLibrary(string fullPath, Library library, bool isMain = false)
        {
            FullPath = fullPath;
            IsMain = isMain;
            Library = library;
        }

        public override async void UpdateAppListAsync()
        {
            try
            {
                SteamAppsFolder.Refresh();

                if (!SteamAppsFolder.Exists)
                {
                    SteamAppsFolder.Create();
                    SteamAppsFolder.Refresh();

                    if (!SteamAppsFolder.Exists)
                    {
                        MessageBox.Show(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.SteamLibrary_CantCreate)), new { SteamAppsFolderFullPath = SteamAppsFolder.FullName }));
                        return;
                    }
                }

                if (Apps.Count > 0)
                {
                    Apps.Clear();
                }

                // Foreach *.acf file found in library
                foreach (var acfFile in SteamAppsFolder.EnumerateFiles("appmanifest_*.acf", SearchOption.TopDirectoryOnly).ToList())
                //Parallel.ForEach(SteamAppsFolder.EnumerateFiles("appmanifest_*.acf", SearchOption.TopDirectoryOnly), async AcfFile =>
                {
                    // Define a new value and call KeyValue
                    var keyValReader = new Framework.KeyValue();

                    // Read the *.acf file as text
                    keyValReader.ReadFileAsText(acfFile.FullName);

                    // If key doesn't contains a child (value in acf file)
                    if (keyValReader.Children.Count == 0)
                    {
                        List.LCProgress.Report(new List.JunkInfo
                        {
                            FSInfo = new FileInfo(acfFile.FullName),
                            Size = acfFile.Length,
                            Library = Library
                        });

                        return;
                    }

                    await Functions.App.AddSteamAppAsync(Convert.ToInt32(keyValReader["appID"].Value),
                        keyValReader["name"].Value ?? keyValReader["UserConfig"]["name"].Value,
                        keyValReader["installdir"].Value, Library, Convert.ToInt64(keyValReader["SizeOnDisk"].Value),
                        Convert.ToInt64(keyValReader["LastUpdated"].Value), false).ConfigureAwait(true);
                }
                //);

                // Do a loop for each *.zip file in library
                Parallel.ForEach(Directory.EnumerateFiles(SteamAppsFolder.FullName, "*.zip", SearchOption.TopDirectoryOnly).ToList(), ArchiveFile => Functions.App.ReadDetailsFromZip(ArchiveFile, Library));

                SteamBackupsFolder.Refresh();
                if (Library.Type == Enums.LibraryType.SLM && SteamBackupsFolder.Exists)
                {
                    foreach (var skuFile in SteamBackupsFolder.EnumerateFiles("*.sis", SearchOption.AllDirectories).ToList())
                    {
                        var keyValReader = new Framework.KeyValue();

                        keyValReader.ReadFileAsText(skuFile.FullName);

                        var appNames = System.Text.RegularExpressions.Regex.Split(keyValReader["name"].Value, " and ");

                        var i = 0;
                        var appSize = Functions.FileSystem.GetDirectorySize(skuFile.Directory, true);
                        foreach (var app in keyValReader["apps"].Children)
                        {
                            if (Apps.Count(x => x.AppId == Convert.ToInt32(app.Value) && x.IsSteamBackup) > 0)
                                continue;

                            await Functions.App.AddSteamAppAsync(Convert.ToInt32(app.Value), appNames[i], skuFile.DirectoryName, Library, appSize, skuFile.LastWriteTimeUtc.ToUnixTimestamp(), false, true).ConfigureAwait(true);

                            if (appNames.Length > 1)
                                i++;
                        }
                    }
                }

                if (SLM.CurrentSelectedLibrary != null && SLM.CurrentSelectedLibrary == Library)
                {
                    Functions.App.UpdateAppPanel(Library);
                }
            }
            catch (UnauthorizedAccessException uaex)
            {
                await Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                {
                    await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.UnauthorizedAccessException)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.UnauthorizedAccessExceptionMessage)), new { FullPath, ExceptionMessage = uaex.Message })).ConfigureAwait(true);
                }, System.Windows.Threading.DispatcherPriority.Normal).ConfigureAwait(true);
            }
            catch (DirectoryNotFoundException dnfex)
            {
                await Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                {
                    await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.DirectoryNotFoundException)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.DirectoryNotFoundExceptionMessage)), new { FolderfullPath = FullPath, ExceptionMessage = dnfex.Message })).ConfigureAwait(true);
                }, System.Windows.Threading.DispatcherPriority.Normal).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Logger.Fatal(ex);
            }
        }

        public override async void ParseMenuItemActionAsync(string action)
        {
            switch (action.ToLowerInvariant())
            {
                // Opens game installation path in explorer
                case "disk":
                    if (SteamAppsFolder.Exists)
                    {
                        Process.Start(SteamAppsFolder.FullName);
                    }

                    break;

                case "deletelibrary":

                    if (IsMain)
                    {
                        await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.DeleteMainSteamLibrary)), Functions.SLM.Translate(nameof(Properties.Resources.DeleteMainSteamLibraryMessage)), MessageDialogStyle.Affirmative).ConfigureAwait(true);
                        return;
                    }

                    var moveGamesBeforeDeletion = await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.MoveGamesInLibrary)), Functions.SLM.Translate(nameof(Properties.Resources.MoveGamesInLibraryMessage)), MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, new MetroDialogSettings
                    {
                        FirstAuxiliaryButtonText = Functions.SLM.Translate(nameof(Properties.Resources.DeleteLibraryWithoutMovingGames))
                    }).ConfigureAwait(true);

                    if (moveGamesBeforeDeletion == MessageDialogResult.Affirmative)
                    {
                        await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.MoveGamesConfirmError)), Functions.SLM.Translate(nameof(Properties.Resources.MoveGamesConfirmErrorMessage)), MessageDialogStyle.Affirmative).ConfigureAwait(true);
                    }
                    else if (moveGamesBeforeDeletion == MessageDialogResult.FirstAuxiliary)
                    {
                        RemoveLibraryAsync(true);
                    }

                    break;

                case "deletelibraryslm":

                    foreach (SteamAppInfo App in Apps.ToList())
                    {
                        if (!await App.DeleteFilesAsync().ConfigureAwait(true))
                        {
                            await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.SteamApp_RemovingError)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.SteamApp_RemovingErrorMessage)), new { FullPath }), MessageDialogStyle.Affirmative).ConfigureAwait(true);

                            return;
                        }
                    }

                    Functions.SLM.Library.UpdateLibraryVisual();

                    await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.DeleteSteamLibrary)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.DeleteSteamLibraryMessage)), new { LibraryFullPath = FullPath }), MessageDialogStyle.Affirmative).ConfigureAwait(true);
                    break;
            }
        }

        public async void UpdateLibraryPathAsync(string NewLibraryPath)
        {
            try
            {
                await Functions.Steam.CloseSteamAsync().ConfigureAwait(true);

                // Make a KeyValue reader
                Framework.KeyValue Key = new Framework.KeyValue();

                // Read vdf file
                Key.ReadFileAsText(Global.Steam.vdfFilePath);

                // Change old library path with new one
                Key["Software"]["Valve"]["Steam"].Children.Find(key => key.Value.Contains(FullPath)).Value = NewLibraryPath;

                // Update config.vdf file with changes
                Key.SaveToFile(Global.Steam.vdfFilePath, false);

                // Since this file started to interrupt us?
                // No need to bother with it since config.vdf is the real deal, just remove it and Steam client will handle with some magic.
                if (File.Exists(Path.Combine(Properties.Settings.Default.steamInstallationPath, "steamapps", "libraryfolders.vdf")))
                {
                    File.Delete(Path.Combine(Properties.Settings.Default.steamInstallationPath, "steamapps", "libraryfolders.vdf"));
                }

                Functions.Steam.RestartSteamAsync();
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }

        public async void DeleteFilesAsync()
        {
            try
            {
                if (SteamAppsFolder.Exists)
                {
                    await Task.Run(() => SteamAppsFolder.Delete(true)).ConfigureAwait(true);
                }

                if (WorkshopFolder.Exists)
                {
                    await Task.Run(() => WorkshopFolder.Delete(true)).ConfigureAwait(true);
                }

                if (DownloadFolder.Exists)
                {
                    await Task.Run(() => DownloadFolder.Delete(true)).ConfigureAwait(true);
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }

        public override async void RemoveLibraryAsync(bool withFiles)
        {
            try
            {
                if (withFiles)
                {
                    DeleteFilesAsync();
                }

                List.Libraries.Remove(Library);

                await Functions.Steam.CloseSteamAsync().ConfigureAwait(true);

                // Make a KeyValue reader
                var keyValReader = new Framework.KeyValue();

                // Read vdf file
                keyValReader.ReadFileAsText(Global.Steam.vdfFilePath);

                // Remove old library
                keyValReader["Software"]["Valve"]["Steam"].Children.RemoveAll(x => x.Value == FullPath);

                var i = 1;
                foreach (var key in keyValReader["Software"]["Valve"]["Steam"].Children.FindAll(x => x.Name.Contains("BaseInstallFolder")))
                {
                    key.Name = $"BaseInstallFolder_{i}";
                    i++;
                }

                // Update libraryFolders.vdf file with changes
                keyValReader.SaveToFile(Global.Steam.vdfFilePath, false);

                // Since this file started to interrupt us?
                // No need to bother with it since config.vdf is the real deal, just remove it and Steam client will handle with some magic.
                if (File.Exists(Path.Combine(Properties.Settings.Default.steamInstallationPath, "steamapps", "libraryfolders.vdf")))
                {
                    File.Delete(Path.Combine(Properties.Settings.Default.steamInstallationPath, "steamapps", "libraryfolders.vdf"));
                }

                Functions.Steam.RestartSteamAsync();
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }

        public void UpdateJunks()
        {
            try
            {
                CommonFolder.Refresh();
                if (CommonFolder.Exists)
                {
                    foreach (var dirInfo in CommonFolder.GetDirectories().ToList().Where(
                        x => Apps.Count(y => string.Equals(y.InstallationDirectory.Name, x.Name, StringComparison.InvariantCultureIgnoreCase)) == 0
                        && x.Name != "241100" // Steam controller configs
                        && Functions.TaskManager.TaskList.Count(
                            z => string.Equals(z.SteamApp.InstallationDirectory.Name, x.Name, StringComparison.InvariantCultureIgnoreCase)
                            && z.TargetLibrary == Library
                            ) == 0
                        ).OrderByDescending(x => Functions.FileSystem.GetDirectorySize(x, true)))
                    {
                        var junk = new List.JunkInfo
                        {
                            FSInfo = dirInfo,
                            Size = Functions.FileSystem.GetDirectorySize(dirInfo, true),
                            Library = Library
                        };

                        if (List.LcItems.Count(x => x.FSInfo.FullName == junk.FSInfo.FullName) == 0)
                        {
                            List.LCProgress.Report(junk);
                        }
                    }
                }

                WorkshopFolder.Refresh();
                if (WorkshopFolder.Exists)
                {
                    foreach (var fileDetails in WorkshopFolder.EnumerateFiles("appworkshop_*.acf", SearchOption.TopDirectoryOnly).ToList().Where(
                        x => Apps.Count(y => x.Name == y.WorkShopAcfName) == 0
                        && !string.Equals(x.Name, "appworkshop_241100.acf" // Steam Controller Configs
, StringComparison.InvariantCultureIgnoreCase) // Steam Controller Configs
                        && Functions.TaskManager.TaskList.Count(
                            z => string.Equals(z.SteamApp.WorkShopAcfName, x.Name
, StringComparison.InvariantCultureIgnoreCase)
                            && z.TargetLibrary == Library
                            ) == 0
                        ))
                    {
                        var junk = new List.JunkInfo
                        {
                            FSInfo = fileDetails,
                            Size = fileDetails.Length,
                            Library = Library
                        };

                        if (List.LcItems.Count(x => x.FSInfo.FullName == junk.FSInfo.FullName) == 0)
                        {
                            List.LCProgress.Report(junk);
                        }
                    }

                    if (Directory.Exists(Path.Combine(WorkshopFolder.FullName, "content")))
                    {
                        foreach (var dirInfo in new DirectoryInfo(Path.Combine(WorkshopFolder.FullName, "content")).GetDirectories().ToList().Where(
                            x => Apps.Count(y => y.AppId.ToString() == x.Name) == 0
                            && x.Name != "241100" // Steam controller configs
                            && Functions.TaskManager.TaskList.Count(
                                z => string.Equals(z.SteamApp.WorkShopPath.Name, x.Name
, StringComparison.InvariantCultureIgnoreCase)
                                && z.TargetLibrary == Library
                            ) == 0
                            ).OrderByDescending(x => Functions.FileSystem.GetDirectorySize(x, true)))
                        {
                            var junk = new List.JunkInfo
                            {
                                FSInfo = dirInfo,
                                Size = Functions.FileSystem.GetDirectorySize(dirInfo, true),
                                Library = Library
                            };

                            if (List.LcItems.Count(x => x.FSInfo.FullName == junk.FSInfo.FullName) == 0)
                            {
                                List.LCProgress.Report(junk);
                            }
                        }
                    }

                    if (Directory.Exists(Path.Combine(WorkshopFolder.FullName, "downloads")))
                    {
                        foreach (var fileDetails in new DirectoryInfo(Path.Combine(WorkshopFolder.FullName, "downloads")).EnumerateFiles("*.patch", SearchOption.TopDirectoryOnly).ToList().Where(
                            x => Apps.Count(y => x.Name.Contains($"state_{y.AppId}_")) == 0
                            ))
                        {
                            var junk = new List.JunkInfo
                            {
                                FSInfo = fileDetails,
                                Size = fileDetails.Length,
                                Library = Library
                            };

                            if (List.LcItems.Count(x => x.FSInfo.FullName == junk.FSInfo.FullName) == 0)
                            {
                                List.LCProgress.Report(junk);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }
    }
}