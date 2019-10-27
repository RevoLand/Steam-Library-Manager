using Alphaleonis.Win32.Filesystem;
using AutoUpdaterDotNET;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Steam_Library_Manager.Forms
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView
    {
        public SettingsView() => InitializeComponent();

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private void CheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AutoUpdater.Start(Definitions.Updater.VersionControlUrl, Application.ResourceAssembly);
                AutoUpdater.CheckForUpdateEvent += async args =>
                {
                    if (!args.IsUpdateAvailable)
                    {
                        await Main.FormAccessor
                            .ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.AutoUpdater)),
                                Functions.SLM.Translate(nameof(Properties.Resources.Updater_LatestVersionMessage)))
                            .ConfigureAwait(true);
                    }
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void ViewLogsButton(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(Definitions.Directories.SLM.Log))
            {
                Process.Start(Definitions.Directories.SLM.Log);
            }
        }

        private async void HeaderImageClearButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Directory.Exists(Definitions.Directories.SLM.Cache))
                {
                    foreach (var file in Directory.EnumerateFiles(Definitions.Directories.SLM.Cache, "*.jpg"))
                    {
                        File.Delete(file);
                    }
                }

                await Main.FormAccessor.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.Forms_Settings_HeaderImageCache)), Functions.SLM.Translate(nameof(Properties.Resources.Forms_Settings_HeaderImageCacheMessage))).ConfigureAwait(false);
            }
            catch { }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.Language = Gu.Localization.Translator.CurrentCulture.TwoLetterISOLanguageName;
        }
    }
}