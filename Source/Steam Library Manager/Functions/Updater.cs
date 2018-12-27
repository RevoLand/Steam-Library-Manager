using AutoUpdaterDotNET;

namespace Steam_Library_Manager.Functions
{
    internal static class Updater
    {
        public static void CheckForUpdates()
        {
            AutoUpdater.Start(Definitions.Updater.VersionControlURL);
            AutoUpdater.ShowUpdateForm();
        }
    }
}