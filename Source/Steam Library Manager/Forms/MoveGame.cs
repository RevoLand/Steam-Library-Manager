using System;
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
        private void MoveGame_Load(object sender, System.EventArgs e)
        {
            // If game library is backup library, target library is backup library and game is compressed OR game library is backup library and game is compressed
            if (Game.Library.Backup && Library.Backup && Game.Compressed || Game.Library.Backup && Game.Compressed)
            {
                checkbox_Validate.Visible = false;
                checkbox_DeCompress.Visible = true;
            }
            // Else if, Game library is backup library and target library is not backup library
            else if (Game.Library.Backup && !Library.Backup)
            {
                checkbox_Validate.Visible = false;
                button_Copy.Text = "Restore";
            }
            // else if, Target library is backup library
            else if (Library.Backup)
            {
                checkbox_Compress.Visible = true;
                button_Copy.Text = "Backup";
            }

            // Load our game image asynchronously
            pictureBox_GameImage.LoadAsync("http://cdn.akamai.steamstatic.com/steam/apps/"+ Game.appID +"/header.jpg");

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
                // so after clicking button changes made on form will not affect the process
                CopyGame(checkbox_Validate.Checked, checkbox_RemoveOldFiles.Checked, checkbox_Compress.Checked, checkbox_DeCompress.Checked, Game.Compressed);
            }
            catch { }
        }

        async void CopyGame(bool Validate, bool RemoveOld, bool Compress, bool deCompress, bool isCompressed)
        {
            // Path definitions
            string downloadPath = Path.Combine(Game.Library.Directory , "downloading");
            string TargetGamePath = Path.Combine(Library.Directory , "common" , Game.installationPath);
            string TargetDownloadPath = Path.Combine(Library.Directory , "downloading" , Game.appID.ToString());
            string zipPath = Library.Directory;

            // Name definitions
            string acfName = "appmanifest_" + Game.appID + ".acf";
            string workShopACFname = "appworkshop_" + Game.appID + ".acf";
            string zipName = Game.appID + ".zip";
            string currentZipName = Path.Combine(Game.Library.Directory, Game.appID.ToString() + ".zip");
            string newFileName;

            // Other definitions
            byte[] currentFileMD5, newFileMD5;
            int FilesToMove = 0, movedFiles = 0;

            try
            {
                // If game is compressed, target library is not backup OR game is compressed and de-compress requested
                if (isCompressed && !Library.Backup || isCompressed && deCompress)
                {
                    // If directory exists at target game path
                    if (Directory.Exists(TargetGamePath))
                        // Remove the directory
                        Directory.Delete(TargetGamePath, true);

                    // Log to user
                    Log("Uncompressing archive... Please wait");

                    // unzip the archive asynchronously to target library
                    await Task.Run(() => ZipFile.ExtractToDirectory(currentZipName, zipPath));
                }
                // If game is compressed and user didn't wanted to decompress and target library is backup
                else if (isCompressed && !deCompress && Library.Backup)
                {
                    // If archive already exists in the target library
                    if (File.Exists(Path.Combine(zipPath, zipName)))
                        // Remove the compressed archive
                        File.Delete(Path.Combine(zipPath, zipName));

                    // Copy the archive asynchronously
                    await Task.Run(() => File.Copy(currentZipName, Path.Combine(zipPath, zipName)));
                }
                else
                {
                    // Define free space we have at target libray
                    long freeSpace = Functions.FileSystem.GetFreeSpace(TargetGamePath);

                    // If free space is less than game size
                    if (freeSpace < Game.sizeOnDisk)
                    {
                        // Show an error to user
                        Log("Free space is not enough! Needed Free Space: " + Game.sizeOnDisk + " Available: " + freeSpace);

                        // And cancel the process
                        return;
                    }

                    // If game has common folder
                    if (Game.exactInstallPath != null)
                        // Increase FilesToMove based on file count in common folder
                        FilesToMove += Framework.FastDirectoryEnumerator.GetFiles(Game.exactInstallPath, "*", SearchOption.AllDirectories).Length;

                    // If game has downloading folder
                    if (Game.downloadPath != null)
                        // Increase FilesToMove based on file count in "downloading" folder
                        FilesToMove += Framework.FastDirectoryEnumerator.GetFiles(Game.downloadPath, "*", SearchOption.AllDirectories).Length;

                    // If game has workshop folder
                    if (Game.workShopPath != null)
                        // Increase FilesToMove based on file count in workshop folder of game
                        FilesToMove += Framework.FastDirectoryEnumerator.GetFiles(Game.workShopPath, "*", SearchOption.AllDirectories).Length;

                    // Set progress bar maximum value to FilesToMove
                    progressBar_CopyStatus.Maximum = FilesToMove;

                    #region Compres game
                    // If game will be compressed
                    if (Compress)
                    {
                        // If compressed archive already exists
                        if (File.Exists(Path.Combine(zipPath, zipName)))
                            // Remove the compressed archive
                            File.Delete(Path.Combine(zipPath, zipName));

                        // Create a new compressed archive at target library
                        using (ZipArchive gameBackup = ZipFile.Open(Path.Combine(zipPath, zipName), ZipArchiveMode.Create))
                        {
                            // If game has common folder
                            if (Game.exactInstallPath != null)
                            {
                                // For each file in common folder of game
                                foreach (Framework.FileData currentFile in Framework.FastDirectoryEnumerator.EnumerateFiles(Game.exactInstallPath, "*", SearchOption.AllDirectories))
                                {
                                    // Add file to archive
                                    await Task.Run(() => gameBackup.CreateEntryFromFile(currentFile.Path, currentFile.Path.Replace(Game.Library.Directory, ""), CompressionLevel.Optimal));

                                    // Increase movedFiles
                                    movedFiles += 1;

                                    // Perform step on progressBar
                                    progressBar_CopyStatus.PerformStep();

                                    // Log details about process
                                    Log("[" + movedFiles.ToString() + "/" + FilesToMove.ToString() + "] Compressed: " + currentFile.Path.Replace(Game.Library.Directory, ""));
                                }
                            }

                            // If game has downloading folder
                            if (Game.downloadPath != null)
                            {
                                // For each file in downloading folder of game
                                foreach (Framework.FileData currentFile in Framework.FastDirectoryEnumerator.EnumerateFiles(Game.downloadPath, "*", SearchOption.AllDirectories))
                                {
                                    // Add file to archive
                                    await Task.Run(() => gameBackup.CreateEntryFromFile(currentFile.Path, currentFile.Path.Replace(Game.Library.Directory, ""), CompressionLevel.Optimal));

                                    // Increase movedFiles
                                    movedFiles += 1;

                                    // Perform step on progressBar
                                    progressBar_CopyStatus.PerformStep();

                                    // Log details about process
                                    Log("[" + movedFiles.ToString() + "/" + FilesToMove.ToString() + "] Compressed: " + currentFile.Path.Replace(Game.Library.Directory, ""));
                                }
                            }

                            // If game has workshop files
                            if (Game.workShopPath != null)
                            {
                                // For each file in workshop folder of game
                                foreach (Framework.FileData currentFile in Framework.FastDirectoryEnumerator.EnumerateFiles(Game.workShopPath, "*", SearchOption.AllDirectories))
                                {
                                    // Add file to archive
                                    await Task.Run(() => gameBackup.CreateEntryFromFile(currentFile.Path, currentFile.Path.Replace(Game.Library.Directory, ""), CompressionLevel.Optimal));

                                    // Increase movedFiles
                                    movedFiles += 1;

                                    // Perform step on progressBar
                                    progressBar_CopyStatus.PerformStep();

                                    // Log details about process
                                    Log("[" + movedFiles.ToString() + "/" + FilesToMove.ToString() + "] Compressed: " + currentFile.Path.Replace(Game.Library.Directory, ""));
                                }
                            }

                            // If game has .patch files in downloading folder
                            foreach (Framework.FileData currentFile in Framework.FastDirectoryEnumerator.EnumerateFiles(downloadPath, "*" + Game.appID + "*.patch", SearchOption.TopDirectoryOnly))
                            {
                                // Add file to archive
                                await Task.Run(() => gameBackup.CreateEntryFromFile(currentFile.Path, currentFile.Path.Replace(Game.Library.Directory, ""), CompressionLevel.Optimal));
                            }

                            // Add .ACF file to archive
                            await Task.Run(() => gameBackup.CreateEntryFromFile(Path.Combine(Game.Library.Directory , acfName), acfName, CompressionLevel.Optimal));

                            // Log .ACF file
                            Log(".ACF file has been compressed");

                            // Workshop .ACF File
                            if (Directory.Exists(Game.workShopPath))
                            {
                                // Add Workshop .ACF file to archive
                                await Task.Run(() => gameBackup.CreateEntryFromFile(Path.Combine(Game.Library.Directory , "workshop" , workShopACFname), Path.Combine("workshop" , workShopACFname), CompressionLevel.Optimal));

                                // Log workshop .ACF file
                                Log("Workshop .ACF file has been compressed");
                            }
                        }
                    }
                    #endregion
                    // If game will not be compressed
                    else
                    {
                        // If directory not exists
                        if (!Directory.Exists(TargetGamePath))
                            // Create the game directory at target library
                            Directory.CreateDirectory(TargetGamePath);

                        // If game has common folder
                        if (Game.exactInstallPath != null)
                        {
                            // For each file in common folder of game
                            foreach (Framework.FileData currentFile in Framework.FastDirectoryEnumerator.EnumerateFiles(Game.exactInstallPath, "*", SearchOption.AllDirectories))
                            {
                                // Make a new file stream from the file we are reading so we can copy the file asynchronously
                                using (FileStream currentFileStream = File.OpenRead(currentFile.Path))
                                {
                                    // Set new file name including target game path
                                    newFileName = TargetGamePath + currentFile.Path.Replace(Game.exactInstallPath, "");

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
                                        movedFiles += 1;
                                    }
                                }

                                // If we will validate files
                                if (Validate)
                                {
                                    // Get MD5 hash of current file
                                    currentFileMD5 = Functions.FileSystem.GetFileMD5(currentFile.Path);

                                    // Get MD5 hash of new file
                                    newFileMD5 = Functions.FileSystem.GetFileMD5(newFileName);

                                    // Compare the hashes, if any of them not equals
                                    if (BitConverter.ToString(currentFileMD5) != BitConverter.ToString(newFileMD5))
                                    {
                                        // Log it
                                        Log("[" + movedFiles.ToString() + "/" + FilesToMove.ToString() + "] File couldn't verified: " + currentFile.Path.Replace(Game.Library.Directory, ""));
                                        
                                        // and cancel the process
                                        return;
                                    }

                                }

                                // Log details about copied file
                                Log("[" + movedFiles.ToString() + "/" + FilesToMove.ToString() + "] Copied: " + currentFile.Path.Replace(Game.Library.Directory, ""));

                                // Perform step on progressbar
                                progressBar_CopyStatus.PerformStep();
                            }
                        }

                        // If game has downloading folder
                        if (Game.downloadPath != null)
                        {
                            // For each files in downloading folder of game
                            foreach (Framework.FileData currentFile in Framework.FastDirectoryEnumerator.EnumerateFiles(Game.downloadPath, "*", SearchOption.AllDirectories))
                            {
                                // Make a new file stream from the file we are reading so we can copy the file asynchronously
                                using (FileStream currentFileStream = File.OpenRead(currentFile.Path))
                                {
                                    // Set new file name including target download path
                                    newFileName = TargetDownloadPath + currentFile.Path.Replace(Game.downloadPath, "");

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
                                        movedFiles += 1;
                                    }
                                }

                                // If we will validate files
                                if (Validate)
                                {
                                    // Get MD5 hash of current file
                                    currentFileMD5 = Functions.FileSystem.GetFileMD5(currentFile.Path);

                                    // Get MD5 hash of new file
                                    newFileMD5 = Functions.FileSystem.GetFileMD5(newFileName);

                                    // Compare the hashes, if any of them not equals
                                    if (BitConverter.ToString(currentFileMD5) != BitConverter.ToString(newFileMD5))
                                    {
                                        // Log it
                                        Log("[" + movedFiles.ToString() + "/" + FilesToMove.ToString() + "] File couldn't verified: " + currentFile.Path.Replace(Game.Library.Directory, ""));

                                        // and cancel the process
                                        return;
                                    }
                                }

                                // Log details about copied file
                                Log("[" + movedFiles.ToString() + "/" + FilesToMove.ToString() + "] Copied: " + currentFile.Path.Replace(Game.Library.Directory, ""));

                                // Perform step on progressbar
                                progressBar_CopyStatus.PerformStep();
                            }
                        }

                        // If game has .patch files in downloading folder
                        // If downloading folder not exists
                        if (!Directory.Exists(Path.Combine(Library.Directory , "downloading")))
                            // Create downloading folder
                            Directory.CreateDirectory(Path.Combine(Library.Directory ,"downloading"));

                        // For each .patch file in downloading folder
                        foreach (Framework.FileData currentFile in Framework.FastDirectoryEnumerator.EnumerateFiles(downloadPath, "*" + Game.appID + "*.patch", SearchOption.TopDirectoryOnly))
                        {
                            // Set new file name
                            newFileName = Path.Combine(Library.Directory , "downloading" , currentFile.Name.Replace(downloadPath, ""));

                            // Copy .patch file to target library asynchronously
                            await Task.Run(() => File.Copy(currentFile.Path, newFileName, true));
                        }

                        // If game has workshop folder
                        if (Game.workShopPath != null)
                        {
                            // For each file in workshop folder of game
                            foreach (Framework.FileData currentFile in Framework.FastDirectoryEnumerator.EnumerateFiles(Game.workShopPath, "*", SearchOption.AllDirectories))
                            {
                                // Set new file name
                                newFileName = Path.Combine(Library.Directory , currentFile.Path.Replace(Game.Library.Directory, ""));

                                // If directory not exists
                                if (!Directory.Exists(Path.GetDirectoryName(newFileName)))
                                    // Create a directory for new file
                                    Directory.CreateDirectory(Path.GetDirectoryName(newFileName));

                                // Copy the file asynchronously
                                await Task.Run(() => File.Copy(currentFile.Path, newFileName, true));

                                // Increase movedFiles
                                movedFiles += 1;

                                // Log details to user
                                Log("[" + movedFiles.ToString() + "/" + FilesToMove.ToString() + "] Copied: " + currentFile.Path.Replace(Game.Library.Directory, ""));

                                // Perform a step on progressbar
                                progressBar_CopyStatus.PerformStep();
                            }
                        }

                        // Copy .ACF file
                        File.Copy(Path.Combine(Game.Library.Directory , acfName), Path.Combine(Library.Directory , acfName), true);

                        // If workshop directory exists
                        if (Directory.Exists(Game.workShopPath))
                        {
                            // Copy workshop .ACF file
                            File.Copy(Path.Combine(Game.Library.Directory , "workshop" , workShopACFname), Path.Combine(Library.Directory , "workshop" , workShopACFname), true);

                            // Log to user
                            Log(".ACF file has been created at the target directory");
                        }

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
                if (isCompressed && RemoveOld)
                {
                    // Delete compressed file
                    await Task.Run(() => File.Delete(currentZipName));

                    // If archive not exists
                    if (!File.Exists(currentZipName))
                        // Log to user, we have removed the zip succesfully
                        Log("Archive file removed as requested.");
                    else
                        // Log to user about failure
                        Log("Couldn't remove archive file!");
                }
                // Else if game is not compressed and we are removing old files
                else if (!isCompressed && RemoveOld)
                {
                    // For each .patch file in downloading folder
                    foreach (Framework.FileData fileName in Framework.FastDirectoryEnumerator.EnumerateFiles(downloadPath, "*" + Game.appID + "*.patch", SearchOption.TopDirectoryOnly))
                    {
                        // remove the file
                        await Task.Run(() => File.Delete(fileName.Path));
                    }

                    // Remove the .ACF file
                    File.Delete(Game.Library.Directory + acfName);

                    // If we removed .ACF file succesfully
                    if (!File.Exists(Game.Library.Directory + acfName))
                        // Log to user
                        Log("Old .ACF file has been removed");

                    // If workshop .ACf file exists
                    if (File.Exists(Path.Combine(Game.Library.Directory , "workshop" , workShopACFname)))
                    {
                        // Remove the file
                        File.Delete(Path.Combine(Game.Library.Directory, "workshop", workShopACFname));

                        // If we removed file succesfully
                        if (!File.Exists(Path.Combine(Game.Library.Directory, "workshop", workShopACFname)))
                            // Log to user
                            Log("Workshop .ACF file has been removed");
                    }

                    // If game has downloading folder
                    if (Game.downloadPath != null)
                        // Remove this folder with contents
                        Directory.Delete(Game.downloadPath, true);

                    // If game has workshop folder
                    if (Game.workShopPath != null)
                        // Remove this folder with contents
                        Directory.Delete(Game.workShopPath, true);

                    // If game has common folder
                    if (Game.exactInstallPath != null)
                        // Remove this folder with contents
                        Directory.Delete(Game.exactInstallPath, true);

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

            // Update button text
            button_Copy.Text = "Completed!";

            // Log to user
            Log("Process has been completed, you may close this window now.");

            // Update game libraries
            Functions.SteamLibrary.UpdateLibraries();

            // Update latest selected library
            Functions.Games.UpdateGamesList(Definitions.SLM.LatestSelectedGame.Library);
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
                        System.Diagnostics.Process.Start("http://store.steampowered.com/app/" + Game.appID.ToString() + "/");
                        break;
                    // Right click
                    case MouseButtons.Right:
                        // Open game installation directory in explorer
                        System.Diagnostics.Process.Start(Game.exactInstallPath);
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
                System.Diagnostics.Process.Start(Library.Directory);
            }
            catch { }
        }

        private void linkLabel_currentLibrary_Click(object sender, EventArgs e)
        {
            try
            {
                // On click to current library, open library in explorer
                System.Diagnostics.Process.Start(Game.Library.Directory);
            }
            catch { }
        }
    }
}
