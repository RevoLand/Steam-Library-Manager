using Dasync.Collections;
using MahApps.Metro.Controls.Dialogs;
using Steam_Library_Manager.Definitions.Enums;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Directory = Alphaleonis.Win32.Filesystem.Directory;
using DirectoryInfo = Alphaleonis.Win32.Filesystem.DirectoryInfo;
using File = Alphaleonis.Win32.Filesystem.File;
using FileInfo = Alphaleonis.Win32.Filesystem.FileInfo;
using Path = Alphaleonis.Win32.Filesystem.Path;

namespace Steam_Library_Manager.Definitions
{
    public class SteamLibrary : Library
    {
        public SteamLibrary(string fullPath, bool isMain = false)
        {
            FullPath = fullPath;
            IsMain = isMain;

            DirectoryList.Add("SteamApps", new DirectoryInfo(Path.Combine(FullPath, "SteamApps")));
            DirectoryList.Add("SteamBackups", new DirectoryInfo(Path.Combine(FullPath, "SteamBackups")));
            DirectoryList.Add("Common", new DirectoryInfo(Path.Combine(DirectoryList["SteamApps"].FullName, "common")));
            DirectoryList.Add("Download", new DirectoryInfo(Path.Combine(DirectoryList["SteamApps"].FullName, "downloading")));
            DirectoryList.Add("Workshop", new DirectoryInfo(Path.Combine(DirectoryList["SteamApps"].FullName, "workshop")));

            AllowedAppTypes.Add(LibraryType.Steam);
            AllowedAppTypes.Add(LibraryType.SLM);
        }

        public override async void UpdateAppList()
        {
            try
            {
                if (IsUpdatingAppList)
                    return;

                IsUpdatingAppList = true;

                DirectoryList["SteamApps"].Refresh();

                if (!DirectoryList["SteamApps"].Exists)
                {
                    DirectoryList["SteamApps"].Create();
                    DirectoryList["SteamApps"].Refresh();

                    if (!DirectoryList["SteamApps"].Exists)
                    {
                        MessageBox.Show(Framework.StringFormat.Format(
                            Functions.SLM.Translate(nameof(Properties.Resources.SteamAppsFolderNotExists)),
                            new { SteamAppsFolderFullPath = DirectoryList["SteamApps"].FullName }));
                        return;
                    }
                }

                if (Apps.Count > 0)
                {
                    Apps.Clear();
                }

                // Foreach *.acf file found in library
                await DirectoryList["SteamApps"].EnumerateFiles("appmanifest_*.acf", SearchOption.TopDirectoryOnly)
                    .ParallelForEachAsync(
                        async acfFile =>
                        {
                            // Define a new value and call KeyValue
                            var keyValReader = new Framework.KeyValue();

                            // Read the *.acf file as text
                            keyValReader.ReadFileAsText(acfFile.FullName);

                            // If key doesn't contains a child (value in acf file)
                            if (keyValReader.Children.Count == 0)
                            {
                                if (List.IgnoredJunkItems.Contains(acfFile.FullName))
                                {
                                    return;
                                }

                                List.LcProgress.Report(new List.JunkInfo
                                {
                                    FSInfo = new FileInfo(acfFile.FullName),
                                    Size = Functions.FileSystem.FormatBytes(acfFile.Length),
                                    Library = this,
                                    Tag = JunkType.CorruptedDataFile
                                });

                                return;
                            }

                            await Functions.App.AddSteamAppAsync(Convert.ToInt32(keyValReader["appid"].Value),
                                keyValReader["name"].Value ?? keyValReader["UserConfig"]["name"].Value,
                                keyValReader["installdir"].Value, Convert.ToInt32(keyValReader["StateFlags"].Value),
                                this, Convert.ToInt64(keyValReader["SizeOnDisk"].Value),
                                Convert.ToInt64(keyValReader["LastUpdated"].Value), false).ConfigureAwait(false);
                        }).ConfigureAwait(false);

                // Do a loop for each *.zip file in library
                await Directory.EnumerateFiles(DirectoryList["SteamApps"].FullName, "*.zip", SearchOption.TopDirectoryOnly)
                    .ParallelForEachAsync(async archive => { await Task.Run(() => Functions.App.ReadDetailsFromZip(archive, this)).ConfigureAwait(false); }).ConfigureAwait(false);

                DirectoryList["SteamBackups"].Refresh();
                if (Type == LibraryType.SLM && DirectoryList["SteamBackups"].Exists)
                {
                    await DirectoryList["SteamBackups"].EnumerateFiles("*.sis", SearchOption.AllDirectories).ParallelForEachAsync(
                        async skuFile =>
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

                                await Functions.App.AddSteamAppAsync(Convert.ToInt32(app.Value), appNames[i],
                                    skuFile.DirectoryName, 4, this, appSize, skuFile.LastWriteTimeUtc.ToUnixTimestamp(),
                                    false, true).ConfigureAwait(false);

                                if (appNames.Length > 1)
                                    i++;
                            }
                        }).ConfigureAwait(false);
                }

                if (SLM.CurrentSelectedLibrary != null && SLM.CurrentSelectedLibrary == this)
                {
                    Functions.App.UpdateAppPanel(this);
                }

                IsUpdatingAppList = false;
            }
            catch (UnauthorizedAccessException ex)
            {
                await Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(
                    async delegate
                    {
                        await Main.FormAccessor.ShowMessageAsync(
                            Functions.SLM.Translate(nameof(Properties.Resources.UnauthorizedAccessException)),
                            Framework.StringFormat.Format(
                                Functions.SLM.Translate(nameof(Properties.Resources.UnauthorizedAccessExceptionMessage)),
                                new { FullPath, ExceptionMessage = ex.Message })).ConfigureAwait(true);
                    }, System.Windows.Threading.DispatcherPriority.Normal).ConfigureAwait(true);
                IsUpdatingAppList = false;
            }
            catch (DirectoryNotFoundException ex)
            {
                await Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(
                    async delegate
                    {
                        await Main.FormAccessor.ShowMessageAsync(
                                Functions.SLM.Translate(nameof(Properties.Resources.DirectoryNotFoundException)),
                                Framework.StringFormat.Format(
                                    Functions.SLM.Translate(nameof(Properties.Resources.DirectoryNotFoundExceptionMessage)),
                                    new { FolderfullPath = FullPath, ExceptionMessage = ex.Message }))
                            .ConfigureAwait(true);
                    }, System.Windows.Threading.DispatcherPriority.Normal).ConfigureAwait(true);
                IsUpdatingAppList = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Logger.Fatal(ex);
                IsUpdatingAppList = false;
            }
        }

        public override async void ParseMenuItemActionAsync(string action)
        {
            switch (action.ToLowerInvariant())
            {
                // Opens game installation path in explorer
                case "disk":
                    if (DirectoryList["SteamApps"].Exists)
                    {
                        Process.Start(DirectoryList["SteamApps"].FullName);
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

                case "removefromlist":
                    if (Functions.TaskManager.TaskList.Count(x => x.TargetLibrary == this || x.App?.Library == this) == 0)
                    {
                        RemoveLibraryAsync(false);
                    }
                    else
                    {
                        await Main.FormAccessor.ShowMessageAsync("Library is in use", "You have to remove the tasks related to this library before removing it from SLM.", MessageDialogStyle.Affirmative).ConfigureAwait(true);
                    }
                    break;
            }
        }

        public async void UpdateLibraryPathAsync(string NewLibraryPath)
        {
            try
            {
                await Functions.Steam.CloseSteamAsync().ConfigureAwait(true);

                // Make a KeyValue reader
                var Key = new Framework.KeyValue();

                // Read vdf file
                Key.ReadFileAsText(Global.Steam.VdfFilePath);

                // Change old library path with new one
                Key["Software"]["Valve"]["Steam"].Children.Find(key => key.Value.Contains(FullPath)).Value = NewLibraryPath;

                // Update config.vdf file with changes
                Key.SaveToFile(Global.Steam.VdfFilePath, false);

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
                if (DirectoryList["SteamApps"].Exists)
                {
                    await Task.Run(() => DirectoryList["SteamApps"].Delete(true)).ConfigureAwait(true);
                }

                if (DirectoryList["Workshop"].Exists)
                {
                    await Task.Run(() => DirectoryList["Workshop"].Delete(true)).ConfigureAwait(true);
                }

                if (DirectoryList["Download"].Exists)
                {
                    await Task.Run(() => DirectoryList["Download"].Delete(true)).ConfigureAwait(true);
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

                List.Libraries.Remove(this);

                if (Type != LibraryType.Steam) return;

                await Functions.Steam.CloseSteamAsync().ConfigureAwait(true);

                // Make a KeyValue reader
                var keyValReader = new Framework.KeyValue();

                // Read vdf file
                keyValReader.ReadFileAsText(Global.Steam.VdfFilePath);

                // Remove old library
                keyValReader["Software"]["Valve"]["Steam"].Children.RemoveAll(x => x.Value == FullPath);

                var i = 1;
                foreach (var key in keyValReader["Software"]["Valve"]["Steam"].Children.FindAll(x => x.Name.Contains("BaseInstallFolder")))
                {
                    key.Name = $"BaseInstallFolder_{i}";
                    i++;
                }

                // Update libraryFolders.vdf file with changes
                keyValReader.SaveToFile(Global.Steam.VdfFilePath, false);

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

        public override void UpdateJunks()
        {
            try
            {
                while (IsUpdatingAppList)
                {
                    Task.Delay(5000);
                }

                DirectoryList["Common"].Refresh();
                if (DirectoryList["Common"].Exists)
                {
                    foreach (var dirInfo in DirectoryList["Common"].GetDirectories().ToList().Where(
                        x => Apps.Count(y => string.Equals(y.InstallationDirectory.Name, x.Name, StringComparison.InvariantCultureIgnoreCase)) == 0
                        && x.Name != "241100" // Steam controller configs
                        && Functions.TaskManager.TaskList.Count(
                            z => string.Equals(z.App.InstallationDirectory.Name, x.Name, StringComparison.InvariantCultureIgnoreCase)
                            && z.TargetLibrary == this
                            ) == 0
                        ).OrderByDescending(x => Functions.FileSystem.GetDirectorySize(x, true)))
                    {
                        var junk = new List.JunkInfo
                        {
                            FSInfo = dirInfo,
                            Size = Functions.FileSystem.FormatBytes(Functions.FileSystem.GetDirectorySize(dirInfo, true)),
                            Library = this,
                            Tag = JunkType.HeadlessFolder
                        };

                        if (List.JunkItems.Count(x => x.FSInfo.FullName == junk.FSInfo.FullName) == 0)
                        {
                            if (List.IgnoredJunkItems.Contains(dirInfo.FullName))
                            {
                                continue;
                            }

                            List.LcProgress.Report(junk);
                        }
                    }
                }

                DirectoryList["Workshop"].Refresh();
                if (DirectoryList["Workshop"].Exists)
                {
                    if (Directory.Exists(Path.Combine(DirectoryList["Workshop"].FullName, "downloads")))
                    {
                        foreach (var fileDetails in new DirectoryInfo(Path.Combine(DirectoryList["Workshop"].FullName, "downloads")).EnumerateFiles("*.patch", SearchOption.TopDirectoryOnly).ToList().Where(
                            x => Apps.Count(y => x.Name.Contains($"state_{y.AppId}_")) == 0
                            ))
                        {
                            var junk = new List.JunkInfo
                            {
                                FSInfo = fileDetails,
                                Size = Functions.FileSystem.FormatBytes(fileDetails.Length),
                                Library = this,
                                Tag = JunkType.HeadlessWorkshopFolder
                            };

                            if (List.JunkItems.Count(x => x.FSInfo.FullName == junk.FSInfo.FullName) == 0)
                            {
                                if (List.IgnoredJunkItems.Contains(fileDetails.FullName))
                                {
                                    continue;
                                }
                                List.LcProgress.Report(junk);
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

        public override void UpdateDupes()
        {
            try
            {
                while (IsUpdatingAppList)
                {
                    Task.Delay(5000);
                }

                foreach (var library in List.Libraries.Where(x => x.Type == LibraryType.Steam && x != this))
                {
                    foreach (var targetApp in library.Apps.Where(x => !x.IsCompressed))
                    {
                        foreach (var currentApp in Apps.Where(x => !x.IsCompressed && x.AppId == targetApp.AppId))
                        {
                            if (List.DupeItems.Count(x => (x.App1 == currentApp && x.App2 == targetApp) || x.App1 == targetApp && x.App2 == currentApp) == 0)
                            {
                                List.DupeItems.Add(new List.DupeInfo()
                                {
                                    App1 = currentApp,
                                    App2 = targetApp,
                                    Size = targetApp.PrettyGameSize,
                                });
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