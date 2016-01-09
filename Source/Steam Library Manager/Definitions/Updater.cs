using System;

namespace Steam_Library_Manager.Definitions
{
    class Updater
    {
        // Update control URL
        public static string VersionControlURL = "https://raw.githubusercontent.com/RevoLand/Steam-Library-Manager/master/Binaries/Version.txt";

        // GitHub Link
        public static string LatestVersionDownloadURL = "https://raw.githubusercontent.com/RevoLand/Steam-Library-Manager/master/Binaries/Steam%20Library%20Manager.exe";

        public static string externalFileDownloadURL = "https://github.com/RevoLand/Steam-Library-Manager/raw/master/Binaries/";

        // Current version, should be increased with each release
        public static Version CurrentVersion = new Version(System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).ProductVersion);

        // Latest SLM version, will be updated from UpdateLink
        public static Version LatestVersion = new Version();
    }
}
