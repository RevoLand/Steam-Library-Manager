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
    public partial class TaskManagerView
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

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
                        Button_StartTaskManager.IsEnabled = true;
                        Button_PauseTaskManager.IsEnabled = false;
                        Button_StopTaskManager.IsEnabled = true;
                        break;

                    case "Stop":
                        Functions.TaskManager.Stop();
                        Button_StartTaskManager.IsEnabled = true;
                        Button_PauseTaskManager.IsEnabled = false;
                        Button_StopTaskManager.IsEnabled = false;
                        break;

                    case "BackupUpdates":
                        Functions.Steam.Library.CheckForBackupUpdatesAsync();
                        Functions.Origin.CheckForBackupUpdatesAsync();
                        Functions.Uplay.CheckForBackupUpdatesAsync();
                        Main.FormAccessor.TmLogs.Report(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.Steam_CheckForBackupUpdates_Completed)), new { CurrentTime = DateTime.Now }));
                        break;

                    case "ClearCompleted":
                        foreach (var currentTask in Functions.TaskManager.TaskList.Where(x => x.Completed).ToList())
                        {
                            Functions.TaskManager.TaskList.Remove(currentTask);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
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

                        foreach (var currentTask in TaskPanel.SelectedItems?.OfType<Definitions.List.TaskInfo>().ToList())
                        {
                            if (currentTask.Active && Functions.TaskManager.Status && !currentTask.Completed)
                            {
                                await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.TM_TaskActiveError)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.TM_TaskActiveErrorMessage)), new { currentTask.App?.AppName })).ConfigureAwait(true);
                            }
                            else
                            {
                                Functions.TaskManager.RemoveTask(currentTask);
                            }
                        }
                        break;

                    case "ToggleCompress":
                        foreach (var currentTask in TaskPanel.SelectedItems?.OfType<Definitions.List.TaskInfo>().Where(x => x.TaskType == Definitions.Enums.TaskType.Copy || x.TaskType == Definitions.Enums.TaskType.Compress).ToList())
                        {
                            currentTask.Compress = !currentTask.Compress;
                        }
                        break;

                    case "ToggleRemoveFiles":
                        foreach (var currentTask in TaskPanel.SelectedItems?.OfType<Definitions.List.TaskInfo>().Where(x => x.TaskType == Definitions.Enums.TaskType.Copy || x.TaskType == Definitions.Enums.TaskType.Compress).ToList())
                        {
                            currentTask.RemoveOldFiles = !currentTask.RemoveOldFiles;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Logger.Fatal(ex);
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2 && ((sender as Grid)?.DataContext as Definitions.List.TaskInfo)?.App.InstallationDirectory.Exists == true)
                {
                    Process.Start(((Definitions.List.TaskInfo)((Grid)sender)?.DataContext)?.App.InstallationDirectory.FullName);
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
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