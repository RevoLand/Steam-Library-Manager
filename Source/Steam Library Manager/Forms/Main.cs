using System;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Forms;

namespace Steam_Library_Manager
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            
            // Set our accessor
            Definitions.Accessors.Main = this;

            // Update main form from settings
            Functions.Settings.UpdateMainForm();

            // Select game & library list as active tab
            tabControl1.SelectedTab = tab_InstalledGames;

        }

        private void linkLabel_SteamPath_LinkClicked(object sender, MouseEventArgs e)
        {
            try
            {
                if (Directory.Exists(Properties.Settings.Default.Steam_InstallationPath))
                    Process.Start(Properties.Settings.Default.Steam_InstallationPath);
                else
                    return;
            }
            catch { }
        }

        private void button_SelectSteamPath_Click(object sender, System.EventArgs e)
        {
            try
            {
                fileDialog_SelectSteamPath.ShowDialog();
            }
            catch { }
        }

        private void fileDialog_SelectSteamPath_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                Properties.Settings.Default.Steam_InstallationPath = Path.GetDirectoryName(fileDialog_SelectSteamPath.FileName) + @"\";

                Functions.Settings.UpdateMainForm();
            }
            catch { }
        }

        /*
        private void button_gameLibraries_AddNew_Click(object sender, System.EventArgs e)
        {
            DialogResult Result = folderBrowser_SelectNewLibraryPath.ShowDialog();
            if (Result == DialogResult.OK)
            {
                string selectedPath = folderBrowser_SelectNewLibraryPath.SelectedPath;

                if (!Functions.SteamLibrary.LibraryExists(selectedPath))
                {
                    if (Directory.GetDirectoryRoot(selectedPath) != selectedPath)
                        Functions.SteamLibrary.CreateNewLibrary(selectedPath);
                    else
                        MessageBox.Show("Steam Libraries can not be created in root");
                }
                else
                    MessageBox.Show("Library exists in the selected path! Are you trying to bug yourself?!");

            }
        }

        private void textBox_Search_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (textBox_Search.Text == "" || textBox_Search.Text == "Search...")
                {
                    if (listBox_InstalledGames.Items.Count != 0)
                        Functions.Games.UpdateMainForm();

                    return;
                }

                listBox_InstalledGames.DataSource = Definitions.List.Game.Where(x => Regex.IsMatch(x.appName, textBox_Search.Text, RegexOptions.IgnoreCase)).ToArray();
            }
            catch
            {
                if (listBox_InstalledGames.Items.Count != 0)
                    Functions.Games.UpdateMainForm();
            }
        }
        */

        private void SLM_sizeCalculationMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Properties.Settings.Default.SLM_GameSizeCalcMethod = (SLM_sizeCalculationMethod.SelectedItem.ToString().StartsWith("ACF")) ? "ACF" : "Enum";
            }
            catch { }
        }

        private void SLM_button_GameSizeCalcHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show("ACF, uses the game size specified in gameid.ACF file, much faster than enumeration of game files but may not be accurate 100%\n\nEnum, enumerates all files in the game installation directory and check for file sizes so in a large game library it may take real long but 100% accurate\n\nTip: ACF is preferred, as because while copying or moving a game if any file fails while copying will cause the process to die and it will not delete any files from source dir, also you wouldn't try moving a game to full disk, would you? Well don't worry, you can try :P", "Game Size Calculation Method");
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Save user settings
            Functions.Settings.Save();
        }

    }
}
