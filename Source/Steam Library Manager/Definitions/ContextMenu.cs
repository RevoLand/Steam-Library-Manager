using System.Windows.Media;

namespace Steam_Library_Manager.Definitions
{

    public partial class List
    {

        public class ContextMenu
        {
            public bool IsActive { get; set; } = true;
            public string Header { get; set; }
            public string Action { get; set; }
            public FontAwesome.WPF.FontAwesomeIcon Icon { get; set; } = FontAwesome.WPF.FontAwesomeIcon.None;
            public Brush IconColor { get; set; }
            public Enums.menuVisibility ShowToNormal { get; set; } = Enums.menuVisibility.Visible;
            public Enums.menuVisibility ShowToSLMBackup { get; set; } = Enums.menuVisibility.Visible;
            public Enums.menuVisibility ShowToSteamBackup { get; set; } = Enums.menuVisibility.Visible;
            public Enums.menuVisibility ShowToCompressed { get; set; } = Enums.menuVisibility.Visible;
            public bool IsSeparator { get; set; }
        }

    }
}
