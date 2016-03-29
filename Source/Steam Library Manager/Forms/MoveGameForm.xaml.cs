using System.Collections.Generic;
using System.Windows;

namespace Steam_Library_Manager.Forms
{
    /// <summary>
    /// Interaction logic for MoveGameForm.xaml
    /// </summary>
    public partial class MoveGameForm : Window
    {
        // Define our game from LatestSelectedGame
        Definitions.List.Game Game;

        // Define our library from LatestDropLibrary
        Definitions.List.Library Library;

        // Define cancellation token
        System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();

        public MoveGameForm(Definitions.List.Game gameToMove, Definitions.List.Library libraryToMove)
        {
            InitializeComponent();

            Game = gameToMove;
            Library = libraryToMove;
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
            button.IsEnabled = false;
            Functions.fileSystem.Game Games = new Functions.fileSystem.Game();
            new System.Threading.Tasks.TaskFactory(cancellationToken).StartNew(async () =>
            {
                List<System.IO.FileSystemInfo> fileList = await Games.getFileList(Game);
                Games.copyGameFiles(this, fileList, Game, Library);

                Functions.Games.AddNewGame(Game.acfPath.Replace(Game.Library.steamAppsPath, Library.steamAppsPath), Game.appID, Game.appName, Game.installationPath, Library, Game.sizeOnDisk, false);
                Functions.Library.updateLibraryVisual(Library);
            });
        }

    }
}
