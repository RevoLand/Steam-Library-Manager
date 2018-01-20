using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Steam_Library_Manager.Functions
{
    internal class SLM
    {
        public class Settings
        {
            public static Func<Definitions.SteamAppInfo, object> GetSortingMethod()
            {
                Func<Definitions.SteamAppInfo, object> Sort;

                switch (Properties.Settings.Default.defaultGameSortingMethod)
                {
                    default:
                    case "appName":
                        Sort = x => x.AppName;
                        break;
                    case "appID":
                        Sort = x => x.AppID;
                        break;
                    case "sizeOnDisk":
                        Sort = x => x.SizeOnDisk;
                        break;
                    case "backupType":
                        Sort = x => x.IsCompressed;
                        break;
                    case "LastUpdated":
                        Sort = x => x.LastUpdated;
                        break;
                }

                return Sort;
            }

            public static void UpdateBackupDirectories()
            {
                try
                {
                    // Define a new string collection to update backup library settings
                    System.Collections.Specialized.StringCollection BackupDirs = new System.Collections.Specialized.StringCollection();

                    // foreach defined library in library list
                    foreach (Definitions.Library Library in Definitions.List.Libraries.Where(x => x.Type == Definitions.Enums.LibraryType.SLM))
                    {
                        // then add this library path to new defined string collection
                        BackupDirs.Add(Library.DirectoryInfo.FullName);
                    }

                    // change our current backup directories setting with new defined string collection
                    Properties.Settings.Default.backupDirectories = BackupDirs;
                }
                catch (Exception ex)
                {
                    Logger.LogToFile(Logger.LogType.SLM, ex.ToString());
                    MessageBox.Show(ex.ToString());
                }
            }

            public static void SaveSettings()
            {
                UpdateBackupDirectories();
            }
        }

        public static void OnLoad()
        {
            try
            {
                if (bool.Parse(Properties.Settings.Default.CheckforUpdatesAtStartup))
                {
                    Updater.CheckForUpdates();
                }

                LoadSteam();
                LoadOrigin();

                // SLM Libraries
                Library.GenerateLibraryList();

                if (Properties.Settings.Default.ParallelAfterSize >= 20000000)
                {
                    Properties.Settings.Default.ParallelAfterSize = Properties.Settings.Default.ParallelAfterSize / 1000000;
                }
            }
            catch (Exception ex)
            {
                Logger.LogToFile(Logger.LogType.Library, ex.ToString());
            }
        }

        public static void LoadSteam()
        {
            try
            {
                Steam.UpdateSteamInstallationPathAsync();
                Steam.PopulateLibraryCMenuItems();
                Steam.PopulateAppCMenuItems();

                Steam.Library.GenerateLibraryList();
            }
            catch (Exception ex)
            {
                Logger.LogToFile(Logger.LogType.Library, ex.ToString());
            }
        }

        public static void LoadOrigin()
        {
            try
            {
                Origin.PopulateLibraryCMenuItems();
                Origin.PopulateAppCMenuItems();

                Origin.GenerateLibraryList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Logger.LogToFile(Logger.LogType.Library, ex.ToString());
            }
        }

        public static void OnClosing()
        {
            Settings.SaveSettings();
        }

        public class Library
        {
            public static void GenerateLibraryList()
            {
                try
                {
                    // If we have a backup library(s)
                    if (Properties.Settings.Default.backupDirectories == null)
                        return;

                    if (Properties.Settings.Default.backupDirectories.Count == 0)
                        return;

                    // for each backup library we have, do a loop
                    foreach (string BackupPath in Properties.Settings.Default.backupDirectories)
                    {
                        if (!string.IsNullOrEmpty(BackupPath))
                            AddNewAsync(BackupPath);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogToFile(Logger.LogType.Library, ex.ToString());
                    MessageBox.Show(ex.ToString());
                }
            }

            public static async void AddNewAsync(string LibraryPath)
            {
                try
                {
                    if (string.IsNullOrEmpty(LibraryPath))
                        return;

                    Definitions.Library Library = new Definitions.Library
                    {
                        Type = Definitions.Enums.LibraryType.SLM,
                        DirectoryInfo = new DirectoryInfo(LibraryPath),
                        Steam = (Directory.Exists(LibraryPath)) ? new Definitions.SteamLibrary()
                        {
                            FullPath = LibraryPath
                        } : null,
                        Origin = (Directory.Exists(Path.Combine(LibraryPath, "Origin"))) ? new Definitions.OriginLibrary(Path.Combine(LibraryPath, "Origin")) : null
                    };

                    Definitions.List.Libraries.Add(Library);

                    if (Library.Steam != null)
                    {
                        await Task.Run(() => Library.Steam.UpdateAppList());
                        await Task.Run(() => Library.Steam.UpdateJunks());
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogToFile(Logger.LogType.Library, ex.ToString());
                    MessageBox.Show(ex.ToString());
                }
            }

            public static bool IsLibraryExists(string NewLibraryPath)
            {
                try
                {
                    NewLibraryPath = NewLibraryPath.ToLowerInvariant();

                    if (Definitions.List.Libraries.Count(x => x.Type == Definitions.Enums.LibraryType.SLM) > 0)
                    {
                        if (Definitions.List.Libraries.Where(x => x.DirectoryInfo.FullName.ToLowerInvariant() == NewLibraryPath).Count() > 0)
                        {
                            return true;
                        }
                    }

                    // else, return false which means library is not exists
                    return false;
                }
                // In any error return true to prevent possible bugs
                catch (Exception ex)
                {
                    Logger.LogToFile(Logger.LogType.Library, ex.ToString());
                    MessageBox.Show(ex.ToString());
                    return true;
                }
            }

            public static async void UpdateBackupLibrary(Definitions.Library Library)
            {
                try
                {
                    if (Library.Steam != null)
                    {
                        await Task.Run(() => Library.Steam.UpdateAppList());
                        await Task.Run(() => Library.Steam.UpdateJunks());
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogToFile(Logger.LogType.Library, ex.ToString());
                }
            }
        }
    }
}
