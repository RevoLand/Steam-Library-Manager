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
                        Game.installationPath = Path.GetDirectoryName(game) + @"\common\" + Key["installdir"].Value + @"\";
                        Game.libraryPath = LibraryPath;
                        Game.sizeOnDisk = Key["SizeOnDisk"].Value; // Not 100% accurate but fast for now
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
