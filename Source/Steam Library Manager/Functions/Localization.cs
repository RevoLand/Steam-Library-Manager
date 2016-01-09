using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace Steam_Library_Manager.Functions
{
    class Localization
    {
        public static Definitions.List.Language getLanguageFromShortName(string shortName) => Definitions.List.Languages.Find(x => x.shortName == shortName);

        public static Definitions.List.Language getDefaultLanguage() => Definitions.List.Languages.Find(x => x.isDefault);

        public static void updateLanguges()
        {
            if (Definitions.List.Languages == null)
                Definitions.List.Languages = new System.Collections.Generic.List<Definitions.List.Language>();

            Definitions.List.Languages.Add(new Definitions.List.Language()
            {
                shortName = "en",
                displayName = "English",
                culture = new System.Globalization.CultureInfo("en"),
                isDefault = true,
                requiresExternalFile = false
            });

            Definitions.List.Languages.Add(new Definitions.List.Language()
            {
                shortName = "tr",
                displayName = "Turkish",
                culture = new System.Globalization.CultureInfo("tr"),
                isDefault = true,
                requiresExternalFile = true,
                externalFileName = "Steam Library Manager.resources.dll"
            });

            if (Definitions.List.Languages.Count(x => x.shortName == Properties.Settings.Default.defaultLanguage) == 0)
                Properties.Settings.Default.defaultLanguage = getDefaultLanguage().shortName;

            Definitions.SLM.currentLanguage = getLanguageFromShortName(Properties.Settings.Default.defaultLanguage);

            downloadMissingFiles(Definitions.SLM.currentLanguage);
        }

        public static void updateCurrentLanguage(Definitions.List.Language selectedLanguage)
        {
            Properties.Settings.Default.defaultLanguage = selectedLanguage.shortName;
            Definitions.SLM.currentLanguage = selectedLanguage;
            Thread.CurrentThread.CurrentUICulture = selectedLanguage.culture;

            downloadMissingFiles(selectedLanguage);
        }

        public static void downloadMissingFiles(Definitions.List.Language selectedLanguage)
        {
            if (selectedLanguage.requiresExternalFile)
            {
                string langDirectory = Path.Combine(Definitions.Directories.SLM.CurrentDirectory, selectedLanguage.shortName);
                string langFile = Path.Combine(langDirectory, selectedLanguage.externalFileName);

                if (!File.Exists(langFile))
                {
                    WebClient fileDownloader = new WebClient();

                    if (!Directory.Exists(langDirectory))
                        Directory.CreateDirectory(langDirectory);

                    fileDownloader.DownloadFile(Path.Combine(Definitions.Updater.externalFileDownloadURL, selectedLanguage.shortName, selectedLanguage.externalFileName), langFile);
                }
            }
        }
    }
}
