using System.Collections.Generic;

namespace Steam_Library_Manager.Definitions
{
    class List
    {
        public static List<LibraryList> Library = new List<LibraryList>();
        public static List<GamesList> Game = new List<GamesList>();

        public class LibraryList
        {
            public bool Main, Backup;
            public int GameCount;
            public string Directory;
        }

        public class GamesList
        {

            public int appID, StateFlag;
            public LibraryList Library;
            public string appName, installationPath, exactInstallPath, downloadPath, workshopPath;
            public long sizeOnDisk;
            public bool Compressed;
        }

    }
}
