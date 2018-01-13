using MahApps.Metro;
using System;
using System.IO;
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
                var fileName = Path.Combine(Definitions.Directories.SLM.Cache, "CustomTheme.xaml");
                if (!File.Exists(fileName))
                {
                    CreateThemeFrom("CustomTheme.xaml", ThemeManager.GetAppTheme("BaseDark").Resources);
                }
                else
                {
                    ThemeManager.AddAppTheme("CustomTheme", new Uri(fileName, UriKind.Absolute));
                }

                ThemeManager.ChangeAppStyle(Current,
                                            ThemeManager.GetAccent(Steam_Library_Manager.Properties.Settings.Default.ThemeAccent),
                                            ThemeManager.GetAppTheme(Steam_Library_Manager.Properties.Settings.Default.BaseTheme));

                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
            }
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString(), "Fatal Exception Caught", MessageBoxButton.OK, MessageBoxImage.Error);
            Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(e.Exception));
            e.Handled = true;
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
                    writer.Close();
                }

                ThemeManager.AddAppTheme(ThemeName.Replace(".xaml", ""), new Uri(fileName.FullName, UriKind.Absolute));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }


}