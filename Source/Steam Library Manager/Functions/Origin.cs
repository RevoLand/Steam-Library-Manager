using Alphaleonis.Win32.Filesystem;
using Dasync.Collections;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using SearchOption = System.IO.SearchOption;
using Stream = System.IO.Stream;

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
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.FolderOpen, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) }
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Origin);

            // Open library in explorer ({0})
            Definitions.List.LibraryCMenuItems.Add(menuItem);

            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.OriginLibrary_CMenu_RemoveFromSLM)),
                Action = "remove",
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.PlaylistRemove, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) },
                ShowToNormal = false
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
                Header = SLM.Translate(nameof(Properties.Resources.Run)),
                Action = "steam://run/{0}", // TO-DO
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.Play, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) },
                ShowToCompressed = false
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Origin);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Compress
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.Compress)),
                Action = "compress",
                Icon = new PackIconOcticons() { Kind = PackIconOcticonsKind.FileZip, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) }
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Origin);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Compact
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.Compact)),
                Action = "compact",
                ShowToCompressed = false,
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.ArrowCollapse, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) }
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
                Header = SLM.Translate(nameof(Properties.Resources.Install)),
                Action = "install",
                Icon = new PackIconEntypo() { Kind = PackIconEntypoKind.Install, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) },
                ShowToCompressed = false
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Origin);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Repair
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.OriginApp_CMenu_Repair)),
                Action = "repair",
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.FileCheck, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) },
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
                Header = SLM.Translate(nameof(Properties.Resources.MenuDiskInfo)),
                Action = "Disk",
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.FolderOpen, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) }
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Origin);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Separator
            menuItem = new Definitions.ContextMenuItem
            {
                IsSeparator = true
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Origin);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Delete files (using SLM)
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.DeleteFilesUsingSlm)),
                Action = "deleteappfiles",
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.Delete, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) }
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Origin);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Delete files (using Task Manager)
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.DeleteFilesUsingTaskmanager)),
                Action = "deleteappfilestm",
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.DeleteSweep, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) }
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Origin);
            Definitions.List.AppCMenuItems.Add(menuItem);

            #endregion App Context Menu Item Definitions
        }

        public static async Task GenerateLibraryListAsync()
        {
            try
            {
                // If local.xml exists
                if (File.Exists(Definitions.Global.Origin.ConfigFilePath))
                {
                    var originConfigKeys = XDocument.Load(Definitions.Global.Origin.ConfigFilePath).Root?.Elements().ToDictionary(a => (string)a.Attribute("key"), a => (string)a.Attribute("value"));

                    if (originConfigKeys?.Count(x => x.Key == "DownloadInPlaceDir") == 0)
                    {
                        Logger.Log(NLog.LogLevel.Error, Framework.StringFormat.Format(SLM.Translate(nameof(Properties.Resources.Origin_MissingKey)), new { OriginConfigFilePath = Definitions.Global.Origin.ConfigFilePath }));
                    }
                    else
                    {
                        if (Directory.Exists(originConfigKeys?["DownloadInPlaceDir"]))
                        {
                            AddNewLibraryAsync(originConfigKeys?["DownloadInPlaceDir"], true);
                        }
                        else
                        {
                            Logger.Info(Framework.StringFormat.Format(SLM.Translate(nameof(Properties.Resources.Origin_DirectoryNotExists)), new { NotFoundDirectoryFullPath = originConfigKeys["DownloadInPlaceDir"] }));
                        }
                    }
                }

                if (Directory.Exists(Definitions.Directories.Origin.LocalContentDirectory))
                {
                    await Directory.EnumerateFiles(Definitions.Directories.Origin.LocalContentDirectory, "*.mfst", SearchOption.AllDirectories).ParallelForEachAsync(originApp =>
                        {
                            var appId = Path.GetFileNameWithoutExtension(originApp);

                            if (!appId.StartsWith("Origin"))
                            {
                                // Get game id by fixing file via adding : before integer part of the name
                                // for example OFB-EAST52017 converts to OFB-EAST:52017
                                var match = System.Text.RegularExpressions.Regex.Match(appId, @"^(.*?)(\d+)$");
                                if (!match.Success)
                                {
                                    return Task.CompletedTask;
                                }

                                appId = match.Groups[1].Value + ":" + match.Groups[2].Value;
                            }

                            Definitions.Global.Origin.AppIds.Add(new KeyValuePair<string, string>(new FileInfo(originApp).Directory.Name, appId));

                            return Task.CompletedTask;
                        });
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }

        public static async void AddNewLibraryAsync(string libraryPath, bool isMainLibrary = false)
        {
            try
            {
                if (!libraryPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    libraryPath += Path.DirectorySeparatorChar;
                }

                var newLibrary = new Definitions.OriginLibrary(libraryPath, isMainLibrary);

                Definitions.List.LibraryProgress.Report(newLibrary);

                await Task.Run(newLibrary.UpdateAppList).ConfigureAwait(true);
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
                if (!newLibraryPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    newLibraryPath += Path.DirectorySeparatorChar;
                }

                return Definitions.List.Libraries.Count(x => x.Type == Definitions.Enums.LibraryType.Origin && string.Equals(x.FullPath, newLibraryPath, StringComparison.InvariantCultureIgnoreCase)) > 0;
            }
            // In any error return true to prevent possible bugs
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return true;
            }
        }

        public static async Task ParseAppDetailsAsync(Stream fileStream, string installerFilePath, Definitions.OriginLibrary library, bool isCompressed = false)
        {
            try
            {
                if (!isCompressed && new FileInfo(installerFilePath).Directory.Parent.Parent.Name != new DirectoryInfo(library.FullPath).Name)
                    return;

                var installerLog = Path.Combine(Directory.GetParent(installerFilePath).FullName, "InstallLog.txt");
                var installedLocale = "en_US";

                if (!isCompressed && File.Exists(installerLog))
                {
                    foreach (var line in File.ReadAllLines(installerLog))
                    {
                        if (!line.Contains("Install Locale:")) continue;

                        installedLocale = line.Split(new string[] { "Install Locale:" },
                            StringSplitOptions.None)[1];
                        break;
                    }

                    installedLocale = installedLocale.Replace(" ", "");
                }

                var xml = XDocument.Load(fileStream);
                var manifestVersion = new Version((xml.Root.Name.LocalName == "game")
                    ? xml.Root.Attribute("manifestVersion").Value
                    : ((xml.Root.Name.LocalName == "DiPManifest")
                        ? xml.Root.Attribute("version").Value
                        : "1.0"));

                Definitions.OriginAppInfo originAppInfo = null;

                if (manifestVersion == new Version("4.0"))
                {
                    originAppInfo = new Definitions.OriginAppInfo(library,
                        xml.Root.Element("gameTitles")?.Elements("gameTitle")
                            ?.First(x => x.Attribute("locale").Value == "en_US")?.Value,
                        Convert.ToInt32(xml.Root.Element("contentIDs")?.Elements()
                            .FirstOrDefault(x => int.TryParse(x.Value, out int appId))?.Value),
                        (isCompressed) ? new FileInfo(installerFilePath).Directory : new FileInfo(installerFilePath).Directory.Parent,
                        new Version(xml.Root.Element("buildMetaData")?.Element("gameVersion")
                            ?.Attribute("version")?.Value),
                        xml.Root.Element("installMetaData")?.Element("locales")?.Value.Split(','),
                        installedLocale,
                        isCompressed,
                        xml.Root.Element("touchup")?.Element("filePath")?.Value,
                        xml.Root.Element("touchup")?.Element("parameters")?.Value,
                        xml.Root.Element("touchup")?.Element("updateParameters")?.Value,
                        xml.Root.Element("touchup")?.Element("repairParameters")?.Value);
                }
                else if (manifestVersion >= new Version("1.1") && manifestVersion <= new Version("3.0"))
                {
                    var locales = new List<string>();
                    foreach (var locale in xml.Root.Element("metadata")?.Elements("localeInfo")
                        ?.Attributes()?.Where(x => x.Name == "locale"))
                    {
                        locales.Add(locale.Value);
                    }

                    originAppInfo = new Definitions.OriginAppInfo(library,
                        xml.Root.Element("metadata")?.Elements("localeInfo")
                            ?.First(x => x.Attribute("locale").Value == "en_US")?.Element("title").Value,
                        Convert.ToInt32(xml.Root.Element("contentIDs")?.Element("contentID")?.Value
                            .Replace("EAX", "")),
                        (isCompressed) ? new FileInfo(installerFilePath).Directory : new FileInfo(installerFilePath).Directory.Parent,
                        new Version(xml.Root.Attribute("gameVersion").Value),
                        locales.ToArray(),
                        installedLocale,
                        isCompressed,
                        xml.Root.Element("executable")?.Element("filePath")?.Value,
                        xml.Root.Element("executable")?.Element("parameters")?.Value);
                }
                else
                {
                    MessageBox.Show(Framework.StringFormat.Format(SLM.Translate(nameof(Properties.Resources.OriginUnknownManifestFile)), new { ManifestVersion = manifestVersion, OriginApp = installerFilePath }));
                    return;
                }

                if (Definitions.Global.Origin.AppIds.Count(x => x.Key == originAppInfo.InstallationDirectory.Name) > 0)
                {
                    var appId = Definitions.Global.Origin.AppIds.First(x => x.Key == originAppInfo.InstallationDirectory.Name);

                    var appLocalData = library.GetGameLocalData(appId.Value);

                    if (appLocalData != null)
                    {
                        await Framework.CachedImage.FileCache.HitAsync(string.Concat(appLocalData["customAttributes"]["imageServer"],
                                appLocalData["localizableAttributes"]["packArtLarge"])
                            , $"{originAppInfo.AppId}_o")
                            .ConfigureAwait(false);
                    }
                }

                originAppInfo.GameHeaderImage = $"{Definitions.Directories.SLM.Cache}\\{originAppInfo.AppId}_o.jpg";

                library.Apps.Add(originAppInfo);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Logger.Error(ex);
            }
        }

        public static async void CheckForBackupUpdatesAsync()
        {
            try
            {
                if (Definitions.List.Libraries.Count(x => x.Type == Definitions.Enums.LibraryType.Origin && x.DirectoryInfo.Exists) == 0)
                {
                    return;
                }

                var progressInformationMessage = await Main.FormAccessor.ShowProgressAsync(SLM.Translate(nameof(Properties.Resources.PleaseWait)), SLM.Translate(nameof(Properties.Resources.Steam_CheckForBackupUpdates))).ConfigureAwait(true);
                progressInformationMessage.SetIndeterminate();

                foreach (var currentLibrary in Definitions.List.Libraries.Where(x => x.Type == Definitions.Enums.LibraryType.Origin && x.DirectoryInfo.Exists).ToList())
                {
                    if (currentLibrary.Apps.Count == 0)
                    {
                        continue;
                    }

                    foreach (var libraryToCheck in Definitions.List.Libraries.Where(x => x != currentLibrary && x.Type == Definitions.Enums.LibraryType.Origin))
                    {
                        foreach (var currentApp in currentLibrary.Apps.Where(x => !x.IsCompressed).ToList())
                        {
                            progressInformationMessage.SetMessage(Framework.StringFormat.Format(SLM.Translate(nameof(Properties.Resources.Steam_CheckForBackupUpdates_Progress)), new { CurrentAppName = currentApp.AppName }));

                            foreach (var latestApp in libraryToCheck.Apps.Where(x => x.AppId == currentApp.AppId && x.LastUpdated > currentApp.LastUpdated && !x.IsCompressed))
                            {
                                if (TaskManager.TaskList.Count(x =>
                                        x.App.AppId == currentApp.AppId && !x.Completed &&
                                        (x.TargetLibrary == latestApp.Library ||
                                         x.TargetLibrary == currentApp.Library)) != 0) continue;

                                var newTask = new Definitions.List.TaskInfo
                                {
                                    App = latestApp,
                                    TargetLibrary = currentApp.Library,
                                    TaskType = Definitions.Enums.TaskType.Copy
                                };

                                TaskManager.AddTask(newTask);
                                Main.FormAccessor.TmLogs.Report(Framework.StringFormat.Format(SLM.Translate(nameof(Properties.Resources.Steam_CheckForBackupUpdates_UpdateFound)), new { CurrentTime = DateTime.Now, CurrentAppName = currentApp.AppName, NewAppLastUpdatedOn = latestApp.LastUpdated, CurrentAppLastUpdatedOn = currentApp.LastUpdated, CurrentAppSteamFullPath = currentApp.Library.FullPath, NewAppSteamFullPath = latestApp.Library.FullPath }));
                            }
                        }
                    }
                }

                await progressInformationMessage.CloseAsync().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }
    }
}