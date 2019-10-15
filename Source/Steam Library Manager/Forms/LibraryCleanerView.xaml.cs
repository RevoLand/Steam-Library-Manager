using MahApps.Metro.Controls.Dialogs;
using Steam_Library_Manager.Definitions.Enums;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Directory = Alphaleonis.Win32.Filesystem.Directory;
using DirectoryInfo = Alphaleonis.Win32.Filesystem.DirectoryInfo;
using File = Alphaleonis.Win32.Filesystem.File;
using FileInfo = Alphaleonis.Win32.Filesystem.FileInfo;

namespace Steam_Library_Manager.Forms
{
    /// <summary>
    /// Interaction logic for LibraryCleanerView.xaml
    /// </summary>
    public partial class LibraryCleanerView
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public LibraryCleanerView() => InitializeComponent();

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

                            Definitions.List.JunkItems.Remove(junk);
                            break;

                        case "Ignore":
                            Definitions.List.IgnoredJunkItems.Add(junk.FSInfo.FullName);
                            Definitions.List.JunkItems.Remove(junk);
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
                            Definitions.List.IgnoredJunkItems.Remove(junk);
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

                            Definitions.List.IgnoredJunkItems.Remove(junk);
                            break;
                    }
                }
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
                if (LibraryCleaner.Items.Count == 0)
                {
                    return;
                }

                switch ((string)(sender as Button)?.Tag)
                {
                    case "MoveAll":
                        {
                            var targetFolderBrowser = new System.Windows.Forms.FolderBrowserDialog();
                            var targetFolderDialogResult = targetFolderBrowser.ShowDialog();

                            if (targetFolderDialogResult != System.Windows.Forms.DialogResult.OK) return;

                            if (Directory.GetDirectoryRoot(targetFolderBrowser.SelectedPath) == targetFolderBrowser.SelectedPath
                                && await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.Forms_LibraryCleaner_RootPathSelected)), Functions.SLM.Translate(nameof(Properties.Resources.Forms_LibraryCleaner_RootPathSelectedMessage)), MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(true) != MessageDialogResult.Affirmative)
                            {
                                return;
                            }

                            var progressInformationMessage = await Main.FormAccessor.ShowProgressAsync(Functions.SLM.Translate(nameof(Properties.Resources.PleaseWait)), Functions.SLM.Translate(nameof(Properties.Resources.Forms_LibraryCleaner_MovingFiles))).ConfigureAwait(true);
                            progressInformationMessage.SetIndeterminate();

                            foreach (var junk in LibraryCleaner.ItemsSource.OfType<Definitions.List.JunkInfo>().ToList())
                            {
                                if (junk.FSInfo is FileInfo)
                                {
                                    junk.FSInfo.Refresh();
                                    if (junk.FSInfo.Exists)
                                    {
                                        progressInformationMessage.SetMessage(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.Forms_LibraryCleaner_MovingFile)), new { FileFullName = junk.FSInfo.FullName }));
                                        ((FileInfo)junk.FSInfo).CopyTo(Alphaleonis.Win32.Filesystem.Path.Combine(targetFolderBrowser.SelectedPath, junk.FSInfo.Name), true);
                                    }

                                    File.SetAttributes(junk.FSInfo.FullName, FileAttributes.Normal);
                                    await Task.Run(() => junk.FSInfo.Delete()).ConfigureAwait(true);
                                }
                                else
                                {
                                    junk.FSInfo.Refresh();
                                    if (junk.FSInfo.Exists)
                                    {
                                        foreach (FileInfo currentFile in ((DirectoryInfo)junk.FSInfo).EnumerateFileSystemInfos("*", SearchOption.AllDirectories).Where(x => x is FileInfo).ToList())
                                        {
                                            var newFile = new FileInfo(currentFile.FullName.Replace(junk.Library.DirectoryList["SteamApps"].FullName, targetFolderBrowser.SelectedPath));

                                            if (!newFile.Exists || (newFile.Length != currentFile.Length || newFile.LastWriteTime != currentFile.LastWriteTime))
                                            {
                                                if (!newFile.Directory.Exists)
                                                {
                                                    newFile.Directory.Create();
                                                }

                                                progressInformationMessage.SetMessage(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.Forms_LibraryCleaner_MovingFile)), new { FileFullName = currentFile.FullName }));
                                                await Task.Run(() => currentFile.CopyTo(newFile.FullName, true)).ConfigureAwait(true);
                                            }
                                        }

                                        progressInformationMessage.SetMessage(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.Forms_LibraryCleaner_DeletingDirectory)), new { DirectoryFullPath = junk.FSInfo.FullName }));
                                        await Task.Run(() => (junk.FSInfo as DirectoryInfo)?.Delete(true)).ConfigureAwait(true);
                                    }
                                }

                                Definitions.List.JunkItems.Remove(junk);
                            }

                            await progressInformationMessage.CloseAsync().ConfigureAwait(true);
                            targetFolderBrowser.Dispose();
                            break;
                        }

                    case "DeleteAll":
                        {
                            if (await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.Forms_LibraryCleaner_DeleteWarning)), Functions.SLM.Translate(nameof(Properties.Resources.Forms_LibraryCleaner_DeleteWarningMessage)), MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(true) == MessageDialogResult.Affirmative)
                            {
                                var progressInformationMessage = await Main.FormAccessor.ShowProgressAsync(Functions.SLM.Translate(nameof(Properties.Resources.PleaseWait)), Functions.SLM.Translate(nameof(Properties.Resources.Forms_LibraryCleaner_Delete)), true).ConfigureAwait(true);
                                progressInformationMessage.SetIndeterminate();

                                foreach (var junk in LibraryCleaner.ItemsSource.OfType<Definitions.List.JunkInfo>().ToList())
                                {
                                    if (junk.FSInfo is FileInfo)
                                    {
                                        junk.FSInfo.Refresh();
                                        if (junk.FSInfo.Exists)
                                        {
                                            File.SetAttributes(junk.FSInfo.FullName, FileAttributes.Normal);
                                            progressInformationMessage.SetMessage(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.Forms_LibraryCleaner_MovingFile)), new { FileFullName = junk.FSInfo.FullName }));
                                            await Task.Run(() => junk.FSInfo.Delete()).ConfigureAwait(true);
                                        }
                                    }
                                    else
                                    {
                                        junk.FSInfo.Refresh();
                                        if (junk.FSInfo.Exists)
                                        {
                                            progressInformationMessage.SetMessage(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.Forms_LibraryCleaner_DeletingDirectory)), new { DirectoryFullPath = junk.FSInfo.FullName }));
                                            await Task.Run(() => ((DirectoryInfo)junk.FSInfo).Delete(true)).ConfigureAwait(true);
                                        }
                                    }

                                    Definitions.List.JunkItems.Remove(junk);
                                }

                                await progressInformationMessage.CloseAsync().ConfigureAwait(true);
                            }

                            break;
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

        private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
        {
            RefreshItems((string)((Button)sender).Tag);
        }

        public void RefreshItems(string target)
        {
            try
            {
                switch (target)
                {
                    default:
                    case "Junks":

                        foreach (var library in Definitions.List.Libraries.Where(x => x.DirectoryInfo.Exists && (x.Type == LibraryType.Steam || x.Type == LibraryType.SLM)))
                        {
                            library.UpdateJunks();
                        }
                        break;

                    case "DupeItems":
                        foreach (var library in Definitions.List.Libraries.Where(x => x.DirectoryInfo.Exists && (x.Type == LibraryType.Steam || x.Type == LibraryType.SLM)))
                        {
                            library.UpdateDupes();
                        }
                        break;

                    case "IgnoredItems":
                        IgnoredItems.ItemsSource = Definitions.List.IgnoredJunkItems;
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                Debug.WriteLine(ex);
            }
        }

        private void DupeItem_OpenButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var senderBtn = sender as Button;
                var senderDupe = (Definitions.List.DupeInfo)senderBtn.DataContext;

                if ((string)senderBtn.Tag == "App1")
                {
                    if (senderDupe.App1.InstallationDirectory.Exists)
                    {
                        Process.Start(senderDupe.App1.InstallationDirectory.FullName);
                    }
                    else
                    {
                        Logger.Warn($"Tried to open a non existing directory: {senderDupe.App1.AppName} - {senderDupe.App1.InstallationDirectory.FullName}");
                    }
                }
                else
                {
                    if (senderDupe.App2.InstallationDirectory.Exists)
                    {
                        Process.Start(senderDupe.App2.InstallationDirectory.FullName);
                    }
                    else
                    {
                        Logger.Warn($"Tried to open a non existing directory: {senderDupe.App2.AppName} - {senderDupe.App2.InstallationDirectory.FullName}");
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }
        }

        private async void DupeItem_DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var senderBtn = sender as Button;
                var senderDupe = (Definitions.List.DupeInfo)senderBtn.DataContext;

                if ((string)senderBtn.Tag == "App1")
                {
                    if (await senderDupe.App1.DeleteFilesAsync())
                    {
                        senderDupe.App1.Library.Apps.Remove(senderDupe.App1);
                        Functions.SLM.Library.UpdateLibraryVisual();

                        if (Definitions.SLM.CurrentSelectedLibrary == senderDupe.App1.Library)
                            Functions.App.UpdateAppPanel(senderDupe.App1.Library);

                        Definitions.List.DupeItems.Remove(senderDupe);
                    }
                    else
                    {
                        Logger.Warn($"An error happened while deleting files for: {senderDupe.App1.AppName} - {senderDupe.App1.InstallationDirectory.FullName}");
                    }
                }
                else
                {
                    if (await senderDupe.App2.DeleteFilesAsync())
                    {
                        senderDupe.App2.Library.Apps.Remove(senderDupe.App2);
                        Functions.SLM.Library.UpdateLibraryVisual();

                        if (Definitions.SLM.CurrentSelectedLibrary == senderDupe.App2.Library)
                            Functions.App.UpdateAppPanel(senderDupe.App2.Library);

                        Definitions.List.DupeItems.Remove(senderDupe);
                    }
                    else
                    {
                        Logger.Warn($"An error happened while deleting files for: {senderDupe.App2.AppName} - {senderDupe.App2.InstallationDirectory.FullName}");
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }
        }

        private void IgnoredItems_ButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var junk in Definitions.List.IgnoredJunkItems.ToList())
                {
                    switch ((string)(sender as Button)?.Tag)
                    {
                        case "remove":

                            break;

                        case "delete":
                            if (File.Exists(junk))
                            {
                                File.Delete(junk);
                            }
                            else if (Directory.Exists(junk))
                            {
                                Directory.Delete(junk, true);
                            }
                            break;
                    }

                    Definitions.List.IgnoredJunkItems.Remove(junk);
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }
    }
}