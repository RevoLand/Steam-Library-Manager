using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Steam_Library_Manager.Forms
{
    public partial class MoveGame : Form
    {
        // Define our game from LatestSelectedGame
        Definitions.List.GamesList Game = Definitions.SLM.LatestSelectedGame;

        // Define our library from LatestDropLibrary
        Definitions.List.LibraryList Library = Definitions.SLM.LatestDropLibrary;

        public MoveGame()
        {
            InitializeComponent();

            // Set our form icon
            Icon = Properties.Resources.steam_icon;

            // Set an error image for pictureBox (game image)
            pictureBox_GameImage.ErrorImage = Properties.Resources.no_image_available;
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

            // Load our game image asynchronously
            pictureBox_GameImage.LoadAsync(string.Format("http://cdn.akamai.steamstatic.com/steam/apps/{0}/header.jpg", Game.appID));

            // Update our label for current library with directory path
            linkLabel_currentLibrary.Text = Game.Library.Directory;

            // Update our label for target library with directory path
            linkLabel_TargetLibrary.Text = Library.Directory;

            // Get free space at target library and update Available space label
            label_AvailableSpace.Text = Functions.FileSystem.FormatBytes(Functions.FileSystem.GetFreeSpace(Library.Directory));

            // Get game size and update Needed space label
            label_NeededSpace.Text = Functions.FileSystem.FormatBytes(Game.sizeOnDisk);
        }

        private void button_Copy_Click(object sender, EventArgs e)
        {
            try
            {
                // Disable button to prevent bugs or missclicks
                button_Copy.Enabled = false;

                // Call our function and start the process, also provide things like validate, compress, backup etc 
                // so after clicking button, changes made on form will not affect the process
                CopyGame(checkbox_Validate.Checked, checkbox_RemoveOldFiles.Checked, checkbox_Compress.Checked, checkbox_DeCompress.Checked, Game.Compressed);
            }
            catch { }
        }

        async void CopyGame(bool Validate, bool RemoveOldFiles, bool Compress, bool deCompressArchive, bool isGameCompressed)
        {
            Stopwatch timeElapsed = new Stopwatch();
            timeElapsed.Start();

            // Path definitions
            string newCommonPath = Path.Combine(Library.Directory, "common", Game.installationPath);
            string newDownloadingPath = Path.Combine(Library.Directory, "downloading", Game.appID.ToString());

            // Name definitions
            string zipName = string.Format("{0}.zip", Game.appID);
            string currentZipNameNpath = Path.Combine(Game.Library.Directory, zipName);
            string newZipNameNpath = Path.Combine(Library.Directory, zipName);
            string newFileName;

            // Other definitions
            byte[] currentFileMD5, newFileMD5;
            int FilesToMove = 0, TotalMovedFiles = 0;

            try
            {
                // If game is compressed, target library is not backup OR game is compressed and de-compress requested
                if (isGameCompressed && !Library.Backup || isGameCompressed && deCompressArchive)
                {
                    // If directory exists at target game path
                    if (Directory.Exists(newCommonPath))
                    {
                        // Remove the directory
                        Directory.Delete(newCommonPath, true);
                        File.Delete(Path.Combine(Library.Directory, Game.acfName));
                    }

                    // Log to user
                    Log("Uncompressing archive... Please wait");

                    // unzip the archive asynchronously to target library
                    await Task.Run(() => ZipFile.ExtractToDirectory(currentZipNameNpath, Library.Directory));
                }
                // If game is compressed and user didn't wanted to decompress and target library is backup
                else if (isGameCompressed && !deCompressArchive && Library.Backup)
                {
                    // If archive already exists in the target library
                    if (File.Exists(newZipNameNpath))
                        // Remove the compressed archive
                        File.Delete(newZipNameNpath);

                    // Copy the archive asynchronously
                    await Task.Run(() => File.Copy(currentZipNameNpath, newZipNameNpath));
                }
                else
                {
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

                    // If game has common folder
                    if (!string.IsNullOrEmpty(Game.commonPath))
                        // Increase FilesToMove based on file count in common folder
                        FilesToMove += Directory.GetFiles(Game.commonPath, "*", SearchOption.AllDirectories).Length;

                    // If game has downloading folder
                    if (!string.IsNullOrEmpty(Game.downloadPath))
                        // Increase FilesToMove based on file count in "downloading" folder
                        FilesToMove += Directory.GetFiles(Game.downloadPath, "*", SearchOption.AllDirectories).Length;

                    // If game has workshop folder
                    if (!string.IsNullOrEmpty(Game.workShopPath))
                        // Increase FilesToMove based on file count in workshop folder of game
                        FilesToMove += Directory.GetFiles(Game.workShopPath, "*", SearchOption.AllDirectories).Length;

                    // Set progress bar maximum value to FilesToMove
                    progressBar_CopyStatus.Maximum = FilesToMove;

                    #region Compres game
                    // If game will be compressed
                    if (Compress)
                    {
                        // If compressed archive already exists
                        if (File.Exists(newZipNameNpath))
                            // Remove the compressed archive
                            File.Delete(newZipNameNpath);

                        // Create a new compressed archive at target library
                        using (ZipArchive gameBackup = ZipFile.Open(newZipNameNpath, ZipArchiveMode.Create))
                        {
                            // If game has common folder
                            if (!string.IsNullOrEmpty(Game.commonPath))
                            {
                                // For each file in common folder of game
                                foreach (string currentFile in Directory.EnumerateFiles(Game.commonPath, "*", SearchOption.AllDirectories))
                                {
                                    // Define a string for better looking
                                    newFileName = currentFile.Substring(Game.Library.Directory.Length + 1);

                                    // Add file to archive
                                    await Task.Run(() => gameBackup.CreateEntryFromFile(currentFile, newFileName, CompressionLevel.Optimal));

                                    // Increase movedFiles
                                    TotalMovedFiles += 1;

                                    // Perform step on progressBar
                                    progressBar_CopyStatus.PerformStep();

                                    // Log details about process
                                    Log(string.Format("[{0}/{1}] Compressed: {2}", TotalMovedFiles, FilesToMove, newFileName));
                                }
                            }

                            // If game has downloading folder
                            if (!string.IsNullOrEmpty(Game.downloadPath))
                            {
                                // For each file in downloading folder of game
                                foreach (string currentFile in Directory.EnumerateFiles(Game.downloadPath, "*", SearchOption.AllDirectories))
                                {
                                    // Define a string for better looking
                                    newFileName = currentFile.Substring(Game.Library.Directory.Length + 1);

                                    // Add file to archive
                                    await Task.Run(() => gameBackup.CreateEntryFromFile(currentFile, newFileName, CompressionLevel.Optimal));

                                    // Increase movedFiles
                                    TotalMovedFiles += 1;

                                    // Perform step on progressBar
                                    progressBar_CopyStatus.PerformStep();

                                    // Log details about process
                                    Log(string.Format("[{0}/{1}] Compressed: {2}", TotalMovedFiles, FilesToMove, newFileName));
                                }

                                // If game has .patch files in downloading folder
                                foreach (string currentFile in Directory.EnumerateFiles(Game.Library.downloadPath, string.Format("*{0}*.patch", Game.appID), SearchOption.TopDirectoryOnly))
                                {
                                    // Define a string for better looking
                                    newFileName = currentFile.Substring(Game.Library.Directory.Length + 1);

                                    // Add file to archive
                                    await Task.Run(() => gameBackup.CreateEntryFromFile(currentFile, newFileName, CompressionLevel.Optimal));
                                }
                            }

                            // If game has workshop files
                            if (!string.IsNullOrEmpty(Game.workShopPath))
                            {
                                // For each file in workshop folder of game
                                foreach (string currentFile in Directory.EnumerateFiles(Game.workShopPath, "*", SearchOption.AllDirectories))
                                {
                                    // Define a string for better looking
                                    newFileName = currentFile.Substring(Game.Library.Directory.Length + 1);

                                    // Add file to archive
                                    await Task.Run(() => gameBackup.CreateEntryFromFile(currentFile, newFileName, CompressionLevel.Optimal));

                                    // Increase movedFiles
                                    TotalMovedFiles += 1;

                                    // Perform step on progressBar
                                    progressBar_CopyStatus.PerformStep();

                                    // Log details about process
                                    Log(string.Format("[{0}/{1}] Compressed: {2}", TotalMovedFiles, FilesToMove, newFileName));

                                    // Add Workshop .ACF file to archive
                                    await Task.Run(() => gameBackup.CreateEntryFromFile(Game.workShopAcfPath, Path.Combine("workshop", Game.workShopAcfName), CompressionLevel.Optimal));

                                    // Log workshop .ACF file
                                    Log("Workshop .ACF file has been compressed");
                                }
                            }

                            // Add .ACF file to archive
                            await Task.Run(() => gameBackup.CreateEntryFromFile(Game.acfPath, Game.acfName, CompressionLevel.Optimal));

                            // Log .ACF file
                            Log(".ACF file has been compressed");
                        }
                    }
                    #endregion
                    // If game will not be compressed
                    else
                    {
                        // If directory not exists
                        if (!Directory.Exists(newCommonPath))
                            // Create the game directory at target library
                            Directory.CreateDirectory(newCommonPath);

                        // If game has common folder
                        if (!string.IsNullOrEmpty(Game.commonPath))
                        {
                            // For each file in common folder of game
                            foreach (string currentFile in Directory.EnumerateFiles(Game.commonPath, "*", SearchOption.AllDirectories))
                            {
                                // Make a new file stream from the file we are reading so we can copy the file asynchronously
                                using (FileStream currentFileStream = File.OpenRead(currentFile))
                                {
                                    // Set new file name including target game path
                                    newFileName = Path.Combine(newCommonPath, currentFile.Replace(Game.commonPath, ""));

                                    // If directory not exists
                                    if (!Directory.Exists(Path.GetDirectoryName(newFileName)))
                                        // Create a directory at target library for new file, if we do not the process will fail
                                        Directory.CreateDirectory(Path.GetDirectoryName(newFileName));

                                    // Create a new file
                                    using (FileStream newFileStream = File.Create(newFileName))
                                    {
                                        // Copy the file to target library asynchronously
                                        await currentFileStream.CopyToAsync(newFileStream);

                                        // Increase movedFiles
                                        TotalMovedFiles += 1;
                                    }
                                }

                                // If we will validate files
                                if (Validate)
                                {
                                    // Get MD5 hash of current file
                                    currentFileMD5 = Functions.FileSystem.GetFileMD5(currentFile);

                                    // Get MD5 hash of new file
                                    newFileMD5 = Functions.FileSystem.GetFileMD5(newFileName);

                                    // Compare the hashes, if any of them not equals
                                    if (BitConverter.ToString(currentFileMD5) != BitConverter.ToString(newFileMD5))
                                    {
                                        // Log it
                                        Log(string.Format("[{0}/{1}] File couldn't verified: {2}", TotalMovedFiles, FilesToMove, newFileName));

                                        // and cancel the process
                                        return;
                                    }

                                }

                                // Log details about copied file
                                Log(string.Format("[{0}/{1}] Copied: {2}", TotalMovedFiles, FilesToMove, newFileName));

                                // Perform step on progressbar
                                progressBar_CopyStatus.PerformStep();
                            }
                        }

                        // If game has downloading folder
                        if (!string.IsNullOrEmpty(Game.downloadPath))
                        {
                            // For each files in downloading folder of game
                            foreach (string currentFile in Directory.EnumerateFiles(Game.downloadPath, "*", SearchOption.AllDirectories))
                            {
                                // Make a new file stream from the file we are reading so we can copy the file asynchronously
                                using (FileStream currentFileStream = File.OpenRead(currentFile))
                                {
                                    // Set new file name including target download path
                                    newFileName = Path.Combine(newDownloadingPath, currentFile.Replace(Game.downloadPath, ""));

                                    // If directory not exists
                                    if (!Directory.Exists(Path.GetDirectoryName(newFileName)))
                                        // Create a directory for new file
                                        Directory.CreateDirectory(Path.GetDirectoryName(newFileName));

                                    // Create the new file
                                    using (FileStream newFileStream = File.Create(newFileName))
                                    {
                                        // And copy contents asynchronously
                                        await currentFileStream.CopyToAsync(newFileStream);

                                        // Increase movedFiles
                                        TotalMovedFiles += 1;
                                    }
                                }

                                // If we will validate files
                                if (Validate)
                                {
                                    // Get MD5 hash of current file
                                    currentFileMD5 = Functions.FileSystem.GetFileMD5(currentFile);

                                    // Get MD5 hash of new file
                                    newFileMD5 = Functions.FileSystem.GetFileMD5(newFileName);

                                    // Compare the hashes, if any of them not equals
                                    if (BitConverter.ToString(currentFileMD5) != BitConverter.ToString(newFileMD5))
                                    {
                                        // Log it
                                        Log(string.Format("[{0}/{1}] File couldn't verified: {2}", TotalMovedFiles, FilesToMove, newFileName));

                                        // and cancel the process
                                        return;
                                    }
                                }

                                // Log details about copied file
                                Log(string.Format("[{0}/{1}] Copied: {2}", TotalMovedFiles, FilesToMove, newFileName));

                                // Perform step on progressbar
                                progressBar_CopyStatus.PerformStep();
                            }
                        }

                        if (Directory.Exists(Game.Library.downloadPath))
                        {
                            // If game has .patch files in downloading folder
                            // If downloading folder not exists
                            if (!Directory.Exists(newDownloadingPath))
                                // Create downloading folder
                                Directory.CreateDirectory(newDownloadingPath);

                            // For each .patch file in downloading folder
                            foreach (string currentFile in Directory.EnumerateFiles(Game.Library.downloadPath, string.Format("*{0}*.patch", Game.appID), SearchOption.TopDirectoryOnly))
                            {
                                // Set new file name
                                newFileName = Path.Combine(Library.Directory, "downloading", currentFile.Replace(Game.Library.downloadPath, ""));

                                // Copy .patch file to target library asynchronously
                                await Task.Run(() => File.Copy(currentFile, newFileName, true));
                            }
                        }

                        // If game has workshop folder
                        if (!string.IsNullOrEmpty(Game.workShopPath))
                        {
                            // For each file in workshop folder of game
                            foreach (string currentFile in Directory.EnumerateFiles(Game.workShopPath, "*", SearchOption.AllDirectories))
                            {
                                // Set new file name
                                newFileName = Path.Combine(Library.Directory, currentFile.Replace(Game.Library.Directory, ""));

                                // If directory not exists
                                if (!Directory.Exists(Path.GetDirectoryName(newFileName)))
                                    // Create a directory for new file
                                    Directory.CreateDirectory(Path.GetDirectoryName(newFileName));

                                // Copy the file asynchronously
                                await Task.Run(() => File.Copy(currentFile, newFileName, true));

                                // Increase movedFiles
                                TotalMovedFiles += 1;

                                // Log details to user
                                Log(string.Format("[{0}/{1}] Copied: {2}", TotalMovedFiles, FilesToMove, newFileName));

                                // Perform a step on progressbar
                                progressBar_CopyStatus.PerformStep();

                                // Copy workshop .ACF file
                                File.Copy(Game.workShopAcfPath, Path.Combine(Library.workshopPath, Game.workShopAcfName), true);

                                // Log to user
                                Log(".ACF file has been created at the target directory");
                            }
                        }

                        // Copy .ACF file
                        File.Copy(Game.acfPath, Path.Combine(Library.Directory, Game.acfName), true);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error to user
                Log("There was an error happened while moving game.");

                // If we are asked to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Then log errors to file
                    Functions.Log.ErrorsToFile("CopyGame", ex.ToString());

                // Cancel the process
                return;
            }

            try
            {
                // If game is compressed and we are removing old files
                if (isGameCompressed && RemoveOldFiles)
                {
                    // Delete compressed file
                    await Task.Run(() => File.Delete(currentZipNameNpath));

                    // If archive not exists
                    if (!File.Exists(currentZipNameNpath))
                        // Log to user, we have removed the zip succesfully
                        Log("Archive file removed as requested.");
                    else
                        // Log to user about failure
                        Log("Couldn't remove archive file!");
                }
                // Else if game is not compressed and we are removing old files
                else if (!isGameCompressed && RemoveOldFiles)
                {
                    // Remove the .ACF file
                    File.Delete(Game.acfPath);

                    // If we removed .ACF file succesfully
                    if (!File.Exists(Game.acfPath))
                        // Log to user
                        Log("Old .ACF file has been removed");

                    if (Directory.Exists(Game.Library.downloadPath))
                    {
                        // For each .patch file in downloading folder
                        foreach (string fileName in Directory.EnumerateFiles(Game.Library.downloadPath, string.Format("*{0}*.patch", Game.appID), SearchOption.TopDirectoryOnly))
                        {
                            // remove the file
                            await Task.Run(() => File.Delete(fileName));
                        }
                    }

                    // If game has downloading folder
                    if (!string.IsNullOrEmpty(Game.downloadPath))
                        // Remove this folder with contents
                        Directory.Delete(Game.downloadPath, true);

                    // If game has workshop folder
                    if (!string.IsNullOrEmpty(Game.workShopPath))
                    {
                        // Remove this folder with contents
                        Directory.Delete(Game.workShopPath, true);

                        if (File.Exists(Path.Combine(Game.Library.workshopPath, Game.workShopAcfName)))
                        {
                            // Remove the file
                            File.Delete(Path.Combine(Game.Library.workshopPath, Game.workShopAcfName));
                        }
                    }

                    // If game has common folder
                    if (!string.IsNullOrEmpty(Game.commonPath))
                        // Remove this folder with contents
                        Directory.Delete(Game.commonPath, true);

                    // And log to user
                    Log("Old files has been deleted.");
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
            Functions.SteamLibrary.UpdateLibraries();

            // Update latest selected library
            Functions.Games.UpdateGameList(Definitions.SLM.LatestSelectedGame.Library);
        }

        private void Log(string Text)
        {
            try
            {
                // Append log to textbox
                textBox_Logs.AppendText(Text + "\n");
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
                Process.Start(Library.Directory);
            }
            catch { }
        }

        private void linkLabel_currentLibrary_Click(object sender, EventArgs e)
        {
            try
            {
                // On click to current library, open library in explorer
                Process.Start(Game.Library.Directory);
            }
            catch { }
        }
    }
}
