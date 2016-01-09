using System;
using System.Linq;

namespace Steam_Library_Manager.Functions
{
    class Settings
    {
        public static void UpdateMainForm()
        {
            try
            {
                // Update Steam Path label
                Definitions.Accessors.MainForm.linkLabel_SteamPath.Text = Properties.Settings.Default.SteamInstallationPath;

                // Update game size calculation method selectbox
                Definitions.Accessors.MainForm.SLM_sizeCalculationMethod.SelectedIndex = (Properties.Settings.Default.GameSizeCalculationMethod != "ACF") ? 1 : 0;

                // Update archive size calculation method selectbox
                Definitions.Accessors.MainForm.SLM_archiveSizeCalcMethod.SelectedIndex = (!Properties.Settings.Default.ArchiveSizeCalculationMethod.StartsWith("Uncompressed")) ? 1 : 0;

                Definitions.Accessors.MainForm.comboBox_moveGameMethod.SelectedIndex = (Properties.Settings.Default.methodForMovingGame == "forEach") ? 1 : 0;

                // Update sort games method by saved one
                Definitions.Accessors.MainForm.SLM_SortGamesBy.SelectedItem = Properties.Settings.Default.SortGamesBy;

                // Update log errors to file checkbox
                Definitions.Accessors.MainForm.checkbox_LogErrorsToFile.Checked = Properties.Settings.Default.LogErrorsToFile;

                // Check for Updates at Startup checkbox visual update
                Definitions.Accessors.MainForm.checkbox_CheckForUpdatesAtStartup.Checked = Properties.Settings.Default.CheckForUpdatesAtStartup;

                // Current Version text
                Definitions.Accessors.MainForm.label_CurrentVersion.Text = Definitions.Updater.CurrentVersion.ToString();

                // Default text editor
                if (string.IsNullOrEmpty(Properties.Settings.Default.DefaultTextEditor) || Properties.Settings.Default.DefaultTextEditor.Contains("%windir%\\notepad.exe"))
                    Properties.Settings.Default.DefaultTextEditor = System.IO.Path.Combine(Environment.SystemDirectory, "notepad.exe");

                // default text editor
                Definitions.Accessors.MainForm.SLM_defaultTextEditor.Text = Properties.Settings.Default.DefaultTextEditor;

                // default language
                Definitions.Accessors.MainForm.comboBox_defaultLanguage.SelectedItem = Localization.getLanguageFromShortName(Properties.Settings.Default.defaultLanguage);

                // vdf path
                Definitions.Steam.vdfFilePath = System.IO.Path.Combine(Properties.Settings.Default.SteamInstallationPath, "config", "config.vdf");

                // Find game directories and update them on form
                SteamLibrary.updateLibraryList();
            }
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log errors to DirectoryRemoval.txt
                    Log.ErrorsToFile("Settings", ex.ToString());
            }
        }

        public static void updateBackupDirs()
        {
            try
            {
                // Define a new string collection to update backup library settings
                System.Collections.Specialized.StringCollection BackupDirs = new System.Collections.Specialized.StringCollection();

                // foreach defined library in library list
                foreach (Definitions.List.Library Library in Definitions.List.Libraries.Where(x => x.Backup))
                {
                    // then add this library path to new defined string collection
                    BackupDirs.Add(Library.fullPath);
                }

                // change our current backup directories setting with new defined string collection
                Properties.Settings.Default.BackupDirectories = BackupDirs;

                // Save the settings to file
                Save();
            }
            catch (Exception ex)
            {
                // If user want us to log errors to file
                if (Properties.Settings.Default.LogErrorsToFile)
                    // Log errors to DirectoryRemoval.txt
                    Log.ErrorsToFile("Settings", ex.ToString());
            }
        }

        public static void Save()
        {
            // Save settings to file
            Properties.Settings.Default.Save();
        }

        public static Func<Definitions.List.Game, object> getSortingMethod()
        {
            Func<Definitions.List.Game, object> Sort;

            // Define our sorting method
            switch (Properties.Settings.Default.SortGamesBy)
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
    }
}
