using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Steam_Library_Manager.Forms
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void CheckForUpdates_Click(object sender, RoutedEventArgs e) => Functions.Updater.CheckForUpdates(true);

        private void ViewLogsButton(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(Definitions.Directories.SLM.Log))
            {
                Process.Start(Definitions.Directories.SLM.Log);
            }
        }

        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            try
            {
                UpdateCustomTheme(((ColorPickerLib.Controls.ColorPicker)sender).Tag.ToString(), e.NewValue.Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private static SolidColorBrush GetSolidColorBrush(Color color, double opacity = 1d)
        {
            var brush = new SolidColorBrush(color) { Opacity = opacity };
            brush.Freeze();
            return brush;
        }

        private async void HeaderImageClearButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Directory.Exists(Definitions.Directories.SLM.Cache))
                {
                    foreach (string file in Directory.EnumerateFiles(Definitions.Directories.SLM.Cache, "*.jpg"))
                    {
                        File.Delete(file);
                    }
                }

                await Main.FormAccessor.ShowMessageAsync("Steam Library Manager", "Header Image Cache cleared.");
            }
            catch { }
        }

        private void DonateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(Definitions.SLM.DonateButtonURL);
            }
            catch { }
        }

        private void UpdateCustomTheme(string Key, Color value)
        {
            try
            {
                var Style = ThemeManager.AppThemes.FirstOrDefault(x => x.Name == "CustomTheme");

                switch (Key)
                {
                    case "TextBrush":
                        Style.Resources["BlackBrush"] = GetSolidColorBrush(value);
                        Style.Resources["LabelTextBrush"] = GetSolidColorBrush(value);
                        Style.Resources["TextBrush"] = GetSolidColorBrush(value);
                        Style.Resources["ControlTextBrush"] = GetSolidColorBrush(value);
                        Style.Resources["MenuTextBrush"] = GetSolidColorBrush(value);
                        break;

                    case "GrayNormalBrush":
                        Style.Resources["GrayNormalBrush"] = GetSolidColorBrush(value);
                        break;

                    case "WhiteBrush":
                    case "ControlBackgroundBrush":
                    case "WindowBackgroundBrush":
                    case "TransparentWhiteBrush":
                    case "GrayBrush1":
                    case "GrayBrush2":
                    case "GrayBrush7":
                    case "GrayBrush8":
                    case "GrayBrush10":
                        Style.Resources[Key] = GetSolidColorBrush(value);
                        break;

                    case "MenuItemBackgroundBrush":
                        Style.Resources[Key] = GetSolidColorBrush(value);
                        Style.Resources["ContextMenuBackgroundBrush"] = GetSolidColorBrush(value);
                        Style.Resources["Gray7"] = value;
                        break;
                }

                /*
                if (Path.Combine(Definitions.Directories.SLM.Cache, "CustomTheme").IsFileLocked())
                {
                    Debug.WriteLine("CustomTheme.xaml file is not accessible atm. Can't save the style!?");
                    return;
                }
                */

                App.CreateThemeFrom("CustomTheme.xaml", Style.Resources);

                if (Properties.Settings.Default.BaseTheme == "CustomTheme")
                {
                    ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(Properties.Settings.Default.ThemeAccent), Style);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                //MessageBox.Show(ex.ToString());
            }
        }
    }
}