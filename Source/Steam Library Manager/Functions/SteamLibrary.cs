using System;
using System.IO;
using System.Windows.Forms;
using System.Linq;

namespace Steam_Library_Manager.Functions
{
    class SteamLibrary
    {

        public static void AddNewLibrary(string libraryPath, bool mainLibrary, bool backupLibrary)
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
                Library.GameCount = Games.GetGamesCountFromLibrary(Library);

                // And add collected informations to our global list
                Definitions.List.Library.Add(Library);
            }
            catch { }

        }

        public static void UpdateLibraryList()
        {
            try
            {
                // If we already have definitions in our list
                if (Definitions.List.Library.Count != 0)
                    // Clear them so they don't conflict
                    Definitions.List.Library.Clear();

                // If Steam.exe not exists in the path we set then return
                if (!File.Exists(Path.Combine(Properties.Settings.Default.SteamInstallationPath, "Steam.exe"))) return;

                AddNewLibrary(Properties.Settings.Default.SteamInstallationPath, true, false);

                // Make a KeyValue reader
                Framework.KeyValue Key = new Framework.KeyValue();

                // If LibraryFolders.vdf exists
                if (File.Exists(Definitions.Steam.vdfFilePath))
                {
                    // Read our vdf file as text
                    Key.ReadFileAsText(Definitions.Steam.vdfFilePath);

                    // Until someone gives a better idea, try to look for 255 Keys but break at first null key
                    for (int i = 1; i < Definitions.Steam.maxLibraryCount; i++)
                    {
                        // break if key is not exists
                        if (Key[i.ToString()].Value == null)
                            break;

                        AddNewLibrary(Key[i.ToString()].Value, false, false);
                    }
                }
                else { /* Could not locate LibraryFolders.vdf */ }

                // If we have a backup library(s)
                if (Properties.Settings.Default.BackupDirectories != null)
                {
                    // for each backup library we have do a loop
                    foreach (string backupDirectory in Properties.Settings.Default.BackupDirectories)
                    {
                        AddNewLibrary(backupDirectory, false, true);
                    }
                }

                // Update Libraries List visually
                UpdateMainForm();
            }
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log errors to DirectoryRemoval.txt
                    Log.ErrorsToFile("Libraries", ex.ToString());
            }
        }

        public static void UpdateMainForm()
        {
            try
            {
                // If our panel for library listing is not empty
                if (Definitions.Accessors.MainForm.panel_LibraryList.Controls.Count != 0)
                    // Clear the panel
                    Definitions.Accessors.MainForm.panel_LibraryList.Controls.Clear();

                // Definition for pictureBox height and width
                int height = 155, width = 195;

                // Do the loop for each Library in our library list
                foreach (Definitions.List.LibraryList Library in Definitions.List.Library)
                {
                    // Define a new pictureBox
                    PictureBox libraryDetailBox = new PictureBox();

                    // Set our image for picturebox
                    libraryDetailBox.Image = Properties.Resources.libraryIcon;

                    // Set our picturebox size
                    libraryDetailBox.Size = new System.Drawing.Size(width, height);

                    // Center our image, we are using an image smaller than our pictureBox size and centering image so our library name label will be seen and read easily
                    libraryDetailBox.SizeMode = PictureBoxSizeMode.CenterImage;

                    // Define our Library as Tag of pictureBox so we can easily get details of this library (pictureBox) in future 
                    libraryDetailBox.Tag = Library;

                    // Allow drops to our pictureBox
                    ((Control)libraryDetailBox).AllowDrop = true;

                    // Definition of DragEnter event
                    libraryDetailBox.DragEnter += libraryDetailBox_DragEnter;

                    // Definition of DragDrop event
                    libraryDetailBox.DragDrop += libraryDetailBox_DragDrop;

                    // Create a new Label to show on pictureBox as Library name
                    Label libraryName = new Label();

                    // Set our label size as same with our pictureBox
                    libraryName.Size = new System.Drawing.Size(width, height);

                    // Set label text, currently it is directory path + game count
                    libraryName.Text = string.Format("{0} ({1})", Library.steamAppsPath, Library.GameCount);

                    // Show our label in bottom center of our pictureBox
                    libraryName.TextAlign = System.Drawing.ContentAlignment.BottomCenter;

                    // Set our label background color to transparent, actually we may try using a color in future
                    libraryName.BackColor = System.Drawing.Color.Transparent;

                    // Set our font to Segoe UI Semilight for better looking, all suggestions are welcome
                    libraryName.Font = new System.Drawing.Font("Segoe UI Semilight", 8);

                    // Define our current Library details as Tag of this Label
                    libraryName.Tag = Library;
                    
                    // Set events for library name click
                    libraryName.MouseClick += libraryDetailBox_OnSelect;

                    // Create a new right click menu (aka context menu)
                    ContextMenu rightClickMenu = new ContextMenu();

                    // Define an event handler to use with library context menu
                    EventHandler mouseClick = new EventHandler(libraryDetailBox_ContextMenuAction);

                    // Set our library details as context menu tag
                    rightClickMenu.Tag = Library;

                    // Add an item which will show our library directory and make it disabled
                    rightClickMenu.MenuItems.Add(Library.steamAppsPath, mouseClick).Name = "Disk";

                    // Move library
                    rightClickMenu.MenuItems.Add("Move Library", mouseClick).Name = "moveLibrary";

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
                    Definitions.Accessors.MainForm.panel_LibraryList.Controls.Add(libraryDetailBox);
                }

                // So mousewheel will work
                Definitions.Accessors.MainForm.panel_LibraryList.Focus();
            }
            catch { }
        }

        static async void libraryDetailBox_ContextMenuAction(object sender, EventArgs e)
        {
            try
            {
                // Define our game from the Tag we given to Context menu
                Definitions.List.LibraryList Library = (sender as MenuItem).Parent.Tag as Definitions.List.LibraryList;

                // switch based on name we set earlier with context menu
                switch ((sender as MenuItem).Name)
                {
                    // Opens game installation path in explorer
                    case "Disk":
                        System.Diagnostics.Process.Start(Library.steamAppsPath);
                        break;

                    case "moveLibrary":
                        if (Library.Main)
                            return;

                        string newFileName;

                        // Create a new dialog result and show to user
                        DialogResult newLibrarySelection = Definitions.Accessors.MainForm.folderBrowser_SelectNewLibraryPath.ShowDialog();

                        // If our dialog is closed with OK (directory selected)
                        if (newLibrarySelection == DialogResult.OK)
                        {
                            // Define selected path for easier usage in future
                            string newLibraryPath = Definitions.Accessors.MainForm.folderBrowser_SelectNewLibraryPath.SelectedPath;

                            // Check if the selected path is exists
                            if (!LibraryExists(newLibraryPath))
                            {
                                DialogResult confirmLibraryAction = MessageBox.Show(string.Format("Do you want to move library located at [{0}] to [{1}] ?", Library.fullPath, newLibraryPath), "Move Library", MessageBoxButtons.YesNo);

                                if (confirmLibraryAction == DialogResult.Yes)
                                {
                                    switch (Library.Backup)
                                    {
                                        case false:

                                            // For each file in common folder of game
                                            foreach (string currentFile in Directory.EnumerateFiles(Library.steamAppsPath, "*", SearchOption.AllDirectories))
                                            {
                                                // Make a new file stream from the file we are reading so we can copy the file asynchronously
                                                using (FileStream currentFileStream = File.OpenRead(currentFile))
                                                {
                                                    // Set new file name including target game path
                                                    newFileName = Path.Combine(newLibraryPath, currentFile.Replace(Library.fullPath + @"\", ""));

                                                    // If directory not exists
                                                    if (!Directory.Exists(Path.GetDirectoryName(newFileName)))
                                                        // Create a directory at target library for new file, if we do not the process will fail
                                                        Directory.CreateDirectory(Path.GetDirectoryName(newFileName));

                                                    // Create a new file
                                                    using (FileStream newFileStream = File.Create(newFileName))
                                                    {
                                                        // Copy the file to target library asynchronously
                                                        await currentFileStream.CopyToAsync(newFileStream);
                                                    }
                                                }
                                            }

                                            // Define steam dll paths for better looking
                                            string currentSteamDLLPath = Path.Combine(Properties.Settings.Default.SteamInstallationPath, "Steam.dll");
                                            string newSteamDLLPath = Path.Combine(newLibraryPath, "Steam.dll");

                                            // Copy Steam.dll as steam needs it
                                            File.Copy(currentSteamDLLPath, newSteamDLLPath, true);

                                            // Make a KeyValue reader
                                            Framework.KeyValue Key = new Framework.KeyValue();

                                            // Read vdf file
                                            Key.ReadFileAsText(Definitions.Steam.vdfFilePath);

                                            // Change old library path with new one
                                            Key.Children.Find(key => key.Value.Contains(Library.fullPath)).Value = newLibraryPath;

                                            // Update libraryFolders.vdf file with changes
                                            Key.SaveToFile(Definitions.Steam.vdfFilePath, false);

                                            // Remove old library
                                            Directory.Delete(Library.steamAppsPath, true);

                                            // Show a message box to user, temporarily
                                            MessageBox.Show(string.Format("Library [{0}] moved to [{1}]", Library.fullPath, newLibraryPath));

                                            // Update library list
                                            UpdateLibraryList();
                                            break;
                                    }
                                }
                                else
                                    return;
                            }
                            else
                            {
                                MessageBox.Show("Library already exists at selected path, please select another path.");
                            }

                        }
                        break;

                    // Removes a backup library from list
                    case "RemoveFromList":
                        if (Library.Backup)
                        {
                            // Remove the library from our list
                            Definitions.List.Library.Remove(Library);

                            // Update backup dir settings
                            Settings.UpdateBackupDirs();

                            // Update main form with new settings
                            UpdateMainForm();
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

        public static void CreateNewLibrary(string newLibraryPath, bool Backup)
        {
            try
            {
                // If we are not creating a backup library
                if (!Backup)
                {
                    // Define steam dll paths for better looking
                    string currentSteamDLLPath = Path.Combine(Properties.Settings.Default.SteamInstallationPath, "Steam.dll");
                    string newSteamDLLPath = Path.Combine(newLibraryPath, "Steam.dll");

                    // Copy Steam.dll as steam needs it
                    File.Copy(currentSteamDLLPath, newSteamDLLPath, true);

                    // create SteamApps directory at requested directory
                    Directory.CreateDirectory(Path.Combine(newLibraryPath, "SteamApps"));

                    // If Steam.dll moved succesfully
                    if (File.Exists(newSteamDLLPath)) // in case of permissions denied
                    {
                        // Set libraryFolders.vdf path
                        string vdfPath = Path.Combine(Properties.Settings.Default.SteamInstallationPath, "SteamApps", "libraryfolders.vdf");

                        // Call KeyValue in act
                        Framework.KeyValue Key = new Framework.KeyValue();

                        // Read vdf file as text
                        Key.ReadFileAsText(vdfPath);

                        // Add our new library to vdf file so steam will know we have a new library
                        Key.Children.Add(new Framework.KeyValue((Key.Children.Count - 1).ToString(), newLibraryPath));

                        // Save vdf file
                        Key.SaveToFile(vdfPath, false);

                        // Show a messagebox to user about process
                        MessageBox.Show("New Steam Library added, Please Restart Steam to see it in action."); // to-do: edit text

                        // Update game libraries
                        UpdateLibraryList();
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
                    UpdateLibraryList();

                    // Save our settings
                    Settings.Save();
                }
            }
            catch { }
        }

        public static bool LibraryExists(string NewLibraryPath)
        {
            try
            {
                // For each current game libraries
                foreach (Definitions.List.LibraryList Library in Definitions.List.Library.Where(x => x.steamAppsPath.ToLowerInvariant().Contains(NewLibraryPath.ToLowerInvariant())))
                {
                    // If current library contains NewLibraryPath
                    // Then return true
                    return true;
                }
                // else, return false which means library is not exists
                return false;
            }
            // In any error return true to prevent possible bugs
            catch { return true; }
        }

        static void libraryDetailBox_OnSelect(object sender, MouseEventArgs e)
        {
            try
            {
                // If user not clicked with left button return (so right-click menu will stay without a problem)
                if (e.Button != MouseButtons.Left) return;

                // Define our library details from .Tag attribute which we set earlier
                Definitions.List.LibraryList Library = (sender as Label).Tag as Definitions.List.LibraryList;

                // If we are selecting the same library do nothing, which could be clicked by mistake and result in extra waiting time based on settings situation
                if (Definitions.SLM.LatestSelectedLibrary == Library) return;

                // Update latest selected library
                Definitions.SLM.LatestSelectedLibrary = Library;

                // Update games list from current selection
                Games.UpdateGameList(Library);
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

                // Set our library details to use in MoveGame form
                Definitions.SLM.LatestDropLibrary = Library;

                // Set our game details to use in MoveGame form
                Definitions.SLM.LatestSelectedGame = Game;

                // Create a new instance of MoveGame form
                new Forms.MoveGame().Show();
            }
            catch { }
        }

    }
}
