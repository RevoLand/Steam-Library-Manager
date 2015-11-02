using System.Collections.Generic;

namespace Steam_Library_Manager.Definitions
{
    // Our Library and Game definitions exists there
    class List
    {
        // Make a new list for Library details
        public static List<LibraryList> Library = new List<LibraryList>();

        // Make a new list for Game details
        public static List<GamesList> Game = new List<GamesList>();

        // Library details we are using, contains things like library path, game count etc.
        public class LibraryList
        {
            public bool Main, Backup;
            public int GameCount;
            public string fullPath, steamAppsPath, commonPath, downloadPath, workshopPath;
        }

        // Game details we are using, contains things like appID, installationPath etc.
        public class GamesList
        {
            public int appID;
            public LibraryList Library;
            public string appName, installationPath, acfName, acfPath, commonPath, downloadPath, workShopPath, workShopAcfName, workShopAcfPath;
            public long sizeOnDisk;
            public bool Compressed;
        }

    }
}
