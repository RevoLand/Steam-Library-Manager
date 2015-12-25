using Steam_Library_Manager.Functions;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Steam_Library_Manager.Content
{
    class Games
    {
        public static ContextMenuStrip generateRightClickMenu(Definitions.List.GamesList Game)
        {
            // Create a new right click menu (context menu)
            ContextMenuStrip rightClickMenu = new ContextMenuStrip();

            // Define an event handler
            EventHandler mouseClick = new EventHandler(gameDetailBox_ContextMenuAction);

            //rightClickMenu.Tag = Game;

            // Game name (appID) // disabled
            rightClickMenu.Items.Add(string.Format("{0} (ID: {1})", Game.appName, Game.appID)).Enabled = false;

            // Game Size on Disk: 124MB // disabled
            rightClickMenu.Items.Add(string.Format("Game Size on Disk: {0}", Functions.FileSystem.FormatBytes(Game.sizeOnDisk))).Enabled = false;

            // Spacer
            rightClickMenu.Items.Add(Definitions.SLM.Spacer);

            // Play
            rightClickMenu.Items.Add("Play", null, mouseClick).Name = "rungameid";

            // Spacer
            rightClickMenu.Items.Add(Definitions.SLM.Spacer);

            // Backup (SLM) // disabled
            rightClickMenu.Items.Add("Backup (SLM)", null, mouseClick).Enabled = false;

            // Backup (Steam)
            rightClickMenu.Items.Add("Backup (Steam)", null, mouseClick).Name = "backup";

            // Defrag game files
            rightClickMenu.Items.Add("Defrag", null, mouseClick).Name = "defrag";

            // Validate game files
            rightClickMenu.Items.Add("Validate Files", null, mouseClick).Name = "validate";

            // Spacer
            rightClickMenu.Items.Add(Definitions.SLM.Spacer);

            // Check system requirements
            rightClickMenu.Items.Add("Check System Requirements", null, mouseClick).Name = "checksysreqs";

            // Open .acf file
            rightClickMenu.Items.Add("Open ACF file", null, mouseClick).Name = "acfFile";

            // Spacer
            rightClickMenu.Items.Add(Definitions.SLM.Spacer);

            // View on Store, opens in user browser not steam browser
            rightClickMenu.Items.Add("View on Store (Steam Client)", null, mouseClick).Name = "store";

            // View on Store, opens in user browser not steam browser
            rightClickMenu.Items.Add("View on Store", null, mouseClick).Name = "uStore";

            // View on Disk, opens in explorer
            rightClickMenu.Items.Add("View on Disk", null, mouseClick).Name = "Disk";

            // Spacer
            rightClickMenu.Items.Add(Definitions.SLM.Spacer);

            // Uninstall, via Steam
            rightClickMenu.Items.Add("Uninstall", null, mouseClick).Name = "uninstall";

            //  Uninstall, via SLM
            rightClickMenu.Items.Add("Uninstall (SLM)", null, mouseClick).Name = "uninstallSLM";

            return rightClickMenu;
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
                        Process.Start(string.Format("steam://{0}/{1}", (sender as MenuItem).Name, Game.appID));
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
                        Functions.Games gameFunctions = new Functions.Games();
                        if (await gameFunctions.deleteGameFiles(Game))
                        {
                            SteamLibrary.updateLibraryList();
                            SteamLibrary.updateMainForm();
                            Functions.Games.UpdateGameList(Game.Library);

                            MessageBox.Show("Successfully removed");
                        }

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
