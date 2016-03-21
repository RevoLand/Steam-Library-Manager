namespace Steam_Library_Manager.Functions
{
    class Steam
    {

        public static void updateSteamInstallationPath()
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.steamInstallationPath) || !System.IO.Directory.Exists(Properties.Settings.Default.steamInstallationPath))
            {
                Properties.Settings.Default.steamInstallationPath = Microsoft.Win32.Registry.GetValue(Definitions.Steam.RegistryKeyPath, "SteamPath", "").ToString().Replace('/', System.IO.Path.DirectorySeparatorChar);

                Definitions.Steam.vdfFilePath = System.IO.Path.Combine(Properties.Settings.Default.steamInstallationPath, "config", "config.vdf");
            }
        }

    }
}
