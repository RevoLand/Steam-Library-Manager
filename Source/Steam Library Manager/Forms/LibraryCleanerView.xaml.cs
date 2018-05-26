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
        public LibraryCleanerView()
        {
            InitializeComponent();
        }

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
                            && await Main.FormAccessor.ShowMessageAsync("Root path selected?", "Are you sure you like to move junks to root of disk?", MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(true) != MessageDialogResult.Affirmative)
                        {
                            return;
                        }

                        var ProgressInformationMessage = await Main.FormAccessor.ShowProgressAsync("Please wait...", "Relocating junk files as you have requested.").ConfigureAwait(true);
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
                                await Task.Run(() => Junk.FSInfo.Delete()).ConfigureAwait(true);
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
                                            await Task.Run(() => currentFile.CopyTo(newFile.FullName, true)).ConfigureAwait(true);
                                        }
                                    }

                                    ProgressInformationMessage.SetMessage("Removing old directory:\n\n" + (Junk.FSInfo as DirectoryInfo)?.FullName);
                                    await Task.Run(() => (Junk.FSInfo as DirectoryInfo)?.Delete(true)).ConfigureAwait(true);
                                }
                            }

                            Definitions.List.LCItems.Remove(Junk);
                        }

                        await ProgressInformationMessage.CloseAsync().ConfigureAwait(true);
                    }
                }
                else if ((string)(sender as Button)?.Tag == "DeleteAll")
                {
                    if (await Main.FormAccessor.ShowMessageAsync("There might be saved games in these folders?!", "Saved Games may be located within these folders, are you sure you want to remove them?", MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(true) == MessageDialogResult.Affirmative)
                    {
                        var ProgressInformationMessage = await Main.FormAccessor.ShowProgressAsync("Please wait...", "Removing junk files as you have requested.", true).ConfigureAwait(true);
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
                                    await Task.Run(() => Junk.FSInfo.Delete()).ConfigureAwait(true);
                                }
                            }
                            else
                            {
                                Junk.FSInfo.Refresh();
                                if (Junk.FSInfo.Exists)
                                {
                                    ProgressInformationMessage.SetMessage("Deleting Folder:\n\n" + Junk.FSInfo.FullName);
                                    await Task.Run(() => ((DirectoryInfo)Junk.FSInfo).Delete(true)).ConfigureAwait(true);
                                }
                            }

                            Definitions.List.LCItems.Remove(Junk);
                        }

                        await ProgressInformationMessage.CloseAsync().ConfigureAwait(true);
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
                Functions.Logger.LogToFile(Functions.Logger.LogType.SLM, ex.ToString());
            }
        }
    }
}