using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Steam_Library_Manager.Definitions
{
    public class Library
    {
        public bool Main { get; set; }
        public bool Backup { get; set; }
        public DirectoryInfo steamAppsPath, commonPath, downloadPath, workshopPath;
        public Framework.AsyncObservableCollection<FrameworkElement> ContextMenu { get; set; }
        public string FullPath { get; set; }
        public int FreeSpacePerc { get; set; }
        public long FreeSpace { get; set; }
        public Framework.AsyncObservableCollection<Game> Games { get; set; } = new Framework.AsyncObservableCollection<Game>();
        public string PrettyFreeSpace
        {
            get => Functions.FileSystem.FormatBytes(FreeSpace);
            set { }
        }

        public void UpdateGameList()
        {
            try
            {
                if (!steamAppsPath.Exists)
                    steamAppsPath.Create();
                else if (Games.Count > 0)
                    Games.Clear();

                // Foreach *.acf file found in library
                Parallel.ForEach(steamAppsPath.EnumerateFiles("*.acf", SearchOption.TopDirectoryOnly), acfFilePath =>
                {
                    // Define a new value and call KeyValue
                    Framework.KeyValue Key = new Framework.KeyValue();

                    // Read the *.acf file as text
                    Key.ReadFileAsText(acfFilePath.FullName);

                    // If key doesn't contains a child (value in acf file)
                    if (Key.Children.Count == 0)
                        return;

                    Functions.Games.AddNewGame(acfFilePath.FullName, Convert.ToInt32(Key["appID"].Value), Key["name"].Value ?? Key["UserConfig"]["name"].Value, Key["installdir"].Value, this, Convert.ToInt64(Key["SizeOnDisk"].Value), false);
                });

                // Do a loop for each *.zip file in library
                Parallel.ForEach(Directory.EnumerateFiles(steamAppsPath.FullName, "*.zip", SearchOption.TopDirectoryOnly), gameArchive =>
                {
                    Functions.Games.ReadGameDetailsFromZip(gameArchive, this);
                });

                // If library is backup library
                if (Backup)
                {
                    foreach (string skuFile in Directory.EnumerateFiles(FullPath, "*.sis", SearchOption.AllDirectories))
                    {
                        Framework.KeyValue Key = new Framework.KeyValue();

                        Key.ReadFileAsText(skuFile);

                        string[] name = System.Text.RegularExpressions.Regex.Split(Key["name"].Value, " and ");

                        int i = 0;
                        long gameSize = Functions.FileSystem.GetDirectorySize(new DirectoryInfo(skuFile).Parent.FullName, true);
                        foreach (Framework.KeyValue app in Key["apps"].Children)
                        {
                            if (Games.Count(x => x.AppID == Convert.ToInt32(app.Value)) > 0)
                                continue;

                            Functions.Games.AddNewGame(skuFile, Convert.ToInt32(app.Value), name[i], Path.GetDirectoryName(skuFile), this, gameSize, false, true);

                            if (name.Count() > 1)
                                i++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public Framework.AsyncObservableCollection<FrameworkElement> GenerateRightClickMenuItems()
        {
            Framework.AsyncObservableCollection<FrameworkElement> rightClickMenu = new Framework.AsyncObservableCollection<FrameworkElement>();
            try
            {
                foreach (ContextMenu cItem in List.libraryContextMenuItems.Where(x => x.IsActive))
                {
                    if (Backup && cItem.ShowToSLMBackup == Enums.menuVisibility.NotVisible)
                        continue;
                    else if (!Backup && cItem.ShowToNormal == Enums.menuVisibility.NotVisible)
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
                        slmItem.Icon = Functions.fAwesome.getAwesomeIcon(cItem.Icon, cItem.IconColor);
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

                return rightClickMenu;
            }
        }

        public void ParseMenuItemAction(string Action)
        {
            switch (Action.ToLowerInvariant())
            {
                // Opens game installation path in explorer
                case "disk":
                    if (steamAppsPath.Exists)
                        System.Diagnostics.Process.Start(steamAppsPath.FullName);
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
                        else
                            Game.RemoveFromLibrary();
                    }

                    UpdateLibraryVisual();

                    MessageBox.Show(string.Format("All game files in library ({0}) successfully removed.", FullPath));
                    break;
                case "movelibrary":
                    //new Forms.moveLibrary(Library).Show();
                    break;

                // Removes a backup library from list
                case "removefromlist":
                    if (Backup)
                    {
                        // Remove the library from our list
                        List.Libraries.Remove(this);

                        if (SLM.selectedLibrary == this)
                            MainWindow.Accessor.gamePanel.ItemsSource = null;
                    }
                    break;
            }
        }

        public void UpdateLibraryVisual()
        {
            try
            {
                foreach (Library libraryToUpdate in List.Libraries.Where(x => x.steamAppsPath.Root == steamAppsPath.Root))
                {
                    UpdateLibraryVisual(libraryToUpdate);
                }

                if (MainWindow.Accessor.libraryPanel.Dispatcher.CheckAccess())
                {
                    MainWindow.Accessor.libraryPanel.Items.Refresh();
                }
                else
                {
                    MainWindow.Accessor.libraryPanel.Dispatcher.Invoke(delegate
                    {
                        MainWindow.Accessor.libraryPanel.Items.Refresh();
                    }, System.Windows.Threading.DispatcherPriority.Normal);
                }
            }
            catch { }
        }

        public void UpdateLibraryVisual(Library libraryToUpdate)
        {
            try
            {
                libraryToUpdate.FreeSpace = Functions.FileSystem.GetAvailableFreeSpace(libraryToUpdate.FullPath);
                libraryToUpdate.FreeSpacePerc = 100 - ((int)Math.Round((double)(100 * libraryToUpdate.FreeSpace) / Functions.FileSystem.GetUsedSpace(libraryToUpdate.FullPath)));
            }
            catch { }
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
            catch { }
        }

        public async void RemoveLibraryAsync(bool deleteFiles)
        {
            try
            {
                if (deleteFiles)
                    Functions.FileSystem.DeleteOldLibrary(this);

                List.Libraries.Remove(this);

                if (Backup)
                {
                    Functions.SLM.Settings.UpdateBackupDirs();
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
                MessageBox.Show(ex.ToString());
            }
        }

    }
}
