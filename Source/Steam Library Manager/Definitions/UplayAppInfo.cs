using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Steam_Library_Manager.Definitions
{
    public class UplayAppInfo : App
    {
        public UplayAppInfo(Library library, string appName, int appId, DirectoryInfo installationDirectory, bool isCompressed)
        {
            Library = library;
            AppName = appName;
            AppId = appId;
            InstallationDirectory = installationDirectory;
            IsCompressed = isCompressed;

            LastUpdated = InstallationDirectory.LastWriteTimeUtc;
            CompressedArchivePath = new FileInfo(Path.Combine(Library.FullPath, AppId + ".zip"));
            SizeOnDisk = (!IsCompressed) ? Functions.FileSystem.GetDirectorySize(InstallationDirectory, true) : CompressedArchivePath.Length;
            IsCompacted = CompactStatus().Result;
        }

        public override async void ParseMenuItemActionAsync(string action)
        {
            try
            {
                switch (action.ToLowerInvariant())
                {
                    case "disk":
                        InstallationDirectory.Refresh();

                        if (InstallationDirectory.Exists)
                        {
                            Process.Start(InstallationDirectory.FullName);
                        }

                        break;

                    case "compress":
                        if (Functions.TaskManager.TaskList.Count(x => x.App == this && x.TargetLibrary == Library && x.TaskType == Enums.TaskType.Compress) == 0)
                        {
                            Functions.TaskManager.AddTask(new List.TaskInfo
                            {
                                App = this,
                                TargetLibrary = Library,
                                TaskType = Enums.TaskType.Compress,
                                Compress = !IsCompressed
                            });
                        }
                        break;

                    case "compact":
                        if (Functions.TaskManager.TaskList.Count(x => x.App == this && x.TargetLibrary == Library && x.TaskType == Enums.TaskType.Compact) == 0)
                        {
                            Functions.TaskManager.AddTask(new List.TaskInfo
                            {
                                App = this,
                                TargetLibrary = Library,
                                TaskType = Enums.TaskType.Compact
                            });
                        }
                        break;

                    case "deleteappfiles":
                        await Task.Run(async () => await DeleteFilesAsync()).ConfigureAwait(false);

                        Library.Apps.Remove(this);
                        if (SLM.CurrentSelectedLibrary == Library)
                            Functions.App.UpdateAppPanel(Library);

                        break;

                    case "deleteappfilestm":
                        Functions.TaskManager.AddTask(new List.TaskInfo
                        {
                            App = this,
                            TargetLibrary = Library,
                            TaskType = Enums.TaskType.Delete
                        });
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}