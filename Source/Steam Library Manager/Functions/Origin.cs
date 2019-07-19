using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace Steam_Library_Manager.Functions
{
    internal static class Origin
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static void PopulateLibraryCMenuItems()
        {
            #region App Context Menu Item Definitions

            var menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.OriginLibrary_CMenu_Open)),
                Action = "Disk",
                Icon = FontAwesome.WPF.FontAwesomeIcon.FolderOpen
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Origin);

            // Open library in explorer ({0})
            Definitions.List.LibraryCMenuItems.Add(menuItem);

            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.OriginLibrary_CMenu_RemoveFromSLM)),
                Action = "remove",
                Icon = FontAwesome.WPF.FontAwesomeIcon.Remove,
                ShowToNormal = false,
                ShowToSLMBackup = false
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Origin);

            Definitions.List.LibraryCMenuItems.Add(menuItem);

            #endregion App Context Menu Item Definitions
        }

        public static void PopulateAppCMenuItems()
        {
            #region App Context Menu Item Definitions

            // Run
            var menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.OriginApp_CMenu_Run)),
                Action = "steam://run/{0}", // TO-DO
                Icon = FontAwesome.WPF.FontAwesomeIcon.Play,
                ShowToCompressed = false
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Origin);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Compact
            menuItem = new Definitions.ContextMenuItem
            {
                Header = "Compact",
                Action = "compact",
                ShowToCompressed = false,
                Icon = FontAwesome.WPF.FontAwesomeIcon.FileArchiveOutline
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Origin);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Separator
            menuItem = new Definitions.ContextMenuItem
            {
                ShowToCompressed = false,
                IsSeparator = true
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Origin);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Install
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.OriginApp_CMenu_Install)),
                Action = "install",
                Icon = FontAwesome.WPF.FontAwesomeIcon.Gear,
                ShowToCompressed = false
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Origin);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Repair
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.OriginApp_CMenu_Repair)),
                Action = "repair",
                Icon = FontAwesome.WPF.FontAwesomeIcon.Gears,
                ShowToCompressed = false
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Origin);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Separator
            menuItem = new Definitions.ContextMenuItem
            {
                ShowToCompressed = false,
                IsSeparator = true
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Origin);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Show on disk
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.OriginApp_CMenu_DiskInfo)),
                Action = "Disk",
                Icon = FontAwesome.WPF.FontAwesomeIcon.FolderOpen
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Origin);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Separator
            menuItem = new Definitions.ContextMenuItem
            {
                ShowToCompressed = false,
                IsSeparator = true
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Origin);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Delete files (using SLM)
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.OriginApp_CMenu_DeleteFilesSLM)),
                Action = "deleteappfiles",
                Icon = FontAwesome.WPF.FontAwesomeIcon.TrashOutline
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Origin);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Delete files (using Task Manager)
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.OriginApp_CMenu_DeleteFilesTM)),
                Action = "deleteappfilestm",
                Icon = FontAwesome.WPF.FontAwesomeIcon.Trash
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Origin);
            Definitions.List.AppCMenuItems.Add(menuItem);

            #endregion App Context Menu Item Definitions
        }

        public static void GenerateLibraryList()
        {
            try
            {
                // If local.xml exists
                if (File.Exists(Definitions.Global.Origin.ConfigFilePath))
                {
                    var originConfigKeys = XDocument.Load(Definitions.Global.Origin.ConfigFilePath).Root?.Elements().ToDictionary(a => (string)a.Attribute("key"), a => (string)a.Attribute("value"));

                    if (originConfigKeys.Count(x => x.Key == "DownloadInPlaceDir") == 0)
                    {
                        Logger.Log(NLog.LogLevel.Error, Framework.StringFormat.Format(SLM.Translate(nameof(Properties.Resources.Origin_MissingKey)), new { OriginConfigFilePath = Definitions.Global.Origin.ConfigFilePath }));
                    }
                    else
                    {
                        if (Directory.Exists(originConfigKeys["DownloadInPlaceDir"]))
                        {
                            AddNewAsync(originConfigKeys["DownloadInPlaceDir"], true);
                        }
                        else
                        {
                            Logger.Info(Framework.StringFormat.Format(SLM.Translate(nameof(Properties.Resources.Origin_DirectoryNotExists)), new { NotFoundDirectoryFullPath = originConfigKeys["DownloadInPlaceDir"] }));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }

        public static async void AddNewAsync(string libraryPath, bool isMainLibrary = false)
        {
            try
            {
                if (!libraryPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    libraryPath += Path.DirectorySeparatorChar;
                }

                var newLibrary = new Definitions.OriginLibrary(libraryPath, isMainLibrary)
                {
                    Type = Definitions.Enums.LibraryType.Origin,
                    DirectoryInfo = new DirectoryInfo(libraryPath)
                };

                Definitions.List.LibraryProgress.Report(newLibrary);

                await Task.Run((Action) newLibrary.UpdateAppListAsync).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }

        public static bool IsLibraryExists(string newLibraryPath)
        {
            try
            {
                newLibraryPath = newLibraryPath.ToLowerInvariant();

                return Definitions.List.Libraries.Count(x =>
                 x.Type == Definitions.Enums.LibraryType.Origin
                 && (
                     x.FullPath.ToLowerInvariant() == newLibraryPath
                 )
                ) > 0;
            }
            // In any error return true to prevent possible bugs
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return true;
            }
        }
    }
}