using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Steam_Library_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class Main : Window
    {
        public static Main Accessor;
        public Framework.AsyncObservableCollection<string> TaskManager_Logs = new Framework.AsyncObservableCollection<string>();
        Framework.Network.Server SLMServer = new Framework.Network.Server();

        public Main()
        {
            InitializeComponent();

            UpdateBindings();
        }

        void UpdateBindings()
        {
            Accessor = this;

            Properties.Settings.Default.SearchText = "";

            libraryPanel.ItemsSource = Definitions.List.Libraries;

            libraryContextMenuItems.ItemsSource = Definitions.List.LibraryCMenuItems;
            gameContextMenuItems.ItemsSource = Definitions.List.GameCMenuItems;

            LibraryCleaner.ItemsSource = Definitions.List.JunkStuff;

            TaskManager_LogsView.ItemsSource = TaskManager_Logs;
        }

        private void MainForm_Loaded(object sender, RoutedEventArgs e)
        {
            Functions.SLM.OnLoad();

            if (Properties.Settings.Default.ParallelAfterSize >= 20000000)
                Properties.Settings.Default.ParallelAfterSize = Properties.Settings.Default.ParallelAfterSize / 1000000;

            settingsGroupBox.DataContext = new Definitions.Settings();
            QuickSettings.DataContext = settingsGroupBox.DataContext;

            if (Properties.Settings.Default.Global_StartTaskManagerOnStartup)
            {
                Framework.TaskManager.Start();
            }

            if (Properties.Settings.Default.Advanced_Logging)
                Functions.Logger.StartLogger();
        }

        private void MainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.MainWindowPlacement = Framework.NativeMethods.WindowPlacement.GetPlacement(this);

            Functions.SLM.OnClosing();

            Application.Current.Shutdown();
        }

        private void MainForm_SourceInitialized(object sender, EventArgs e)
        {
            Framework.NativeMethods.WindowPlacement.SetPlacement(this, Properties.Settings.Default.MainWindowPlacement);
        }

        private void LibraryGrid_Drop(object sender, DragEventArgs e)
        {
            try
            {
                Definitions.Library Library = (sender as Grid).DataContext as Definitions.Library;

                if (gamePanel.SelectedItems.Count == 0 || Library == null)
                    return;

                foreach (Definitions.Game gameToMove in gamePanel.SelectedItems)
                {
                    if (Library.IsOffline)
                    {
                        if (!Directory.Exists(Library.FullPath))
                            continue;
                        else
                            Functions.Library.UpdateBackupLibraryAsync(Library);
                    }

                    if (Library == gameToMove.InstalledLibrary && !gameToMove.IsSteamBackup)
                        continue;

                    if (gameToMove.IsSteamBackup)
                        Process.Start(Path.Combine(Properties.Settings.Default.steamInstallationPath, "Steam.exe"), $"-install \"{gameToMove.InstallationPath}\"");
                    else
                    {
                        if (Framework.TaskManager.TaskList.Count(x => x.TargetGame == gameToMove && x.TargetLibrary == Library) == 0)
                        {
                            Definitions.List.TaskList newTask = new Definitions.List.TaskList
                            {
                                TargetGame = gameToMove,
                                TargetLibrary = Library
                            };

                            Framework.TaskManager.TaskList.Add(newTask);
                            taskPanel.Items.Add(newTask);

                            DoubleAnimation da = new DoubleAnimation()
                            {
                                From = 12,
                                To = 14,
                                AutoReverse = true,
                                Duration = new Duration(TimeSpan.FromSeconds(0.3))
                            };

                            Tab_TaskManager.BeginAnimation(TextBlock.FontSizeProperty, da);
                        }
                        else
                        {
                            MessageBox.Show($"This item is already tasked.\n\nGame: {gameToMove.AppName}\nTarget Library: {Library.FullPath}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Functions.Logger.LogToFile(Functions.Logger.LogType.SLM, ex.ToString());
                MessageBox.Show(ex.ToString());
            }
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
                    if (!Functions.Library.IsLibraryExists(droppedItem))
                    {
                        if (Directory.GetDirectoryRoot(droppedItem) != droppedItem)
                        {
                            bool isNewLibraryForBackup = false;
                            MessageBoxResult selectedLibraryType = MessageBox.Show("Is this selected folder going to be used for backups?", "SLM library or Steam library?", MessageBoxButton.YesNoCancel);

                            if (selectedLibraryType == MessageBoxResult.Cancel)
                                return;
                            else if (selectedLibraryType == MessageBoxResult.Yes)
                                isNewLibraryForBackup = true;

                            Functions.Library.CreateNewLibraryAsync(details.FullName, isNewLibraryForBackup);
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

        private void Gamelibrary_ContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ((Definitions.Game)(sender as MenuItem).DataContext).ParseMenuItemAction((string)(sender as MenuItem).Tag);
        }

        private void LibraryDataGridMenuItem_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = libraryContextMenuItems.SelectedIndex;

            if (selectedIndex == -1 || selectedIndex >= Definitions.List.LibraryCMenuItems.Count)
                return;

            switch(((MenuItem)sender).Tag.ToString())
            {
                case "moveUp":
                    if (selectedIndex < 1)
                        return;

                    Definitions.List.LibraryCMenuItems.Move(selectedIndex, selectedIndex - 1);
                    break;

                case "moveDown":
                    if (selectedIndex == Definitions.List.LibraryCMenuItems.Count - 1)
                        return;

                    Definitions.List.LibraryCMenuItems.Move(selectedIndex, selectedIndex + 1);
                    break;
            }
        }

        private void GameDataGridMenuItem_Click(object sender, RoutedEventArgs e)
        {

            int selectedIndex = gameContextMenuItems.SelectedIndex;

            if (selectedIndex == -1 || selectedIndex >= Definitions.List.GameCMenuItems.Count)
                return;

            switch (((MenuItem)sender).Tag.ToString())
            {
                case "moveUp":
                    if (selectedIndex < 1)
                        return;

                    Definitions.List.GameCMenuItems.Move(selectedIndex, selectedIndex - 1);
                    break;

                case "moveDown":
                    if (selectedIndex == Definitions.List.GameCMenuItems.Count - 1)
                        return;

                    Definitions.List.GameCMenuItems.Move(selectedIndex, selectedIndex + 1);
                    break;
            }
        }

        private void CheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Functions.Updater.CheckForUpdates();
            }
            catch (Exception ex)
            {
                Functions.Logger.LogToFile(Functions.Logger.LogType.SLM, ex.ToString());
            }
        }

        private void LibraryGrid_MouseDown(object sender, SelectionChangedEventArgs e)
        {
            Definitions.SLM.selectedLibrary = libraryPanel.SelectedItem as Definitions.Library;

            if (Definitions.SLM.selectedLibrary == null)
                return;

            if (Directory.Exists(Definitions.SLM.selectedLibrary.FullPath) && Definitions.SLM.selectedLibrary.IsOffline)
            {
                Functions.Library.UpdateBackupLibraryAsync(Definitions.SLM.selectedLibrary);
            }

            // Update games list from current selection
            Functions.Games.UpdateMainForm(Definitions.SLM.selectedLibrary, (Properties.Settings.Default.includeSearchResults) ? Properties.Settings.Default.SearchText : null);
        }

        private void TaskManager_Buttons_Click(object sender, RoutedEventArgs e)
        {
            switch((sender as Button).Tag)
            {
                default:
                case "Start":
                    Framework.TaskManager.Start();
                    Button_StopTaskManager.IsEnabled = true;
                    break;
                case "Stop":
                    Framework.TaskManager.Stop();
                    Button_StopTaskManager.IsEnabled = false;
                    break;
                case "BackupUpdates":
                    Functions.Library.CheckForBackupUpdates();
                    break;
                case "ClearCompleted":
                    if (taskPanel.Items.Count == 0)
                        return;

                    List<Definitions.List.TaskList> taskPanelItems = taskPanel.Items.OfType<Definitions.List.TaskList>().ToList();

                    foreach (Definitions.List.TaskList currentTask in taskPanelItems)
                    {
                        if (currentTask.Completed)
                            taskPanel.Items.Remove(currentTask);
                    }
                    break;
            }
        }

        private void TaskManager_ContextMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch ((sender as MenuItem).Tag)
                {
                    default:
                    case "Remove":
                        if (taskPanel.SelectedItems.Count == 0)
                            return;

                        List<Definitions.List.TaskList> selectedItems = taskPanel.SelectedItems.OfType<Definitions.List.TaskList>().ToList();

                        foreach (Definitions.List.TaskList currentTask in selectedItems)
                        {
                            if (currentTask.Moving && Framework.TaskManager.Status && !currentTask.Completed)
                                MessageBox.Show($"[{currentTask.TargetGame.AppName}] You can't remove a game from Task Manager which is currently being moven.\n\nPlease Stop the Task Manager first.");
                            else
                            {
                                Framework.TaskManager.RemoveTask(currentTask);
                                taskPanel.Items.Remove(currentTask);
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Functions.Logger.LogToFile(Functions.Logger.LogType.SLM, ex.ToString());
            }
        }

        private void Gamelibrary_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (sender is Grid grid && e.LeftButton == MouseButtonState.Pressed)
                {
                    // Do drag & drop with our pictureBox
                    DragDrop.DoDragDrop(grid, grid.DataContext, DragDropEffects.Move);
                }
            }
            catch { }
        }

        private void GameSortingMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Functions.Games.UpdateMainForm(Definitions.SLM.selectedLibrary, Properties.Settings.Default.SearchText);
        }

        private void LibraryCleaner_ContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LibraryCleaner.SelectedItems.Count == 0)
                    return;

                List<Definitions.List.JunkInfo> selectedItems = LibraryCleaner.SelectedItems.OfType<Definitions.List.JunkInfo>().ToList();

                foreach (Definitions.List.JunkInfo currentJunk in selectedItems)
                {
                    if ((string)(sender as MenuItem).Tag == "Explorer")
                    {
                        Process.Start(currentJunk.FileSystemInfo.FullName);
                    }
                    else
                    {
                        if (currentJunk.FileSystemInfo is FileInfo)
                        {
                            if (((FileInfo)currentJunk.FileSystemInfo).Exists)
                                ((FileInfo)currentJunk.FileSystemInfo).Delete();
                        }
                        else
                        {
                            if (((DirectoryInfo)currentJunk.FileSystemInfo).Exists)
                                ((DirectoryInfo)currentJunk.FileSystemInfo).Delete(true);
                        }

                        Definitions.List.JunkStuff.Remove(currentJunk);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Debug.WriteLine(ex);
                Functions.Logger.LogToFile(Functions.Logger.LogType.SLM, ex.ToString());
            }
        }

        private void LibraryCleaner_ButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((string)(sender as Button).Tag == "Refresh")
                {
                    foreach (Definitions.Library library in Definitions.List.Libraries)
                    {
                        library.UpdateJunks();
                    }
                }

                if (LibraryCleaner.Items.Count == 0)
                    return;

                if ((string)(sender as Button).Tag == "MoveAll")
                {
                    var TargetFolderBrowser = new System.Windows.Forms.FolderBrowserDialog();
                    System.Windows.Forms.DialogResult TargetFolderDialogResult = TargetFolderBrowser.ShowDialog();

                    if (TargetFolderDialogResult == System.Windows.Forms.DialogResult.OK)
                    {
                        if (Directory.GetDirectoryRoot(TargetFolderBrowser.SelectedPath) == TargetFolderBrowser.SelectedPath)
                        {
                            if (MessageBox.Show("Are you sure you like to move junks to root of disk?", "Root?", MessageBoxButton.YesNoCancel) != MessageBoxResult.Yes)
                                return;
                        }
                        
                        List<Definitions.List.JunkInfo> LibraryCleanerItems = LibraryCleaner.ItemsSource.OfType<Definitions.List.JunkInfo>().ToList();

                        foreach (Definitions.List.JunkInfo currentJunk in LibraryCleanerItems)
                        {
                            if (currentJunk.FileSystemInfo is FileInfo)
                            {
                                if (((FileInfo)currentJunk.FileSystemInfo).Exists)
                                    (currentJunk.FileSystemInfo as FileInfo).CopyTo(currentJunk.FileSystemInfo.Name, true);

                                currentJunk.FileSystemInfo.Delete();
                            }
                            else
                            {
                                if (((DirectoryInfo)currentJunk.FileSystemInfo).Exists)
                                {
                                    foreach(FileInfo currentFile in (currentJunk.FileSystemInfo as DirectoryInfo).EnumerateFileSystemInfos("*", SearchOption.AllDirectories).Where(x => x is FileInfo).ToList())
                                    {
                                        FileInfo newFile = new FileInfo(currentFile.FullName.Replace(currentJunk.Library.SteamAppsFolder.FullName, TargetFolderBrowser.SelectedPath));

                                        if (!newFile.Exists || (newFile.Length != currentFile.Length || newFile.LastWriteTime != currentFile.LastWriteTime))
                                        {
                                            if (!newFile.Directory.Exists)
                                            {
                                                newFile.Directory.Create();
                                            }

                                            currentFile.CopyTo(newFile.FullName, true);
                                        }
                                    }

                                    (currentJunk.FileSystemInfo as DirectoryInfo).Delete(true);
                                }
                            }

                            Definitions.List.JunkStuff.Remove(currentJunk);
                        }
                    }
                }
                else if ((string)(sender as Button).Tag == "DeleteAll")
                {
                    if (MessageBox.Show("Saved Games may be located within these folders, are you sure you want to remove them?", "There might be saved games in these folders?!", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        List<Definitions.List.JunkInfo> LibraryCleanerItems = LibraryCleaner.ItemsSource.OfType<Definitions.List.JunkInfo>().ToList();

                        foreach (Definitions.List.JunkInfo currentJunk in LibraryCleanerItems)
                        {
                            if (currentJunk.FileSystemInfo is FileInfo)
                            {
                                if (((FileInfo)currentJunk.FileSystemInfo).Exists)
                                    ((FileInfo)currentJunk.FileSystemInfo).Delete();
                            }
                            else
                            {
                                if (((DirectoryInfo)currentJunk.FileSystemInfo).Exists)
                                    ((DirectoryInfo)currentJunk.FileSystemInfo).Delete(true);
                            }

                            Definitions.List.JunkStuff.Remove(currentJunk);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Functions.Logger.LogToFile(Functions.Logger.LogType.SLM, ex.ToString());
            }
        }

        private void ViewLogsButton(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(Definitions.Directories.SLM.LogDirectory))
                Process.Start(Definitions.Directories.SLM.LogDirectory);
        }

        private void GetIPButton_Click(object sender, RoutedEventArgs e)
        {
            Functions.Network.UpdatePublicIP();
        }

        private void GetPortButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ListenPort = Functions.Network.GetAvailablePort();
        }

        private void ToggleSLMServerButton_Click(object sender, RoutedEventArgs e)
        {
            //ToggleSLMServer.Content = "Stop Server";
            //SLMServer.StartServer();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Framework.Network.Client SLMClient = new Framework.Network.Client();

            SLMClient.ConnectToServer();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
                {
                    if ((sender as Grid).DataContext as Definitions.List.TaskList is Definitions.List.TaskList)
                    {
                        if (((sender as Grid).DataContext as Definitions.List.TaskList).TargetGame.CommonFolder.Exists)
                            Process.Start(((sender as Grid).DataContext as Definitions.List.TaskList).TargetGame.CommonFolder.FullName);
                    }
                    else if (((sender as Grid).DataContext is Definitions.Game))
                    {
                        if (((sender as Grid).DataContext as Definitions.Game).CommonFolder.Exists)
                            Process.Start(((sender as Grid).DataContext as Definitions.Game).CommonFolder.FullName);
                    }
                    else if (((sender as Grid).DataContext is Definitions.Library))
                    {
                        if (((sender as Grid).DataContext as Definitions.Library).SteamAppsFolder.Exists)
                            Process.Start(((sender as Grid).DataContext as Definitions.Library).SteamAppsFolder.FullName);
                    }
                    else if (((sender as Grid).DataContext is Definitions.List.JunkInfo))
                    {
                        if (((sender as Grid).DataContext as Definitions.List.JunkInfo).FileSystemInfo.Exists)
                            Process.Start(((sender as Grid).DataContext as Definitions.List.JunkInfo).FileSystemInfo.FullName);
                    }
                }
            }
            catch (Exception ex)
            {
                Functions.Logger.LogToFile(Functions.Logger.LogType.SLM, ex.ToString());
            }
        }

        private void DonateButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Process.Start(Definitions.SLM.DonateButtonURL);
            }
            catch { }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Definitions.SLM.selectedLibrary != null)
                Functions.Games.UpdateMainForm(Definitions.SLM.selectedLibrary, Properties.Settings.Default.SearchText);
        }

        private void ResetSearchTextButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SearchText = "";
        }

        private void HeaderImageClearButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Directory.Exists(Definitions.Directories.SLM.HeaderImageDirectory))
                {
                    Directory.Delete(Definitions.Directories.SLM.HeaderImageDirectory, true);
                    MessageBox.Show("Header Image Cache cleared.");
                }
            }
            catch { }
        }
    }
}
