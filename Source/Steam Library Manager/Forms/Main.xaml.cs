using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Steam_Library_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///
    public partial class Main
    {
        public static Main FormAccessor;
        public Framework.AsyncObservableCollection<string> TaskManager_Logs = new Framework.AsyncObservableCollection<string>();
        //Framework.Network.Server SLMServer = new Framework.Network.Server();

        public Main()
        {
            InitializeComponent();

            UpdateBindings();
            MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Accented;
        }

        private void UpdateBindings()
        {
            try
            {
                Definitions.SLM.RavenClient.Release = Definitions.Updater.CurrentVersion.ToString();
                FormAccessor = this;
                Properties.Settings.Default.SearchText = "";

                LibraryPanel.ItemsSource = Definitions.List.Libraries;
                TaskPanel.ItemsSource = Framework.TaskManager.TaskList;
                TaskManager_LogsView.ItemsSource = TaskManager_Logs;

                LibraryCleaner.ItemsSource = Definitions.List.LCItems;
            }
            catch { }
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
            {
                Functions.Logger.StartLogger();
            }
        }

        private async void MainForm_ClosingAsync(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (e.Cancel) return;
            if (Framework.TaskManager.TaskList.Count(x => x.Active) > 0)
            {
                e.Cancel = true;

                if (await this.ShowMessageAsync("Quit application?",
                    "There are active tasked jobs available in Task Manager. Are you sure you want to quit SLM? This might result in a data loss.",
                    MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings()
                    {
                        AffirmativeButtonText = "Quit",
                        NegativeButtonText = "Cancel"
                    }).ConfigureAwait(true) != MessageDialogResult.Affirmative)
                {
                    return;
                }
            }

            Functions.SLM.OnClosing();
            Application.Current.Shutdown();
        }

        private async void LibraryGrid_Drop(object sender, DragEventArgs e)
        {
            try
            {
                Definitions.Library Library = ((Grid)sender).DataContext as Definitions.Library;

                if (AppPanel.SelectedItems.Count == 0 || Library == null)
                {
                    return;
                }

                Library.DirectoryInfo.Refresh();
                if (!Library.DirectoryInfo.Exists)
                {
                    return;
                }

                foreach (dynamic App in AppPanel.SelectedItems)
                {
                    if (App is Definitions.SteamAppInfo)
                    {
                        if (App.IsSteamBackup)
                        {
                            Process.Start(Path.Combine(Properties.Settings.Default.steamInstallationPath, "Steam.exe"), $"-install \"{App.InstallationDirectory}\"");
                        }
                        else
                        {
                            if (Library == App.Library || Library.Type == Definitions.Enums.LibraryType.Origin)
                            {
                                continue;
                            }

                            if (Framework.TaskManager.TaskList.ToList().Count(x => x.SteamApp == App && x.TargetLibrary == Library) == 0)
                            {
                                Framework.TaskManager.AddTask(new Definitions.List.TaskInfo
                                {
                                    SteamApp = App,
                                    TargetLibrary = Library,
                                    TaskType = Definitions.Enums.TaskType.Copy
                                });
                            }
                            else
                            {
                                await this.ShowMessageAsync("Steam Library Manager", $"This item is already tasked.\n\nGame: {App.AppName}\nTarget Library: {Library.DirectoryInfo.FullName}").ConfigureAwait(true);
                            }
                        }
                    }
                    else if (App is Definitions.OriginAppInfo)
                    {
                        if (Library == App.Library || Library.Type != Definitions.Enums.LibraryType.Origin)
                            continue;

                        if (Framework.TaskManager.TaskList.ToList().Count(x => x.OriginApp == App && x.TargetLibrary == Library) == 0)
                        {
                            Framework.TaskManager.AddTask(new Definitions.List.TaskInfo
                            {
                                OriginApp = App,
                                TargetLibrary = Library,
                                TaskType = Definitions.Enums.TaskType.Copy
                            });
                        }
                        else
                        {
                            await this.ShowMessageAsync("Steam Library Manager", $"This item is already tasked.\n\nGame: {App.AppName}\nTarget Library: {Library.DirectoryInfo.FullName}").ConfigureAwait(true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Functions.Logger.LogToFile(Functions.Logger.LogType.SLM, ex.ToString());
                Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
            }
        }

        private void LibraryGrid_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
        }

        private async void LibraryPanel_Drop(object sender, DragEventArgs e)
        {
            try
            {
                string[] DroppedItems = (string[])e.Data.GetData(DataFormats.FileDrop, false);

                if (DroppedItems == null)
                {
                    return;
                }

                foreach (string DroppedItem in DroppedItems)
                {
                    FileInfo Info = new FileInfo(DroppedItem);

                    if ((Info.Attributes & FileAttributes.Directory) != 0)
                    {
                        var LibraryDialog = await this.ShowMessageAsync("Steam Library Manager", $"Select Library type you want to create in folder:\n{DroppedItem}", MessageDialogStyle.AffirmativeAndNegativeAndDoubleAuxiliary, new MetroDialogSettings
                        {
                            AffirmativeButtonText = "Steam",
                            NegativeButtonText = "SLM",
                            FirstAuxiliaryButtonText = "Origin",
                            SecondAuxiliaryButtonText = "Cancel"
                        }).ConfigureAwait(true);

                        switch (LibraryDialog)
                        {
                            // Steam
                            case MessageDialogResult.Affirmative:
                                if (!Functions.Steam.Library.IsLibraryExists(DroppedItem))
                                {
                                    if (Directory.GetDirectoryRoot(DroppedItem) != DroppedItem)
                                    {
                                        Functions.Steam.Library.CreateNew(DroppedItem, false);
                                    }
                                    else
                                    {
                                        await this.ShowMessageAsync("Steam Library Manager", "Libraries can not be created at root").ConfigureAwait(true);
                                    }
                                }
                                else
                                {
                                    await this.ShowMessageAsync("Steam Library Manager", "Library already exists at " + DroppedItem).ConfigureAwait(true);
                                }
                                break;
                            // SLM
                            case MessageDialogResult.Negative:
                                if (!Functions.SLM.Library.IsLibraryExists(DroppedItem))
                                {
                                    if (Directory.GetDirectoryRoot(DroppedItem) != DroppedItem)
                                    {
                                        Functions.SLM.Library.AddNewAsync(Info.FullName);
                                    }
                                    else
                                    {
                                        await this.ShowMessageAsync("Steam Library Manager", "Libraries can not be created at root").ConfigureAwait(true);
                                    }
                                }
                                else
                                {
                                    await this.ShowMessageAsync("Steam Library Manager", "Library already exists at " + DroppedItem).ConfigureAwait(true);
                                }
                                break;
                            // Origin
                            case MessageDialogResult.FirstAuxiliary:
                                if (!Functions.Origin.IsLibraryExists(DroppedItem))
                                {
                                    if (Directory.GetDirectoryRoot(DroppedItem) != DroppedItem)
                                    {
                                        Functions.Origin.AddNewAsync(Info.FullName);
                                    }
                                    else
                                    {
                                        await this.ShowMessageAsync("Steam Library Manager", "Libraries can not be created at root").ConfigureAwait(true);
                                    }
                                }
                                else
                                {
                                    await this.ShowMessageAsync("Steam Library Manager", "Library already exists at " + DroppedItem).ConfigureAwait(true);
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
            }
        }

        public void LibraryCMenuItem_Click(object sender, RoutedEventArgs e) => ((Definitions.Library)(sender as MenuItem)?.DataContext).ParseMenuItemAction((string)(sender as MenuItem)?.Tag);

        public void AppCMenuItem_Click(object sender, RoutedEventArgs e)
        {
            switch (Definitions.SLM.CurrentSelectedLibrary.Type)
            {
                case Definitions.Enums.LibraryType.Steam:
                case Definitions.Enums.LibraryType.SLM:
                    ((Definitions.SteamAppInfo)(sender as MenuItem)?.DataContext).ParseMenuItemActionAsync((string)(sender as MenuItem)?.Tag);
                    break;

                case Definitions.Enums.LibraryType.Origin:
                    ((Definitions.OriginAppInfo)(sender as MenuItem)?.DataContext).ParseMenuItemActionAsync((string)(sender as MenuItem)?.Tag);
                    break;
            }
        }

        private void RightWindowCommands_SettingsButton_Click(object sender, RoutedEventArgs e) => TabItem_Settings.IsSelected = true;

        private void CheckForUpdates_Click(object sender, RoutedEventArgs e) => Functions.Updater.CheckForUpdates(true);

        private void LibraryGrid_MouseDown(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Definitions.SLM.CurrentSelectedLibrary = LibraryPanel.SelectedItem as Definitions.Library;

                if (Definitions.SLM.CurrentSelectedLibrary == null)
                {
                    return;
                }

                if (Definitions.SLM.CurrentSelectedLibrary.Type == Definitions.Enums.LibraryType.SLM)
                {
                    if (Directory.Exists(Definitions.SLM.CurrentSelectedLibrary.DirectoryInfo.FullName))
                    {
                        Functions.SLM.Library.UpdateBackupLibrary(Definitions.SLM.CurrentSelectedLibrary);
                    }
                }

                // Update games list from current selection
                Functions.App.UpdateAppPanel(Definitions.SLM.CurrentSelectedLibrary);
            }
            catch (Exception ex)
            {
                Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
            }
        }

        private void TaskManager_Buttons_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch ((sender as Button)?.Tag)
                {
                    case "Start":
                    default:
                        Framework.TaskManager.Start();
                        Button_StartTaskManager.IsEnabled = false;
                        Button_PauseTaskManager.IsEnabled = true;
                        Button_StopTaskManager.IsEnabled = true;
                        break;

                    case "Pause":
                        Framework.TaskManager.Pause();
                        Button_PauseTaskManager.IsEnabled = false;
                        Button_StopTaskManager.IsEnabled = true;
                        break;

                    case "Stop":
                        Framework.TaskManager.Stop();
                        Button_PauseTaskManager.IsEnabled = false;
                        Button_StopTaskManager.IsEnabled = false;
                        break;

                    case "BackupUpdates":
                        Functions.Steam.Library.CheckForBackupUpdatesAsync();
                        break;

                    case "ClearCompleted":
                        if (Framework.TaskManager.TaskList.Count == 0)
                        {
                            return;
                        }

                        foreach (Definitions.List.TaskInfo CurrentTask in Framework.TaskManager.TaskList.ToList())
                        {
                            if (CurrentTask.Completed)
                            {
                                Framework.TaskManager.TaskList.Remove(CurrentTask);
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
            }
        }

        private async void TaskManager_ContextMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch ((sender as MenuItem)?.Tag)
                {
                    case "Remove":
                    default:
                        if (TaskPanel.SelectedItems.Count == 0)
                        {
                            return;
                        }

                        foreach (Definitions.List.TaskInfo CurrentTask in TaskPanel.SelectedItems.OfType<Definitions.List.TaskInfo>().ToList())
                        {
                            if (CurrentTask.Active && Framework.TaskManager.Status && !CurrentTask.Completed)
                            {
                                await this.ShowMessageAsync("Steam Library Manager", $"[{CurrentTask.SteamApp.AppName}] You can't remove an app from Task Manager which is currently being moved.\n\nPlease Stop the Task Manager first.").ConfigureAwait(true);
                            }
                            else
                            {
                                Framework.TaskManager.RemoveTask(CurrentTask);
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Functions.Logger.LogToFile(Functions.Logger.LogType.SLM, ex.ToString());
                Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
            }
        }

        private void AppSortingMethod_SelectionChanged(object sender, SelectionChangedEventArgs e) => Functions.App.UpdateAppPanel(Definitions.SLM.CurrentSelectedLibrary);

        private async void LibraryCleaner_ContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LibraryCleaner.SelectedItems.Count == 0)
                {
                    return;
                }

                foreach (Definitions.List.JunkInfo Junk in LibraryCleaner.SelectedItems.OfType<Definitions.List.JunkInfo>().ToList())
                {
                    if ((string)(sender as MenuItem)?.Tag == "Explorer")
                    {
                        Junk.FSInfo.Refresh();

                        if (Junk.FSInfo.Exists)
                            Process.Start(Junk.FSInfo.FullName);
                    }
                    else
                    {
                        Junk.FSInfo.Refresh();

                        if (Junk.FSInfo is FileInfo)
                        {
                            if (Junk.FSInfo.Exists)
                            {
                                File.SetAttributes(Junk.FSInfo.FullName, FileAttributes.Normal);
                                await Task.Run(() => Junk.FSInfo.Delete()).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            if (((DirectoryInfo)Junk.FSInfo).Exists)
                            {
                                await Task.Run(() => ((DirectoryInfo)Junk.FSInfo).Delete(true)).ConfigureAwait(false);
                            }
                        }

                        Definitions.List.LCItems.Remove(Junk);
                    }
                }
            }
            catch (IOException ioex)
            {
                Functions.Logger.LogToFile(Functions.Logger.LogType.SLM, ioex.ToString());
            }
            catch (UnauthorizedAccessException uaex)
            {
                Functions.Logger.LogToFile(Functions.Logger.LogType.SLM, uaex.ToString());
            }
            catch (Exception ex)
            {
                Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
                Functions.Logger.LogToFile(Functions.Logger.LogType.SLM, ex.ToString());
            }
        }

        // Library Cleaner Button actions
        private async void LibraryCleaner_ButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((string)(sender as Button)?.Tag == "Refresh")
                {
                    foreach (Definitions.Library Library in Definitions.List.Libraries.Where(x => x.DirectoryInfo.Exists && (x.Type == Definitions.Enums.LibraryType.Steam || x.Type == Definitions.Enums.LibraryType.SLM)))
                    {
                        Library.Steam.UpdateJunks();
                    }
                }

                if (LibraryCleaner.Items.Count == 0)
                {
                    return;
                }

                if ((string)(sender as Button)?.Tag == "MoveAll")
                {
                    var TargetFolderBrowser = new System.Windows.Forms.FolderBrowserDialog();
                    System.Windows.Forms.DialogResult TargetFolderDialogResult = TargetFolderBrowser.ShowDialog();

                    if (TargetFolderDialogResult == System.Windows.Forms.DialogResult.OK)
                    {
                        if (Directory.GetDirectoryRoot(TargetFolderBrowser.SelectedPath) == TargetFolderBrowser.SelectedPath
                            && await this.ShowMessageAsync("Root path selected?", "Are you sure you like to move junks to root of disk?", MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(true) != MessageDialogResult.Affirmative)
                        {
                            return;
                        }

                        var ProgressInformationMessage = await this.ShowProgressAsync("Please wait...", "Relocating junk files as you have requested.").ConfigureAwait(true);
                        ProgressInformationMessage.SetIndeterminate();

                        foreach (Definitions.List.JunkInfo Junk in LibraryCleaner.ItemsSource.OfType<Definitions.List.JunkInfo>().ToList())
                        {
                            if (Junk.FSInfo is FileInfo)
                            {
                                Junk.FSInfo.Refresh();
                                if (Junk.FSInfo.Exists)
                                {
                                    ProgressInformationMessage.SetMessage("Relocating file:\n\n" + Junk.FSInfo.FullName);
                                    ((FileInfo)Junk.FSInfo).CopyTo(Path.Combine(TargetFolderBrowser.SelectedPath, Junk.FSInfo.Name), true);
                                }

                                File.SetAttributes(Junk.FSInfo.FullName, FileAttributes.Normal);
                                await Task.Run(() => Junk.FSInfo.Delete()).ConfigureAwait(false);
                            }
                            else
                            {
                                Junk.FSInfo.Refresh();
                                if (Junk.FSInfo.Exists)
                                {
                                    foreach (FileInfo currentFile in ((DirectoryInfo)Junk.FSInfo).EnumerateFileSystemInfos("*", SearchOption.AllDirectories).Where(x => x is FileInfo).ToList())
                                    {
                                        FileInfo newFile = new FileInfo(currentFile.FullName.Replace(Junk.Library.Steam.SteamAppsFolder.FullName, TargetFolderBrowser.SelectedPath));

                                        if (!newFile.Exists || (newFile.Length != currentFile.Length || newFile.LastWriteTime != currentFile.LastWriteTime))
                                        {
                                            if (!newFile.Directory.Exists)
                                            {
                                                newFile.Directory.Create();
                                            }

                                            ProgressInformationMessage.SetMessage("Relocating file:\n\n" + currentFile.FullName);
                                            await Task.Run(() => currentFile.CopyTo(newFile.FullName, true)).ConfigureAwait(false);
                                        }
                                    }

                                    ProgressInformationMessage.SetMessage("Removing old directory:\n\n" + (Junk.FSInfo as DirectoryInfo)?.FullName);
                                    await Task.Run(() => (Junk.FSInfo as DirectoryInfo)?.Delete(true)).ConfigureAwait(false);
                                }
                            }

                            Definitions.List.LCItems.Remove(Junk);
                        }

                        await ProgressInformationMessage.CloseAsync().ConfigureAwait(false);
                    }
                }
                else if ((string)(sender as Button)?.Tag == "DeleteAll")
                {
                    if (await this.ShowMessageAsync("There might be saved games in these folders?!", "Saved Games may be located within these folders, are you sure you want to remove them?", MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(true) == MessageDialogResult.Affirmative)
                    {
                        var ProgressInformationMessage = await this.ShowProgressAsync("Please wait...", "Removing junk files as you have requested.", true).ConfigureAwait(true);
                        ProgressInformationMessage.SetIndeterminate();

                        foreach (Definitions.List.JunkInfo Junk in LibraryCleaner.ItemsSource.OfType<Definitions.List.JunkInfo>().ToList())
                        {
                            if (Junk.FSInfo is FileInfo)
                            {
                                Junk.FSInfo.Refresh();
                                if (Junk.FSInfo.Exists)
                                {
                                    File.SetAttributes(Junk.FSInfo.FullName, FileAttributes.Normal);
                                    ProgressInformationMessage.SetMessage("Deleting file:\n\n" + Junk.FSInfo.FullName);
                                    await Task.Run(() => Junk.FSInfo.Delete()).ConfigureAwait(false);
                                }
                            }
                            else
                            {
                                Junk.FSInfo.Refresh();
                                if (Junk.FSInfo.Exists)
                                {
                                    ProgressInformationMessage.SetMessage("Deleting Folder:\n\n" + Junk.FSInfo.FullName);
                                    await Task.Run(() => ((DirectoryInfo)Junk.FSInfo).Delete(true)).ConfigureAwait(false);
                                }
                            }

                            Definitions.List.LCItems.Remove(Junk);
                        }

                        await ProgressInformationMessage.CloseAsync().ConfigureAwait(false);
                    }
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (Exception ex)
            {
                Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
                Functions.Logger.LogToFile(Functions.Logger.LogType.SLM, ex.ToString());
            }
        }

        private void ViewLogsButton(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(Definitions.Directories.SLM.Log))
            {
                Process.Start(Definitions.Directories.SLM.Log);
            }
        }

        //private void GetIPButton_Click(object sender, RoutedEventArgs e) => Functions.Network.UpdatePublicIP();

        //private void GetPortButton_Click(object sender, RoutedEventArgs e) => Properties.Settings.Default.ListenPort = Functions.Network.GetAvailablePort();

        //private void ToggleSLMServerButton_Click(object sender, RoutedEventArgs e)
        //{
        //    //ToggleSLMServer.Content = "Stop Server";
        //    //SLMServer.StartServer();
        //}

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    Framework.Network.Client SLMClient = new Framework.Network.Client();

        //    SLMClient.ConnectToServer();
        //}

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
                {
                    if ((sender as Grid)?.DataContext as Definitions.List.TaskInfo is Definitions.List.TaskInfo)
                    {
                        if (((sender as Grid)?.DataContext as Definitions.List.TaskInfo)?.SteamApp.CommonFolder.Exists == true)
                        {
                            Process.Start(((sender as Grid)?.DataContext as Definitions.List.TaskInfo)?.SteamApp.CommonFolder.FullName);
                        }
                    }
                    else if ((sender as Grid)?.DataContext is Definitions.SteamAppInfo)
                    {
                        if (((sender as Grid)?.DataContext as Definitions.SteamAppInfo)?.CommonFolder.Exists == true)
                        {
                            Process.Start(((sender as Grid)?.DataContext as Definitions.SteamAppInfo)?.CommonFolder.FullName);
                        }
                    }
                    else if ((sender as Grid)?.DataContext is Definitions.Library)
                    {
                        if (((sender as Grid)?.DataContext as Definitions.Library)?.Steam != null)
                        {
                            if (((sender as Grid)?.DataContext as Definitions.Library)?.Steam.SteamAppsFolder.Exists == true)
                            {
                                Process.Start(((sender as Grid)?.DataContext as Definitions.Library)?.Steam.SteamAppsFolder.FullName);
                            }
                        }

                        if (((sender as Grid)?.DataContext as Definitions.Library)?.Origin != null)
                        {
                            if (Directory.Exists(((sender as Grid)?.DataContext as Definitions.Library)?.Origin.FullPath))
                            {
                                Process.Start(((sender as Grid)?.DataContext as Definitions.Library)?.Origin.FullPath);
                            }
                        }
                    }
                    else if ((sender as Grid)?.DataContext is Definitions.List.JunkInfo)
                    {
                        if (((sender as Grid)?.DataContext as Definitions.List.JunkInfo)?.FSInfo.Exists == true)
                        {
                            Process.Start(((sender as Grid)?.DataContext as Definitions.List.JunkInfo)?.FSInfo.FullName);
                        }
                    }
                    else if ((sender as Grid)?.DataContext is Definitions.OriginAppInfo)
                    {
                        if (((sender as Grid)?.DataContext as Definitions.OriginAppInfo)?.InstallationDirectory.Exists == true)
                        {
                            Process.Start(((sender as Grid)?.DataContext as Definitions.OriginAppInfo)?.InstallationDirectory.FullName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Functions.Logger.LogToFile(Functions.Logger.LogType.SLM, ex.ToString());
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Definitions.SLM.CurrentSelectedLibrary != null)
            {
                Functions.App.UpdateAppPanel(Definitions.SLM.CurrentSelectedLibrary);
            }
        }

        private async void HeaderImageClearButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Directory.Exists(Definitions.Directories.SLM.Cache))
                {
                    foreach (string file in Directory.EnumerateFiles(Definitions.Directories.SLM.Cache, "*.jpg"))
                    {
                        File.Delete(file);
                    }
                }

                await this.ShowMessageAsync("Steam Library Manager", "Header Image Cache cleared.").ConfigureAwait(true);
            }
            catch { }
        }

        private void DonateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(Definitions.SLM.DonateButtonURL);
            }
            catch { }
        }

        private void UpdateCustomTheme(string Key, Color value)
        {
            try
            {
                Tuple<AppTheme, Accent> Style = ThemeManager.DetectAppStyle(Application.Current);

                switch (Key)
                {
                    case "TextBrush":
                        Style.Item1.Resources["BlackBrush"] = GetSolidColorBrush(value);
                        Style.Item1.Resources["LabelTextBrush"] = GetSolidColorBrush(value);
                        Style.Item1.Resources["TextBrush"] = GetSolidColorBrush(value);
                        Style.Item1.Resources["ControlTextBrush"] = GetSolidColorBrush(value);
                        Style.Item1.Resources["MenuTextBrush"] = GetSolidColorBrush(value);
                        break;

                    case "GrayNormalBrush":
                        Style.Item1.Resources["GrayNormalBrush"] = GetSolidColorBrush(value);
                        break;

                    case "WhiteBrush":
                    case "ControlBackgroundBrush":
                    case "WindowBackgroundBrush":
                    case "TransparentWhiteBrush":
                    case "GrayBrush1":
                    case "GrayBrush2":
                    case "GrayBrush7":
                    case "GrayBrush8":
                    case "GrayBrush10":
                        Style.Item1.Resources[Key] = GetSolidColorBrush(value);
                        break;

                    case "MenuItemBackgroundBrush":
                        Style.Item1.Resources[Key] = GetSolidColorBrush(value);
                        Style.Item1.Resources["ContextMenuBackgroundBrush"] = GetSolidColorBrush(value);
                        Style.Item1.Resources["Gray7"] = value;
                        break;
                }

                App.CreateThemeFrom("CustomTheme.xaml", Style.Item1.Resources);

                if (Properties.Settings.Default.BaseTheme == "CustomTheme")
                {
                    ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(Properties.Settings.Default.ThemeAccent), Style.Item1);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            try
            {
                UpdateCustomTheme(((ColorPickerLib.Controls.ColorPicker)sender).Tag.ToString(), e.NewValue.Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private static SolidColorBrush GetSolidColorBrush(Color color, double opacity = 1d)
        {
            var brush = new SolidColorBrush(color) { Opacity = opacity };
            brush.Freeze();
            return brush;
        }

        private void RightWindowCommands_PatreonButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.patreon.com/revoland");
        }

        private void RightWindowCommands_DiscordButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://discordapp.com/invite/Rwvs9Ng");
        }
    }
}