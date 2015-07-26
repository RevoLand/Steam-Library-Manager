using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Steam_Library_Manager.Forms
{
    public partial class MoveGame : Form
    {
        public MoveGame()
        {
            InitializeComponent();

            // Same as mainform, when we set ErrorImage or Form Icon PortableSettingsProvider gives an error
            this.Icon = Steam_Library_Manager.Properties.Resources.steam_icon;
            this.pictureBox_GameImage.ErrorImage = global::Steam_Library_Manager.Properties.Resources.no_image_available;
        }

        Definitions.List.GamesList Game = Definitions.SLM.LatestSelectedGame;
        Definitions.List.LibraryList Library = Definitions.SLM.LatestDropLibrary;

        private void MoveGame_Load(object sender, System.EventArgs e)
        {
            if (Game.Library.Backup && Library.Backup && Game.Compressed)
            {
                checkbox_Validate.Visible = false;
                checkbox_DeCompress.Visible = true;
            }
            else if (Game.Library.Backup && !Library.Backup)
            {
                checkbox_Validate.Visible = false;
                button_Copy.Text = "Restore";
            }
            else if (Library.Backup)
            {
                checkbox_Compress.Visible = true;
                button_Copy.Text = "Backup";
            }

            pictureBox_GameImage.LoadAsync("http://cdn.akamai.steamstatic.com/steam/apps/"+ Game.appID +"/header.jpg");
            linkLabel_currentLibrary.Text = Game.Library.Directory;
            linkLabel_TargetLibrary.Text = Library.Directory;

            long NeededSpace = 0;

            if (Game.exactInstallPath != null)
                NeededSpace += Functions.FileSystem.GetDirectorySize(Game.exactInstallPath, true);

            if (Game.downloadPath != null)
                NeededSpace += Functions.FileSystem.GetDirectorySize(Game.downloadPath, true);

            if (Game.Compressed)
            {
                if (Properties.Settings.Default.SLM_ArchiveSizeCalcMethod.StartsWith("Uncompressed"))
                {
                    // Gets uncompressed file size
                    using (ZipArchive zip = ZipFile.OpenRead(Game.Library.Directory + Game.appID + ".zip"))
                    {
                        foreach (ZipArchiveEntry entry in zip.Entries)
                        {
                            NeededSpace += entry.Length;
                        }
                    }
                }
                else
                {
                    // Archive size
                    FileInfo zip = new FileInfo(Game.Library.Directory + Game.appID + ".zip");
                    NeededSpace += zip.Length;
                }
            }

            label_AvailableSpace.Text = Functions.FileSystem.FormatBytes(Functions.FileSystem.GetFreeSpace(Library.Directory));
            label_NeededSpace.Text = Functions.FileSystem.FormatBytes(NeededSpace);
        }

        private void button_Copy_Click(object sender, EventArgs e)
        {
            try
            {
                button_Copy.Enabled = false;

                CopyGame(checkbox_Validate.Checked, checkbox_RemoveOldFiles.Checked, checkbox_Compress.Checked, checkbox_DeCompress.Checked, Game.Compressed);
            }
            catch { }
        }

        async void CopyGame(bool Validate, bool RemoveOld, bool Compress,  bool deCompress, bool isCompressed)
        {
            byte[] currentFileMD5, newFileMD5;
            string newFileName;

            int FilesToMove = 0, movedFiles = 0;
            string downloadPath = Game.Library.Directory + @"downloading\";
            string TargetGamePath = Library.Directory + @"common\" + Game.installationPath;
            string TargetDownloadPath = Library.Directory + @"downloading\" + Game.appID;
            string acfName = "appmanifest_" + Game.appID + ".acf";

            string zipPath = Library.Directory;
            string zipName = Game.appID + ".zip";
            string currentZipName = Game.Library.Directory + Game.appID + ".zip";

            // If we have create & remove permissions at the target game path
            if (Functions.FileSystem.TestFile(TargetGamePath))
            {
                try
                {
                    if (isCompressed && !Game.Library.Backup || isCompressed && deCompress)
                    {
                        Log("Uncompressing archive... Please wait");
                        await Task.Run(() => ZipFile.ExtractToDirectory(currentZipName, zipPath));
                    }
                    else if (isCompressed && !deCompress && Game.Library.Backup)
                    {
                        if (File.Exists(zipPath + zipName))
                            File.Delete(zipPath + zipName);

                        await Task.Run(() => File.Copy(currentZipName, zipPath + zipName));
                    }
                    else
                    {
                        // If something is wrong and current game directory doesn't exists
                        if (!Directory.Exists(Game.exactInstallPath) && !Directory.Exists(Game.downloadPath))
                        {
                            // Show error to user
                            System.Windows.Forms.MessageBox.Show("Can not find selected game files... Is there something went wrong with coding?\nDirectory: " + Game.exactInstallPath);
                            return;
                        }

                        long freeSpace = Functions.FileSystem.GetFreeSpace(TargetGamePath);

                        if (freeSpace < Game.sizeOnDisk)
                        {
                            Log("Free space is not enough! Needed Free Space: " + Game.sizeOnDisk + " Available: " + freeSpace);
                            return;
                        }

                        if (Game.exactInstallPath != null)
                            FilesToMove += Framework.FastDirectoryEnumerator.GetFiles(Game.exactInstallPath, "*", SearchOption.AllDirectories).Length;

                        if (Game.downloadPath != null)
                            FilesToMove += Framework.FastDirectoryEnumerator.GetFiles(Game.downloadPath, "*", SearchOption.AllDirectories).Length;

                        progressBar_CopyStatus.Maximum = FilesToMove;

                        #region Compressed file
                        if (Compress)
                        {
                            if (File.Exists(zipPath + zipName))
                                File.Delete(zipPath + zipName);

                            using (ZipArchive gameBackup = ZipFile.Open(zipPath + zipName, ZipArchiveMode.Create))
                            {
                                // common folder
                                if (Game.exactInstallPath != null)
                                {
                                    foreach (Framework.FileData currentFile in Framework.FastDirectoryEnumerator.EnumerateFiles(Game.exactInstallPath, "*", SearchOption.AllDirectories))
                                    {
                                        await Task.Run(() => gameBackup.CreateEntryFromFile(currentFile.Path, currentFile.Path.Replace(Game.Library.Directory, ""), CompressionLevel.Optimal));
                                        movedFiles += 1;

                                        Log("[" + movedFiles.ToString() + "/" + FilesToMove.ToString() + "] Compressed: " + currentFile.Path.Replace(Game.exactInstallPath, ""));
                                        progressBar_CopyStatus.PerformStep();
                                    }
                                }

                                // downloading folder
                                if (Game.downloadPath != null)
                                {
                                    foreach (Framework.FileData currentFile in Framework.FastDirectoryEnumerator.EnumerateFiles(Game.downloadPath, "*", SearchOption.AllDirectories))
                                    {
                                        await Task.Run(() => gameBackup.CreateEntryFromFile(currentFile.Path, currentFile.Path.Replace(Game.Library.Directory, ""), CompressionLevel.Optimal));
                                        movedFiles += 1;

                                        Log("[" + movedFiles.ToString() + "/" + FilesToMove.ToString() + "] Compressed: " + currentFile.Path.Replace(Game.exactInstallPath, ""));
                                        progressBar_CopyStatus.PerformStep();
                                    }
                                }

                                // .patch files
                                foreach (Framework.FileData fileName in Framework.FastDirectoryEnumerator.EnumerateFiles(downloadPath, "*" + Game.appID + "*.patch", SearchOption.TopDirectoryOnly))
                                {
                                    await Task.Run(() => gameBackup.CreateEntryFromFile(fileName.Path, fileName.Path.Replace(Game.Library.Directory, ""), CompressionLevel.Optimal));
                                }

                                // .ACF File
                                await Task.Run(() => gameBackup.CreateEntryFromFile(Game.Library.Directory + acfName, acfName, CompressionLevel.Optimal));
                                Log(".ACF file has been compressed");
                            }
                        }
                        #endregion
                        else
                        {
                            // Create the directory
                            Directory.CreateDirectory(TargetGamePath);

                            // common
                            if (Game.exactInstallPath != null)
                            {
                                foreach (Framework.FileData currentFile in Framework.FastDirectoryEnumerator.EnumerateFiles(Game.exactInstallPath, "*", SearchOption.AllDirectories))
                                {
                                    using (FileStream currentFileStream = File.Open(currentFile.Path, FileMode.Open, FileAccess.Read))
                                    {
                                        newFileName = TargetGamePath + currentFile.Path.Replace(Game.exactInstallPath, "");
                                        Directory.CreateDirectory(Path.GetDirectoryName(newFileName));
                                        using (FileStream newFileStream = File.Create(newFileName))
                                        {
                                            await currentFileStream.CopyToAsync(newFileStream);

                                            movedFiles += 1;
                                        }
                                    }

                                    if (Validate)
                                    {
                                        currentFileMD5 = Functions.FileSystem.GetFileMD5(currentFile.Path);
                                        newFileMD5 = Functions.FileSystem.GetFileMD5(newFileName);
                                        if (BitConverter.ToString(currentFileMD5) != BitConverter.ToString(newFileMD5))
                                        {
                                            Log("[" + movedFiles.ToString() + "/" + FilesToMove.ToString() + "] File couldn't verified: " + currentFile.Path.Replace(Game.exactInstallPath, ""));
                                            break;
                                        }

                                    }

                                    Log("[" + movedFiles.ToString() + "/" + FilesToMove.ToString() + "] Copied: " + currentFile.Path.Replace(Game.exactInstallPath, ""));
                                    progressBar_CopyStatus.PerformStep();
                                }
                            }

                            // downloading
                            if (Game.downloadPath != null)
                            {
                                foreach (Framework.FileData currentFile in Framework.FastDirectoryEnumerator.EnumerateFiles(Game.downloadPath, "*", SearchOption.AllDirectories))
                                {
                                    using (FileStream currentFileStream = File.Open(currentFile.Path, FileMode.Open, FileAccess.Read))
                                    {
                                        newFileName = TargetDownloadPath + currentFile.Path.Replace(Game.downloadPath, "");
                                        Directory.CreateDirectory(Path.GetDirectoryName(newFileName));
                                        using (FileStream newFileStream = File.Create(newFileName))
                                        {
                                            await currentFileStream.CopyToAsync(newFileStream);

                                            movedFiles += 1;
                                        }
                                    }

                                    if (Validate)
                                    {
                                        currentFileMD5 = Functions.FileSystem.GetFileMD5(currentFile.Path);
                                        newFileMD5 = Functions.FileSystem.GetFileMD5(newFileName);
                                        if (BitConverter.ToString(currentFileMD5) != BitConverter.ToString(newFileMD5))
                                        {
                                            Log("[" + movedFiles.ToString() + "/" + FilesToMove.ToString() + "] File couldn't verified: " + currentFile.Path.Replace(Game.downloadPath, ""));
                                            break;
                                        }

                                    }

                                    Log("[" + movedFiles.ToString() + "/" + FilesToMove.ToString() + "] Copied: " + currentFile.Path.Replace(Game.downloadPath, ""));
                                    progressBar_CopyStatus.PerformStep();
                                }
                            }

                            // .Patch files
                            Directory.CreateDirectory(Library.Directory + @"downloading\");
                            foreach (Framework.FileData fileName in Framework.FastDirectoryEnumerator.EnumerateFiles(downloadPath, "*" + Game.appID + "*.patch", SearchOption.TopDirectoryOnly))
                            {
                                newFileName = Library.Directory + @"downloading\" + fileName.Name.Replace(downloadPath, "");

                                Directory.CreateDirectory(Path.GetDirectoryName(newFileName));

                                File.Copy(fileName.Path, newFileName, true);
                            }

                            // .ACF File
                            File.Copy(Game.Library.Directory + acfName, Library.Directory + acfName, true);
                            Log(".ACF file has been created at the target directory");

                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                try
                {
                    if (isCompressed && RemoveOld)
                    {
                        File.Delete(currentZipName);
                        Log("Archive file removed as requested.");
                    }

                    if (RemoveOld)
                    {
                        foreach (Framework.FileData fileName in Framework.FastDirectoryEnumerator.EnumerateFiles(downloadPath, "*" + Game.appID + "*.patch", SearchOption.TopDirectoryOnly))
                        {
                            File.Delete(fileName.Path);
                        }
                        // .ACF
                        File.Delete(Game.Library.Directory + acfName);

                        if (Game.exactInstallPath != null)
                            // common
                            Directory.Delete(Game.exactInstallPath, true);

                        if (Game.downloadPath != null)
                            // downloading
                            Directory.Delete(Game.downloadPath, true);

                        Log("Old files has been deleted.");
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.ToString());
                    Log("There was an error happened while removing old files but if you only seeing this error then don't worry, you may have to remove leftovers manually. Check DirectoryRemoval.txt for more details");
                    Functions.Log.LogErrorsToFile("DirectoryRemoval", ex.ToString());

                    Functions.SteamLibrary.UpdateGameLibraries();
                    Functions.Games.UpdateGamesList(Definitions.SLM.LatestSelectedGame.Library);
                }


                // More Visual
                button_Copy.Text = "Done!";
                Log("Completed! All files successfully copied!");
                Functions.SteamLibrary.UpdateGameLibraries();
                Functions.Games.UpdateGamesList(Definitions.SLM.LatestSelectedGame.Library);
            }
            else
            {
                Log("Failed");
                System.Windows.Forms.MessageBox.Show("We don't have enough perms at the target library path, try to run as Administrator maybe?");
            }
        }

        private void Log(string Text)
        {
            try
            {
                textBox_CopyLogs.AppendText(Text + "\n");
            }
            catch { }
        }

        private void pictureBox_GameImage_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                switch (e.Button)
                {
                    default:
                        System.Diagnostics.Process.Start("http://store.steampowered.com/app/" + Game.appID.ToString() + "/");
                        break;
                    case MouseButtons.Right:
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
                System.Diagnostics.Process.Start(Library.Directory);
            }
            catch { }
        }

        private void linkLabel_currentLibrary_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(Game.Library.Directory);
            }
            catch { }
        }
    }
}
