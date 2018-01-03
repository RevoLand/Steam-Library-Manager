using MahApps.Metro.Controls.Dialogs;
using System;
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
        public Library Library => List.Libraries.First(x => x.Steam == this);

        //private FileSystemWatcher SteamFolderWD;
        //private FileSystemWatcher SLMFolderWD;

        public bool IsMain { get; set; }

        public DirectoryInfo SteamAppsFolder => new DirectoryInfo(Path.Combine(FullPath, "SteamApps"));

        public DirectoryInfo SteamBackupsFolder => new DirectoryInfo(Path.Combine(FullPath, "SteamBackups"));

        public DirectoryInfo CommonFolder => new DirectoryInfo(Path.Combine(SteamAppsFolder.FullName, "common"));

        public DirectoryInfo DownloadFolder => new DirectoryInfo(Path.Combine(SteamAppsFolder.FullName, "downloading"));

        public DirectoryInfo WorkshopFolder => new DirectoryInfo(Path.Combine(SteamAppsFolder.FullName, "workshop"));

        public Framework.AsyncObservableCollection<FrameworkElement> ContextMenu => GenerateCMenuItems();

        public string FullPath { get; set; }
        public Framework.AsyncObservableCollection<AppInfo> Apps { get; set; } = new Framework.AsyncObservableCollection<AppInfo>();

        public void UpdateDiskDetails()
        {
            OnPropertyChanged("FreeSpace");
            OnPropertyChanged("PrettyFreeSpace");
            OnPropertyChanged("FreeSpacePerc");
        }

        public void UpdateAppList()
        {
            try
            {
                SteamAppsFolder.Refresh();

                if (!SteamAppsFolder.Exists)
                {
                    SteamAppsFolder.Create();
                }

                if (Apps.Count > 0)
                {
                    Apps.Clear();
                }

                // Foreach *.acf file found in library
                Parallel.ForEach(SteamAppsFolder.EnumerateFiles("appmanifest_*.acf", SearchOption.TopDirectoryOnly), AcfFile =>
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
                Parallel.ForEach(Directory.EnumerateFiles(SteamAppsFolder.FullName, "*.zip", SearchOption.TopDirectoryOnly), ArchiveFile =>
                {
                    Functions.App.ReadDetailsFromZip(ArchiveFile, Library);
                });

                if (Library.Type == Enums.LibraryType.SLM && SteamBackupsFolder.Exists)
                {
                    foreach (FileInfo SkuFile in SteamBackupsFolder.EnumerateFiles("*.sis", SearchOption.AllDirectories))
                    {
                        Framework.KeyValue KeyValReader = new Framework.KeyValue();

                        KeyValReader.ReadFileAsText(SkuFile.FullName);

                        string[] AppNames = System.Text.RegularExpressions.Regex.Split(KeyValReader["name"].Value, " and ");

                        int i = 0;
                        long AppSize = Functions.FileSystem.GetDirectorySize(SkuFile.Directory, true);
                        foreach (Framework.KeyValue App in KeyValReader["apps"].Children)
                        {
                            if (Apps.Count(x => x.AppID == Convert.ToInt32(App.Value)) > 0)
                                continue;

                            Functions.App.AddSteamApp(Convert.ToInt32(App.Value), AppNames[i], SkuFile.DirectoryName, Library, AppSize, SkuFile.LastWriteTimeUtc.ToUnixTimestamp(), false, true);

                            if (AppNames.Count() > 1)
                                i++;
                        }
                    }
                }

                if (SLM.CurrentSelectedLibrary != null)
                {
                    if (SLM.CurrentSelectedLibrary == Library)
                    {
                        Functions.App.UpdateAppPanel(Library);
                    }
                }

                /*
                SteamFolderWD = new FileSystemWatcher()
                {
                    Path = SteamAppsFolder.FullName,
                    Filter = "appmanifest_*.acf",
                    EnableRaisingEvents = true,
                };

                SteamFolderWD.Created += FolderWD_Created;
                SteamFolderWD.Changed += FolderWD_Changed;
                SteamFolderWD.Deleted += FolderWD_Deleted;

                if (Library.Type == Enums.LibraryType.SLM)
                {
                    SLMFolderWD = new FileSystemWatcher()
                    {
                        Path = SteamAppsFolder.FullName,
                        Filter = "*.zip",
                        EnableRaisingEvents = true
                    };

                    SLMFolderWD.Created += SLMFolderWD_Created;
                    SLMFolderWD.Renamed += SLMFolderWD_Renamed;
                    SLMFolderWD.Deleted += SLMFolderWD_Deleted;
                }
                */
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Functions.Logger.LogToFile(Functions.Logger.LogType.Library, ex.ToString());
                SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
            }
        }

        private void SLMFolderWD_Created(object sender, FileSystemEventArgs e)
        {
            try
            {
                Functions.App.ReadDetailsFromZip(e.FullPath, Library);
            }
            catch (Exception ex)
            {
                Functions.Logger.LogToFile(Functions.Logger.LogType.Library, ex.ToString());
            }
        }

        private void SLMFolderWD_Renamed(object sender, RenamedEventArgs e)
        {
            try
            {
                if (e.Name.EndsWith(".zip"))
                {
                    Functions.App.ReadDetailsFromZip(e.FullPath, Library);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                Functions.Logger.LogToFile(Functions.Logger.LogType.Library, ex.ToString());
            }
        }

        private void SLMFolderWD_Deleted(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (Apps.Count(x => x.AppID.ToString() == e.Name.Replace(".zip", "") && x.IsCompressed) > 0)
                {
                    Apps.Remove(Apps.First(x => x.AppID.ToString() == e.Name.Replace(".zip", "") && x.IsCompressed));

                    if (SLM.CurrentSelectedLibrary.Steam == this)
                    {
                        Functions.App.UpdateAppPanel(SLM.CurrentSelectedLibrary);
                    }

                    UpdateLibraryVisual();
                }
            }
            catch (Exception ex)
            {
                Functions.Logger.LogToFile(Functions.Logger.LogType.Library, ex.ToString());
            }
        }

        private void FolderWD_Created(object sender, FileSystemEventArgs e)
        {
            try
            {
                // Define a new value and call KeyValue
                Framework.KeyValue KeyValReader = new Framework.KeyValue();

                // Read the *.acf file as text
                KeyValReader.ReadFileAsText(e.FullPath);

                // If key doesn't contains a child (value in acf file)
                if (KeyValReader.Children.Count == 0)
                {
                    List.LCItems.Add(new List.JunkInfo
                    {
                        FSInfo = new FileInfo(e.FullPath),
                        Library = Library
                    });

                    return;
                }

                if (Apps.Count(x => x.AppID == Convert.ToInt32(KeyValReader["appID"].Value)) > 0)
                {
                    return;
                }

                Functions.App.AddSteamApp(Convert.ToInt32(KeyValReader["appID"].Value), KeyValReader["name"].Value ?? KeyValReader["UserConfig"]["name"].Value, KeyValReader["installdir"].Value, Library, Convert.ToInt64(KeyValReader["SizeOnDisk"].Value), Convert.ToInt64(KeyValReader["LastUpdated"].Value), false);

                if (SLM.CurrentSelectedLibrary.Steam == this)
                {
                    Functions.App.UpdateAppPanel(SLM.CurrentSelectedLibrary);
                }

                UpdateLibraryVisual();
            }
            catch (FormatException FormatEx)
            {
                Debug.WriteLine(FormatEx);
                Functions.Logger.LogToFile(Functions.Logger.LogType.Library, FormatEx.ToString());
            }
            catch (Exception Ex)
            {
                Functions.Logger.LogToFile(Functions.Logger.LogType.Library, Ex.ToString());
            }
        }

        private void FolderWD_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                FolderWD_Deleted(sender, e);
                FolderWD_Created(sender, e);
            }
            catch (Exception ex)
            {
                Functions.Logger.LogToFile(Functions.Logger.LogType.Library, ex.ToString());
            }
        }

        private void FolderWD_Deleted(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (Apps.Count(x => x.AcfName == e.Name) > 0)
                {
                    AppInfo RemovedApp = Apps.First(x => x.AcfName == e.Name);
                    Apps.Remove(RemovedApp);

                    if (SLM.CurrentSelectedLibrary.Steam == this)
                    {
                        Functions.App.UpdateAppPanel(SLM.CurrentSelectedLibrary);
                    }

                    UpdateLibraryVisual();
                }
            }
            catch (Exception ex)
            {
                Functions.Logger.LogToFile(Functions.Logger.LogType.Library, ex.ToString());
            }
        }

        public Framework.AsyncObservableCollection<FrameworkElement> GenerateCMenuItems()
        {
            Framework.AsyncObservableCollection<FrameworkElement> CMenu = new Framework.AsyncObservableCollection<FrameworkElement>();
            try
            {
                foreach (ContextMenuItem CMenuItem in List.LibraryCMenuItems.Where(x => x.IsActive))
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
                            Header = string.Format(CMenuItem.Header, FullPath, Library.PrettyFreeSpace),
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
                MessageBox.Show($"An error happened while parsing context menu, most likely happened duo typo on color name.\n\n{ex}");

                Functions.Logger.LogToFile(Functions.Logger.LogType.Library, ex.ToString());
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
                        await Main.FormAccessor.ShowMessageAsync("Steam Library Manager", "You can't remove the main library of Steam, can you? Never tested tbh. TODO: TEST!", MessageDialogStyle.Affirmative);
                        return;
                    }

                    MessageDialogResult MoveAppsBeforeDeletion = await Main.FormAccessor.ShowMessageAsync("Steam Library Manager", "Move apps in Library before deleting the library?", MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, new MetroDialogSettings
                    {
                        FirstAuxiliaryButtonText = "Delete library without moving apps"
                    });

                    if (MoveAppsBeforeDeletion == MessageDialogResult.Affirmative)
                    {
                        await Main.FormAccessor.ShowMessageAsync("Steam Library Manager", "Function is not implemented, process cancelled", MessageDialogStyle.Affirmative);
                    }
                    else if (MoveAppsBeforeDeletion == MessageDialogResult.FirstAuxiliary)
                    {
                        RemoveLibraryAsync(true);
                    }

                    break;
                case "deletelibraryslm":

                    foreach (AppInfo App in Apps.ToList())
                    {
                        if (!await App.DeleteFilesAsync())
                        {
                            await Main.FormAccessor.ShowMessageAsync("Steam Library Manager", $"An unknown error happened while removing app files. {FullPath}", MessageDialogStyle.Affirmative);

                            return;
                        }
                    }

                    UpdateLibraryVisual();

                    await Main.FormAccessor.ShowMessageAsync("Steam Library Manager", $"All app files in library successfully removed.\n\nLibrary: {FullPath}", MessageDialogStyle.Affirmative);
                    break;
            }
        }

        public void UpdateLibraryVisual()
        {
            try
            {
                Parallel.ForEach(List.Libraries.Where(x => x.Type == Enums.LibraryType.Steam && x.Steam.SteamAppsFolder.Root.FullName.ToLowerInvariant() == SteamAppsFolder.Root.FullName.ToLowerInvariant()), LibraryToUpdate =>
                {
                    LibraryToUpdate.Steam.UpdateDiskDetails();
                });
            }
            catch (Exception ex)
            {
                Functions.Logger.LogToFile(Functions.Logger.LogType.Library, ex.ToString());
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
                Functions.Logger.LogToFile(Functions.Logger.LogType.Library, ex.ToString());
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
            catch (Exception Ex)
            {
                Functions.Logger.LogToFile(Functions.Logger.LogType.SLM, Ex.ToString());
                SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(Ex));
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
                Functions.Logger.LogToFile(Functions.Logger.LogType.Library, ex.ToString());
                SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
            }
        }

        public void UpdateJunks()
        {
            try
            {
                if (CommonFolder.Exists)
                {
                    foreach (DirectoryInfo DirInfo in CommonFolder.GetDirectories().Where(
                        x => Apps.Count(y => y.InstallationPath.Name.ToLowerInvariant() == x.Name.ToLowerInvariant()) == 0
                        && x.Name != "241100" // Steam controller configs
                        && Framework.TaskManager.TaskList.Count(
                            z => z.App.InstallationPath.Name.ToLowerInvariant() == x.Name.ToLowerInvariant()
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


                        if (List.LCItems.Count(x => x.FSInfo.FullName == Junk.FSInfo.FullName) == 0)
                        {
                            List.LCItems.Add(Junk);
                        }
                    }
                }

                if (WorkshopFolder.Exists)
                {
                    foreach (FileInfo FileDetails in WorkshopFolder.EnumerateFiles("*.acf", SearchOption.TopDirectoryOnly).Where(
                        x => Apps.Count(y => x.Name == y.WorkShopAcfName) == 0
                        && x.Name.ToLowerInvariant() != "appworkshop_241100.acf" // Steam Controller Configs
                        && Framework.TaskManager.TaskList.Count(
                            z => z.App.WorkShopPath.Name.ToLowerInvariant() == x.Name.ToLowerInvariant()
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


                        if (List.LCItems.Count(x => x.FSInfo.FullName == Junk.FSInfo.FullName) == 0)
                        {
                            List.LCItems.Add(Junk);
                        }
                    }

                    if (Directory.Exists(Path.Combine(WorkshopFolder.FullName, "content")))
                    {
                        foreach (DirectoryInfo DirInfo in new DirectoryInfo(Path.Combine(WorkshopFolder.FullName, "content")).GetDirectories().Where(
                            x => Apps.Count(y => y.AppID.ToString() == x.Name) == 0
                            && x.Name != "241100" // Steam controller configs
                            ).OrderByDescending(x => Functions.FileSystem.GetDirectorySize(x, true)))
                        {
                            List.JunkInfo Junk = new List.JunkInfo
                            {
                                FSInfo = DirInfo,
                                Size = Functions.FileSystem.GetDirectorySize(DirInfo, true),
                                Library = Library
                            };


                            if (List.LCItems.Count(x => x.FSInfo.FullName == Junk.FSInfo.FullName) == 0)
                            {
                                List.LCItems.Add(Junk);
                            }
                        }
                    }

                    if (Directory.Exists(Path.Combine(WorkshopFolder.FullName, "downloads")))
                    {
                        foreach (FileInfo FileDetails in new DirectoryInfo(Path.Combine(WorkshopFolder.FullName, "downloads")).EnumerateFiles("*.patch", SearchOption.TopDirectoryOnly).Where(
                            x => Apps.Count(y => x.Name.Contains($"state_{y.AppID}_")) == 0
                            ))
                        {
                            List.JunkInfo Junk = new List.JunkInfo
                            {
                                FSInfo = FileDetails,
                                Size = FileDetails.Length,
                                Library = Library
                            };


                            if (List.LCItems.Count(x => x.FSInfo.FullName == Junk.FSInfo.FullName) == 0)
                            {
                                List.LCItems.Add(Junk);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Functions.Logger.LogToFile(Functions.Logger.LogType.Library, ex.ToString());
                SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
    }
}
