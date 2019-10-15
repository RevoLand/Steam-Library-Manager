using MahApps.Metro.Controls.Dialogs;
using System;
using System.Diagnostics;
using Alphaleonis.Win32.Filesystem;
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
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

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

                foreach (var app in Main.FormAccessor.AppView.AppPanel.SelectedItems.Cast<dynamic>().ToList())
                {
                    if (app is Definitions.SteamAppInfo && app.IsSteamBackup)
                    {
                        Process.Start(Path.Combine(Properties.Settings.Default.steamInstallationPath, "Steam.exe"), $"-install \"{app.InstallationDirectory}\"");
                    }
                    else
                    {
                        if (library == app.Library || !library.AllowedAppTypes.Contains(app.Library.Type))
                        {
                            Logger.Warn($"Tried to move an app to the same library OR a library that doesn't support the current app type. App Library: {app.Library.FullPath} - Target Library: {library.FullPath} - App Type: {app.Library.Type} - Target Library Type: {library.Type}");
                            continue;
                        }

                        if (Functions.TaskManager.TaskList.Count(x => x.App == app && x.TargetLibrary == library && !x.Completed) == 0)
                        {
                            Functions.TaskManager.AddTask(new Definitions.List.TaskInfo
                            {
                                App = app,
                                TargetLibrary = library,
                                TaskType = Definitions.Enums.TaskType.Copy
                            });
                        }
                        else
                        {
                            await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.TaskManager_AlreadyTasked)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskManager_AlreadyTaskedMessage)), new { app.AppName, LibraryFullPath = library.DirectoryInfo.FullName })).ConfigureAwait(true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
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
                Logger.Fatal(ex);
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
                Logger.Error(ex);
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
                Logger.Fatal(ex);
                Debug.WriteLine(ex);
            }
        }
    }
}