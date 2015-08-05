using System.Drawing;

namespace Steam_Library_Manager.Functions
{
    class Settings
    {
        public static void UpdateMainForm()
        {
            try
            {
                // Update Steam Path label
                Definitions.Accessors.MainForm.linkLabel_SteamPath.Text = Properties.Settings.Default.SteamInstallationPath;

                // Update game size calculation method selectbox
                Definitions.Accessors.MainForm.SLM_sizeCalculationMethod.SelectedIndex = (Properties.Settings.Default.GameSizeCalculationMethod != "ACF") ? 1 : 0;

                // Update archive size calculation method selectbox
                Definitions.Accessors.MainForm.SLM_archiveSizeCalcMethod.SelectedIndex = (!Properties.Settings.Default.ArchiveSizeCalculationMethod.StartsWith("Uncompressed")) ? 1 : 0;

                // Update log errors to file checkbox
                Definitions.Accessors.MainForm.checkbox_LogErrorsToFile.Checked = Properties.Settings.Default.LogErrorsToFile;

                // Check for Updates at Startup checkbox visual update
                Definitions.Accessors.MainForm.checkbox_CheckForUpdatesAtStartup.Checked = Properties.Settings.Default.CheckForUpdatesAtStartup;

                // Current Version text
                Definitions.Accessors.MainForm.label_CurrentVersion.Text = Definitions.Updater.CurrentVersion.ToString();

                // Find game directories and update them on form
                Functions.SteamLibrary.UpdateLibraries();
            }
            catch { }
        }

        public static void Save()
        {
            try
            {
                // Save settings to file
                Properties.Settings.Default.Save();
            }
            catch { }
        }

    }
}
