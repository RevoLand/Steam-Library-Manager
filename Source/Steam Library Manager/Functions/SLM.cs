using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Steam_Library_Manager.Functions
{
    internal static class SLM
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static class Settings
        {
            public static Func<dynamic, object> GetSortingMethod()
            {
                switch (Properties.Settings.Default.defaultGameSortingMethod)
                {
                    case "appName":
                    default:
                        return x => x.AppName;

                    case "appID":
                        return x => x.AppID;

                    case "sizeOnDisk":
                        return x => x.SizeOnDisk;

                    case "backupType":
                        return x => x.IsCompressed;

                    case "LastUpdated":
                        return x => x.LastUpdated;
                }
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
                    logger.Fatal(ex);
                    MessageBox.Show(ex.ToString());
                }
            }

            public static void UpdateOriginBackupDirectories()
            {
                try
                {
                    // Define a new string collection to update backup library settings
                    System.Collections.Specialized.StringCollection BackupDirs = new System.Collections.Specialized.StringCollection();

                    // foreach defined library in library list
                    foreach (Definitions.Library Library in Definitions.List.Libraries.Where(x => x.Type == Definitions.Enums.LibraryType.Origin && !x.Origin.IsMain))
                    {
                        // then add this library path to new defined string collection
                        BackupDirs.Add(Library.DirectoryInfo.FullName);
                    }

                    // change our current backup directories setting with new defined string collection
                    Properties.Settings.Default.OriginLibraries = BackupDirs;
                }
                catch (Exception ex)
                {
                    logger.Fatal(ex);
                    MessageBox.Show(ex.ToString());
                }
            }

            public static void SaveSettings()
            {
                UpdateBackupDirectories();
                UpdateOriginBackupDirectories();
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
                Library.GenerateOriginLibraryList();

                if (Properties.Settings.Default.ParallelAfterSize >= 20000000)
                {
                    Properties.Settings.Default.ParallelAfterSize /= 1000000;
                }
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
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
                logger.Fatal(ex);
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
                logger.Fatal(ex);
            }
        }

        public static void OnClosing()
        {
            Settings.SaveSettings();
            NLog.LogManager.Shutdown();
        }

        public static class Library
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
                        AddNewAsync(BackupPath);
                    }
                }
                catch (Exception ex)
                {
                    logger.Fatal(ex);
                    MessageBox.Show(ex.ToString());
                }
            }

            public static void GenerateOriginLibraryList()
            {
                try
                {
                    // If we have a backup library(s)
                    if (Properties.Settings.Default.OriginLibraries == null)
                        return;

                    if (Properties.Settings.Default.OriginLibraries.Count == 0)
                        return;

                    // for each backup library we have, do a loop
                    foreach (string BackupPath in Properties.Settings.Default.OriginLibraries)
                    {
                        Origin.AddNewAsync(BackupPath);
                    }
                }
                catch (Exception ex)
                {
                    logger.Fatal(ex);
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
                        Steam = (Directory.Exists(LibraryPath)) ? new Definitions.SteamLibrary(LibraryPath) : null,
                        //Origin = (Directory.Exists(Path.Combine(LibraryPath, "Origin"))) ? new Definitions.OriginLibrary(Path.Combine(LibraryPath, "Origin")) : null
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
                    logger.Fatal(ex);
                    MessageBox.Show(ex.ToString());
                }
            }

            public static bool IsLibraryExists(string NewLibraryPath)
            {
                try
                {
                    return Definitions.List.Libraries.Count(x => x.Type == Definitions.Enums.LibraryType.SLM) > 0 && Definitions.List.Libraries.Any(x => string.Equals(x.DirectoryInfo.FullName, NewLibraryPath, StringComparison.InvariantCultureIgnoreCase));
                }
                // In any error return true to prevent possible bugs
                catch (Exception ex)
                {
                    logger.Fatal(ex);
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
                    logger.Fatal(ex);
                }
            }
        }
    }
}