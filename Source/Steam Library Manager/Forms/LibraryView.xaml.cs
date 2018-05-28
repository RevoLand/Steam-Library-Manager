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
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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

                foreach (dynamic App in Main.FormAccessor.AppView.AppPanel.SelectedItems)
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
                                await Main.FormAccessor.ShowMessageAsync("Steam Library Manager", $"This item is already tasked.\n\nGame: {App.AppName}\nTarget Library: {Library.DirectoryInfo.FullName}");
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
                            await Main.FormAccessor.ShowMessageAsync("Steam Library Manager", $"This item is already tasked.\n\nGame: {App.AppName}\nTarget Library: {Library.DirectoryInfo.FullName}");
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
                    FileInfo Info = new FileInfo(DroppedItem);

                    if ((Info.Attributes & FileAttributes.Directory) != 0)
                    {
                        var LibraryDialog = await Main.FormAccessor.ShowMessageAsync("Steam Library Manager", $"Select Library type you want to create in folder:\n{DroppedItem}", MessageDialogStyle.AffirmativeAndNegativeAndDoubleAuxiliary, new MetroDialogSettings
                        {
                            AffirmativeButtonText = "Steam",
                            NegativeButtonText = "SLM",
                            FirstAuxiliaryButtonText = "Origin",
                            SecondAuxiliaryButtonText = "Cancel"
                        });

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
                                        await Main.FormAccessor.ShowMessageAsync("Steam Library Manager", "Libraries can not be created at root");
                                    }
                                }
                                else
                                {
                                    await Main.FormAccessor.ShowMessageAsync("Steam Library Manager", "Library already exists at " + DroppedItem);
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
                                        await Main.FormAccessor.ShowMessageAsync("Steam Library Manager", "Libraries can not be created at root");
                                    }
                                }
                                else
                                {
                                    await Main.FormAccessor.ShowMessageAsync("Steam Library Manager", "Library already exists at " + DroppedItem);
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
                                        await Main.FormAccessor.ShowMessageAsync("Steam Library Manager", "Libraries can not be created at root");
                                    }
                                }
                                else
                                {
                                    await Main.FormAccessor.ShowMessageAsync("Steam Library Manager", "Library already exists at " + DroppedItem);
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
    }
}