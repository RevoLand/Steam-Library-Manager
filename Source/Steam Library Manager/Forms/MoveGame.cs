using System;
using System.IO;
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

        private void MoveGame_Load(object sender, System.EventArgs e)
        {
            linkLabel_GameName.Text = Game.appName;

            foreach (Definitions.List.InstallDirsList Library in Definitions.List.InstallDirs)
            {
                if (Library.Directory != Game.libraryPath)
                    comboBox_TargetLibrary.Items.Add(Library.Directory);
            }
        }

        private void linkLabel_GameName_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://store.steampowered.com/app/" + Game.appID.ToString() + "/");
            }
            catch { }
        }

        public async void CopyGame(string currentPath, string TargetPath, string GameName, int appID, bool Validate, bool RemoveOld)
        {
            /*
             * currentPath = Current installation path of game, C:\Steam\SteamApps\ as example
             * TargetPath = Where the game will be moved to, D:\Steam\SteamApps\ as example
             * GameName = Game installation folder name as provided in ACF file, "Dying Light" as example (C:\Steam\SteamApps\common\+ GameName +)
             * Owerwrite = Owerwrite existing files or not
             * Validate = Validate game files after move
             * RemoveOld = Remove files from currentPath after process has been done (and validated if set)
             */
            string currentGamePath = currentPath + @"common\" + GameName + @"\";
            string TargetGamePath = TargetPath + @"common\" + GameName + @"\";

            try
            {
                // If we have create & remove permissions at the target game path
                if (Functions.FileSystem.TestFile(TargetGamePath))
                {
                    // If something is wrong and current game directory doesn't exists
                    if (!Directory.Exists(currentGamePath))
                        // Show error to user
                        System.Windows.Forms.MessageBox.Show("Can not find selected game files... Is there something went wrong with coding?\nDirectory: " + TargetGamePath);

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
                        using (FileStream currentFileStream = File.OpenRead(currentFile))
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

                            Log("[" + movedFiles.ToString() + "/" + FilesToMove.ToString() + "] Copied: " + newFileName);
                        }

                        progressBar_CopyStatus.PerformStep();
                    }

                    File.Copy(currentPath + acfName, TargetPath + acfName);
                    Log(".ACF file has been created at the target directory");

                    if (RemoveOld)
                    {
                        Directory.Delete(currentGamePath, true);
                        File.Delete(currentPath + acfName);

                        Log("Old files has been deleted.");
                    }

                    Log("Completed! All files successfully copied!");
                    button_Copy.Text = "Done!";

                    Functions.SteamLibrary.UpdateGameLibraries();
                }
                else
                    System.Windows.Forms.MessageBox.Show("We don't have enough perms at the target library path, try to run as Administrator maybe?");
            }
            catch { }
        }

        private void Log(string Text)
        {
            try
            {
                this.textBox_CopyLogs.AppendText(Text + "\n");
            }
            catch { }
        }


        private void button_Copy_Click(object sender, EventArgs e)
        {
            try
            {
                button_Copy.Enabled = false;

                this.CopyGame(Game.libraryPath, this.comboBox_TargetLibrary.SelectedItem.ToString(), Game.appName, Game.appID, this.checkbox_Validate.Checked, this.checkbox_RemoveOldFiles.Checked);
            }
            catch { }
        }
    }
}
