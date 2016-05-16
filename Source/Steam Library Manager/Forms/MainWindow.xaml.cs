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

        private void mainForm_Loaded(object sender, RoutedEventArgs e)
        {

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
            ((Definitions.Library)(sender as MenuItem).DataContext).parseMenuItemAction((string)(sender as MenuItem).Tag);
        }

        private void gameContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ((Definitions.Game)(sender as MenuItem).DataContext).parseMenuItemAction((string)(sender as MenuItem).Tag);
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

        private void libraryDataGridMenuItem_Click(object sender, RoutedEventArgs e)
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

        private void gameDataGridMenuItem_Click(object sender, RoutedEventArgs e)
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

        private void donateButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(Definitions.SLM.paypalDonationURL);
            }
            catch { }
        }
    }
}
