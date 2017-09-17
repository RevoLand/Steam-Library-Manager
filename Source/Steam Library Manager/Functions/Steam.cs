using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Steam_Library_Manager.Functions
{
    class Steam
    {
        public static void UpdateSteamInstallationPath()
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.steamInstallationPath) || !System.IO.Directory.Exists(Properties.Settings.Default.steamInstallationPath))
            {
                Properties.Settings.Default.steamInstallationPath = Registry.GetValue(Definitions.Global.Steam.RegistryKeyPath, "SteamPath", "").ToString().Replace('/', Path.DirectorySeparatorChar);

                if (string.IsNullOrEmpty(Properties.Settings.Default.steamInstallationPath))
                {
                    if (MessageBox.Show("Steam couldn't be found under registry. Would you like to locate Steam manually?", "Steam installation couldn't be found", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        OpenFileDialog SteamPathSelector = new OpenFileDialog()
                        {
                            Filter = "Steam (Steam.exe)|Steam.exe"
                        };

                        if (SteamPathSelector.ShowDialog() == true)
                        {
                            Properties.Settings.Default.steamInstallationPath = Path.GetDirectoryName(SteamPathSelector.FileName);
                        }
                    }
                }

                Definitions.Global.Steam.vdfFilePath = Path.Combine(Properties.Settings.Default.steamInstallationPath, "config", "config.vdf");
            }
        }

        public static bool IsSteamWorking()
        {
            try
            {
                Process[] SteamProcesses = Process.GetProcessesByName("Steam");

                if (SteamProcesses.Length > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Logger.LogToFile(Logger.LogType.SLM, ex.ToString());
                return true;
            }

        }

        public static async Task CloseSteamAsync()
        {
            try
            {
                if (IsSteamWorking())
                {
                    if (MessageBox.Show("Steam needs to be closed for this action. Would you like SLM to close Steam?", "Steam needs to be closed", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        if (File.Exists(Path.Combine(Properties.Settings.Default.steamInstallationPath, "steam.exe")))
                            Process.Start($"{Path.Combine(Properties.Settings.Default.steamInstallationPath, "steam.exe")}", "-shutdown");
                        else if (MessageBox.Show("Steam.exe could not found, SLM will try to terminate Steam processes now.", "Steam needs to be closed", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            Process[] SteamProcesses = Process.GetProcessesByName("Steam");

                            foreach (Process SteamProcess in SteamProcesses)
                            {
                                SteamProcess.Kill();
                            }
                        }
                        else
                            throw new Exception("Steam.exe could not found and user doesn't wants to terminate the process.");
                    }
                    else
                        throw new Exception("User doesn't wants to close Steam, can not continue to process.");

                    await Task.Delay(6000);
                }
            }
            catch (Exception ex)
            {
                Logger.LogToFile(Logger.LogType.SLM, ex.ToString());
                MessageBox.Show(ex.Message, ex.Source);
            }
        }

        public static async void RestartSteamAsync()
        {
            try
            {
                if (MessageBox.Show("Would you like to Restart Steam?", "Restart Steam?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    await CloseSteamAsync();

                    if (File.Exists(Path.Combine(Properties.Settings.Default.steamInstallationPath, "steam.exe")))
                        Process.Start($"{Path.Combine(Properties.Settings.Default.steamInstallationPath, "steam.exe")}");
                }
                else
                    throw new Exception("User doesn't wants to restart Steam.");
            }
            catch (Exception ex)
            {
                Logger.LogToFile(Logger.LogType.SLM, ex.ToString());
            }
        }
    }
}
