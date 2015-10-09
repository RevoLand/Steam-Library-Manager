using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;

namespace Steam_Library_Manager.Functions
{
    class Games
    {
        public static int GetGamesCountFromLibrary(Definitions.List.LibraryList Library)
        {
            try
            {
                // Define an int for total game count
                int gameCount = 0;

                // Get *.acf file count from library path
                gameCount += Directory.GetFiles(Library.Directory, "*.acf", SearchOption.TopDirectoryOnly).Length;

                // If library is a backup library
                if (Library.Backup)
                    // Also get *.zip file count from backup library path
                    gameCount += Directory.GetFiles(Library.Directory, "*.zip", SearchOption.TopDirectoryOnly).Length;

                // return total game count we have found
                return gameCount;
            }
            catch { return 0; }
        }

        public static void UpdateGamesList(Definitions.List.LibraryList Library)
        {
            try
            {
                // If our list is not empty
                if (Definitions.List.Game.Count != 0)
                    // Clear the list
                    Definitions.List.Game.Clear();

                // Foreach *.acf file found in library
                foreach (string game in Directory.EnumerateFiles(Library.Directory, "*.acf", SearchOption.TopDirectoryOnly))
                {
                    // Define a new value and call KeyValue
                    Framework.KeyValue Key = new Framework.KeyValue();

                    // Read the *.acf file as text
                    Key.ReadFileAsText(game);

                    // If key doesn't contains a child (value in acf file)
                    if (Key.Children.Count == 0)
                        // Skip this file (game)
                        continue;

                    // Make a new definition for game
                    Definitions.List.GamesList Game = new Definitions.List.GamesList();

                    // Set game appID
                    Game.appID = Convert.ToInt32(Key["appID"].Value);

                    // Set game name
                    Game.appName = Key["name"].Value;

                    // If app name couldn't find
                    if (string.IsNullOrEmpty(Game.appName))
                        // Check for userconfig for name
                        Game.appName = Key["UserConfig"]["name"].Value;

                    // Set installation path
                    Game.installationPath = Key["installdir"].Value;

                    // Set game library
                    Game.Library = Library;

                    // Set game acf path
                    Game.acfPath = game;

                    // If game has a folder in "common" dir, define it as exactInstallPath
                    if (Directory.Exists(Path.Combine(Library.Directory, "common", Game.installationPath)))
                        Game.exactInstallPath = Path.Combine(Library.Directory, "common", Game.installationPath) + Path.DirectorySeparatorChar.ToString();

                    // If game has a folder in "downloading" dir, define it as downloadPath
                    if (Directory.Exists(Path.Combine(Library.Directory, "downloading", Game.installationPath)))
                        Game.downloadPath = Path.Combine(Library.Directory, "downloading", Game.installationPath) + Path.DirectorySeparatorChar.ToString();

                    // If game has a folder in "workshop" dir, define it as workShopPath
                    if (Directory.Exists(Path.Combine(Library.Directory, "workshop", "content", Game.appID.ToString())))
                        Game.workShopPath = Path.Combine(Library.Directory, "workshop", "content", Game.appID.ToString()) + Path.DirectorySeparatorChar.ToString();

                    // If game do not have a folder in "common" directory and "downloading" directory then skip this game
                    if (Game.exactInstallPath == null && Game.downloadPath == null)
                        continue; // Do not add pre-loads to list

                    // If SizeOnDisk value from .ACF file is not equals to 0
                    if (Key["SizeOnDisk"].Value != "0")
                    {
                        // If game has "common" folder
                        if (Game.exactInstallPath != null)
                        {
                            // If game size calculation method NOT set as "ACF"
                            if (Properties.Settings.Default.GameSizeCalculationMethod != "ACF")
                                // Calculate game size on disk
                                Game.sizeOnDisk += Functions.FileSystem.GetDirectorySize(Game.exactInstallPath, true);
                            else
                                // Else use the game size from .ACF file
                                Game.sizeOnDisk += Convert.ToInt64(Key["SizeOnDisk"].Value);
                        }

                        // If game has downloading files
                        if (Game.downloadPath != null)
                        {
                            // If game size calculation method NOT set as "ACF"
                            if (Properties.Settings.Default.GameSizeCalculationMethod != "ACF")
                                // Calculate "downloading" folder size
                                Game.sizeOnDisk += Functions.FileSystem.GetDirectorySize(Game.downloadPath, true);
                        }

                        // If game has "workshop" files
                        if (Game.workShopPath != null)
                        {
                            // If game size calculation method NOT set as "ACF"
                            if (Properties.Settings.Default.GameSizeCalculationMethod != "ACF")
                                // Calculate "workshop" files size
                                Game.sizeOnDisk += Functions.FileSystem.GetDirectorySize(Game.workShopPath, true);
                        }

                    }
                    else
                        // Else set game size to 0
                        Game.sizeOnDisk = 0;

                    // Add our game details to global list
                    Definitions.List.Game.Add(Game);
                }

                // If library is backup library
                if (Library.Backup)
                {
                    // Do a loop for each *.zip file in library
                    foreach (string gameArchive in Directory.EnumerateFiles(Library.Directory, "*.zip", SearchOption.TopDirectoryOnly))
                    {
                        // Open archive for read
                        using (ZipArchive compressedArchive = ZipFile.OpenRead(gameArchive))
                        {
                            // For each file in opened archive
                            foreach (ZipArchiveEntry file in compressedArchive.Entries.Where(x => x.Name.Contains(".acf")))
                            {
                                // If it contains
                                // Define a KeyValue reader
                                Framework.KeyValue Key = new Framework.KeyValue();

                                // Open .acf file from archive as text
                                Key.ReadAsText(file.Open());

                                // If acf file has no children, skip this archive
                                if (Key.Children.Count == 0)
                                    return;

                                // Define a new game
                                Definitions.List.GamesList Game = new Definitions.List.GamesList();

                                // Define our app ID
                                Game.appID = Convert.ToInt32(Key["appID"].Value);

                                // Define our app name
                                Game.appName = Key["name"].Value;

                                // Define it is an archive
                                Game.Compressed = true;

                                // Define installation path for game
                                Game.installationPath = Key["installdir"].Value;

                                // Define our library
                                Game.Library = Library;

                                // If user want us to get archive size from real uncompressed size
                                if (Properties.Settings.Default.ArchiveSizeCalculationMethod.StartsWith("Uncompressed"))
                                {
                                    // Open archive to read
                                    using (ZipArchive zip = ZipFile.OpenRead(Path.Combine(Game.Library.Directory, Game.appID + ".zip")))
                                    {
                                        // For each file in archive
                                        foreach (ZipArchiveEntry entry in zip.Entries)
                                        {
                                            // Add file size to sizeOnDisk
                                            Game.sizeOnDisk += entry.Length;
                                        }
                                    }
                                }
                                // Else
                                else
                                {
                                    // Use FileInfo to get our archive details
                                    FileInfo zip = new FileInfo(Path.Combine(Game.Library.Directory, Game.appID + ".zip"));

                                    // And set archive size as game size
                                    Game.sizeOnDisk = zip.Length;
                                }

                                // Add our new game to global definiton
                                Definitions.List.Game.Add(Game);

                                // Update main form as visual
                                Functions.Games.UpdateMainForm(null, null);

                                // we found what we are looking for, return
                                return;
                            }
                        }
                    }
                }

                // Focus to game list panel
                Definitions.Accessors.MainForm.panel_GameList.Focus();

                // Update main form as visual
                Functions.Games.UpdateMainForm(null, null);
            }
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log
                    Log.ErrorsToFile("UpdateGameList", ex.ToString());

                // Show a messagebox to user
                MessageBox.Show("An error happened while updating game list!\n" + ex.ToString());
            }
        }

        public static void UpdateMainForm(Func<Definitions.List.GamesList, Object> Sort, string Search)
        {
            try
            {
                // If our panel for game list not empty
                if (Definitions.Accessors.MainForm.panel_GameList.Controls.Count != 0)
                    // Then clean panel
                    Definitions.Accessors.MainForm.panel_GameList.Controls.Clear();

                // Define our sorting method
                switch (Properties.Settings.Default.SortGamesBy)
                {
                    default:
                    case "appName":
                        Sort = x => x.appName;
                        break;
                    case "appID":
                        Sort = x => x.appID;
                        break;
                    case "sizeOnDisk":
                        Sort = x => x.sizeOnDisk;
                        break;
                }

                // Do a loop for each game in library
                foreach (Definitions.List.GamesList Game in ((string.IsNullOrEmpty(Search)) ? Definitions.List.Game.OrderBy(Sort) : Definitions.List.Game.Where(
                    y => y.appName.ToLowerInvariant().Contains(Search.ToLowerInvariant()) || // Search by appName
                    y.appID.ToString().Contains(Search) // Search by app ID
                    ).OrderBy(Sort)
                    ))
                {
                    // Define a new pictureBox for game
                    Framework.PictureBoxWithCaching gameDetailBox = new Framework.PictureBoxWithCaching();

                    // Set picture mode of pictureBox
                    gameDetailBox.SizeMode = PictureBoxSizeMode.StretchImage;

                    // Set game image size
                    gameDetailBox.Size = new System.Drawing.Size(230, 107);

                    // Load game header image asynchronously
                    gameDetailBox.LoadAsync(string.Format("https://steamcdn-a.akamaihd.net/steam/apps/{0}/header.jpg", Game.appID));

                    // Set error image in case of couldn't load game header image
                    gameDetailBox.ErrorImage = Properties.Resources.no_image_available;

                    // Space between pictureBoxes for better looking
                    gameDetailBox.Margin = new Padding(20);

                    // Set our game details as Tag to pictureBox
                    gameDetailBox.Tag = Game;

                    // On we click to pictureBox (drag & drop event)
                    gameDetailBox.MouseDown += gameDetailBox_MouseDown;

                    // Create a new right click menu (context menu)
                    ContextMenu rightClickMenu = new ContextMenu();

                    // Set our game details to context menu as Tag
                    rightClickMenu.Tag = Game;

                    // Define an event handler
                    EventHandler mouseClick = new EventHandler(gameDetailBox_ContextMenuAction);

                    // Add right click menu items
                    // Game name (appID) // disabled
                    rightClickMenu.MenuItems.Add(string.Format("{0} (ID: {1})", Game.appName, Game.appID)).Enabled = false;

                    // Game Size on Disk: 124MB // disabled
                    rightClickMenu.MenuItems.Add(string.Format("Game Size on Disk: {0}", Functions.FileSystem.FormatBytes(Game.sizeOnDisk))).Enabled = false;

                    // Spacer
                    rightClickMenu.MenuItems.Add("-");

                    // Play
                    rightClickMenu.MenuItems.Add("Play", mouseClick).Name = "rungameid";

                    // Spacer
                    rightClickMenu.MenuItems.Add("-");

                    // Backup (SLM) // disabled
                    rightClickMenu.MenuItems.Add("Backup (SLM)", mouseClick).Enabled = false;

                    // Backup (Steam)
                    rightClickMenu.MenuItems.Add("Backup (Steam)", mouseClick).Name = "backup";

                    // Defrag game files
                    rightClickMenu.MenuItems.Add("Defrag", mouseClick).Name = "defrag";

                    // Validate game files
                    rightClickMenu.MenuItems.Add("Validate Files", mouseClick).Name = "validate";

                    // Spacer
                    rightClickMenu.MenuItems.Add("-");

                    // Check system requirements
                    rightClickMenu.MenuItems.Add("Check System Requirements", mouseClick).Name = "checksysreqs";

                    // Open .acf file
                    rightClickMenu.MenuItems.Add("Open ACF file", mouseClick).Name = "acfFile";

                    // Spacer
                    rightClickMenu.MenuItems.Add("-");

                    // View on Store, opens in user browser not steam browser
                    rightClickMenu.MenuItems.Add("View on Store", mouseClick).Name = "Store";

                    // View on Disk, opens in explorer
                    rightClickMenu.MenuItems.Add("View on Disk", mouseClick).Name = "Disk";

                    // Uninstall, via Steam
                    rightClickMenu.MenuItems.Add("Uninstall", mouseClick).Name = "uninstall";

                    // Set our context menu to pictureBox
                    gameDetailBox.ContextMenu = rightClickMenu;

                    // Add our new game pictureBox to panel
                    Definitions.Accessors.MainForm.panel_GameList.Controls.Add(gameDetailBox);
                }
            }
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log errors to DirectoryRemoval.txt
                    Log.ErrorsToFile("Games", ex.ToString());
            }
        }

        static void gameDetailBox_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                // If clicked button is left (so it will not conflict with context menu)
                if (e.Button == MouseButtons.Left)
                {
                    // Define our picturebox from sender
                    PictureBox img = sender as PictureBox;

                    // Do drag & drop with our pictureBox
                    img.DoDragDrop(img, DragDropEffects.Move);
                }
            }
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log errors to DirectoryRemoval.txt
                    Log.ErrorsToFile("Games", ex.ToString());
            }
        }

        static void gameDetailBox_ContextMenuAction(object sender, EventArgs e)
        {
            try
            {
                // Define our game from the Tag we given to Context menu
                Definitions.List.GamesList Game = (sender as MenuItem).Parent.Tag as Definitions.List.GamesList;

                // switch based on name we set earlier with context menu
                switch ((sender as MenuItem).Name)
                {

                    // default use steam to act
                    // more details: https://developer.valvesoftware.com/wiki/Steam_browser_protocol
                    default:
                        Process.Start(string.Format("steam://{0}/{1}", (sender as MenuItem).Name, Game.appID));
                        break;

                    // Opens game store page in user browser
                    case "Store":
                        Process.Start(string.Format("http://store.steampowered.com/app/{0}", Game.appID));
                        break;
                    
                    // Opens game installation path in explorer
                    case "Disk":
                        Process.Start(Game.exactInstallPath);
                        break;

                        // Opens game acf file in default text viewer
                    case "acfFile":
                        Process.Start(Game.acfPath);
                        break;
                }

            }
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log errors to DirectoryRemoval.txt
                    Log.ErrorsToFile("Games", ex.ToString());
            }
        }

    }
}
