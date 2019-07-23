using System.Collections.Generic;
namespace Steam_Library_Manager.Definitions
{
    internal static class Global
    {
        public static class Steam
        {
            // Registry key from Steam, which is used to get Steam installation directory if user didn't set
            public static string RegistryKeyPath = @"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam";

            public static string vdfFilePath = System.IO.Path.Combine(Properties.Settings.Default.steamInstallationPath, "config", "config.vdf");
        }

        public static class Origin
        {
            public static string ConfigFilePath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "Origin", "local.xml");
            public static List<KeyValuePair<string, string>> AppIds = new List<KeyValuePair<string, string>>();
        }
    }
}