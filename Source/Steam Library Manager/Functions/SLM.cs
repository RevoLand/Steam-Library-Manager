using System;

namespace Steam_Library_Manager.Functions
{
    class SLM
    {
        public class Settings
        {
            public static Func<Definitions.List.Game, object> getSortingMethod()
            {
                Func<Definitions.List.Game, object> Sort;

                switch (Properties.Settings.Default.defaultGameSortingMethod)
                {
                    default:
                    case "appName":
                        Sort = x => x.appName;
                        break;
                    case "appID":
                        Sort = x => x.appID;
                        break;
                    case "sizeOnDisk":
                        Sort = x => x.sizeOnDisk;
                        break;
                }

                return Sort;
            }

            public static void saveSettings()
            {
                Properties.Settings.Default.Save();
            }
        }

        public static void onLoaded()
        {
            Steam.updateSteamInstallationPath();

            Library.generateLibraryList();
        }

        public static void onClosing()
        {
            Settings.saveSettings();
        }

    }
}
