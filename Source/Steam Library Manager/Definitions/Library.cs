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
    public class Library : INotifyPropertyChanged
    {
        bool _Offline;
        FileSystemWatcher FolderWD;

        public bool IsMain { get; set; }
        public bool IsBackup { get; set; }

        public DirectoryInfo SteamAppsFolder
        {
            get => new DirectoryInfo(Path.Combine(FullPath, "SteamApps"));
        }

        public DirectoryInfo CommonFolder
        {
            get => new DirectoryInfo(Path.Combine(SteamAppsFolder.FullName, "common"));
        }

        public DirectoryInfo DownloadFolder
        {
            get => new DirectoryInfo(Path.Combine(SteamAppsFolder.FullName, "downloading"));
        }

        public DirectoryInfo WorkshopFolder
        {
            get => new DirectoryInfo(Path.Combine(SteamAppsFolder.FullName, "workshop"));
        }

        public Framework.AsyncObservableCollection<FrameworkElement> ContextMenu
        {
            get => GenerateRightClickMenuItems();
        }
        public string FullPath { get; set; }
        public Framework.AsyncObservableCollection<Game> Games { get; set; } = new Framework.AsyncObservableCollection<Game>();

        public bool IsOffline
        {
            get => _Offline;
            set
            {
                _Offline = value;
                OnPropertyChanged("IsOffline");
            }
        }

        public long FreeSpace
        {
            get => Functions.FileSystem.GetAvailableFreeSpace(FullPath);
            set
            {
                OnPropertyChanged("FreeSpace");
                OnPropertyChanged("PrettyFreeSpace");
            }
        }

        public string PrettyFreeSpace
        {
            get => Functions.FileSystem.FormatBytes(FreeSpace);
        }

        public int FreeSpacePerc
        {
            get => 100 - ((int)Math.Round((double)(100 * FreeSpace) / Functions.FileSystem.GetTotalSize(FullPath)));
            set
            {
                OnPropertyChanged("FreeSpacePerc");
            }
        }

        public void UpdateGameList()
        {
            try
            {
                if (!SteamAppsFolder.Exists)
                    SteamAppsFolder.Create();

                if (Games.Count > 0)
                    Games.Clear();

                // Foreach *.acf file found in library
                Parallel.ForEach(SteamAppsFolder.EnumerateFiles("appmanifest_*.acf", SearchOption.TopDirectoryOnly), AcfFile =>
                {
                    // Define a new value and call KeyValue
                    Framework.KeyValue Key = new Framework.KeyValue();

                    // Read the *.acf file as text
                    Key.ReadFileAsText(AcfFile.FullName);

                    // If key doesn't contains a child (value in acf file)
                    if (Key.Children.Count == 0)
                    {
                        List.JunkStuff.Add(new List.JunkInfo
                        {
                            FileSystemInfo = new FileInfo(AcfFile.FullName),
                            FolderSize = AcfFile.Length,
                            Library = this
                        });

                        return;
                    }

                    Functions.Games.AddNewGame(Convert.ToInt32(Key["appID"].Value), Key["name"].Value ?? Key["UserConfig"]["name"].Value, Key["installdir"].Value, this, Convert.ToInt64(Key["SizeOnDisk"].Value), Convert.ToInt64(Key["LastUpdated"].Value), false);
                });

                // Do a loop for each *.zip file in library
                Parallel.ForEach(Directory.EnumerateFiles(SteamAppsFolder.FullName, "*.zip", SearchOption.TopDirectoryOnly), gameArchive =>
                {
                    Functions.Games.ReadGameDetailsFromZip(gameArchive, this);
                });

                // If library is backup library
                if (IsBackup)
                {
                    foreach (string skuFile in Directory.EnumerateFiles(FullPath, "*.sis", SearchOption.AllDirectories))
                    {
                        Framework.KeyValue Key = new Framework.KeyValue();

                        Key.ReadFileAsText(skuFile);

                        string[] name = System.Text.RegularExpressions.Regex.Split(Key["name"].Value, " and ");

                        int i = 0;
                        long gameSize = Functions.FileSystem.GetDirectorySize(new DirectoryInfo(skuFile).Parent, true);
                        foreach (Framework.KeyValue app in Key["apps"].Children)
                        {
                            if (Games.Count(x => x.AppID == Convert.ToInt32(app.Value)) > 0)
                                continue;

                            Functions.Games.AddNewGame(Convert.ToInt32(app.Value), name[i], Path.GetDirectoryName(skuFile), this, gameSize, new FileInfo(skuFile).LastWriteTimeUtc.Ticks, false, true);

                            if (name.Count() > 1)
                                i++;
                        }
                    }
                }

                if (SLM.selectedLibrary == this)
                    Functions.Games.UpdateMainForm(this);

                FolderWD = new FileSystemWatcher()
                {
                    Path = SteamAppsFolder.FullName,
                    Filter = "appmanifest_*.acf",
                    EnableRaisingEvents = true
                };

                FolderWD.Created += FolderWD_Created;
                FolderWD.Deleted += FolderWD_Deleted;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Functions.Logger.LogToFile(Functions.Logger.LogType.Library, ex.ToString());
            }
        }

        private void FolderWD_Created(object sender, FileSystemEventArgs e)
        {
            try
            {
                // Define a new value and call KeyValue
                Framework.KeyValue Key = new Framework.KeyValue();

                // Read the *.acf file as text
                Key.ReadFileAsText(e.FullPath);

                // If key doesn't contains a child (value in acf file)
                if (Key.Children.Count == 0)
                {
                    List.JunkStuff.Add(new List.JunkInfo
                    {
                        FileSystemInfo = new FileInfo(e.FullPath),
                        Library = this
                    });

                    return;
                }

                if (Games.Count(x => x.AppID == Convert.ToInt32(Key["appID"].Value)) > 0)
                    return;

                Functions.Games.AddNewGame(Convert.ToInt32(Key["appID"].Value), Key["name"].Value ?? Key["UserConfig"]["name"].Value, Key["installdir"].Value, this, Convert.ToInt64(Key["SizeOnDisk"].Value), Convert.ToInt64(Key["LastUpdated"].Value), false);

                if (SLM.selectedLibrary == this)
                    Functions.Games.UpdateMainForm(this);

                UpdateLibraryVisual();
            }
            catch (FormatException fEx)
            {
                Debug.WriteLine(fEx);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Functions.Logger.LogToFile(Functions.Logger.LogType.Library, ex.ToString());
            }
        }

        private void FolderWD_Deleted(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (Games.Count(x => x.AcfName == e.Name) > 0)
                {
                    Games.Remove(Games.First(x => x.AcfName == e.Name));

                    if (SLM.selectedLibrary == this)
                        Functions.Games.UpdateMainForm(this);

                    UpdateLibraryVisual();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Functions.Logger.LogToFile(Functions.Logger.LogType.Library, ex.ToString());
            }
        }

        public Framework.AsyncObservableCollection<FrameworkElement> GenerateRightClickMenuItems()
        {
            Framework.AsyncObservableCollection<FrameworkElement> rightClickMenu = new Framework.AsyncObservableCollection<FrameworkElement>();
            try
            {
                foreach (ContextMenuItem cItem in List.LibraryCMenuItems.Where(x => x.IsActive))
                {
                    if (IsBackup && cItem.ShowToSLMBackup == Enums.MenuVisibility.NotVisible)
                        continue;
                    else if (!IsBackup && cItem.ShowToNormal == Enums.MenuVisibility.NotVisible)
                        continue;

                    if (cItem.IsSeparator)
                        rightClickMenu.Add(new Separator());
                    else
                    {
                        MenuItem slmItem = new MenuItem()
                        {
                            Tag = this,
                            Header = string.Format(cItem.Header, FullPath, PrettyFreeSpace)
                        };
                        slmItem.Tag = cItem.Action;
                        slmItem.Icon = Functions.FAwesome.GetAwesomeIcon(cItem.Icon, cItem.IconColor);
                        slmItem.HorizontalContentAlignment = HorizontalAlignment.Left;
                        slmItem.VerticalContentAlignment = VerticalAlignment.Center;

                        rightClickMenu.Add(slmItem);
                    }
                }

                return rightClickMenu;
            }
            catch (FormatException ex)
            {
                MessageBox.Show($"An error happened while parsing context menu, most likely happened duo typo on color name.\n\n{ex}");

                Functions.Logger.LogToFile(Functions.Logger.LogType.Library, ex.ToString());
                return rightClickMenu;
            }
        }

        public void ParseMenuItemAction(string Action)
        {
            switch (Action.ToLowerInvariant())
            {
                // Opens game installation path in explorer
                case "disk":
                    if (SteamAppsFolder.Exists)
                        System.Diagnostics.Process.Start(SteamAppsFolder.FullName);
                    break;
                case "deletelibrary":

                    MessageBoxResult moveGamesBeforeDeletion = MessageBox.Show("Move games in Library before deleting the library?", "Move games first?", MessageBoxButton.YesNoCancel);

                    if (moveGamesBeforeDeletion == MessageBoxResult.Yes)
                        //new Forms.moveLibrary(Library).Show();
                        MessageBox.Show("Function not implemented, process cancelled");
                    else if (moveGamesBeforeDeletion == MessageBoxResult.No)
                        RemoveLibraryAsync(true);

                    break;
                case "deletelibraryslm":

                    foreach (Game Game in Games.ToList())
                    {
                        if (!Game.DeleteFiles())
                        {
                            MessageBox.Show(string.Format("An unknown error happened while removing game files. {0}", FullPath));

                            return;
                        }
                    }

                    UpdateLibraryVisual();

                    MessageBox.Show(string.Format("All game files in library ({0}) successfully removed.", FullPath));
                    break;
                case "movelibrary":
                    
                    //new Forms.moveLibrary(Library).Show();
                    break;

                // Removes a backup library from list
                case "removefromlist":
                    if (IsBackup)
                    {
                        // Remove the library from our list
                        List.Libraries.Remove(this);

                        if (SLM.selectedLibrary == this)
                            Main.Accessor.gamePanel.ItemsSource = null;
                    }
                    break;
            }
        }

        public void UpdateLibraryVisual()
        {
            try
            {
                Parallel.ForEach(List.Libraries.Where(x => x.SteamAppsFolder.Root.FullName.ToLowerInvariant() == SteamAppsFolder.Root.FullName.ToLowerInvariant()), libraryToUpdate =>
                {
                    Functions.Library.UpdateLibraryVisual(libraryToUpdate);
                });
            }
            catch (Exception ex)
            {
                Functions.Logger.LogToFile(Functions.Logger.LogType.Library, ex.ToString());
            }
        }

        public async void UpdateLibraryPathAsync(string newLibraryPath)
        {
            try
            {
                await Functions.Steam.CloseSteamAsync();

                // Make a KeyValue reader
                Framework.KeyValue Key = new Framework.KeyValue();

                // Read vdf file
                Key.ReadFileAsText(Steam.vdfFilePath);

                // Change old library path with new one
                Key["Software"]["Valve"]["Steam"].Children.Find(key => key.Value.Contains(FullPath)).Value = newLibraryPath;

                // Update config.vdf file with changes
                Key.SaveToFile(Steam.vdfFilePath, false);

                // Since this file started to interrupt us? 
                // No need to bother with it since config.vdf is the real deal, just remove it and Steam client will handle.
                if (File.Exists(Path.Combine(Properties.Settings.Default.steamInstallationPath, "steamapps", "libraryfolders.vdf")))
                    File.Delete(Path.Combine(Properties.Settings.Default.steamInstallationPath, "steamapps", "libraryfolders.vdf"));

                Functions.Steam.RestartSteamAsync();
            }
            catch (Exception ex)
            {
                Functions.Logger.LogToFile(Functions.Logger.LogType.Library, ex.ToString());
            }
        }

        public async void RemoveLibraryAsync(bool deleteFiles)
        {
            try
            {
                if (deleteFiles)
                    Functions.FileSystem.DeleteOldLibrary(this);

                List.Libraries.Remove(this);

                if (IsBackup)
                {
                    Functions.SLM.Settings.SaveSettings();
                }
                else
                {
                    await Functions.Steam.CloseSteamAsync();

                    // Make a KeyValue reader
                    Framework.KeyValue Key = new Framework.KeyValue();

                    // Read vdf file
                    Key.ReadFileAsText(Steam.vdfFilePath);

                    // Remove old library
                    Key["Software"]["Valve"]["Steam"].Children.RemoveAll(x => x.Value == FullPath);

                    int i = 1;
                    foreach (Framework.KeyValue key in Key["Software"]["Valve"]["Steam"].Children.FindAll(x => x.Name.Contains("BaseInstallFolder")))
                    {
                        key.Name = string.Format("BaseInstallFolder_{0}", i);
                        i++;
                    }

                    // Update libraryFolders.vdf file with changes
                    Key.SaveToFile(Steam.vdfFilePath, false);

                    // Since this file started to interrupt us? 
                    // No need to bother with it since config.vdf is the real deal, just remove it and Steam client will handle.
                    if (File.Exists(Path.Combine(Properties.Settings.Default.steamInstallationPath, "steamapps", "libraryfolders.vdf")))
                        File.Delete(Path.Combine(Properties.Settings.Default.steamInstallationPath, "steamapps", "libraryfolders.vdf"));

                    Functions.Steam.RestartSteamAsync();
                }
            }
            catch (Exception ex)
            {
                Functions.Logger.LogToFile(Functions.Logger.LogType.Library, ex.ToString());
                MessageBox.Show(ex.ToString());
            }
        }

        public void UpdateJunks()
        {
            try
            {
                if (CommonFolder.Exists)
                {
                    foreach (DirectoryInfo DirInfo in CommonFolder.GetDirectories().Where(
                        x => Games.Count(y => y.InstallationPath.Name.ToLowerInvariant() == x.Name.ToLowerInvariant()) == 0
                        && x.Name != "241100" // Steam controller configs
                        ).OrderByDescending(x => Functions.FileSystem.GetDirectorySize(x, true)))
                    {
                        List.JunkInfo JunkInfo = new List.JunkInfo
                        {
                            FileSystemInfo = DirInfo,
                            FolderSize = Functions.FileSystem.GetDirectorySize(DirInfo, true),
                            Library = this
                        };


                        if (!List.JunkStuff.Contains(JunkInfo))
                            List.JunkStuff.Add(JunkInfo);
                    }
                }

                if (WorkshopFolder.Exists)
                {
                    foreach (FileInfo DirInfo in WorkshopFolder.EnumerateFiles("*.acf", SearchOption.TopDirectoryOnly).Where(
                        x => Games.Count(y => x.Name == y.WorkShopAcfName) == 0
                        && x.Name.ToLowerInvariant() != "appworkshop_241100.acf" // Steam Controller Configs
                        ))
                    {
                        List.JunkInfo JunkInfo = new List.JunkInfo
                        {
                            FileSystemInfo = DirInfo,
                            FolderSize = DirInfo.Length,
                            Library = this
                        };


                        if (!List.JunkStuff.Contains(JunkInfo))
                            List.JunkStuff.Add(JunkInfo);
                    }
                }

                if (Directory.Exists(Path.Combine(WorkshopFolder.FullName, "content")))
                {
                    foreach (DirectoryInfo DirInfo in new DirectoryInfo(Path.Combine(WorkshopFolder.FullName, "content")).GetDirectories().Where(
                        x => Games.Count(y => y.AppID.ToString() == x.Name) == 0
                        && x.Name != "241100" // Steam controller configs
                        ).OrderByDescending(x => Functions.FileSystem.GetDirectorySize(x, true)))
                    {
                        List.JunkInfo JunkInfo = new List.JunkInfo
                        {
                            FileSystemInfo = DirInfo,
                            FolderSize = Functions.FileSystem.GetDirectorySize(DirInfo, true),
                            Library = this
                        };


                        if (!List.JunkStuff.Contains(JunkInfo))
                            List.JunkStuff.Add(JunkInfo);
                    }
                }

            }
            catch (Exception ex)
            {
                Functions.Logger.LogToFile(Functions.Logger.LogType.Library, ex.ToString());
                MessageBox.Show(ex.ToString());
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
