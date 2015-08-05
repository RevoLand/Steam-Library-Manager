namespace Steam_Library_Manager.Definitions
{
    // Definitions about Steam
    class Steam
    {
        // Set maxLibraryCount to 255 which we are using while checking LibraryFolders.vdf
        public static int maxLibraryCount = 255;

        // Registry key from Steam, which is used to get Steam installation directory if user didn't set
        public static string RegistryKeyPath = @"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam";
    }
}
