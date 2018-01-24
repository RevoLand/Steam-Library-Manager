using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Steam_Library_Manager.Framework
{
    internal class TaskManager
    {
        public static AsyncObservableCollection<Definitions.List.TaskInfo> TaskList = new AsyncObservableCollection<Definitions.List.TaskInfo>();
        public static CancellationTokenSource CancellationToken;
        public static bool Status, Paused, IsRestartRequired;

        public static async Task ProcessTaskAsync(Definitions.List.TaskInfo CurrentTask)
        {
            try
            {
                CurrentTask.Active = true;

                if (CurrentTask.SteamApp != null)
                {
                    switch (CurrentTask.TaskType)
                    {
                        default:
                            CurrentTask.SteamApp.CopyFilesAsync(CurrentTask, CancellationToken.Token);
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
                            Main.FormAccessor.TaskManager_Logs.Add($"[{DateTime.Now}] [{CurrentTask.SteamApp.AppName}] Removing moved files as requested. This may take a while, please wait.");
                            await CurrentTask.SteamApp.DeleteFilesAsync(CurrentTask);
                            CurrentTask.SteamApp.Library.Steam.Apps.Remove(CurrentTask.SteamApp);
                            Main.FormAccessor.TaskManager_Logs.Add($"[{DateTime.Now}] [{CurrentTask.SteamApp.AppName}] Files removed, task is completed now.");
                        }

                        if (CurrentTask.TargetLibrary?.Type == Definitions.Enums.LibraryType.Steam)
                        {
                            IsRestartRequired = true;
                        }

                        CurrentTask.TaskStatusInfo = "Completed";
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
                            Main.FormAccessor.TaskManager_Logs.Add($"[{DateTime.Now}] [{CurrentTask.OriginApp.AppName}] Removing moved files as requested. This may take a while, please wait.");

                            CurrentTask.OriginApp.DeleteFiles(CurrentTask);
                            CurrentTask.OriginApp.Library.Origin.Apps.Remove(CurrentTask.OriginApp);

                            if (CurrentTask.Compress)
                            {
                                JunctionPoint.Create(CurrentTask.OriginApp.InstallationDirectory.FullName.Replace(CurrentTask.OriginApp.Library.Origin.FullPath, CurrentTask.TargetLibrary.Origin.FullPath), CurrentTask.OriginApp.InstallationDirectory.FullName, true);
                            }

                            Main.FormAccessor.TaskManager_Logs.Add($"[{DateTime.Now}] [{CurrentTask.OriginApp.AppName}] Files removed, task is completed now.");
                        }

                        CurrentTask.TaskStatusInfo = "Completed";
                        CurrentTask.Active = false;
                        CurrentTask.Completed = true;

                        CurrentTask.TargetLibrary?.Origin.UpdateAppList();

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
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                
                if (CurrentTask.SteamApp != null)
                    Functions.Logger.LogToFile(Functions.Logger.LogType.TaskManager, $"[{CurrentTask.SteamApp.AppName}][{CurrentTask.SteamApp.AppID}][{CurrentTask.SteamApp.AcfName}] {ex}");
                else if(CurrentTask.OriginApp != null)
                    Functions.Logger.LogToFile(Functions.Logger.LogType.TaskManager, $"[{CurrentTask.OriginApp.AppName}][{CurrentTask.OriginApp.AppID}] {ex}");
            }
        }

        public static void Start()
        {
            if (!Status && !Paused)
            {
                Main.FormAccessor.TaskManager_Logs.Add($"[{DateTime.Now}] [TaskManager] Task Manager is now active and waiting for tasks...");
                CancellationToken = new CancellationTokenSource();
                Status = true;

                Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        while (!CancellationToken.IsCancellationRequested && Status)
                        {
                            if (TaskList.ToList().Count(x => !x.Completed) > 0)
                            {
                                await ProcessTaskAsync(TaskList.First(x => !x.Completed));
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Stop();
                        Main.FormAccessor.TaskManager_Logs.Add($"[{DateTime.Now}] [TaskManager] Task Manager is stopped.");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        Functions.Logger.LogToFile(Functions.Logger.LogType.TaskManager, ex.ToString());
                    }
                });
            }
            else if (Paused)
            {
                Paused = false;

                Main.FormAccessor.Button_StartTaskManager.Dispatcher.Invoke(delegate
                {
                    Main.FormAccessor.Button_StartTaskManager.IsEnabled = false;
                });
                Main.FormAccessor.Button_PauseTaskManager.Dispatcher.Invoke(delegate
                {
                    Main.FormAccessor.Button_PauseTaskManager.IsEnabled = true;
                });
                Main.FormAccessor.Button_StopTaskManager.Dispatcher.Invoke(delegate
                {
                    Main.FormAccessor.Button_StopTaskManager.IsEnabled = true;
                });
            }
        }

        public static void Pause()
        {
            try
            {
                if (Status)
                {
                    Main.FormAccessor.Button_StartTaskManager.Dispatcher.Invoke(delegate
                    {
                        Main.FormAccessor.Button_StartTaskManager.IsEnabled = true;
                    });
                    Main.FormAccessor.Button_PauseTaskManager.Dispatcher.Invoke(delegate
                    {
                        Main.FormAccessor.Button_PauseTaskManager.IsEnabled = false;
                    });
                    Main.FormAccessor.Button_StopTaskManager.Dispatcher.Invoke(delegate
                    {
                        Main.FormAccessor.Button_StopTaskManager.IsEnabled = true;
                    });

                    Paused = true;

                    Main.FormAccessor.TaskManager_Logs.Add($"[{DateTime.Now}] [TaskManager] Task Manager is paused as requested.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Functions.Logger.LogToFile(Functions.Logger.LogType.TaskManager, ex.ToString());
            }
        }

        public static void Stop()
        {
            try
            {
                if (Status)
                {
                    Main.FormAccessor.Button_StartTaskManager.Dispatcher.Invoke(delegate
                    {
                        Main.FormAccessor.Button_StartTaskManager.IsEnabled = true;
                    });
                    Main.FormAccessor.Button_PauseTaskManager.Dispatcher.Invoke(delegate
                    {
                        Main.FormAccessor.Button_PauseTaskManager.IsEnabled = false;
                    });
                    Main.FormAccessor.Button_StopTaskManager.Dispatcher.Invoke(delegate
                    {
                        Main.FormAccessor.Button_StopTaskManager.IsEnabled = false;
                    });

                    Status = false;
                    Paused = false;
                    CancellationToken.Cancel();
                    IsRestartRequired = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Functions.Logger.LogToFile(Functions.Logger.LogType.TaskManager, ex.ToString());
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
                Debug.WriteLine(ex);
                Functions.Logger.LogToFile(Functions.Logger.LogType.TaskManager, ex.ToString());
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
                Debug.WriteLine(ex);
                Functions.Logger.LogToFile(Functions.Logger.LogType.TaskManager, ex.ToString());
            }
        }

    }
}