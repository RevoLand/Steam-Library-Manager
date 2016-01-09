using System.Collections.Generic;
using System.Globalization;

namespace Steam_Library_Manager.Definitions
{
    // Our Library and Game definitions exists there
    class List
    {
        // Make a new list for Library details
        public static List<Library> Libraries = new List<Library>();

        // Make a new list for Game details
        public static List<Game> Games = new List<Game>();

        public static List<Language> Languages = new List<Language>();

        // Library details we are using, contains things like library path, game count etc.
        public class Library
        {
            public bool Main, Backup;
            public int GameCount;
            public string steamAppsPath, commonPath, downloadPath, workshopPath;

            public string fullPath { get; set; }
        }

        // Game details we are using, contains things like appID, installationPath etc.
        public class Game
        {
            public int appID;
            public Library Library;
            public string appName, installationPath, acfName, acfPath, commonPath, downloadPath, workShopPath, workShopAcfName, workShopAcfPath;
            public long sizeOnDisk;
            public bool Compressed;
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
