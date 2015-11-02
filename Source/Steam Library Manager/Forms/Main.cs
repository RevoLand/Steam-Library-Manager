﻿using System;
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
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log errors to DirectoryRemoval.txt
                    Functions.Log.ErrorsToFile("MainForm", ex.ToString());
            }
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
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log errors to DirectoryRemoval.txt
                    Functions.Log.ErrorsToFile("MainForm", ex.ToString());
            }
        }

        private void button_SelectSteamPath_Click(object sender, EventArgs e)
        {
            try
            {
                // Show file dialog to select Steam path
                fileDialog_SelectSteamPath.ShowDialog();
            }
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log errors to DirectoryRemoval.txt
                    Functions.Log.ErrorsToFile("MainForm", ex.ToString());
            }
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
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log errors to DirectoryRemoval.txt
                    Functions.Log.ErrorsToFile("MainForm", ex.ToString());
            }
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
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log errors to DirectoryRemoval.txt
                    Functions.Log.ErrorsToFile("MainForm", ex.ToString());
            }
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
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log errors to DirectoryRemoval.txt
                    Functions.Log.ErrorsToFile("MainForm", ex.ToString());
            }
        }

        private void button_RefreshLibraries_Click(object sender, EventArgs e)
        {
            try
            {
                // Update game & backup libraries
                Functions.SteamLibrary.UpdateLibraryList();

                // Clear current selected game library
                panel_GameList.Controls.Clear();
            }
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log errors to DirectoryRemoval.txt
                    Functions.Log.ErrorsToFile("MainForm", ex.ToString());
            }
        }


        private void button_CheckForUpdates_Click(object sender, EventArgs e)
        {
            try
            {
                // Check for updates manually
                Functions.Updater.CheckForUpdates();
            }
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log errors to DirectoryRemoval.txt
                    Functions.Log.ErrorsToFile("MainForm", ex.ToString());
            }
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
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log errors to DirectoryRemoval.txt
                    Functions.Log.ErrorsToFile("MainForm", ex.ToString());
            }
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
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log errors to DirectoryRemoval.txt
                    Functions.Log.ErrorsToFile("MainForm", ex.ToString());
            }
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
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log errors to DirectoryRemoval.txt
                    Functions.Log.ErrorsToFile("MainForm", ex.ToString());
            }
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
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log errors to DirectoryRemoval.txt
                    Functions.Log.ErrorsToFile("MainForm", ex.ToString());
            }
        }

        private void textBox_searchInGames_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (Definitions.SLM.LatestSelectedLibrary == null || Definitions.SLM.LatestSelectedLibrary.GameCount == panel_GameList.Controls.Count && string.IsNullOrEmpty(textBox_searchInGames.Text))
                    return;

                Functions.Games.UpdateMainForm(null, textBox_searchInGames.Text);
            }
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log errors to DirectoryRemoval.txt
                    Functions.Log.ErrorsToFile("MainForm", ex.ToString());
            }
        }

        private void button_changeDefaultTextEditor_Click(object sender, EventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log errors to DirectoryRemoval.txt
                    Functions.Log.ErrorsToFile("MainForm", ex.ToString());
            }
        }
    }
}
