using System.Windows.Controls;

namespace Steam_Library_Manager.Forms.InstallationWizard
{
    /// <summary>
    /// Interaction logic for Welcome.xaml
    /// </summary>
    public partial class Welcome
    {
        public Welcome()
        {
            InitializeComponent();
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.Language = Gu.Localization.Translator.CurrentCulture.TwoLetterISOLanguageName;
        }
    }
}