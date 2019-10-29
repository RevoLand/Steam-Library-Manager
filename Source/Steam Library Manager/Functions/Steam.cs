using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.IconPacks;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using Alphaleonis.Win32.Filesystem;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Steam_Library_Manager.Functions
{
    internal static class Steam
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static async void UpdateSteamInstallationPathAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(Properties.Settings.Default.steamInstallationPath) || !Directory.Exists(Properties.Settings.Default.steamInstallationPath))
                {
                    Properties.Settings.Default.steamInstallationPath = Registry.GetValue(Definitions.Global.Steam.RegistryKeyPath, "SteamPath", "").ToString().Replace('/', Path.DirectorySeparatorChar);

                    if (string.IsNullOrEmpty(Properties.Settings.Default.steamInstallationPath))
                    {
                        if (await Main.FormAccessor.ShowMessageAsync(SLM.Translate(nameof(Properties.Resources.Steam_NotInstalled)), SLM.Translate(Properties.Resources.Steam_NotInstalledMessage), MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(true) == MessageDialogResult.Affirmative)
                        {
                            var steamPathSelector = new OpenFileDialog()
                            {
                                Filter = "Steam Executable (Steam.exe)|Steam.exe"
                            };

                            if (steamPathSelector.ShowDialog() == true)
                            {
                                if (Directory.Exists(Path.GetDirectoryName(steamPathSelector.FileName)))
                                    Properties.Settings.Default.steamInstallationPath = Path.GetDirectoryName(steamPathSelector.FileName);
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(Properties.Settings.Default.steamInstallationPath))
                    {
                        Definitions.Global.Steam.VdfFilePath = Path.Combine(Properties.Settings.Default.steamInstallationPath, "config", "config.vdf");
                    }
                }
            }
            catch (NullReferenceException ex)
            {
                Logger.Error(ex);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }

        public static void PopulateLibraryCMenuItems()
        {
            #region Library Context Menu Item Definitions

            // Open library in explorer ({0})
            var menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.SteamLibrary_CMenu_Open)),
                Action = "Disk",
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.FolderOpen, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) },
                ShowToOffline = false
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Steam);
            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.SLM);
            Definitions.List.LibraryCMenuItems.Add(menuItem);

            // Separator
            menuItem = new Definitions.ContextMenuItem
            {
                IsSeparator = true,
                ShowToOffline = false
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Steam);
            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.SLM);
            Definitions.List.LibraryCMenuItems.Add(menuItem);

            // Remove library & files
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.SteamLibrary_CMenu_RemoveFromSteam)),
                Action = "deleteLibrary",
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.DeleteForever, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) },
                ShowToOffline = false
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Steam);
            Definitions.List.LibraryCMenuItems.Add(menuItem);

            // Delete games in library
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.SteamLibrary_CMenu_DeleteGames)),
                Action = "deleteLibrarySLM",
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.DeleteSweep, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) },
                ShowToOffline = false
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Steam);
            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.SLM);
            Definitions.List.LibraryCMenuItems.Add(menuItem);

            // Separator
            menuItem = new Definitions.ContextMenuItem
            {
                IsSeparator = true,
                ShowToNormal = false,
                ShowToOffline = false
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.SLM);
            Definitions.List.LibraryCMenuItems.Add(menuItem);

            // Remove from SLM
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.RemoveFromSlm)),
                Action = "RemoveFromList",
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.PlaylistRemove, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) }
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.SLM);
            Definitions.List.LibraryCMenuItems.Add(menuItem);

            #endregion Library Context Menu Item Definitions
        }

        public static void PopulateAppCMenuItems()
        {
            #region App Context Menu Item Definitions

            // Run
            var menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.SteamApp_CMenu_Play)),
                Action = "steam://run/{0}",
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.Play, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) },
                ShowToSteamBackup = false,
                ShowToCompressed = false
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Steam);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Separator
            menuItem = new Definitions.ContextMenuItem
            {
                ShowToCompressed = false,
                IsSeparator = true,
                ShowToSteamBackup = false
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Steam);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Compress
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.SteamApp_CMenu_Compress)),
                Action = "Compress",
                ShowToSteamBackup = false,
                Icon = new PackIconOcticons() { Kind = PackIconOcticonsKind.FileZip, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) }
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Steam);
            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.SLM);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Compact
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.Compact)),
                Action = "compact",
                ShowToCompressed = false,
                ShowToSteamBackup = false,
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.ArrowCollapse, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) }
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Steam);
            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.SLM);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Separator
            menuItem = new Definitions.ContextMenuItem
            {
                IsSeparator = true,
                ShowToSteamBackup = false
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Steam);
            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.SLM);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Show on disk
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.SteamApp_CMenu_DiskInfo)),
                Action = "Disk",
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.FolderOpen, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) }
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Steam);
            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.SLM);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // View ACF
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.SteamApp_CMenu_ViewACF)),
                Action = "acffile",
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.FileDocument, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) },
                ShowToCompressed = false
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Steam);
            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.SLM);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Separator
            menuItem = new Definitions.ContextMenuItem
            {
                ShowToCompressed = false,
                IsSeparator = true,
                ShowToSteamBackup = false
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Steam);
            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.SLM);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // View Store Page
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.SteamApp_CMenu_ViewStore)),
                Action = "steam://store/{0}",
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.Steam, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) }
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Steam);
            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.SLM);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Verify files
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.SteamApp_CMenu_VerifyFiles)),
                Action = "verify",
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.CheckAll, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) },
                ShowToCompressed = false
            };
            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Steam);
            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.SLM);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Google Search
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.SteamApp_CMenu_GoogleSearch)),
                Action = "google",
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.Google, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) }
            };
            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Steam);
            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.SLM);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Youtube Search
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.SteamApp_CMenu_YoutubeSearch)),
                Action = "youtube",
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.Youtube, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) }
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Steam);
            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.SLM);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Game hub
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.SteamApp_CMenu_GameHub)),
                Action = "steam://url/GameHub/{0}",
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.BookOpenPageVariant, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) }
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Steam);
            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.SLM);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Separator
            menuItem = new Definitions.ContextMenuItem
            {
                IsSeparator = true
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Steam);
            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.SLM);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Workshop
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.SteamApp_CMenu_Workshop)),
                Action = "steam://url/SteamWorkshopPage/{0}",
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.FormatListCheckbox, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) }
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Steam);
            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.SLM);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Subscribed Workshop Items
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.SteamApp_CMenu_SubscribedWorkshopItems)),
                Action = "https://steamcommunity.com/profiles/{1}/myworkshopfiles/?appid={0}&browsefilter=mysubscriptions&sortmethod=lastupdated",
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.PlaylistCheck, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) }
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Steam);
            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.SLM);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Separator
            menuItem = new Definitions.ContextMenuItem
            {
                IsSeparator = true,
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Steam);
            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.SLM);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Delete files (using Task Manager)
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.SteamApp_CMenu_DeleteFilesSLM)),
                Action = "deleteappfiles",
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.Delete, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) }
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Steam);
            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.SLM);
            Definitions.List.AppCMenuItems.Add(menuItem);

            // Delete files (using Task Manager)
            menuItem = new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.SteamApp_CMenu_DeleteFilesTM)),
                Action = "deleteappfilestm",
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.DeleteSweep, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Foreground = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"]) }
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Steam);
            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.SLM);
            Definitions.List.AppCMenuItems.Add(menuItem);

            #endregion App Context Menu Item Definitions
        }

        public static string GetActiveSteamProcessPath()
        {
            try
            {
                var activeSteamProcess = Process.GetProcessesByName("Steam").FirstOrDefault();
                return activeSteamProcess?.MainModule?.FileName;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);

                return null;
            }
        }

        public static async Task CloseSteamAsync()
        {
            try
            {
                var activeSteamPath = GetActiveSteamProcessPath();
                if (!string.IsNullOrEmpty(activeSteamPath))
                {
                    if (await Main.FormAccessor.ShowMessageAsync(SLM.Translate(nameof(Properties.Resources.Steam_NeedsToBeClosed)), SLM.Translate(nameof(Properties.Resources.Steam_NeedsToBeClosedMessage)), MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(true) == MessageDialogResult.Affirmative)
                    {
                        if (File.Exists(activeSteamPath))
                        {
                            Process.Start(activeSteamPath, "-shutdown");
                        }
                        else if (await Main.FormAccessor.ShowMessageAsync(SLM.Translate(nameof(Properties.Resources.Steam_NeedsToBeClosed)), Framework.StringFormat.Format(SLM.Translate(nameof(Properties.Resources.Steam_NeedsToBeClosedMessage2)), new { ActiveSteamPath = activeSteamPath }), MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(true) == MessageDialogResult.Affirmative)
                        {
                            foreach (var steamProcess in Process.GetProcessesByName("Steam"))
                            {
                                steamProcess.Kill();
                            }
                        }
                        else
                        {
                            throw new OperationCanceledException(SLM.Translate(nameof(Properties.Resources.Steam_NeedsToBeClosed_NotFoundAndUserCancelled)));
                        }
                    }
                    else
                    {
                        throw new OperationCanceledException(SLM.Translate(nameof(Properties.Resources.Steam_NeedsToBeClosed_UserCancelled)));
                    }

                    await Task.Delay(6000).ConfigureAwait(true);
                }
            }
            catch (OperationCanceledException)
            { }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }

        public static async void RestartSteamAsync()
        {
            try
            {
                await Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                {
                    if (await Main.FormAccessor.ShowMessageAsync(SLM.Translate(nameof(Properties.Resources.Steam_Start)), SLM.Translate(nameof(Properties.Resources.Steam_StartMessage)), MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(true) == MessageDialogResult.Affirmative)
                    {
                        await CloseSteamAsync().ConfigureAwait(true);

                        if (File.Exists(Path.Combine(Properties.Settings.Default.steamInstallationPath, "steam.exe")))
                        {
                            Process.Start(Path.Combine(Properties.Settings.Default.steamInstallationPath, "steam.exe"), "-silent");
                        }
                    }
                    else
                    {
                        throw new OperationCanceledException(SLM.Translate(nameof(Properties.Resources.Steam_Start_UserCancelled)));
                    }
                }, System.Windows.Threading.DispatcherPriority.Normal).ConfigureAwait(true);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }

        public static class Library
        {
            public static async void CreateNew(string NewLibraryPath, bool Backup)
            {
                try
                {
                    if (string.IsNullOrEmpty(NewLibraryPath))
                        return;

                    // If we are not creating a backup library
                    if (!Backup)
                    {
                        await CloseSteamAsync().ConfigureAwait(true);

                        // Define steam dll paths for better looking
                        string SteamDLLPath = Path.Combine(NewLibraryPath, "Steam.dll");

                        if (!File.Exists(SteamDLLPath))
                        {
                            // Copy Steam.dll as steam needs it
                            File.Copy(Path.Combine(Properties.Settings.Default.steamInstallationPath, "Steam.dll"), SteamDLLPath, true);
                        }

                        if (!Directory.Exists(Path.Combine(NewLibraryPath, "SteamApps")))
                        {
                            // create SteamApps directory at requested directory
                            Directory.CreateDirectory(Path.Combine(NewLibraryPath, "SteamApps"));
                        }

                        // If Steam.dll moved succesfully
                        if (File.Exists(SteamDLLPath)) // in case of permissions denied
                        {
                            // Call KeyValue in act
                            Framework.KeyValue Key = new Framework.KeyValue();

                            // Read vdf file as text
                            Key.ReadFileAsText(Definitions.Global.Steam.VdfFilePath);

                            // Add our new library to vdf file so steam will know we have a new library
                            Key["Software"]["Valve"]["Steam"].Children.Add(new Framework.KeyValue(string.Format("BaseInstallFolder_{0}", Definitions.List.Libraries.Select(x => x.Type == Definitions.Enums.LibraryType.Steam).Count()), NewLibraryPath));

                            // Save vdf file
                            Key.SaveToFile(Definitions.Global.Steam.VdfFilePath, false);

                            // Show a messagebox to user about process
                            MessageBox.Show(SLM.Translate(nameof(Properties.Resources.CreateSteamLibrary_Created)));

                            // Since this file started to interrupt us?
                            // No need to bother with it since config.vdf is the real deal, just remove it and Steam client will handle.
                            if (File.Exists(Path.Combine(Properties.Settings.Default.steamInstallationPath, "steamapps", "libraryfolders.vdf")))
                            {
                                File.Delete(Path.Combine(Properties.Settings.Default.steamInstallationPath, "steamapps", "libraryfolders.vdf"));
                            }

                            RestartSteamAsync();
                        }
                        else
                        {
                            // Show an error to user and cancel the process because we couldn't get Steam.dll in new library dir
                            MessageBox.Show(SLM.Translate(nameof(Properties.Resources.CreateSteamLibrary_UnknownError)));
                        }
                    }

                    // Add library to list
                    AddNew(NewLibraryPath);

                    // Save our settings
                    SLM.Settings.SaveSettings();
                }
                catch (UnauthorizedAccessException ex)
                {
                    await Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                     {
                         await Main.FormAccessor.ShowMessageAsync(SLM.Translate(nameof(Properties.Resources.CreateSteamLibrary_UnauthorizedAccessException)), Framework.StringFormat.Format(SLM.Translate(nameof(Properties.Resources.CreateSteamLibrary_UnauthorizedAccessExceptionMessage)), new { NewLibraryPath, ExceptionMessage = ex.Message }), MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(true);
                     }, System.Windows.Threading.DispatcherPriority.Normal).ConfigureAwait(true);

                    Logger.Fatal(ex);
                }
                catch (Exception ex)
                {
                    Logger.Fatal(ex);
                }
            }

            public static async void CheckForBackupUpdatesAsync()
            {
                try
                {
                    if (Definitions.List.Libraries.Count(x => x.Type == Definitions.Enums.LibraryType.SLM && x.DirectoryInfo.Exists) == 0)
                    {
                        return;
                    }

                    var progressInformationMessage = await Main.FormAccessor.ShowProgressAsync(SLM.Translate(nameof(Properties.Resources.PleaseWait)), SLM.Translate(nameof(Properties.Resources.Steam_CheckForBackupUpdates))).ConfigureAwait(true);
                    progressInformationMessage.SetIndeterminate();

                    foreach (var currentLibrary in Definitions.List.Libraries.Where(x => x.Type == Definitions.Enums.LibraryType.SLM && x.DirectoryInfo.Exists).ToList())
                    {
                        if (currentLibrary.Apps.Count == 0)
                        {
                            continue;
                        }

                        foreach (var libraryToCheck in Definitions.List.Libraries.Where(x => x.Type == Definitions.Enums.LibraryType.Steam))
                        {
                            foreach (var currentApp in currentLibrary.Apps.Where(x => !x.IsSteamBackup && !x.IsCompressed).ToList())
                            {
                                progressInformationMessage.SetMessage(Framework.StringFormat.Format(SLM.Translate(nameof(Properties.Resources.Steam_CheckForBackupUpdates_Progress)), new { CurrentAppName = currentApp.AppName }));

                                if (libraryToCheck.Apps.Count(x => x.AppId == currentApp.AppId && x.LastUpdated > currentApp.LastUpdated && !x.IsSteamBackup) > 0)
                                {
                                    var latestApp = libraryToCheck.Apps.First(x => x.AppId == currentApp.AppId && x.LastUpdated > currentApp.LastUpdated && !x.IsSteamBackup);

                                    if (TaskManager.TaskList.Count(x => x.App.AppId == currentApp.AppId && !x.Completed && (x.TargetLibrary == latestApp.Library || x.TargetLibrary == currentApp.Library)) == 0)
                                    {
                                        var newTask = new Definitions.List.TaskInfo
                                        {
                                            App = latestApp,
                                            TargetLibrary = currentApp.Library,
                                            TaskType = (currentApp.IsCompressed) ? Definitions.Enums.TaskType.Compress : Definitions.Enums.TaskType.Copy
                                        };

                                        TaskManager.AddTask(newTask);
                                        Main.FormAccessor.TmLogs.Report(Framework.StringFormat.Format(SLM.Translate(nameof(Properties.Resources.Steam_CheckForBackupUpdates_UpdateFound)), new { CurrentTime = DateTime.Now, CurrentAppName = currentApp.AppName, NewAppLastUpdatedOn = latestApp.LastUpdated, CurrentAppLastUpdatedOn = currentApp.LastUpdated, CurrentAppSteamFullPath = currentApp.Library.FullPath, NewAppSteamFullPath = latestApp.Library.FullPath }));
                                    }
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

            public static async void AddNew(string libraryPath, bool isMainLibrary = false)
            {
                try
                {
                    if (!libraryPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    {
                        libraryPath += Path.DirectorySeparatorChar;
                    }

                    var newLibrary = new Definitions.SteamLibrary(libraryPath, isMainLibrary)
                    {
                        Type = Definitions.Enums.LibraryType.Steam,
                        DirectoryInfo = new DirectoryInfo(libraryPath)
                    };

                    Definitions.List.LibraryProgress.Report(newLibrary);

                    await Task.Run(newLibrary.UpdateAppList).ConfigureAwait(true);
                    await Task.Run(newLibrary.UpdateJunks).ConfigureAwait(true);
                }
                catch (Exception ex)
                {
                    Logger.Fatal(ex);
                }
            }

            public static void GenerateLibraryList()
            {
                try
                {
                    if (!string.IsNullOrEmpty(Properties.Settings.Default.SteamID64))
                    {
                        var localConfigFilePath = Path.Combine(Properties.Settings.Default.steamInstallationPath, "userdata", Framework.SteamIDConvert.Steam64ToSteam32(Convert.ToInt64(Properties.Settings.Default.SteamID64)).Split(':').Last(), "config", "localconfig.vdf");
                        if (File.Exists(localConfigFilePath))
                        {
                            var configFile = new Framework.KeyValue();
                            configFile.ReadFileAsText(localConfigFilePath);

                            var appsPath = configFile["Software"]["Valve"]["Steam"]["apps"];

                            if (appsPath?.Children.Count > 0)
                            {
                                foreach (var app in appsPath.Children)
                                {
                                    var lastPlayed = app["LastPlayed"].Value;
                                    if (lastPlayed != null && !Definitions.List.SteamAppsLastPlayedDic.ContainsKey(Convert.ToInt32(app.Name)))
                                    {
                                        Definitions.List.SteamAppsLastPlayedDic.Add(Convert.ToInt32(app.Name), new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Convert.ToInt64(lastPlayed)));
                                    }
                                }
                            }
                        }
                    }

                    if (File.Exists(Path.Combine(Properties.Settings.Default.steamInstallationPath, "Steam.exe")))
                    {
                        AddNew(Properties.Settings.Default.steamInstallationPath, true);
                    }

                    // If config.vdf exists
                    if (File.Exists(Definitions.Global.Steam.VdfFilePath))
                    {
                        // Make a KeyValue reader
                        var keyValReader = new Framework.KeyValue();

                        // Read our vdf file as text
                        keyValReader.ReadFileAsText(Definitions.Global.Steam.VdfFilePath);

                        keyValReader = keyValReader["Software"]["Valve"]["Steam"];
                        if (keyValReader?.Children.Count > 0)
                        {
                            foreach (var key in keyValReader.Children.Where(x => x.Name.StartsWith("BaseInstallFolder", StringComparison.OrdinalIgnoreCase)))
                            {
                                AddNew(key.Value);
                            }

                            if (keyValReader["Accounts"]?.Children.Count > 0)
                            {
                                foreach (var account in keyValReader["Accounts"].Children)
                                {
                                    var steamId = account.Children.SingleOrDefault(x => x.Name == "SteamID");
                                    if (steamId == null)
                                    {
                                        continue;
                                    }

                                    Definitions.List.SteamUserIdList.Add(new Tuple<string, string>(account.Name, steamId.Value));
                                }

                                Main.FormAccessor.SettingsView.SteamUserIDList.SelectedItem = Definitions.List.SteamUserIdList.Find(x => x.Item2 == Properties.Settings.Default.SteamID64);
                            }
                        }
                    }
                    else
                    {
                        Logger.Warn($"Couldn't locate config.vdf for Steam at: {Definitions.Global.Steam.VdfFilePath}");
                    }
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

                    if (!newLibraryPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    {
                        newLibraryPath += Path.DirectorySeparatorChar;
                    }

                    return Definitions.List.Libraries.Any(x =>
                     x.Type == Definitions.Enums.LibraryType.Steam
                     && (x.FullPath.ToLowerInvariant() == newLibraryPath
                     || x.DirectoryList["Common"].FullName.ToLowerInvariant() == newLibraryPath
                     || x.DirectoryList["Download"].FullName.ToLowerInvariant() == newLibraryPath
                     || x.DirectoryList["Workshop"].FullName.ToLowerInvariant() == newLibraryPath
                     || x.DirectoryList["SteamApps"].FullName.ToLowerInvariant() == newLibraryPath)
                    );
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
}