using MahApps.Metro.Controls.Dialogs;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Steam_Library_Manager.Forms
{
    /// <summary>
    /// Interaction logic for LibraryCleanerView.xaml
    /// </summary>
    public partial class LibraryCleanerView
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public LibraryCleanerView() => InitializeComponent();

        private bool _toggleItemList;

        private async void LibraryCleaner_ContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LibraryCleaner.SelectedItems.Count == 0)
                {
                    return;
                }

                foreach (var junk in LibraryCleaner.SelectedItems.OfType<Definitions.List.JunkInfo>().ToList())
                {
                    switch ((string)(sender as MenuItem)?.Tag)
                    {
                        default:
                        case "Explorer":
                            junk.FSInfo.Refresh();

                            if (junk.FSInfo.Exists)
                                Process.Start(junk.FSInfo.FullName);
                            break;

                        case "Delete":
                            junk.FSInfo.Refresh();

                            if (junk.FSInfo is FileInfo)
                            {
                                if (junk.FSInfo.Exists)
                                {
                                    File.SetAttributes(junk.FSInfo.FullName, FileAttributes.Normal);
                                    await Task.Run(() => junk.FSInfo.Delete()).ConfigureAwait(true);
                                }
                            }
                            else
                            {
                                if (((DirectoryInfo)junk.FSInfo).Exists)
                                {
                                    await Task.Run(() => ((DirectoryInfo)junk.FSInfo).Delete(true)).ConfigureAwait(true);
                                }
                            }

                            Definitions.List.LcItems.Remove(junk);
                            break;

                        case "Ignore":
                            if (Properties.Settings.Default.IgnoredJunks == null)
                            {
                                Properties.Settings.Default.IgnoredJunks = new System.Collections.Specialized.StringCollection();
                            }

                            Properties.Settings.Default.IgnoredJunks.Add(junk.FSInfo.FullName);
                            Definitions.List.LcItems.Remove(junk);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }

        private void IgnoredItems_ContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var junk in IgnoredItems.SelectedItems.OfType<string>().ToList())
                {
                    switch ((string)(sender as MenuItem)?.Tag)
                    {
                        case "Explorer":
                            if (File.Exists(junk) || Directory.Exists(junk))
                                Process.Start(junk);
                            break;

                        case "Ignore":
                            Properties.Settings.Default.IgnoredJunks.Remove(junk);
                            break;

                        case "Delete":
                            if (File.Exists(junk))
                            {
                                File.Delete(junk);
                            }
                            else if (Directory.Exists(junk))
                            {
                                Directory.Delete(junk, true);
                            }

                            Properties.Settings.Default.IgnoredJunks.Remove(junk);
                            break;
                    }
                }

                IgnoredItems.ItemsSource = Properties.Settings.Default.IgnoredJunks;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }

        // Library Cleaner Button actions
        private async void LibraryCleaner_ButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((string)(sender as Button)?.Tag == "Refresh")
                {
                    foreach (var library in Definitions.List.Libraries.Where(x => x.DirectoryInfo.Exists && (x.Type == Definitions.Enums.LibraryType.Steam || x.Type == Definitions.Enums.LibraryType.SLM)))
                    {
                        library.UpdateJunks();
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
                            && await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.Forms_LibraryCleaner_RootPathSelected)), Functions.SLM.Translate(nameof(Properties.Resources.Forms_LibraryCleaner_RootPathSelectedMessage)), MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(true) != MessageDialogResult.Affirmative)
                        {
                            return;
                        }

                        var ProgressInformationMessage = await Main.FormAccessor.ShowProgressAsync(Functions.SLM.Translate(nameof(Properties.Resources.PleaseWait)), Functions.SLM.Translate(nameof(Properties.Resources.Forms_LibraryCleaner_MovingFiles))).ConfigureAwait(true);
                        ProgressInformationMessage.SetIndeterminate();

                        foreach (Definitions.List.JunkInfo Junk in LibraryCleaner.ItemsSource.OfType<Definitions.List.JunkInfo>().ToList())
                        {
                            if (Junk.FSInfo is FileInfo)
                            {
                                Junk.FSInfo.Refresh();
                                if (Junk.FSInfo.Exists)
                                {
                                    ProgressInformationMessage.SetMessage(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.Forms_LibraryCleaner_MovingFile)), new { FileFullName = Junk.FSInfo.FullName }));
                                    ((FileInfo)Junk.FSInfo).CopyTo(Path.Combine(TargetFolderBrowser.SelectedPath, Junk.FSInfo.Name), true);
                                }

                                File.SetAttributes(Junk.FSInfo.FullName, FileAttributes.Normal);
                                await Task.Run(() => Junk.FSInfo.Delete()).ConfigureAwait(true);
                            }
                            else
                            {
                                Junk.FSInfo.Refresh();
                                if (Junk.FSInfo.Exists)
                                {
                                    foreach (FileInfo currentFile in ((DirectoryInfo)Junk.FSInfo).EnumerateFileSystemInfos("*", SearchOption.AllDirectories).Where(x => x is FileInfo).ToList())
                                    {
                                        FileInfo newFile = new FileInfo(currentFile.FullName.Replace(Junk.Library.DirectoryList["SteamApps"].FullName, TargetFolderBrowser.SelectedPath));

                                        if (!newFile.Exists || (newFile.Length != currentFile.Length || newFile.LastWriteTime != currentFile.LastWriteTime))
                                        {
                                            if (!newFile.Directory.Exists)
                                            {
                                                newFile.Directory.Create();
                                            }

                                            ProgressInformationMessage.SetMessage(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.Forms_LibraryCleaner_MovingFile)), new { FileFullName = currentFile.FullName }));
                                            await Task.Run(() => currentFile.CopyTo(newFile.FullName, true)).ConfigureAwait(true);
                                        }
                                    }

                                    ProgressInformationMessage.SetMessage(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.Forms_LibraryCleaner_DeletingDirectory)), new { DirectoryFullPath = Junk.FSInfo.FullName }));
                                    await Task.Run(() => (Junk.FSInfo as DirectoryInfo)?.Delete(true)).ConfigureAwait(true);
                                }
                            }

                            Definitions.List.LcItems.Remove(Junk);
                        }

                        await ProgressInformationMessage.CloseAsync().ConfigureAwait(true);
                    }
                }
                else if ((string)(sender as Button)?.Tag == "DeleteAll")
                {
                    if (await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.Forms_LibraryCleaner_DeleteWarning)), Functions.SLM.Translate(nameof(Properties.Resources.Forms_LibraryCleaner_DeleteWarningMessage)), MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(true) == MessageDialogResult.Affirmative)
                    {
                        var ProgressInformationMessage = await Main.FormAccessor.ShowProgressAsync(Functions.SLM.Translate(nameof(Properties.Resources.PleaseWait)), Functions.SLM.Translate(nameof(Properties.Resources.Forms_LibraryCleaner_Delete)), true).ConfigureAwait(true);
                        ProgressInformationMessage.SetIndeterminate();

                        foreach (Definitions.List.JunkInfo Junk in LibraryCleaner.ItemsSource.OfType<Definitions.List.JunkInfo>().ToList())
                        {
                            if (Junk.FSInfo is FileInfo)
                            {
                                Junk.FSInfo.Refresh();
                                if (Junk.FSInfo.Exists)
                                {
                                    File.SetAttributes(Junk.FSInfo.FullName, FileAttributes.Normal);
                                    ProgressInformationMessage.SetMessage(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.Forms_LibraryCleaner_MovingFile)), new { FileFullName = Junk.FSInfo.FullName }));
                                    await Task.Run(() => Junk.FSInfo.Delete()).ConfigureAwait(true);
                                }
                            }
                            else
                            {
                                Junk.FSInfo.Refresh();
                                if (Junk.FSInfo.Exists)
                                {
                                    ProgressInformationMessage.SetMessage(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.Forms_LibraryCleaner_DeletingDirectory)), new { DirectoryFullPath = Junk.FSInfo.FullName }));
                                    await Task.Run(() => ((DirectoryInfo)Junk.FSInfo).Delete(true)).ConfigureAwait(true);
                                }
                            }

                            Definitions.List.LcItems.Remove(Junk);
                        }

                        await ProgressInformationMessage.CloseAsync().ConfigureAwait(true);
                    }
                }
                else if ((string)(sender as Button)?.Tag == "ToggleIgnoredItems")
                {
                    if (_toggleItemList)
                    {
                        _toggleItemList = false;

                        IgnoredItems.Visibility = Visibility.Collapsed;
                        LibraryCleaner.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        _toggleItemList = true;

                        IgnoredItems.ItemsSource = Properties.Settings.Default.IgnoredJunks;
                        IgnoredItems.Visibility = Visibility.Visible;
                        LibraryCleaner.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (IOException ex)
            {
                Logger.Error(ex);

                if (Main.FormAccessor.IsAnyDialogOpen)
                {
                    await Main.FormAccessor.LibraryCleanerView.Dispatcher.Invoke(async delegate
                    {
                        await Main.FormAccessor.HideMetroDialogAsync(await Main.FormAccessor.GetCurrentDialogAsync<BaseMetroDialog>().ConfigureAwait(true)).ConfigureAwait(true);
                    }, System.Windows.Threading.DispatcherPriority.Normal).ConfigureAwait(true);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Error(ex);

                if (Main.FormAccessor.IsAnyDialogOpen)
                {
                    await Main.FormAccessor.LibraryCleanerView.Dispatcher.Invoke(async delegate
                    {
                        await Main.FormAccessor.HideMetroDialogAsync(await Main.FormAccessor.GetCurrentDialogAsync<BaseMetroDialog>().ConfigureAwait(true)).ConfigureAwait(true);
                    }, System.Windows.Threading.DispatcherPriority.Normal).ConfigureAwait(true);
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);

                if (Main.FormAccessor.IsAnyDialogOpen)
                {
                    await Main.FormAccessor.LibraryCleanerView.Dispatcher.Invoke(async delegate
                     {
                         await Main.FormAccessor.HideMetroDialogAsync(await Main.FormAccessor.GetCurrentDialogAsync<BaseMetroDialog>().ConfigureAwait(true)).ConfigureAwait(true);
                     }, System.Windows.Threading.DispatcherPriority.Normal).ConfigureAwait(true);
                }
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2 && ((sender as Grid)?.DataContext as Definitions.List.JunkInfo)?.FSInfo.Exists == true)
                {
                    Process.Start(((sender as Grid)?.DataContext as Definitions.List.JunkInfo)?.FSInfo.FullName);
                }

                if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2 && (File.Exists((sender as Grid)?.DataContext as string) || Directory.Exists((sender as Grid)?.DataContext as string)))
                {
                    Process.Start((sender as Grid)?.DataContext as string);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}