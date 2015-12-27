using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Resources;
using Steam_Library_Manager.Languages.Forms;

namespace Steam_Library_Manager
{

    public partial class MainForm : Form
    {
        public MainForm()
        {
            try
            {
                if (!string.IsNullOrEmpty(Properties.Settings.Default.defaultLanguage))
                    System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(Properties.Settings.Default.defaultLanguage, true);

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
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log errors to DirectoryRemoval.txt
                    Functions.Log.ErrorsToFile(mainForm.form_MainForm, ex.ToString());
            }
        }

        public static void SafeInvoke(Control control, Action handler)
        {
            if (control.InvokeRequired)
                control.Invoke(handler);
            else
                handler();
        }

        private void linkLabel_SteamPath_LinkClicked(object sender, MouseEventArgs e)
        {
            // If the selected Steam installation path exists
            if (Directory.Exists(Properties.Settings.Default.SteamInstallationPath))
                // Open the path in explorer as user requested
                Process.Start(Properties.Settings.Default.SteamInstallationPath);
            else
                // Else, do nothing
                return;
        }

        private void button_SelectSteamPath_Click(object sender, EventArgs e)
        {
            // Show file dialog to select Steam path
            fileDialog_SelectSteamPath.ShowDialog();
        }

        // If file has been selected from file selection dialog (fileDialog_SelectSteamPath)
        private void fileDialog_SelectSteamPath_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Update our setting in memory
            Properties.Settings.Default.SteamInstallationPath = Path.GetDirectoryName(fileDialog_SelectSteamPath.FileName);

            // Update main form as visual
            Functions.Settings.UpdateMainForm();

            // Save settings
            Functions.Settings.Save();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Save user settings
            Functions.Settings.Save();
        }

        private void SLM_sizeCalculationMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Update setting value
            Properties.Settings.Default.GameSizeCalculationMethod = (SLM_sizeCalculationMethod.SelectedIndex == 0) ? "ACF" : "Enum";

            // Save settings to file
            Functions.Settings.Save();
        }

        private void SLM_archiveSizeCalcMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Update setting value
            Properties.Settings.Default.ArchiveSizeCalculationMethod = (SLM_archiveSizeCalcMethod.SelectedIndex == 0) ? "Uncompressed" : "Archive";

            // Save settings to file
            Functions.Settings.Save();
        }

        private void button_RefreshLibraries_Click(object sender, EventArgs e)
        {
            // Update game & backup libraries
            Functions.SteamLibrary.updateLibraryList();

            // Clear current selected game library
            panel_GameList.Controls.Clear();
        }


        private void button_CheckForUpdates_Click(object sender, EventArgs e)
        {
            // Check for updates manually
            Functions.Updater.CheckForUpdates();
        }


        private void checkbox_LogErrorsToFile_CheckedChanged(object sender, EventArgs e)
        {
            // Update setting value
            Properties.Settings.Default.LogErrorsToFile = checkbox_LogErrorsToFile.Checked;

            // Save settings to file
            Functions.Settings.Save();
        }

        private void checkbox_CheckForUpdatesAtStartup_CheckedChanged(object sender, EventArgs e)
        {
            // Update setting value
            Properties.Settings.Default.CheckForUpdatesAtStartup = checkbox_CheckForUpdatesAtStartup.Checked;

            // Save settings to file
            Functions.Settings.Save();
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
                    if (!Functions.SteamLibrary.libraryExists(selectedPath))
                    {
                        // If not exists then get directory root of selected path and see if it is equals with our selected path
                        if (Directory.GetDirectoryRoot(selectedPath) != selectedPath)
                            // If it is not equals then create a new library at selected path
                            Functions.SteamLibrary.createNewLibrary(selectedPath, Backup);
                        else
                            // Else show an error message to user
                            MessageBox.Show($"{libraryType} {mainForm.message_noLibraryInDiskRoot}");
                    }
                    else
                        // If selected path exists as library then show an error to user
                        MessageBox.Show(mainForm.message_libraryExistsAtSelectedPath);
                }
            }
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log errors to DirectoryRemoval.txt
                    Functions.Log.ErrorsToFile(mainForm.form_MainForm, ex.ToString());
            }
        }

        private void SLM_SortGamesBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Update value in memory
            Properties.Settings.Default.SortGamesBy = SLM_SortGamesBy.SelectedItem.ToString();

            // Save settings to file
            Functions.Settings.Save();

            if (Definitions.SLM.LatestSelectedLibrary != null)
                // Update main form with new settings
                Functions.Games.UpdateMainForm(null, null, Definitions.SLM.LatestSelectedLibrary);
        }

        private void textBox_searchInGames_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (Definitions.SLM.LatestSelectedLibrary == null || Definitions.SLM.LatestSelectedLibrary.GameCount == panel_GameList.Controls.Count && string.IsNullOrEmpty(textBox_searchInGames.Text))
                    return;

                Functions.Games.UpdateMainForm(null, textBox_searchInGames.Text, Definitions.SLM.LatestSelectedLibrary);
            }
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log errors to DirectoryRemoval.txt
                    Functions.Log.ErrorsToFile(mainForm.form_MainForm, ex.ToString());
            }
        }

        private void button_changeDefaultTextEditor_Click(object sender, EventArgs e)
        {
            // Create a new dialog result and show to user
            DialogResult defaultTextEditorDialog = fileDialog_defaultTextEditor.ShowDialog();

            // If our dialog is closed with OK (directory selected)
            if (defaultTextEditorDialog == DialogResult.OK)
            {
                Properties.Settings.Default.DefaultTextEditor = fileDialog_defaultTextEditor.FileName;

                SLM_defaultTextEditor.Text = Properties.Settings.Default.DefaultTextEditor;

                Functions.Settings.Save();
            }
        }

        private void comboBox_defaultLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.defaultLanguage == comboBox_defaultLanguage.SelectedItem.ToString()) return;

            Properties.Settings.Default.defaultLanguage = comboBox_defaultLanguage.SelectedItem.ToString();

            MessageBox.Show(mainForm.message_restartToChangeLanguage);

            Functions.Settings.Save();
        }
    }
}
