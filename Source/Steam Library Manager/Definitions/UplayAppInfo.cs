using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Steam_Library_Manager.Definitions
{
    public class UplayAppInfo : App
    {
        public string SpaceId { get; set; }

        public UplayAppInfo(Library library, string appName, string spaceId, DirectoryInfo installationDirectory, string headerImage, bool isCompressed)
        {
            Library = library;
            AppName = appName;
            SpaceId = spaceId;
            InstallationDirectory = installationDirectory;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.UplayExePath))
            {
                var fileInfo = new FileInfo(Properties.Settings.Default.UplayExePath);

                if (fileInfo.Exists)
                {
                    var assetsDirectoryInfo = new DirectoryInfo(Path.Combine(fileInfo.DirectoryName, "cache", "assets"));

                    if (assetsDirectoryInfo.Exists)
                    {
                        Framework.CachedImage.FileCache.HitAsync(Path.Combine(assetsDirectoryInfo.FullName, headerImage), $"{headerImage.Replace(".jpg", "")}_u")
                            .ConfigureAwait(false);

                        GameHeaderImage = $"{Directories.SLM.Cache}\\{headerImage.Replace(".jpg", "")}_u.jpg";
                    }
                    else
                    {
                        Logger.Warn($"Cache/Assets directory doesn't exists: {assetsDirectoryInfo.FullName}");
                    }
                }
                else
                {
                    Logger.Warn($"Uplay Executable Path doesn't exists: {Properties.Settings.Default.UplayExePath}");
                }
            }
            else
            {
                Logger.Warn($"Uplay Executable Path not set.");
            }

            IsCompressed = isCompressed;

            LastUpdated = InstallationDirectory.LastWriteTime;
            CompressedArchivePath = new FileInfo(Path.Combine(Library.FullPath, AppName + ".zip"));
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