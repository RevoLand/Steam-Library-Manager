using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
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

        CancellationTokenSource processCancelation = new CancellationTokenSource();
        bool isWorkingCurrently = false;

        public moveGame(Definitions.List.GamesList gameToMove, Definitions.List.LibraryList libraryToMove)
        {
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(Properties.Settings.Default.defaultLanguage);

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
            loadForm();
        }

        void loadForm()
        {
            isWorkingCurrently = false;

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
                button_Copy.Text = Languages.Forms.moveGame.button_copyText_Restore;

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
                button_Copy.Text = Languages.Forms.moveGame.button_copyText_Backup;
            }

            Text = string.Format(Languages.Forms.moveGame.form_Text, Game.appName);

            // Load our game image asynchronously
            pictureBox_GameImage.LoadAsync(string.Format("http://cdn.akamai.steamstatic.com/steam/apps/{0}/header.jpg", Game.appID));

            // Update our label for current library with directory path
            linkLabel_currentLibrary.Text = Game.Library.steamAppsPath;

            // Update our label for target library with directory path
            linkLabel_TargetLibrary.Text = Library.steamAppsPath;

            // Get free space at target library and update Available space label
            label_AvailableSpace.Text = Functions.FileSystem.FormatBytes(Functions.FileSystem.getAvailableFreeSpace(Library.steamAppsPath));

            // Get game size and update Needed space label
            label_NeededSpace.Text = Functions.FileSystem.FormatBytes(Game.sizeOnDisk);

            if (Properties.Settings.Default.methodForMovingGame == "forEach")
                label_movedFileSize.Visible = false;
        }

        private void button_Copy_Click(object sender, EventArgs e)
        {
            if (!isWorkingCurrently)
            {
                isWorkingCurrently = true;

                button_Copy.Text = Languages.Forms.moveGame.button_copyText_Cancel;

                // Call our function and start the process, also provide things like validate, compress, backup etc 
                // so after clicking button, changes made on form will not affect the process
                CopyGame(checkbox_Validate.Checked, checkbox_RemoveOldFiles.Checked, checkbox_Compress.Checked, checkbox_DeCompress.Checked, Game.Compressed);
            }
            else
            {
                processCancelation.Cancel();

                loadForm();
            }
        }

        async void CopyGame(bool Validate, bool RemoveOldFiles, bool Compress, bool deCompressArchive, bool isGameCompressed)
        {
            Stopwatch timeElapsed = new Stopwatch();
            Functions.FileSystem.Game gameFunctions = new Functions.FileSystem.Game();
            List<string> gameFiles = new List<string>();

            timeElapsed.Start();

            if (!Game.Compressed)
            {
                logToForm(Languages.Forms.moveGame.logMessage_generatingFileList);

                gameFiles.AddRange(await gameFunctions.getFileList(Game, true, true));

                logToForm(string.Format(Languages.Forms.moveGame.logMessage_fileListGenerated, gameFiles.Count));
            }

            // Name definitions
            string zipName = $"{Game.appID}.zip";
            string currentZipNameNpath = Path.Combine(Game.Library.steamAppsPath, zipName);
            string newZipNameNpath = Path.Combine(Library.steamAppsPath, zipName);

            // Define free space we have at target libray
            long freeSpaceOnTargetDisk = Functions.FileSystem.getAvailableFreeSpace(Library.fullPath);

            // If free space is less than game size
            if (freeSpaceOnTargetDisk < Game.sizeOnDisk)
            {
                // Show an error to user
                logToForm(string.Format(Languages.Forms.moveGame.logError_freeSpaceIsNotEnough, Game.sizeOnDisk, freeSpaceOnTargetDisk));

                // And cancel the process
                return;
            }

            // If game is compressed, target library is not backup OR game is compressed and de-compress requested
            if (isGameCompressed && !Library.Backup || isGameCompressed && deCompressArchive)
            {
                if (!await gameFunctions.decompressArchive(this, currentZipNameNpath, Game, Library))
                {
                    logToForm(Languages.Forms.moveGame.logError_unknownErrorWhileDecompressing);

                    return;
                }
            }
            // If game is compressed and user didn't wanted to decompress and target library is backup
            else if (isGameCompressed && !deCompressArchive && Library.Backup)
            {
                if (!await gameFunctions.copyGameArchive(this, currentZipNameNpath, newZipNameNpath))
                {
                    logToForm(Languages.Forms.moveGame.logError_unknownErrorWhileCopyingArchive);

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
                        logToForm(Languages.Forms.moveGame.logError_unknownErrorWhileCompressing);

                        return;
                    }

                }
                // If game will not be compressed
                else
                {
                    int copyResult = 0;

                    if (Properties.Settings.Default.methodForMovingGame == "forEach")
                        copyResult = await gameFunctions.copyGameFiles(this, gameFiles, Game, Library, Validate, processCancelation.Token);
                    else
                        copyResult = await gameFunctions.copyGameFilesNew(this, gameFiles, Game, Library, Validate, processCancelation.Token);

                    if (copyResult == 0)
                    {
                        logToForm(Languages.Forms.moveGame.logError_unknownErrorWhileCopyingFiles);

                        return;
                    }
                    else if (copyResult == -1)
                    {
                        logToForm(Languages.Forms.moveGame.logMessage_userCanceledProcess);

                        return;
                    }
                }
            }

            if (RemoveOldFiles)
            {
                if (!await gameFunctions.deleteGameFiles(Game, gameFiles))
                {
                    logToForm(Languages.Forms.moveGame.logError_unknownErrorWhileRemovingFiles);

                    return;
                }
            }

            // Update button text
            button_Copy.Text = Languages.Forms.moveGame.button_copyText_Completed;
            button_Copy.Enabled = false;
            timeElapsed.Stop();

            // Log to user
            logToForm(Languages.Forms.moveGame.logMessage_processCompleted);
            logToForm(string.Format(Languages.Forms.moveGame.logMessage_timeElapsed, timeElapsed.Elapsed));

            // Update game libraries
            Functions.SteamLibrary.updateLibraryList();
        }

        void logToForm(string Text)
        {
            // Append log to textbox
            textBox_Logs.AppendText(Text + Environment.NewLine);
        }

        public async void logToFormAsync(string Text)
        {
            await Task.Run(() => MainForm.SafeInvoke(textBox_Logs, () => textBox_Logs.AppendText(Text + Environment.NewLine)));
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
