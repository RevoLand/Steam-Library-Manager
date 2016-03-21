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
    }
}
