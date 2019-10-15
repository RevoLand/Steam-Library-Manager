using System;
using Alphaleonis.Win32.Filesystem;

namespace Steam_Library_Manager.Definitions
{
    // Definitions about directories
    internal static class Directories
    {
        // SLM directory definitions
        public static class SLM
        {
            // Current running directory of SLM
            public static string Current = AppDomain.CurrentDomain.BaseDirectory;

            public static string Log = Path.Combine(Current, "logs");
            public static string Cache = Path.Combine(Current, ".slmcache");
        }

        public static class Origin
        {
            public static string LocalContentDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Origin", "LocalContent");
        }
    }
}