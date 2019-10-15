using Steam_Library_Manager.Definitions.Enums;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Alphaleonis.Win32.Filesystem;
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
        public static bool Status, Paused;
        public static Definitions.List.TaskInfo ActiveTask;
        public static Definitions.List.TmInfo TmInfo { get; } = new Definitions.List.TmInfo();
        private static bool _isRestartRequired;

        private static readonly IProgress<Definitions.List.TaskInfo> AddTaskProgress = new Progress<Definitions.List.TaskInfo>(task => TaskList.Add(task));
        private static readonly IProgress<Definitions.List.TaskInfo> RemoveTaskProgress = new Progress<Definitions.List.TaskInfo>(task => TaskList.Remove(task));

        private static async Task ProcessTaskAsync(Definitions.List.TaskInfo currentTask)
        {
            try
            {
                TmInfoUpdate();

                ActiveTask = currentTask;
                currentTask.Active = true;

                switch (currentTask.TaskType)
                {
                    default:
                        await currentTask.App.CopyFilesAsync(currentTask, CancellationToken.Token).ConfigureAwait(false);
                        break;

                    case TaskType.Delete:
                        await currentTask.App.DeleteFilesAsync(currentTask).ConfigureAwait(false);
                        currentTask.App.Library.Apps.Remove(currentTask.App);
                        break;

                    case TaskType.Compact:
                        await currentTask.App.CompactTask(currentTask, CancellationToken.Token).ConfigureAwait(false);
                        break;
                }

                if (!CancellationToken.IsCancellationRequested && !currentTask.ErrorHappened)
                {
                    if (currentTask.RemoveOldFiles && currentTask.TaskType != TaskType.Delete && currentTask.TaskType != TaskType.Compact)
                    {
                        Main.FormAccessor.TmLogs.Report(Framework.StringFormat.Format(SLM.Translate(nameof(Properties.Resources.TaskManager_RemoveOldFiles)), new { CurrentTime = DateTime.Now, currentTask.App.AppName }));
                        await currentTask.App.DeleteFilesAsync(currentTask).ConfigureAwait(false);
                        currentTask.App.Library.Apps.Remove(currentTask.App);
                        Main.FormAccessor.TmLogs.Report(Framework.StringFormat.Format(SLM.Translate(nameof(Properties.Resources.TaskManager_RemoveOldFilesCompleted)), new { CurrentTime = DateTime.Now, currentTask.App.AppName }));
                    }

                    if (currentTask.TargetLibrary?.Type == LibraryType.Steam)
                    {
                        _isRestartRequired = true;
                    }

                    currentTask.TaskStatusInfo = SLM.Translate(nameof(Properties.Resources.TaskStatus_Completed));
                    currentTask.Active = false;
                    currentTask.Completed = true;

                    currentTask.TargetLibrary?.UpdateAppList();

                    if (currentTask.AutoInstall && !currentTask.Compress)
                    {
                        while (currentTask.TargetLibrary.IsUpdatingAppList)
                        {
                            await Task.Delay(100);
                        }

                        switch (currentTask.TargetLibrary.Type)
                        {
                            case LibraryType.Steam:
                            case LibraryType.SLM:
                                // Not available
                                break;

                            case LibraryType.Origin:
                            case LibraryType.Uplay:
                                currentTask.TargetLibrary.Apps.First(x => x.AppId == currentTask.App.AppId && x.IsCompressed == currentTask.Compress)?.InstallAsync();
                                break;
                        }
                    }

                    // Update library details
                    if (Definitions.SLM.CurrentSelectedLibrary == currentTask.App.Library)
                    {
                        App.UpdateAppPanel(currentTask.App.Library);
                    }
                }

                if (TaskList.Count(x => !x.Completed) == 0)
                {
                    if (Properties.Settings.Default.PlayASoundOnCompletion)
                    {
                        if (!string.IsNullOrEmpty(Properties.Settings.Default.CustomSoundFile) && File.Exists(Properties.Settings.Default.CustomSoundFile))
                        {
                            using (var soundPlayer = new System.Media.SoundPlayer(Properties.Settings.Default.CustomSoundFile))
                            {
                                soundPlayer.Play();
                            }
                        }
                        else
                        {
                            System.Media.SystemSounds.Exclamation.Play();
                        }
                    }

                    if (_isRestartRequired && !Properties.Settings.Default.TaskManager_SteamRestartSkip)
                    {
                        Steam.RestartSteamAsync();
                        _isRestartRequired = false;
                    }
                }

                if (Properties.Settings.Default.TaskManager_AutoClear && !currentTask.ErrorHappened)
                {
                    RemoveTaskProgress.Report(currentTask);
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
                ActiveTask.Active = true;
                ActiveTask.mre.Set();
            }
        }

        public static void Pause()
        {
            try
            {
                if (!Status || ActiveTask == null) return;

                Paused = true;
                ActiveTask.Active = false;
                ActiveTask.mre.Reset();

                Main.FormAccessor.TmLogs.Report(Framework.StringFormat.Format(SLM.Translate(nameof(Properties.Resources.TaskManager_Paused)), new { CurrentTime = DateTime.Now }));
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
                if (!Status) return;

                Status = false;
                Paused = false;
                CancellationToken.Cancel();
                _isRestartRequired = false;
                ActiveTask?.mre?.Set();
                TmInfoUpdate();
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }

        public static void AddTask(Definitions.List.TaskInfo task)
        {
            try
            {
                AddTaskProgress.Report(task);

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
            TmInfo.PendingTasks = TaskList.Count(x => !x.Active && !x.Completed);
            TmInfo.CompletedTasks = TaskList.Count(x => !x.Active && x.Completed);
            TmInfo.TotalTasks = TaskList.Count;
        }
    }
}