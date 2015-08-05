using System.Collections.Generic;
using System.Drawing;

namespace Steam_Library_Manager.Definitions
{
    // Definitions about Steam Library Manager (SLM)
    class SLM
    {
        // Definitions we are using to pass library and game details to MoveGame form
        public static List.LibraryList LatestSelectedLibrary, LatestDropLibrary;
        public static List.GamesList LatestSelectedGame;

        // Update control URL
        public static string UpdateLink = "https://raw.githubusercontent.com/RevoLand/Steam-Library-Manager/master/Binaries/Version.txt";

        // GitHub Link
        public static string LatestVersionLink = "https://raw.githubusercontent.com/RevoLand/Steam-Library-Manager/master/Binaries/Steam%20Library%20Manager.exe";

        // SLM Version
        public static string CurrentVersion = "1.0.1", LatestVersion = "", LatestVersionImportance = "";

        // Version Importance Visual Colors
        public static Dictionary<string, Color> VersionImportanceColors = new Dictionary<string, Color>();

    }
}
