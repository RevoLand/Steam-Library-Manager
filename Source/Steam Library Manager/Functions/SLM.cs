using System;
using System.Linq;

namespace Steam_Library_Manager.Functions
{
    class SLM
    {
        public class Settings
        {
            public static Func<Definitions.List.Game, object> getSortingMethod()
            {
                Func<Definitions.List.Game, object> Sort;

                switch (Properties.Settings.Default.defaultGameSortingMethod)
                {
                    default:
                    case "appName":
                        Sort = x => x.appName;
                        break;
                    case "appID":
                        Sort = x => x.appID;
                        break;
                    case "sizeOnDisk":
                        Sort = x => x.sizeOnDisk;
                        break;
                }

                return Sort;
            }

            public static void updateBackupDirs()
            {
                try
                {
                    // Define a new string collection to update backup library settings
                    System.Collections.Specialized.StringCollection BackupDirs = new System.Collections.Specialized.StringCollection();

                    // foreach defined library in library list
                    foreach (Definitions.List.Library Library in Definitions.List.Libraries.Where(x => x.Backup))
                    {
                        // then add this library path to new defined string collection
                        BackupDirs.Add(Library.fullPath);
                    }

                    // change our current backup directories setting with new defined string collection
                    Properties.Settings.Default.backupDirectories = BackupDirs;
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.ToString());
                }
            }

            public static void saveSettings()
            {
                Properties.Settings.Default.Save();
            }
        }

        public static void onLoaded()
        {
            Steam.updateSteamInstallationPath();

            Library.generateLibraryList();
        }

        public static void onClosing()
        {
            Settings.updateBackupDirs();
            Settings.saveSettings();
        }

    }
}
