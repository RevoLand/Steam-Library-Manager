using System;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace Steam_Library_Manager.Functions
{
    class Games
    {
        public static int GetGamesCountFromDirectory(Definitions.List.LibraryList Library)
        {
            try
            {
                int gameCount = 0;
                if (Library.Backup)
                    gameCount += Directory.GetFiles(Library.Directory, "*.zip", SearchOption.TopDirectoryOnly).Length;

                gameCount += Directory.GetFiles(Library.Directory, "*.acf", SearchOption.TopDirectoryOnly).Length;

                return gameCount;
            }
            catch { return 0; }
        }

        public static void UpdateGamesList(Definitions.List.LibraryList Library)
        {
            try
            {
                if (Definitions.List.Game.Count != 0)
                    Definitions.List.Game.Clear();

                foreach (Framework.FileData game in Framework.FastDirectoryEnumerator.EnumerateFiles(Library.Directory, "*.acf", SearchOption.TopDirectoryOnly))
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
                    Game.Library = Library;

                    if (Directory.Exists(Library.Directory + @"common\" + Game.installationPath))
                        Game.exactInstallPath = Library.Directory + @"common\" + Game.installationPath;

                    if (Directory.Exists(Library.Directory + @"downloading\" + Game.appID))
                        Game.downloadPath = Library.Directory + @"downloading\" + Game.appID;

                    if (Game.exactInstallPath == null && Game.downloadPath == null)
                        continue; // Do not add pre-loads to list

                    if (Key["SizeOnDisk"].Value != "0")
                    {
                        if (Game.exactInstallPath != null)
                        {
                            if (Properties.Settings.Default.SLM_GameSizeCalcMethod != "ACF")
                                Game.sizeOnDisk += Functions.FileSystem.GetDirectorySize(Game.exactInstallPath, true);
                            else
                                Game.sizeOnDisk += Convert.ToInt64(Key["SizeOnDisk"].Value);
                        }

                        if (Game.downloadPath != null)
                        {
                            if (Properties.Settings.Default.SLM_GameSizeCalcMethod != "ACF")
                                Game.sizeOnDisk += Functions.FileSystem.GetDirectorySize(Game.downloadPath, true);
                        }
                    }
                    else
                        Game.sizeOnDisk = 0;

                    Definitions.List.Game.Add(Game);
                }

                if (Library.Backup)
                {
                    foreach (Framework.FileData gameArchive in Framework.FastDirectoryEnumerator.EnumerateFiles(Library.Directory, "*.zip", SearchOption.TopDirectoryOnly))
                    {
                        using (ZipArchive compressedArchive = ZipFile.OpenRead(gameArchive.Path))
                        {
                            foreach (ZipArchiveEntry file in compressedArchive.Entries)
                            {
                                if (file.Name.Contains(".acf"))
                                {
                                    Framework.KeyValue Key = new Framework.KeyValue();
                                    Key.ReadAsText(file.Open());

                                    if (Key.Children.Count == 0)
                                        continue;

                                    Definitions.List.GamesList Game = new Definitions.List.GamesList();
                                    Game.appID = Convert.ToInt32(Key["appID"].Value);
                                    Game.appName = Key["name"].Value;
                                    Game.Compressed = true;
                                    Game.StateFlag = Convert.ToInt16(Key["StateFlags"].Value);
                                    Game.installationPath = Key["installdir"].Value;
                                    Game.Library = Library;
                                    Game.exactInstallPath = Library.Directory + @"common\" + Game.installationPath;
                                    Game.downloadPath = Library.Directory + @"downloading\" + Game.appID;

                                    if (Key["SizeOnDisk"].Value != "0")
                                    {
                                        if (Game.exactInstallPath != null)
                                        {
                                            if (Properties.Settings.Default.SLM_GameSizeCalcMethod != "ACF")
                                                Game.sizeOnDisk += Functions.FileSystem.GetDirectorySize(Game.exactInstallPath, true);
                                            else
                                                Game.sizeOnDisk += Convert.ToInt64(Key["SizeOnDisk"].Value);
                                        }

                                        if (Game.downloadPath != null)
                                        {
                                            if (Properties.Settings.Default.SLM_GameSizeCalcMethod != "ACF")
                                                Game.sizeOnDisk += Functions.FileSystem.GetDirectorySize(Game.downloadPath, true);
                                        }
                                    }
                                    else
                                        Game.sizeOnDisk = 0;

                                    Definitions.List.Game.Add(Game);

                                }
                            }
                            //foreach

                        }
                    }
                }

                // Update main form as visual
                Functions.Games.UpdateMainForm();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public static void UpdateMainForm()
        {
            try
            {
                if (Definitions.Accessors.Main.panel_GameList.Controls.Count != 0)
                    Definitions.Accessors.Main.panel_GameList.Controls.Clear();

                foreach (Definitions.List.GamesList Game in Definitions.List.Game)
                {

                    PictureBox gameDetailBox = new PictureBox();
                    gameDetailBox.SizeMode = PictureBoxSizeMode.StretchImage;
                    gameDetailBox.Size = new System.Drawing.Size(200, 93);
                    gameDetailBox.LoadAsync("https://steamcdn-a.akamaihd.net/steam/apps/" + Game.appID + "/header.jpg");
                    gameDetailBox.ErrorImage = global::Steam_Library_Manager.Properties.Resources.no_image_available;
                    gameDetailBox.Margin = new System.Windows.Forms.Padding(20);
                    gameDetailBox.Tag = Game;
                    gameDetailBox.MouseDown += gameDetailBox_MouseDown;

                    ContextMenu rightClickMenu = new ContextMenu();
                    rightClickMenu.Tag = Game;

                    EventHandler mouseClick = new EventHandler(gameDetailBox_ContextMenuAction);

                    rightClickMenu.MenuItems.Add(Game.appName + " (" + Game.appID.ToString() + ")").Enabled = false;
                    rightClickMenu.MenuItems.Add("Size on Disk: " + Functions.FileSystem.FormatBytes(Game.sizeOnDisk)).Enabled = false;
                    rightClickMenu.MenuItems.Add("-");
                    rightClickMenu.MenuItems.Add("Play", mouseClick).Name = "rungameid";
                    rightClickMenu.MenuItems.Add("-");
                    rightClickMenu.MenuItems.Add("Backup (SLM)", mouseClick).Name = "Backup-SLM";
                    rightClickMenu.MenuItems.Add("Backup (Steam)", mouseClick).Name = "backup";
                    rightClickMenu.MenuItems.Add("Defrag", mouseClick).Name = "defrag";
                    rightClickMenu.MenuItems.Add("Validate Files", mouseClick).Name = "validate";
                    rightClickMenu.MenuItems.Add("-");
                    rightClickMenu.MenuItems.Add("Check System Requirements", mouseClick).Name = "checksysreqs";
                    rightClickMenu.MenuItems.Add("-");
                    rightClickMenu.MenuItems.Add("View on Store", mouseClick).Name = "Store";
                    rightClickMenu.MenuItems.Add("View on Disk", mouseClick).Name = "Disk";
                    rightClickMenu.MenuItems.Add("Uninstall", mouseClick).Name = "uninstall";

                    gameDetailBox.ContextMenu = rightClickMenu;

                    Definitions.Accessors.Main.panel_GameList.Controls.Add(gameDetailBox);

                }
            }
            catch { }
        }

        static void gameDetailBox_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    PictureBox img = sender as PictureBox;
                    img.DoDragDrop(img, DragDropEffects.Move);
                }
            }
            catch { }
        }

        static void gameDetailBox_ContextMenuAction(object sender, EventArgs e)
        {
            try
            {
                Definitions.List.GamesList Game = (sender as MenuItem).Parent.Tag as Definitions.List.GamesList;

                switch ((sender as MenuItem).Name)
                {
                    default:
                        System.Diagnostics.Process.Start("steam://" + (sender as MenuItem).Name + "/" + Game.appID);
                        break;

                    case "Backup-SLM":
                        MessageBox.Show("Please create a backup library first then drag & drop game on to backup library.");
                        break;

                    case "Store":
                        System.Diagnostics.Process.Start("http://store.steampowered.com/app/" + Game.appID.ToString() + "/");
                        break;

                    case "Disk":
                        System.Diagnostics.Process.Start(Game.exactInstallPath);
                        break;
                }

            }
            catch { }
        }

    }
}
