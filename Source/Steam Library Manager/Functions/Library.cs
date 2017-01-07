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
                        Key.ReadFileAsText(Definitions.Steam.vdfFilePath);

                        // Add our new library to vdf file so steam will know we have a new library
                        Key["Software"]["Valve"]["Steam"].Children.Add(new Framework.KeyValue(string.Format("BaseInstallFolder_{0}", Definitions.List.Libraries.Select(x => !x.Backup).Count()), newLibraryPath));

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
                MessageBox.Show(ex.ToString());
            }
        }

        public static async void AddNewLibraryAsync(string libraryPath, bool mainLibrary, bool backupLibrary, bool OfflineLibrary = false)
        {
            try
            {
                Definitions.Library Library = new Definitions.Library()
                {

                    // Define if library is main library
                    Main = mainLibrary,

                    // Define if library is a backup dir
                    Backup = backupLibrary,

                    Offline = OfflineLibrary,

                    // Define full path of library
                    FullPath = libraryPath,

                    // Define our library path to SteamApps
                    steamAppsPath = new DirectoryInfo(Path.Combine(libraryPath, "SteamApps"))
                };

                // Define common folder path for future use
                Library.commonPath = new DirectoryInfo(Path.Combine(Library.steamAppsPath.FullName, "common"));

                // Define download folder path
                Library.downloadPath = new DirectoryInfo(Path.Combine(Library.steamAppsPath.FullName, "downloading"));

                // Define workshop folder path
                Library.workshopPath = new DirectoryInfo(Path.Combine(Library.steamAppsPath.FullName, "workshop"));

                Library.FreeSpace = FileSystem.GetAvailableFreeSpace(Library.FullPath);
                Library.FreeSpacePerc = 100 - ((int)Math.Round((double)(100 * Library.FreeSpace) / FileSystem.GetUsedSpace(Library.FullPath)));

                Library.ContextMenu = Library.GenerateRightClickMenuItems();

                // And add collected informations to our global list
                Definitions.List.Libraries.Add(Library);

                if (!OfflineLibrary)
                    await Task.Run(() => Library.UpdateGameList());
            }
            catch (Exception ex)
            {
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
                MessageBox.Show(ex.ToString());
            }
        }

        public static async void UpdateBackupLibraryAsync(Definitions.Library libraryToUpdate)
        {
            try
            {
                libraryToUpdate.Offline = false;
                await Task.Run(() => libraryToUpdate.UpdateGameList());
            }
            catch { }
        }

        public static bool IsLibraryExists(string NewLibraryPath)
        {
            try
            {
                NewLibraryPath = NewLibraryPath.ToLowerInvariant();

                if (Definitions.List.Libraries.Where(x => x.FullPath.ToLowerInvariant() == NewLibraryPath ||
                x.commonPath.FullName.ToLowerInvariant() == NewLibraryPath ||
                x.downloadPath.FullName.ToLowerInvariant() == NewLibraryPath ||
                x.workshopPath.FullName.ToLowerInvariant() == NewLibraryPath ||
                x.steamAppsPath.FullName.ToLowerInvariant() == NewLibraryPath).Count() > 0)
                    return true;

                // else, return false which means library is not exists
                return false;
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
