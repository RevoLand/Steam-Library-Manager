using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Steam_Library_Manager.Forms
{
    partial class moveGame : Form
    {
        // Define our game from LatestSelectedGame
        Definitions.List.GamesList Game;

        // Define our library from LatestDropLibrary
        Definitions.List.LibraryList Library;

        public moveGame(Definitions.List.GamesList gameToMove, Definitions.List.LibraryList libraryToMove)
        {
            InitializeComponent();

            // Set our form icon
            Icon = Properties.Resources.steam_icon;

            // Set an error image for pictureBox (game image)
            pictureBox_GameImage.ErrorImage = Properties.Resources.no_image_available;

            Game = gameToMove;
            Library = libraryToMove;
        }

        // On MoveGame form load
        private void MoveGame_Load(object sender, EventArgs e)
        {
            // If target library is backup library and game is compressed
            if (Library.Backup && Game.Compressed)
            {
                checkbox_Validate.Visible = false;
                checkbox_DeCompress.Visible = true;
            }
            // Else if, Game library is backup library and target library is not backup library
            else if (Game.Library.Backup && !Library.Backup)
            {
                checkbox_Validate.Visible = false;
                button_Copy.Text = "Restore";

                // If game is compressed, set De-compress checkbox visible, set checkbox checked and set checkbox disabled
                if (Game.Compressed)
                {
                    checkbox_DeCompress.Visible = true;
                    checkbox_DeCompress.Checked = true;
                    checkbox_DeCompress.Enabled = false;
                }
            }
            // else if, Target library is backup library
            else if (Library.Backup)
            {
                checkbox_Compress.Visible = true;
                button_Copy.Text = "Backup";
            }

            Text = string.Format("{0} - SLM", Game.appName);

            // Load our game image asynchronously
            pictureBox_GameImage.LoadAsync(string.Format("http://cdn.akamai.steamstatic.com/steam/apps/{0}/header.jpg", Game.appID));

            // Update our label for current library with directory path
            linkLabel_currentLibrary.Text = Game.Library.steamAppsPath;

            // Update our label for target library with directory path
            linkLabel_TargetLibrary.Text = Library.steamAppsPath;

            // Get free space at target library and update Available space label
            label_AvailableSpace.Text = Functions.FileSystem.FormatBytes(Functions.FileSystem.GetFreeSpace(Library.steamAppsPath));

            // Get game size and update Needed space label
            label_NeededSpace.Text = Functions.FileSystem.FormatBytes(Game.sizeOnDisk);
        }

        private void button_Copy_Click(object sender, EventArgs e)
        {
            // Disable button to prevent bugs or missclicks
            button_Copy.Enabled = false;

            // Call our function and start the process, also provide things like validate, compress, backup etc 
            // so after clicking button, changes made on form will not affect the process
            CopyGame(checkbox_Validate.Checked, checkbox_RemoveOldFiles.Checked, checkbox_Compress.Checked, checkbox_DeCompress.Checked, Game.Compressed);
        }

        async void CopyGame(bool Validate, bool RemoveOldFiles, bool Compress, bool deCompressArchive, bool isGameCompressed)
        {
            Functions.Games gameFunctions = new Functions.Games();
            List<string> gameFiles = new List<string>();

            if (!Game.Compressed)
            {
                Log("Please wait while SLM generates file list.");

                gameFiles.AddRange(await gameFunctions.getFileList(Game, true, true));

                Log($"File list generated. Total file count will be moved: {gameFiles.Count}");
            }

            Stopwatch timeElapsed = new Stopwatch();
            timeElapsed.Start();

            // Path definitions
            string newCommonPath = Path.Combine(Library.commonPath, Game.installationPath);

            // Name definitions
            string zipName = $"{Game.appID}.zip";
            string currentZipNameNpath = Path.Combine(Game.Library.steamAppsPath, zipName);
            string newZipNameNpath = Path.Combine(Library.steamAppsPath, zipName);

            // Define free space we have at target libray
            long freeSpaceOnTargetDisk = Functions.FileSystem.GetFreeSpace(newCommonPath);

            // If free space is less than game size
            if (freeSpaceOnTargetDisk < Game.sizeOnDisk)
            {
                // Show an error to user
                Log(string.Format("Free space is not enough! Needed Free Space: {0} Available: {1}", Game.sizeOnDisk, freeSpaceOnTargetDisk));

                // And cancel the process
                return;
            }

            // If game is compressed, target library is not backup OR game is compressed and de-compress requested
            if (isGameCompressed && !Library.Backup || isGameCompressed && deCompressArchive)
            {
                if (!await gameFunctions.decompressArchive(this, newCommonPath, currentZipNameNpath, Game, Library))
                {
                    Log("An error happened while de-compressing archive");

                    return;
                }
            }
            // If game is compressed and user didn't wanted to decompress and target library is backup
            else if (isGameCompressed && !deCompressArchive && Library.Backup)
            {
                if (!await gameFunctions.copyGameArchive(this, currentZipNameNpath, newZipNameNpath))
                {
                    Log("An error happened while copying game archive");

                    return;
                }
            }
            else
            {
                // Set progress bar maximum value to FilesToMove
                progressBar_CopyStatus.Maximum = gameFiles.Count;

                // If game will be compressed
                if (Compress)
                {
                    if (!await gameFunctions.compressGameFiles(this, gameFiles, newZipNameNpath, Game, Library))
                    {
                        Log("An error happened while compressing game files");

                        return;
                    }

                }
                // If game will not be compressed
                else
                {
                    if (!await gameFunctions.copyGameFiles(this, gameFiles, newCommonPath, Game, Library, Validate))
                    {
                        Log("An error happened while coping game files");

                        return;
                    }
                }
            }

            try
            {
                if (RemoveOldFiles)
                {
                    if (!await gameFunctions.deleteGameFiles(Game, gameFiles))
                    {
                        Log("An error happened while removing old files");

                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log errors to DirectoryRemoval.txt
                    Functions.Log.ErrorsToFile("DirectoryRemoval", ex.ToString());
            }

            // stop our stopwatch
            timeElapsed.Stop();

            // Update button text
            button_Copy.Text = "Completed!";

            // Log to user
            Log("Process has been completed, you may close this window now.");
            Log(string.Format("Time elapsed: {0}", timeElapsed.Elapsed));

            // Update game libraries
            Functions.SteamLibrary.updateLibraryList();
        }

        public void Log(string Text)
        {
            try
            {
                // Append log to textbox
                textBox_Logs.AppendText(Text + Environment.NewLine);
            }
            catch { }
        }

        // On click to game image
        private void pictureBox_GameImage_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                switch (e.Button)
                {
                    default:
                        // Open store in user browser
                        Process.Start(string.Format("http://store.steampowered.com/app/{0}/", Game.appID));
                        break;

                    // Right click
                    case MouseButtons.Right:
                        // Open game installation directory in explorer
                        Process.Start(Game.commonPath);
                        break;
                }
            }
            catch { }
        }

        private void linkLabel_TargetLibrary_Click(object sender, EventArgs e)
        {
            try
            {
                // On click to target library, open library in explorer
                Process.Start(Library.steamAppsPath);
            }
            catch { }
        }

        private void linkLabel_currentLibrary_Click(object sender, EventArgs e)
        {
            try
            {
                // On click to current library, open library in explorer
                Process.Start(Game.Library.steamAppsPath);
            }
            catch { }
        }
    }
}
