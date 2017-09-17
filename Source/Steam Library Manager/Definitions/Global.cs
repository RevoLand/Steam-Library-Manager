namespace Steam_Library_Manager.Definitions
{
    class Global
    {
        public class Steam
        {
            // Registry key from Steam, which is used to get Steam installation directory if user didn't set
            public static string RegistryKeyPath = @"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam";

            public static string vdfFilePath = System.IO.Path.Combine(Properties.Settings.Default.steamInstallationPath, "config", "config.vdf");
        }
    }
}
