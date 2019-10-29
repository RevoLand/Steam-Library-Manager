using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.IconPacks;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Directory = Alphaleonis.Win32.Filesystem.Directory;
using DirectoryInfo = Alphaleonis.Win32.Filesystem.DirectoryInfo;
using File = Alphaleonis.Win32.Filesystem.File;
using Path = Alphaleonis.Win32.Filesystem.Path;

namespace Steam_Library_Manager.Functions
{
    internal static class Uplay
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static void PopulateLibraryCMenuItems()
        {
            #region App Context Menu Item Definitions

            var menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.Uplay_Open)),
                Action = "Disk",
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.FolderOpen, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) }
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Uplay);

            // Open library in explorer ({0})
            Definitions.List.LibraryCMenuItems.Add(menuItem);

            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.RemoveFromSlm)),
                Action = "remove",
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.PlaylistRemove, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) },
                ShowToNormal = false
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Uplay);

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
                Action = "uplay://launch/{0}",
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.Play, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) },
                ShowToCompressed = false
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Uplay);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Compress
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.Compress)),
                Action = "compress",
                Icon = new PackIconOcticons() { Kind = PackIconOcticonsKind.FileZip, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) }
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Uplay);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Compact
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.Compact)),
                Action = "compact",
                ShowToCompressed = false,
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.ArrowCollapse, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) }
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Uplay);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Separator
            menuItem = new Definitions.ContextMenuItem
            {
                ShowToCompressed = false,
                IsSeparator = true
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Uplay);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Show on disk
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.MenuDiskInfo)),
                Action = "Disk",
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.FolderOpen, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) }
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Uplay);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Install
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.Install)),
                Action = "install",
                Icon = new PackIconEntypo() { Kind = PackIconEntypoKind.Install, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) },
                ShowToCompressed = false
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Uplay);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Separator
            menuItem = new Definitions.ContextMenuItem
            {
                IsSeparator = true
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Uplay);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Delete files (using SLM)
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.DeleteFilesUsingSlm)),
                Action = "deleteappfiles",
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.Delete, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) }
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Uplay);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Delete files (using Task Manager)
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.DeleteFilesUsingTaskmanager)),
                Action = "deleteappfilestm",
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.DeleteSweep, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) }
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Uplay);
            Definitions.List.AppCMenuItems.Add(menuItem);

            #endregion App Context Menu Item Definitions
        }

        public static void GenerateLibraryList()
        {
            if (File.Exists(Definitions.Global.Uplay.ConfigFilePath))
            {
                foreach (var line in File.ReadAllLines(Definitions.Global.Uplay.ConfigFilePath))
                {
                    if (!line.Contains("game_installation_path")) continue;

                    var newLine = line.Split(new[] { "  game_installation_path: " }, StringSplitOptions.RemoveEmptyEntries);

                    if (newLine.Length > 0)
                    {
                        if (Directory.Exists(newLine[0]))
                        {
                            AddNewLibraryAsync(newLine[0], true);
                        }
                        else
                        {
                            Logger.Info($"Directory set in the Uplay config file does not exists.\nDirectory path: {newLine[0]}");
                        }
                    }
                    else
                    {
                        Logger.Info("Couldn't parse Uplay config file for 'game_installation_path' - If you can find the value in the file, fill an issue form on github.");
                    }
                }
            }
            else
            {
                Logger.Info("Uplay config file is not found, skipping main uplay library detection.");
            }
        }

        public static void UpdateInstallationPath()
        {
            try
            {
                Properties.Settings.Default.UplayExePath = Registry
                    .GetValue(Definitions.Global.Uplay.LauncherRegistryPath, "InstallDir", "").ToString()
                    .Replace('/', Path.DirectorySeparatorChar);

                var installationsRegistry = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(Definitions.Global.Uplay.InstallationsRegistryPath) ?? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(Definitions.Global.Uplay.InstallationsRegistryPath);

                using (var appIdsRegistry = installationsRegistry)
                {
                    foreach (var appId in appIdsRegistry.GetSubKeyNames())
                    {
                        var appRegistryKey = appIdsRegistry.OpenSubKey(appId);
                        var installDirValue = appRegistryKey?.GetValue("InstallDir");

                        if (installDirValue == null) continue;

                        var installDir = ((string)installDirValue);

                        var appNamesSplit = (installDir.EndsWith(Path.DirectorySeparatorChar.ToString())) ? installDir.Split(Path.DirectorySeparatorChar) : installDir.Split('/');

                        var appName = appNamesSplit.ElementAtOrDefault(appNamesSplit.Length - 2);

                        if (appName != null && !Definitions.List.UplayAppIds.ContainsKey(appName))
                        {
                            Definitions.List.UplayAppIds.Add(appName, Convert.ToInt32(appId));
                        }
                    }
                }

                if (!string.IsNullOrEmpty(Properties.Settings.Default.UplayExePath))
                {
                    if (!Properties.Settings.Default.UplayExePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    {
                        Properties.Settings.Default.UplayExePath += Path.DirectorySeparatorChar;
                    }

                    Properties.Settings.Default.UplayDbPath = Path.Combine(Properties.Settings.Default.UplayExePath, "cache", "configuration", "configurations");
                    Properties.Settings.Default.UplayExePath += "Uplay.exe";
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                Debug.WriteLine(e);
            }
        }

        public static async Task InitializeUplayDb()
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.UplayDbPath) && File.Exists(Properties.Settings.Default.UplayDbPath))
            {
                await InitializeUplayDb(File.OpenText(Properties.Settings.Default.UplayDbPath));
            }
            else
            {
                await InitializeUplayDb(new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Steam_Library_Manager.Resources.configurations"), Encoding.UTF8));
            }
        }

        public static async Task InitializeUplayDb(StreamReader dbFileStream)
        {
            try
            {
                if (Definitions.List.UplayConfigurations.Count > 0)
                {
                    Definitions.List.UplayConfigurations.Clear();
                }

                using (var uplayDb = dbFileStream)
                {
                    var uplayDbEntry = new Definitions.UplayConfigurationDb();
                    var uplayDbEntryStarted = false;
                    while (!uplayDb.EndOfStream)
                    {
                        var line = await uplayDb.ReadLineAsync();

                        if (string.IsNullOrEmpty(line)) continue;

                        if (line.Contains("root:"))
                        {
                            if (uplayDbEntryStarted)
                            {
                                uplayDbEntry.Legacy = string.IsNullOrEmpty(uplayDbEntry.SpaceId);

                                if (!string.IsNullOrEmpty(uplayDbEntry.Name) &&
                                    !string.IsNullOrEmpty(uplayDbEntry.ThumbImage))
                                {
                                    Definitions.List.UplayConfigurations.Add(uplayDbEntry);
                                }

                                uplayDbEntry = new Definitions.UplayConfigurationDb();
                            }
                            else
                            {
                                uplayDbEntryStarted = true;
                            }
                        }

                        if (line.Contains("  name: ") && string.IsNullOrEmpty(uplayDbEntry.Name))
                        {
                            var name = line.Split(new[] { "name: " }, StringSplitOptions.RemoveEmptyEntries);

                            if (name.Length > 1 && !name[1].Contains("RELATED") && name[1].Length > 3)
                            {
                                uplayDbEntry.Name = name[1].Replace("\"", "");
                            }
                        }

                        if (line.Contains("game_identifier: "))
                        {
                            var identifier = line.Split(new[] { "  game_identifier: " },
                                StringSplitOptions.RemoveEmptyEntries);

                            if (identifier.Length > 1)
                            {
                                uplayDbEntry.Name = identifier[1].Replace("\"", "");
                            }
                        }

                        // thumb_image:
                        if (!string.IsNullOrEmpty(uplayDbEntry.ThumbImage) && (!uplayDbEntry.ThumbImage.Contains(".jpg") || !uplayDbEntry.ThumbImage.Contains(".png")) && line.Contains(uplayDbEntry.ThumbImage))
                        {
                            var thumbImage = line.Split(new[] { $"{uplayDbEntry.ThumbImage}:" }, StringSplitOptions.RemoveEmptyEntries);

                            if (thumbImage.Length > 1 && (thumbImage[1].Contains(".jpg") || thumbImage[1].Contains(".png")))
                            {
                                uplayDbEntry.ThumbImage = thumbImage[1].Replace(" ", "");
                            }
                        }

                        if (line.Contains("thumb_image:"))
                        {
                            var thumbImage = line.Split(new[] { "thumb_image:" }, StringSplitOptions.RemoveEmptyEntries);

                            if (thumbImage.Length > 1 && (thumbImage[1].Contains(".jpg") || thumbImage[1].Contains(".png")))
                            {
                                uplayDbEntry.ThumbImage = thumbImage[1].Replace(" ", "");
                            }
                            else
                            {
                                uplayDbEntry.ThumbImage = thumbImage[1];
                            }
                        }

                        if (line.Contains("  space_id: "))
                        {
                            var spaceId = line.Split(new[] { "  space_id: " }, StringSplitOptions.RemoveEmptyEntries);

                            if (spaceId.Length > 0)
                            {
                                uplayDbEntry.SpaceId = spaceId[0];
                            }
                        }
                    }

                    Debug.WriteLine($"Total Entries in Uplay Configuration DB: {Definitions.List.UplayConfigurations.Count}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
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

                var newLibrary = new Definitions.UplayLibrary(libraryPath, isMainLibrary);

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

                return Definitions.List.Libraries.Count(x => x.Type == Definitions.Enums.LibraryType.Uplay && string.Equals(x.FullPath, newLibraryPath, StringComparison.InvariantCultureIgnoreCase)) > 0;
            }
            // In any error return true to prevent possible bugs
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Logger.Fatal(ex);
                return true;
            }
        }

        public static void ParseAppDetails(string name, DirectoryInfo installationDirectory, Definitions.UplayLibrary library, bool isCompressed = false)
        {
            try
            {
                var gameDetails = Definitions.List.UplayConfigurations.FirstOrDefault(x => x.Name == name);
                if (gameDetails != null)
                {
                    library.Apps.Add(new Definitions.UplayAppInfo(library, gameDetails.Name, gameDetails.SpaceId, installationDirectory, gameDetails.ThumbImage, isCompressed));
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                MessageBox.Show(ex.ToString());
            }
        }

        public static async void CheckForBackupUpdatesAsync()
        {
            try
            {
                if (Definitions.List.Libraries.Count(x => x.Type == Definitions.Enums.LibraryType.Uplay && x.DirectoryInfo.Exists) == 0)
                {
                    return;
                }

                var progressInformationMessage = await Main.FormAccessor.ShowProgressAsync(SLM.Translate(nameof(Properties.Resources.PleaseWait)), SLM.Translate(nameof(Properties.Resources.Steam_CheckForBackupUpdates))).ConfigureAwait(true);
                progressInformationMessage.SetIndeterminate();

                foreach (var currentLibrary in Definitions.List.Libraries.Where(x => x.Type == Definitions.Enums.LibraryType.Uplay && x.DirectoryInfo.Exists).ToList())
                {
                    if (currentLibrary.Apps.Count == 0)
                    {
                        continue;
                    }

                    foreach (var libraryToCheck in Definitions.List.Libraries.Where(x => x != currentLibrary && x.Type == Definitions.Enums.LibraryType.Uplay))
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