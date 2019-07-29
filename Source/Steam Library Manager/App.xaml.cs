using MahApps.Metro;
using System;
using System.Windows;

namespace Steam_Library_Manager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                ThemeManager.ChangeAppStyle(Current,
                                            ThemeManager.GetAccent(Steam_Library_Manager.Properties.Settings.Default.ThemeAccent),
                                            ThemeManager.GetAppTheme(Steam_Library_Manager.Properties.Settings.Default.BaseTheme));

                base.OnStartup(e);
            }
            catch (UnauthorizedAccessException ex)
            {
                base.OnStartup(e);
                MessageBox.Show(ex.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}