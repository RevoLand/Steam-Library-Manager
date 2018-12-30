using MahApps.Metro.Controls.Dialogs;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Steam_Library_Manager.Forms
{
    /// <summary>
    /// Interaction logic for LibraryView.xaml
    /// </summary>
    public partial class LibraryView : UserControl
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public LibraryView()
        {
            InitializeComponent();
        }

        private async void LibraryGrid_Drop(object sender, DragEventArgs e)
        {
            try
            {
                Definitions.Library Library = ((Grid)sender).DataContext as Definitions.Library;

                if (Main.FormAccessor.AppView.AppPanel.SelectedItems.Count == 0 || Library == null)
                {
                    return;
                }

                Library.DirectoryInfo.Refresh();
                if (!Library.DirectoryInfo.Exists)
                {
                    return;
                }

                foreach (dynamic App in Main.FormAccessor.AppView.AppPanel.SelectedItems.Cast<dynamic>().ToList())
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
                                await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(Properties.Resources.TaskManager_AlreadyTasked), string.Format(Functions.SLM.Translate(Properties.Resources.TaskManager_AlreadyTaskedMessage), (Definitions.SteamAppInfo)App.AppName, Library.DirectoryInfo.FullName));
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
                            await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(Properties.Resources.TaskManager_AlreadyTasked), string.Format(Functions.SLM.Translate(Properties.Resources.TaskManager_AlreadyTaskedMessage), (Definitions.SteamAppInfo)App.AppName, Library.DirectoryInfo.FullName));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
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
                    var Info = new DirectoryInfo(DroppedItem);

                    if ((Info.Attributes & FileAttributes.Directory) != 0)
                    {
                        await CreateLibraryAsync(Info.FullName);
                    }
                }
            }
            catch (Exception ex)
            {
                Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
            }
        }

        private async System.Threading.Tasks.Task CreateLibraryAsync(string LibraryPath)
        {
            var LibraryDialog = await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(Properties.Resources.CreateLibraryDialog), string.Format(Functions.SLM.Translate(Properties.Resources.CreateLibraryDialogMessage), LibraryPath), MessageDialogStyle.AffirmativeAndNegativeAndDoubleAuxiliary, new MetroDialogSettings
            {
                AffirmativeButtonText = Functions.SLM.Translate(Properties.Resources.Forms_Steam),
                NegativeButtonText = Functions.SLM.Translate(Properties.Resources.Forms_SLM),
                FirstAuxiliaryButtonText = Functions.SLM.Translate(Properties.Resources.Forms_Origin),
                SecondAuxiliaryButtonText = Functions.SLM.Translate(Properties.Resources.Forms_Cancel)
            });

            switch (LibraryDialog)
            {
                // Steam
                case MessageDialogResult.Affirmative:
                    if (!Functions.Steam.Library.IsLibraryExists(LibraryPath))
                    {
                        if (Directory.GetDirectoryRoot(LibraryPath) != LibraryPath)
                        {
                            Functions.Steam.Library.CreateNew(LibraryPath, false);
                        }
                        else
                        {
                            await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(Properties.Resources.CreateLibrary_RootError), Functions.SLM.Translate(Properties.Resources.CreateLibrary_RootErrorMessage));
                        }
                    }
                    else
                    {
                        await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(Properties.Resources.CreateLibrary_Exists), string.Format(Functions.SLM.Translate(Properties.Resources.CreateLibrary_ExistsMessage), LibraryPath));
                    }
                    break;
                // SLM
                case MessageDialogResult.Negative:
                    if (!Functions.SLM.Library.IsLibraryExists(LibraryPath))
                    {
                        if (Directory.GetDirectoryRoot(LibraryPath) != LibraryPath)
                        {
                            Functions.SLM.Library.AddNewAsync(LibraryPath);
                        }
                        else
                        {
                            await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(Properties.Resources.CreateLibrary_RootError), Functions.SLM.Translate(Properties.Resources.CreateLibrary_RootErrorMessage));
                        }
                    }
                    else
                    {
                        await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(Properties.Resources.CreateLibrary_Exists), string.Format(Functions.SLM.Translate(Properties.Resources.CreateLibrary_ExistsMessage), LibraryPath));
                    }
                    break;
                // Origin
                case MessageDialogResult.FirstAuxiliary:
                    if (!Functions.Origin.IsLibraryExists(LibraryPath))
                    {
                        if (Directory.GetDirectoryRoot(LibraryPath) != LibraryPath)
                        {
                            Functions.Origin.AddNewAsync(LibraryPath);
                        }
                        else
                        {
                            await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(Properties.Resources.CreateLibrary_RootError), Functions.SLM.Translate(Properties.Resources.CreateLibrary_RootErrorMessage));
                        }
                    }
                    else
                    {
                        await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(Properties.Resources.CreateLibrary_Exists), string.Format(Functions.SLM.Translate(Properties.Resources.CreateLibrary_ExistsMessage), LibraryPath));
                    }
                    break;
            }
        }

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

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
                {
                    if (((sender as Grid)?.DataContext as Definitions.Library)?.Steam != null && ((sender as Grid)?.DataContext as Definitions.Library)?.Steam.SteamAppsFolder.Exists == true)
                    {
                        Process.Start(((sender as Grid)?.DataContext as Definitions.Library)?.Steam.SteamAppsFolder.FullName);
                    }

                    if (((sender as Grid)?.DataContext as Definitions.Library)?.Origin != null && Directory.Exists(((sender as Grid)?.DataContext as Definitions.Library)?.Origin.FullPath))
                    {
                        Process.Start(((sender as Grid)?.DataContext as Definitions.Library)?.Origin.FullPath);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private async void CreateLibraryButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    await CreateLibraryAsync(dialog.SelectedPath);
                }
            }
        }
    }
}