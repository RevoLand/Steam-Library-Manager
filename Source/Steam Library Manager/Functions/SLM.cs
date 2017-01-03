using FontAwesome.WPF;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

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

            public static void ParseLibraryContextMenuItems()
            {
                string[] menuItems = Properties.Settings.Default.libraryContextMenu.Split('|');

                foreach (string menuItem in menuItems)
                {
                    if (string.IsNullOrEmpty(menuItem))
                        continue;

                    Definitions.ContextMenu cItem = new Definitions.ContextMenu();
                    string[] Item = menuItem.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string hardtonamethings in Item)
                    {
                        string[] itemDetails = hardtonamethings.Split(new char[] { '=' }, 2);
                        cItem.IconColor = (Brush)new BrushConverter().ConvertFromInvariantString("black");

                        switch (itemDetails[0].ToLowerInvariant())
                        {
                            case "text":
                                cItem.Header = itemDetails[1];
                                break;
                            case "action":
                                cItem.Action = itemDetails[1];
                                break;
                            case "iconcolor":
                                cItem.IconColor = (Brush)new BrushConverter().ConvertFromInvariantString(itemDetails[1]);
                                break;
                            case "icon":
                                cItem.Icon = (FontAwesomeIcon)Enum.Parse(typeof(FontAwesomeIcon), itemDetails[1], true);
                                break;
                            case "showtonormal":
                                cItem.ShowToNormal = (Definitions.Enums.menuVisibility)Enum.Parse(typeof(Definitions.Enums.menuVisibility), itemDetails[1], true);
                                break;
                            case "showtoslmbackup":
                                cItem.ShowToSLMBackup = (Definitions.Enums.menuVisibility)Enum.Parse(typeof(Definitions.Enums.menuVisibility), itemDetails[1], true);
                                break;
                            case "active":
                                cItem.IsActive = bool.Parse(itemDetails[1]);
                                break;
                            case "separator":
                                cItem.IsSeparator = bool.Parse(itemDetails[1]);
                                break;
                        }
                    }

                    Definitions.List.libraryContextMenuItems.Add(cItem);
                }
            }

            public static void SaveLibraryContextMenuItems()
            {
                string libraryContextMenu = "";
                foreach (Definitions.ContextMenu cItem in Definitions.List.libraryContextMenuItems)
                {
                    if (!string.IsNullOrEmpty(cItem.Header))
                        libraryContextMenu += $"text={cItem.Header}";

                    if (!string.IsNullOrEmpty(cItem.Action))
                        libraryContextMenu += $";;action={cItem.Action}";

                    if (!string.IsNullOrEmpty(cItem.Icon.ToString()))
                        libraryContextMenu += $";;icon={cItem.Icon}";

                    if (cItem.IconColor != null)
                        libraryContextMenu += $";;iconcolor={cItem.IconColor}";


                    libraryContextMenu += $";;showToNormal={cItem.ShowToNormal}";

                    libraryContextMenu += $";;showToSLMBackup={cItem.ShowToSLMBackup}";

                    libraryContextMenu += $";;separator={cItem.IsSeparator}";

                    libraryContextMenu += $";;active={cItem.IsActive}";

                    libraryContextMenu += "|";
                }

                Properties.Settings.Default.libraryContextMenu = libraryContextMenu;
            }

            public static void ParseGameContextMenuItems()
            {
                try
                {
                    string[] menuItems = Properties.Settings.Default.gameContextMenu.Split('|');

                    foreach (string menuItem in menuItems)
                    {
                        if (string.IsNullOrEmpty(menuItem))
                            continue;

                        Definitions.ContextMenu cItem = new Definitions.ContextMenu();
                        string[] Item = menuItem.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string hardtonamethings in Item)
                        {
                            string[] itemDetails = hardtonamethings.Split(new char[] { '=' }, 2);

                            cItem.IconColor = (Brush)new BrushConverter().ConvertFromInvariantString("black");

                            switch (itemDetails[0].ToLowerInvariant())
                            {
                                case "text":
                                    cItem.Header = itemDetails[1];
                                    break;
                                case "action":
                                    cItem.Action = itemDetails[1];
                                    break;
                                case "iconcolor":
                                    cItem.IconColor = (Brush)new BrushConverter().ConvertFromInvariantString(itemDetails[1]);
                                    break;
                                case "icon":
                                    cItem.Icon = (FontAwesomeIcon)Enum.Parse(typeof(FontAwesomeIcon), itemDetails[1], true);
                                    break;
                                case "showToNormal":
                                    cItem.ShowToNormal = (Definitions.Enums.menuVisibility)Enum.Parse(typeof(Definitions.Enums.menuVisibility), itemDetails[1], true);
                                    break;
                                case "showToSLMBackup":
                                    cItem.ShowToSLMBackup = (Definitions.Enums.menuVisibility)Enum.Parse(typeof(Definitions.Enums.menuVisibility), itemDetails[1], true);
                                    break;
                                case "showToSteamBackup":
                                    cItem.ShowToSteamBackup = (Definitions.Enums.menuVisibility)Enum.Parse(typeof(Definitions.Enums.menuVisibility), itemDetails[1], true);
                                    break;
                                case "showToCompressed":
                                    cItem.ShowToCompressed = (Definitions.Enums.menuVisibility)Enum.Parse(typeof(Definitions.Enums.menuVisibility), itemDetails[1], true);
                                    break;
                                case "active":
                                    cItem.IsActive = bool.Parse(itemDetails[1]);
                                    break;
                                case "separator":
                                    cItem.IsSeparator = bool.Parse(itemDetails[1]);
                                    break;
                            }
                        }

                        Definitions.List.gameContextMenuItems.Add(cItem);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

            public static void SaveGameContextMenuItems()
            {
                string gameContextMenu = "";
                foreach (Definitions.ContextMenu cItem in Definitions.List.gameContextMenuItems)
                {
                    if (!string.IsNullOrEmpty(cItem.Header))
                        gameContextMenu += $"text={cItem.Header}";

                    if (!string.IsNullOrEmpty(cItem.Action))
                        gameContextMenu += $";;action={cItem.Action}";

                    gameContextMenu += $";;icon={cItem.Icon}";

                    if (cItem.IconColor != null)
                        gameContextMenu += $";;iconcolor={cItem.IconColor}";

                    gameContextMenu += $";;showToNormal={cItem.ShowToNormal}";
                    gameContextMenu += $";;showToSLMBackup={cItem.ShowToSLMBackup}";
                    gameContextMenu += $";;showToSteamBackup={cItem.ShowToSteamBackup}";
                    gameContextMenu += $";;showToCompressed={cItem.ShowToCompressed}";

                    gameContextMenu += $";;active={cItem.IsActive}";
                    gameContextMenu += $";;separator={cItem.IsSeparator}";

                    gameContextMenu += "|";
                }

                Properties.Settings.Default.gameContextMenu = gameContextMenu;
            }

            public static void UpdateBackupDirs()
            {
                try
                {
                    // Define a new string collection to update backup library settings
                    System.Collections.Specialized.StringCollection BackupDirs = new System.Collections.Specialized.StringCollection();

                    // foreach defined library in library list
                    foreach (Definitions.Library Library in Definitions.List.Libraries.Where(x => x.Backup))
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
                SaveLibraryContextMenuItems();
                SaveGameContextMenuItems();
            }
        }

        public static void OnLoaded()
        {
            if (bool.Parse(Properties.Settings.Default.CheckforUpdatesAtStartup))
                Updater.CheckForUpdates();

            Steam.UpdateSteamInstallationPath();

            Settings.ParseLibraryContextMenuItems();
            Settings.ParseGameContextMenuItems();

            Library.GenerateLibraryList();
        }

        public static void OnClosing()
        {
            Settings.UpdateBackupDirs();
            Settings.SaveSettings();
        }

    }
}
