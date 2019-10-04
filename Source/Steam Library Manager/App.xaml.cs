using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Threading;

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
                //ThemeManager.ChangeTheme(Current, "ThemeName?")

                base.OnStartup(e);

                Dispatcher.UnhandledException += OnDispatcherUnhandledException;
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

        private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"{e.Exception}\n\n{Environment.StackTrace}");
            Debug.WriteLine(e.Exception);
            Debug.WriteLine(Environment.StackTrace);
            //e.Handled = true;
        }
    }
}