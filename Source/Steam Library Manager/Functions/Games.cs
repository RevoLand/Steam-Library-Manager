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

                string[] gameList = Directory.GetFiles(LibraryPath, "*.acf", SearchOption.TopDirectoryOnly);

                foreach (string game in gameList)
                {
                    Framework.KeyValue Key = new Framework.KeyValue();
                    if (File.Exists(game)) // Just in case
                    {
                        Key.ReadFileAsText(game);

                        if (Key.Children.Count == 0)
                            continue;

                        Definitions.List.GamesList Game = new Definitions.List.GamesList();
                        Game.appID = Convert.ToInt32(Key["appID"].Value);
                        Game.appName = Key["name"].Value;
                        Game.StateFlag = Convert.ToInt16(Key["StateFlags"].Value);
                        switch (Game.StateFlag)
                        {
                            case 4: // Installed
                                Game.installationPath = Key["installdir"].Value;
                                Game.exactInstallPath = LibraryPath + @"common\" + Game.installationPath;
                                Game.sizeOnDisk = Functions.FileSystem.GetDirectorySize(Game.exactInstallPath, true);
                                break;
                            case 1024: // Pre-Load

                                break;
                            case 1026: // Downloading
                                Game.installationPath = @"downloading\" + Game.appID;
                                Game.exactInstallPath = LibraryPath + Game.installationPath;
                                break;
                            case 2: // Preparing for install
                            default:
                                Game.sizeOnDisk = 0;
                                break;
                        }
                        Game.libraryPath = LibraryPath;
                        
                        Definitions.List.Games.Add(Game);
                    }
                    else { }
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
