using Gu.Localization;
using Steam_Library_Manager.Definitions.Enums;
using System;
using Alphaleonis.Win32.Filesystem;
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
                        return x => (Library.Type == LibraryType.Origin) ? x.AppName : x.IsCompressed;

                    case "LastUpdated":
                        return x => x.LastUpdated;

                    case "LastPlayed":
                        return x => (Library.Type == LibraryType.Origin) ? x.AppName : x.LastPlayed;
                }
            }

            private static void UpdateSlmLibraries()
            {
                try
                {
                    // Define a new string collection to update backup library settings
                    var backupDirs = new System.Collections.Specialized.StringCollection();

                    // foreach defined library in library list
                    foreach (var library in Definitions.List.Libraries.Where(x => x.Type == LibraryType.SLM).ToList())
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
                    foreach (var library in Definitions.List.Libraries.Where(x => x.Type == LibraryType.Origin && !x.IsMain).ToList())
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
                    foreach (var library in Definitions.List.Libraries.Where(x => x.Type == LibraryType.Uplay && !x.IsMain).ToList())
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
                if (Properties.Settings.Default.Steam_IsEnabled)
                {
                    UpdateSlmLibraries();
                }

                if (Properties.Settings.Default.Origin_IsEnabled)
                {
                    UpdateOriginLibraries();
                }

                if (Properties.Settings.Default.Uplay_IsEnabled)
                {
                    UpdateUplayLibraries();
                }

                UpdateJunkItems();
            }
        }

        public static async Task OnLoadAsync()
        {
            try
            {
                if (bool.Parse(Properties.Settings.Default.CheckforUpdatesAtStartup))
                {
                    AutoUpdaterDotNET.AutoUpdater.Start(Definitions.Updater.VersionControlUrl);
                }

                if (Properties.Settings.Default.Steam_IsEnabled)
                {
                    LoadSteam();
                    GenerateJunkList();
                }

                if (Properties.Settings.Default.Origin_IsEnabled)
                {
                    await LoadOriginAsync();
                }

                if (Properties.Settings.Default.Uplay_IsEnabled)
                {
                    await LoadUplayAsync();
                }

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

        public static bool LoadSteam()
        {
            try
            {
                if (Definitions.Global.Steam.IsStateChanging || Definitions.Global.Steam.Loaded)
                    return false;

                Definitions.Global.Steam.IsStateChanging = true;

                Steam.UpdateSteamInstallationPathAsync();
                Steam.PopulateLibraryCMenuItems();
                Steam.PopulateAppCMenuItems();

                Steam.Library.GenerateLibraryList();

                Library.GenerateLibraryList();

                Definitions.Global.Steam.IsStateChanging = false;
                Definitions.Global.Steam.Loaded = true;
                return true;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                Definitions.Global.Steam.IsStateChanging = false;
                return false;
            }
        }

        public static bool UnloadLibrary(LibraryType targetLibraryType)
        {
            try
            {
                switch (targetLibraryType)
                {
                    case LibraryType.Steam:
                    case LibraryType.SLM:
                        if (Definitions.Global.Steam.IsStateChanging)
                        {
                            return false;
                        }
                        else
                        {
                            Definitions.Global.Steam.IsStateChanging = true;
                            Definitions.Global.Steam.Loaded = false;
                        }
                        break;

                    case LibraryType.Origin:
                        if (Definitions.Global.Origin.IsStateChanging)
                        {
                            return false;
                        }
                        else
                        {
                            Definitions.Global.Origin.IsStateChanging = true;
                            Definitions.Global.Origin.Loaded = false;
                        }
                        break;

                    case LibraryType.Uplay:
                        if (Definitions.Global.Uplay.IsStateChanging)
                        {
                            return false;
                        }
                        else
                        {
                            Definitions.Global.Uplay.IsStateChanging = true;
                            Definitions.Global.Uplay.Loaded = false;
                        }
                        break;
                }

                if (TaskManager.TaskList.Count(x => (x.App.Library.Type == targetLibraryType || x.TargetLibrary.Type == targetLibraryType) && !x.Completed) > 0)
                {
                    Logger.Warn(Framework.StringFormat.Format(Translate(nameof(Properties.Resources.CantUnloadLibraryWithActiveTask)), new { targetLibraryType }));

                    ToggleOffLibrarySwitchState(targetLibraryType);
                    return false;
                }

                // Library Context Menu Items
                foreach (var menuItem in Definitions.List.LibraryCMenuItems.Where(x => x.AllowedLibraryTypes.Contains(targetLibraryType)).ToList())
                {
                    Definitions.List.LibraryCMenuItems.Remove(menuItem);
                }

                // App Context Menu Items
                foreach (var menuItem in Definitions.List.AppCMenuItems.Where(x => x.AllowedLibraryTypes.Contains(targetLibraryType)).ToList())
                {
                    Definitions.List.AppCMenuItems.Remove(menuItem);
                }

                foreach (var library in Definitions.List.Libraries.Where(x => x.Type == targetLibraryType).ToList())
                {
                    if (Definitions.SLM.CurrentSelectedLibrary == library)
                    {
                        Definitions.SLM.CurrentSelectedLibrary = null;
                        Main.FormAccessor.AppView.AppPanel.ItemsSource = null;
                    }

                    Definitions.List.Libraries.Remove(library);
                }

                ToggleOffLibrarySwitchState(targetLibraryType);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Logger.Fatal(ex);

                ToggleOffLibrarySwitchState(targetLibraryType);
                return false;
            }
        }

        private static void ToggleOffLibrarySwitchState(LibraryType targetLibraryType)
        {
            switch (targetLibraryType)
            {
                case LibraryType.Steam:
                case LibraryType.SLM:
                    Definitions.Global.Steam.IsStateChanging = false;
                    break;

                case LibraryType.Origin:
                    Definitions.Global.Origin.IsStateChanging = false;
                    break;

                case LibraryType.Uplay:
                    Definitions.Global.Uplay.IsStateChanging = false;
                    break;
            }
        }

        public static async Task<bool> LoadOriginAsync()
        {
            try
            {
                if (Definitions.Global.Origin.IsStateChanging || Definitions.Global.Origin.Loaded)
                    return false;

                Definitions.Global.Origin.IsStateChanging = true;

                Origin.PopulateLibraryCMenuItems();
                Origin.PopulateAppCMenuItems();

                await Origin.GenerateLibraryListAsync();

                Library.GenerateOriginLibraryList();

                Definitions.Global.Origin.IsStateChanging = false;
                Definitions.Global.Origin.Loaded = true;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Logger.Fatal(ex);
                Definitions.Global.Origin.IsStateChanging = false;
                return false;
            }
        }

        public static async Task<bool> LoadUplayAsync()
        {
            try
            {
                if (Definitions.Global.Uplay.IsStateChanging || Definitions.Global.Uplay.Loaded)
                    return false;

                Definitions.Global.Uplay.IsStateChanging = true;

                Uplay.PopulateLibraryCMenuItems();

                Uplay.PopulateAppCMenuItems();

                Uplay.UpdateInstallationPath();

                await Uplay.InitializeUplayDb();

                Uplay.GenerateLibraryList();

                Library.GenerateUplayLibraryList();

                Definitions.Global.Uplay.IsStateChanging = false;
                Definitions.Global.Uplay.Loaded = true;

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Logger.Fatal(ex);
                Definitions.Global.Uplay.IsStateChanging = false;

                return false;
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
                if (Properties.Settings.Default.IgnoredJunks == null)
                    return;

                if (Properties.Settings.Default.IgnoredJunks.Count == 0)
                    return;

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
                        Type = LibraryType.SLM,
                        DirectoryInfo = new DirectoryInfo(libraryPath)
                    };

                    Definitions.List.LibraryProgress.Report(library);

                    if (!Directory.Exists(libraryPath)) return;

                    await Task.Run(() => library.UpdateAppList()).ConfigureAwait(true);
                    await Task.Run(() => library.UpdateJunks()).ConfigureAwait(true);
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
                    return Definitions.List.Libraries.Any(x => string.Equals(x.DirectoryInfo.FullName, newLibraryPath, StringComparison.InvariantCultureIgnoreCase));
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

                    await Task.Run(library.UpdateAppList).ConfigureAwait(true);

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