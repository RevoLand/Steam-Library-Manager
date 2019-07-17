using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Steam_Library_Manager.Functions
{
    internal static class TaskManager
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static readonly ObservableCollection<Definitions.List.TaskInfo> TaskList = new ObservableCollection<Definitions.List.TaskInfo>();
        public static CancellationTokenSource CancellationToken;
        public static bool Status, Paused, IsRestartRequired;
        public static Definitions.List.TaskInfo ActiveTask;
        public static Definitions.List.TmInfo TMInfo { get; } = new Definitions.List.TmInfo();

        private static readonly IProgress<Definitions.List.TaskInfo> RemoveTaskProgress = new Progress<Definitions.List.TaskInfo>(task => TaskList.Remove(task));

        private static async Task ProcessTaskAsync(Definitions.List.TaskInfo CurrentTask)
        {
            try
            {
                TmInfoUpdate();

                ActiveTask = CurrentTask;
                CurrentTask.Active = true;

                switch (CurrentTask.TaskType)
                {
                    default:
                        await CurrentTask.App.CopyFilesAsync(CurrentTask, CancellationToken.Token).ConfigureAwait(false);
                        break;

                    case Definitions.Enums.TaskType.Delete:
                        await CurrentTask.App.DeleteFilesAsync(CurrentTask).ConfigureAwait(false);
                        CurrentTask.App.Library.Apps.Remove(CurrentTask.App);
                        break;

                    case Definitions.Enums.TaskType.Compact:
                        await CurrentTask.App.CompactTask(CurrentTask, CancellationToken.Token).ConfigureAwait(false);
                        break;
                }

                if (!CancellationToken.IsCancellationRequested && !CurrentTask.ErrorHappened)
                {
                    if (CurrentTask.RemoveOldFiles && CurrentTask.TaskType != Definitions.Enums.TaskType.Delete && CurrentTask.TaskType != Definitions.Enums.TaskType.Compact)
                    {
                        Main.FormAccessor.TmLogs.Report(Framework.StringFormat.Format(
                            SLM.Translate(nameof(Properties.Resources.TaskManager_RemoveOldFiles)),
                            new { CurrentTime = DateTime.Now, CurrentTask.App.AppName }));
                        await CurrentTask.App.DeleteFilesAsync(CurrentTask).ConfigureAwait(false);
                        CurrentTask.App.Library.Apps.Remove(CurrentTask.App);
                        Main.FormAccessor.TmLogs.Report(Framework.StringFormat.Format(
                            SLM.Translate(nameof(Properties.Resources.TaskManager_RemoveOldFilesCompleted)),
                            new { CurrentTime = DateTime.Now, CurrentTask.App.AppName }));
                    }

                    if (CurrentTask.TargetLibrary?.Type == Definitions.Enums.LibraryType.Steam)
                    {
                        IsRestartRequired = true;
                    }

                    CurrentTask.TaskStatusInfo = SLM.Translate(nameof(Properties.Resources.TaskStatus_Completed));
                    CurrentTask.Active = false;
                    CurrentTask.Completed = true;

                    CurrentTask.TargetLibrary?.UpdateAppListAsync();

                    // Update library details
                    if (Definitions.SLM.CurrentSelectedLibrary == CurrentTask.App.Library)
                    {
                        App.UpdateAppPanel(CurrentTask.App.Library);
                    }
                }

                if (TaskList.Count(x => !x.Completed) == 0)
                {
                    if (Properties.Settings.Default.PlayASoundOnCompletion)
                    {
                        if (!string.IsNullOrEmpty(Properties.Settings.Default.CustomSoundFile) && File.Exists(Properties.Settings.Default.CustomSoundFile))
                        {
                            new System.Media.SoundPlayer(Properties.Settings.Default.CustomSoundFile).Play();
                        }
                        else
                        {
                            System.Media.SystemSounds.Exclamation.Play();
                        }
                    }

                    if (IsRestartRequired)
                    {
                        Steam.RestartSteamAsync();
                        IsRestartRequired = false;
                    }
                }

                if (Properties.Settings.Default.TaskManager_AutoClear && !CurrentTask.ErrorHappened)
                {
                    RemoveTaskProgress.Report(CurrentTask);
                }

                SLM.Library.UpdateLibraryVisual();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Logger.Fatal(ex);
            }
            finally
            {
                TmInfoUpdate();
            }
        }

        public static void Start()
        {
            if (!Status && !Paused)
            {
                Main.FormAccessor.TmLogs.Report(Framework.StringFormat.Format(SLM.Translate(nameof(Properties.Resources.TaskManager_Active)), new { CurrentTime = DateTime.Now }));
                CancellationToken = new CancellationTokenSource();
                Status = true;

                Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        while (!CancellationToken.IsCancellationRequested && Status)
                        {
                            if (TaskList.ToList().Any(x => !x.Completed))
                            {
                                await ProcessTaskAsync(TaskList.First(x => !x.Completed)).ConfigureAwait(false);
                            }

                            Thread.Sleep(100);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Stop();
                        Main.FormAccessor.TmLogs.Report(Framework.StringFormat.Format(SLM.Translate(nameof(Properties.Resources.TaskManager_Stopped)), new { CurrentTime = DateTime.Now }));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        Logger.Fatal(ex);
                    }
                });
            }
            else if (Paused)
            {
                Paused = false;
                ActiveTask.mre.Set();

                Main.FormAccessor.TaskManagerView.Button_StartTaskManager.Dispatcher.Invoke(delegate
                {
                    Main.FormAccessor.TaskManagerView.Button_StartTaskManager.IsEnabled = false;
                });
                Main.FormAccessor.TaskManagerView.Button_PauseTaskManager.Dispatcher.Invoke(delegate
                {
                    Main.FormAccessor.TaskManagerView.Button_PauseTaskManager.IsEnabled = true;
                });
                Main.FormAccessor.TaskManagerView.Button_StopTaskManager.Dispatcher.Invoke(delegate
                {
                    Main.FormAccessor.TaskManagerView.Button_StopTaskManager.IsEnabled = true;
                });
            }
        }

        public static void Pause()
        {
            try
            {
                if (Status && ActiveTask != null)
                {
                    Main.FormAccessor.TaskManagerView.Button_StartTaskManager.Dispatcher.Invoke(delegate
                    {
                        Main.FormAccessor.TaskManagerView.Button_StartTaskManager.IsEnabled = true;
                    });
                    Main.FormAccessor.TaskManagerView.Button_PauseTaskManager.Dispatcher.Invoke(delegate
                    {
                        Main.FormAccessor.TaskManagerView.Button_PauseTaskManager.IsEnabled = false;
                    });
                    Main.FormAccessor.TaskManagerView.Button_StopTaskManager.Dispatcher.Invoke(delegate
                    {
                        Main.FormAccessor.TaskManagerView.Button_StopTaskManager.IsEnabled = true;
                    });

                    Paused = true;
                    ActiveTask.mre.Reset();

                    Main.FormAccessor.TmLogs.Report(Framework.StringFormat.Format(SLM.Translate(nameof(Properties.Resources.TaskManager_Paused)), new { CurrentTime = DateTime.Now }));
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }

        public static void Stop()
        {
            try
            {
                if (Status)
                {
                    Main.FormAccessor.TaskManagerView.Button_StartTaskManager.Dispatcher.Invoke(delegate
                    {
                        Main.FormAccessor.TaskManagerView.Button_StartTaskManager.IsEnabled = true;
                    });
                    Main.FormAccessor.TaskManagerView.Button_PauseTaskManager.Dispatcher.Invoke(delegate
                    {
                        Main.FormAccessor.TaskManagerView.Button_PauseTaskManager.IsEnabled = false;
                    });
                    Main.FormAccessor.TaskManagerView.Button_StopTaskManager.Dispatcher.Invoke(delegate
                    {
                        Main.FormAccessor.TaskManagerView.Button_StopTaskManager.IsEnabled = false;
                    });

                    Status = false;
                    Paused = false;
                    CancellationToken.Cancel();
                    IsRestartRequired = false;
                    ActiveTask?.mre?.Set();
                    TmInfoUpdate();
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }

        public static void AddTask(Definitions.List.TaskInfo Task)
        {
            try
            {
                TaskList.Add(Task);

                TmInfoUpdate();
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }

        public static void RemoveTask(Definitions.List.TaskInfo Task)
        {
            try
            {
                TaskList.Remove(Task);

                TmInfoUpdate();
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }

        private static void TmInfoUpdate()
        {
            TMInfo.PendingTasks = TaskList.Count(x => !x.Active && !x.Completed);
            TMInfo.CompletedTasks = TaskList.Count(x => !x.Active && x.Completed);
            TMInfo.TotalTasks = TaskList.Count;
        }
    }
}