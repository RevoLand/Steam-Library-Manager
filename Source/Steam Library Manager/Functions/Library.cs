using System;
using System.Diagnostics;
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
                        Key.ReadFileAsText(Definitions.Steam.vdfFilePath);

                        // Add our new library to vdf file so steam will know we have a new library
                        Key["Software"]["Valve"]["Steam"].Children.Add(new Framework.KeyValue(string.Format("BaseInstallFolder_{0}", Definitions.List.Libraries.Select(x => !x.IsBackup).Count()), newLibraryPath));

                        // Save vdf file
                        Key.SaveToFile(Definitions.Steam.vdfFilePath, false);

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
                if (Definitions.List.Libraries.Count(x => x.IsBackup) == 0)
                    return;

                foreach (Definitions.Library CurrentLibrary in Definitions.List.Libraries.Where(x => !x.IsBackup))
                {
                    if (CurrentLibrary.Games.Count == 0)
                        continue;

                    foreach (Definitions.Library LibraryToCheck in Definitions.List.Libraries.Where(x => x.IsBackup))
                    {
                        foreach (Definitions.Game LatestGame in CurrentLibrary.Games.Where(x => !x.IsSteamBackup))
                        {
                            if (LibraryToCheck.Games.Count(x => x.AppID == LatestGame.AppID && x.LastUpdated < LatestGame.LastUpdated && !x.IsSteamBackup) > 0)
                            {
                                Definitions.Game OldGameBackup = LibraryToCheck.Games.First(x => x.AppID == LatestGame.AppID && x.LastUpdated < LatestGame.LastUpdated && !x.IsSteamBackup);

                                if (Framework.TaskManager.TaskList.Count(x => x.TargetGame.AppID == LatestGame.AppID && x.TargetLibrary == OldGameBackup.InstalledLibrary) == 0)
                                {
                                    Definitions.List.TaskList newTask = new Definitions.List.TaskList
                                    {
                                        TargetGame = LatestGame,
                                        TargetLibrary = OldGameBackup.InstalledLibrary
                                    };

                                    Framework.TaskManager.TaskList.Add(newTask);
                                    Main.Accessor.taskPanel.Items.Add(newTask);
                                }

                                Debug.WriteLine($"An update is available for: {LatestGame.AppName} - Old backup time: {OldGameBackup.LastUpdated} - Latest game time: {LatestGame.LastUpdated}");
                                Main.Accessor.TaskManager_Logs.Add($"[{DateTime.Now}] An update is available for: {LatestGame.AppName} - Old backup time: {OldGameBackup.LastUpdated} - Updated on: {LatestGame.LastUpdated} - Target: {LatestGame.InstalledLibrary.FullPath} - Source: {OldGameBackup.InstalledLibrary.FullPath}");
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
                Definitions.Library Library = new Definitions.Library()
                {
                    IsMain = IsMainLibrary,
                    IsBackup = IsBackupLibrary,
                    IsOffline = IsOfflineLibrary,

                    // Define full path of library
                    FullPath = LibraryPath
                };

                // And add collected informations to our global list
                Definitions.List.Libraries.Add(Library);

                if (!IsOfflineLibrary)
                {
                    await Task.Run(() => Library.UpdateGameList());
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
                if (Definitions.List.Libraries.Count != 0)
                    // Clear them so they don't conflict
                    Definitions.List.Libraries.Clear();

                if (File.Exists(Path.Combine(Properties.Settings.Default.steamInstallationPath, "Steam.exe")))
                    AddNewLibraryAsync(Properties.Settings.Default.steamInstallationPath, true, false);

                // Make a KeyValue reader
                Framework.KeyValue Key = new Framework.KeyValue();

                // If config.vdf exists
                if (File.Exists(Definitions.Steam.vdfFilePath))
                {
                    // Read our vdf file as text
                    Key.ReadFileAsText(Definitions.Steam.vdfFilePath);

                    Key = Key["Software"]["Valve"]["Steam"];
                    if (Key.Children.Count > 0)
                    {
                        Definitions.SLM.userSteamID64 = (Key["Accounts"].Children.Count > 0) ? Key["Accounts"].Children[0].Children[0].Value : null;

                        foreach (Framework.KeyValue key in Key.Children.FindAll(x => x.Name.Contains("BaseInstallFolder")))
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
                    foreach (string backupDirectory in Properties.Settings.Default.backupDirectories)
                    {
                        AddNewLibraryAsync(backupDirectory, false, true, !Directory.Exists(backupDirectory));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogToFile(Logger.LogType.Library, ex.ToString());
                MessageBox.Show(ex.ToString());
            }
        }

        public static void UpdateLibraryVisual(Definitions.Library libraryToUpdate)
        {
            libraryToUpdate.FreeSpace = FileSystem.GetAvailableFreeSpace(libraryToUpdate.FullPath);
            libraryToUpdate.FreeSpacePerc = 100 - ((int)Math.Round((double)(100 * libraryToUpdate.FreeSpace) / FileSystem.GetTotalSize(libraryToUpdate.FullPath)));
        }

        public static async void UpdateBackupLibraryAsync(Definitions.Library libraryToUpdate)
        {
            try
            {
                libraryToUpdate.IsOffline = false;
                UpdateLibraryVisual(libraryToUpdate);

                await Task.Run(() => libraryToUpdate.UpdateGameList());
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

                if (Definitions.List.Libraries.Where(x => x.FullPath.ToLowerInvariant() == NewLibraryPath ||
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
