using AutoUpdaterDotNET;

namespace Steam_Library_Manager.Functions
{
    internal static class Updater
    {
        public static void CheckForUpdates()
        {
            try
            {
                AutoUpdater.Start(Definitions.Updater.VersionControlURL, System.Windows.Application.ResourceAssembly);
                AutoUpdater.ShowUpdateForm();
            }
            catch { }
        }
    }
}