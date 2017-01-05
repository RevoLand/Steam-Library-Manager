using System;
using System.Collections.Concurrent;
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
        public static BlockingCollection<Definitions.List.TaskList> TaskList = new BlockingCollection<Definitions.List.TaskList>();
        public static CancellationTokenSource CancellationToken = new CancellationTokenSource();
        public static bool Status = false;

        public static void ProcessTask(Definitions.List.TaskList currentTask)
        {
            try
            {
                Debug.WriteLine($"[{DateTime.Now}][TaskManager] Moving \"{currentTask.TargetGame.AppName}\" to \"{currentTask.TargetLibrary.FullPath}\"");

                currentTask.TargetGame.CopyGameFiles(currentTask.TargetLibrary, CancellationToken.Token, currentTask.Compress);

                if (!CancellationToken.IsCancellationRequested)
                {
                    // If game is not exists in the target library
                    if (currentTask.TargetLibrary.Games.Count(x => x.AcfName == currentTask.TargetGame.AcfName && currentTask.Compress == x.IsCompressed) == 0)
                    {
                        // Add game to new library
                        Functions.Games.AddNewGame(currentTask.TargetGame.FullAcfPath.FullName.Replace(currentTask.TargetGame.InstalledLibrary.steamAppsPath.FullName, currentTask.TargetLibrary.steamAppsPath.FullName), currentTask.TargetGame.AppID, currentTask.TargetGame.AppName, currentTask.TargetGame.InstallationPath.Name, currentTask.TargetLibrary, currentTask.TargetGame.SizeOnDisk, currentTask.Compress);

                        // Update library details
                        currentTask.TargetLibrary.UpdateLibraryVisual();
                    }

                    if (currentTask.RemoveOldFiles)
                    {
                        if (currentTask.TargetGame.DeleteFiles())
                        {
                            currentTask.TargetGame.RemoveFromLibrary();
                        }
                    }


                    if (TaskList.Count == 0)
                    {

                        if (Properties.Settings.Default.PlayASoundOnCompletion)
                        {
                            if (!string.IsNullOrEmpty(Properties.Settings.Default.CustomSoundFile) && File.Exists(Properties.Settings.Default.CustomSoundFile))
                                new System.Media.SoundPlayer(Properties.Settings.Default.CustomSoundFile).Play();
                            else
                                System.Media.SystemSounds.Exclamation.Play();
                        }

                        Functions.Steam.RestartSteamAsync();

                    }

                }

                Debug.WriteLine($"[{DateTime.Now}][TaskManager] Moven. \"{currentTask.TargetGame.AppName}\" to \"{currentTask.TargetLibrary.FullPath}\"");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show(ex.ToString());
            }
        }

        public static void Start()
        {
            try
            {
                if (!Status)
                {
                    Status = true;

                    Task.Factory.StartNew(() =>
                    {
                        while (true && !CancellationToken.IsCancellationRequested && Status)
                        {
                            Debug.WriteLine($"[{DateTime.Now}][TaskManager] Waiting for tasks...");
                            ProcessTask(TaskList.Take(CancellationToken.Token));
                        }
                    });
                }
            }
            catch (OperationCanceledException oEx)
            {
                Cancel();
                MessageBox.Show(oEx.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show(ex.ToString());
            }
        }

        public static void Cancel()
        {
            try
            {
                CancellationToken = new CancellationTokenSource();
                Status = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show(ex.ToString());
            }
        }

        public static void Stop()
        {
            try
            {
                CancellationToken.Cancel();
                Status = false;
                Debug.WriteLine($"[{DateTime.Now}][TaskManager] Stopped...");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show(ex.ToString());
            }
        }

        public static void RemoveTask(Definitions.List.TaskList Task)
        {
            try
            {
                TaskList.TryTake(out Task);
            }
            catch { }
        }

    }
}