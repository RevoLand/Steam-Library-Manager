using Alphaleonis.Win32.Filesystem;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NLog;
using Steam_Library_Manager.Definitions.Enums;
using System;
using System.Collections.ObjectModel;
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
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private ObservableCollection<string> TmViewLogs { get; } = new ObservableCollection<string>();
        private LibraryType _libraryType;

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
                library.Logger.Fatal(ex);
            }
        });

        public Main()
        {
            SetNLogConfig();
            Gu.Localization.Translator.Culture = System.Globalization.CultureInfo.GetCultureInfo(Properties.Settings.Default.Language);

            InitializeComponent();

            if (!Properties.Settings.Default.InstallationWizardShown)
            {
                var installationWizard = new Forms.InstallationWizard.Wizard
                {
                    WizardControl = { DataContext = new Definitions.Settings() }
                };
                installationWizard.ShowDialog();
            }

            UpdateBindings();
            MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Theme;
        }

        private static void SetNLogConfig()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var asyncWrapper = new NLog.Targets.Wrappers.AsyncTargetWrapper(new NLog.Targets.FileTarget() { ArchiveAboveSize = 10000000, FileName = "${basedir}/logs/${shortdate}.log", Name = "f", Layout = "${longdate} ${uppercase:${level}} ${message}" });

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

                HamburgerMenuControl.Control.ItemClick += (sender, args) =>
                {
                    var clickedItemTag = ((HamburgerMenuIconItem)args.ClickedItem).Tag;
                    if (clickedItemTag == null) return;

                    UpdateLibraryList(clickedItemTag);
                };

                LibraryView.LibraryPanel.ItemsSource = Definitions.List.Libraries;

                TaskManagerView.TaskPanel.ItemsSource = Functions.TaskManager.TaskList;
                TaskManagerView.TaskManagerInformation.DataContext = Functions.TaskManager.TmInfo;
                TaskManagerView.TaskManager_LogsView.ItemsSource = TmViewLogs;

                // Library Cleaner View
                LibraryCleanerView.LibraryCleaner.ItemsSource = Definitions.List.JunkItems;
                LibraryCleanerView.DupeItems.ItemsSource = Definitions.List.DupeItems;
                LibraryCleanerView.IgnoredItems.ItemsSource = Definitions.List.IgnoredJunkItems;

                SettingsView.SteamUserIDList.ItemsSource = Definitions.List.SteamUserIdList;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Logger.Fatal(ex);
            }
        }

        public void UpdateLibraryList(object targetLibraryType)
        {
            try
            {
                if (Enum.TryParse<LibraryType>(targetLibraryType.ToString(), out var libraryTypeEnum))
                {
                    if (Definitions.SLM.CurrentSelectedLibrary != null && Definitions.SLM.CurrentSelectedLibrary.Type != libraryTypeEnum)
                    {
                        AppView.AppPanel.ItemsSource = null;
                    }

                    LibraryView.LibraryPanel.ItemsSource = libraryTypeEnum == LibraryType.Steam ? Definitions.List.Libraries.Where(x => x.Type == libraryTypeEnum || x.Type == LibraryType.SLM) : Definitions.List.Libraries.Where(x => x.Type == libraryTypeEnum);
                }

                HamburgerMenuControl.Control.IsPaneOpen = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Logger.Error(ex);
            }
        }

        private async void MainForm_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await Functions.SLM.OnLoadAsync();

                SettingsView.SettingsPanel.DataContext = new Definitions.Settings();
                QuickSettings.DataContext = SettingsView.SettingsPanel.DataContext;

                if (Properties.Settings.Default.Global_StartTaskManagerOnStartup)
                {
                    Functions.TaskManager.Start();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Logger.Fatal(ex);
            }
        }

        private async void MainForm_ClosingAsync(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
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
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Logger.Fatal(ex);
                Application.Current.Shutdown();
                Environment.Exit(0);
            }
        }

        public void LibraryCMenuItem_Click(object sender, RoutedEventArgs e) => ((Definitions.Library)(sender as MenuItem)?.DataContext)?.ParseMenuItemActionAsync((string)((MenuItem)sender)?.Tag);

        public void AppCMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ((Definitions.App)(sender as MenuItem)?.DataContext)?.ParseMenuItemActionAsync(
                    (string)((MenuItem)sender)?.Tag);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
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

                _libraryType = (LibraryType)createLibrary_Type.SelectedItem;
                createLibrary_TypeText.Text = _libraryType.ToString();
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
                    createLibrary_ResultText.Text = Functions.SLM.Translate(nameof(Properties.Resources.CreateLibraryButton_Click_DirectoryIsNotSelectedForLibraryCreation));
                    return;
                }

                if (createLibrary_Type.SelectedItem == null)
                {
                    createLibrary_ResultText.Text = Functions.SLM.Translate(nameof(Properties.Resources.CreateLibraryButton_Click_LibraryTypeIsNotSelectedForLibraryCreation));
                    return;
                }

                if (!Directory.Exists(libraryPath))
                {
                    createLibrary_ResultText.Text = Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.CreateLibraryButton_Click_SelectedDirectoryDoesNotExistsLibraryPath)), new { libraryPath });
                    return;
                }

                switch (_libraryType)
                {
                    case LibraryType.Steam:
                        if (!Properties.Settings.Default.Steam_IsEnabled)
                        {
                            createLibrary_ResultText.Text = Functions.SLM.Translate(nameof(Properties.Resources.CreateLibraryButton_Click_SteamLibrarySupportMustBeEnabledForThisAction));
                            return;
                        }

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

                    case LibraryType.SLM:
                        if (!Properties.Settings.Default.Steam_IsEnabled)
                        {
                            createLibrary_ResultText.Text = Functions.SLM.Translate(nameof(Properties.Resources.CreateLibraryButton_Click_SteamLibrarySupportMustBeEnabledForThisAction));
                            return;
                        }

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

                    case LibraryType.Origin:
                        if (!Properties.Settings.Default.Origin_IsEnabled)
                        {
                            createLibrary_ResultText.Text = Functions.SLM.Translate(nameof(Properties.Resources.CreateLibraryButton_Click_OriginLibrarySupportMustBeEnabledForThisAction));
                            return;
                        }

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

                    case LibraryType.Uplay:
                        if (!Properties.Settings.Default.Uplay_IsEnabled)
                        {
                            createLibrary_ResultText.Text = Functions.SLM.Translate(nameof(Properties.Resources.CreateLibraryButton_Click_UplayLibrarySupportMustBeEnabledForThisAction));
                            return;
                        }

                        if (!Functions.Uplay.IsLibraryExists(libraryPath))
                        {
                            if (Directory.GetDirectoryRoot(libraryPath) != libraryPath)
                            {
                                Functions.Uplay.AddNewLibraryAsync(libraryPath);
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
            if (createLibraryFlyout.IsOpen) return;

            createLibrary_ResultText.Text = "";
            createLibrary_Path.Text = "";
            createLibrary_TypeText.Text = "";
            createLibrary_Type.SelectedItem = null;
        }
    }
}