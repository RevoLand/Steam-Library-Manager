using System.Diagnostics;
using System.IO;
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

            // Read Settings
            Functions.Settings.Read();
        }

        private void linkLabel_SteamPath_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                if (Directory.Exists(Definitions.Directories.Steam.Path))
                    Process.Start("explorer.exe", Definitions.Directories.Steam.Path);
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
                Definitions.Directories.Steam.Path = Path.GetDirectoryName(fileDialog_SelectSteamPath.FileName) + @"\";

                linkLabel_SteamPath.Text = Definitions.Directories.Steam.Path;

                Functions.Settings.UpdateSetting("Steam", "InstallationPath", linkLabel_SteamPath.Text);
            }
            catch { }
        }

        private void listBox_GameLibraries_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            try
            {
                if (listBox_GameLibraries.SelectedIndex == -1 || Definitions.SLM.LatestSelectedLibrary == listBox_GameLibraries.SelectedItem.ToString())
                    return;

                Definitions.SLM.LatestSelectedLibrary = listBox_GameLibraries.SelectedItem.ToString();
                Functions.Games.UpdateGamesList(listBox_GameLibraries.SelectedItem.ToString());
            }
            catch { }
        }

        private void button_gameLibraries_Refresh_Click(object sender, System.EventArgs e)
        {
            try
            {
                Functions.SteamLibrary.UpdateGameLibraries();
            }
            catch { }
        }

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

        private void button_InstalledGames_MoveGame_Click(object sender, System.EventArgs e)
        {
            if (listBox_InstalledGames.SelectedIndex == -1)
                return;

            Forms.MoveGame MoveGameForm = new Forms.MoveGame();

            MoveGameForm.Show();
        }


    }
}
