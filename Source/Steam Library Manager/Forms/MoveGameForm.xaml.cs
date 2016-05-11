using System;
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
        Definitions.Game Game;

        // Define our library from LatestDropLibrary
        Definitions.Library Library;

        // Define cancellation token
        System.Threading.CancellationTokenSource cancellationToken;

        // Define task
        System.Threading.Tasks.Task task;

        public Framework.AsyncObservableCollection<string> formLogs = new Framework.AsyncObservableCollection<string>();

        public MoveGameForm(Definitions.Game gameToMove, Definitions.Library libraryToMove)
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

            gameLibrary.Content = Game.installedLibrary.fullPath;
            targetLibrary.Content = Library.fullPath;
        }

        public void reportFileMovement(string movenFileName, int movenFileCount, int totalFileCount, long movenFileSize, long totalFileSize)
        {
            formLogs.Add(string.Format("[{0}/{1}] {2}\n", movenFileCount, totalFileCount, movenFileName));

            if (progressReportLabel.Dispatcher.CheckAccess())
            {
                progressReportLabel.Content = $"{Functions.fileSystem.FormatBytes(totalFileSize - movenFileSize)} left - {Functions.fileSystem.FormatBytes(movenFileSize)} / {Functions.fileSystem.FormatBytes(totalFileSize)}";
            }
            else
            {
                progressReportLabel.Dispatcher.Invoke(delegate
                {
                    progressReportLabel.Content = $"{Functions.fileSystem.FormatBytes(totalFileSize - movenFileSize)} left - {Functions.fileSystem.FormatBytes(movenFileSize)} / {Functions.fileSystem.FormatBytes(totalFileSize)}";
                }, System.Windows.Threading.DispatcherPriority.Normal);
            }

            if (progressReport.Dispatcher.CheckAccess())
            {
                progressReport.Value = ((int)Math.Round((double)(100 * movenFileSize) / totalFileSize));
            }
            else
            {
                progressReport.Dispatcher.Invoke(delegate
                {
                    progressReport.Value = ((int)Math.Round((double)(100 * movenFileSize) / totalFileSize));
                }, System.Windows.Threading.DispatcherPriority.Normal);
            }
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
                        Game.copyGameFiles(this, Library, cancellationToken, compressGame);
                        //Games.copyGameFiles(this, fileList, Game, Library, cancellationToken, compressGame);

                        if (!cancellationToken.IsCancellationRequested)
                        {
                            // If game is not exists in the target library
                            if (Library.Games.Count(x => x.acfName == Game.acfName) == 0)
                            {
                                // Add game to new library
                                Functions.Games.AddNewGame(Game.fullAcfPath.FullName.Replace(Game.installedLibrary.steamAppsPath.FullName, Library.steamAppsPath.FullName), Game.appID, Game.appName, Game.installationPath.Name, Library, Game.sizeOnDisk, compressGame);

                                // Update library details
                                Library.updateLibraryVisual();
                            }

                            if (removeOldGame)
                            {
                                if (Game.deleteFiles())
                                {
                                    if (Definitions.SLM.selectedLibrary == Game.installedLibrary)
                                        Functions.Games.UpdateMainForm(Game.installedLibrary);

                                    Game.RemoveFromLibrary();
                                }
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
