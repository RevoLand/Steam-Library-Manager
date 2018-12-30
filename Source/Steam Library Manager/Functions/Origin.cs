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
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void PopulateLibraryCMenuItems()
        {
            #region App Context Menu Item Definitions

            // Open library in explorer ({0})
            Definitions.List.LibraryCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(Properties.Resources.OriginLibrary_CMenu_Open),
                Action = "Disk",
                Icon = FontAwesome.WPF.FontAwesomeIcon.FolderOpen,
                LibraryType = Definitions.Enums.LibraryType.Origin
            });

            Definitions.List.LibraryCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(Properties.Resources.OriginLibrary_CMenu_RemoveFromSLM),
                Action = "remove",
                Icon = FontAwesome.WPF.FontAwesomeIcon.Remove,
                LibraryType = Definitions.Enums.LibraryType.Origin,
                ShowToNormal = false,
                ShowToSLMBackup = false
            });

            #endregion App Context Menu Item Definitions
        }

        public static void PopulateAppCMenuItems()
        {
            #region App Context Menu Item Definitions

            // Run
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(Properties.Resources.OriginApp_CMenu_Run),
                Action = "steam://run/{0}", // TO-DO
                Icon = FontAwesome.WPF.FontAwesomeIcon.Play,
                LibraryType = Definitions.Enums.LibraryType.Origin,
                ShowToCompressed = false
            });

            // Separator
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                ShowToCompressed = false,
                LibraryType = Definitions.Enums.LibraryType.Origin,
                IsSeparator = true
            });

            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(Properties.Resources.OriginApp_CMenu_Install),
                Action = "install",
                Icon = FontAwesome.WPF.FontAwesomeIcon.Gear,
                LibraryType = Definitions.Enums.LibraryType.Origin,
                ShowToCompressed = false
            });

            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(Properties.Resources.OriginApp_CMenu_Repair),
                Action = "repair",
                Icon = FontAwesome.WPF.FontAwesomeIcon.Gears,
                LibraryType = Definitions.Enums.LibraryType.Origin,
                ShowToCompressed = false
            });

            // Separator
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                ShowToCompressed = false,
                LibraryType = Definitions.Enums.LibraryType.Origin,
                IsSeparator = true
            });

            // Show on disk
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(Properties.Resources.OriginApp_CMenu_DiskInfo),
                Action = "Disk",
                LibraryType = Definitions.Enums.LibraryType.Origin,
                Icon = FontAwesome.WPF.FontAwesomeIcon.FolderOpen
            });

            // Separator
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                IsSeparator = true,
                LibraryType = Definitions.Enums.LibraryType.Origin
            });

            // Delete files (using SLM)
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(Properties.Resources.OriginApp_CMenu_DeleteFilesSLM),
                Action = "deleteappfiles",
                LibraryType = Definitions.Enums.LibraryType.Origin,
                Icon = FontAwesome.WPF.FontAwesomeIcon.TrashOutline
            });

            // Delete files (using Task Manager)
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(Properties.Resources.OriginApp_CMenu_DeleteFilesTM),
                Action = "deleteappfilestm",
                LibraryType = Definitions.Enums.LibraryType.Origin,
                Icon = FontAwesome.WPF.FontAwesomeIcon.Trash
            });

            #endregion App Context Menu Item Definitions
        }

        public static void GenerateLibraryList()
        {
            try
            {
                // If local.xml exists
                if (File.Exists(Definitions.Global.Origin.ConfigFilePath))
                {
                    var OriginConfigKeys = XDocument.Load(Definitions.Global.Origin.ConfigFilePath).Root.Elements().ToDictionary(a => (string)a.Attribute("key"), a => (string)a.Attribute("value"));

                    if (OriginConfigKeys.Count(x => x.Key == "DownloadInPlaceDir") == 0)
                    {
                        logger.Log(NLog.LogLevel.Error, SLM.Translate(Properties.Resources.Origin_MissingKey), Definitions.Global.Origin.ConfigFilePath);
                    }
                    else
                    {
                        if (Directory.Exists(OriginConfigKeys["DownloadInPlaceDir"]))
                        {
                            AddNewAsync(OriginConfigKeys["DownloadInPlaceDir"], true);
                        }
                        else
                        {
                            MessageBox.Show(string.Format(SLM.Translate(Properties.Resources.Origin_DirectoryNotExists), OriginConfigKeys["DownloadInPlaceDir"]));
                        }
                    }
                }
                else { /* Could not locate local.xml */ }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                logger.Fatal(ex);
                Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
            }
        }

        public static async void AddNewAsync(string LibraryPath, bool IsMainLibrary = false)
        {
            try
            {
                Definitions.OriginLibrary Library = new Definitions.OriginLibrary(LibraryPath, IsMainLibrary);

                Definitions.List.Libraries.Add(new Definitions.Library
                {
                    Type = Definitions.Enums.LibraryType.Origin,
                    DirectoryInfo = new DirectoryInfo(LibraryPath),
                    Origin = Library
                });

                await Task.Run(() => Library.UpdateAppList());
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
                ex.Data.Add("LibraryPath", LibraryPath);
                ex.Data.Add("CurrentLibraries", Definitions.List.Libraries);
                Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
            }
        }

        public static bool IsLibraryExists(string NewLibraryPath)
        {
            try
            {
                NewLibraryPath = NewLibraryPath.ToLowerInvariant();

                return Definitions.List.Libraries.Count(x =>
                 x.Type == Definitions.Enums.LibraryType.Origin
                 && (
                     x.Origin.FullPath.ToLowerInvariant() == NewLibraryPath
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