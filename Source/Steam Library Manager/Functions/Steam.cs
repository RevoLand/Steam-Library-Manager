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
                        if (await Main.FormAccessor.ShowMessageAsync("Steam installation couldn't be found", "Steam couldn't be found under registry. Would you like to locate Steam manually?", MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Affirmative)
                        {
                            OpenFileDialog SteamPathSelector = new OpenFileDialog()
                            {
                                Filter = "Steam (Steam.exe)|Steam.exe"
                            };

                            if (SteamPathSelector.ShowDialog() == true)
                            {
                                if (Directory.Exists(Path.GetDirectoryName(SteamPathSelector.FileName)))
                                    Properties.Settings.Default.steamInstallationPath = Path.GetDirectoryName(SteamPathSelector.FileName);
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
                Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
            }
        }

        public static void PopulateLibraryCMenuItems()
        {
            #region Library Context Menu Item Definitions

            // Open library in explorer ({0})
            Definitions.List.LibraryCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = "Open library in explorer ({0})",
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
                Header = "Remove library from Steam (/w files)",
                Action = "deleteLibrary",
                Icon = FontAwesome.WPF.FontAwesomeIcon.Trash,
                LibraryType = Definitions.Enums.LibraryType.Steam,
                ShowToOffline = false
            });

            // Delete games in library
            Definitions.List.LibraryCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = "Delete games in library",
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
                Header = "Remove from SLM",
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
                Header = "Run",
                Action = "steam://run/{0}",
                Icon = FontAwesome.WPF.FontAwesomeIcon.Play,
                LibraryType = Definitions.Enums.LibraryType.Steam,
                ShowToCompressed = false
            });

            // Separator
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                ShowToCompressed = false,
                LibraryType = Definitions.Enums.LibraryType.Steam,
                IsSeparator = true
            });

            // Show on disk
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = "{0} ({1})",
                Action = "Disk",
                LibraryType = Definitions.Enums.LibraryType.Steam,
                Icon = FontAwesome.WPF.FontAwesomeIcon.FolderOpen
            });

            // View ACF
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = "View ACF File",
                Action = "acffile",
                Icon = FontAwesome.WPF.FontAwesomeIcon.PencilSquareOutline,
                LibraryType = Definitions.Enums.LibraryType.Steam,
                ShowToCompressed = false
            });

            // Game hub
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = "Game Hub",
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
                Header = "Workshop",
                Action = "steam://url/SteamWorkshopPage/{0}",
                LibraryType = Definitions.Enums.LibraryType.Steam,
                Icon = FontAwesome.WPF.FontAwesomeIcon.Cog
            });

            // Subscribed Workshop Items
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = "Subscribed Workshop Items",
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
                Header = "Delete files (using SLM)",
                Action = "deleteappfiles",
                LibraryType = Definitions.Enums.LibraryType.Steam,
                Icon = FontAwesome.WPF.FontAwesomeIcon.TrashOutline
            });

            // Delete files (using Task Manager)
            Definitions.List.AppCMenuItems.Add(new Definitions.ContextMenuItem
            {
                Header = "Delete files (using TaskManager)",
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
                var test = Process.GetProcessesByName("Steam").FirstOrDefault();
                return test?.MainModule.FileName;
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
                Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
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
                    if (await Main.FormAccessor.ShowMessageAsync("Steam needs to be closed", "Steam needs to be closed for this action. Would you like SLM to close Steam?", MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Affirmative)
                    {
                        if (File.Exists(ActiveSteamPath))
                        {
                            Process.Start(ActiveSteamPath, "-shutdown");
                        }
                        else if (await Main.FormAccessor.ShowMessageAsync("Steam needs to be closed", $"Steam.exe could not found (even it is already working?), SLM can try to terminate the Steam processes now if you want to.\n\nActive Steam process path: {ActiveSteamPath}", MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Affirmative)
                        {
                            foreach (var SteamProcess in Process.GetProcessesByName("Steam"))
                            {
                                SteamProcess.Kill();
                            }
                        }
                        else
                        {
                            throw new OperationCanceledException("Steam.exe could not found and user doesn't wants to terminate the process.");
                        }
                    }
                    else
                    {
                        throw new OperationCanceledException("User doesn't wants to close Steam, can not continue to process.");
                    }

                    await Task.Delay(6000);
                }
            }
            catch (OperationCanceledException)
            { }
            catch (Exception ex)
            {
                logger.Fatal(ex);
                Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
            }
        }

        public static async void RestartSteamAsync()
        {
            try
            {
                await Main.FormAccessor.AppView.AppPanel.Dispatcher.Invoke(async delegate
                {
                    if (await Main.FormAccessor.ShowMessageAsync("Restart Steam?", "Would you like SLM to Restart Steam?", MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Affirmative)
                    {
                        await CloseSteamAsync();

                        if (File.Exists(Path.Combine(Properties.Settings.Default.steamInstallationPath, "steam.exe")))
                        {
                            Process.Start(Path.Combine(Properties.Settings.Default.steamInstallationPath, "steam.exe"), "-silent");
                        }
                    }
                    else
                    {
                        throw new OperationCanceledException("User doesn't wants to restart Steam.");
                    }
                }, System.Windows.Threading.DispatcherPriority.Normal);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                logger.Fatal(ex);
                Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
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
                        await CloseSteamAsync();

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
                            MessageBox.Show("New Steam library created");

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
                            MessageBox.Show("Failed to copy Steam.dll into new library directory. Permission error, maybe?");
                        }
                    }

                    // Add library to list
                    AddNew(NewLibraryPath);

                    // Save our settings
                    SLM.Settings.SaveSettings();
                }
                catch (Exception ex)
                {
                    logger.Fatal(ex);
                    Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
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

                    var ProgressInformationMessage = await Main.FormAccessor.ShowProgressAsync("Please wait...", "Checking for backup updates as you have requested.");
                    ProgressInformationMessage.SetIndeterminate();

                    foreach (Definitions.Library CurrentLibrary in Definitions.List.Libraries.Where(x => x.Type == Definitions.Enums.LibraryType.SLM && x.DirectoryInfo.Exists))
                    {
                        if (CurrentLibrary.Steam.Apps.Count == 0)
                        {
                            continue;
                        }

                        foreach (Definitions.Library LibraryToCheck in Definitions.List.Libraries.Where(x => x.Type == Definitions.Enums.LibraryType.Steam))
                        {
                            foreach (Definitions.SteamAppInfo CurrentApp in CurrentLibrary.Steam.Apps.Where(x => !x.IsSteamBackup).ToList())
                            {
                                ProgressInformationMessage.SetMessage("Checking for:\n\n" + CurrentApp.AppName);

                                if (LibraryToCheck.Steam.Apps.Count(x => x.AppID == CurrentApp.AppID && x.LastUpdated > CurrentApp.LastUpdated) > 0)
                                {
                                    Definitions.SteamAppInfo LatestApp = LibraryToCheck.Steam.Apps.First(x => x.AppID == CurrentApp.AppID && x.LastUpdated > CurrentApp.LastUpdated);

                                    if (Framework.TaskManager.TaskList.Count(x => x.SteamApp.AppID == CurrentApp.AppID && x.TargetLibrary == LatestApp.Library && !x.Completed) == 0)
                                    {
                                        Definitions.List.TaskInfo NewTask = new Definitions.List.TaskInfo
                                        {
                                            SteamApp = LatestApp,
                                            TargetLibrary = CurrentApp.Library
                                        };

                                        Framework.TaskManager.AddTask(NewTask);
                                    }

                                    Main.FormAccessor.TaskManager_Logs.Add($"[{DateTime.Now}] An update is available for: {CurrentApp.AppName} - Old backup time: {LatestApp.LastUpdated} - Updated on: {CurrentApp.LastUpdated} - Target: {CurrentApp.Library.Steam.FullPath} - Source: {LatestApp.Library.Steam.FullPath}");
                                }
                            }
                        }
                    }

                    await ProgressInformationMessage.CloseAsync();
                    Main.FormAccessor.TaskManager_Logs.Add($"[{DateTime.Now}] Checked for Backup updates.");
                }
                catch (Exception ex)
                {
                    logger.Fatal(ex);
                    Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
                }
            }

            public static async void AddNew(string LibraryPath, bool IsMainLibrary = false)
            {
                try
                {
                    Definitions.SteamLibrary Library = new Definitions.SteamLibrary(LibraryPath, IsMainLibrary);

                    Definitions.List.Libraries.Add(new Definitions.Library
                    {
                        Type = Definitions.Enums.LibraryType.Steam,
                        DirectoryInfo = new DirectoryInfo(LibraryPath),
                        Steam = Library
                    });

                    await Task.Run(() => Library.UpdateAppList());
                    await Task.Run(() => Library.UpdateJunks());
                }
                catch (Exception ex)
                {
                    logger.Fatal(ex);
                    ex.Data.Add("LibraryPath", LibraryPath);
                    ex.Data.Add("CurrentLibraries", Definitions.List.Libraries.ToList());
                    Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
                }
            }

            public static void GenerateLibraryList()
            {
                try
                {
                    if (File.Exists(Path.Combine(Properties.Settings.Default.steamInstallationPath, "Steam.exe")))
                    {
                        AddNew(Properties.Settings.Default.steamInstallationPath, true);
                    }

                    // Make a KeyValue reader
                    Framework.KeyValue KeyValReader = new Framework.KeyValue();

                    // If config.vdf exists
                    if (File.Exists(Definitions.Global.Steam.vdfFilePath))
                    {
                        // Read our vdf file as text
                        KeyValReader.ReadFileAsText(Definitions.Global.Steam.vdfFilePath);

                        KeyValReader = KeyValReader["Software"]["Valve"]["Steam"];
                        if (KeyValReader?.Children.Count > 0)
                        {
                            foreach (Framework.KeyValue key in KeyValReader.Children.Where(x => x.Name.Contains("BaseInstallFolder")))
                            {
                                AddNew(key.Value);
                            }

                            Definitions.SLM.UserSteamID64 = (KeyValReader["Accounts"].Children.Count > 0) ? KeyValReader["Accounts"]?.Children[0]?.Children[0]?.Value : null;
                        }
                    }
                    else { /* Could not locate LibraryFolders.vdf */ }
                }
                catch (Exception ex)
                {
                    logger.Fatal(ex);
                    Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
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