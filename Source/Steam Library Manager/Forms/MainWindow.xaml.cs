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
                //Functions.Games.UpdateMainForm(null, null, Library);
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
    }
}
