﻿using MahApps.Metro.Controls.Dialogs;
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
    public partial class LibraryView
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public LibraryView() => InitializeComponent();

        private async void LibraryGrid_Drop(object sender, DragEventArgs e)
        {
            try
            {
                var Library = ((Grid)sender).DataContext as Definitions.Library;

                if (Main.FormAccessor.AppView.AppPanel.SelectedItems.Count == 0 || Library == null)
                {
                    return;
                }

                Library.DirectoryInfo.Refresh();
                if (!Library.DirectoryInfo.Exists)
                {
                    return;
                }

                foreach (var App in Main.FormAccessor.AppView.AppPanel.SelectedItems.Cast<dynamic>().ToList())
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

                            if (Functions.TaskManager.TaskList.Count(x => x.App == App && x.TargetLibrary == Library && !x.Completed) == 0)
                            {
                                Functions.TaskManager.AddTask(new Definitions.List.TaskInfo
                                {
                                    App = App,
                                    TargetLibrary = Library,
                                    TaskType = Definitions.Enums.TaskType.Copy
                                });
                            }
                            else
                            {
                                await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.TaskManager_AlreadyTasked)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskManager_AlreadyTaskedMessage)), new { App.AppName, LibraryFullPath = Library.DirectoryInfo.FullName })).ConfigureAwait(false);
                            }
                        }
                    }
                    else if (App is Definitions.OriginAppInfo)
                    {
                        if (Library == App.Library || Library.Type != Definitions.Enums.LibraryType.Origin)
                            continue;

                        if (Functions.TaskManager.TaskList.Count(x => x.App == App && x.TargetLibrary == Library && !x.Completed) == 0)
                        {
                            Functions.TaskManager.AddTask(new Definitions.List.TaskInfo
                            {
                                App = App,
                                TargetLibrary = Library,
                                TaskType = Definitions.Enums.TaskType.Copy
                            });
                        }
                        else
                        {
                            await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.TaskManager_AlreadyTasked)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskManager_AlreadyTaskedMessage)), new { App.AppName, LibraryFullPath = Library.DirectoryInfo.FullName })).ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
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
                var DroppedItems = (string[])e.Data.GetData(DataFormats.FileDrop, false);

                if (DroppedItems == null)
                {
                    return;
                }

                foreach (var DroppedItem in DroppedItems)
                {
                    var Info = new DirectoryInfo(DroppedItem);

                    if ((Info.Attributes & FileAttributes.Directory) != 0)
                    {
                        await CreateLibraryAsync(Info.FullName).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
            }
        }

        private async System.Threading.Tasks.Task CreateLibraryAsync(string LibraryPath)
        {
            var LibraryDialog = await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.CreateLibraryDialog)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.CreateLibraryDialogMessage)), new { LibraryPath }), MessageDialogStyle.AffirmativeAndNegativeAndDoubleAuxiliary, new MetroDialogSettings
            {
                AffirmativeButtonText = Functions.SLM.Translate(nameof(Properties.Resources.Forms_Steam)),
                NegativeButtonText = Functions.SLM.Translate(nameof(Properties.Resources.Forms_SLM)),
                FirstAuxiliaryButtonText = Functions.SLM.Translate(nameof(Properties.Resources.Forms_Origin)),
                SecondAuxiliaryButtonText = Functions.SLM.Translate(nameof(Properties.Resources.Forms_Cancel))
            }).ConfigureAwait(false);

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
                            await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.Steam_Library_Manager)), Functions.SLM.Translate(nameof(Properties.Resources.CreateLibrary_RootErrorMessage))).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.CreateLibrary_Exists)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.CreateLibrary_ExistsMessage)), new { LibraryPath })).ConfigureAwait(false);
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
                            await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.Steam_Library_Manager)), Functions.SLM.Translate(nameof(Properties.Resources.CreateLibrary_RootErrorMessage))).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.CreateLibrary_Exists)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.CreateLibrary_ExistsMessage)), new { LibraryPath })).ConfigureAwait(false);
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
                            await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.Steam_Library_Manager)), Functions.SLM.Translate(nameof(Properties.Resources.CreateLibrary_RootErrorMessage))).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.CreateLibrary_Exists)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.CreateLibrary_ExistsMessage)), new { LibraryPath })).ConfigureAwait(false);
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

                if (Definitions.SLM.CurrentSelectedLibrary.Type != Definitions.Enums.LibraryType.Steam && Directory.Exists(Definitions.SLM.CurrentSelectedLibrary.DirectoryInfo.FullName) && !Definitions.SLM.CurrentSelectedLibrary.DirectoryInfo.Exists)
                {
                    Functions.SLM.Library.UpdateLibrary(Definitions.SLM.CurrentSelectedLibrary);
                }

                // Update games list from current selection
                Functions.App.UpdateAppPanel(Definitions.SLM.CurrentSelectedLibrary);
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
                {
                    if (((sender as Grid)?.DataContext as Definitions.Library) != null && Directory.Exists(((sender as Grid)?.DataContext as Definitions.Library)?.FullPath))
                    {
                        Process.Start(((sender as Grid)?.DataContext as Definitions.Library)?.FullPath);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private async void LibraryActionButtons_ClickAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (((Button)sender).Tag)
                {
                    case "create":
                        using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                        {
                            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                await CreateLibraryAsync(dialog.SelectedPath).ConfigureAwait(false);
                            }
                        }
                        break;

                    case "remove":
                        if (Definitions.SLM.CurrentSelectedLibrary != null && !Definitions.SLM.CurrentSelectedLibrary.IsMain && Functions.TaskManager.TaskList.Count(x => x.TargetLibrary == Definitions.SLM.CurrentSelectedLibrary || x.App?.Library == Definitions.SLM.CurrentSelectedLibrary) == 0)
                        {
                            Definitions.List.Libraries.Remove(Definitions.SLM.CurrentSelectedLibrary);

                            Main.FormAccessor.AppView.AppPanel.ItemsSource = null;
                        }
                        break;

                    case "refresh":
                        if (Definitions.SLM.CurrentSelectedLibrary != null && Functions.TaskManager.TaskList.Count(x =>
                                      x.TargetLibrary == Definitions.SLM.CurrentSelectedLibrary
                                      || x.App?.Library == Definitions.SLM.CurrentSelectedLibrary) == 0)
                        {
                            Functions.SLM.Library.UpdateLibrary(Definitions.SLM.CurrentSelectedLibrary);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
                Debug.WriteLine(ex);
            }
        }
    }
}