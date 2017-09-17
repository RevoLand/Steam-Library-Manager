using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Steam_Library_Manager.Functions
{
    class Library
    {
        public static async void CreateNewLibraryAsync(string newLibraryPath, bool Backup)
        {
            try
            {
                // If we are not creating a backup library
                if (!Backup)
                {
                    await Steam.CloseSteamAsync();

                    // Define steam dll paths for better looking
                    string currentSteamDLLPath = Path.Combine(Properties.Settings.Default.steamInstallationPath, "Steam.dll");
                    string newSteamDLLPath = Path.Combine(newLibraryPath, "Steam.dll");

                    if (!File.Exists(newSteamDLLPath))
                        // Copy Steam.dll as steam needs it
                        File.Copy(currentSteamDLLPath, newSteamDLLPath, true);

                    if (!Directory.Exists(Path.Combine(newLibraryPath, "SteamApps")))
                        // create SteamApps directory at requested directory
                        Directory.CreateDirectory(Path.Combine(newLibraryPath, "SteamApps"));

                    // If Steam.dll moved succesfully
                    if (File.Exists(newSteamDLLPath)) // in case of permissions denied
                    {
                        // Call KeyValue in act
                        Framework.KeyValue Key = new Framework.KeyValue();

                        // Read vdf file as text
                        Key.ReadFileAsText(Definitions.Global.Steam.vdfFilePath);

                        // Add our new library to vdf file so steam will know we have a new library
                        Key["Software"]["Valve"]["Steam"].Children.Add(new Framework.KeyValue(string.Format("BaseInstallFolder_{0}", Definitions.List.SteamLibraries.Select(x => !x.IsBackup).Count()), newLibraryPath));

                        // Save vdf file
                        Key.SaveToFile(Definitions.Global.Steam.vdfFilePath, false);

                        // Show a messagebox to user about process
                        MessageBox.Show("New library created");

                        // Since this file started to interrupt us? 
                        // No need to bother with it since config.vdf is the real deal, just remove it and Steam client will handle.
                        if (File.Exists(Path.Combine(Properties.Settings.Default.steamInstallationPath, "steamapps", "libraryfolders.vdf")))
                            File.Delete(Path.Combine(Properties.Settings.Default.steamInstallationPath, "steamapps", "libraryfolders.vdf"));

                        Steam.RestartSteamAsync();
                    }
                    else
                        // Show an error to user and cancel the process because we couldn't get Steam.dll in new library dir
                        MessageBox.Show("failed to create new library");
                }

                // Add library to list
                AddNewLibraryAsync(newLibraryPath, false, Backup);

                // Save our settings
                SLM.Settings.SaveSettings();
            }
            catch (Exception ex)
            {
                Logger.LogToFile(Logger.LogType.Library, ex.ToString());
                MessageBox.Show(ex.ToString());
            }
        }

        public static void CheckForBackupUpdates()
        {
            try
            {
                if (Definitions.List.SteamLibraries.Count(x => x.IsBackup) == 0)
                    return;

                foreach (Definitions.Steam.Library CurrentLibrary in Definitions.List.SteamLibraries.Where(x => !x.IsBackup))
                {
                    if (CurrentLibrary.Apps.Count == 0)
                        continue;

                    foreach (Definitions.Steam.Library LibraryToCheck in Definitions.List.SteamLibraries.Where(x => x.IsBackup))
                    {
                        foreach (Definitions.Steam.AppInfo LatestApp in CurrentLibrary.Apps.Where(x => !x.SteamBackup))
                        {
                            if (LibraryToCheck.Apps.Count(x => x.AppID == LatestApp.AppID && x.LastUpdated < LatestApp.LastUpdated && !x.SteamBackup) > 0)
                            {
                                Definitions.Steam.AppInfo OldAppBackup = LibraryToCheck.Apps.First(x => x.AppID == LatestApp.AppID && x.LastUpdated < LatestApp.LastUpdated && !x.SteamBackup);

                                if (Framework.TaskManager.TaskList.Count(x => x.TargetApp.AppID == LatestApp.AppID && x.TargetLibrary == OldAppBackup.Library && !x.Completed) == 0)
                                {
                                    Definitions.List.TaskList NewTask = new Definitions.List.TaskList
                                    {
                                        TargetApp = LatestApp,
                                        TargetLibrary = OldAppBackup.Library
                                    };

                                    Framework.TaskManager.AddTask(NewTask);
                                }

                                Main.FormAccessor.TaskManager_Logs.Add($"[{DateTime.Now}] An update is available for: {LatestApp.AppName} - Old backup time: {OldAppBackup.LastUpdated} - Updated on: {LatestApp.LastUpdated} - Target: {LatestApp.Library.FullPath} - Source: {OldAppBackup.Library.FullPath}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Logger.LogToFile(Logger.LogType.Library, ex.ToString());
            }
        }

        public static async void AddNewLibraryAsync(string LibraryPath, bool IsMainLibrary, bool IsBackupLibrary, bool IsOfflineLibrary = false)
        {
            try
            {
                Definitions.Steam.Library Library = new Definitions.Steam.Library()
                {
                    IsMain = IsMainLibrary,
                    IsBackup = IsBackupLibrary,
                    IsOffline = IsOfflineLibrary,

                    // Define full path of library
                    FullPath = LibraryPath
                };

                // And add collected informations to our global list
                Definitions.List.SteamLibraries.Add(Library);

                if (!IsOfflineLibrary)
                {
                    await Task.Run(() => Library.UpdateAppList());
                    await Task.Run(() => Library.UpdateJunks());
                }
            }
            catch (Exception ex)
            {
                Logger.LogToFile(Logger.LogType.Library, ex.ToString());
                MessageBox.Show(ex.ToString());
            }
        }

        public static void GenerateLibraryList()
        {
            try
            {
                // If we already have definitions in our list
                if (Definitions.List.SteamLibraries.Count != 0)
                    // Clear them so they don't conflict
                    Definitions.List.SteamLibraries.Clear();

                if (File.Exists(Path.Combine(Properties.Settings.Default.steamInstallationPath, "Steam.exe")))
                    AddNewLibraryAsync(Properties.Settings.Default.steamInstallationPath, true, false);

                // Make a KeyValue reader
                Framework.KeyValue KeyValReader = new Framework.KeyValue();

                // If config.vdf exists
                if (File.Exists(Definitions.Global.Steam.vdfFilePath))
                {
                    // Read our vdf file as text
                    KeyValReader.ReadFileAsText(Definitions.Global.Steam.vdfFilePath);

                    KeyValReader = KeyValReader["Software"]["Valve"]["Steam"];
                    if (KeyValReader.Children.Count > 0)
                    {
                        Definitions.SLM.UserSteamID64 = (KeyValReader["Accounts"].Children.Count > 0) ? KeyValReader["Accounts"].Children[0].Children[0].Value : null;

                        foreach (Framework.KeyValue key in KeyValReader.Children.FindAll(x => x.Name.Contains("BaseInstallFolder")))
                        {
                            AddNewLibraryAsync(key.Value, false, false);
                        }
                    }
                }
                else { /* Could not locate LibraryFolders.vdf */ }

                // If we have a backup library(s)
                if (Properties.Settings.Default.backupDirectories != null)
                {
                    // for each backup library we have do a loop
                    foreach (string BackupPath in Properties.Settings.Default.backupDirectories)
                    {
                        AddNewLibraryAsync(BackupPath, false, true, !Directory.Exists(BackupPath));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogToFile(Logger.LogType.Library, ex.ToString());
                MessageBox.Show(ex.ToString());
            }
        }

        public static async void UpdateBackupLibraryAsync(Definitions.Steam.Library libraryToUpdate)
        {
            try
            {
                libraryToUpdate.IsOffline = false;
                libraryToUpdate.UpdateDiskDetails();

                await Task.Run(() => libraryToUpdate.UpdateAppList());
            }
            catch (Exception ex)
            {
                Logger.LogToFile(Logger.LogType.Library, ex.ToString());
            }
        }

        public static bool IsLibraryExists(string NewLibraryPath)
        {
            try
            {
                NewLibraryPath = NewLibraryPath.ToLowerInvariant();

                if (Definitions.List.SteamLibraries.Where(x => x.FullPath.ToLowerInvariant() == NewLibraryPath ||
                x.CommonFolder.FullName.ToLowerInvariant() == NewLibraryPath ||
                x.DownloadFolder.FullName.ToLowerInvariant() == NewLibraryPath ||
                x.WorkshopFolder.FullName.ToLowerInvariant() == NewLibraryPath ||
                x.SteamAppsFolder.FullName.ToLowerInvariant() == NewLibraryPath).Count() > 0)
                    return true;

                // else, return false which means library is not exists
                return false;
            }
            // In any error return true to prevent possible bugs
            catch (Exception ex)
            {
                Logger.LogToFile(Logger.LogType.Library, ex.ToString());
                MessageBox.Show(ex.ToString());
                return true;
            }
        }

    }
}
