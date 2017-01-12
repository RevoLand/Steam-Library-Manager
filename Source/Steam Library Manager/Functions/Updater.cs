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
                if (string.IsNullOrEmpty(versionFileContents))
                    return;

                // Split file content by "|"
                string[] versionFileContent = versionFileContents.Split('|');

                // Update latest version
                Definitions.Updater.LatestVersion = new Version(versionFileContent[0]);

                // If latest version is newer than current version
                if (Definitions.Updater.LatestVersion > Definitions.Updater.CurrentVersion)
                {

                    // Show a messagebox to user and ask to open github page to download latest version
                    System.Windows.Forms.DialogResult updateMessageBox = System.Windows.Forms.MessageBox.Show(string.Format("An update versioned ({0}) is available to download. Would you like to update SLM auto?", Definitions.Updater.LatestVersion, versionFileContent[1], Environment.NewLine), "An update available for SLM", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Asterisk);

                    // If user would like to open GitHub page in browser and close SLM
                    if (updateMessageBox == System.Windows.Forms.DialogResult.Yes)
                    {
                        // Download latest version of SLM from GitHub and name it as LatestVersionSLM.exe
                        await UpdaterClient.DownloadFileTaskAsync(new Uri(Definitions.Updater.LatestVersionDownloadURL), Definitions.Directories.SLM.CurrentDirectory + "LatestVersionSLM.exe");

                        // Use CMD with delay to rename file
                        // Define a process and start info to use with cmd
                        Process cmdProcess = new Process();
                        ProcessStartInfo cmdStartInfo = new ProcessStartInfo()
                        {

                            // Define cmd filename
                            FileName = "cmd.exe",

                            // Define working directory as current SLM path
                            WorkingDirectory = Definitions.Directories.SLM.CurrentDirectory,

                            // Hide CMD window
                            WindowStyle = ProcessWindowStyle.Hidden,

                            // Set CMD arguments
                            Arguments = string.Format("/C ping 1.1.1.1 -n 1 -w 2000 > nul & move /y LatestVersionSLM.exe \"{0}\" & msg %username% \"{1}\"", AppDomain.CurrentDomain.FriendlyName, "SLM is successfully updated!?")
                        };

                        // Set startinfo for cmd process
                        cmdProcess.StartInfo = cmdStartInfo;

                        // Start CMD process
                        cmdProcess.Start();

                        // Exit SLM completely
                        System.Windows.Forms.Application.Exit();
                    }
                }
                //else
                    //MessageBox.Show("You are using the latest version of SLM, thank you!");
            }
            catch (Exception ex)
            {
                Logger.LogToFile(Logger.LogType.SLM, ex.ToString());
            }
        }
    }
}
