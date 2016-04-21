using FontAwesome.WPF;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Steam_Library_Manager.Content
{
    class Libraries
    {

        public static ContextMenu generateRightClickMenu(Definitions.List.Library Library)
        {
            generateRightClickMenuItems(Library);

            // Create a new right click menu (aka context menu)
            ContextMenu cMenu = new ContextMenu();
            cMenu.Tag = Library;

            foreach (Definitions.List.rightClickMenuItem Item in Definitions.List.rightClickMenuItems.Where(x => x.Library == Library).OrderBy(x => x.order))
            {
                if (Item.ShownToBackup && !Library.Backup) continue;

                MenuItem newMenuItem = new MenuItem();

                if (Item.IsSeperator)
                    cMenu.Items.Add(new Separator());
                else
                {
                    newMenuItem.Click += menuItem_Click;

                    newMenuItem.Header = Item.DisplayText;
                    newMenuItem.Name = Item.Action;
                    newMenuItem.IsEnabled = Item.IsEnabled;

                    if (Item.icon != FontAwesomeIcon.None)
                        newMenuItem.Icon = Functions.fAwesome.getAwesomeIcon(Item.icon, Item.iconColor);

                    cMenu.Items.Add(newMenuItem);
                }
            }
            return cMenu;
        }

        private static void menuItem_Click(object sender, RoutedEventArgs e)
        {
            // Define our game from the Tag we given to Context menu
            Definitions.List.Library Library = ((sender as MenuItem).Parent as ContextMenu).Tag as Definitions.List.Library;

            // switch based on name we set earlier with context menu
            switch ((sender as MenuItem).Name)
            {
                // Opens game installation path in explorer
                case "Disk":
                    if (Library.steamAppsPath.Exists)
                        System.Diagnostics.Process.Start(Library.steamAppsPath.FullName);
                    break;
                case "deleteLibrary":

                    MessageBoxResult moveGamesBeforeDeletion = MessageBox.Show("Move games in Library before deleting the library?", "Move games first?", MessageBoxButton.YesNoCancel);

                    if (moveGamesBeforeDeletion == MessageBoxResult.Yes)
                        //new Forms.moveLibrary(Library).Show();
                        MessageBox.Show("Not implemented");
                    else if (moveGamesBeforeDeletion == MessageBoxResult.No)
                        Functions.Library.removeLibrary(Library, true);

                    break;
                case "deleteLibrarySLM":

                    foreach (Definitions.List.Game Game in Library.Games.ToList())
                    {
                        Functions.fileSystem.Game gameFunctions = new Functions.fileSystem.Game();

                        if (!gameFunctions.deleteGameFiles(Game))
                        {
                            MessageBox.Show(string.Format("An unknown error happened while removing game files. {0}", Library.fullPath));

                            return;
                        }
                    }

                    Functions.Library.updateLibraryVisual(Library);
                    MessageBox.Show(string.Format("All game files in library ({0}) successfully removed.", Library.fullPath));

                    break;
                case "moveLibrary":
                    //new Forms.moveLibrary(Library).Show();
                    break;

                // Removes a backup library from list
                case "RemoveFromList":
                    if (Library.Backup)
                    {
                        // Remove the library from our list
                        Definitions.List.Libraries.Remove(Library);

                        MainWindow.Accessor.gamePanel.ItemsSource = null;
                    }
                    break;
            }
        }


        public static void generateRightClickMenuItems(Definitions.List.Library Library)
        {
            if (Definitions.List.rightClickMenuItems == null)
                Definitions.List.rightClickMenuItems = new System.Collections.Generic.List<Definitions.List.rightClickMenuItem>();

            Definitions.List.rightClickMenuItems.Add(new Definitions.List.rightClickMenuItem
            {
                order = 1,
                DisplayText = $"Open library in explorer ({Library.fullPath})",
                Action = "Disk",
                icon = FontAwesomeIcon.FolderOpen,
                Library = Library
            });

            Definitions.List.rightClickMenuItems.Add(new Definitions.List.rightClickMenuItem
            {
                order = 2,
                IsSeperator = true,
                Library = Library
            });

            Definitions.List.rightClickMenuItems.Add(new Definitions.List.rightClickMenuItem
            {
                order = 3,
                DisplayText = "Move library",
                Action = "moveLibrary",
                icon = FontAwesomeIcon.Paste,
                Library = Library
            });

            Definitions.List.rightClickMenuItems.Add(new Definitions.List.rightClickMenuItem
            {
                order = 4,
                DisplayText = "Refresh game list in library",
                Action = "RefreshGameList",
                icon = FontAwesomeIcon.Refresh,
                Library = Library
            });

            Definitions.List.rightClickMenuItems.Add(new Definitions.List.rightClickMenuItem
            {
                order = 5,
                IsSeperator = true,
                Library = Library
            });

            Definitions.List.rightClickMenuItems.Add(new Definitions.List.rightClickMenuItem
            {
                order = 6,
                DisplayText = "Delete library",
                Action = "deleteLibrary",
                icon = FontAwesomeIcon.Trash,
                Library = Library
            });

            Definitions.List.rightClickMenuItems.Add(new Definitions.List.rightClickMenuItem
            {
                order = 7,
                DisplayText = "Delete games in library",
                Action = "deleteLibrarySLM",
                icon = FontAwesomeIcon.TrashOutline,
                Library = Library
            });

            Definitions.List.rightClickMenuItems.Add(new Definitions.List.rightClickMenuItem
            {
                order = 8,
                DisplayText = "Remove from list",
                Action = "RemoveFromList",
                icon = FontAwesomeIcon.Minus,
                ShownToBackup = true,
                Library = Library
            });
        }
    }
}
