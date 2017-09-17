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
        public static Main FormAccessor;
        public Framework.AsyncObservableCollection<string> TaskManager_Logs = new Framework.AsyncObservableCollection<string>();
        //Framework.Network.Server SLMServer = new Framework.Network.Server();

        public Main()
        {
            InitializeComponent();

            UpdateBindings();
        }

        void UpdateBindings()
        {
            FormAccessor = this;

            Properties.Settings.Default.SearchText = "";

            LibraryPanel.ItemsSource = Definitions.List.SteamLibraries;
            TaskPanel.ItemsSource = Framework.TaskManager.TaskList;
            TaskManager_LogsView.ItemsSource = TaskManager_Logs;

            LibraryCMenuItems.ItemsSource = Definitions.List.LibraryCMenuItems;
            AppCMenuItems.ItemsSource = Definitions.List.AppCMenuItems;

            LibraryCleaner.ItemsSource = Definitions.List.Junks;
        }

        private void MainForm_Loaded(object sender, RoutedEventArgs e)
        {
            Functions.SLM.OnLoad();

            GeneralSettingsGroupBox.DataContext = new Definitions.Settings();
            QuickSettings.DataContext = GeneralSettingsGroupBox.DataContext;

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
                Definitions.Steam.Library Library = (sender as Grid).DataContext as Definitions.Steam.Library;

                if (AppPanel.SelectedItems.Count == 0 || Library == null)
                    return;

                foreach (Definitions.Steam.AppInfo App in AppPanel.SelectedItems)
                {
                    if (Library.IsOffline)
                    {
                        if (!Directory.Exists(Library.FullPath))
                            continue;
                        else
                            Functions.Library.UpdateBackupLibraryAsync(Library);
                    }

                    if (Library == App.Library && !App.SteamBackup)
                        continue;

                    if (App.SteamBackup)
                        Process.Start(Path.Combine(Properties.Settings.Default.steamInstallationPath, "Steam.exe"), $"-install \"{App.InstallationPath}\"");
                    else
                    {
                        if (Framework.TaskManager.TaskList.Count(x => x.TargetApp == App && x.TargetLibrary == Library) == 0)
                        {
                            Definitions.List.TaskList newTask = new Definitions.List.TaskList
                            {
                                TargetApp = App,
                                TargetLibrary = Library
                            };

                            Framework.TaskManager.AddTask(newTask);

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
                            MessageBox.Show($"This item is already tasked.\n\nGame: {App.AppName}\nTarget Library: {Library.FullPath}");
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
            string[] DroppedItems = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            if (DroppedItems == null) return;

            foreach (string DroppedItem in DroppedItems)
            {
                FileInfo Info = new FileInfo(DroppedItem);

                if (Info.Attributes.HasFlag(FileAttributes.Directory))
                {
                    if (!Functions.Library.IsLibraryExists(DroppedItem))
                    {
                        if (Directory.GetDirectoryRoot(DroppedItem) != DroppedItem)
                        {
                            bool IsNewLibraryForBackup = false;
                            MessageBoxResult LibraryType = MessageBox.Show("Is this selected folder going to be used for backups?", "SLM library or Steam library?", MessageBoxButton.YesNoCancel);

                            if (LibraryType == MessageBoxResult.Cancel)
                                return;
                            else if (LibraryType == MessageBoxResult.Yes)
                                IsNewLibraryForBackup = true;

                            Functions.Library.CreateNewLibraryAsync(Info.FullName, IsNewLibraryForBackup);
                        }
                        else
                            MessageBox.Show("Libraries can not be created at root");
                    }
                    else
                        MessageBox.Show("Library already exists at " + DroppedItem);
                }
            }
        }

        private void LibraryCMenuItem_Click(object sender, RoutedEventArgs e) => ((Definitions.Steam.Library)(sender as MenuItem).DataContext).ParseMenuItemAction((string)(sender as MenuItem).Tag);

        private void Gamelibrary_ContextMenuItem_Click(object sender, RoutedEventArgs e) => ((Definitions.Steam.AppInfo)(sender as MenuItem).DataContext).ParseMenuItemAction((string)(sender as MenuItem).Tag);

        private void LibraryDataGridMenuItem_Click(object sender, RoutedEventArgs e)
        {
            int SelectedIndex = LibraryCMenuItems.SelectedIndex;

            if (SelectedIndex == -1 || SelectedIndex >= Definitions.List.LibraryCMenuItems.Count)
                return;

            switch(((MenuItem)sender).Tag.ToString())
            {
                case "moveUp":
                    if (SelectedIndex < 1)
                        return;

                    Definitions.List.LibraryCMenuItems.Move(SelectedIndex, SelectedIndex - 1);
                    break;

                case "moveDown":
                    if (SelectedIndex == Definitions.List.LibraryCMenuItems.Count - 1)
                        return;

                    Definitions.List.LibraryCMenuItems.Move(SelectedIndex, SelectedIndex + 1);
                    break;
            }
        }

        private void AppDataGridMenuItem_Click(object sender, RoutedEventArgs e)
        {

            int SelectedIndex = AppCMenuItems.SelectedIndex;

            if (SelectedIndex == -1 || SelectedIndex >= Definitions.List.AppCMenuItems.Count)
                return;

            switch (((MenuItem)sender).Tag.ToString())
            {
                case "moveUp":
                    if (SelectedIndex < 1)
                        return;

                    Definitions.List.AppCMenuItems.Move(SelectedIndex, SelectedIndex - 1);
                    break;

                case "moveDown":
                    if (SelectedIndex == Definitions.List.AppCMenuItems.Count - 1)
                        return;

                    Definitions.List.AppCMenuItems.Move(SelectedIndex, SelectedIndex + 1);
                    break;
            }
        }

        private void CheckForUpdates_Click(object sender, RoutedEventArgs e) => Functions.Updater.CheckForUpdates(true);

        private void LibraryGrid_MouseDown(object sender, SelectionChangedEventArgs e)
        {
            Definitions.SLM.CurrentSelectedLibrary = LibraryPanel.SelectedItem as Definitions.Steam.Library;

            if (Definitions.SLM.CurrentSelectedLibrary == null)
                return;

            if (Directory.Exists(Definitions.SLM.CurrentSelectedLibrary.FullPath) && Definitions.SLM.CurrentSelectedLibrary.IsOffline)
            {
                Functions.Library.UpdateBackupLibraryAsync(Definitions.SLM.CurrentSelectedLibrary);
            }

            // Update games list from current selection
            Functions.App.UpdateAppPanel(Definitions.SLM.CurrentSelectedLibrary);
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
                    if (Framework.TaskManager.TaskList.Count == 0)
                        return;

                    foreach (Definitions.List.TaskList CurrentTask in Framework.TaskManager.TaskList.ToList())
                    {
                        if (CurrentTask.Completed)
                            Framework.TaskManager.TaskList.Remove(CurrentTask);
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
                        if (TaskPanel.SelectedItems.Count == 0)
                            return;

                        List<Definitions.List.TaskList> SelectedItems = TaskPanel.SelectedItems.OfType<Definitions.List.TaskList>().ToList();

                        foreach (Definitions.List.TaskList CurrentTask in SelectedItems)
                        {
                            if (CurrentTask.Moving && Framework.TaskManager.Status && !CurrentTask.Completed)
                                MessageBox.Show($"[{CurrentTask.TargetApp.AppName}] You can't remove an app from Task Manager which is currently being moven.\n\nPlease Stop the Task Manager first.");
                            else
                            {
                                Framework.TaskManager.RemoveTask(CurrentTask);
                                TaskPanel.Items.Remove(CurrentTask);
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

        private void GameSortingMethod_SelectionChanged(object sender, SelectionChangedEventArgs e) => Functions.App.UpdateAppPanel(Definitions.SLM.CurrentSelectedLibrary);

        private void LibraryCleaner_ContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LibraryCleaner.SelectedItems.Count == 0)
                    return;

                List<Definitions.List.JunkInfo> SelectedJunks = LibraryCleaner.SelectedItems.OfType<Definitions.List.JunkInfo>().ToList();

                foreach (Definitions.List.JunkInfo Junk in SelectedJunks)
                {
                    if ((string)(sender as MenuItem).Tag == "Explorer")
                    {
                        Process.Start(Junk.FSInfo.FullName);
                    }
                    else
                    {
                        if (Junk.FSInfo is FileInfo)
                        {
                            if (((FileInfo)Junk.FSInfo).Exists)
                                ((FileInfo)Junk.FSInfo).Delete();
                        }
                        else
                        {
                            if (((DirectoryInfo)Junk.FSInfo).Exists)
                                ((DirectoryInfo)Junk.FSInfo).Delete(true);
                        }

                        Definitions.List.Junks.Remove(Junk);
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
                    foreach (Definitions.Steam.Library Library in Definitions.List.SteamLibraries)
                    {
                        Library.UpdateJunks();
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

                        foreach (Definitions.List.JunkInfo Junk in LibraryCleanerItems)
                        {
                            if (Junk.FSInfo is FileInfo)
                            {
                                if (((FileInfo)Junk.FSInfo).Exists)
                                    (Junk.FSInfo as FileInfo).CopyTo(Junk.FSInfo.Name, true);

                                Junk.FSInfo.Delete();
                            }
                            else
                            {
                                if (((DirectoryInfo)Junk.FSInfo).Exists)
                                {
                                    foreach(FileInfo currentFile in (Junk.FSInfo as DirectoryInfo).EnumerateFileSystemInfos("*", SearchOption.AllDirectories).Where(x => x is FileInfo).ToList())
                                    {
                                        FileInfo newFile = new FileInfo(currentFile.FullName.Replace(Junk.Library.SteamAppsFolder.FullName, TargetFolderBrowser.SelectedPath));

                                        if (!newFile.Exists || (newFile.Length != currentFile.Length || newFile.LastWriteTime != currentFile.LastWriteTime))
                                        {
                                            if (!newFile.Directory.Exists)
                                            {
                                                newFile.Directory.Create();
                                            }

                                            currentFile.CopyTo(newFile.FullName, true);
                                        }
                                    }

                                    (Junk.FSInfo as DirectoryInfo).Delete(true);
                                }
                            }

                            Definitions.List.Junks.Remove(Junk);
                        }
                    }
                }
                else if ((string)(sender as Button).Tag == "DeleteAll")
                {
                    if (MessageBox.Show("Saved Games may be located within these folders, are you sure you want to remove them?", "There might be saved games in these folders?!", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        List<Definitions.List.JunkInfo> LibraryCleanerItems = LibraryCleaner.ItemsSource.OfType<Definitions.List.JunkInfo>().ToList();

                        foreach (Definitions.List.JunkInfo Junk in LibraryCleanerItems)
                        {
                            if (Junk.FSInfo is FileInfo)
                            {
                                if (((FileInfo)Junk.FSInfo).Exists)
                                    ((FileInfo)Junk.FSInfo).Delete();
                            }
                            else
                            {
                                if (((DirectoryInfo)Junk.FSInfo).Exists)
                                    ((DirectoryInfo)Junk.FSInfo).Delete(true);
                            }

                            Definitions.List.Junks.Remove(Junk);
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
            if (Directory.Exists(Definitions.Directories.SLM.Log))
                Process.Start(Definitions.Directories.SLM.Log);
        }

        private void GetIPButton_Click(object sender, RoutedEventArgs e) => Functions.Network.UpdatePublicIP();

        private void GetPortButton_Click(object sender, RoutedEventArgs e) => Properties.Settings.Default.ListenPort = Functions.Network.GetAvailablePort();

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
                        if (((sender as Grid).DataContext as Definitions.List.TaskList).TargetApp.CommonFolder.Exists)
                            Process.Start(((sender as Grid).DataContext as Definitions.List.TaskList).TargetApp.CommonFolder.FullName);
                    }
                    else if (((sender as Grid).DataContext is Definitions.Steam.AppInfo))
                    {
                        if (((sender as Grid).DataContext as Definitions.Steam.AppInfo).CommonFolder.Exists)
                            Process.Start(((sender as Grid).DataContext as Definitions.Steam.AppInfo).CommonFolder.FullName);
                    }
                    else if (((sender as Grid).DataContext is Definitions.Steam.Library))
                    {
                        if (((sender as Grid).DataContext as Definitions.Steam.Library).SteamAppsFolder.Exists)
                            Process.Start(((sender as Grid).DataContext as Definitions.Steam.Library).SteamAppsFolder.FullName);
                    }
                    else if (((sender as Grid).DataContext is Definitions.List.JunkInfo))
                    {
                        if (((sender as Grid).DataContext as Definitions.List.JunkInfo).FSInfo.Exists)
                            Process.Start(((sender as Grid).DataContext as Definitions.List.JunkInfo).FSInfo.FullName);
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
            if (Definitions.SLM.CurrentSelectedLibrary != null)
                Functions.App.UpdateAppPanel(Definitions.SLM.CurrentSelectedLibrary);
        }

        private void ResetSearchTextButton_Click(object sender, RoutedEventArgs e) => Properties.Settings.Default.SearchText = "";

        private void HeaderImageClearButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Directory.Exists(Definitions.Directories.SLM.HeaderImage))
                {
                    Directory.Delete(Definitions.Directories.SLM.HeaderImage, true);
                    MessageBox.Show("Header Image Cache cleared.");
                }
            }
            catch { }
        }
    }
}
