using MahApps.Metro;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Steam_Library_Manager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                SetupExceptionHandling();

                var fileName = Path.Combine(Definitions.Directories.SLM.Cache, "CustomTheme.xaml");
                if (!File.Exists(fileName))
                {
                    CreateThemeFrom("CustomTheme.xaml", ThemeManager.GetAppTheme("BaseDark").Resources);
                }
                else
                {
                    var tryCount = 0;
                    while (fileName.IsFileLocked())
                    {
                        tryCount++;
                        Task.Delay(1000);

                        if (tryCount > 5)
                        {
                            base.OnStartup(e);
                            MessageBox.Show("CustomTheme.xaml is locked/not accessible. Skipping the custom theme loading.");
                            break;
                        }
                    }

                    ThemeManager.AddAppTheme("CustomTheme", new Uri(fileName, UriKind.Absolute));
                }

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
                Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
            }
        }

        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, e) =>
                LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");

            TaskScheduler.UnobservedTaskException += (s, e) =>
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
        }

        private void LogUnhandledException(Exception exception, string source)
        {
            string message = $"Unhandled exception ({source})";
            try
            {
                System.Reflection.AssemblyName assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
                message = string.Format("Unhandled exception in {0} v{1}", assemblyName.Name, assemblyName.Version);
                Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(exception));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception in LogUnhandledException");
            }
            finally
            {
                _logger.Error(exception, message);
            }
        }

        public static void CreateThemeFrom(string ThemeName, ResourceDictionary resourceDictionary)
        {
            try
            {
                FileInfo fileName = new FileInfo(Path.Combine(Definitions.Directories.SLM.Cache, ThemeName));

                if (!fileName.Directory.Exists)
                    fileName.Directory.Create();

                using (var writer = System.Xml.XmlWriter.Create(fileName.FullName, new System.Xml.XmlWriterSettings { Indent = true }))
                {
                    System.Windows.Markup.XamlWriter.Save(resourceDictionary, writer);
                }

                ThemeManager.AddAppTheme(ThemeName.Replace(".xaml", ""), new Uri(fileName.FullName, UriKind.Absolute));
                ThemeManager.ChangeAppStyle(Current, ThemeManager.GetAccent(Steam_Library_Manager.Properties.Settings.Default.ThemeAccent), ThemeManager.GetAppTheme(Steam_Library_Manager.Properties.Settings.Default.BaseTheme));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}