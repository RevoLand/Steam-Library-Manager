using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Steam_Library_Manager.Framework
{
    class TaskManager
    {
        public static BlockingCollection<Definitions.List.TaskList> TaskList = new BlockingCollection<Definitions.List.TaskList>();
        public static CancellationTokenSource CancellationToken;
        public static bool Status = false;
        public static bool IsRestartRequired = false;

        public static void ProcessTask(Definitions.List.TaskList CurrentTask)
        {
            try
            {
                CurrentTask.Moving = true;
                CurrentTask.TargetGame.CopyGameFiles(CurrentTask, CancellationToken.Token);

                if (!CancellationToken.IsCancellationRequested)
                {
                    if (CurrentTask.RemoveOldFiles)
                    {
                        Main.Accessor.TaskManager_Logs.Add($"[{DateTime.Now}] [{CurrentTask.TargetGame.AppName}] Removing moven files as requested. This may take a while, please wait.");
                        CurrentTask.TargetGame.DeleteFiles();
                        Main.Accessor.TaskManager_Logs.Add($"[{DateTime.Now}] [{CurrentTask.TargetGame.AppName}] Files removen, task is completed now.");
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
                Functions.Logger.LogToFile(Functions.Logger.LogType.TaskManager, $"[{CurrentTask.TargetGame.AppName}][{CurrentTask.TargetGame.AppID}][{CurrentTask.TargetGame.AcfName}] {ex}");
            }
        }

        public static void Start()
        {
            if (!Status)
            {
                Main.Accessor.TaskManager_Logs.Add($"[{DateTime.Now}] [TaskManager] Task Manager is now active and waiting for tasks...");
                Main.Accessor.Button_StartTaskManager.IsEnabled = false;
                Main.Accessor.Button_StopTaskManager.IsEnabled = true;
                CancellationToken = new CancellationTokenSource();
                Status = true;

                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        while (true && !CancellationToken.IsCancellationRequested && Status)
                        {
                            ProcessTask(TaskList.Take(CancellationToken.Token));
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Stop();
                        Main.Accessor.TaskManager_Logs.Add($"[{DateTime.Now}] [TaskManager] Task Manager is stopped now...");
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
                    Main.Accessor.Button_StartTaskManager.IsEnabled = true;
                    Main.Accessor.Button_StopTaskManager.IsEnabled = false;

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

        public static void RemoveTask(Definitions.List.TaskList Task)
        {
            try
            {
                TaskList.TryTake(out Task);
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