using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Steam_Library_Manager.Definitions
{
    public class SteamAppInfo : AppBase
    {
        public bool IsSteamBackup { get; set; }
        public DateTime LastPlayed { get; set; }

        public DirectoryInfo DownloadFolder => new DirectoryInfo(Path.Combine(Library.Steam.DownloadFolder.FullName, InstallationDirectory.Name));
        public DirectoryInfo WorkShopPath => new DirectoryInfo(Path.Combine(Library.Steam.WorkshopFolder.FullName, "content", AppId.ToString()));
        public FileInfo FullAcfPath => new FileInfo(Path.Combine(Library.Steam.SteamAppsFolder.FullName, AcfName));
        public FileInfo WorkShopAcfPath => new FileInfo(Path.Combine(Library.Steam.WorkshopFolder.FullName, WorkShopAcfName));
        public string AcfName { get; }
        public string WorkShopAcfName { get; }

        public SteamAppInfo(int appId, Library library, DirectoryInfo installationDirectory)
        {
            AppId = appId;
            Library = library;
            InstallationDirectory = installationDirectory;
            GameHeaderImage = $"http://cdn.akamai.steamstatic.com/steam/apps/{AppId}/header.jpg";
            AcfName = $"appmanifest_{AppId}.acf";
            WorkShopAcfName = $"appworkshop_{AppId}.acf";
            CompressedArchivePath = new FileInfo(Path.Combine(Library.Steam.SteamAppsFolder.FullName, AppId + ".zip"));
        }

        public async void ParseMenuItemActionAsync(string action)
        {
            try
            {
                switch (action.ToLowerInvariant())
                {
                    default:
                        if (string.IsNullOrEmpty(Properties.Settings.Default.SteamID64))
                        {
                            return;
                        }

                        Process.Start(string.Format(action, AppId, Properties.Settings.Default.SteamID64));
                        break;

                    case "compress":
                        if (Functions.TaskManager.TaskList.Count(x => x.SteamApp == this && x.TargetLibrary == Library && x.TaskType == Enums.TaskType.Compress && !x.Completed) == 0)
                        {
                            Functions.TaskManager.AddTask(new List.TaskInfo
                            {
                                SteamApp = this,
                                TargetLibrary = Library,
                                Compress = !IsCompressed,
                                TaskType = Enums.TaskType.Compress
                            });
                        }
                        break;

                    case "compact":
                        if (Functions.TaskManager.TaskList.Count(x => x.SteamApp == this && x.TargetLibrary == Library && x.TaskType == Enums.TaskType.Compact && !x.Completed) == 0)
                        {
                            Functions.TaskManager.AddTask(new List.TaskInfo
                            {
                                SteamApp = this,
                                TargetLibrary = Library,
                                TaskType = Enums.TaskType.Compact
                            });
                        }
                        break;

                    case "disk":
                        InstallationDirectory.Refresh();

                        if (InstallationDirectory.Exists)
                        {
                            Process.Start(InstallationDirectory.FullName);
                        }

                        break;

                    case "acffile":
                        FullAcfPath.Refresh();

                        if (FullAcfPath.Exists)
                            Process.Start(FullAcfPath.FullName);
                        break;

                    case "deleteappfiles":
                        await Task.Run(() => DeleteFilesAsync()).ConfigureAwait(false);

                        Library.Steam.Apps.Remove(this);
                        Functions.SLM.Library.UpdateLibraryVisual();

                        if (SLM.CurrentSelectedLibrary == Library)
                            Functions.App.UpdateAppPanel(Library);
                        break;

                    case "deleteappfilestm":
                        Functions.TaskManager.AddTask(new List.TaskInfo
                        {
                            SteamApp = this,
                            TargetLibrary = Library,
                            TaskType = Enums.TaskType.Delete
                        });
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }

        public override List<FileInfo> GetFileList()
        {
            try
            {
                var fileList = new List<FileInfo>();

                if (IsCompressed)
                {
                    fileList.Add(CompressedArchivePath);
                }
                else
                {
                    fileList.AddRange(base.GetFileList());

                    DownloadFolder.Refresh();
                    if (DownloadFolder.Exists)
                    {
                        var downloadFiles = GetDownloadFiles();
                        var patchFiles = GetPatchFiles();

                        if (downloadFiles != null)
                        {
                            fileList.AddRange(downloadFiles);
                        }

                        if (patchFiles != null)
                        {
                            fileList.AddRange(patchFiles);
                        }
                    }

                    WorkShopPath.Refresh();
                    if (WorkShopPath.Exists)
                    {
                        var workshopPath = GetWorkshopFiles();
                        fileList.AddRange(workshopPath);
                    }

                    FullAcfPath.Refresh();
                    if (FullAcfPath.Exists)
                    {
                        fileList.Add(FullAcfPath);
                    }

                    WorkShopPath.Refresh();
                    if (WorkShopAcfPath.Exists)
                    {
                        fileList.Add(WorkShopAcfPath);
                    }
                }

                return fileList;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                return null;
            }
        }

        private IEnumerable<FileInfo> GetDownloadFiles() => DownloadFolder.EnumerateFiles("*", SearchOption.AllDirectories);

        private IEnumerable<FileInfo> GetPatchFiles() => Library.Steam.DownloadFolder.EnumerateFiles($"*{AppId}*.patch", SearchOption.TopDirectoryOnly);

        private IEnumerable<FileInfo> GetWorkshopFiles() => WorkShopPath.EnumerateFiles("*", SearchOption.AllDirectories);

        public override async Task<bool> DeleteFilesAsync(List.TaskInfo CurrentTask = null)
        {
            try
            {
                if (IsCompressed)
                {
                    CompressedArchivePath.Refresh();

                    if (CompressedArchivePath.Exists)
                        await Task.Run(() => CompressedArchivePath.Delete()).ConfigureAwait(false);
                }
                else
                {
                    await base.DeleteFilesAsync();

                    DownloadFolder.Refresh();
                    // downloading folder, if exists
                    if (DownloadFolder.Exists)
                    {
                        await Task.Run(() => DownloadFolder.Delete(true)).ConfigureAwait(false);
                    }

                    WorkShopPath.Refresh();
                    // workshop folder, if exists
                    if (WorkShopPath.Exists)
                    {
                        await Task.Run(() => WorkShopPath.Delete(true)).ConfigureAwait(false);
                    }

                    FullAcfPath.Refresh();
                    // game .acf file
                    if (FullAcfPath.Exists)
                    {
                        File.SetAttributes(FullAcfPath.FullName, FileAttributes.Normal);
                        FullAcfPath.Delete();
                    }

                    WorkShopAcfPath.Refresh();
                    // workshop .acf file
                    if (WorkShopAcfPath.Exists)
                    {
                        File.SetAttributes(WorkShopAcfPath.FullName, FileAttributes.Normal);
                        WorkShopAcfPath.Delete();
                    }

                    if (CurrentTask != null)
                    {
                        CurrentTask.TaskStatusInfo = "";
                    }
                }

                return true;
            }
            catch (FileNotFoundException ex)
            {
                Logger.Error(ex);
                return true;
            }
            catch (DirectoryNotFoundException ex)
            {
                Logger.Error(ex);
                return true;
            }
            catch (IOException ex)
            {
                Logger.Error(ex);
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Error(ex);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Logger.Fatal(ex);

                return false;
            }
        }
    }
}