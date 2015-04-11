using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Steam_Library_Manager.Forms
{
    public partial class MoveGame : Form
    {
        public MoveGame()
        {
            InitializeComponent();
        }

        Definitions.List.GamesList Game = Definitions.Accessors.Main.listBox_InstalledGames.SelectedItem as Definitions.List.GamesList;
        Stopwatch TimeElapsed = new Stopwatch();

        private void MoveGame_Load(object sender, System.EventArgs e)
        {
            pictureBox_GameImage.LoadAsync("http://cdn.akamai.steamstatic.com/steam/apps/"+ Game.appID +"/header.jpg");
            pictureBox_GameImage.Tag = Game.appName;
            linkLabel_currentLibrary.Text = Game.libraryPath;

            foreach (Definitions.List.InstallDirsList Library in Definitions.List.InstallDirs)
            {
                if (Library.Directory != Game.libraryPath)
                    comboBox_TargetLibrary.Items.Add(Library.Directory);
            }
        }

        private void button_Copy_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBox_TargetLibrary.SelectedItem == null)
                    return;

                button_Copy.Enabled = false;
                comboBox_TargetLibrary.Enabled = false;

                CopyGame(Game.exactInstallPath, this.comboBox_TargetLibrary.SelectedItem.ToString(), Game.installationPath, Game.appID, this.checkbox_Validate.Checked, this.checkbox_RemoveOldFiles.Checked);
            }
            catch { }
        }

        async void CopyGame(string currentGamePath, string TargetPath, string GameName, int appID, bool Validate, bool RemoveOld)
        {
            string TargetGamePath = "";
            switch (Game.StateFlag)
            {
                case 4:
                    TargetGamePath = TargetPath + @"common\" + GameName;
                    break;
                case 1026:
                    TargetGamePath = TargetPath + @"downloading\" + appID;
                    break;
                default:
                    Log("This Installation State is not supported yet. State: " + Game.StateFlag);
                    return;
            }

            try
            {
                TimeElapsed.Start();
                timer_TimeElapsed.Start();

                // If we have create & remove permissions at the target game path
                if (Functions.FileSystem.TestFile(TargetGamePath))
                {
                    // If something is wrong and current game directory doesn't exists
                    if (!Directory.Exists(currentGamePath))
                    {
                        // Show error to user
                        System.Windows.Forms.MessageBox.Show("Can not find selected game files... Is there something went wrong with coding?\nDirectory: " + currentGamePath);
                        return;
                    }

                    long freeSpace = Functions.FileSystem.GetFreeSpace(TargetGamePath);

                    if (freeSpace < Game.sizeOnDisk)
                    {
                        Log("Free space is not enough! Needed Free Space: " + Game.sizeOnDisk + " Available: " + freeSpace);
                        return;
                    }

                    // If the directory we will move game is not exists
                    if (!Directory.Exists(TargetGamePath))
                        // Create the directory
                        Directory.CreateDirectory(TargetGamePath);

                    string acfName = "appmanifest_" + appID + ".acf";

                    byte[] currentFileMD5, newFileMD5;
                    string newFileName;
                    int FilesToMove = Directory.GetFiles(currentGamePath, "*", SearchOption.AllDirectories).Length;

                    progressBar_CopyStatus.Maximum = FilesToMove;

                    int movedFiles = 0;

                    foreach (string currentFile in Directory.EnumerateFiles(currentGamePath, "*", SearchOption.AllDirectories))
                    {
                        using (FileStream currentFileStream = File.Open(currentFile, FileMode.Open, FileAccess.Read))
                        {
                            newFileName = TargetGamePath + currentFile.Replace(currentGamePath, "");
                            Directory.CreateDirectory(Path.GetDirectoryName(newFileName));
                            using (FileStream newFileStream = File.Create(newFileName))
                            {
                                await currentFileStream.CopyToAsync(newFileStream);

                                movedFiles += 1;
                            }
                        }

                        if (Validate)
                        {
                            currentFileMD5 = Functions.FileSystem.GetFileMD5(currentFile);
                            newFileMD5 = Functions.FileSystem.GetFileMD5(newFileName);
                            if (BitConverter.ToString(currentFileMD5) != BitConverter.ToString(newFileMD5))
                            {
                                Log("[" + movedFiles.ToString() + "/" + FilesToMove.ToString() + "] File couldn't verified: " + newFileName);
                                break;
                            }

                        }

                        Log("[" + movedFiles.ToString() + "/" + FilesToMove.ToString() + "] Copied: " + newFileName);
                        progressBar_CopyStatus.PerformStep();
                    }

                    File.Copy(Game.libraryPath + acfName, TargetPath + acfName, true);

                    Log(".ACF file has been created at the target directory");

                    if (RemoveOld)
                    {
                        Directory.Delete(currentGamePath, true);
                        File.Delete(Game.libraryPath + acfName);

                        Log("Old files has been deleted.");
                    }

                    timer_TimeElapsed.Stop();
                    TimeElapsed.Start();

                    button_Copy.Text = "Done!";
                    Log("Completed! All files successfully copied!");
                }
                else
                {
                    Log("Failed");
                    System.Windows.Forms.MessageBox.Show("We don't have enough perms at the target library path, try to run as Administrator maybe?");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Log(string Text)
        {
            try
            {
                this.textBox_CopyLogs.AppendText(Text + "\n");
            }
            catch { }
        }

        private void linkLabel_currentLibrary_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(Game.libraryPath);
            }
            catch { }
        }

        private void timer_TimeElapsed_Tick(object sender, EventArgs e)
        {
            label_TimeElapsed.Text = String.Format("Time Elapsed: {0}", TimeElapsed.Elapsed);
        }

        private void comboBox_TargetLibrary_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (Game.StateFlag)
            {
                case 4: // Installed
                case 1026:
                    label_NeededSpace.Text = Functions.FileSystem.FormatBytes(Functions.FileSystem.GetDirectorySize(Game.exactInstallPath, true));
                    label_AvailableSpace.Text = Functions.FileSystem.FormatBytes(Functions.FileSystem.GetFreeSpace(comboBox_TargetLibrary.SelectedItem.ToString()));
                    break;
                case 1024: // Pre-Load

                    break;
            }
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
    }
}
