using System.Windows.Data;

namespace Steam_Library_Manager
{
    public class SettingBindingExtension : Binding
    {
        public SettingBindingExtension() => Initialize();

        public SettingBindingExtension(string path) : base(path) => Initialize();

        private void Initialize()
        {
            Source = Properties.Settings.Default;
            Mode = BindingMode.TwoWay;
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
        }
    }
}