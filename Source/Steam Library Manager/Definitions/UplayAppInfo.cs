using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using Alphaleonis.Win32.Filesystem;
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

            if (List.UplayAppIds.ContainsKey(AppName))
            {
                AppId = List.UplayAppIds[AppName];

                Debug.WriteLine($"AppId ({AppId}) set for Uplay game: {AppName}");
            }

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
                    default:
                        if (AppId != 0)
                        {
                            Process.Start(string.Format(action, AppId));
                        }

                        break;

                    case "disk":
                        InstallationDirectory.Refresh();

                        if (InstallationDirectory.Exists)
                        {
                            Process.Start(InstallationDirectory.FullName);
                        }

                        break;

                    case "install":
                        await InstallAsync();
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

        public async Task InstallAsync()
        {
            try
            {
                if (AppId <= 0) return;

                var installationsRegistry =
                    RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                        .OpenSubKey(Global.Uplay.InstallationsRegistryPath) ?? RegistryKey
                        .OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                        .OpenSubKey(Global.Uplay.InstallationsRegistryPath);

                using (var registry = installationsRegistry)
                {
                    using (var appRegistry = registry?.OpenSubKey(AppId.ToString(), RegistryKeyPermissionCheck.ReadWriteSubTree))
                    {
                        if (appRegistry?.GetValue("InstallDir") != null)
                        {
                            appRegistry.SetValue("InstallDir", !InstallationDirectory.FullName.EndsWith(Path.DirectorySeparatorChar.ToString()) ? string.Join("", InstallationDirectory.FullName, Path.DirectorySeparatorChar).Replace(Path.DirectorySeparatorChar, '/') : InstallationDirectory.FullName.Replace(Path.DirectorySeparatorChar, '/'));
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                await Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                    {
                        await Main.FormAccessor.ShowMessageAsync(
                            Functions.SLM.Translate(nameof(Properties.Resources.Uplay_UnauthorizedAccessExceptionTitle)),
                            Framework.StringFormat.Format(
                                Functions.SLM.Translate(nameof(Properties.Resources.Uplay_UnauthorizedAccessExceptionMessage)),
                                new { AppName, ExceptionMessage = ex.Message })).ConfigureAwait(true);
                    }, System.Windows.Threading.DispatcherPriority.Normal).ConfigureAwait(true);

                Logger.Fatal(ex);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }
    }
}