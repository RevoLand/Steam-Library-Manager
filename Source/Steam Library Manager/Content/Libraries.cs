using FontAwesome.WPF;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Steam_Library_Manager.Content
{
    class Libraries
    {

        public static ObservableCollection<FrameworkElement> generateRightClickMenuItems(Definitions.List.Library Library)
        {
            ObservableCollection<FrameworkElement> rightClickMenu = new ObservableCollection<FrameworkElement>();

            rightClickMenu.Add(new MenuItem
            {
                Header = $"Open library in explorer ({Library.fullPath})",
                Name = "Disk",
                Tag = Library,
                Icon = Functions.fAwesome.getAwesomeIcon(FontAwesomeIcon.FolderOpen, System.Windows.Media.Brushes.Black),
            });

            // Separator
            rightClickMenu.Add(new Separator());

            rightClickMenu.Add(new MenuItem
            {
                Header = "Move library",
                Name = "moveLibrary",
                Tag = Library,
                Icon = Functions.fAwesome.getAwesomeIcon(FontAwesomeIcon.Paste, System.Windows.Media.Brushes.Black),
            });

            rightClickMenu.Add(new MenuItem
            {
                Header = "Refresh game list in library",
                Name = "RefreshGameList",
                Tag = Library,
                Icon = Functions.fAwesome.getAwesomeIcon(FontAwesomeIcon.Refresh, System.Windows.Media.Brushes.Black),
            });

            // Separator
            rightClickMenu.Add(new Separator());

            rightClickMenu.Add(new MenuItem
            {
                Header = "Delete library",
                Name = "deleteLibrary",
                Tag = Library,
                Icon = Functions.fAwesome.getAwesomeIcon(FontAwesomeIcon.Trash, System.Windows.Media.Brushes.Black),
            });

            rightClickMenu.Add(new MenuItem
            {
                Header = "Delete games in library",
                Name = "deleteLibrarySLM",
                Tag = Library,
                Icon = Functions.fAwesome.getAwesomeIcon(FontAwesomeIcon.TrashOutline, System.Windows.Media.Brushes.Black),
            });

            rightClickMenu.Add(new MenuItem
            {
                Header = "Remove from list",
                Name = "RemoveFromList",
                Tag = Library,
                Icon = Functions.fAwesome.getAwesomeIcon(FontAwesomeIcon.Minus, System.Windows.Media.Brushes.Black),
            });

            return rightClickMenu;
        }
    }
}
