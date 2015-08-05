using System;
using System.Diagnostics;
using System.Net;

namespace Steam_Library_Manager.Functions
{
    class Updater
    {
        public async static void CheckForUpdates()
        {
            try
            {
                // Create a new webclient
                var UpdaterClient = new WebClient();

                // Download update file contents
                string versionFileContents = await UpdaterClient.DownloadStringTaskAsync(Definitions.Updater.VersionControlURL);

                // If couldn't get file content (ex: not connected to web) return
                if (versionFileContents == null)
                    return;

                // Split file content by "|"
                string[] versionFileContent = versionFileContents.Split('|');

                // Update latest version
                Definitions.Updater.LatestVersion = new Version(versionFileContent[0]);

                // Latest Version text
                Definitions.Accessors.MainForm.label_LatestVersion.Text = string.Format("{0} ({1})", Definitions.Updater.LatestVersion, versionFileContent[1]);

                // If latest version is newer than current version
                if (Definitions.Updater.LatestVersion > Definitions.Updater.CurrentVersion)
                {
                    // Show a messagebox to user and ask to open github page to download latest version
                    System.Windows.Forms.DialogResult updateMessageBox = System.Windows.Forms.MessageBox.Show("There is an update available for SLM. Would you like to download it?\n\nClicking \"YES\" will download latest version of SLM close current instance of SLM and update\n\nLatest Version: " + Definitions.Updater.LatestVersion + " - Importance: " + versionFileContent[1], "SLM Update Checker", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Asterisk);

                    // If user would like to open GitHub page in browser and close SLM
                    if (updateMessageBox == System.Windows.Forms.DialogResult.Yes)
                    {
                        // Download latest version of SLM from GitHub and name it as LatestVersionSLM.exe
                        await UpdaterClient.DownloadFileTaskAsync(new Uri(Definitions.Updater.LatestVersionDownloadURL), Definitions.Directories.SLM.CurrentDirectory + "LatestVersionSLM.exe");

                        // Use CMD with delay to rename file
                        // Define a process and start info to use with cmd
                        Process cmdProcess = new Process();
                        ProcessStartInfo cmdStartInfo = new ProcessStartInfo();

                        // Define cmd filename
                        cmdStartInfo.FileName = "cmd.exe";

                        // Define working directory as current SLM path
                        cmdStartInfo.WorkingDirectory = Definitions.Directories.SLM.CurrentDirectory;

                        // Hide CMD window
                        cmdStartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                        // Set CMD arguments
                        cmdStartInfo.Arguments = "/C ping 1.1.1.1 -n 1 -w 1500 > nul & move /y LatestVersionSLM.exe \"" + System.AppDomain.CurrentDomain.FriendlyName  + "\" & msg %username% \"SLM Updated!\"";

                        // Set startinfo for cmd process
                        cmdProcess.StartInfo = cmdStartInfo;

                        // Start CMD process
                        cmdProcess.Start();

                        // Exit SLM completely
                        System.Windows.Forms.Application.Exit();
                    }
                    else if (versionFileContent[1] == "Important")
                        System.Windows.Forms.MessageBox.Show("IT IS NOT SUGGESTED TO SKIP AN IMPORTANT UPGRADE, YOU MAY LOSE DATA WHILE MOVING A GAME, BE AWARE!", "YOU SHOULD NOT SKIP AN IMPORTANT UPDATE", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                }
            }
            catch { }
        }
    }
}
