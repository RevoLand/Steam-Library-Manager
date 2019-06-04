using MahApps.Metro.Controls.Dialogs;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Steam_Library_Manager.Forms
{
    /// <summary>
    /// Interaction logic for TaskManagerView.xaml
    /// </summary>
    public partial class TaskManagerView : UserControl
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public TaskManagerView() => InitializeComponent();

        private void TaskManager_Buttons_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch ((sender as Button)?.Tag)
                {
                    case "Start":
                    default:
                        Functions.TaskManager.Start();
                        Button_StartTaskManager.IsEnabled = false;
                        Button_PauseTaskManager.IsEnabled = true;
                        Button_StopTaskManager.IsEnabled = true;
                        break;

                    case "Pause":
                        Functions.TaskManager.Pause();
                        Button_PauseTaskManager.IsEnabled = false;
                        Button_StopTaskManager.IsEnabled = true;
                        break;

                    case "Stop":
                        Functions.TaskManager.Stop();
                        Button_PauseTaskManager.IsEnabled = false;
                        Button_StopTaskManager.IsEnabled = false;
                        break;

                    case "BackupUpdates":
                        Functions.Steam.Library.CheckForBackupUpdatesAsync();
                        break;

                    case "ClearCompleted":
                        if (Functions.TaskManager.TaskList.Count == 0)
                        {
                            return;
                        }

                        foreach (Definitions.List.TaskInfo CurrentTask in Functions.TaskManager.TaskList.ToList())
                        {
                            if (CurrentTask.Completed)
                            {
                                Functions.TaskManager.TaskList.Remove(CurrentTask);
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
            }
        }

        private async void TaskManager_ContextMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TaskPanel.SelectedItems.Count == 0)
                {
                    return;
                }
                switch ((sender as MenuItem)?.Tag)
                {
                    default:

                        foreach (var CurrentTask in TaskPanel.SelectedItems?.OfType<Definitions.List.TaskInfo>().ToList())
                        {
                            if (CurrentTask.Active && Functions.TaskManager.Status && !CurrentTask.Completed)
                            {
                                await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.TM_TaskActiveError)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TM_TaskActiveErrorMessage)), new { AppName = CurrentTask.SteamApp?.AppName ?? CurrentTask.OriginApp?.AppName })).ConfigureAwait(true);
                            }
                            else
                            {
                                Functions.TaskManager.RemoveTask(CurrentTask);
                            }
                        }
                        break;

                    case "ToggleCompress":
                        foreach (var CurrentTask in TaskPanel.SelectedItems?.OfType<Definitions.List.TaskInfo>().ToList())
                        {
                            CurrentTask.Compress = !CurrentTask.Compress;
                        }
                        break;

                    case "ToggleRemoveFiles":
                        foreach (var CurrentTask in TaskPanel.SelectedItems?.OfType<Definitions.List.TaskInfo>().ToList())
                        {
                            CurrentTask.RemoveOldFiles = !CurrentTask.RemoveOldFiles;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                logger.Fatal(ex);
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2 && ((sender as Grid)?.DataContext as Definitions.List.TaskInfo)?.SteamApp.InstallationDirectory.Exists == true)
                {
                    Process.Start(((sender as Grid)?.DataContext as Definitions.List.TaskInfo)?.SteamApp.InstallationDirectory.FullName);
                }
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
            }
        }

        private void TaskManager_LogsView_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            try
            {
                var scrollViewer = ((ScrollViewer)e.OriginalSource);

                // Content scroll event : autoscroll eventually
                if (Properties.Settings.Default.TaskManager_Logs_AutoScroll && e.ExtentHeightChange != 0)
                {
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.ExtentHeight);
                }
            }
            catch { }
        }
    }
}