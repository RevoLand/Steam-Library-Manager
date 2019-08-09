using System;
using System.Diagnostics;
using System.Windows;

namespace Steam_Library_Manager.Forms.InstallationWizard
{
    /// <summary>
    /// Interaction logic for Final.xaml
    /// </summary>
    public partial class Final
    {
        public Final()
        {
            InitializeComponent();
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Window.GetWindow(this)?.Close();
            }
            catch (Exception)
            {
                Debug.WriteLine("Failed to get window for installation wizard?");
            }
        }
    }
}