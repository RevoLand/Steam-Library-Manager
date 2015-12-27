using Steam_Library_Manager.Functions;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using Steam_Library_Manager.Languages.Forms.Games;
using System.Threading.Tasks;

namespace Steam_Library_Manager.Content
{
    class Games
    {
        public static async Task<Framework.PictureBoxWithCaching> generateGameBox(Definitions.List.GamesList Game)
        {
            // Define a new pictureBox for game
            Framework.PictureBoxWithCaching gameDetailBox = new Framework.PictureBoxWithCaching();

            // Set picture mode of pictureBox
            gameDetailBox.SizeMode = PictureBoxSizeMode.StretchImage;

            // Set game image size
            gameDetailBox.Size = Properties.Settings.Default.GamePictureBoxSize;

            // Load game header image asynchronously
            await Task.Run(() => gameDetailBox.LoadAsync(string.Format("https://steamcdn-a.akamaihd.net/steam/apps/{0}/header.jpg", Game.appID)));

            // Set error image in case of couldn't load game header image
            gameDetailBox.ErrorImage = Properties.Resources.no_image_available;

            // Space between pictureBoxes for better looking
            gameDetailBox.Margin = new Padding(20);

            // Set our game details as Tag to pictureBox
            gameDetailBox.Tag = Game;

            // On we click to pictureBox (drag & drop event)
            gameDetailBox.MouseDown += gameDetailBox_MouseDown;

            // If game is compressed
            if (Game.Compressed)
            {
                // Make a new picturebox
                PictureBox compressedIcon = new PictureBox();

                // Set picture box image to compressedLibraryIcon
                compressedIcon.Image = Properties.Resources.compressedLibraryIcon;

                // Put picturebox to right corner of game image
                compressedIcon.Left = Properties.Settings.Default.GamePictureBoxSize.Width - 20;
                compressedIcon.Top = 5;

                // Add icon to game picture
                gameDetailBox.Controls.Add(compressedIcon);
            }

            // Set our context menu to pictureBox
            gameDetailBox.ContextMenuStrip = generateRightClickMenu(Game);

            return gameDetailBox;
        }


        static void gameDetailBox_MouseDown(object sender, MouseEventArgs e)
        {
            // If clicked button is left (so it will not conflict with context menu)
            if (e.Button == MouseButtons.Left)
            {
                // Define our picturebox from sender
                Framework.PictureBoxWithCaching img = sender as Framework.PictureBoxWithCaching;

                // Do drag & drop with our pictureBox
                img.DoDragDrop(img, DragDropEffects.Move);
            }
        }

        public static ContextMenuStrip generateRightClickMenu(Definitions.List.GamesList Game)
        {
            // Create a new right click menu (context menu)
            ContextMenuStrip menu = new ContextMenuStrip();

            // Define an event handler
            EventHandler mouseClick = new EventHandler(gameDetailBox_ContextMenuAction);

            //rightClickMenu.Tag = Game;

            // Game name (appID) // disabled
            menu.Items.Add(string.Format(rightClickMenu.menuItem_gameNameWithAppID, Game.appName, Game.appID)).Enabled = false;

            // Game Size on Disk: 124MB // disabled
            menu.Items.Add(string.Format(rightClickMenu.menuItem_gameSizeOnDisk, Functions.FileSystem.FormatBytes(Game.sizeOnDisk))).Enabled = false;

            // Spacer
            menu.Items.Add(Definitions.SLM.Spacer);

            // Play
            menu.Items.Add(rightClickMenu.menuItem_play, null, mouseClick).Name = "rungameid";

            // Spacer
            menu.Items.Add(Definitions.SLM.Spacer);

            // Backup (Steam)
            menu.Items.Add(rightClickMenu.menuItem_backupUsingSteam, null, mouseClick).Name = "backup";

            // Defrag game files
            menu.Items.Add(rightClickMenu.menuItem_defrag, null, mouseClick).Name = "defrag";

            // Validate game files
            menu.Items.Add(rightClickMenu.menuItem_validateGameFiles, null, mouseClick).Name = "validate";

            // Spacer
            menu.Items.Add(Definitions.SLM.Spacer);

            // Check system requirements
            menu.Items.Add(rightClickMenu.menuItem_checkSysRequirements, null, mouseClick).Name = "checksysreqs";

            // Open .acf file
            menu.Items.Add(rightClickMenu.menuItem_openAcfFile, null, mouseClick).Name = "acfFile";

            // Spacer
            menu.Items.Add(Definitions.SLM.Spacer);

            // View on Store, opens in user browser not steam browser
            menu.Items.Add(rightClickMenu.menuItem_viewOnStoreWithSteam, null, mouseClick).Name = "store";

            // View on Store, opens in user browser not steam browser
            menu.Items.Add(rightClickMenu.menuItem_viewOnStore, null, mouseClick).Name = "uStore";

            // View on Disk, opens in explorer
            menu.Items.Add(rightClickMenu.menuItem_viewOnDisk, null, mouseClick).Name = "Disk";

            // Spacer
            menu.Items.Add(Definitions.SLM.Spacer);

            // Uninstall, via Steam
            menu.Items.Add(rightClickMenu.menuItem_uninstall, null, mouseClick).Name = "uninstall";

            //  Uninstall, via SLM
            menu.Items.Add(rightClickMenu.menuItem_uninstallWithSLM, null, mouseClick).Name = "uninstallSLM";

            return menu;
        }

        static async void gameDetailBox_ContextMenuAction(object sender, EventArgs e)
        {
            try
            {
                // Define our game from the Tag we given to Context menu
                Definitions.List.GamesList Game = ((sender as ToolStripMenuItem).Owner as ContextMenuStrip).SourceControl.Tag as Definitions.List.GamesList;

                // switch based on name we set earlier with context menu
                switch ((sender as ToolStripMenuItem).Name)
                {

                    // default use steam to act
                    // more details: https://developer.valvesoftware.com/wiki/Steam_browser_protocol
                    default:
                        Process.Start(string.Format("steam://{0}/{1}", (sender as ToolStripMenuItem).Name, Game.appID));
                        break;

                    // Opens game store page in user browser
                    case "uStore":
                        Process.Start(string.Format("http://store.steampowered.com/app/{0}", Game.appID));
                        break;

                    // Opens game installation path in explorer
                    case "Disk":
                        Process.Start(Game.commonPath);
                        break;

                    // Opens game acf file in default text viewer
                    case "acfFile":
                        Process.Start(Properties.Settings.Default.DefaultTextEditor, Game.acfPath);
                        break;
                    case "uninstallSLM":

                        DialogResult areYouSure = MessageBox.Show(string.Format(rightClickMenu.message_sureToRemoveGame, Game.appName), rightClickMenu.messageTitle_sureToRemoveGame, MessageBoxButtons.YesNoCancel);

                        if (areYouSure == DialogResult.Yes)
                        {
                            Functions.Games gameFunctions = new Functions.Games();
                            if (await gameFunctions.deleteGameFiles(Game))
                            {
                                SteamLibrary.updateLibraryList();
                                SteamLibrary.updateMainForm();
                                Functions.Games.UpdateGameList(Game.Library);

                                MessageBox.Show(string.Format(rightClickMenu.message_appRemovedAsRequested, Game.appName));
                            }
                        }
                        break;
                }

            }
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log errors to DirectoryRemoval.txt
                    Log.ErrorsToFile(rightClickMenu.Games, ex.ToString());
            }
        }

    }
}
