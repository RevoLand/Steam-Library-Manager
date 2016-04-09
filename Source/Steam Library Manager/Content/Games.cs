using FontAwesome.WPF;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Steam_Library_Manager.Content
{
    class Games
    {
        public static ContextMenu generateRightClickMenu(Definitions.List.Game Game)
        {
            List<Definitions.List.rightClickMenuItem> rightClickMenu = generateRightClickMenuItems(Game);

            ContextMenu cMenu = new ContextMenu();
            cMenu.Tag = Game;

            foreach (Definitions.List.rightClickMenuItem Item in rightClickMenu.OrderBy(x => x.order))
            {
                if (Item.ShownToBackup && !Game.Library.Backup) continue;

                MenuItem newMenuItem = new MenuItem();

                if (Item.IsSeperator)
                    cMenu.Items.Add(new Separator());
                else
                {
                    newMenuItem.Click += NewMenuItem_Click;

                    newMenuItem.Header = Item.DisplayText;
                    newMenuItem.Name = Item.Action;
                    newMenuItem.IsEnabled = Item.IsEnabled;

                    if (Item.icon != FontAwesomeIcon.None)
                        newMenuItem.Icon = Functions.fAwesome.getAwesomeIcon(Item.icon, Item.iconColor);

                    cMenu.Items.Add(newMenuItem);
                }
            }
            return cMenu;
        }

        private static async void NewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Definitions.List.Game Game = ((((sender as MenuItem).Parent as ContextMenu).Parent as System.Windows.Controls.Primitives.Popup).PlacementTarget as Grid).Tag as Definitions.List.Game;

            switch ((sender as MenuItem).Name)
            {
                default:
                    System.Diagnostics.Process.Start(string.Format("steam://{0}/{1}", (sender as MenuItem).Name, Game.appID));
                    break;
                case "Disk":
                    System.Diagnostics.Process.Start(Game.commonPath);
                    break;
                case "acfFile":
                    System.Diagnostics.Process.Start(Game.acfPath);
                    break;
                case "deleteGameFilesSLM":

                    Functions.fileSystem.Game gameFunctions = new Functions.fileSystem.Game();
                    await gameFunctions.deleteGameFiles(Game);

                    break;
            }
        }

        public static List<Definitions.List.rightClickMenuItem> generateRightClickMenuItems(Definitions.List.Game Game)
        {
            List<Definitions.List.rightClickMenuItem> rightClickMenu = new List<Definitions.List.rightClickMenuItem>();

            rightClickMenu.Add(new Definitions.List.rightClickMenuItem
            {
                order = 1,
                DisplayText = "Play",
                Action = "run",
                icon = FontAwesomeIcon.Play
            });

            rightClickMenu.Add(new Definitions.List.rightClickMenuItem
            {
                order = 2,
                IsSeperator = true
            });

            rightClickMenu.Add(new Definitions.List.rightClickMenuItem
            {
                order = 3,
                DisplayText = $"{Game.appName} ({Game.appID})",
                Action = "Disk",
                icon = FontAwesomeIcon.FolderOpen
            });

            rightClickMenu.Add(new Definitions.List.rightClickMenuItem
            {
                order = 4,
                DisplayText = $"Size on disk: {Functions.fileSystem.FormatBytes(Game.sizeOnDisk)}",
                Action = "Disk",
                icon = FontAwesomeIcon.HddOutline
            });

            rightClickMenu.Add(new Definitions.List.rightClickMenuItem
            {
                order = 5,
                IsSeperator = true
            });

            rightClickMenu.Add(new Definitions.List.rightClickMenuItem
            {
                order = 6,
                DisplayText = "Delete Game files (SLM)",
                Action = "deleteGameFilesSLM",
                icon = FontAwesomeIcon.Trash
            });

            return rightClickMenu;
        }
    }
}
