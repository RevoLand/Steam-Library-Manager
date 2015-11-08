using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Steam_Library_Manager.Forms
{
    partial class moveLibrary : Form
    {
        Definitions.List.LibraryList libraryToMove;
        string newLibraryPath;
        bool backupLibrary;

        public moveLibrary(Definitions.List.LibraryList Library)
        {
            InitializeComponent();

            // Set our form icon
            Icon = Properties.Resources.steam_icon;

            libraryToMove = Library;

            Text = string.Format("{0} - SLM - Move Library", libraryToMove.fullPath);
        }

        private void moveLibrary_Load(object sender, EventArgs e)
        {
            try
            {
                Func<Definitions.List.GamesList, object> Sort;

                // Define our sorting method
                switch (Properties.Settings.Default.SortGamesBy)
                {
                    default:
                    case "appName":
                        Sort = x => x.appName;
                        break;
                    case "appID":
                        Sort = x => x.appID;
                        break;
                    case "sizeOnDisk":
                        Sort = x => x.sizeOnDisk;
                        break;
                }

                long neededSize = 0;

                foreach (Definitions.List.GamesList Game in Definitions.List.Game.Where(x => x.Library == libraryToMove).OrderBy(Sort))
                {
                    // Define a new pictureBox for game
                    Framework.PictureBoxWithCaching gameDetailBox = new Framework.PictureBoxWithCaching();

                    // Set picture mode of pictureBox
                    gameDetailBox.SizeMode = PictureBoxSizeMode.StretchImage;

                    // Set game image size
                    gameDetailBox.Size = new System.Drawing.Size(Properties.Settings.Default.GamePictureBoxSize.Width / 2, Properties.Settings.Default.GamePictureBoxSize.Height / 2);

                    // Load game header image asynchronously
                    gameDetailBox.LoadAsync(string.Format("https://steamcdn-a.akamaihd.net/steam/apps/{0}/header.jpg", Game.appID));

                    // Set error image in case of couldn't load game header image
                    gameDetailBox.ErrorImage = Properties.Resources.no_image_available;

                    // add picturebox to our panel
                    panel_gamesInLibrary.Controls.Add(gameDetailBox);

                    // Add game size to total size we need
                    neededSize += Game.sizeOnDisk;
                }

                combobox_libraryList.DataSource = Definitions.List.Library.FindAll(x => x != libraryToMove);
                combobox_libraryList.DisplayMember = "fullPath";

                groupBox_selectedLibrary.Text = string.Format("Details: {0}", libraryToMove.fullPath);
                label_gamesInLibrary.Text = libraryToMove.GameCount.ToString();
                label_neededSpace.Text = Functions.FileSystem.FormatBytes(neededSize);
            }
            catch { }
        }

        private void combobox_libraryList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (combobox_libraryList.SelectedItem != null)
                updateForm((combobox_libraryList.SelectedItem as Definitions.List.LibraryList));
        }

        void updateForm(Definitions.List.LibraryList Library)
        {
            try
            {
                if (Library != null)
                {
                    label_gamesInTargetLibrary.Text = Library.GameCount.ToString();
                    label_availableSpaceAtTargetLibrary.Text = Functions.FileSystem.FormatBytes(Functions.FileSystem.GetFreeSpace(Library.fullPath));

                    groupBox_targetLibrary.Text = string.Format("Target: {0}", Library.fullPath);
                }
                else
                {
                    label_gamesInTargetLibrary.Text = "0";
                    label_availableSpaceAtTargetLibrary.Text = Functions.FileSystem.FormatBytes(Functions.FileSystem.GetFreeSpace(newLibraryPath));

                    groupBox_targetLibrary.Text = string.Format("Target: {0}", newLibraryPath);
                }
            }
            catch { }
        }

        private void button_newLibraryButton_Click(object sender, EventArgs e)
        {
            DialogResult newLibrarySelection = folderBrowser_selectNewLibraryPath.ShowDialog();

            if (newLibrarySelection != DialogResult.OK) return;

            newLibraryPath = folderBrowser_selectNewLibraryPath.SelectedPath;

            if (!Functions.SteamLibrary.libraryExists(newLibraryPath))
            {
                if (Directory.GetDirectoryRoot(newLibraryPath) != newLibraryPath)
                {
                    DialogResult backupLibraryDialog = MessageBox.Show("Backup Library?", "Backup?", MessageBoxButtons.YesNo);
                    if (backupLibraryDialog == DialogResult.Yes)
                        backupLibrary = true;
                    else
                        backupLibrary = false;

                    updateForm(null);

                }
                else
                {
                    newLibraryPath = "";
                    MessageBox.Show("Steam libraries can not be created in root");
                }
            }
            else
            {
                newLibraryPath = "";
                MessageBox.Show("Library exists at selected path");
            }
        }

        private void button_moveLibrary_Click(object sender, EventArgs e)
        {
            button_moveLibrary.Enabled = false;

            if (libraryToMove.Main)
                checkbox_removeOldFiles.Checked = false;

            move((string.IsNullOrEmpty(newLibraryPath)) ? (combobox_libraryList.SelectedItem as Definitions.List.LibraryList) : null, newLibraryPath, checkbox_removeOldFiles.Checked, backupLibrary);
        }


        async void move(Definitions.List.LibraryList targetLibrary, string newLibraryPath, bool removeOldLibrary, bool backupLibrary)
        {
            int movedFiles = 0;
            int totalFilesToMove = Directory.GetFiles(libraryToMove.steamAppsPath, "*", SearchOption.AllDirectories).Length;

            string newFileName;
            try
            {
                progressBar_libraryMoveProgress.Maximum = totalFilesToMove;
                label_progressInformation.Text = string.Format("Files: {0} / {1}", movedFiles, totalFilesToMove);

                if (string.IsNullOrEmpty(newLibraryPath))
                {
                    newLibraryPath = targetLibrary.fullPath;
                }

                // For each file in common folder of game
                foreach (string currentFile in Directory.EnumerateFiles(libraryToMove.steamAppsPath, "*", SearchOption.AllDirectories))
                {
                    // Make a new file stream from the file we are reading so we can copy the file asynchronously
                    using (FileStream currentFileStream = File.OpenRead(currentFile))
                    {
                        // Set new file name including target game path
                        newFileName = Path.Combine(newLibraryPath, currentFile.Replace(libraryToMove.fullPath + @"\", ""));

                        // If directory not exists
                        if (!Directory.Exists(Path.GetDirectoryName(newFileName)))
                            // Create a directory at target library for new file, if we do not the process will fail
                            Directory.CreateDirectory(Path.GetDirectoryName(newFileName));

                        // Create a new file
                        using (FileStream newFileStream = File.Create(newFileName))
                        {
                            // Copy the file to target library asynchronously
                            await currentFileStream.CopyToAsync(newFileStream);

                            progressBar_libraryMoveProgress.PerformStep();
                            label_progressInformation.Text = string.Format("Files: {0} / {1}", movedFiles += 1, totalFilesToMove);
                        }
                    }
                }

                if (removeOldLibrary)
                {
                    if (!backupLibrary)
                        Functions.SteamLibrary.updateLibraryDetails(libraryToMove, newLibraryPath);

                    Functions.SteamLibrary.removeLibrary(libraryToMove, removeOldLibrary);
                }

                if (!Functions.SteamLibrary.libraryExists(newLibraryPath))
                    Functions.SteamLibrary.createNewLibrary(newLibraryPath, backupLibrary);

                // Update library list
                Functions.SteamLibrary.updateLibraryList();

                MessageBox.Show("Completed!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
