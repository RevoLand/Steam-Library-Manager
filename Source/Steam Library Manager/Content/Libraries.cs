using System;
using System.Linq;
using System.Windows.Forms;
using Steam_Library_Manager.Languages.Forms.Libraries;

namespace Steam_Library_Manager.Content
{
    class Libraries
    {

        public static ContextMenuStrip generateRightClickMenu(Definitions.List.LibraryList Library)
        {
            // Create a new right click menu (aka context menu)
            ContextMenuStrip menu = new ContextMenuStrip();

            EventHandler clickHandler = new EventHandler(libraryDetailBox_ContextMenuAction);

            // Add an item which will show our library directory and make it disabled
            menu.Items.Add(rightClickMenu.menuItem_openLibraryInExplorer, null, clickHandler).Name = "Disk";

            // spacer
            menu.Items.Add(Definitions.SLM.Spacer);

            // Move library
            menu.Items.Add(rightClickMenu.menuItem_moveLibrary, null, clickHandler).Name = "moveLibrary";

            // spacer
            menu.Items.Add(Definitions.SLM.Spacer);

            // Refresh games in library
            menu.Items.Add(rightClickMenu.menuItem_refreshGameList, null, clickHandler).Name = "RefreshGameList";

            // spacer
            menu.Items.Add(Definitions.SLM.Spacer);

            // Delete library
            menu.Items.Add(rightClickMenu.menuItem_deleteLibrary, null, clickHandler).Name = "deleteLibrary";

            // Delete games in library
            menu.Items.Add(rightClickMenu.menuItem_deleteGamesInLibrary, null, clickHandler).Name = "deleteLibrarySLM";

            if (Library.Backup)
            {
                // Spacer
                menu.Items.Add(Definitions.SLM.Spacer);

                // Remove the library from slm (only from list)
                menu.Items.Add(rightClickMenu.menuITem_removeBackupLibraryFromList).Name = "RemoveFromList";
            }

            return menu;
        }

        static async void libraryDetailBox_ContextMenuAction(object sender, EventArgs e)
        {
            try
            {
                // Define our game from the Tag we given to Context menu
                Definitions.List.LibraryList Library = ((sender as ToolStripMenuItem).Owner as ContextMenuStrip).SourceControl.Tag as Definitions.List.LibraryList;

                // switch based on name we set earlier with context menu
                switch ((sender as ToolStripMenuItem).Name)
                {
                    // Opens game installation path in explorer
                    case "Disk":
                        System.Diagnostics.Process.Start(Library.steamAppsPath);
                        break;
                    case "RefreshGameList":
                        Functions.Games.UpdateGameList(Library);
                        break;
                    case "deleteLibrary":
                        DialogResult moveGamesBeforeDeletion = MessageBox.Show(rightClickMenu.message_moveGamesInLibraryBeforeDelete, rightClickMenu.messageTitle_moveGamesInLibraryBeforeDelete, MessageBoxButtons.YesNoCancel);

                        if (moveGamesBeforeDeletion == DialogResult.Yes)
                            new Forms.moveLibrary(Library).Show();
                        else if (moveGamesBeforeDeletion == DialogResult.No)
                            Functions.SteamLibrary.removeLibrary(Library, true);
                        break;
                    case "deleteLibrarySLM":
                        foreach (Definitions.List.GamesList Game in Definitions.List.Game.Where(x => x.Library == Library))
                        {
                            Functions.Games gameFunctions = new Functions.Games();

                            if (!await gameFunctions.deleteGameFiles(Game))
                            {
                                MessageBox.Show(string.Format(rightClickMenu.message_unknownErrorWhileDeletingGames, Library.fullPath));

                                return;
                            }
                        }

                        Functions.SteamLibrary.updateLibraryList();
                        Functions.SteamLibrary.updateMainForm();
                        Functions.Games.UpdateGameList(Library);

                        MessageBox.Show(string.Format(rightClickMenu.message_allGamesSuccesfullyDeleted, Library.fullPath));
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
                            Functions.Settings.updateBackupDirs();

                            // Update main form with new settings
                            Functions.SteamLibrary.updateMainForm();
                        }
                        break;
                }

            }
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log errors to DirectoryRemoval.txt
                    Functions.Log.ErrorsToFile(rightClickMenu.Libraries, ex.ToString());
            }
        }

    }
}
