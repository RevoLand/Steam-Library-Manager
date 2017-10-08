using System.Windows.Media;

namespace Steam_Library_Manager.Definitions
{
    public class ContextMenuItem
    {
        public Enums.LibraryType LibraryType = Enums.LibraryType.Steam;
        public bool IsActive = true;
        public string Header;
        public string Action;
        public FontAwesome.WPF.FontAwesomeIcon Icon = FontAwesome.WPF.FontAwesomeIcon.None;
        public Brush IconColor = Brushes.Black;
        public bool ShowToNormal = true;
        public bool ShowToSLMBackup = true;
        public bool ShowToSteamBackup = true;
        public bool ShowToCompressed = true;
        public bool ShowToOffline = true;
        public bool IsSeparator;
    }
}
