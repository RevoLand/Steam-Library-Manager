using MahApps.Metro;
using System.Windows;

namespace Steam_Library_Manager.Properties
{
    // This class allows you to handle specific events on the settings class:
    //  The SettingChanging event is raised before a setting's value is changed.
    //  The PropertyChanged event is raised after a setting's value is changed.
    //  The SettingsLoaded event is raised after the setting values are loaded.
    //  The SettingsSaving event is raised before the setting values are saved.
    [System.Configuration.SettingsProvider(typeof(Framework.PortableSettingsProvider))]
    internal sealed partial class Settings {

        public Settings() => PropertyChanged += Settings_PropertyChanged;

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
