using System;
using System.Linq;
using System.Windows.Forms;

namespace Steam_Library_Manager.Content
{
    class Libraries
    {
        public static PictureBox generateLibraryBox(Definitions.List.LibraryList Library)
        {
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
            libraryName.Text = string.Format(Languages.SteamLibrary.label_libraryName, Library.fullPath, Library.GameCount, Functions.FileSystem.FormatBytes(Functions.FileSystem.getAvailableFreeSpace(Library.fullPath)));

            // Show our label in bottom center of our pictureBox
            libraryName.TextAlign = System.Drawing.ContentAlignment.BottomCenter;

            // Set our label background color to transparent, actually we may try using a color in future
            libraryName.BackColor = System.Drawing.Color.Transparent;

            // Set our font to Segoe UI Semilight for better looking, all suggestions are welcome
            libraryName.Font = new System.Drawing.Font("Segoe UI Semilight", 8);
            libraryName.Top = Properties.Settings.Default.libraryPictureSize.Height - 15;

            // Add our label to pictureBox
            libraryDetailBox.Controls.Add(libraryName);

            // Add our right click (context) menu to pictureBox
            libraryDetailBox.ContextMenuStrip = generateRightClickMenu(Library);

            return libraryDetailBox;
        }

        public static ContextMenuStrip generateRightClickMenu(Definitions.List.LibraryList Library)
        {
            // Create a new right click menu (aka context menu)
            ContextMenuStrip menu = new ContextMenuStrip();

            EventHandler clickHandler = new EventHandler(libraryDetailBox_ContextMenuAction);

            // Add an item which will show our library directory and make it disabled
            menu.Items.Add(Languages.Forms.Libraries.rightClickMenu.menuItem_openLibraryInExplorer, null, clickHandler).Name = "Disk";

            // spacer
            menu.Items.Add(Definitions.SLM.Spacer);

            // Move library
            menu.Items.Add(Languages.Forms.Libraries.rightClickMenu.menuItem_moveLibrary, null, clickHandler).Name = "moveLibrary";

            // spacer
            menu.Items.Add(Definitions.SLM.Spacer);

            // Refresh games in library
            menu.Items.Add(Languages.Forms.Libraries.rightClickMenu.menuItem_refreshGameList, null, clickHandler).Name = "RefreshGameList";

            // spacer
            menu.Items.Add(Definitions.SLM.Spacer);

            // Delete library
            menu.Items.Add(Languages.Forms.Libraries.rightClickMenu.menuItem_deleteLibrary, null, clickHandler).Name = "deleteLibrary";

            // Delete games in library
            menu.Items.Add(Languages.Forms.Libraries.rightClickMenu.menuItem_deleteGamesInLibrary, null, clickHandler).Name = "deleteLibrarySLM";

            if (Library.Backup)
            {
                // Spacer
                menu.Items.Add(Definitions.SLM.Spacer);

                // Remove the library from slm (only from list)
                menu.Items.Add(Languages.Forms.Libraries.rightClickMenu.menuITem_removeBackupLibraryFromList).Name = "RemoveFromList";
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
                        DialogResult moveGamesBeforeDeletion = MessageBox.Show(Languages.Forms.Libraries.rightClickMenu.message_moveGamesInLibraryBeforeDelete, Languages.Forms.Libraries.rightClickMenu.messageTitle_moveGamesInLibraryBeforeDelete, MessageBoxButtons.YesNoCancel);

                        if (moveGamesBeforeDeletion == DialogResult.Yes)
                            new Forms.moveLibrary(Library).Show();
                        else if (moveGamesBeforeDeletion == DialogResult.No)
                            Functions.SteamLibrary.removeLibrary(Library, true);
                        break;
                    case "deleteLibrarySLM":
                        foreach (Definitions.List.GamesList Game in Definitions.List.Game.Where(x => x.Library == Library))
                        {
                            Functions.FileSystem.Game gameFunctions = new Functions.FileSystem.Game();

                            if (!await gameFunctions.deleteGameFiles(Game))
                            {
                                MessageBox.Show(string.Format(Languages.Forms.Libraries.rightClickMenu.message_unknownErrorWhileDeletingGames, Library.fullPath));

                                return;
                            }
                        }

                        Functions.SteamLibrary.updateLibraryList();
                        Functions.SteamLibrary.updateMainForm();
                        Functions.Games.UpdateGameList(Library);

                        MessageBox.Show(string.Format(Languages.Forms.Libraries.rightClickMenu.message_allGamesSuccesfullyDeleted, Library.fullPath, Environment.NewLine));
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
                    Functions.Log.ErrorsToFile(Languages.Forms.Libraries.rightClickMenu.Libraries, ex.ToString());
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
                if (Definitions.SLM.LatestSelectedLibrary == Library && Definitions.SLM.LatestSelectedLibrary.GameCount == Library.GameCount && Definitions.Accessors.MainForm.panel_GameList.Controls.Count == Library.GameCount) return;

                // Update latest selected library
                Definitions.SLM.LatestSelectedLibrary = Library;

                // Update games list from current selection
                Functions.Games.UpdateMainForm(null, null, Library);
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
