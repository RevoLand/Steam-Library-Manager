using ControlzEx;

namespace Steam_Library_Manager.Definitions
{
    public class ContextMenuItem
    {
        public readonly System.Collections.Generic.List<Enums.LibraryType> AllowedLibraryTypes = new System.Collections.Generic.List<Enums.LibraryType>();
        public bool IsActive = true;
        public string Header;
        public string Action;
        public PackIconBase Icon;
        public bool ShowToNormal = true;
        public bool ShowToSteamBackup = true;
        public bool ShowToCompressed = true;
        public bool ShowToOffline = true;
        public bool IsSeparator;
    }
}