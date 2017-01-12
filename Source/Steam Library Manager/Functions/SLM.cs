using System;
using System.Linq;
using System.Windows;

namespace Steam_Library_Manager.Functions
{
    class SLM
    {
        public class Settings
        {
            public static Func<Definitions.Game, object> GetSortingMethod()
            {
                Func<Definitions.Game, object> Sort;

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
                }

                return Sort;
            }

            public static void UpdateBackupDirs()
            {
                try
                {
                    // Define a new string collection to update backup library settings
                    System.Collections.Specialized.StringCollection BackupDirs = new System.Collections.Specialized.StringCollection();

                    // foreach defined library in library list
                    foreach (Definitions.Library Library in Definitions.List.Libraries.Where(x => x.IsBackup))
                    {
                        // then add this library path to new defined string collection
                        BackupDirs.Add(Library.FullPath);
                    }

                    // change our current backup directories setting with new defined string collection
                    Properties.Settings.Default.backupDirectories = BackupDirs;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

            public static void SaveSettings()
            {
                UpdateBackupDirs();
                ContextMenu.SaveLibraryContextMenuItems();
                ContextMenu.SaveGameContextMenuItems();
            }
        }

        public static void OnLoaded()
        {
            if (bool.Parse(Properties.Settings.Default.CheckforUpdatesAtStartup))
                Updater.CheckForUpdates();

            Steam.UpdateSteamInstallationPath();

            ContextMenu.ParseLibraryContextMenuItems();
            ContextMenu.ParseGameContextMenuItems();

            Library.GenerateLibraryList();
        }

        public static void OnClosing()
        {
            Settings.SaveSettings();
        }

    }
}
