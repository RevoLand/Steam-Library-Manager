using FontAwesome.WPF;
using System;
using System.Windows;
using System.Windows.Media;

namespace Steam_Library_Manager.Functions
{
    class ContextMenu
    {

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

    }
}
