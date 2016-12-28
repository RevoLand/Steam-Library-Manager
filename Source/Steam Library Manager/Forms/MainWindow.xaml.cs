using System.IO;
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

            libraryContextMenuItems.ItemsSource = Definitions.List.libraryContextMenuItems;
            gameContextMenuItems.ItemsSource = Definitions.List.gameContextMenuItems;
        }

        private void MainForm_Loaded(object sender, RoutedEventArgs e)
        {
            Functions.SLM.onLoaded();
        }

        private void MainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.MainWindowPlacement = Framework.NativeMethods.WindowPlacement.GetPlacement(this);

            Functions.SLM.onClosing();

            Application.Current.Shutdown();
        }

        private void MainForm_SourceInitialized(object sender, System.EventArgs e)
        {
            Framework.NativeMethods.WindowPlacement.SetPlacement(this, Properties.Settings.Default.MainWindowPlacement);
        }

        private void GameGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // If clicked button is left (so it will not conflict with context menu)
            if (!SystemParameters.SwapButtons && e.ChangedButton == MouseButton.Left || SystemParameters.SwapButtons && e.ChangedButton == MouseButton.Right)
            {
                // Define our picturebox from sender
                Grid grid = sender as Grid;

                // Do drag & drop with our pictureBox
                DragDrop.DoDragDrop(grid, grid.DataContext, DragDropEffects.Move);
            }
        }

        private void LibraryGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // If clicked button is left (so it will not conflict with context menu)
            if (!SystemParameters.SwapButtons && e.ChangedButton == MouseButton.Left || SystemParameters.SwapButtons && e.ChangedButton == MouseButton.Right)
            {
                // Define our library details from .Tag attribute which we set earlier
                Definitions.Library Library = (sender as Grid).DataContext as Definitions.Library;

                Definitions.SLM.selectedLibrary = Library;

                // Update games list from current selection
                Functions.Games.UpdateMainForm(Library, (Properties.Settings.Default.includeSearchResults && searchText.Text != "Search in Library (by app Name or app ID)") ? searchText.Text : null );
            }
        }

        private void LibraryGrid_Drop(object sender, DragEventArgs e)
        {
            Definitions.Library Library = (sender as Grid).DataContext as Definitions.Library;

            Definitions.Game Game = e.Data.GetData(typeof(Definitions.Game)) as Definitions.Game;

            if (Game == null || Library == null)
                return;

            if (Library == Game.InstalledLibrary && !Game.IsSteamBackup)
                return;

            if (Game.IsSteamBackup)
                System.Diagnostics.Process.Start(Path.Combine(Properties.Settings.Default.steamInstallationPath, "Steam.exe"), $"-install \"{Game.InstallationPath}\"");
            else
                new Forms.MoveGameForm(Game, Library).Show();
        }

        private void LibraryGrid_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
        }

        private void LibraryPanel_Drop(object sender, DragEventArgs e)
        {
            string[] droppedItems = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            if (droppedItems == null) return;

            foreach (string droppedItem in droppedItems)
            {
                FileInfo details = new FileInfo(droppedItem);

                if (details.Attributes.HasFlag(FileAttributes.Directory))
                {
                    if (!Functions.Library.LibraryExists(droppedItem))
                    {
                        if (Directory.GetDirectoryRoot(droppedItem) != droppedItem)
                        {
                            bool isNewLibraryForBackup = false;
                            MessageBoxResult selectedLibraryType = MessageBox.Show("Is this selected folder going to be used for backups?", "SLM library or Steam library?", MessageBoxButton.YesNoCancel);

                            if (selectedLibraryType == MessageBoxResult.Cancel)
                                return;
                            else if (selectedLibraryType == MessageBoxResult.Yes)
                                isNewLibraryForBackup = true;

                            Functions.Library.CreateNewLibrary(details.FullName, isNewLibraryForBackup);
                        }
                        else
                            MessageBox.Show("Libraries can not be created at root");
                    }
                    else
                        MessageBox.Show("Library exists");
                }
            }
        }

        private void LibraryContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ((Definitions.Library)(sender as MenuItem).DataContext).ParseMenuItemAction((string)(sender as MenuItem).Tag);
        }

        private void GameContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ((Definitions.Game)(sender as MenuItem).DataContext).ParseMenuItemAction((string)(sender as MenuItem).Tag);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Definitions.SLM.selectedLibrary != null && !string.IsNullOrEmpty(searchText.Text) && searchText.Text != "Search in Library (by app Name or app ID)")
                Functions.Games.UpdateMainForm(Definitions.SLM.selectedLibrary, searchText.Text);
        }

        private void SearchText_GotFocus(object sender, RoutedEventArgs e)
        {
            if (searchText.Text == "Search in Library (by app Name or app ID)")
                searchText.Text = "";
        }

        private void SearchText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(searchText.Text))
                searchText.Text = "Search in Library (by app Name or app ID)";
        }

        private void LibraryDataGridMenuItem_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = libraryContextMenuItems.SelectedIndex;

            if (selectedIndex == -1 || selectedIndex >= Definitions.List.libraryContextMenuItems.Count)
                return;

            switch(((MenuItem)sender).Tag.ToString())
            {
                case "moveUp":
                    if (selectedIndex < 1)
                        return;

                    Definitions.List.libraryContextMenuItems.Move(selectedIndex, selectedIndex - 1);
                    break;

                case "moveDown":
                    if (selectedIndex == Definitions.List.libraryContextMenuItems.Count - 1)
                        return;

                    Definitions.List.libraryContextMenuItems.Move(selectedIndex, selectedIndex + 1);
                    break;
            }
        }

        private void GameDataGridMenuItem_Click(object sender, RoutedEventArgs e)
        {

            int selectedIndex = gameContextMenuItems.SelectedIndex;

            if (selectedIndex == -1 || selectedIndex >= Definitions.List.gameContextMenuItems.Count)
                return;

            switch (((MenuItem)sender).Tag.ToString())
            {
                case "moveUp":
                    if (selectedIndex < 1)
                        return;

                    Definitions.List.gameContextMenuItems.Move(selectedIndex, selectedIndex - 1);
                    break;

                case "moveDown":
                    if (selectedIndex == Definitions.List.gameContextMenuItems.Count - 1)
                        return;

                    Definitions.List.gameContextMenuItems.Move(selectedIndex, selectedIndex + 1);
                    break;
            }
        }

        private void DonateButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(Definitions.SLM.paypalDonationURL);
            }
            catch { }
        }

        private void GameSortingMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.defaultGameSortingMethod = gameSortingMethod.SelectedItem.ToString();
            }
            catch { }
        }

        private void GameSizeCalcMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.gameSizeCalculationMethod = gameSizeCalcMethod.SelectedItem.ToString();
            }
            catch { }
        }

        private void ArchiveSizeCalcMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.archiveSizeCalculationMethod = archiveSizeCalcMethod.SelectedItem.ToString();
            }
            catch { }
        }

        private void CheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Functions.Updater.CheckForUpdates();
            }
            catch { }
        }
    }
}
