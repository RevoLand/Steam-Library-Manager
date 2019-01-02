using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Steam_Library_Manager.Definitions
{
    public class SteamLibrary : INotifyPropertyChanged
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public Library Library => List.Libraries.ToList().First(x => x.Steam == this);

        public bool IsMain { get; set; }

        public DirectoryInfo SteamAppsFolder => new DirectoryInfo(Path.Combine(FullPath, "SteamApps"));

        public DirectoryInfo SteamBackupsFolder => new DirectoryInfo(Path.Combine(FullPath, "SteamBackups"));

        public DirectoryInfo CommonFolder => new DirectoryInfo(Path.Combine(SteamAppsFolder.FullName, "common"));

        public DirectoryInfo DownloadFolder => new DirectoryInfo(Path.Combine(SteamAppsFolder.FullName, "downloading"));

        public DirectoryInfo WorkshopFolder => new DirectoryInfo(Path.Combine(SteamAppsFolder.FullName, "workshop"));

        public List<FrameworkElement> ContextMenu => GenerateCMenuItems();

        public string FullPath { get; set; }
        public Framework.AsyncObservableCollection<SteamAppInfo> Apps { get; set; } = new Framework.AsyncObservableCollection<SteamAppInfo>();

        public SteamLibrary(string fullPath, bool _IsMain = false)
        {
            FullPath = fullPath;
            IsMain = _IsMain;
        }

        public void UpdateAppList()
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
                        MessageBox.Show(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.SteamLibrary_CantCreate)), new { SteamAppsFolderFullPath =  SteamAppsFolder.FullName }));
                        return;
                    }
                }

                if (Apps.Count > 0)
                {
                    Apps.Clear();
                }

                // Foreach *.acf file found in library
                Parallel.ForEach(SteamAppsFolder.EnumerateFiles("appmanifest_*.acf", SearchOption.TopDirectoryOnly).ToList(), AcfFile =>
                {
                    // Define a new value and call KeyValue
                    Framework.KeyValue KeyValReader = new Framework.KeyValue();

                    // Read the *.acf file as text
                    KeyValReader.ReadFileAsText(AcfFile.FullName);

                    // If key doesn't contains a child (value in acf file)
                    if (KeyValReader.Children.Count == 0)
                    {
                        List.LCItems.Add(new List.JunkInfo
                        {
                            FSInfo = new FileInfo(AcfFile.FullName),
                            Size = AcfFile.Length,
                            Library = Library
                        });

                        return;
                    }

                    Functions.App.AddSteamApp(Convert.ToInt32(KeyValReader["appID"].Value), KeyValReader["name"].Value ?? KeyValReader["UserConfig"]["name"].Value, KeyValReader["installdir"].Value, Library, Convert.ToInt64(KeyValReader["SizeOnDisk"].Value), Convert.ToInt64(KeyValReader["LastUpdated"].Value), false);
                });

                // Do a loop for each *.zip file in library
                Parallel.ForEach(Directory.EnumerateFiles(SteamAppsFolder.FullName, "*.zip", SearchOption.TopDirectoryOnly).ToList(), ArchiveFile => Functions.App.ReadDetailsFromZip(ArchiveFile, Library));

                SteamBackupsFolder.Refresh();
                if (Library.Type == Enums.LibraryType.SLM && SteamBackupsFolder.Exists)
                {
                    foreach (FileInfo SkuFile in SteamBackupsFolder.EnumerateFiles("*.sis", SearchOption.AllDirectories).ToList())
                    {
                        Framework.KeyValue KeyValReader = new Framework.KeyValue();

                        KeyValReader.ReadFileAsText(SkuFile.FullName);

                        string[] AppNames = System.Text.RegularExpressions.Regex.Split(KeyValReader["name"].Value, " and ");

                        int i = 0;
                        long AppSize = Functions.FileSystem.GetDirectorySize(SkuFile.Directory, true);
                        foreach (Framework.KeyValue App in KeyValReader["apps"].Children)
                        {
                            if (Apps.Count(x => x.AppID == Convert.ToInt32(App.Value) && x.IsSteamBackup) > 0)
                                continue;

                            Functions.App.AddSteamApp(Convert.ToInt32(App.Value), AppNames[i], SkuFile.DirectoryName, Library, AppSize, SkuFile.LastWriteTimeUtc.ToUnixTimestamp(), false, true);

                            if (AppNames.Length > 1)
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
                Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                {
                    await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.UnauthorizedAccessException)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.UnauthorizedAccessExceptionMessage)), new { FullPath, ExceptionMessage = uaex.Message }));
                }, System.Windows.Threading.DispatcherPriority.Normal);
            }
            catch (DirectoryNotFoundException dnfex)
            {
                Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                {
                    await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.DirectoryNotFoundException)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.DirectoryNotFoundExceptionMessage)), new { FolderfullPath = FullPath, ExceptionMessage = dnfex.Message }));
                }, System.Windows.Threading.DispatcherPriority.Normal);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                logger.Fatal(ex);
                SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
            }
        }

        public List<FrameworkElement> GenerateCMenuItems()
        {
            var CMenu = new List<FrameworkElement>();
            try
            {
                foreach (ContextMenuItem CMenuItem in List.LibraryCMenuItems.ToList().Where(x => x.IsActive && x.LibraryType == Enums.LibraryType.Steam))
                {
                    if (!CMenuItem.ShowToNormal)
                    {
                        continue;
                    }

                    if (CMenuItem.IsSeparator)
                    {
                        CMenu.Add(new Separator());
                    }
                    else
                    {
                        MenuItem SLMItem = new MenuItem()
                        {
                            Tag = CMenuItem.Action,
                            Header = Framework.StringFormat.Format(CMenuItem.Header, new { LibraryFullPath = Library.DirectoryInfo.FullName, FreeDiskSpace = Library.PrettyFreeSpace }),
                            Icon = Functions.FAwesome.GetAwesomeIcon(CMenuItem.Icon, CMenuItem.IconColor),
                            HorizontalContentAlignment = HorizontalAlignment.Left,
                            VerticalContentAlignment = VerticalAlignment.Center
                        };

                        SLMItem.Click += Main.FormAccessor.LibraryCMenuItem_Click;

                        CMenu.Add(SLMItem);
                    }
                }

                return CMenu;
            }
            catch (FormatException ex)
            {
                MessageBox.Show(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.SteamAppInfo_FormatException)), new { ExceptionMessage = ex.Message }));
                return CMenu;
            }
        }

        public async void ParseMenuItemActionAsync(string Action)
        {
            switch (Action.ToLowerInvariant())
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
                        await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.DeleteMainSteamLibrary)), Functions.SLM.Translate(nameof(Properties.Resources.DeleteMainSteamLibraryMessage)), MessageDialogStyle.Affirmative);
                        return;
                    }

                    MessageDialogResult MoveGamesBeforeDeletion = await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.MoveGamesInLibrary)), Functions.SLM.Translate(nameof(Properties.Resources.MoveGamesInLibraryMessage)), MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, new MetroDialogSettings
                    {
                        FirstAuxiliaryButtonText = Functions.SLM.Translate(nameof(Properties.Resources.DeleteLibraryWithoutMovingGames))
                    });

                    if (MoveGamesBeforeDeletion == MessageDialogResult.Affirmative)
                    {
                        await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.MoveGamesConfirmError)), Functions.SLM.Translate(nameof(Properties.Resources.MoveGamesConfirmErrorMessage)), MessageDialogStyle.Affirmative);
                    }
                    else if (MoveGamesBeforeDeletion == MessageDialogResult.FirstAuxiliary)
                    {
                        RemoveLibraryAsync(true);
                    }

                    break;

                case "deletelibraryslm":

                    foreach (SteamAppInfo App in Apps.ToList())
                    {
                        if (!await App.DeleteFilesAsync())
                        {
                            await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.SteamApp_RemovingError)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.SteamApp_RemovingErrorMessage)), new { FullPath }), MessageDialogStyle.Affirmative);

                            return;
                        }
                    }

                    Functions.SLM.Library.UpdateLibraryVisual();

                    await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.DeleteSteamLibrary)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.DeleteSteamLibraryMessage)), new { LibraryFullPath = FullPath }), MessageDialogStyle.Affirmative);
                    break;
            }
        }

        public async void UpdateLibraryPathAsync(string NewLibraryPath)
        {
            try
            {
                await Functions.Steam.CloseSteamAsync();

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
                logger.Fatal(ex);
                SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
            }
        }

        public async void DeleteFilesAsync()
        {
            try
            {
                if (SteamAppsFolder.Exists)
                {
                    await Task.Run(() => SteamAppsFolder.Delete(true));
                }

                if (WorkshopFolder.Exists)
                {
                    await Task.Run(() => WorkshopFolder.Delete(true));
                }

                if (DownloadFolder.Exists)
                {
                    await Task.Run(() => DownloadFolder.Delete(true));
                }
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
                SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
            }
        }

        public async void RemoveLibraryAsync(bool ShouldDeleteFiles)
        {
            try
            {
                if (ShouldDeleteFiles)
                {
                    DeleteFilesAsync();
                }

                List.Libraries.Remove(List.Libraries.First(x => x == Library));

                await Functions.Steam.CloseSteamAsync();

                // Make a KeyValue reader
                Framework.KeyValue KeyValReader = new Framework.KeyValue();

                // Read vdf file
                KeyValReader.ReadFileAsText(Global.Steam.vdfFilePath);

                // Remove old library
                KeyValReader["Software"]["Valve"]["Steam"].Children.RemoveAll(x => x.Value == FullPath);

                int i = 1;
                foreach (Framework.KeyValue Key in KeyValReader["Software"]["Valve"]["Steam"].Children.FindAll(x => x.Name.Contains("BaseInstallFolder")))
                {
                    Key.Name = $"BaseInstallFolder_{i}";
                    i++;
                }

                // Update libraryFolders.vdf file with changes
                KeyValReader.SaveToFile(Global.Steam.vdfFilePath, false);

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
                logger.Fatal(ex);
                SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
            }
        }

        public void UpdateJunks()
        {
            try
            {
                CommonFolder.Refresh();
                if (CommonFolder.Exists)
                {
                    foreach (DirectoryInfo DirInfo in CommonFolder.GetDirectories().ToList().Where(
                        x => Apps.Count(y => string.Equals(y.InstallationDirectory.Name, x.Name, StringComparison.InvariantCultureIgnoreCase)) == 0
                        && x.Name != "241100" // Steam controller configs
                        && Framework.TaskManager.TaskList.Count(
                            z => string.Equals(z.SteamApp.InstallationDirectory.Name, x.Name, StringComparison.InvariantCultureIgnoreCase)
                            && z.TargetLibrary == Library
                            ) == 0
                        ).OrderByDescending(x => Functions.FileSystem.GetDirectorySize(x, true)))
                    {
                        List.JunkInfo Junk = new List.JunkInfo
                        {
                            FSInfo = DirInfo,
                            Size = Functions.FileSystem.GetDirectorySize(DirInfo, true),
                            Library = Library
                        };

                        if (List.LCItems.ToList().Count(x => x.FSInfo.FullName == Junk.FSInfo.FullName) == 0)
                        {
                            List.LCItems.Add(Junk);
                        }
                    }
                }

                WorkshopFolder.Refresh();
                if (WorkshopFolder.Exists)
                {
                    foreach (FileInfo FileDetails in WorkshopFolder.EnumerateFiles("appworkshop_*.acf", SearchOption.TopDirectoryOnly).ToList().Where(
                        x => Apps.Count(y => x.Name == y.WorkShopAcfName) == 0
                        && !string.Equals(x.Name, "appworkshop_241100.acf" // Steam Controller Configs
, StringComparison.InvariantCultureIgnoreCase) // Steam Controller Configs
                        && Framework.TaskManager.TaskList.Count(
                            z => string.Equals(z.SteamApp.WorkShopAcfName, x.Name
, StringComparison.InvariantCultureIgnoreCase)
                            && z.TargetLibrary == Library
                            ) == 0
                        ))
                    {
                        List.JunkInfo Junk = new List.JunkInfo
                        {
                            FSInfo = FileDetails,
                            Size = FileDetails.Length,
                            Library = Library
                        };

                        if (List.LCItems.ToList().Count(x => x.FSInfo.FullName == Junk.FSInfo.FullName) == 0)
                        {
                            List.LCItems.Add(Junk);
                        }
                    }

                    if (Directory.Exists(Path.Combine(WorkshopFolder.FullName, "content")))
                    {
                        foreach (DirectoryInfo DirInfo in new DirectoryInfo(Path.Combine(WorkshopFolder.FullName, "content")).GetDirectories().ToList().Where(
                            x => Apps.Count(y => y.AppID.ToString() == x.Name) == 0
                            && x.Name != "241100" // Steam controller configs
                            && Framework.TaskManager.TaskList.Count(
                                z => string.Equals(z.SteamApp.WorkShopPath.Name, x.Name
, StringComparison.InvariantCultureIgnoreCase)
                                && z.TargetLibrary == Library
                            ) == 0
                            ).OrderByDescending(x => Functions.FileSystem.GetDirectorySize(x, true)))
                        {
                            List.JunkInfo Junk = new List.JunkInfo
                            {
                                FSInfo = DirInfo,
                                Size = Functions.FileSystem.GetDirectorySize(DirInfo, true),
                                Library = Library
                            };

                            if (List.LCItems.ToList().Count(x => x.FSInfo.FullName == Junk.FSInfo.FullName) == 0)
                            {
                                List.LCItems.Add(Junk);
                            }
                        }
                    }

                    if (Directory.Exists(Path.Combine(WorkshopFolder.FullName, "downloads")))
                    {
                        foreach (FileInfo FileDetails in new DirectoryInfo(Path.Combine(WorkshopFolder.FullName, "downloads")).EnumerateFiles("*.patch", SearchOption.TopDirectoryOnly).ToList().Where(
                            x => Apps.Count(y => x.Name.Contains($"state_{y.AppID}_")) == 0
                            ))
                        {
                            List.JunkInfo Junk = new List.JunkInfo
                            {
                                FSInfo = FileDetails,
                                Size = FileDetails.Length,
                                Library = Library
                            };

                            if (List.LCItems.ToList().Count(x => x.FSInfo.FullName == Junk.FSInfo.FullName) == 0)
                            {
                                List.LCItems.Add(Junk);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
                SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
    }
}