using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;
using Steam_Library_Manager.Definitions.Enums;
using System;
using System.Diagnostics;
using System.Windows;

namespace Steam_Library_Manager.Properties
{
    // This class allows you to handle specific events on the settings class:
    //  The SettingChanging event is raised before a setting's value is changed.
    //  The PropertyChanged event is raised after a setting's value is changed.
    //  The SettingsLoaded event is raised after the setting values are loaded.
    //  The SettingsSaving event is raised before the setting values are saved.
    [System.Configuration.SettingsProvider(typeof(Framework.PortableSettingsProvider))]
    internal sealed partial class Settings
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public Settings()
        {
            PropertyChanged += Settings_PropertyChanged;
            SettingChanging += Settings_SettingChanging;
        }

        private async void Settings_SettingChanging(object sender, System.Configuration.SettingChangingEventArgs e)
        {
            try
            {
                if (e.SettingName == "Steam_IsEnabled")
                {
                    if (Default.Steam_IsEnabled == (bool)e.NewValue) return;

                    if (Definitions.Global.Steam.IsStateChanging)
                    {
                        Main.FormAccessor.AppView.AppPanel.Dispatcher?.Invoke(async delegate
                        {
                            await Main.FormAccessor.ShowMessageAsync("State is already changing!",
                                "State is already being changed for Steam libraries; please wait.",
                                MessageDialogStyle.AffirmativeAndNegative);
                        }, System.Windows.Threading.DispatcherPriority.Normal);
                        e.Cancel = true;
                    }
                    else
                    {
                        if ((bool)e.NewValue)
                        {
                            Functions.SLM.LoadSteam();
                        }
                        else
                        {
                            Functions.SLM.UnloadLibrary(LibraryType.Steam);
                            Functions.SLM.UnloadLibrary(LibraryType.SLM);
                        }

                        Main.FormAccessor.HamburgerMenuControl.Control.SelectedIndex = 0;
                        Main.FormAccessor.UpdateLibraryList("All");
                    }
                }
                else if (e.SettingName == "Origin_IsEnabled")
                {
                    if (Default.Origin_IsEnabled == (bool)e.NewValue) return;

                    if (Definitions.Global.Origin.IsStateChanging)
                    {
                        Main.FormAccessor.AppView.AppPanel.Dispatcher?.Invoke(async delegate
                        {
                            await Main.FormAccessor.ShowMessageAsync("State is already changing!",
                                "State is already being changed for Origin libraries; please wait.",
                                MessageDialogStyle.AffirmativeAndNegative);
                        }, System.Windows.Threading.DispatcherPriority.Normal);
                        e.Cancel = true;
                    }
                    else
                    {
                        if ((bool)e.NewValue)
                        {
                            await Functions.SLM.LoadOriginAsync();
                        }
                        else
                        {
                            Functions.SLM.UnloadLibrary(LibraryType.Origin);
                        }

                        Main.FormAccessor.HamburgerMenuControl.Control.SelectedIndex = 0;
                        Main.FormAccessor.UpdateLibraryList("All");
                    }
                }
                else if (e.SettingName == "Uplay_IsEnabled")
                {
                    if (Default.Uplay_IsEnabled == (bool)e.NewValue) return;

                    if (Definitions.Global.Uplay.IsStateChanging)
                    {
                        Main.FormAccessor.AppView.AppPanel.Dispatcher?.Invoke(async delegate
                        {
                            await Main.FormAccessor.ShowMessageAsync("State is already changing!",
                                "State is already being changed for Uplay libraries; please wait.",
                                MessageDialogStyle.AffirmativeAndNegative);
                        }, System.Windows.Threading.DispatcherPriority.Normal);
                        e.Cancel = true;
                    }
                    else
                    {
                        if ((bool)e.NewValue)
                        {
                            await Functions.SLM.LoadUplayAsync();
                        }
                        else
                        {
                            Functions.SLM.UnloadLibrary(LibraryType.Uplay);
                        }

                        Main.FormAccessor.HamburgerMenuControl.Control.SelectedIndex = 0;
                        Main.FormAccessor.UpdateLibraryList("All");
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                Logger.Fatal(exception);
                throw;
            }
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Save();

            if (e.PropertyName == "BaseTheme" || e.PropertyName == "ThemeAccent")
            {
                ThemeManager.ChangeAppStyle(Application.Current,
                                ThemeManager.GetAccent(ThemeAccent),
                                ThemeManager.GetAppTheme(BaseTheme));
            }
        }
    }
}