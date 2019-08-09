using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace Steam_Library_Manager.Forms.InstallationWizard
{
    /// <summary>
    /// Interaction logic for Steam.xaml
    /// </summary>
    public partial class Library
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public Library()
        {
            InitializeComponent();
        }

        private void SteamInstallationPathSelector_OnClick(object sender, RoutedEventArgs e)
        {
            var steamPathSelector = new OpenFileDialog()
            {
                Filter = "Steam Executable (Steam.exe)|Steam.exe"
            };

            if (steamPathSelector.ShowDialog() != true) return;

            if (Directory.Exists(Path.GetDirectoryName(steamPathSelector.FileName)))
                Properties.Settings.Default.steamInstallationPath = Path.GetDirectoryName(steamPathSelector.FileName);

            if (!string.IsNullOrEmpty(Properties.Settings.Default.steamInstallationPath))
            {
                Definitions.Global.Steam.VdfFilePath = Path.Combine(Properties.Settings.Default.steamInstallationPath, "config", "config.vdf");
            }
        }

        private void UplayExecutablePathSelector_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var dialog = new System.Windows.Forms.OpenFileDialog())
                {
                    dialog.Filter = "Uplay executable file|Uplay.exe|All executable files (*.exe)|*.exe";
                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        Properties.Settings.Default.UplayExePath = dialog.FileName;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private async void UplayDbPathSelector_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var dialog = new System.Windows.Forms.OpenFileDialog())
                {
                    dialog.Filter = "Uplay Database File|configurations";
                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        Properties.Settings.Default.UplayDbPath = dialog.FileName;

                        if (File.Exists(Properties.Settings.Default.UplayDbPath))
                        {
                            await Functions.Uplay.InitializeUplayDb();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            throw new NotImplementedException();
        }

        private async void UplayDbPathClearButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Properties.Settings.Default.UplayDbPath = string.Empty;

                await Functions.Uplay.InitializeUplayDb();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}