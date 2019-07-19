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
    public partial class LibraryView
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public LibraryView() => InitializeComponent();

        private async void LibraryGrid_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (Main.FormAccessor.AppView.AppPanel.SelectedItems.Count == 0 || !(((Grid)sender).DataContext is Definitions.Library library))
                {
                    return;
                }

                library.DirectoryInfo.Refresh();
                if (!library.DirectoryInfo.Exists)
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
                            if (library == App.Library || library.Type == Definitions.Enums.LibraryType.Origin)
                            {
                                continue;
                            }

                            if (Functions.TaskManager.TaskList.Count(x => x.App == App && x.TargetLibrary == library && !x.Completed) == 0)
                            {
                                Functions.TaskManager.AddTask(new Definitions.List.TaskInfo
                                {
                                    App = App,
                                    TargetLibrary = library,
                                    TaskType = Definitions.Enums.TaskType.Copy
                                });
                            }
                            else
                            {
                                await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.TaskManager_AlreadyTasked)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskManager_AlreadyTaskedMessage)), new { App.AppName, LibraryFullPath = library.DirectoryInfo.FullName })).ConfigureAwait(false);
                            }
                        }
                    }
                    else if (App is Definitions.OriginAppInfo)
                    {
                        if (library == App.Library || library.Type != Definitions.Enums.LibraryType.Origin)
                            continue;

                        if (Functions.TaskManager.TaskList.Count(x => x.App == App && x.TargetLibrary == library && !x.Completed) == 0)
                        {
                            Functions.TaskManager.AddTask(new Definitions.List.TaskInfo
                            {
                                App = App,
                                TargetLibrary = library,
                                TaskType = Definitions.Enums.TaskType.Copy
                            });
                        }
                        else
                        {
                            await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.TaskManager_AlreadyTasked)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskManager_AlreadyTaskedMessage)), new { App.AppName, LibraryFullPath = library.DirectoryInfo.FullName })).ConfigureAwait(false);
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

                // Update app panel with selected library
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

        private void LibraryActionButtons_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (((Button)sender).Tag)
                {
                    case "create":
                        Main.FormAccessor.createLibraryFlyout.IsOpen = true;
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