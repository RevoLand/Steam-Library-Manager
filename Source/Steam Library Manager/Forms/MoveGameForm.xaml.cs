using System;
using System.Collections.Generic;
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

        public Framework.AsyncObservableCollection<string> formLogs = new Framework.AsyncObservableCollection<string>();

        public MoveGameForm(Definitions.List.Game gameToMove, Definitions.List.Library libraryToMove)
        {
            InitializeComponent();

            Game = gameToMove;
            Library = libraryToMove;

            textBox.ItemsSource = formLogs;
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
            bool compressGame = compress.IsChecked.Value;

            if (task == null || task.Status == System.Threading.Tasks.TaskStatus.Canceled)
            {
                formLogs.Clear();
                button.Content = "Cancel";

                cancellationToken = new System.Threading.CancellationTokenSource();

                Functions.fileSystem.Game Games = new Functions.fileSystem.Game();
                task = new System.Threading.Tasks.TaskFactory(cancellationToken.Token).StartNew(() =>
                {
                    try
                    {
                        List<System.IO.FileSystemInfo> fileList = Games.getFileList(Game);
                        Games.copyGameFiles(this, fileList, Game, Library, cancellationToken, compressGame);

                        if (!cancellationToken.IsCancellationRequested)
                        {
                            // If game is not exists in the target library
                            if (Library.Games.Count(x => x.acfName == Game.acfName) == 0)
                            {
                                // Add game to new library
                                Functions.Games.AddNewGame(Game.acfPath.FullName.Replace(Game.Library.steamAppsPath.FullName, Library.steamAppsPath.FullName), Game.appID, Game.appName, Game.installationPath.Name, Library, Game.sizeOnDisk, compressGame);

                                // Update library details
                                Functions.Library.updateLibraryVisual(Library);
                            }

                            if (removeOldGame)
                            {
                                Games.deleteGameFiles(Game, fileList);
                                Functions.Games.UpdateMainForm(Game.Library);
                            }
                        }

                    }
                    catch (Exception ex)
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
