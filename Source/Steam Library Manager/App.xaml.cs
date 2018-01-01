using MahApps.Metro;
using System.Windows;

namespace Steam_Library_Manager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
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
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
            }
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Exception.Data.Add(".Net", Functions.SLM.Get45PlusFromRegistry());
            MessageBox.Show(e.Exception.ToString(), "Exception Caught", MessageBoxButton.OK, MessageBoxImage.Error);
            System.Diagnostics.Debug.WriteLine(e.Exception);
            Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(e.Exception));
            e.Handled = true;
        }
    }


}
