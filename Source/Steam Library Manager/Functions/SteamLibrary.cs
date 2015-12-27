using System;
using System.IO;
using System.Windows.Forms;

namespace Steam_Library_Manager.Functions
{
    class SteamLibrary
    {
        public static void updateLibraryDetails(Definitions.List.LibraryList Library, string newLibraryPath)
        {
            try
            {
                // Make a KeyValue reader
                Framework.KeyValue Key = new Framework.KeyValue();

                // Read vdf file
                Key.ReadFileAsText(Definitions.Steam.vdfFilePath);

                // Change old library path with new one
                Key.Children[0].Children[0].Children[0].Children.Find(key => key.Value.Contains(Library.fullPath)).Value = newLibraryPath;

                // Update libraryFolders.vdf file with changes
                Key.SaveToFile(Definitions.Steam.vdfFilePath, false);
            }
            catch { }
        }

        public static void removeLibrary(Definitions.List.LibraryList Library, bool deleteFiles)
        {
            try
            {
                if (deleteFiles)
                    FileSystem.deleteOldLibrary(Library);

                Definitions.List.Library.Remove(Library);

                if (Library.Backup)
                    Settings.updateBackupDirs();
                else
                {
                    // Make a KeyValue reader
                    Framework.KeyValue Key = new Framework.KeyValue();

                    // Read vdf file
                    Key.ReadFileAsText(Definitions.Steam.vdfFilePath);

                    // Remove old library
                    Key.Children[0].Children[0].Children[0].Children.RemoveAll(x => x.Value == Library.fullPath);

                    int i = 1;
                    foreach (Framework.KeyValue key in Key.Children[0].Children[0].Children[0].Children.FindAll(x => x.Name.Contains("BaseInstallFolder")))
                    {
                        key.Name = string.Format("BaseInstallFolder_{0}", i);
                        i++;
                    }

                    // Update libraryFolders.vdf file with changes
                    Key.SaveToFile(Definitions.Steam.vdfFilePath, false);
                }

                updateMainForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public static void createNewLibrary(string newLibraryPath, bool Backup)
        {
            try
            {
                // If we are not creating a backup library
                if (!Backup)
                {
                    // Define steam dll paths for better looking
                    string currentSteamDLLPath = Path.Combine(Properties.Settings.Default.SteamInstallationPath, "Steam.dll");
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
                        Key.Children[0].Children[0].Children[0].Children.Add(new Framework.KeyValue(string.Format("BaseInstallFolder_{0}", Definitions.List.Library.FindAll(x => !x.Backup).Count), newLibraryPath));

                        // Save vdf file
                        Key.SaveToFile(Definitions.Steam.vdfFilePath, false);

                        // Show a messagebox to user about process
                        MessageBox.Show(Languages.SteamLibrary.message_newSteamLibraryCreated);

                        // Update game libraries
                        updateLibraryList();
                    }
                    else
                        // Show an error to user and cancel the process because we couldn't get Steam.dll in new library dir
                        MessageBox.Show(Languages.SteamLibrary.messageError_failedToCreateNewLibrary);
                }
                else
                {
                    // If backup directories in settings not set
                    if (Properties.Settings.Default.BackupDirectories == null)
                        // make a new definition
                        Properties.Settings.Default.BackupDirectories = new System.Collections.Specialized.StringCollection();

                    // Add our newest backup library to settings
                    Properties.Settings.Default.BackupDirectories.Add(newLibraryPath);

                    // Update game libraries
                    updateLibraryList();

                    // Save our settings
                    Settings.Save();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public static void addNewLibrary(string libraryPath, bool mainLibrary, bool backupLibrary)
        {
            try
            {
                Definitions.List.LibraryList Library = new Definitions.List.LibraryList();

                // Define if library is main library
                Library.Main = mainLibrary;

                // Define if library is a backup dir
                Library.Backup = backupLibrary;

                // Define full path of library
                Library.fullPath = libraryPath;

                // Define our library path to SteamApps
                Library.steamAppsPath = Path.Combine(libraryPath, "SteamApps");

                // Define common folder path for future use
                Library.commonPath = Path.Combine(Library.steamAppsPath, "common");

                // Define download folder path
                Library.downloadPath = Path.Combine(Library.steamAppsPath, "downloading");

                // Define workshop folder path
                Library.workshopPath = Path.Combine(Library.steamAppsPath, "workshop");

                // Count how many games we have installed in our library
                Library.GameCount = Games.GetGameCountFromLibrary(Library);

                // And add collected informations to our global list
                Definitions.List.Library.Add(Library);

                Games.UpdateGameList(Library);
            }
            catch { }

        }

        public static void updateLibraryList()
        {
            try
            {
                // If we already have definitions in our list
                if (Definitions.List.Library.Count != 0)
                    // Clear them so they don't conflict
                    Definitions.List.Library.Clear();

                // If Steam.exe not exists in the path we set, then return
                if (File.Exists(Path.Combine(Properties.Settings.Default.SteamInstallationPath, "Steam.exe")))
                    addNewLibrary(Properties.Settings.Default.SteamInstallationPath, true, false);

                // Make a KeyValue reader
                Framework.KeyValue Key = new Framework.KeyValue();

                // If LibraryFolders.vdf exists
                if (File.Exists(Definitions.Steam.vdfFilePath))
                {
                    // Read our vdf file as text
                    Key.ReadFileAsText(Definitions.Steam.vdfFilePath);

                    foreach (Framework.KeyValue key in Key.Children[0].Children[0].Children[0].Children.FindAll(x => x.Name.Contains("BaseInstallFolder")))
                    {
                        addNewLibrary(key.Value, false, false);
                    }
                }
                else { /* Could not locate LibraryFolders.vdf */ }

                // If we have a backup library(s)
                if (Properties.Settings.Default.BackupDirectories != null)
                {
                    // for each backup library we have do a loop
                    foreach (string backupDirectory in Properties.Settings.Default.BackupDirectories)
                    {
                        // If directory not exists
                        if (!Directory.Exists(backupDirectory))
                        {
                            // Make a new dialog and ask user to update library path
                            DialogResult askUserToUpdatePath = MessageBox.Show(string.Format(Languages.SteamLibrary.messageWarning_updateWrongBackupDir, backupDirectory, Environment.NewLine), string.Format(Languages.SteamLibrary.messageWarningTitle_updateWrongBackupDir, backupDirectory), MessageBoxButtons.YesNo);

                            // If user wants to update
                            if (askUserToUpdatePath == DialogResult.Yes)
                            {
                                // Show another dialog to select new path
                                DialogResult newBackupDirectoryPath = Definitions.Accessors.MainForm.folderBrowser_SelectNewLibraryPath.ShowDialog();

                                // If new path selected from dialog
                                if (newBackupDirectoryPath == DialogResult.OK)
                                {
                                    // define selected path to variable
                                    string newLibraryPath = Definitions.Accessors.MainForm.folderBrowser_SelectNewLibraryPath.SelectedPath;

                                    // Check if the selected path is exists
                                    if (!libraryExists(newLibraryPath))
                                    {
                                        // If not exists then get directory root of selected path and see if it is equals with our selected path
                                        if (Directory.GetDirectoryRoot(newLibraryPath) != newLibraryPath)
                                            addNewLibrary(newLibraryPath, false, true);
                                        else
                                            // Else show an error message to user
                                            MessageBox.Show(Languages.SteamLibrary.messageError_noLibraryInRoot);
                                    }
                                    else
                                        // If selected path exists as library then show an error to user
                                        MessageBox.Show(Languages.SteamLibrary.messageError_libraryExists);
                                }
                            }
                        }
                        else
                            addNewLibrary(backupDirectory, false, true);
                    }

                    // Save backup dirs
                    Settings.updateBackupDirs();
                }

                // Update Libraries visually
                updateMainForm();
            }
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log errors to DirectoryRemoval.txt
                    Log.ErrorsToFile(Languages.SteamLibrary.source_Libraries, ex.ToString());
            }
        }

        public static void updateMainForm()
        {
            try
            {
                // If our panel for library listing is not empty
                if (Definitions.Accessors.MainForm.panel_LibraryList.Controls.Count > 0)
                    // Clear the panel
                    MainForm.SafeInvoke(Definitions.Accessors.MainForm.panel_LibraryList, () => Definitions.Accessors.MainForm.panel_LibraryList.Controls.Clear());

                // Do the loop for each Library in our library list
                foreach (Definitions.List.LibraryList Library in Definitions.List.Library)
                {
                    // Define a new pictureBox
                    PictureBox libraryDetailBox = Content.Libraries.generateLibraryBox(Library);

                    // Add our pictureBox to library listening panel
                    MainForm.SafeInvoke(Definitions.Accessors.MainForm.panel_LibraryList, () => Definitions.Accessors.MainForm.panel_LibraryList.Controls.Add(libraryDetailBox));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public static bool libraryExists(string NewLibraryPath)
        {
            try
            {
                if (Definitions.List.Library.FindAll(x => x.fullPath == NewLibraryPath).Count > 0)
                    return true;

                // else, return false which means library is not exists
                return false;
            }
            // In any error return true to prevent possible bugs
            catch { return true; }
        }
    }
}
