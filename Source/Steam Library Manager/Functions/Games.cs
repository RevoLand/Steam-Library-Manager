using System;
using System.IO;

namespace Steam_Library_Manager.Functions
{
    class Games
    {
        public static int GetGamesCountFromDirectory(string LibraryPath)
        {
            try
            {
                return Directory.GetFiles(LibraryPath, "*.acf", SearchOption.TopDirectoryOnly).Length;
            }
            catch { return 0; }
        }

        public static void UpdateGamesList(string LibraryPath)
        {
            try
            {
                if (Definitions.List.Games.Count != 0)
                    Definitions.List.Games.Clear();

                foreach (Framework.FileData game in Framework.FastDirectoryEnumerator.EnumerateFiles(LibraryPath, "*.acf", SearchOption.TopDirectoryOnly))
                {
                    Framework.KeyValue Key = new Framework.KeyValue();
                    Key.ReadFileAsText(game.Path);

                    if (Key.Children.Count == 0)
                        continue;

                    Definitions.List.GamesList Game = new Definitions.List.GamesList();
                    Game.appID = Convert.ToInt32(Key["appID"].Value);
                    Game.appName = Key["name"].Value;
                    Game.StateFlag = Convert.ToInt16(Key["StateFlags"].Value);
                    Game.installationPath = Key["installdir"].Value;
                    Game.libraryPath = LibraryPath;

                    if (Directory.Exists(LibraryPath + @"common\" + Game.installationPath))
                        Game.exactInstallPath = LibraryPath + @"common\" + Game.installationPath;
                    
                    if (Directory.Exists(LibraryPath + @"downloading\" + Game.appID))
                        Game.downloadPath = LibraryPath + @"downloading\" + Game.appID;

                    if (Game.exactInstallPath == null && Game.downloadPath == null)
                        continue; // Do not add pre-loads to list

                    if (Key["SizeOnDisk"].Value != "0")
                    {
                        if (Game.exactInstallPath != null)
                            Game.sizeOnDisk += Functions.FileSystem.GetDirectorySize(Game.exactInstallPath, true);

                        if (Game.downloadPath != null)
                            Game.sizeOnDisk += Functions.FileSystem.GetDirectorySize(Game.downloadPath, true);
                    }
                    else
                        Game.sizeOnDisk = 0;

                    Definitions.List.Games.Add(Game);
                }

                // Update main form as visual
                Functions.Games.UpdateMainForm();

            }
            catch { }
        }

        public static void UpdateMainForm()
        {
            try
            {
                if (Definitions.Accessors.Main.listBox_InstalledGames.Items.Count != 0)
                    Definitions.Accessors.Main.listBox_InstalledGames.DataSource = null;

                if (Definitions.List.Games.Count > 0)
                    Definitions.Accessors.Main.listBox_InstalledGames.DataSource = Definitions.List.Games;
                else
                    Definitions.Accessors.Main.listBox_InstalledGames.DataSource = null;

                Definitions.Accessors.Main.listBox_InstalledGames.DisplayMember = "appName";
                Definitions.Accessors.Main.listBox_InstalledGames.ValueMember = "appID";
            }
            catch { }
        }

    }
}
