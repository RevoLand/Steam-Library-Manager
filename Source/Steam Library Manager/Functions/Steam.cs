using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Steam_Library_Manager.Functions
{
    internal static class Steam
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static async void UpdateSteamInstallationPathAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(Properties.Settings.Default.steamInstallationPath) || !Directory.Exists(Properties.Settings.Default.steamInstallationPath))
                {
                    Properties.Settings.Default.steamInstallationPath = Registry.GetValue(Definitions.Global.Steam.RegistryKeyPath, "SteamPath", "").ToString().Replace('/', Path.DirectorySeparatorChar);

                    if (string.IsNullOrEmpty(Properties.Settings.Default.steamInstallationPath))
                    {
                        if (await Main.FormAccessor.ShowMessageAsync(SLM.Translate(nameof(Properties.Resources.Steam_NotInstalled)), SLM.Translate(Properties.Resources.Steam_NotInstalledMessage), MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(false) == MessageDialogResult.Affirmative)
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
                        Definitions.Global.Steam.vdfFilePath = Path.Combine(Properties.Settings.Default.steamInstallationPath, "config", "config.vdf");
                    }
                }
            }
            catch (NullReferenceException ex)
            {
                logger.Error(ex);
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
            }
        }

        public static void PopulateLibraryCMenuItems()
        {
            #region Library Context Menu Item Definitions

            // Open library in explorer ({0})
            Definitions.List.LibraryCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.SteamLibrary_CMenu_Open)),
                Action = "Disk",
                Icon = FontAwesome.WPF.FontAwesomeIcon.FolderOpen,
                LibraryType = Definitions.Enums.LibraryType.Steam,
                ShowToOffline = false
            });

            // Separator
            Definitions.List.LibraryCMenuItems.Add(new Definitions.ContextMenuItem
            {
                IsSeparator = true,
                LibraryType = Definitions.Enums.LibraryType.Steam,
                ShowToOffline = false
            });

            // Remove library & files
            Definitions.List.LibraryCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.SteamLibrary_CMenu_RemoveFromSteam)),
                Action = "deleteLibrary",
                Icon = FontAwesome.WPF.FontAwesomeIcon.Trash,
                LibraryType = Definitions.Enums.LibraryType.Steam,
                ShowToOffline = false
            });

            // Delete games in library
            Definitions.List.LibraryCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.SteamLibrary_CMenu_DeleteGames)),
                Action = "deleteLibrarySLM",
                Icon = FontAwesome.WPF.FontAwesomeIcon.TrashOutline,
                LibraryType = Definitions.Enums.LibraryType.Steam,
                ShowToOffline = false
            });

            // Separator
            Definitions.List.LibraryCMenuItems.Add(new Definitions.ContextMenuItem
            {
                IsSeparator = true,
                ShowToNormal = false,
                LibraryType = Definitions.Enums.LibraryType.SLM,
                ShowToOffline = false,
                ShowToSLMBackup = true
            });

            // Remove from SLM
            Definitions.List.LibraryCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.SteamLibrary_CMenu_RemoveFromSLM)),
                Action = "RemoveFromList",
                Icon = FontAwesome.WPF.FontAwesomeIcon.Minus,
                LibraryType = Definitions.Enums.LibraryType.SLM,
                ShowToNormal = false,
                ShowToSLMBackup = true
            });

            #endregion Library Context Menu Item Definitions
        }

        public static void PopulateAppCMenuItems()
        {
            #region App Context Menu Item Definitions

            // Run
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.SteamApp_CMenu_Play)),
                Action = "steam://run/{0}",
                Icon = FontAwesome.WPF.FontAwesomeIcon.Play,
                LibraryType = Definitions.Enums.LibraryType.Steam,
                ShowToSteamBackup = false,
                ShowToCompressed = false
            });

            // Separator
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                ShowToCompressed = false,
                LibraryType = Definitions.Enums.LibraryType.Steam,
                IsSeparator = true,
                ShowToSteamBackup = false
            });

            // Compress
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.SteamApp_CMenu_Compress)),
                Action = "Compress",
                LibraryType = Definitions.Enums.LibraryType.Steam,
                ShowToCompressed = true,
                ShowToSteamBackup = false,
                Icon = FontAwesome.WPF.FontAwesomeIcon.FileZipOutline
            });

            // Compact
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = "Compact",
                Action = "compact",
                LibraryType = Definitions.Enums.LibraryType.Steam,
                ShowToCompressed = false,
                ShowToSteamBackup = false,
                Icon = FontAwesome.WPF.FontAwesomeIcon.FileArchiveOutline
            });

            // Separator
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                ShowToCompressed = false,
                LibraryType = Definitions.Enums.LibraryType.Steam,
                IsSeparator = true,
                ShowToSteamBackup = false
            });

            // Show on disk
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.SteamApp_CMenu_DiskInfo)),
                Action = "Disk",
                LibraryType = Definitions.Enums.LibraryType.Steam,
                ShowToCompressed = true,
                Icon = FontAwesome.WPF.FontAwesomeIcon.FolderOpen
            });

            // View ACF
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.SteamApp_CMenu_ViewACF)),
                Action = "acffile",
                Icon = FontAwesome.WPF.FontAwesomeIcon.PencilSquareOutline,
                LibraryType = Definitions.Enums.LibraryType.Steam,
                ShowToCompressed = false
            });

            // Game hub
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.SteamApp_CMenu_GameHub)),
                Action = "steam://url/GameHub/{0}",
                LibraryType = Definitions.Enums.LibraryType.Steam,
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
                Header = SLM.Translate(nameof(Properties.Resources.SteamApp_CMenu_Workshop)),
                Action = "steam://url/SteamWorkshopPage/{0}",
                LibraryType = Definitions.Enums.LibraryType.Steam,
                Icon = FontAwesome.WPF.FontAwesomeIcon.Cog
            });

            // Subscribed Workshop Items
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.SteamApp_CMenu_SubscribedWorkshopItems)),
                Action = "https://steamcommunity.com/profiles/{1}/myworkshopfiles/?appid={0}&browsefilter=mysubscriptions&sortmethod=lastupdated",
                LibraryType = Definitions.Enums.LibraryType.Steam,
                Icon = FontAwesome.WPF.FontAwesomeIcon.Cogs
            });

            // Separator
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                IsSeparator = true,
                LibraryType = Definitions.Enums.LibraryType.Steam
            });

            // Delete files (using Task Manager)
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.SteamApp_CMenu_DeleteFilesSLM)),
                Action = "deleteappfiles",
                LibraryType = Definitions.Enums.LibraryType.Steam,
                Icon = FontAwesome.WPF.FontAwesomeIcon.TrashOutline
            });

            // Delete files (using Task Manager)
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = SLM.Translate(nameof(Properties.Resources.SteamApp_CMenu_DeleteFilesTM)),
                Action = "deleteappfilestm",
                LibraryType = Definitions.Enums.LibraryType.Steam,
                Icon = FontAwesome.WPF.FontAwesomeIcon.Trash
            });

            #endregion App Context Menu Item Definitions
        }

        public static string GetActiveSteamProcessPath()
        {
            try
            {
                var activeSteamProcess = Process.GetProcessesByName("Steam").FirstOrDefault();
                return activeSteamProcess?.MainModule.FileName;
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);

                return null;
            }
        }

        public static async Task CloseSteamAsync()
        {
            try
            {
                var ActiveSteamPath = GetActiveSteamProcessPath();
                if (!string.IsNullOrEmpty(ActiveSteamPath))
                {
                    if (await Main.FormAccessor.ShowMessageAsync(SLM.Translate(nameof(Properties.Resources.Steam_NeedsToBeClosed)), SLM.Translate(nameof(Properties.Resources.Steam_NeedsToBeClosedMessage)), MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(false) == MessageDialogResult.Affirmative)
                    {
                        if (File.Exists(ActiveSteamPath))
                        {
                            Process.Start(ActiveSteamPath, "-shutdown");
                        }
                        else if (await Main.FormAccessor.ShowMessageAsync(SLM.Translate(nameof(Properties.Resources.Steam_NeedsToBeClosed)), Framework.StringFormat.Format(SLM.Translate(nameof(Properties.Resources.Steam_NeedsToBeClosedMessage2)), new { ActiveSteamPath }), MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(false) == MessageDialogResult.Affirmative)
                        {
                            foreach (var SteamProcess in Process.GetProcessesByName("Steam"))
                            {
                                SteamProcess.Kill();
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

                    await Task.Delay(6000).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            { }
            catch (Exception ex)
            {
                logger.Fatal(ex);
            }
        }

        public static async void RestartSteamAsync()
        {
            try
            {
                await Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                {
                    if (await Main.FormAccessor.ShowMessageAsync(SLM.Translate(nameof(Properties.Resources.Steam_Start)), SLM.Translate(nameof(Properties.Resources.Steam_StartMessage)), MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(false) == MessageDialogResult.Affirmative)
                    {
                        await CloseSteamAsync().ConfigureAwait(false);

                        if (File.Exists(Path.Combine(Properties.Settings.Default.steamInstallationPath, "steam.exe")))
                        {
                            Process.Start(Path.Combine(Properties.Settings.Default.steamInstallationPath, "steam.exe"), "-silent");
                        }
                    }
                    else
                    {
                        throw new OperationCanceledException(SLM.Translate(nameof(Properties.Resources.Steam_Start_UserCancelled)));
                    }
                }, System.Windows.Threading.DispatcherPriority.Normal).ConfigureAwait(false);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                logger.Fatal(ex);
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
                        await CloseSteamAsync().ConfigureAwait(false);

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
                            Key.ReadFileAsText(Definitions.Global.Steam.vdfFilePath);

                            // Add our new library to vdf file so steam will know we have a new library
                            Key["Software"]["Valve"]["Steam"].Children.Add(new Framework.KeyValue(string.Format("BaseInstallFolder_{0}", Definitions.List.Libraries.Select(x => x.Type == Definitions.Enums.LibraryType.Steam).Count()), NewLibraryPath));

                            // Save vdf file
                            Key.SaveToFile(Definitions.Global.Steam.vdfFilePath, false);

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
                         await Main.FormAccessor.ShowMessageAsync(SLM.Translate(nameof(Properties.Resources.CreateSteamLibrary_UnauthorizedAccessException)), Framework.StringFormat.Format(SLM.Translate(nameof(Properties.Resources.CreateSteamLibrary_UnauthorizedAccessExceptionMessage)), new { NewLibraryPath, ExceptionMessage = ex.Message }), MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(false);
                     }, System.Windows.Threading.DispatcherPriority.Normal).ConfigureAwait(false);

                    logger.Fatal(ex);
                }
                catch (Exception ex)
                {
                    logger.Fatal(ex);
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

                    var ProgressInformationMessage = await Main.FormAccessor.ShowProgressAsync(SLM.Translate(nameof(Properties.Resources.PleaseWait)), SLM.Translate(nameof(Properties.Resources.Steam_CheckForBackupUpdates))).ConfigureAwait(false);
                    ProgressInformationMessage.SetIndeterminate();

                    foreach (var CurrentLibrary in Definitions.List.Libraries.Where(x => x.Type == Definitions.Enums.LibraryType.SLM && x.DirectoryInfo.Exists).ToList())
                    {
                        if (CurrentLibrary.Steam.Apps.Count == 0)
                        {
                            continue;
                        }

                        foreach (var LibraryToCheck in Definitions.List.Libraries.Where(x => x.Type == Definitions.Enums.LibraryType.Steam))
                        {
                            foreach (var CurrentApp in CurrentLibrary.Steam.Apps.Where(x => !x.IsSteamBackup && !x.IsCompressed).ToList())
                            {
                                ProgressInformationMessage.SetMessage(Framework.StringFormat.Format(SLM.Translate(nameof(Properties.Resources.Steam_CheckForBackupUpdates_Progress)), new { CurrentAppName = CurrentApp.AppName }));

                                if (LibraryToCheck.Steam.Apps.Count(x => x.AppID == CurrentApp.AppID && x.LastUpdated > CurrentApp.LastUpdated && !x.IsSteamBackup) > 0)
                                {
                                    var LatestApp = LibraryToCheck.Steam.Apps.First(x => x.AppID == CurrentApp.AppID && x.LastUpdated > CurrentApp.LastUpdated && !x.IsSteamBackup);

                                    if (Functions.TaskManager.TaskList.Count(x => x.SteamApp.AppID == CurrentApp.AppID && !x.Completed && (x.TargetLibrary == LatestApp.Library || x.TargetLibrary == CurrentApp.Library)) == 0)
                                    {
                                        Definitions.List.TaskInfo NewTask = new Definitions.List.TaskInfo
                                        {
                                            SteamApp = LatestApp,
                                            TargetLibrary = CurrentApp.Library
                                        };

                                        Functions.TaskManager.AddTask(NewTask);
                                        Main.FormAccessor.TmLogs.Report(Framework.StringFormat.Format(SLM.Translate(nameof(Properties.Resources.Steam_CheckForBackupUpdates_UpdateFound)), new { CurrentTime = DateTime.Now, CurrentAppName = CurrentApp.AppName, NewAppLastUpdatedOn = LatestApp.LastUpdated, CurrentAppLastUpdatedOn = CurrentApp.LastUpdated, CurrentAppSteamFullPath = CurrentApp.Library.Steam.FullPath, NewAppSteamFullPath = LatestApp.Library.Steam.FullPath }));
                                    }
                                }
                            }
                        }
                    }

                    await ProgressInformationMessage.CloseAsync().ConfigureAwait(false);
                    Main.FormAccessor.TmLogs.Report(Framework.StringFormat.Format(SLM.Translate(nameof(Properties.Resources.Steam_CheckForBackupUpdates_Completed)), new { CurrentTime = DateTime.Now }));
                }
                catch (Exception ex)
                {
                    logger.Fatal(ex);
                }
            }

            public static async void AddNew(string LibraryPath, bool IsMainLibrary = false)
            {
                try
                {
                    var newLibrary = new Definitions.Library
                    {
                        Type = Definitions.Enums.LibraryType.Steam,
                        DirectoryInfo = new DirectoryInfo(LibraryPath)
                    };

                    newLibrary.Steam = new Definitions.SteamLibrary(LibraryPath, newLibrary, IsMainLibrary);

                    Definitions.List.Libraries.Add(newLibrary);

                    await Task.Run(() => newLibrary.Steam.UpdateAppListAsync()).ConfigureAwait(false);
                    await Task.Run(() => newLibrary.Steam.UpdateJunks()).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.Fatal(ex);
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
                            Framework.KeyValue configFile = new Framework.KeyValue();
                            configFile.ReadFileAsText(localConfigFilePath);

                            var appsPath = configFile["Software"]["Valve"]["Steam"]["apps"];

                            if (appsPath?.Children.Count > 0)
                            {
                                foreach (var app in appsPath.Children)
                                {
                                    var lastPlayed = app["LastPlayed"].Value;
                                    if (lastPlayed != null && !Definitions.List.SteamApps_LastPlayedDic.ContainsKey(Convert.ToInt32(app.Name)))
                                    {
                                        Definitions.List.SteamApps_LastPlayedDic.Add(Convert.ToInt32(app.Name), new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Convert.ToInt64(lastPlayed)));
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
                    if (File.Exists(Definitions.Global.Steam.vdfFilePath))
                    {
                        // Make a KeyValue reader
                        Framework.KeyValue KeyValReader = new Framework.KeyValue();

                        // Read our vdf file as text
                        KeyValReader.ReadFileAsText(Definitions.Global.Steam.vdfFilePath);

                        KeyValReader = KeyValReader["Software"]["Valve"]["Steam"];
                        if (KeyValReader?.Children.Count > 0)
                        {
                            foreach (var key in KeyValReader.Children.Where(x => x.Name.StartsWith("BaseInstallFolder", StringComparison.OrdinalIgnoreCase)))
                            {
                                AddNew(key.Value);
                            }

                            if (KeyValReader["Accounts"]?.Children.Count > 0)
                            {
                                foreach (var account in KeyValReader["Accounts"].Children)
                                {
                                    var steamID = account.Children.SingleOrDefault(x => x.Name == "SteamID");
                                    if (steamID == null)
                                    {
                                        continue;
                                    }

                                    Definitions.List.SteamUserIDList.Add(new Tuple<string, string>(account.Name, steamID.Value));
                                }

                                Main.FormAccessor.SettingsView.SteamUserIDList.SelectedItem = Definitions.List.SteamUserIDList.Find(x => x.Item2 == Properties.Settings.Default.SteamID64);
                            }
                        }
                    }
                    else { /* Could not locate LibraryFolders.vdf */ }
                }
                catch (Exception ex)
                {
                    logger.Fatal(ex);
                }
            }

            public static bool IsLibraryExists(string NewLibraryPath)
            {
                try
                {
                    NewLibraryPath = NewLibraryPath.ToLowerInvariant();

                    return Definitions.List.Libraries.Any(x =>
                     x.Type == Definitions.Enums.LibraryType.Steam
                     && (x.Steam.FullPath.ToLowerInvariant() == NewLibraryPath
                     || x.Steam.CommonFolder.FullName.ToLowerInvariant() == NewLibraryPath
                     || x.Steam.DownloadFolder.FullName.ToLowerInvariant() == NewLibraryPath
                     || x.Steam.WorkshopFolder.FullName.ToLowerInvariant() == NewLibraryPath
                     || x.Steam.SteamAppsFolder.FullName.ToLowerInvariant() == NewLibraryPath)
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