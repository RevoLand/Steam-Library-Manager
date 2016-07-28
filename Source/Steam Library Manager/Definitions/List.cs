using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Media;

namespace Steam_Library_Manager.Definitions
{
    // Our Library and Game definitions exists there
    public class List
    {
        // Make a new list for Library details
        public static Framework.AsyncObservableCollection<Library> Libraries = new Framework.AsyncObservableCollection<Library>();
        public static Framework.AsyncObservableCollection<contextMenu> libraryContextMenuItems = new Framework.AsyncObservableCollection<contextMenu>();
        public static Framework.AsyncObservableCollection<contextMenu> gameContextMenuItems = new Framework.AsyncObservableCollection<contextMenu>();

        public class Setting
        {
            public string steamInstallationPath { get; set; } = Properties.Settings.Default.steamInstallationPath;
            public System.Collections.Specialized.StringCollection backupDirectories { get; set; } = Properties.Settings.Default.backupDirectories;
            public SLM.Settings.GameSortingMethod defaultGameSortingMethod { get; set; } = (SLM.Settings.GameSortingMethod)Enum.Parse(typeof(SLM.Settings.GameSortingMethod), Properties.Settings.Default.defaultGameSortingMethod, true);
            public SLM.Settings.gameSizeCalculationMethod gameSizeCalculationMethod { get; set; } = (SLM.Settings.gameSizeCalculationMethod)Enum.Parse(typeof(SLM.Settings.gameSizeCalculationMethod), Properties.Settings.Default.gameSizeCalculationMethod, true);
            public SLM.Settings.archiveSizeCalculationMethod archiveSizeCalculationMethod { get; set; } = (SLM.Settings.archiveSizeCalculationMethod)Enum.Parse(typeof(SLM.Settings.archiveSizeCalculationMethod), Properties.Settings.Default.archiveSizeCalculationMethod, true);
            public long ParallelAfterSize { get; set; } = Properties.Settings.Default.ParallelAfterSize;
            public bool includeSearchResults { get; set; } = Properties.Settings.Default.includeSearchResults;
        }

        public class contextMenu
        {
            public bool IsActive { get; set; } = true;
            public string Header { get; set; }
            public string Action { get; set; }
            public FontAwesome.WPF.FontAwesomeIcon Icon { get; set; } = FontAwesome.WPF.FontAwesomeIcon.None;
            public Brush IconColor { get; set; }
            public SLM.Settings.menuVisibility showToNormal { get; set; } = SLM.Settings.menuVisibility.Visible;
            public SLM.Settings.menuVisibility showToSLMBackup { get; set; } = SLM.Settings.menuVisibility.Visible;
            public SLM.Settings.menuVisibility showToSteamBackup { get; set; } = SLM.Settings.menuVisibility.Visible;
            public SLM.Settings.menuVisibility showToCompressed { get; set; } = SLM.Settings.menuVisibility.Visible;
            public bool IsSeparator { get; set; }
        }

        public class Language
        {
            public string shortName { get; set; }
            public string displayName { get; set; }

            public CultureInfo culture;
            public string externalFileName;
            public bool isDefault, requiresExternalFile;
        }

    }
}
