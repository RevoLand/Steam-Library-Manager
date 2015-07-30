namespace Steam_Library_Manager.Functions
{
    class Settings
    {
        public static void UpdateMainForm()
        {
            try
            {
                // Update Steam Path label
                Definitions.Accessors.MainForm.linkLabel_SteamPath.Text = Properties.Settings.Default.Steam_InstallationPath;

                // Update game size calculation method selectbox
                Definitions.Accessors.MainForm.SLM_sizeCalculationMethod.SelectedIndex = (Properties.Settings.Default.SLM_GameSizeCalcMethod != "ACF") ? 1 : 0;

                // Update archive size calculation method selectbox
                Definitions.Accessors.MainForm.SLM_archiveSizeCalcMethod.SelectedIndex = (!Properties.Settings.Default.SLM_ArchiveSizeCalcMethod.StartsWith("Uncompressed")) ? 1 : 0;

                // Update log errors to file checkbox
                Definitions.Accessors.MainForm.checkbox_LogErrorsToFile.Checked = Properties.Settings.Default.SLM_LogErrorsToFile;

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
