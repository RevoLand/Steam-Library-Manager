using System;
using System.IO;
using System.Windows.Forms;

namespace Steam_Library_Manager.Functions
{
    class SteamLibrary
    {
        public static void UpdateGameLibraries()
        {
            try
            {
                // If we already have definitions in our list
                if (Definitions.List.Library.Count != 0)
                    // Clear them so they don't conflict
                    Definitions.List.Library.Clear();

                if (!File.Exists(Properties.Settings.Default.Steam_InstallationPath + "Steam.exe"))
                    return;

                // Our main library doesn't included in LibraryFolders.vdf so we have to include it manually
                Definitions.List.LibraryList Library = new Definitions.List.LibraryList();

                // Tell it is our main game library which can be handy in future
                Library.Main = true;

                // It's not a backup directory and should be treaten as library dir
                Library.Backup = false;

                // Define our library path to SteamApps
                Library.Directory = Properties.Settings.Default.Steam_InstallationPath + @"SteamApps\";

                // Count how many games we have installed in our library
                Library.GameCount = Functions.Games.GetGamesCountFromDirectory(Library);

                // And add collected informations to our global list
                Definitions.List.Library.Add(Library);

                Framework.KeyValue Key = new Framework.KeyValue();

                string filePath = Properties.Settings.Default.Steam_InstallationPath + @"SteamApps\libraryfolders.vdf";
                if (System.IO.File.Exists(filePath))
                {
                    Key.ReadFileAsText(filePath);

                    // Until someone gives a better idea, try to look for 255 Keys but break at first null key
                    for (int i = 1; i < Definitions.Steam.maxLibraryCount; i++)
                    {
                        if (Key[i.ToString()].Value == null)
                            break;

                        Library = new Definitions.List.LibraryList();
                        Library.Main = false;
                        Library.Backup = false;
                        Library.Directory = Key[i.ToString()].Value + @"\SteamApps\";
                        Library.GameCount = Functions.Games.GetGamesCountFromDirectory(Library);
                        Definitions.List.Library.Add(Library);
                    }
                }
                else
                {
                    // Could not locate LibraryFolders.vdf
                }

                if (Properties.Settings.Default.SLM_BackupDirectories != null)
                {
                    foreach (Object obj in Properties.Settings.Default.SLM_BackupDirectories)
                    {
                        Library = new Definitions.List.LibraryList();
                        Library.Main = false;
                        Library.Backup = true;
                        Library.Directory = obj.ToString();
                        Library.GameCount = Functions.Games.GetGamesCountFromDirectory(Library);
                        Definitions.List.Library.Add(Library);
                    }
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
                if (Definitions.Accessors.Main.panel_LibraryList.Controls.Count != 0)
                    Definitions.Accessors.Main.panel_LibraryList.Controls.Clear();

                int height = 128, width = 128;
                foreach (Definitions.List.LibraryList Library in Definitions.List.Library)
                {
                    // Folder image
                    PictureBox libraryDetailBox = new PictureBox();
                    libraryDetailBox.Image = global::Steam_Library_Manager.Properties.Resources.Folder;
                    libraryDetailBox.Size = new System.Drawing.Size(width, height);
                    libraryDetailBox.SizeMode = PictureBoxSizeMode.StretchImage;
                    libraryDetailBox.Margin = new System.Windows.Forms.Padding(10);
                    libraryDetailBox.Tag = Library;

                    // Drag & Drop events
                    ((Control)libraryDetailBox).AllowDrop = true;
                    libraryDetailBox.DragEnter += libraryDetailBox_DragEnter;
                    libraryDetailBox.DragDrop += libraryDetailBox_DragDrop;

                    // Label
                    Label libraryName = new Label();
                    libraryName.Size = new System.Drawing.Size(width, height);
                    libraryName.Text = Library.Directory + " (" + Library.GameCount.ToString()  + ")";
                    libraryName.TextAlign = System.Drawing.ContentAlignment.BottomRight;
                    libraryName.BackColor = System.Drawing.Color.Transparent;
                    libraryName.Tag = Library;
                    
                    libraryName.MouseClick += libraryDetailBox_OnSelect;

                    // Right click menu
                    ContextMenu rightClickMenu = new ContextMenu();
                    rightClickMenu.Tag = Library;
                    rightClickMenu.MenuItems.Add("Game Count: " + Library.GameCount.ToString()).Enabled = false;

                    // Add to controls
                    libraryDetailBox.Controls.Add(libraryName);
                    libraryDetailBox.ContextMenu = rightClickMenu;
                    Definitions.Accessors.Main.panel_LibraryList.Controls.Add(libraryDetailBox);
                }

                // New Library & Backup dir
                PictureBox newLibraryBox = new PictureBox();
                newLibraryBox.Image = global::Steam_Library_Manager.Properties.Resources.Folder;
                newLibraryBox.Size = new System.Drawing.Size(width, height);
                newLibraryBox.SizeMode = PictureBoxSizeMode.StretchImage;
                newLibraryBox.Margin = new System.Windows.Forms.Padding(10);

                // Label
                Label newLibraryBoxName = new Label();
                newLibraryBoxName.Size = new System.Drawing.Size(width, height);
                newLibraryBoxName.Text = "ADD NEW";
                newLibraryBoxName.TextAlign = System.Drawing.ContentAlignment.BottomRight;
                newLibraryBoxName.BackColor = System.Drawing.Color.Transparent;
                newLibraryBoxName.MouseClick += newLibraryBox_MouseClick;

                newLibraryBox.Controls.Add(newLibraryBoxName);
                Definitions.Accessors.Main.panel_LibraryList.Controls.Add(newLibraryBox);
            }
            catch { }
        }

        static void newLibraryBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left) return;

            DialogResult newLibrary = MessageBox.Show("Would you like to create a new Library?", "New Library?", MessageBoxButtons.YesNoCancel);
            if (newLibrary == DialogResult.Yes)
            {
                DialogResult newLibrarySelection = Definitions.Accessors.Main.folderBrowser_SelectNewLibraryPath.ShowDialog();
                if (newLibrarySelection == DialogResult.OK)
                {
                    string selectedPath = Definitions.Accessors.Main.folderBrowser_SelectNewLibraryPath.SelectedPath;

                    if (!LibraryExists(selectedPath))
                    {
                        if (Directory.GetDirectoryRoot(selectedPath) != selectedPath)
                            CreateNewLibrary(selectedPath, false);
                        else
                            MessageBox.Show("Steam Libraries can not be created in root");
                    }
                    else
                        MessageBox.Show("Library exists in the selected path! Are you trying to bug yourself?!");
                }
            }
            else if (newLibrary == DialogResult.No)
            {
                DialogResult newBackup = MessageBox.Show("Would you like to create a new Backup directory?", "New Backup dir?", MessageBoxButtons.YesNo);
                if (newBackup == DialogResult.Yes)
                {
                    DialogResult newLibrarySelection = Definitions.Accessors.Main.folderBrowser_SelectNewLibraryPath.ShowDialog();
                    if (newLibrarySelection == DialogResult.OK)
                    {
                        string selectedPath = Definitions.Accessors.Main.folderBrowser_SelectNewLibraryPath.SelectedPath;

                        if (!LibraryExists(selectedPath))
                        {
                            if (Directory.GetDirectoryRoot(selectedPath) != selectedPath)
                                CreateNewLibrary(selectedPath, true);
                            else
                                MessageBox.Show("Steam Backup Libraries can not be created in root but might be implemented in future");
                        }
                        else
                            MessageBox.Show("Library exists in the selected path! Are you trying to bug yourself?!");
                    }
                }
                else
                    return;
            }
        }

        public static bool LibraryExists(string Path)
        {
            try
            {
                foreach (Definitions.List.LibraryList Library in Definitions.List.Library)
                {
                    if (Library.Directory.ToLowerInvariant().Contains(Path.ToLowerInvariant() + "\\steamapps\\"))
                        return true;
                }
                return false;
            }
            catch { return true; }
        }

        public static void CreateNewLibrary(string newLibraryPath, bool BackupDir)
        {
            try
            {
                if (!BackupDir)
                {
                    File.Copy(Properties.Settings.Default.Steam_InstallationPath + "Steam.dll", newLibraryPath + @"\Steam.dll", true);
                    Directory.CreateDirectory(newLibraryPath + @"\SteamApps");
                    Directory.CreateDirectory(newLibraryPath + @"\SteamApps\common");

                    if (File.Exists(newLibraryPath + @"\Steam.dll")) // in case of permissions denied
                    {
                        string libraryFolders = Properties.Settings.Default.Steam_InstallationPath + @"SteamApps\libraryfolders.vdf";
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
                else
                {
                    if (Properties.Settings.Default.SLM_BackupDirectories == null)
                        Properties.Settings.Default.SLM_BackupDirectories = new System.Collections.Specialized.StringCollection();

                    Properties.Settings.Default.SLM_BackupDirectories.Add(newLibraryPath + @"\");

                    UpdateGameLibraries();
                }
            }
            catch { }
        }

        static void libraryDetailBox_OnSelect(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button != System.Windows.Forms.MouseButtons.Left) return;

                Definitions.List.LibraryList Library = (sender as Label).Tag as Definitions.List.LibraryList;

                if (Definitions.SLM.LatestSelectedLibrary == Library) return;

                // So mousewheel will work to scroll
                Definitions.Accessors.Main.panel_GameList.Focus();

                Definitions.SLM.LatestSelectedLibrary = Library;
                Functions.Games.UpdateGamesList(Library);
            }
            catch { }
        }

        static void libraryDetailBox_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                e.Effect = DragDropEffects.Move;
            }
            catch { }
        }

        static void libraryDetailBox_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                Definitions.List.LibraryList Library = (sender as PictureBox).Tag as Definitions.List.LibraryList;
                Definitions.List.GamesList Game = (e.Data.GetData("System.Windows.Forms.PictureBox") as PictureBox).Tag as Definitions.List.GamesList;

                if (Game.Library == Library)
                    return;

                Definitions.SLM.LatestDropLibrary = Library;
                Definitions.SLM.LatestSelectedGame = Game;

                new Forms.MoveGame().Show();
            }
            catch { }
        }

    }
}
