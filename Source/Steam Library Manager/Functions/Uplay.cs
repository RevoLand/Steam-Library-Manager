using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Steam_Library_Manager.Functions
{
    internal static class Uplay
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static void PopulateLibraryCMenuItems()
        {
            #region App Context Menu Item Definitions

            var menuItem = new Definitions.ContextMenuItem
            {
                Header = "Open",
                Action = "Disk",
                Icon = FontAwesome.WPF.FontAwesomeIcon.FolderOpen
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Uplay);

            // Open library in explorer ({0})
            Definitions.List.LibraryCMenuItems.Add(menuItem);

            menuItem = new Definitions.ContextMenuItem
            {
                Header = "Remove from SLM",
                Action = "remove",
                Icon = FontAwesome.WPF.FontAwesomeIcon.Remove,
                ShowToNormal = false
            };

            menuItem.AllowedLibraryTypes.Add(Definitions.Enums.LibraryType.Uplay);

            Definitions.List.LibraryCMenuItems.Add(menuItem);

            #endregion App Context Menu Item Definitions
        }

        public static void GenerateLibraryListAsync()
        {
            if (File.Exists(Definitions.Global.Uplay.ConfigFilePath))
            {
                foreach (var line in File.ReadAllLines(Definitions.Global.Uplay.ConfigFilePath))
                {
                    if (!line.Contains("game_installation_path")) continue;

                    var newLine = line.Split(new[] { "  game_installation_path: " }, StringSplitOptions.RemoveEmptyEntries);

                    if (newLine.Length > 0)
                    {
                        if (Directory.Exists(newLine[0]))
                        {
                            AddNewLibraryAsync(newLine[0], true);
                        }
                        else
                        {
                            Logger.Info($"Directory set in the Uplay config file does not exists.\nDirectory path: {newLine[0]}");
                        }
                    }
                    else
                    {
                        Logger.Info("Couldn't parse Uplay config file for 'game_installation_path' - If you can find the value in the file, fill an issue form on github.");
                    }
                }
            }
            else
            {
                Logger.Info("Uplay config file is not found, skipping main uplay library detection.");
            }
        }

        public static async void AddNewLibraryAsync(string libraryPath, bool isMainLibrary = false)
        {
            try
            {
                if (!libraryPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    libraryPath += Path.DirectorySeparatorChar;
                }

                var newLibrary = new Definitions.UplayLibrary(libraryPath, isMainLibrary);

                Definitions.List.LibraryProgress.Report(newLibrary);

                await Task.Run(newLibrary.UpdateAppListAsync).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }

        public static bool IsLibraryExists(string newLibraryPath)
        {
            try
            {
                return Definitions.List.Libraries.Count(x => x.Type == Definitions.Enums.LibraryType.Uplay && string.Equals(x.FullPath, newLibraryPath, StringComparison.InvariantCultureIgnoreCase)) > 0;
            }
            // In any error return true to prevent possible bugs
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Logger.Fatal(ex);
                return true;
            }
        }

        public static async Task ParseAppDetailsAsync(Stream fileStream, string installerFilePath, Definitions.UplayLibrary library, bool isCompressed = false)
        {
            try
            {
                var sr = new StreamReader(fileStream);

                using (sr)
                {
                    while (!sr.EndOfStream)
                    {
                        var line = await sr.ReadLineAsync();

                        if (string.IsNullOrEmpty(line))
                            continue;

                        Debug.WriteLine(line);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                MessageBox.Show(ex.ToString());
            }
        }
    }
}