using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;

namespace Steam_Library_Manager.Forms
{
    /// <summary>
    /// Interaction logic for AppView.xaml
    /// </summary>
    public partial class AppView
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public AppView() => InitializeComponent();

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton != MouseButton.Left || e.ClickCount != 2) return;

                switch (((Grid)sender)?.DataContext)
                {
                    default:
                        {
                            var appInfo = (Definitions.App)((Grid)sender)?.DataContext;
                            if (appInfo?.InstallationDirectory.Exists == true)
                            {
                                Process.Start(appInfo?.InstallationDirectory.FullName);
                            }

                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}