
namespace Steam_Library_Manager.Functions
{
    class Settings
    {

        public static void UpdateMainForm()
        {
            try
            {
                Definitions.Accessors.Main.linkLabel_SteamPath.Text = Properties.Settings.Default.Steam_InstallationPath;
                Definitions.Accessors.Main.SLM_sizeCalculationMethod.SelectedIndex = (Properties.Settings.Default.SLM_GameSizeCalcMethod != "ACF") ? 1 : 0;

                // Find game directories and update them on form
                Functions.SteamLibrary.UpdateGameLibraries();
            }
            catch { }
        }

        public static void Save()
        {
            try
            {
                Properties.Settings.Default.Save();
            }
            catch { }
        }

    }
}
