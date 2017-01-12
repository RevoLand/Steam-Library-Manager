using System.Windows.Media;

namespace Steam_Library_Manager.Definitions
{
    public class ContextMenuItem
    {
        public bool IsActive { get; set; } = true;
        public string Header { get; set; }
        public string Action { get; set; }
        public FontAwesome.WPF.FontAwesomeIcon Icon { get; set; } = FontAwesome.WPF.FontAwesomeIcon.None;
        public Brush IconColor { get; set; }
        public Enums.MenuVisibility ShowToNormal { get; set; } = Enums.MenuVisibility.Visible;
        public Enums.MenuVisibility ShowToSLMBackup { get; set; } = Enums.MenuVisibility.Visible;
        public Enums.MenuVisibility ShowToSteamBackup { get; set; } = Enums.MenuVisibility.Visible;
        public Enums.MenuVisibility ShowToCompressed { get; set; } = Enums.MenuVisibility.Visible;
        public bool IsSeparator { get; set; }
    }
}
