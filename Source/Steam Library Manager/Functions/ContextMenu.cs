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
            try
            {
                string[] MenuItems = Properties.Settings.Default.libraryContextMenu.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string MenuItem in MenuItems)
                {
                    Definitions.ContextMenuItem CMenuITem = new Definitions.ContextMenuItem();

                    foreach (string MenuItemDetail in MenuItem.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        string[] MenuItemValue = MenuItemDetail.Split(new char[] { '=' }, 2);
                        CMenuITem.IconColor = (Brush)new BrushConverter().ConvertFromInvariantString("black");

                        switch (MenuItemValue[0].ToLowerInvariant())
                        {
                            case "text":
                                CMenuITem.Header = MenuItemValue[1];
                                break;
                            case "action":
                                CMenuITem.Action = MenuItemValue[1];
                                break;
                            case "iconcolor":
                                CMenuITem.IconColor = (Brush)new BrushConverter().ConvertFromInvariantString(MenuItemValue[1]);
                                break;
                            case "icon":
                                CMenuITem.Icon = (FontAwesomeIcon)Enum.Parse(typeof(FontAwesomeIcon), MenuItemValue[1], true);
                                break;
                            case "showtonormal":
                                CMenuITem.ShowToNormal = (Definitions.Enums.MenuVisibility)Enum.Parse(typeof(Definitions.Enums.MenuVisibility), MenuItemValue[1], true);
                                break;
                            case "showtoslmbackup":
                                CMenuITem.ShowToSLMBackup = (Definitions.Enums.MenuVisibility)Enum.Parse(typeof(Definitions.Enums.MenuVisibility), MenuItemValue[1], true);
                                break;
                            case "active":
                                CMenuITem.IsActive = bool.Parse(MenuItemValue[1]);
                                break;
                            case "separator":
                                CMenuITem.IsSeparator = bool.Parse(MenuItemValue[1]);
                                break;
                        }
                    }

                    Definitions.List.LibraryCMenuItems.Add(CMenuITem);

                }
            }
            catch (Exception ex)
            {
                Logger.LogToFile(Logger.LogType.SLM, ex.ToString());
                MessageBox.Show(ex.ToString());
            }
        }

        public static void SaveLibraryContextMenuItems()
        {
            string libraryContextMenu = "";
            foreach (Definitions.ContextMenuItem CMenuItem in Definitions.List.LibraryCMenuItems)
            {
                if (!string.IsNullOrEmpty(CMenuItem.Header))
                    libraryContextMenu += $"text={CMenuItem.Header}";

                if (!string.IsNullOrEmpty(CMenuItem.Action))
                    libraryContextMenu += $";;action={CMenuItem.Action}";

                if (!string.IsNullOrEmpty(CMenuItem.Icon.ToString()))
                    libraryContextMenu += $";;icon={CMenuItem.Icon}";

                if (CMenuItem.IconColor != null)
                    libraryContextMenu += $";;iconcolor={CMenuItem.IconColor}";


                libraryContextMenu += $";;showToNormal={CMenuItem.ShowToNormal}";

                libraryContextMenu += $";;showToSLMBackup={CMenuItem.ShowToSLMBackup}";

                libraryContextMenu += $";;separator={CMenuItem.IsSeparator}";

                libraryContextMenu += $";;active={CMenuItem.IsActive}";

                libraryContextMenu += "|";
            }

            Properties.Settings.Default.libraryContextMenu = libraryContextMenu;
        }

        public static void ParseGameContextMenuItems()
        {
            try
            {
                string[] MenuItems = Properties.Settings.Default.gameContextMenu.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string MenuItem in MenuItems)
                {
                    Definitions.ContextMenuItem CMenuItem = new Definitions.ContextMenuItem();

                    foreach (string MenuItemDetail in MenuItem.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        string[] MenuItemValue = MenuItemDetail.Split(new char[] { '=' }, 2);

                        CMenuItem.IconColor = (Brush)new BrushConverter().ConvertFromInvariantString("black");

                        switch (MenuItemValue[0].ToLowerInvariant())
                        {
                            case "text":
                                CMenuItem.Header = MenuItemValue[1];
                                break;
                            case "action":
                                CMenuItem.Action = MenuItemValue[1];
                                break;
                            case "iconcolor":
                                CMenuItem.IconColor = (Brush)new BrushConverter().ConvertFromInvariantString(MenuItemValue[1]);
                                break;
                            case "icon":
                                CMenuItem.Icon = (FontAwesomeIcon)Enum.Parse(typeof(FontAwesomeIcon), MenuItemValue[1], true);
                                break;
                            case "showToNormal":
                                CMenuItem.ShowToNormal = (Definitions.Enums.MenuVisibility)Enum.Parse(typeof(Definitions.Enums.MenuVisibility), MenuItemValue[1], true);
                                break;
                            case "showToSLMBackup":
                                CMenuItem.ShowToSLMBackup = (Definitions.Enums.MenuVisibility)Enum.Parse(typeof(Definitions.Enums.MenuVisibility), MenuItemValue[1], true);
                                break;
                            case "showToSteamBackup":
                                CMenuItem.ShowToSteamBackup = (Definitions.Enums.MenuVisibility)Enum.Parse(typeof(Definitions.Enums.MenuVisibility), MenuItemValue[1], true);
                                break;
                            case "showToCompressed":
                                CMenuItem.ShowToCompressed = (Definitions.Enums.MenuVisibility)Enum.Parse(typeof(Definitions.Enums.MenuVisibility), MenuItemValue[1], true);
                                break;
                            case "active":
                                CMenuItem.IsActive = bool.Parse(MenuItemValue[1]);
                                break;
                            case "separator":
                                CMenuItem.IsSeparator = bool.Parse(MenuItemValue[1]);
                                break;
                        }
                    }

                    Definitions.List.GameCMenuItems.Add(CMenuItem);
                }
            }
            catch (Exception ex)
            {
                Logger.LogToFile(Logger.LogType.SLM, ex.ToString());
                MessageBox.Show(ex.ToString());
            }
        }

        public static void SaveGameContextMenuItems()
        {
            string gameContextMenu = "";
            foreach (Definitions.ContextMenuItem CMenuItem in Definitions.List.GameCMenuItems)
            {
                if (!string.IsNullOrEmpty(CMenuItem.Header))
                    gameContextMenu += $"text={CMenuItem.Header}";

                if (!string.IsNullOrEmpty(CMenuItem.Action))
                    gameContextMenu += $";;action={CMenuItem.Action}";

                gameContextMenu += $";;icon={CMenuItem.Icon}";

                if (CMenuItem.IconColor != null)
                    gameContextMenu += $";;iconcolor={CMenuItem.IconColor}";

                gameContextMenu += $";;showToNormal={CMenuItem.ShowToNormal}";
                gameContextMenu += $";;showToSLMBackup={CMenuItem.ShowToSLMBackup}";
                gameContextMenu += $";;showToSteamBackup={CMenuItem.ShowToSteamBackup}";
                gameContextMenu += $";;showToCompressed={CMenuItem.ShowToCompressed}";

                gameContextMenu += $";;active={CMenuItem.IsActive}";
                gameContextMenu += $";;separator={CMenuItem.IsSeparator}";

                gameContextMenu += "|";
            }

            Properties.Settings.Default.gameContextMenu = gameContextMenu;
        }

    }
}
