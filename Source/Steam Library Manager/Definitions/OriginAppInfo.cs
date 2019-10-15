using MahApps.Metro.Controls.Dialogs;
using System;
using System.Diagnostics;
using Alphaleonis.Win32.Filesystem;
using System.Linq;
using System.Threading.Tasks;

namespace Steam_Library_Manager.Definitions
{
    public class OriginAppInfo : App
    {
        public string[] Locales { get; set; }

        public string InstalledLocale { get; set; }
        public FileInfo TouchupFile { get; set; }
        public string InstallationParameter { get; set; }
        public string UpdateParameter { get; set; }
        public string RepairParameter { get; set; }
        public Version AppVersion { get; set; }

        public OriginAppInfo(Library library, string appName, int appId, DirectoryInfo installationDirectory, Version appVersion, string[] locales, string installedLocale, bool isCompressed, string touchupFile, string installationParameter, string updateParameter = null, string repairParameter = null)
        {
            Library = library;
            AppName = appName;
            AppId = appId;
            Locales = locales;
            InstalledLocale = installedLocale;
            InstallationDirectory = installationDirectory;
            TouchupFile = new FileInfo(installationDirectory.FullName + touchupFile);
            InstallationParameter = installationParameter;
            UpdateParameter = updateParameter;
            RepairParameter = repairParameter;
            AppVersion = appVersion;
            LastUpdated = InstallationDirectory.LastWriteTime;
            IsCompressed = isCompressed;
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

                    case "install":

                        await InstallAsync().ConfigureAwait(true);

                        break;

                    case "repair":

                        await InstallAsync(true).ConfigureAwait(true);

                        break;

                    case "deleteappfiles":
                        await Task.Run(async () => await DeleteFilesAsync()).ConfigureAwait(true);

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

        public async Task InstallAsync(bool repair = false)
        {
            try
            {
                TouchupFile.Refresh();

                if (TouchupFile.Exists && !string.IsNullOrEmpty(InstallationParameter))
                {
                    if (repair && string.IsNullOrEmpty(RepairParameter))
                    {
                        return;
                    }

                    await Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                    {
                        var progressInformationMessage = await Main.FormAccessor.ShowProgressAsync(Functions.SLM.Translate(nameof(Properties.Resources.PleaseWait)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.OriginInstallation_Start)), new { AppName })).ConfigureAwait(true);
                        progressInformationMessage.SetIndeterminate();

                        var process = Process.Start(TouchupFile.FullName, ((repair) ? RepairParameter : InstallationParameter).Replace("{locale}", InstalledLocale).Replace("{installLocation}", InstallationDirectory.FullName));

                        Debug.WriteLine(InstallationParameter.Replace("{locale}", InstalledLocale).Replace("{installLocation}", InstallationDirectory.FullName));

                        progressInformationMessage.SetMessage(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.OriginInstallation_Ongoing)), new { AppName }));

                        while (!process.HasExited)
                        {
                            await Task.Delay(100).ConfigureAwait(true);
                        }

                        await progressInformationMessage.CloseAsync().ConfigureAwait(true);

                        var installLog = File.ReadAllLines(Path.Combine(InstallationDirectory.FullName, "__Installer", "InstallLog.txt")).Reverse().ToList();
                        if (installLog.Any(x => x.IndexOf("Installer finished with exit code:", StringComparison.OrdinalIgnoreCase) != -1))
                        {
                            var installerResult = installLog.FirstOrDefault(x => x.IndexOf("Installer finished with exit code:", StringComparison.OrdinalIgnoreCase) != -1);

                            await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.OriginInstallation)), Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.OriginInstallation_Completed)), new { installerResult })).ConfigureAwait(true);
                        }
                    }).ConfigureAwait(true);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}