using System.IO;

namespace Steam_Library_Manager.Functions
{
    class SteamLibrary
    {
        public static void UpdateGameLibraries()
        {
            try
            {
                // If we already have definitions in our list
                if (Definitions.List.InstallDirs.Count != 0)
                    // Clear them so they don't conflict
                    Definitions.List.InstallDirs.Clear();

                if (!File.Exists(Definitions.Directories.Steam.Path + "Steam.exe"))
                    return;

                // Our main library doesn't included in LibraryFolders.vdf so we have to include it manually
                Definitions.List.InstallDirsList InstallDir = new Definitions.List.InstallDirsList();

                // Tell it is our main game library which can be handy in future
                InstallDir.Main = true;

                // Define our library path to SteamApps
                InstallDir.Directory = Definitions.Directories.Steam.Path + @"SteamApps\";

                // Count how many games we have installed in our library
                InstallDir.NumGames = Functions.Games.GetGamesCountFromDirectory(InstallDir.Directory);

                // And add collected informations to our global list
                Definitions.List.InstallDirs.Add(InstallDir);


                Framework.KeyValue Key = new Framework.KeyValue();

                string filePath = Definitions.Directories.Steam.Path + @"SteamApps\libraryfolders.vdf";
                if (System.IO.File.Exists(filePath))
                {
                    Key.ReadFileAsText(filePath);

                    // Until someone gives a better idea, try to look for 255 Keys but break at first null key
                    for (int i = 1; i < Definitions.Steam.maxLibraryCount; i++)
                    {
                        if (Key[i.ToString()].Value == null)
                            break;

                        InstallDir = new Definitions.List.InstallDirsList();
                        InstallDir.Directory = Key[i.ToString()].Value + @"\SteamApps\";
                        InstallDir.NumGames = Functions.Games.GetGamesCountFromDirectory(InstallDir.Directory);
                        InstallDir.Main = false;
                        Definitions.List.InstallDirs.Add(InstallDir);
                    }
                }
                else
                {
                    // Could not locate LibraryFolders.vdf
                }

                // Update Libraries List visually
                UpdateMainForm();
            }
            catch { }
        }

        public static void UpdateMainForm()
        {
            try
            {
                if (Definitions.Accessors.Main.listBox_GameLibraries.Items.Count != 0)
                    Definitions.Accessors.Main.listBox_GameLibraries.Items.Clear();

                foreach (Definitions.List.InstallDirsList InstallDir in Definitions.List.InstallDirs)
                    Definitions.Accessors.Main.listBox_GameLibraries.Items.Add(InstallDir.Directory);

            }
            catch { }
        }

        public static bool LibraryExists(string Path)
        {
            try
            {
                foreach (Definitions.List.InstallDirsList Library in Definitions.List.InstallDirs)
                {
                    if (Library.Directory.Contains(Path)) // Should be tweaked more
                        return true;
                }
                return false;
            }
            catch { return true; }
        }

        public static void CreateNewLibrary(string newLibraryPath)
        {
            try
            {
                newLibraryPath = newLibraryPath + @"\";
                File.Copy(Definitions.Directories.Steam.Path + "Steam.dll", newLibraryPath + "steam.dll", true);
                Directory.CreateDirectory(newLibraryPath + "SteamApps");
                Directory.CreateDirectory(newLibraryPath + @"SteamApps\common");

                if (File.Exists(newLibraryPath + "steam.dll")) // in case of permissions denied
                {
                    string libraryFolders = Definitions.Directories.Steam.Path + @"SteamApps\libraryfolders.vdf";
                    Framework.KeyValue Key = new Framework.KeyValue();
                    Key.ReadFileAsText(libraryFolders);

                    Key.Children.Add(new Framework.KeyValue((Key.Children.Count - 1).ToString(), newLibraryPath));
                    Key.SaveToFile(libraryFolders, false);

                    System.Windows.Forms.MessageBox.Show("New Steam Library added, Please Restart Steam to see it in work."); // to-do: edit text

                    UpdateGameLibraries();
                }
                else
                    System.Windows.Forms.MessageBox.Show("Failed to create new Steam Library, Try to run SLM as Administrator?");

            }
            catch { }
        }
    }
}
