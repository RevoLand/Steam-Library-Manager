using MahApps.Metro.Controls.Dialogs;
using NLog;
using NLog.Targets.Wrappers;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
        private ObservableCollection<string> TmViewLogs { get; } = new ObservableCollection<string>();
        private Definitions.Enums.LibraryType libraryType;

        public readonly IProgress<string> TmLogs = new Progress<string>(log => FormAccessor.TmViewLogs.Add(log));

        public readonly IProgress<Definitions.Library> LibraryChange = new Progress<Definitions.Library>(library =>
        {
            try
            {
                if (library == null)
                    return;

                var search = Properties.Settings.Default.includeSearchResults
                    ? Properties.Settings.Default.SearchText
                    : null;
                if (Definitions.List.Libraries.Count(x => x == library) == 0 || !library.DirectoryInfo.Exists)
                {
                    FormAccessor.AppView.AppPanel.ItemsSource = null;
                    return;
                }

                var sortingMethod = Functions.SLM.Settings.GetSortingMethod(library);

                switch (library.Type)
                {
                    default:
                        FormAccessor.AppView.AppPanel.ItemsSource =
                            Properties.Settings.Default.defaultGameSortingMethod == "sizeOnDisk"
                            || Properties.Settings.Default.defaultGameSortingMethod == "LastUpdated"
                            || Properties.Settings.Default.defaultGameSortingMethod == "LastPlayed"
                                ? string.IsNullOrEmpty(search)
                                    ? library.Apps.OrderByDescending(sortingMethod).ToList()
                                    : library.Apps.Where(
                                        y => y.AppName.IndexOf(search, StringComparison.InvariantCultureIgnoreCase)
                                             >= 0 || y.AppId.ToString().Contains(search) // Search by app ID
                                    ).OrderByDescending(sortingMethod).ToList()
                                : string.IsNullOrEmpty(search)
                                    ? library.Apps.OrderBy(sortingMethod).ToList()
                                    : library.Apps.Where(
                                        y => y.AppName.IndexOf(search, StringComparison.InvariantCultureIgnoreCase)
                                             >= 0 || y.AppId.ToString().Contains(search) // Search by app ID
                                    ).OrderBy(sortingMethod).ToList();
                        break;
                }
            }
            catch (Exception ex)
            {
                FormAccessor.TmLogs.Report(ex.ToString());
            }
        });

        public Main()
        {
            Gu.Localization.Translator.Culture = System.Globalization.CultureInfo.GetCultureInfo(Properties.Settings.Default.Language);

            InitializeComponent();

            SetNLogConfig();
            UpdateBindings();
            MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Theme;
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
                System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;

                FormAccessor = this;
                Properties.Settings.Default.SearchText = "";

                LibraryView.LibraryPanel.ItemsSource = Definitions.List.Libraries;

                TaskManagerView.TaskPanel.ItemsSource = Functions.TaskManager.TaskList;
                TaskManagerView.TaskManagerInformation.DataContext = Functions.TaskManager.TMInfo;
                TaskManagerView.TaskManager_LogsView.ItemsSource = TmViewLogs;

                LibraryCleanerView.LibraryCleaner.ItemsSource = Definitions.List.LcItems;

                SettingsView.SteamUserIDList.ItemsSource = Definitions.List.SteamUserIDList;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void MainForm_Loaded(object sender, RoutedEventArgs e)
        {
            Functions.SLM.OnLoad();

            SettingsView.GeneralSettingsGroupBox.DataContext = new Definitions.Settings();
            QuickSettings.DataContext = SettingsView.GeneralSettingsGroupBox.DataContext;

            if (Properties.Settings.Default.Global_StartTaskManagerOnStartup)
            {
                Functions.TaskManager.Start();
            }
        }

        private async void MainForm_ClosingAsync(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (e.Cancel) return;
            if (Functions.TaskManager.TaskList.Count(x => x.Active) > 0)
            {
                e.Cancel = true;

                if (await this.ShowMessageAsync(Functions.SLM.Translate(nameof(Properties.Resources.Forms_QuitSLM)),
                    Functions.SLM.Translate(nameof(Properties.Resources.Forms_QuitSLMMessage)),
                    MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings()
                    {
                        AffirmativeButtonText = Functions.SLM.Translate(nameof(Properties.Resources.Forms_Quit)),
                        NegativeButtonText = Functions.SLM.Translate(nameof(Properties.Resources.Forms_Cancel))
                    }).ConfigureAwait(true) != MessageDialogResult.Affirmative)
                {
                    return;
                }
            }

            Functions.SLM.OnClosing();
            Application.Current.Shutdown();
        }

        public void LibraryCMenuItem_Click(object sender, RoutedEventArgs e) => ((Definitions.Library)(sender as MenuItem)?.DataContext)?.ParseMenuItemActionAsync((string)(sender as MenuItem)?.Tag);

        public void AppCMenuItem_Click(object sender, RoutedEventArgs e)
        {
            switch (Definitions.SLM.CurrentSelectedLibrary.Type)
            {
                default:
                    ((Definitions.App)(sender as MenuItem)?.DataContext)?.ParseMenuItemActionAsync((string)((MenuItem)sender)?.Tag);
                    break;
            }
        }

        private void AppSortingMethod_SelectionChanged(object sender, SelectionChangedEventArgs e) => Functions.App.UpdateAppPanel(Definitions.SLM.CurrentSelectedLibrary);

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Definitions.SLM.CurrentSelectedLibrary != null)
            {
                Functions.App.UpdateAppPanel(Definitions.SLM.CurrentSelectedLibrary);
            }
        }

        private void RightWindowCommands_DonateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                Process.Start(new ProcessStartInfo("cmd", $"/c start https://github.com/RevoLand/Steam-Library-Manager/wiki/Donations") { CreateNoWindow = true });
            }
            catch { }
        }

        private void RightWindowCommands_DiscordButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                Process.Start(new ProcessStartInfo("cmd", $"/c start https://discordapp.com/invite/Rwvs9Ng") { CreateNoWindow = true });
            }
            catch { }
        }

        private void RightWindowCommands_TranslateFormButton_Click(object sender, RoutedEventArgs e)
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            Process.Start(new ProcessStartInfo("cmd", $"/c start https://crowdin.com/project/steam-library-manager") { CreateNoWindow = true });
        }

        private void CreateLibrary_PathSelectionButtonClick(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    createLibrary_Path.Text = dialog.SelectedPath;
                }
            }
        }

        private void CreateLibrary_Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (createLibrary_Type.SelectedItem == null)
                    return;

                libraryType = (Definitions.Enums.LibraryType)createLibrary_Type.SelectedItem;
                createLibrary_TypeText.Text = libraryType.ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                TmLogs.Report(ex.ToString());
            }
        }

        private void CreateLibraryButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var libraryPath = createLibrary_Path.Text;

                if (string.IsNullOrEmpty(libraryPath))
                {
                    createLibrary_ResultText.Text = "Directory is not selected for library creation.";
                    return;
                }

                if (createLibrary_Type.SelectedItem == null)
                {
                    createLibrary_ResultText.Text = "Library type is not selected for library creation.";
                    return;
                }

                if (!Directory.Exists(libraryPath))
                {
                    createLibrary_ResultText.Text = $"Selected directory does not exists: \"{libraryPath}\"";
                    return;
                }
                switch (libraryType)
                {
                    case Definitions.Enums.LibraryType.Steam:
                        if (!Functions.Steam.Library.IsLibraryExists(libraryPath))
                        {
                            if (Directory.GetDirectoryRoot(libraryPath) != libraryPath)
                            {
                                Functions.Steam.Library.CreateNew(libraryPath, false);
                                createLibraryFlyout.IsOpen = false;
                            }
                            else
                            {
                                createLibrary_ResultText.Text = Functions.SLM.Translate(nameof(Properties.Resources.CreateLibrary_RootErrorMessage));
                            }
                        }
                        else
                        {
                            createLibrary_ResultText.Text = Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.CreateLibrary_ExistsMessage)), new { LibraryPath = libraryPath });
                        }
                        break;

                    case Definitions.Enums.LibraryType.SLM:
                        if (!Functions.SLM.Library.IsLibraryExists(libraryPath))
                        {
                            if (Directory.GetDirectoryRoot(libraryPath) != libraryPath)
                            {
                                Functions.SLM.Library.AddNewAsync(libraryPath);
                                createLibraryFlyout.IsOpen = false;
                            }
                            else
                            {
                                createLibrary_ResultText.Text = Functions.SLM.Translate(nameof(Properties.Resources.CreateLibrary_RootErrorMessage));
                            }
                        }
                        else
                        {
                            createLibrary_ResultText.Text = Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.CreateLibrary_ExistsMessage)), new { LibraryPath = libraryPath });
                        }
                        break;

                    case Definitions.Enums.LibraryType.Origin:
                        if (!Functions.Origin.IsLibraryExists(libraryPath))
                        {
                            if (Directory.GetDirectoryRoot(libraryPath) != libraryPath)
                            {
                                Functions.Origin.AddNewLibraryAsync(libraryPath);
                                createLibraryFlyout.IsOpen = false;
                            }
                            else
                            {
                                createLibrary_ResultText.Text = Functions.SLM.Translate(nameof(Properties.Resources.CreateLibrary_RootErrorMessage));
                            }
                        }
                        else
                        {
                            createLibrary_ResultText.Text = Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.CreateLibrary_ExistsMessage)), new { LibraryPath = libraryPath });
                        }
                        break;

                    case Definitions.Enums.LibraryType.Uplay:
                        createLibrary_ResultText.Text = "Selected library type is not implemented yet.";
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                TmLogs.Report(ex.ToString());
            }
        }

        private void CreateLibraryFlyout_IsOpenChanged(object sender, RoutedEventArgs e)
        {
            if (!createLibraryFlyout.IsOpen)
            {
                createLibrary_ResultText.Text = "";
                createLibrary_Path.Text = "";
                createLibrary_TypeText.Text = "";
                createLibrary_Type.SelectedItem = null;
            }
        }
    }
}