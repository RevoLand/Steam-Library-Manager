using System;

namespace Steam_Library_Manager.Definitions
{
    public class Library
    {
        public Enums.LibraryType Type { get; set; }
        public System.IO.DirectoryInfo DirectoryInfo { get; set; }

        public SteamLibrary Steam { get; set; }
    }
}
