using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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

        public static async Task GenerateLibraryListAsync()
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
                            await InitializeUplayDb();
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

        private static async Task InitializeUplayDb()
        {
            try
            {
                if (Definitions.List.UplayConfigurations.Count > 0)
                {
                    Definitions.List.UplayConfigurations.Clear();
                }

                using (var uplayDb =
                    new StreamReader(
                        Assembly.GetExecutingAssembly()
                            .GetManifestResourceStream("Steam_Library_Manager.Resources.configurations"),
                        Encoding.UTF8))
                {
                    var uplayDbEntry = new Definitions.UplayConfigurationDb();
                    var uplayDbEntryStarted = false;
                    while (!uplayDb.EndOfStream)
                    {
                        var line = await uplayDb.ReadLineAsync();

                        if (string.IsNullOrEmpty(line)) continue;

                        if (line.Contains("root:"))
                        {
                            if (uplayDbEntryStarted)
                            {
                                uplayDbEntry.Legacy = string.IsNullOrEmpty(uplayDbEntry.SpaceId);

                                if (!string.IsNullOrEmpty(uplayDbEntry.Name) &&
                                    !string.IsNullOrEmpty(uplayDbEntry.ThumbImage))
                                {
                                    Definitions.List.UplayConfigurations.Add(uplayDbEntry);
                                }

                                uplayDbEntry = new Definitions.UplayConfigurationDb();
                            }
                            else
                            {
                                uplayDbEntryStarted = true;
                            }
                        }

                        if (line.Contains("  name: ") && string.IsNullOrEmpty(uplayDbEntry.Name))
                        {
                            var name = line.Split(new[] { "name: " }, StringSplitOptions.RemoveEmptyEntries);

                            if (name.Length > 1 && !name[1].Contains("RELATED") && name[1].Length > 3)
                            {
                                uplayDbEntry.Name = name[1].Replace("\"", "");
                            }
                        }

                        if (line.Contains("game_identifier: "))
                        {
                            var identifier = line.Split(new[] { "  game_identifier: " },
                                StringSplitOptions.RemoveEmptyEntries);

                            if (identifier.Length > 1)
                            {
                                uplayDbEntry.Name = identifier[1];
                            }
                        }

                        // thumb_image:
                        if (line.Contains("THUMBIMAGE: "))
                        {
                            var thumbImage = line.Split(new[] { "    THUMBIMAGE: " },
                                StringSplitOptions.RemoveEmptyEntries);

                            if (thumbImage.Length > 0 &&
                                (thumbImage[0].Contains(".jpg") || thumbImage[0].Contains(".png")))
                            {
                                uplayDbEntry.ThumbImage = thumbImage[0].Replace(" ", "");
                            }
                        }

                        if (line.Contains("thumb_image:"))
                        {
                            var thumbImage = line.Split(new[] { "thumb_image:" }, StringSplitOptions.RemoveEmptyEntries);

                            if (thumbImage.Length > 1 &&
                                (thumbImage[1].Contains(".jpg") || thumbImage[1].Contains(".png")))
                            {
                                uplayDbEntry.ThumbImage = thumbImage[1].Replace(" ", "");
                            }
                        }

                        if (line.Contains("  space_id: "))
                        {
                            var spaceId = line.Split(new[] { "  space_id: " }, StringSplitOptions.RemoveEmptyEntries);

                            if (spaceId.Length > 0)
                            {
                                uplayDbEntry.SpaceId = spaceId[0];
                            }
                        }
                    }

                    Debug.WriteLine(
                        $"Total Entries in Uplay Configuration DB: {Definitions.List.UplayConfigurations.Count}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
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
                if (!newLibraryPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    newLibraryPath += Path.DirectorySeparatorChar;
                }

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

        public static void ParseAppDetails(string name, DirectoryInfo installationDirectory, Definitions.UplayLibrary library, bool isCompressed = false)
        {
            try
            {
                var gameDetails = Definitions.List.UplayConfigurations.FirstOrDefault(x => x.Name == name);
                if (gameDetails != null)
                {
                    library.Apps.Add(new Definitions.UplayAppInfo(library, gameDetails.Name, gameDetails.SpaceId, installationDirectory, gameDetails.ThumbImage, isCompressed));
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