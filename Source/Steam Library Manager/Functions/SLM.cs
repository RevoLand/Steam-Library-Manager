using System;
using System.Linq;
using System.Windows;

namespace Steam_Library_Manager.Functions
{
    class SLM
    {
        public class Settings
        {
            public static Func<Definitions.Steam.AppInfo, object> GetSortingMethod()
            {
                Func<Definitions.Steam.AppInfo, object> Sort;

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

            public static void UpdateBackupDirs()
            {
                try
                {
                    // Define a new string collection to update backup library settings
                    System.Collections.Specialized.StringCollection BackupDirs = new System.Collections.Specialized.StringCollection();

                    // foreach defined library in library list
                    foreach (Definitions.Steam.Library Library in Definitions.List.SteamLibraries.Where(x => x.IsBackup))
                    {
                        // then add this library path to new defined string collection
                        BackupDirs.Add(Library.FullPath);
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
                UpdateBackupDirs();
            }
        }

        public static void PopulateLibraryCMenuItems()
        {
            #region App Context Menu Item Definitions

            // Open library in explorer ({0})
            Definitions.List.LibraryCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = "Open library in explorer ({0})",
                Action = "Disk",
                Icon = FontAwesome.WPF.FontAwesomeIcon.FolderOpen,
                ShowToOffline = false
            });

            // Separator
            Definitions.List.LibraryCMenuItems.Add(new Definitions.ContextMenuItem
            {
                IsSeparator = true,
                ShowToOffline = false
            });

            // Remove library & files
            Definitions.List.LibraryCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = "Remove library from Steam (/w files)",
                Action = "deleteLibrary",
                Icon = FontAwesome.WPF.FontAwesomeIcon.Trash,
                ShowToOffline = false
            });

            // Delete games in library
            Definitions.List.LibraryCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = "Delete games in library",
                Action = "deleteLibrarySLM",
                Icon = FontAwesome.WPF.FontAwesomeIcon.TrashOutline,
                ShowToOffline = false
            });

            // Separator
            Definitions.List.LibraryCMenuItems.Add(new Definitions.ContextMenuItem
            {
                IsSeparator = true,
                ShowToNormal = false,
                ShowToOffline = false
            });

            // Remove from SLM
            Definitions.List.LibraryCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = "Remove from SLM",
                Action = "RemoveFromList",
                Icon = FontAwesome.WPF.FontAwesomeIcon.Minus,
                ShowToNormal = false
            });

            #endregion
        }

        public static void PopulateAppCMenuItems()
        {
            #region App Context Menu Item Definitions

            // Run
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = "Run",
                Action = "steam://run/{0}",
                Icon = FontAwesome.WPF.FontAwesomeIcon.Play,
                ShowToCompressed = false
            });

            // Separator
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                ShowToCompressed = false,
                IsSeparator = true
            });

            // Show on disk
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = "{0} ({1})",
                Action = "Disk",
                Icon = FontAwesome.WPF.FontAwesomeIcon.FolderOpen
            });

            // View ACF
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = "View ACF File",
                Action = "acffile",
                Icon = FontAwesome.WPF.FontAwesomeIcon.PencilSquareOutline,
                ShowToCompressed = false
            });

            // Game hub
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = "Game Hub",
                Action = "steam://url/GameHub/{0}",
                Icon = FontAwesome.WPF.FontAwesomeIcon.Book
            });

            // Separator
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                IsSeparator = true
            });

            // Workshop
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = "Workshop",
                Action = "steam://url/SteamWorkshopPage/{0}",
                Icon = FontAwesome.WPF.FontAwesomeIcon.Cog
            });

            // Subscribed Workshop Items
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = "Subscribed Workshop Items",
                Action = "https://steamcommunity.com/profiles/{1}/myworkshopfiles/?appid={0}&browsefilter=mysubscriptions&sortmethod=lastupdated",
                Icon = FontAwesome.WPF.FontAwesomeIcon.Cogs
            });

            // Separator
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                IsSeparator = true
            });

            // Subscribed Workshop Items
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = "Delete files (using SLM)",
                Action = "deleteappfiles",
                Icon = FontAwesome.WPF.FontAwesomeIcon.Trash
            });

            #endregion
        }

        public static void OnLoad()
        {
            if (bool.Parse(Properties.Settings.Default.CheckforUpdatesAtStartup))
                Updater.CheckForUpdates();

            Steam.UpdateSteamInstallationPath();

            if (Properties.Settings.Default.ParallelAfterSize >= 20000000)
                Properties.Settings.Default.ParallelAfterSize = Properties.Settings.Default.ParallelAfterSize / 1000000;

            PopulateLibraryCMenuItems();
            PopulateAppCMenuItems();

            Library.GenerateLibraryList();
        }

        public static void OnClosing()
        {
            Settings.SaveSettings();
        }

    }
}
