using MahApps.Metro.Controls.Dialogs;
using NLog;
using NLog.Targets.Wrappers;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Steam_Library_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///
    public partial class Main
    {
        public static Main FormAccessor;
        public Framework.AsyncObservableCollection<string> TaskManager_Logs = new Framework.AsyncObservableCollection<string>();
        //Framework.Network.Server SLMServer = new Framework.Network.Server();

        public Main()
        {
            InitializeComponent();

            SetNLogConfig();
            UpdateBindings();
            MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Accented;
        }

        private void SetNLogConfig()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var asyncWrapper = new AsyncTargetWrapper(new NLog.Targets.FileTarget() { ArchiveAboveSize = 10000000, FileName = "${basedir}/logs/${shortdate}.log", Name = "f", Layout = "${longdate} ${uppercase:${level}} ${message}" });

            config.LoggingRules.Add(new NLog.Config.LoggingRule("*", LogLevel.Debug, asyncWrapper));

            LogManager.Configuration = config;
        }

        private void UpdateBindings()
        {
            try
            {
                Definitions.SLM.RavenClient.Release = Definitions.Updater.CurrentVersion.ToString();
                FormAccessor = this;
                Properties.Settings.Default.SearchText = "";

                LibraryView.LibraryPanel.ItemsSource = Definitions.List.Libraries;

                TaskManagerView.TaskPanel.ItemsSource = Framework.TaskManager.TaskList;
                TaskManagerView.TaskManager_LogsView.ItemsSource = TaskManager_Logs;

                LibraryCleanerView.LibraryCleaner.ItemsSource = Definitions.List.LCItems;
            }
            catch { }
        }

        private void MainForm_Loaded(object sender, RoutedEventArgs e)
        {
            Functions.SLM.OnLoad();

            SettingsView.GeneralSettingsGroupBox.DataContext = new Definitions.Settings();
            QuickSettings.DataContext = SettingsView.GeneralSettingsGroupBox.DataContext;

            if (Properties.Settings.Default.Global_StartTaskManagerOnStartup)
            {
                Framework.TaskManager.Start();
            }
        }

        private async void MainForm_ClosingAsync(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (e.Cancel) return;
            if (Framework.TaskManager.TaskList.Count(x => x.Active) > 0)
            {
                e.Cancel = true;

                if (await this.ShowMessageAsync("Quit application?",
                    "There are active tasked jobs available in Task Manager. Are you sure you want to quit SLM? This might result in a data loss.",
                    MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings()
                    {
                        AffirmativeButtonText = "Quit",
                        NegativeButtonText = "Cancel"
                    }).ConfigureAwait(true) != MessageDialogResult.Affirmative)
                {
                    return;
                }
            }

            Functions.SLM.OnClosing();
            Application.Current.Shutdown();
        }

        public void LibraryCMenuItem_Click(object sender, RoutedEventArgs e) => ((Definitions.Library)(sender as MenuItem)?.DataContext).ParseMenuItemAction((string)(sender as MenuItem)?.Tag);

        public void AppCMenuItem_Click(object sender, RoutedEventArgs e)
        {
            switch (Definitions.SLM.CurrentSelectedLibrary.Type)
            {
                case Definitions.Enums.LibraryType.Steam:
                case Definitions.Enums.LibraryType.SLM:
                    ((Definitions.SteamAppInfo)(sender as MenuItem)?.DataContext).ParseMenuItemActionAsync((string)(sender as MenuItem)?.Tag);
                    break;

                case Definitions.Enums.LibraryType.Origin:
                    ((Definitions.OriginAppInfo)(sender as MenuItem)?.DataContext).ParseMenuItemActionAsync((string)(sender as MenuItem)?.Tag);
                    break;
            }
        }

        private void RightWindowCommands_SettingsButton_Click(object sender, RoutedEventArgs e) => TabItem_Settings.IsSelected = true;

        private void AppSortingMethod_SelectionChanged(object sender, SelectionChangedEventArgs e) => Functions.App.UpdateAppPanel(Definitions.SLM.CurrentSelectedLibrary);

        //private void GetIPButton_Click(object sender, RoutedEventArgs e) => Functions.Network.UpdatePublicIP();

        //private void GetPortButton_Click(object sender, RoutedEventArgs e) => Properties.Settings.Default.ListenPort = Functions.Network.GetAvailablePort();

        //private void ToggleSLMServerButton_Click(object sender, RoutedEventArgs e)
        //{
        //    //ToggleSLMServer.Content = "Stop Server";
        //    //SLMServer.StartServer();
        //}

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    Framework.Network.Client SLMClient = new Framework.Network.Client();

        //    SLMClient.ConnectToServer();
        //}

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Definitions.SLM.CurrentSelectedLibrary != null)
            {
                Functions.App.UpdateAppPanel(Definitions.SLM.CurrentSelectedLibrary);
            }
        }

        private void RightWindowCommands_PatreonButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.patreon.com/revoland");
        }

        private void RightWindowCommands_DiscordButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://discordapp.com/invite/Rwvs9Ng");
        }
    }
}