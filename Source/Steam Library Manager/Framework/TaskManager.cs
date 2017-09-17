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
        public static AsyncObservableCollection<Definitions.List.TaskList> TaskList = new AsyncObservableCollection<Definitions.List.TaskList>();
        public static ManualResetEvent manualResetEvent = new ManualResetEvent(false);
        public static CancellationTokenSource CancellationToken;
        public static bool Status = false;
        public static bool IsRestartRequired = false;

        public static void ProcessTask(Definitions.List.TaskList CurrentTask)
        {
            try
            {
                if (!CurrentTask.TargetApp.Library.Apps.Contains(CurrentTask.TargetApp))
                    return;

                CurrentTask.Moving = true;
                CurrentTask.TargetApp.CopyGameFiles(CurrentTask, CancellationToken.Token);

                if (!CancellationToken.IsCancellationRequested)
                {
                    if (CurrentTask.RemoveOldFiles)
                    {
                        Main.FormAccessor.TaskManager_Logs.Add($"[{DateTime.Now}] [{CurrentTask.TargetApp.AppName}] Removing moven files as requested. This may take a while, please wait.");
                        CurrentTask.TargetApp.DeleteFiles();
                        Main.FormAccessor.TaskManager_Logs.Add($"[{DateTime.Now}] [{CurrentTask.TargetApp.AppName}] Files removen, task is completed now.");
                    }

                    if (!CurrentTask.TargetLibrary.IsBackup)
                        IsRestartRequired = true;

                    CurrentTask.Moving = false;
                    CurrentTask.Completed = true;

                    if (TaskList.Count == 0)
                    {
                        if (Properties.Settings.Default.PlayASoundOnCompletion)
                        {
                            if (!string.IsNullOrEmpty(Properties.Settings.Default.CustomSoundFile) && File.Exists(Properties.Settings.Default.CustomSoundFile))
                                new System.Media.SoundPlayer(Properties.Settings.Default.CustomSoundFile).Play();
                            else
                                System.Media.SystemSounds.Exclamation.Play();
                        }

                        if (IsRestartRequired)
                            Functions.Steam.RestartSteamAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show(ex.ToString());
                Functions.Logger.LogToFile(Functions.Logger.LogType.TaskManager, $"[{CurrentTask.TargetApp.AppName}][{CurrentTask.TargetApp.AppID}][{CurrentTask.TargetApp.AcfName}] {ex}");
            }
        }

        public static void Start()
        {
            if (!Status)
            {
                Main.FormAccessor.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;

                Main.FormAccessor.TaskManager_Logs.Add($"[{DateTime.Now}] [TaskManager] Task Manager is now active and waiting for tasks...");
                Main.FormAccessor.Button_StartTaskManager.IsEnabled = false;
                Main.FormAccessor.Button_StopTaskManager.IsEnabled = true;
                CancellationToken = new CancellationTokenSource();
                Status = true;

                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        while (true && !CancellationToken.IsCancellationRequested && Status)
                        {
                            manualResetEvent.Set();
                            if (TaskList.Count(x => !x.Completed) > 0)
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
                    Main.FormAccessor.Button_StopTaskManager.IsEnabled = false;

                    Status = false;
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

        public static void AddTask(Definitions.List.TaskList Task)
        {
            try
            {
                TaskList.Add(Task);

                if (Status)
                    manualResetEvent.Set();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show(ex.ToString());
                Functions.Logger.LogToFile(Functions.Logger.LogType.TaskManager, ex.ToString());
            }
        }

        public static void RemoveTask(Definitions.List.TaskList Task)
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