using FontAwesome.WPF;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;

namespace Steam_Library_Manager.Definitions
{
    // Our Library and Game definitions exists there
    public class List
    {
        // Make a new list for Library details
        public static ObservableCollection<Library> Libraries = new ObservableCollection<Library>();

        public static List<Language> Languages = new List<Language>();

        public static List<rightClickMenuItem> rightClickMenuItems = new List<rightClickMenuItem>();

        // Library details we are using, contains things like library path, game count etc.
        public class Library
        {
            public bool Main { get; set; }
            public bool Backup { get; set; }
            public int GameCount { get; set; }
            public DirectoryInfo steamAppsPath, commonPath, downloadPath, workshopPath;
            public System.Windows.Controls.ContextMenu contextMenu { get; set; }
            public string fullPath { get; set; }
            public string prettyFreeSpace { get; set; }
            public int freeSpacePerc { get; set; }
            public long freeSpace { get; set; }
            public ObservableCollection<Game> Games = new ObservableCollection<Game>();
        }

        // Game details we are using, contains things like appID, installationPath etc.
        public class Game
        {
            public int appID { get; set; }
            public string appName { get; set; }
            public string gameHeaderImage { get; set; }
            public string prettyGameSize { get; set; }
            public DirectoryInfo installationPath, commonPath, downloadPath, workShopPath;
            public FileInfo acfPath, workShopAcfPath, compressedName;
            public string acfName, workShopAcfName;
            public long sizeOnDisk { get; set; }
            public System.Windows.Controls.ContextMenu contextMenu { get; set; }
            public bool Compressed { get; set; }
            public bool SteamBackup { get; set; }
            public Library Library { get; set; }
        }

        public class Language
        {
            public string shortName { get; set; }
            public string displayName { get; set; }

            public CultureInfo culture;
            public string externalFileName;
            public bool isDefault, requiresExternalFile;
        }

        public class rightClickMenuItem
        {
            public int order;
            public string DisplayText, Action;
            public FontAwesomeIcon icon = FontAwesomeIcon.None;
            public System.Windows.Media.Brush iconColor = System.Windows.Media.Brushes.Black;
            public bool IsSeperator, IsEnabled = true, ShownToBackup;
            public Library Library;
            public Game Game;
        }

    }
}
