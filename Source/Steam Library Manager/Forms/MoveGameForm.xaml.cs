using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Steam_Library_Manager.Forms
{
    /// <summary>
    /// Interaction logic for MoveGameForm.xaml
    /// </summary>
    /// 

    public partial class MoveGameForm : Window
    {
        // Define our game from LatestSelectedGame
        Definitions.List.Game Game;

        // Define our library from LatestDropLibrary
        Definitions.List.Library Library;

        // Define cancellation token
        System.Threading.CancellationTokenSource cancellationToken;

        // Define task
        System.Threading.Tasks.Task task;

        public MoveGameForm(Definitions.List.Game gameToMove, Definitions.List.Library libraryToMove)
        {
            InitializeComponent();

            Game = gameToMove;
            Library = libraryToMove;

            textBox.ItemsSource = formLogs;
        }

        private ObservableCollection<string> formlogs = new ObservableCollection<string>();

        public ObservableCollection<string> formLogs
        {
            get { return formlogs; }
            set
            {
                if (value != formlogs)
                {
                    formlogs = value;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            gameHeaderLabel.Text = Game.appName;

            gameHeaderImage.ImageUrl = Game.gameHeaderImage;

            gameLibrary.Content = Game.Library.fullPath;
            targetLibrary.Content = Library.fullPath;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            bool removeOldGame = removeOldFiles.IsChecked.Value;

            if (task == null || task.Status == System.Threading.Tasks.TaskStatus.Canceled)
            {
                formLogs.Clear();
                button.Content = "Cancel";

                cancellationToken = new System.Threading.CancellationTokenSource();

                Functions.fileSystem.Game Games = new Functions.fileSystem.Game();
                task = new System.Threading.Tasks.TaskFactory(cancellationToken.Token).StartNew(async () =>
                {
                    try
                    {
                    List<System.IO.FileSystemInfo> fileList = await Games.getFileList(Game);
                    Games.copyGameFiles(this, fileList, Game, Library, cancellationToken);

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        // If game is not exists in the target library
                        if (Library.Games.Count(x => x.acfName == Game.acfName) == 0)
                        {
                            // Add game to new library
                            Functions.Games.AddNewGame(Game.acfPath.Replace(Game.Library.steamAppsPath.FullName, Library.steamAppsPath.FullName), Game.appID, Game.appName, Game.installationPath, Library, Game.sizeOnDisk, false);

                            // Update library details
                            Functions.Library.updateLibraryVisual(Library);
                        }

                        if (removeOldGame)
                            await Games.deleteGameFiles(Game, fileList);
                    }

                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                });
            }
            else if (button.Content.ToString() != "Close")
            {
                button.Content = "Copy";
                cancellationToken.Cancel();
            }
            else
            {
                Close();
            }
        }
    }
}
