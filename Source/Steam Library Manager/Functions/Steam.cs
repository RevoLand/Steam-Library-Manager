using Microsoft.Win32;
using System.Windows;

namespace Steam_Library_Manager.Functions
{
    class Steam
    {
        public static void updateSteamInstallationPath()
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.steamInstallationPath) || !System.IO.Directory.Exists(Properties.Settings.Default.steamInstallationPath))
            {
                Properties.Settings.Default.steamInstallationPath = Registry.GetValue(Definitions.Steam.RegistryKeyPath, "SteamPath", "").ToString().Replace('/', System.IO.Path.DirectorySeparatorChar);

                if (string.IsNullOrEmpty(Properties.Settings.Default.steamInstallationPath))
                {
                    MessageBoxResult selectSteamPath = MessageBox.Show("Steam couldn't be found under registry. Would you like to locate Steam manually?", "Steam installation couldn't be found", MessageBoxButton.YesNo);

                    if (selectSteamPath == MessageBoxResult.Yes)
                    {
                        OpenFileDialog steamPathSelector = new OpenFileDialog();
                        steamPathSelector.Filter = "Steam (Steam.exe)|Steam.exe";

                        if (steamPathSelector.ShowDialog() == true)
                        {
                            Properties.Settings.Default.steamInstallationPath = System.IO.Path.GetDirectoryName(steamPathSelector.FileName);
                        }
                    }
                }

                Definitions.Steam.vdfFilePath = System.IO.Path.Combine(Properties.Settings.Default.steamInstallationPath, "config", "config.vdf");
            }
        }

    }
}
