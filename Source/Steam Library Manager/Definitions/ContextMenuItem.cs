using System.Windows.Media;

namespace Steam_Library_Manager.Definitions
{
    public class ContextMenuItem
    {
        public readonly System.Collections.Generic.List<Enums.LibraryType> AllowedLibraryTypes = new System.Collections.Generic.List<Enums.LibraryType>();
        public bool IsActive = true;
        public string Header;
        public string Action;
        public FontAwesome.WPF.FontAwesomeIcon Icon;
        public Brush IconColor = new SolidColorBrush((Color)MahApps.Metro.ThemeManager.DetectAppStyle(System.Windows.Application.Current).Item2.Resources["AccentColor"]);
        public bool ShowToNormal = true;
        public bool ShowToSLMBackup = true;
        public bool ShowToSteamBackup = true;
        public bool ShowToCompressed = true;
        public bool ShowToOffline = true;
        public bool IsSeparator;
    }
}