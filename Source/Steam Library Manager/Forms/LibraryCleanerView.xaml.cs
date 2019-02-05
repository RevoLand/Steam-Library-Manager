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
    public partial class LibraryCleanerView : UserControl
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public LibraryCleanerView() => InitializeComponent();

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
                                await Task.Run(() => Junk.FSInfo.Delete());
                            }
                        }
                        else
                        {
                            if (((DirectoryInfo)Junk.FSInfo).Exists)
                            {
                                await Task.Run(() => ((DirectoryInfo)Junk.FSInfo).Delete(true));
                            }
                        }

                        Definitions.List.LCItems.Remove(Junk);
                    }
                }
            }
            catch (IOException ex)
            {
                logger.Fatal(ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Fatal(ex);
            }
            catch (Exception ex)
            {
                Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
                logger.Fatal(ex);
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
                            && await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.Forms_LibraryCleaner_RootPathSelected)), Functions.SLM.Translate(nameof(Properties.Resources.Forms_LibraryCleaner_RootPathSelectedMessage)), MessageDialogStyle.AffirmativeAndNegative) != MessageDialogResult.Affirmative)
                        {
                            return;
                        }

                        var ProgressInformationMessage = await Main.FormAccessor.ShowProgressAsync(Functions.SLM.Translate(nameof(Properties.Resources.PleaseWait)), Functions.SLM.Translate(nameof(Properties.Resources.Forms_LibraryCleaner_MovingFiles)));
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
                                await Task.Run(() => Junk.FSInfo.Delete());
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

                                            ProgressInformationMessage.SetMessage(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.Forms_LibraryCleaner_MovingFile)), new { FileFullName = currentFile.FullName }));
                                            await Task.Run(() => currentFile.CopyTo(newFile.FullName, true));
                                        }
                                    }

                                    ProgressInformationMessage.SetMessage(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.Forms_LibraryCleaner_DeletingDirectory)), new { DirectoryFullPath = Junk.FSInfo.FullName }));
                                    await Task.Run(() => (Junk.FSInfo as DirectoryInfo)?.Delete(true));
                                }
                            }

                            Definitions.List.LCItems.Remove(Junk);
                        }

                        await ProgressInformationMessage.CloseAsync();
                    }
                }
                else if ((string)(sender as Button)?.Tag == "DeleteAll")
                {
                    if (await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.Forms_LibraryCleaner_DeleteWarning)), Functions.SLM.Translate(nameof(Properties.Resources.Forms_LibraryCleaner_DeleteWarningMessage)), MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Affirmative)
                    {
                        var ProgressInformationMessage = await Main.FormAccessor.ShowProgressAsync(Functions.SLM.Translate(nameof(Properties.Resources.PleaseWait)), Functions.SLM.Translate(nameof(Properties.Resources.Forms_LibraryCleaner_Delete)), true);
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
                                    await Task.Run(() => Junk.FSInfo.Delete());
                                }
                            }
                            else
                            {
                                Junk.FSInfo.Refresh();
                                if (Junk.FSInfo.Exists)
                                {
                                    ProgressInformationMessage.SetMessage(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.Forms_LibraryCleaner_DeletingDirectory)), new { DirectoryFullPath = Junk.FSInfo.FullName }));
                                    await Task.Run(() => ((DirectoryInfo)Junk.FSInfo).Delete(true));
                                }
                            }

                            Definitions.List.LCItems.Remove(Junk);
                        }

                        await ProgressInformationMessage.CloseAsync();
                    }
                }
            }
            catch (IOException ex)
            {
                logger.Error(ex);

                if (Main.FormAccessor.IsAnyDialogOpen)
                {
                    await Main.FormAccessor.LibraryCleanerView.Dispatcher.Invoke(async delegate
                    {
                        await Main.FormAccessor.HideMetroDialogAsync(await Main.FormAccessor.GetCurrentDialogAsync<BaseMetroDialog>());
                    }, System.Windows.Threading.DispatcherPriority.Normal);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Error(ex);

                if (Main.FormAccessor.IsAnyDialogOpen)
                {
                    await Main.FormAccessor.LibraryCleanerView.Dispatcher.Invoke(async delegate
                    {
                        await Main.FormAccessor.HideMetroDialogAsync(await Main.FormAccessor.GetCurrentDialogAsync<BaseMetroDialog>());
                    }, System.Windows.Threading.DispatcherPriority.Normal);
                }
            }
            catch (Exception ex)
            {
                Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
                logger.Fatal(ex);

                if (Main.FormAccessor.IsAnyDialogOpen)
                {
                    await Main.FormAccessor.LibraryCleanerView.Dispatcher.Invoke(async delegate
                     {
                         await Main.FormAccessor.HideMetroDialogAsync(await Main.FormAccessor.GetCurrentDialogAsync<BaseMetroDialog>());
                     }, System.Windows.Threading.DispatcherPriority.Normal);
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
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
    }
}