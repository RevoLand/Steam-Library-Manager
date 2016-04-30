using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Steam_Library_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        public static MainWindow Accessor;

        public MainWindow()
        {
            InitializeComponent();

            Accessor = this;

            libraryPanel.ItemsSource = Definitions.List.Libraries;
        }

        private void mainForm_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();

            if (Properties.Settings.Default.Maximised)
                WindowState = WindowState.Maximized;

            Functions.SLM.onLoaded();
        }

        private void mainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                Properties.Settings.Default.Top = RestoreBounds.Top;
                Properties.Settings.Default.Left = RestoreBounds.Left;
                Properties.Settings.Default.Height = RestoreBounds.Height;
                Properties.Settings.Default.Width = RestoreBounds.Width;
                Properties.Settings.Default.Maximised = true;
            }
            else
            {
                Properties.Settings.Default.Top = Top;
                Properties.Settings.Default.Left = Left;
                Properties.Settings.Default.Height = Height;
                Properties.Settings.Default.Width = Width;
                Properties.Settings.Default.Maximised = false;
            }

            Functions.SLM.onClosing();

            Application.Current.Shutdown();
        }

        private void gameGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // If clicked button is left (so it will not conflict with context menu)
            if (!SystemParameters.SwapButtons && e.ChangedButton == MouseButton.Left || SystemParameters.SwapButtons && e.ChangedButton == MouseButton.Right)
            {
                // Define our picturebox from sender
                Grid grid = sender as Grid;

                // Do drag & drop with our pictureBox
                DragDrop.DoDragDrop(grid, grid.Tag, DragDropEffects.Move);
            }
        }

        private void libraryGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // If clicked button is left (so it will not conflict with context menu)
            if (!SystemParameters.SwapButtons && e.ChangedButton == MouseButton.Left || SystemParameters.SwapButtons && e.ChangedButton == MouseButton.Right)
            {
                // Define our library details from .Tag attribute which we set earlier
                Definitions.List.Library Library = (sender as Grid).Tag as Definitions.List.Library;

                Definitions.SLM.selectedLibrary = Library;
                // Update games list from current selection

                gamePanel.ItemsSource = Library.Games;
            }
        }

        private void libraryGrid_Drop(object sender, DragEventArgs e)
        {
            Definitions.List.Library Library = (sender as Grid).Tag as Definitions.List.Library;

            Definitions.List.Game Game = e.Data.GetData(typeof(Definitions.List.Game)) as Definitions.List.Game;

            if (Game == null || Library == null || Library == Game.Library)
                return;

            if (Game.SteamBackup)
                System.Diagnostics.Process.Start(Path.Combine(Properties.Settings.Default.steamInstallationPath, "Steam.exe"), $"-install \"{Game.installationPath}\"");
            else
                new Forms.MoveGameForm(Game, Library).Show();
        }

        private void libraryGrid_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
        }

        private void libraryPanel_Drop(object sender, DragEventArgs e)
        {
            string[] droppedItems = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            if (droppedItems == null) return;

            foreach (string droppedItem in droppedItems)
            {
                FileInfo details = new FileInfo(droppedItem);

                if (details.Attributes.HasFlag(FileAttributes.Directory))
                {
                    if (!Functions.Library.libraryExists(droppedItem))
                    {
                        if (Directory.GetDirectoryRoot(droppedItem) != droppedItem)
                            Functions.Library.createNewLibrary(details.FullName, true);
                        else
                            MessageBox.Show("Libraries can not be created at root");
                    }
                    else
                        MessageBox.Show("Library exists");
                }
            }
        }


        private void libraryContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Define our game from the Tag we given to Context menu
            Definitions.List.Library Library = (sender as MenuItem).Tag as Definitions.List.Library;

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
                        MessageBox.Show("Function not implemented, process cancelled");
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

        private void gameContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Definitions.List.Game Game = (sender as MenuItem).Tag as Definitions.List.Game;

            switch ((sender as MenuItem).Name)
            {
                default:
                    System.Diagnostics.Process.Start(string.Format("steam://{0}/{1}", (sender as MenuItem).Name, Game.appID));
                    break;
                case "Disk":
                    System.Diagnostics.Process.Start(Game.commonPath.FullName);
                    break;
                case "acfFile":
                    System.Diagnostics.Process.Start(Game.acfPath.FullName);
                    break;
                case "deleteGameFilesSLM":

                    Functions.fileSystem.Game gameFunctions = new Functions.fileSystem.Game();
                    gameFunctions.deleteGameFiles(Game);
                    break;
            }
        }
    }
}
