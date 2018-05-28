using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Diagnostics;
using System.IO;
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
                Tuple<AppTheme, Accent> Style = ThemeManager.DetectAppStyle();

                switch (Key)
                {
                    case "TextBrush":
                        Style.Item1.Resources["BlackBrush"] = GetSolidColorBrush(value);
                        Style.Item1.Resources["LabelTextBrush"] = GetSolidColorBrush(value);
                        Style.Item1.Resources["TextBrush"] = GetSolidColorBrush(value);
                        Style.Item1.Resources["ControlTextBrush"] = GetSolidColorBrush(value);
                        Style.Item1.Resources["MenuTextBrush"] = GetSolidColorBrush(value);
                        break;

                    case "GrayNormalBrush":
                        Style.Item1.Resources["GrayNormalBrush"] = GetSolidColorBrush(value);
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
                        Style.Item1.Resources[Key] = GetSolidColorBrush(value);
                        break;

                    case "MenuItemBackgroundBrush":
                        Style.Item1.Resources[Key] = GetSolidColorBrush(value);
                        Style.Item1.Resources["ContextMenuBackgroundBrush"] = GetSolidColorBrush(value);
                        Style.Item1.Resources["Gray7"] = value;
                        break;
                }

                App.CreateThemeFrom("CustomTheme.xaml", Style.Item1.Resources);

                if (Properties.Settings.Default.BaseTheme == "CustomTheme")
                {
                    ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(Properties.Settings.Default.ThemeAccent), Style.Item1);
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