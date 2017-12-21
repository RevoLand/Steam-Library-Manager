using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Steam_Library_Manager.Framework
{
    class TaskManager
    {
        public static AsyncObservableCollection<Definitions.List.TaskInfo> TaskList = new AsyncObservableCollection<Definitions.List.TaskInfo>();
        public static ManualResetEvent manualResetEvent = new ManualResetEvent(false);
        public static CancellationTokenSource CancellationToken;
        public static bool Status = false;
        public static bool Paused = false;
        public static bool IsRestartRequired = false;

        public static void ProcessTask(Definitions.List.TaskInfo CurrentTask)
        {
            try
            {
                CurrentTask.Active = true;

                switch(CurrentTask.TaskType)
                {
                    default:
                    case Definitions.Enums.TaskType.Copy:
                        CurrentTask.App.CopyFilesAsync(CurrentTask, CancellationToken.Token);
                        break;
                    case Definitions.Enums.TaskType.Delete:
                        CurrentTask.App.DeleteFiles(CurrentTask);
                        break;
                }

                if (!CancellationToken.IsCancellationRequested && !CurrentTask.ErrorHappened)
                {
                    if (CurrentTask.RemoveOldFiles && CurrentTask.TaskType != Definitions.Enums.TaskType.Delete)
                    {
                        Main.FormAccessor.TaskManager_Logs.Add($"[{DateTime.Now}] [{CurrentTask.App.AppName}] Removing moved files as requested. This may take a while, please wait.");
                        CurrentTask.App.DeleteFiles(CurrentTask);
                        Main.FormAccessor.TaskManager_Logs.Add($"[{DateTime.Now}] [{CurrentTask.App.AppName}] Files removed, task is completed now.");
                    }

                    if (CurrentTask.TargetLibrary != null)
                    {
                        if (CurrentTask.TargetLibrary.Type == Definitions.Enums.LibraryType.Steam)
                        {
                            IsRestartRequired = true;
                        }
                    }

                    CurrentTask.TaskStatusInfo = "Completed";
                    CurrentTask.Active = false;
                    CurrentTask.Completed = true;

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
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show(ex.ToString());
                Functions.Logger.LogToFile(Functions.Logger.LogType.TaskManager, $"[{CurrentTask.App.AppName}][{CurrentTask.App.AppID}][{CurrentTask.App.AcfName}] {ex}");
            }
        }

        public static void Start()
        {
            if (!Status && !Paused)
            {
                Main.FormAccessor.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;

                Main.FormAccessor.TaskManager_Logs.Add($"[{DateTime.Now}] [TaskManager] Task Manager is now active and waiting for tasks...");
                CancellationToken = new CancellationTokenSource();
                Status = true;

                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        while (true && !CancellationToken.IsCancellationRequested && Status)
                        {
                            manualResetEvent.Set();
                            if (TaskList.ToList().Count(x => !x.Completed) > 0)
                            {
                                ProcessTask(TaskList.First(x => !x.Completed));
                            }
                            manualResetEvent.WaitOne();
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
                        MessageBox.Show(ex.ToString());

                        Functions.Logger.LogToFile(Functions.Logger.LogType.TaskManager, ex.ToString());
                    }
                });
            }
            else if (Paused)
            {
                Paused = false;

                Main.FormAccessor.Button_StartTaskManager.IsEnabled = false;
                Main.FormAccessor.Button_PauseTaskManager.IsEnabled = true;
                Main.FormAccessor.Button_StopTaskManager.IsEnabled = true;
            }
        }

        public static void Pause()
        {
            try
            {
                if (Status)
                {
                    Main.FormAccessor.Button_StartTaskManager.IsEnabled = true;
                    Main.FormAccessor.Button_PauseTaskManager.IsEnabled = false;
                    Main.FormAccessor.Button_StopTaskManager.IsEnabled = true;

                    Paused = true;

                    Main.FormAccessor.TaskManager_Logs.Add($"[{DateTime.Now}] [TaskManager] Task Manager is paused as requested.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show(ex.ToString());

                Functions.Logger.LogToFile(Functions.Logger.LogType.TaskManager, ex.ToString());
            }
        }

        public static void Stop()
        {
            try
            {
                if (Status)
                {
                    Main.FormAccessor.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                    Main.FormAccessor.TaskbarItemInfo.ProgressValue = 0;
                    Main.FormAccessor.Button_StartTaskManager.IsEnabled = true;
                    Main.FormAccessor.Button_PauseTaskManager.IsEnabled = false;
                    Main.FormAccessor.Button_StopTaskManager.IsEnabled = false;

                    Status = false;
                    Paused = false;
                    CancellationToken.Cancel();
                    IsRestartRequired = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show(ex.ToString());

                Functions.Logger.LogToFile(Functions.Logger.LogType.TaskManager, ex.ToString());
            }
        }

        public static void AddTask(Definitions.List.TaskInfo Task)
        {
            try
            {
                TaskList.Add(Task);

                if (Status)
                {
                    manualResetEvent.Set();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show(ex.ToString());
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
                MessageBox.Show(ex.ToString());
                Functions.Logger.LogToFile(Functions.Logger.LogType.TaskManager, ex.ToString());
            }
        }

    }
}