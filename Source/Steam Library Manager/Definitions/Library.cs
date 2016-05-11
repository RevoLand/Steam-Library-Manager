using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Steam_Library_Manager.Definitions
{
    public class Library
    {
        public bool Main { get; set; }
        public bool Backup { get; set; }
        public int GameCount { get; set; }
        public DirectoryInfo steamAppsPath, commonPath, downloadPath, workshopPath;
        public Framework.AsyncObservableCollection<System.Windows.FrameworkElement> contextMenu { get; set; }
        public string fullPath { get; set; }
        public string prettyFreeSpace { get; set; }
        public int freeSpacePerc { get; set; }
        public long freeSpace;
        public Framework.AsyncObservableCollection<Game> Games { get; set; } = new Framework.AsyncObservableCollection<Game>();

        public Framework.AsyncObservableCollection<FrameworkElement> generateRightClickMenuItems()
        {
            Framework.AsyncObservableCollection<FrameworkElement> rightClickMenu = new Framework.AsyncObservableCollection<FrameworkElement>();
            try
            {
                foreach (List.contextMenu cItem in List.libraryContextMenuItems.Where(x => x.IsVisible))
                {
                    if (cItem.shownToBackup && !Backup)
                        continue;

                    if (cItem.IsSeparator)
                        rightClickMenu.Add(new Separator());
                    else
                    {
                        MenuItem slmItem = new MenuItem();

                        slmItem.Tag = this;
                        slmItem.Header = string.Format(cItem.Header, fullPath, prettyFreeSpace);
                        slmItem.Tag = cItem.Action;
                        slmItem.Icon = Functions.fAwesome.getAwesomeIcon(cItem.Icon, cItem.IconColor);

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

        public void parseMenuItemAction(string Action)
        {
            switch (Action.ToLowerInvariant())
            {
                // Opens game installation path in explorer
                case "Disk":
                    if (steamAppsPath.Exists)
                        System.Diagnostics.Process.Start(steamAppsPath.FullName);
                    break;
                case "deleteLibrary":

                    MessageBoxResult moveGamesBeforeDeletion = MessageBox.Show("Move games in Library before deleting the library?", "Move games first?", MessageBoxButton.YesNoCancel);

                    if (moveGamesBeforeDeletion == MessageBoxResult.Yes)
                        //new Forms.moveLibrary(Library).Show();
                        MessageBox.Show("Function not implemented, process cancelled");
                    else if (moveGamesBeforeDeletion == MessageBoxResult.No)
                        removeLibrary(true);

                    break;
                case "deleteLibrarySLM":

                    foreach (Game Game in Games.ToList())
                    {
                        Functions.fileSystem.Game gameFunctions = new Functions.fileSystem.Game();

                        if (!Game.deleteFiles())
                        {
                            MessageBox.Show(string.Format("An unknown error happened while removing game files. {0}", fullPath));

                            return;
                        }
                        else
                            Game.RemoveFromLibrary();
                    }

                    updateLibraryVisual();

                    MessageBox.Show(string.Format("All game files in library ({0}) successfully removed.", fullPath));
                    break;
                case "moveLibrary":
                    //new Forms.moveLibrary(Library).Show();
                    break;

                // Removes a backup library from list
                case "RemoveFromList":
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

        public void updateLibraryVisual()
        {
            freeSpace = Functions.fileSystem.getAvailableFreeSpace(fullPath);
            prettyFreeSpace = Functions.fileSystem.FormatBytes(freeSpace);
            freeSpacePerc = 100 - ((int)Math.Round((double)(100 * freeSpace) / Functions.fileSystem.getUsedSpace(fullPath)));

            /*
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
            */
        }

        public void updateLibraryPath(string newLibraryPath)
        {
            try
            {
                // Make a KeyValue reader
                Framework.KeyValue Key = new Framework.KeyValue();

                // Read vdf file
                Key.ReadFileAsText(Steam.vdfFilePath);

                // Change old library path with new one
                Key.Children[0].Children[0].Children[0].Children.Find(key => key.Value.Contains(fullPath)).Value = newLibraryPath;

                // Update config.vdf file with changes
                Key.SaveToFile(Steam.vdfFilePath, false);
            }
            catch { }
        }

        public void removeLibrary(bool deleteFiles)
        {
            try
            {
                if (deleteFiles)
                    Functions.fileSystem.deleteOldLibrary(this);

                List.Libraries.Remove(this);

                if (Backup)
                {
                    Functions.SLM.Settings.updateBackupDirs();
                    Functions.SLM.Settings.saveSettings();
                }
                else
                {
                    // Make a KeyValue reader
                    Framework.KeyValue Key = new Framework.KeyValue();

                    // Read vdf file
                    Key.ReadFileAsText(Definitions.Steam.vdfFilePath);

                    // Remove old library
                    Key.Children[0].Children[0].Children[0].Children.RemoveAll(x => x.Value == fullPath);

                    int i = 1;
                    foreach (Framework.KeyValue key in Key.Children[0].Children[0].Children[0].Children.FindAll(x => x.Name.Contains("BaseInstallFolder")))
                    {
                        key.Name = string.Format("BaseInstallFolder_{0}", i);
                        i++;
                    }

                    // Update libraryFolders.vdf file with changes
                    Key.SaveToFile(Definitions.Steam.vdfFilePath, false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

    }
}
