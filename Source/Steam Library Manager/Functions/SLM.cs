using Gu.Localization;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Steam_Library_Manager.Functions
{
    internal static class SLM
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static string Translate(string key)
        {
            return Translator.Translate(Properties.Resources.ResourceManager, key);
        }

        public static class Settings
        {
            public static Func<dynamic, object> GetSortingMethod(Definitions.Library Library)
            {
                switch (Properties.Settings.Default.defaultGameSortingMethod)
                {
                    case "appName":
                    default:
                        return x => x.AppName;

                    case "appID":
                        return x => x.AppId;

                    case "sizeOnDisk":
                        return x => x.SizeOnDisk;

                    case "backupType":
                        return x => (Library.Type == Definitions.Enums.LibraryType.Origin) ? x.AppName : x.IsCompressed;

                    case "LastUpdated":
                        return x => x.LastUpdated;

                    case "LastPlayed":
                        return x => (Library.Type == Definitions.Enums.LibraryType.Origin) ? x.AppName : x.LastPlayed;
                }
            }

            private static void UpdateSlmLibraries()
            {
                try
                {
                    // Define a new string collection to update backup library settings
                    var backupDirs = new System.Collections.Specialized.StringCollection();

                    // foreach defined library in library list
                    foreach (var library in Definitions.List.Libraries.Where(x => x.Type == Definitions.Enums.LibraryType.SLM).ToList())
                    {
                        if (backupDirs.Contains(library.DirectoryInfo.FullName))
                            continue;

                        // then add this library path to new defined string collection
                        backupDirs.Add(library.DirectoryInfo.FullName);
                    }

                    // change our current backup directories setting with new defined string collection
                    Properties.Settings.Default.backupDirectories = backupDirs;
                }
                catch (Exception ex)
                {
                    Logger.Fatal(ex);
                    MessageBox.Show(ex.ToString());
                }
            }

            private static void UpdateOriginLibraries()
            {
                try
                {
                    // Define a new string collection to update backup library settings
                    var backupDirs = new System.Collections.Specialized.StringCollection();

                    // foreach defined library in library list
                    foreach (var library in Definitions.List.Libraries.Where(x => x.Type == Definitions.Enums.LibraryType.Origin && !x.IsMain).ToList())
                    {
                        if (backupDirs.Contains(library.DirectoryInfo.FullName))
                            continue;

                        // then add this library path to new defined string collection
                        backupDirs.Add(library.DirectoryInfo.FullName);
                    }

                    // change our current backup directories setting with new defined string collection
                    Properties.Settings.Default.OriginLibraries = backupDirs;
                }
                catch (Exception ex)
                {
                    Logger.Fatal(ex);
                    MessageBox.Show(ex.ToString());
                }
            }

            private static void UpdateUplayLibraries()
            {
                try
                {
                    // Define a new string collection to update backup library settings
                    var backupDirs = new System.Collections.Specialized.StringCollection();

                    // foreach defined library in library list
                    foreach (var library in Definitions.List.Libraries.Where(x => x.Type == Definitions.Enums.LibraryType.Uplay && !x.IsMain).ToList())
                    {
                        if (backupDirs.Contains(library.DirectoryInfo.FullName))
                            continue;

                        // then add this library path to new defined string collection
                        backupDirs.Add(library.DirectoryInfo.FullName);
                    }

                    // change our current backup directories setting with new defined string collection
                    Properties.Settings.Default.UplayLibraries = backupDirs;
                }
                catch (Exception ex)
                {
                    Logger.Fatal(ex);
                    MessageBox.Show(ex.ToString());
                }
            }

            private static void UpdateJunkItems()
            {
                try
                {
                    // Define a new string collection to update backup library settings
                    var ignoredJunks = new System.Collections.Specialized.StringCollection();

                    // foreach defined library in library list
                    foreach (var junk in Definitions.List.IgnoredJunkItems.ToList())
                    {
                        if (ignoredJunks.Contains(junk))
                            continue;

                        // then add this library path to new defined string collection
                        ignoredJunks.Add(junk);
                    }

                    // change our current backup directories setting with new defined string collection
                    Properties.Settings.Default.IgnoredJunks = ignoredJunks;
                }
                catch (Exception ex)
                {
                    Logger.Fatal(ex);
                    MessageBox.Show(ex.ToString());
                }
            }

            public static void SaveSettings()
            {
                UpdateSlmLibraries();
                UpdateOriginLibraries();
                UpdateUplayLibraries();
                UpdateJunkItems();
            }
        }

        public static async Task OnLoadAsync()
        {
            try
            {
                if (bool.Parse(Properties.Settings.Default.CheckforUpdatesAtStartup))
                {
                    AutoUpdaterDotNET.AutoUpdater.Start(Definitions.Updater.VersionControlURL);
                }

                LoadSteam();
                await LoadOriginAsync();
                LoadUplay();

                // SLM Libraries
                Library.GenerateLibraryList();
                Library.GenerateOriginLibraryList();
                Library.GenerateUplayLibraryList();

                GenerateJunkList();

                if (Properties.Settings.Default.ParallelAfterSize >= 20000000)
                {
                    Properties.Settings.Default.ParallelAfterSize /= 1000000;
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }

        private static void LoadSteam()
        {
            try
            {
                Steam.UpdateSteamInstallationPathAsync();
                Steam.PopulateLibraryCMenuItems();
                Steam.PopulateAppCMenuItems();

                Steam.Library.GenerateLibraryList();
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }

        private static async Task LoadOriginAsync()
        {
            try
            {
                Origin.PopulateLibraryCMenuItems();
                Origin.PopulateAppCMenuItems();

                await Origin.GenerateLibraryListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Logger.Fatal(ex);
            }
        }

        private static void LoadUplay()
        {
            try
            {
                Uplay.PopulateLibraryCMenuItems();

                Uplay.GenerateLibraryListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Logger.Fatal(ex);
            }
        }

        public static void OnClosing()
        {
            Settings.SaveSettings();
            NLog.LogManager.Shutdown();
        }

        public static void GenerateJunkList()
        {
            try
            {
                // If we don't have any SLM libraries available
                if (Properties.Settings.Default.IgnoredJunks == null)
                    return;

                if (Properties.Settings.Default.IgnoredJunks.Count == 0)
                    return;

                // for each backup library we have, do a loop
                foreach (var ignoredJunk in Properties.Settings.Default.IgnoredJunks)
                {
                    Definitions.List.IgnoredJunkItems.Add(ignoredJunk);
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                MessageBox.Show(ex.ToString());
            }
        }

        // SLM Library
        public static class Library
        {
            public static void GenerateLibraryList()
            {
                try
                {
                    // If we don't have any SLM libraries available
                    if (Properties.Settings.Default.backupDirectories == null)
                        return;

                    if (Properties.Settings.Default.backupDirectories.Count == 0)
                        return;

                    // for each backup library we have, do a loop
                    foreach (var backupPath in Properties.Settings.Default.backupDirectories)
                    {
                        AddNewAsync(backupPath);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Fatal(ex);
                    MessageBox.Show(ex.ToString());
                }
            }

            public static void GenerateOriginLibraryList()
            {
                try
                {
                    // If we don't have any SLM libraries available
                    if (Properties.Settings.Default.OriginLibraries == null)
                        return;

                    if (Properties.Settings.Default.OriginLibraries.Count == 0)
                        return;

                    // for each backup library we have, do a loop
                    foreach (var backupPath in Properties.Settings.Default.OriginLibraries)
                    {
                        Origin.AddNewLibraryAsync(backupPath);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Fatal(ex);
                    MessageBox.Show(ex.ToString());
                }
            }

            public static void GenerateUplayLibraryList()
            {
                try
                {
                    // If we don't have any SLM libraries available
                    if (Properties.Settings.Default.UplayLibraries == null)
                        return;

                    if (Properties.Settings.Default.UplayLibraries.Count == 0)
                        return;

                    // for each backup library we have, do a loop
                    foreach (var backupPath in Properties.Settings.Default.UplayLibraries)
                    {
                        Uplay.AddNewLibraryAsync(backupPath);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Fatal(ex);
                    MessageBox.Show(ex.ToString());
                }
            }

            public static async void AddNewAsync(string libraryPath)
            {
                try
                {
                    if (string.IsNullOrEmpty(libraryPath))
                        return;

                    if (!libraryPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    {
                        libraryPath += Path.DirectorySeparatorChar;
                    }

                    var library = new Definitions.SteamLibrary(libraryPath)
                    {
                        Type = Definitions.Enums.LibraryType.SLM,
                        DirectoryInfo = new DirectoryInfo(libraryPath)
                    };

                    Definitions.List.LibraryProgress.Report(library);

                    if (!Directory.Exists(libraryPath)) return;

                    await Task.Run(() => library.UpdateAppListAsync()).ConfigureAwait(true);
                    await Task.Run(() => library.UpdateJunks()).ConfigureAwait(true);
                    await Task.Run(() => library.UpdateDupes()).ConfigureAwait(true);
                }
                catch (Exception ex)
                {
                    Logger.Fatal(ex);
                    MessageBox.Show(ex.ToString());
                }
            }

            public static bool IsLibraryExists(string newLibraryPath)
            {
                try
                {
                    return Definitions.List.Libraries.Count(x => x.Type == Definitions.Enums.LibraryType.SLM) > 0 || Definitions.List.Libraries.Any(x => string.Equals(x.DirectoryInfo.FullName, newLibraryPath, StringComparison.InvariantCultureIgnoreCase));
                }
                catch (Exception ex)
                {
                    Logger.Fatal(ex);
                    MessageBox.Show(ex.ToString());
                    return true;
                }
            }

            public static async void UpdateLibrary(Definitions.Library library)
            {
                try
                {
                    library.DirectoryInfo.Refresh();

                    await Task.Run(library.UpdateAppListAsync).ConfigureAwait(true);

                    library.UpdateDiskDetails();
                }
                catch (Exception ex)
                {
                    Logger.Fatal(ex);
                }
            }

            public static void UpdateLibraryVisual()
            {
                try
                {
                    Parallel.ForEach(Definitions.List.Libraries, libraryToUpdate => libraryToUpdate.UpdateDiskDetails());
                }
                catch (Exception ex)
                {
                    Logger.Fatal(ex);
                }
            }
        }
    }
}