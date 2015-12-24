using System;
using System.IO;
using System.Linq;
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
                        MessageBox.Show("New Steam Library added, Please Restart Steam to see it in action."); // to-do: edit text

                        // Update game libraries
                        updateLibraryList();
                    }
                    else
                        // Show an error to user and cancel the process because we couldn't get Steam.dll in new library dir
                        MessageBox.Show("Failed to create new Steam Library, Try to run SLM as Administrator?");
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

                // If Steam.exe not exists in the path we set then return
                if (!File.Exists(Path.Combine(Properties.Settings.Default.SteamInstallationPath, "Steam.exe"))) return;

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
                            DialogResult askUserToUpdatePath = MessageBox.Show(string.Format("Backup library couldn't be found: {0}\n\n Would you like to change path?", backupDirectory), string.Format("Backup directory {0} not found", backupDirectory), MessageBoxButtons.YesNo);

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
                                        {
                                            addNewLibrary(newLibraryPath, false, true);
                                        }
                                        else
                                            // Else show an error message to user
                                            MessageBox.Show("Libraries can not be created in root");
                                    }
                                    else
                                        // If selected path exists as library then show an error to user
                                        MessageBox.Show("Library exists in the selected path! Are you trying to bug yourself?!");
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
                    Log.ErrorsToFile("Libraries", ex.ToString());
            }
        }

        public static void updateMainForm()
        {
            try
            {
                // If our panel for library listing is not empty
                if (Definitions.Accessors.MainForm.panel_LibraryList.Controls.Count > 0)
                    // Clear the panel
                    Main.SafeInvoke(Definitions.Accessors.MainForm.panel_LibraryList, () => Definitions.Accessors.MainForm.panel_LibraryList.Controls.Clear());

                // Do the loop for each Library in our library list
                foreach (Definitions.List.LibraryList Library in Definitions.List.Library)
                {
                    // Define a new pictureBox
                    PictureBox libraryDetailBox = new PictureBox();

                    // Set our image for picturebox
                    libraryDetailBox.Image = Properties.Resources.libraryIcon;

                    // Set our picturebox size
                    libraryDetailBox.Size = Properties.Settings.Default.libraryPictureSize;

                    // Center our image, we are using an image smaller than our pictureBox size and centering image so our library name label will read easily
                    libraryDetailBox.SizeMode = PictureBoxSizeMode.CenterImage;

                    // Define our Library as Tag of pictureBox so we can easily get details of this library (pictureBox) in future 
                    libraryDetailBox.Tag = Library;

                    // Set events for library name click
                    libraryDetailBox.MouseClick += libraryDetailBox_OnSelect;

                    // Allow drops to our pictureBox
                    ((Control)libraryDetailBox).AllowDrop = true;

                    // Definition of DragEnter event
                    libraryDetailBox.DragEnter += libraryDetailBox_DragEnter;

                    // Definition of DragDrop event
                    libraryDetailBox.DragDrop += libraryDetailBox_DragDrop;

                    // Create a new Label to show on pictureBox as Library name
                    Label libraryName = new Label();

                    // Set our label size
                    libraryName.AutoSize = true;

                    // Set label text, currently it is directory path + game count
                    libraryName.Text = string.Format("[{2}] {0} ({1})", Library.fullPath, Library.GameCount, FileSystem.FormatBytes(FileSystem.GetFreeSpace(Library.fullPath)));

                    // Show our label in bottom center of our pictureBox
                    libraryName.TextAlign = System.Drawing.ContentAlignment.BottomCenter;

                    // Set our label background color to transparent, actually we may try using a color in future
                    libraryName.BackColor = System.Drawing.Color.Transparent;

                    // Set our font to Segoe UI Semilight for better looking, all suggestions are welcome
                    libraryName.Font = new System.Drawing.Font("Segoe UI Semilight", 8);
                    libraryName.Top = Properties.Settings.Default.libraryPictureSize.Height - 15;

                    // Create a new right click menu (aka context menu)
                    ContextMenu rightClickMenu = new ContextMenu();

                    // Define an event handler to use with library context menu
                    EventHandler mouseClick = new EventHandler(libraryDetailBox_ContextMenuAction);

                    // Add an item which will show our library directory and make it disabled
                    rightClickMenu.MenuItems.Add("Open Library in Explorer", mouseClick).Name = "Disk";

                    // spacer
                    rightClickMenu.MenuItems.Add("-");

                    // Move library
                    rightClickMenu.MenuItems.Add("Move Library", mouseClick).Name = "moveLibrary";

                    // spacer
                    rightClickMenu.MenuItems.Add("-");

                    // Refresh games in library
                    rightClickMenu.MenuItems.Add("Refresh games in library", mouseClick).Name = "RefreshGameList";

                    // spacer
                    rightClickMenu.MenuItems.Add("-");

                    // Delete library
                    rightClickMenu.MenuItems.Add("Delete Library", mouseClick).Name = "deleteLibrary";

                    // Delete games in library
                    rightClickMenu.MenuItems.Add("Delete Games in Library", mouseClick).Name = "deleteLibrarySLM";

                    if (Library.Backup)
                    {
                        // Spacer
                        rightClickMenu.MenuItems.Add("-");

                        // Remove the library from slm (only from list)
                        rightClickMenu.MenuItems.Add("Remove from List", mouseClick).Name = "RemoveFromList";
                    }

                    // Add our label to pictureBox
                    libraryDetailBox.Controls.Add(libraryName);

                    // Add our right click (context) menu to pictureBox
                    libraryDetailBox.ContextMenu = rightClickMenu;

                    // Add our pictureBox to library listening panel
                    Main.SafeInvoke(Definitions.Accessors.MainForm.panel_LibraryList, () => Definitions.Accessors.MainForm.panel_LibraryList.Controls.Add(libraryDetailBox));
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

        static async void libraryDetailBox_ContextMenuAction(object sender, EventArgs e)
        {
            try
            {
                // Define our game from the Tag we given to Context menu
                Definitions.List.LibraryList Library = (sender as MenuItem).GetContextMenu().SourceControl.Tag as Definitions.List.LibraryList;

                // switch based on name we set earlier with context menu
                switch ((sender as MenuItem).Name)
                {
                    // Opens game installation path in explorer
                    case "Disk":
                        System.Diagnostics.Process.Start(Library.steamAppsPath);
                        break;
                    case "RefreshGameList":
                        Games.UpdateGameList(Library);
                        break;
                    case "deleteLibrary":
                        DialogResult moveGamesBeforeDeletion = MessageBox.Show("Move Games in Library before deleting?", "Move Games", MessageBoxButtons.YesNoCancel);

                        if (moveGamesBeforeDeletion == DialogResult.Yes)
                        {
                            new Forms.moveLibrary(Library).Show();
                        }
                        else if (moveGamesBeforeDeletion == DialogResult.No)
                            removeLibrary(Library, true);
                        break;
                    case "deleteLibrarySLM":
                        foreach (Definitions.List.GamesList Game in Definitions.List.Game.Where(x => x.Library == Library))
                        {
                            Games gameFunctions = new Games();

                            if (!await gameFunctions.deleteGameFiles(Game))
                            {
                                MessageBox.Show($"An error happened while removing games from library: {Library.fullPath}");

                                return;
                            }
                        }

                        SteamLibrary.updateLibraryList();
                        SteamLibrary.updateMainForm();
                        Games.UpdateGameList(Library);

                        MessageBox.Show($"Done!\nAll games in Library ({Library.fullPath}) deleted.");
                        break;
                    case "moveLibrary":
                        new Forms.moveLibrary(Library).Show();
                        break;

                    // Removes a backup library from list
                    case "RemoveFromList":
                        if (Library.Backup)
                        {
                            // Remove the library from our list
                            Definitions.List.Library.Remove(Library);

                            // Update backup dir settings
                            Settings.updateBackupDirs();

                            // Update main form with new settings
                            updateMainForm();
                        }
                        break;
                }

            }
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log errors to DirectoryRemoval.txt
                    Log.ErrorsToFile("Libraries", ex.ToString());
            }
        }

        static void libraryDetailBox_OnSelect(object sender, MouseEventArgs e)
        {
            try
            {
                // If user not clicked with left button return (so right-click menu will stay without a problem)
                if (e.Button != MouseButtons.Left) return;

                // Define our library details from .Tag attribute which we set earlier
                Definitions.List.LibraryList Library = (sender as PictureBox).Tag as Definitions.List.LibraryList;

                // If we are selecting the same library do nothing, which could be clicked by mistake and result in extra waiting time based on settings situation
                if (Definitions.SLM.LatestSelectedLibrary == Library) return;

                // Update latest selected library
                Definitions.SLM.LatestSelectedLibrary = Library;

                // Update games list from current selection
                Games.UpdateMainForm(null, null, Library);
            }
            catch { }
        }

        static void libraryDetailBox_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                // Sets visual effect, if we do not set it we will not be able to drop games to library
                e.Effect = DragDropEffects.Move;
            }
            catch { }
        }

        // On drop of dragged game image
        static void libraryDetailBox_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                // Define our library details
                Definitions.List.LibraryList Library = (sender as PictureBox).Tag as Definitions.List.LibraryList;

                // Define our game details
                Definitions.List.GamesList Game = (e.Data.GetData("Steam_Library_Manager.Framework.PictureBoxWithCaching") as Framework.PictureBoxWithCaching).Tag as Definitions.List.GamesList;

                // If we dropped game to the library which is already on it then do nothing
                if (Game.Library == Library) return;

                // Create a new instance of MoveGame form
                new Forms.moveGame(Game, Library).Show();
            }
            catch { }
        }

    }
}
