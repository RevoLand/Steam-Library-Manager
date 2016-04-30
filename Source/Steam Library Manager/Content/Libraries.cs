using FontAwesome.WPF;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Steam_Library_Manager.Content
{
    class Libraries
    {

        public static ObservableCollection<FrameworkElement> generateRightClickMenuItems(Definitions.List.Library Library)
        {
            ObservableCollection<FrameworkElement> rightClickMenu = new ObservableCollection<FrameworkElement>();
            try
            {
                string[] menuItems = Properties.Settings.Default.libraryContextMenu.Split('|');

                foreach (string menuItem in menuItems)
                {
                    if (menuItem.Equals("separator", System.StringComparison.InvariantCultureIgnoreCase))
                        rightClickMenu.Add(new Separator());
                    else
                    {
                        string[] Item = menuItem.Split(';');

                        if (Item.Length <= 2) continue;
                        else
                        {
                            string header = Item[0];
                            string action = Item[1];
                            FontAwesomeIcon icon = FontAwesomeIcon.None;
                            BrushConverter bc = new BrushConverter();
                            Brush iconColor = (Brush)bc.ConvertFromInvariantString("black");

                            if (Item.Length > 2)
                                Enum.TryParse(Item[2], out icon);

                            if (Item.Length > 3)
                                iconColor = (Brush)bc.ConvertFromInvariantString(Item[3]);

                            rightClickMenu.Add(new MenuItem
                            {
                                Header = string.Format(header, Library.fullPath),
                                Name = action,
                                Icon = Functions.fAwesome.getAwesomeIcon(icon, iconColor),
                                Tag = Library
                            });
                        }
                    }
                }

                return rightClickMenu;
            }
            catch (FormatException)
            {
                MessageBox.Show("An error happened while parsing context menu, most likely happened duo typo on color name.");

                return rightClickMenu;
            }
        }
    }
}
