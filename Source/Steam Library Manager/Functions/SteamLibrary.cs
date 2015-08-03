using System.IO;
using System.Windows.Forms;

namespace Steam_Library_Manager.Functions
{
    class SteamLibrary
    {
        public static void UpdateLibraries()
        {
            try
            {
                // If we already have definitions in our list
                if (Definitions.List.Library.Count != 0)
                    // Clear them so they don't conflict
                    Definitions.List.Library.Clear();

                // If Steam.exe not exists in the path we set then return
                if (!File.Exists(Properties.Settings.Default.SteamInstallationPath + "Steam.exe")) return;

                // Our main library doesn't included in LibraryFolders.vdf so we have to include it manually
                Definitions.List.LibraryList Library = new Definitions.List.LibraryList();

                // Tell it is our main game library which can be handy in future
                Library.Main = true;

                // Define our library path to SteamApps
                Library.Directory = Properties.Settings.Default.SteamInstallationPath + @"SteamApps\";

                // Count how many games we have installed in our library
                Library.GameCount = Games.GetGamesCountFromLibrary(Library);

                // And add collected informations to our global list
                Definitions.List.Library.Add(Library);

                // Make a KeyValue reader
                Framework.KeyValue Key = new Framework.KeyValue();

                // Define our LibraryFolders.VDF path for easier use
                string vdfFilePath = Properties.Settings.Default.SteamInstallationPath + @"SteamApps\libraryfolders.vdf";

                // If LibraryFolders.vdf exists
                if (System.IO.File.Exists(vdfFilePath))
                {
                    // Read our vdf file as text
                    Key.ReadFileAsText(vdfFilePath);

                    // Until someone gives a better idea, try to look for 255 Keys but break at first null key
                    for (int i = 1; i < Definitions.Steam.maxLibraryCount; i++)
                    {
                        // break if key is not exists
                        if (Key[i.ToString()].Value == null)
                            break;

                        // Define a new library
                        Library = new Definitions.List.LibraryList();

                        // Define library path
                        Library.Directory = Key[i.ToString()].Value + @"\SteamApps\";

                        // Define game count in library
                        Library.GameCount = Games.GetGamesCountFromLibrary(Library);

                        // Add our new library to list
                        Definitions.List.Library.Add(Library);
                    }
                }
                else { /* Could not locate LibraryFolders.vdf */ }

                // If we have a backup library(s)
                if (Properties.Settings.Default.BackupDirectories != null)
                {
                    // for each backup library we have do a loop
                    foreach (object backupDirectory in Properties.Settings.Default.BackupDirectories)
                    {
                        // Define a new library
                        Library = new Definitions.List.LibraryList();

                        // Define it is a backup library
                        Library.Backup = true;

                        // Define library path
                        Library.Directory = backupDirectory.ToString();

                        // Define game count in library
                        Library.GameCount = Functions.Games.GetGamesCountFromLibrary(Library);

                        // Add our new library to list
                        Definitions.List.Library.Add(Library);
                    }
                }

                // Update Libraries List visually
                UpdateMainForm();
            }
            catch { }
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
                    libraryDetailBox.Image = global::Steam_Library_Manager.Properties.Resources.Folder;

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
                    libraryName.Text = Library.Directory + " (" + Library.GameCount.ToString()  + ")";

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

                    // Add an item which will show our library directory and make it disabled
                    rightClickMenu.MenuItems.Add(Library.Directory).Enabled = false;

                    // Add an item which will show game count in our library and make it disabled
                    rightClickMenu.MenuItems.Add("Game Count: " + Library.GameCount.ToString()).Enabled = false;

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

        public static void CreateNewLibrary(string newLibraryPath, bool Backup)
        {
            try
            {
                // If we are not creating a backup library
                if (!Backup)
                {
                    // Copy Steam.dll as steam needs it
                    File.Copy(Properties.Settings.Default.SteamInstallationPath + "Steam.dll", newLibraryPath + @"\Steam.dll", true);

                    // create SteamApps directory at requested directory
                    Directory.CreateDirectory(newLibraryPath + @"\SteamApps");

                    // If Steam.dll moved succesfully
                    if (File.Exists(newLibraryPath + @"\Steam.dll")) // in case of permissions denied
                    {
                        // Set libraryFolders.vdf path
                        string vdfPath = Properties.Settings.Default.SteamInstallationPath + @"SteamApps\libraryfolders.vdf";

                        // Call KeyValue in act
                        Framework.KeyValue Key = new Framework.KeyValue();

                        // Read vdf file as text
                        Key.ReadFileAsText(vdfPath);

                        // Add our new library to vdf file so steam will know we have a new library
                        Key.Children.Add(new Framework.KeyValue((Key.Children.Count - 1).ToString(), newLibraryPath));

                        // Save vdf file
                        Key.SaveToFile(vdfPath, false);

                        // Show a messagebox to user about process
                        System.Windows.Forms.MessageBox.Show("New Steam Library added, Please Restart Steam to see it in work."); // to-do: edit text

                        // Update game libraries
                        UpdateLibraries();
                    }
                    else
                        // Show an error to user and cancel the process because we couldn't get Steam.dll in new library dir
                        System.Windows.Forms.MessageBox.Show("Failed to create new Steam Library, Try to run SLM as Administrator?");
                }
                else
                {
                    // If backup directories in settings null
                    if (Properties.Settings.Default.BackupDirectories == null)
                        // make a new definition
                        Properties.Settings.Default.BackupDirectories = new System.Collections.Specialized.StringCollection();

                    // Add our newest backup library to settings
                    Properties.Settings.Default.BackupDirectories.Add(newLibraryPath + @"\");

                    // Update game libraries
                    UpdateLibraries();

                    // Save our settings
                    Functions.Settings.Save();
                }
            }
            catch { }
        }


        public static bool LibraryExists(string NewLibraryPath)
        {
            try
            {
                // For each current game libraries
                foreach (Definitions.List.LibraryList Library in Definitions.List.Library)
                {
                    // If current library contains NewLibraryPath
                    if (Library.Directory.ToLowerInvariant().Contains(NewLibraryPath.ToLowerInvariant() + "\\steamapps\\"))
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
                if (e.Button != System.Windows.Forms.MouseButtons.Left) return;

                // Define our library details from .Tag attribute which we set earlier
                Definitions.List.LibraryList Library = (sender as Label).Tag as Definitions.List.LibraryList;

                // If we are selecting the same library do nothing, which could be clicked by mistake and result in extra waiting time based on settings situation
                if (Definitions.SLM.LatestSelectedLibrary == Library) return;

                // Update latest selected library
                Definitions.SLM.LatestSelectedLibrary = Library;

                // Update games list from current selection
                Functions.Games.UpdateGamesList(Library);
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
                Definitions.List.GamesList Game = (e.Data.GetData("System.Windows.Forms.PictureBox") as PictureBox).Tag as Definitions.List.GamesList;

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
