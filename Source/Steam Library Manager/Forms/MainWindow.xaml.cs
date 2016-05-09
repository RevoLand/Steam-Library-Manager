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
            GameContextMenuItems.ItemsSource = Definitions.List.contextMenuItems;
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
                Definitions.Library Library = (sender as Grid).Tag as Definitions.Library;

                Definitions.SLM.selectedLibrary = Library;

                // Update games list from current selection
                Functions.Games.UpdateMainForm(Library, (Properties.Settings.Default.includeSearchResults && searchText.Text != "Search in Library (by app Name or app ID)") ? searchText.Text : null );
            }
        }

        private void libraryGrid_Drop(object sender, DragEventArgs e)
        {
            Definitions.Library Library = (sender as Grid).Tag as Definitions.Library;

            Definitions.Game Game = e.Data.GetData(typeof(Definitions.Game)) as Definitions.Game;

            if (Game == null || Library == null || Library == Game.installedLibrary)
                return;

            if (Game.IsSteamBackup)
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
            Definitions.Library Library = (sender as MenuItem).Tag as Definitions.Library;

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

                    foreach (Definitions.Game Game in Library.Games.ToList())
                    {
                        Functions.fileSystem.Game gameFunctions = new Functions.fileSystem.Game();

                        if (!Game.deleteFiles())
                        {
                            MessageBox.Show(string.Format("An unknown error happened while removing game files. {0}", Library.fullPath));

                            return;
                        }
                        else
                            Game.RemoveFromLibrary();
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
            Steam_Library_Manager.Content.Games.parseAction((Definitions.Game)(sender as MenuItem).DataContext, (string)(sender as MenuItem).Tag);
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Definitions.SLM.selectedLibrary != null)
                Functions.Games.UpdateMainForm(Definitions.SLM.selectedLibrary, searchText.Text);
        }

        private void searchText_GotFocus(object sender, RoutedEventArgs e)
        {
            if (searchText.Text == "Search in Library (by app Name or app ID)")
                searchText.Text = "";
        }

        private void searchText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(searchText.Text))
                searchText.Text = "Search in Library (by app Name or app ID)";
        }
    }
}
