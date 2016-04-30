using FontAwesome.WPF;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Steam_Library_Manager.Content
{
    class Games
    {
        public static ObservableCollection<FrameworkElement> generateRightClickMenuItems(Definitions.List.Game Game)
        {
            ObservableCollection<FrameworkElement> rightClickMenu = new ObservableCollection<FrameworkElement>();

            rightClickMenu.Add(new MenuItem
            {
                Header = "Play",
                Name = "run",
                Tag = Game,
                Icon = Functions.fAwesome.getAwesomeIcon(FontAwesomeIcon.Play, System.Windows.Media.Brushes.Black)
            });

            rightClickMenu.Add(new Separator());

            rightClickMenu.Add(new MenuItem
            {
                Header = $"{Game.appName} ({Game.appID})",
                Name = "Disk",
                Tag = Game,
                Icon = Functions.fAwesome.getAwesomeIcon(FontAwesomeIcon.FolderOpen, System.Windows.Media.Brushes.Black)
            });

            rightClickMenu.Add(new MenuItem
            {
                Header = $"Size on disk: {Functions.fileSystem.FormatBytes(Game.sizeOnDisk)}",
                Name = "Disk",
                Tag = Game,
                Icon = Functions.fAwesome.getAwesomeIcon(FontAwesomeIcon.HddOutline, System.Windows.Media.Brushes.Black)
            });

            rightClickMenu.Add(new Separator());

            rightClickMenu.Add(new MenuItem
            {
                Header = "Delete Game files (SLM)",
                Name = "deleteGameFilesSLM",
                Tag = Game,
                Icon = Functions.fAwesome.getAwesomeIcon(FontAwesomeIcon.Trash, System.Windows.Media.Brushes.Black)
            });

            return rightClickMenu;
        }
    }
}
