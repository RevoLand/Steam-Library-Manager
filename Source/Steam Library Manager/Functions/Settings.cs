using System.IO;

namespace Steam_Library_Manager.Functions
{
    class Settings
    {
        public static void Create()
        {
            try
            {
                using (StreamWriter Settings = File.CreateText(Definitions.Directories.SLM.SettingsFile))
                {
                    Settings.WriteLine("[Steam]");
                    Settings.WriteLine("InstallationPath=");
                }
            }
            catch { }
        }

        public static void Read()
        {
            try
            {
                if (!File.Exists(Definitions.Directories.SLM.SettingsFile))
                    Create();

                Definitions.Directories.Steam.Path = Framework.INIFile.ReadValue("Steam", "InstallationPath", Definitions.Directories.SLM.SettingsFile);

                if (File.Exists(Definitions.Directories.Steam.Path + "Steam.exe"))
                    UpdateMainForm();
            }
            catch { }
        }

        public static void UpdateSetting(string Section, string Key, string Value)
        {
            try
            {
                Framework.INIFile.WriteValue(Section, Key, Value, Definitions.Directories.SLM.SettingsFile);
            }
            catch { }
        }

        public static void UpdateMainForm()
        {
            try
            {
                Definitions.Accessors.Main.linkLabel_SteamPath.Text = Definitions.Directories.Steam.Path;
                
                // Find game directories and update them on form
                Functions.SteamLibrary.UpdateGameLibraries();
            }
            catch { }
        }

    }
}
