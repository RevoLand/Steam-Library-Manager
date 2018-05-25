using MahApps.Metro.Controls.Dialogs;
using System;
using System.Diagnostics;
using System.Net;

namespace Steam_Library_Manager.Functions
{
    internal static class Updater
    {
        public static async void CheckForUpdates(bool InformUser = false)
        {
            try
            {
                // Create a new webclient
                var UpdaterClient = new WebClient();

                // Download update file contents
                string VersionFileContents = await UpdaterClient.DownloadStringTaskAsync(Definitions.Updater.VersionControlURL).ConfigureAwait(false);

                // If couldn't get file content (ex: not connected to web) return
                if (string.IsNullOrEmpty(VersionFileContents))
                {
                    return;
                }

                // Split file content by "|"
                string[] VersionFileContent = VersionFileContents.Split('|');

                // Update latest version
                Definitions.Updater.LatestVersion = new Version(VersionFileContent[0]);

                // If latest version is newer than current version
                if (Definitions.Updater.LatestVersion > Definitions.Updater.CurrentVersion)
                {
                    // If user would like to open GitHub page in browser and close SLM
                    if (await Main.FormAccessor.ShowMessageAsync("An update available for SLM", $"An update versioned ({Definitions.Updater.LatestVersion}) is available to download. Would you like to update SLM auto?", MessageDialogStyle.AffirmativeAndNegative).ConfigureAwait(true) == MessageDialogResult.Affirmative)
                    {
                        // Download latest version of SLM from GitHub and name it as LatestVersionSLM.exe
                        await UpdaterClient.DownloadFileTaskAsync(new Uri(Definitions.Updater.LatestVersionDownloadURL), Definitions.Directories.SLM.Current + "LatestVersionSLM.exe").ConfigureAwait(false);

                        // Use CMD with delay to rename file
                        // Define a process and start info to use with cmd
                        Process CmdProcess = new Process
                        {
                            // Set startinfo for cmd process
                            StartInfo = new ProcessStartInfo()
                            {
                                // Define cmd filename
                                FileName = "cmd.exe",

                                // Define working directory as current SLM path
                                WorkingDirectory = Definitions.Directories.SLM.Current,

                                // Hide CMD window
                                WindowStyle = ProcessWindowStyle.Hidden,

                                // Set CMD arguments
                                Arguments = $"/C ping 1.1.1.1 -n 1 -w 2000 > nul & move /y LatestVersionSLM.exe \"{AppDomain.CurrentDomain.FriendlyName}\" & msg %username% \"SLM is successfully updated(?)\""
                            }
                        };

                        // Start CMD process
                        CmdProcess.Start();

                        // Exit SLM completely
                        System.Windows.Application.Current.Shutdown();
                    }
                }
                else if (InformUser)
                {
                    await Main.FormAccessor.ShowMessageAsync("Steam Library Manager Updater", "You are using the latest version of SLM, thank you!").ConfigureAwait(true);
                }
            }
            catch (Exception ex)
            {
                Logger.LogToFile(Logger.LogType.SLM, ex.ToString());
            }
        }
    }
}