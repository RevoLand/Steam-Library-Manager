using System;

namespace Steam_Library_Manager.Definitions
{
    class Directories
    {
        public class SLM
        {
            public static string CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            public static string SettingsFile = CurrentDirectory + "Settings.ini";
        }

        public class Steam
        {
            public static string Path;
        }
    }
}
