using System.ComponentModel;

namespace Steam_Library_Manager.Forms.InstallationWizard
{
    /// <summary>
    /// Interaction logic for Wizard.xaml
    /// </summary>
    public partial class Wizard
    {
        public Wizard()
        {
            InitializeComponent();
        }

        private void Wizard_OnClosing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.InstallationWizardShown = true;
        }
    }
}