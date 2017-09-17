using System;
using System.IO;

namespace Steam_Library_Manager.Definitions
{
    // Definitions about directories
    class Directories
    {
        // SLM directory definitions
        public class SLM
        {
            // Current running directory of SLM
            public static string Current = AppDomain.CurrentDomain.BaseDirectory;

            public static string Log = Path.Combine(Current, "Logs");

            // Set cache directory of SLM to %temp%/Assembly Name (Steam Library Manager)
            public static string Cache = Path.Combine(Path.GetTempPath(), System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);

            public static string HeaderImage = Path.Combine(Directories.SLM.Current, ".slmcache");
        }
    }
}
