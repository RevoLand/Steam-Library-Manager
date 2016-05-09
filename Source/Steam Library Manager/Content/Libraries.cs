using FontAwesome.WPF;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Steam_Library_Manager.Content
{
    class Libraries
    {

        public static Framework.AsyncObservableCollection<FrameworkElement> generateRightClickMenuItems(Definitions.Library Library)
        {
            Framework.AsyncObservableCollection<FrameworkElement> rightClickMenu = new Framework.AsyncObservableCollection<FrameworkElement>();
            try
            {
                string[] menuItems = Properties.Settings.Default.libraryContextMenu.Split('|');

                foreach (string menuItem in menuItems)
                {
                    if (menuItem.Equals("separator", StringComparison.InvariantCultureIgnoreCase))
                        rightClickMenu.Add(new Separator());
                    else
                    {
                        MenuItem slmItem = new MenuItem();

                        slmItem.Tag = Library;

                        string[] Item = menuItem.Split(';');

                        foreach (string hardtonamethings in Item)
                        {
                            string[] itemDetails = hardtonamethings.Split(new char[] { '=' }, 2);
                            FontAwesomeIcon icon = FontAwesomeIcon.None;
                            Brush iconColor = (Brush)new BrushConverter().ConvertFromInvariantString("black");

                            switch (itemDetails[0].ToLowerInvariant())
                            {
                                case "text":
                                    slmItem.Header = string.Format(itemDetails[1], Library.fullPath);
                                    break;
                                case "action":
                                    slmItem.Name = itemDetails[1];
                                    break;
                                case "iconcolor":
                                    iconColor = (Brush)new BrushConverter().ConvertFromInvariantString(itemDetails[1]);
                                    break;
                                case "icon":
                                    Enum.TryParse(itemDetails[1], true, out icon);
                                    slmItem.Icon = Functions.fAwesome.getAwesomeIcon(icon, iconColor);
                                    break;
                                case "backup":
                                    if (bool.Parse(itemDetails[1]) != Library.Backup)
                                        slmItem.IsEnabled = false;
                                    break;
                            }
                        }

                        if (slmItem.IsEnabled)
                            rightClickMenu.Add(slmItem);
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
