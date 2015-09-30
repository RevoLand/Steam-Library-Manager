using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Steam_Library_Manager
{
    public partial class Main : Form
    {
        public Main()
        {
            try
            {
                InitializeComponent();

                // Set our accessor
                Definitions.Accessors.MainForm = this;

                // If Steam installation path is not set by user
                if (string.IsNullOrEmpty(Properties.Settings.Default.SteamInstallationPath))
                {
                    // Read Steam path from Registry
                    Properties.Settings.Default.SteamInstallationPath = Microsoft.Win32.Registry.GetValue(Definitions.Steam.RegistryKeyPath, "SteamPath", "").ToString();
                }

                // Update main form from settings
                Functions.Settings.UpdateMainForm();

                // Select game & library list as active tab
                tabControl1.SelectedTab = tab_InstalledGames;

                // Set form icon from resources
                Icon = Properties.Resources.steam_icon;

                // If allowed by user, check for updates
                if (Properties.Settings.Default.CheckForUpdatesAtStartup)
                    Functions.Updater.CheckForUpdates();
            }
            catch { }
        }

        private void linkLabel_SteamPath_LinkClicked(object sender, MouseEventArgs e)
        {
            try
            {
                // If the selected Steam installation path exists
                if (Directory.Exists(Properties.Settings.Default.SteamInstallationPath))
                    // Open the path in explorer as user requested
                    Process.Start(Properties.Settings.Default.SteamInstallationPath);
                else
                    // Else, do nothing
                    return;
            }
            catch { }
        }

        private void button_SelectSteamPath_Click(object sender, System.EventArgs e)
        {
            try
            {
                // Show file dialog to select Steam path
                fileDialog_SelectSteamPath.ShowDialog();
            }
            catch { }
        }

        // If file has been selected from file selection dialog (fileDialog_SelectSteamPath)
        private void fileDialog_SelectSteamPath_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                // Update our setting in memory
                Properties.Settings.Default.SteamInstallationPath = Path.GetDirectoryName(fileDialog_SelectSteamPath.FileName);

                // Update main form as visual
                Functions.Settings.UpdateMainForm();

                // Save settings
                Functions.Settings.Save();
            }
            catch { }
        }

        private void SLM_button_GameSizeCalcHelp_Click(object sender, EventArgs e)
        {
            // Show messagebox to user
            MessageBox.Show("ACF, uses the game size specified in {GameID}.ACF file, much faster than enumeration of game files but may not be accurate 100%\n\nEnum, enumerates all files in the game installation directory and check for file sizes so in a large game library it may take real long but 100% accurate\n\nTip: ACF is preferred, as because while copying or moving a game if any file fails while copying will cause the process to die and it will not delete any files from source dir, also you wouldn't try moving a game to full disk, would you? Well don't worry, you can try :P", "Game Size Calculation Method");
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Save user settings
            Functions.Settings.Save();
        }

        private void SLM_sizeCalculationMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Update setting value
                Properties.Settings.Default.GameSizeCalculationMethod = (SLM_sizeCalculationMethod.SelectedIndex == 0) ? "ACF" : "Enum";

                // Save settings to file
                Functions.Settings.Save();
            }
            catch { }
        }

        private void SLM_archiveSizeCalcMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Update setting value
                Properties.Settings.Default.ArchiveSizeCalculationMethod = (SLM_archiveSizeCalcMethod.SelectedIndex == 0) ? "Uncompressed" : "Archive";

                // Save settings to file
                Functions.Settings.Save();
            }
            catch { }
        }

        private void button_RefreshLibraries_Click(object sender, EventArgs e)
        {
            try
            {
                // Update game & backup libraries
                Functions.SteamLibrary.UpdateLibraries();

                // Clear current selected game library
                panel_GameList.Controls.Clear();
            }
            catch { }
        }


        private void button_CheckForUpdates_Click(object sender, EventArgs e)
        {
            try
            {
                // Check for updates manually
                Functions.Updater.CheckForUpdates();
            }
            catch { }
        }


        private void checkbox_LogErrorsToFile_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                // Update setting value
                Properties.Settings.Default.LogErrorsToFile = checkbox_LogErrorsToFile.Checked;

                // Save settings to file
                Functions.Settings.Save();
            }
            catch { }
        }

        private void checkbox_CheckForUpdatesAtStartup_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                // Update setting value
                Properties.Settings.Default.CheckForUpdatesAtStartup = checkbox_CheckForUpdatesAtStartup.Checked;

                // Save settings to file
                Functions.Settings.Save();
            }
            catch { }
        }

        private void newLibrary_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if it is a Backup or not from our button Tag
                bool Backup = Convert.ToBoolean((sender as Button).Tag);

                // If it is not Backup then set libraryType to Steam, else set it to Backup (only visual)
                string libraryType = (!Backup) ? "Steam" : "Backup";

                // Create a new dialog result and show to user
                DialogResult newLibrarySelection = folderBrowser_SelectNewLibraryPath.ShowDialog();

                // If our dialog is closed with OK (directory selected)
                if (newLibrarySelection == DialogResult.OK)
                {
                    // Define selected path for easier usage in future
                    string selectedPath = folderBrowser_SelectNewLibraryPath.SelectedPath;

                    // Check if the selected path is exists
                    if (!Functions.SteamLibrary.LibraryExists(selectedPath))
                    {
                        // If not exists then get directory root of selected path and see if it is equals with our selected path
                        if (Directory.GetDirectoryRoot(selectedPath) != selectedPath)
                            // If it is not equals then create a new library at selected path
                            Functions.SteamLibrary.CreateNewLibrary(selectedPath, Backup);
                        else
                            // Else show an error message to user
                            MessageBox.Show(libraryType + " Libraries can not be created in root");
                    }
                    else
                        // If selected path exists as library then show an error to user
                        MessageBox.Show("Library exists in the selected path! Are you trying to bug yourself?!");
                }
            }
            catch { }
        }

        private void SLM_SortGamesBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Update value in memory
                Properties.Settings.Default.SortGamesBy = SLM_SortGamesBy.SelectedItem.ToString();

                // Save settings to file
                Functions.Settings.Save();

                if (Definitions.SLM.LatestSelectedLibrary != null)
                    // Update main form with new settings
                    Functions.Games.UpdateMainForm(null, null);
            }
            catch { }
        }

        private void textBox_searchInGames_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (Definitions.SLM.LatestSelectedLibrary.GameCount == panel_GameList.Controls.Count && string.IsNullOrEmpty(textBox_searchInGames.Text))
                    return;

                Functions.Games.UpdateMainForm(null, textBox_searchInGames.Text);
            }
            catch { }
        }
    }
}
