using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Steam_Library_Manager.Framework
{
    internal static class TaskManager
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static ObservableCollection<Definitions.List.TaskInfo> TaskList = new ObservableCollection<Definitions.List.TaskInfo>();
        public static CancellationTokenSource CancellationToken;
        public static bool Status, Paused, IsRestartRequired;
        public static Definitions.List.TaskInfo ActiveTask;

        public static async Task ProcessTaskAsync(Definitions.List.TaskInfo CurrentTask)
        {
            try
            {
                ActiveTask = CurrentTask;
                CurrentTask.Active = true;

                if (CurrentTask.SteamApp != null)
                {
                    switch (CurrentTask.TaskType)
                    {
                        default:
                            await CurrentTask.SteamApp.CopyFilesAsync(CurrentTask, CancellationToken.Token);
                            break;

                        case Definitions.Enums.TaskType.Delete:
                            await CurrentTask.SteamApp.DeleteFilesAsync(CurrentTask);
                            CurrentTask.SteamApp.Library.Steam.Apps.Remove(CurrentTask.SteamApp);
                            break;
                    }

                    if (!CancellationToken.IsCancellationRequested && !CurrentTask.ErrorHappened)
                    {
                        if (CurrentTask.RemoveOldFiles && CurrentTask.TaskType != Definitions.Enums.TaskType.Delete)
                        {
                            Main.FormAccessor.TaskManager_Logs.Add(string.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskManager_RemoveOldFiles)), DateTime.Now, CurrentTask.SteamApp.AppName));
                            await CurrentTask.SteamApp.DeleteFilesAsync(CurrentTask);
                            CurrentTask.SteamApp.Library.Steam.Apps.Remove(CurrentTask.SteamApp);
                            Main.FormAccessor.TaskManager_Logs.Add(string.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskManager_RemoveOldFilesCompleted)), DateTime.Now, CurrentTask.SteamApp.AppName));
                        }

                        if (CurrentTask.TargetLibrary?.Type == Definitions.Enums.LibraryType.Steam)
                        {
                            IsRestartRequired = true;
                        }

                        CurrentTask.TaskStatusInfo = Functions.SLM.Translate(nameof(Properties.Resources.TaskStatus_Completed));
                        CurrentTask.Active = false;
                        CurrentTask.Completed = true;

                        CurrentTask.TargetLibrary?.Steam.UpdateAppList();

                        // Update library details
                        if (Definitions.SLM.CurrentSelectedLibrary == CurrentTask.SteamApp.Library)
                        {
                            Functions.App.UpdateAppPanel(CurrentTask.SteamApp.Library);
                        }
                    }
                }
                else if (CurrentTask.OriginApp != null)
                {
                    switch (CurrentTask.TaskType)
                    {
                        default:
                            CurrentTask.OriginApp.CopyFilesAsync(CurrentTask, CancellationToken.Token);
                            break;

                        case Definitions.Enums.TaskType.Delete:
                            CurrentTask.OriginApp.DeleteFiles(CurrentTask);

                            CurrentTask.OriginApp.Library.Origin.Apps.Remove(CurrentTask.OriginApp);
                            break;
                    }

                    if (!CancellationToken.IsCancellationRequested && !CurrentTask.ErrorHappened)
                    {
                        if (CurrentTask.RemoveOldFiles && CurrentTask.TaskType != Definitions.Enums.TaskType.Delete)
                        {
                            Main.FormAccessor.TaskManager_Logs.Add(string.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskManager_RemoveOldFiles)), DateTime.Now, CurrentTask.OriginApp.AppName));

                            CurrentTask.OriginApp.DeleteFiles(CurrentTask);
                            CurrentTask.OriginApp.Library.Origin.Apps.Remove(CurrentTask.OriginApp);

                            Main.FormAccessor.TaskManager_Logs.Add(string.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskManager_RemoveOldFilesCompleted)), DateTime.Now, CurrentTask.OriginApp.AppName));
                        }

                        CurrentTask.TaskStatusInfo = Functions.SLM.Translate(nameof(Properties.Resources.TaskStatus_Completed));
                        CurrentTask.Active = false;
                        CurrentTask.Completed = true;

                        CurrentTask.TargetLibrary?.Origin.UpdateAppList();

                        if (CurrentTask.Compress)
                        {
                            await CurrentTask.TargetLibrary?.Origin.Apps.FirstOrDefault(x => x.AppID == CurrentTask.OriginApp.AppID)?.InstallAsync();
                        }

                        // Update library details
                        if (Definitions.SLM.CurrentSelectedLibrary == CurrentTask.OriginApp.Library)
                        {
                            Functions.App.UpdateAppPanel(CurrentTask.OriginApp.Library);
                        }
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
                        Functions.Steam.RestartSteamAsync();
                        IsRestartRequired = false;
                    }
                }

                Functions.SLM.Library.UpdateLibraryVisual();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                logger.Fatal(ex);
            }
        }

        public static void Start()
        {
            if (!Status && !Paused)
            {
                Main.FormAccessor.TaskManager_Logs.Add(string.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskManager_Active)), DateTime.Now));
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
                                await ProcessTaskAsync(TaskList.First(x => !x.Completed));
                            }

                            Thread.Sleep(1);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Stop();
                        Main.FormAccessor.TaskManager_Logs.Add(string.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskManager_Stopped)), DateTime.Now));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        logger.Fatal(ex);
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

                    Main.FormAccessor.TaskManager_Logs.Add(string.Format(Functions.SLM.Translate(nameof(Properties.Resources.TaskManager_Paused)), DateTime.Now));
                }
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
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
                }
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
            }
        }

        public static void AddTask(Definitions.List.TaskInfo Task)
        {
            try
            {
                TaskList.Add(Task);
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
            }
        }

        public static void RemoveTask(Definitions.List.TaskInfo Task)
        {
            try
            {
                TaskList.Remove(Task);
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
            }
        }
    }
}