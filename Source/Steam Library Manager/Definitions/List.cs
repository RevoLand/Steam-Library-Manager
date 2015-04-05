using System.Collections.Generic;

namespace Steam_Library_Manager.Definitions
{
    class List
    {
        public static List<InstallDirsList> InstallDirs = new List<InstallDirsList>();
        public static List<GamesList> Games = new List<GamesList>();

        public class InstallDirsList
        {
            public bool Main;
            public int NumGames;
            public string Directory;
        }

        public class GamesList
        {
            public int appID { get; set; }
            public string appName { get; set; }

            public int StateFlag;
            public string installationPath, libraryPath, sizeOnDisk;
        }

    }
}
