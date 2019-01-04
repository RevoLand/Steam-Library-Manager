using System;
using System.IO;

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

            // Set cache directory of SLM to %temp%/Assembly Name (Steam Library Manager)
            //public static string Cache = Path.Combine(Path.GetTempPath(), System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
            public static string Cache = Path.Combine(Current, ".slmcache");
        }

        public static class Origin
        {
            public static string LocalContentDirectoy = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Origin", "LocalContent");
        }
    }
}