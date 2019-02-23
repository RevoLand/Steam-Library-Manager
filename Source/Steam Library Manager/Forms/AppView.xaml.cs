using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;

namespace Steam_Library_Manager.Forms
{
    /// <summary>
    /// Interaction logic for AppView.xaml
    /// </summary>
    public partial class AppView : UserControl
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public AppView() => InitializeComponent();

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
                {
                    if ((sender as Grid)?.DataContext is Definitions.SteamAppInfo)
                    {
                        if (((sender as Grid)?.DataContext as Definitions.SteamAppInfo)?.CommonFolder.Exists == true)
                        {
                            Process.Start(((sender as Grid)?.DataContext as Definitions.SteamAppInfo)?.CommonFolder.FullName);
                        }
                    }
                    else if ((sender as Grid)?.DataContext is Definitions.OriginAppInfo)
                    {
                        if (((sender as Grid)?.DataContext as Definitions.OriginAppInfo)?.InstallationDirectory.Exists == true)
                        {
                            Process.Start(((sender as Grid)?.DataContext as Definitions.OriginAppInfo)?.InstallationDirectory.FullName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
    }
}