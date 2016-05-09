using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Steam_Library_Manager.Content
{
    public class Games
    {
        public static Framework.AsyncObservableCollection<FrameworkElement> generateRightClickMenuItems(Definitions.Game Game)
        {
            Framework.AsyncObservableCollection<FrameworkElement> rightClickMenu = new Framework.AsyncObservableCollection<FrameworkElement>();
            try
            {
                foreach (Definitions.List.contextMenu cItem in Definitions.List.contextMenuItems.Where(x => x.IsVisible))
                {
                    if ((cItem.shownToBackup && !Game.installedLibrary.Backup) || (cItem.shownToCompressed && !Game.IsCompressed))
                        continue;

                    if (cItem.IsSeparator)
                        rightClickMenu.Add(new Separator());
                    else
                    {
                        MenuItem slmItem = new MenuItem();

                        slmItem.Tag = Game;
                        slmItem.Header = string.Format(cItem.Header, Game.appName, Game.appID, Functions.fileSystem.FormatBytes(Game.sizeOnDisk));
                        slmItem.Tag = cItem.Action;
                        slmItem.Icon = Functions.fAwesome.getAwesomeIcon(cItem.Icon, cItem.IconColor);

                        rightClickMenu.Add(slmItem);
                    }
                }

                return rightClickMenu;
            }
            catch (FormatException ex)
            {
                MessageBox.Show($"An error happened while parsing context menu, most likely happened duo typo on color name.\n\n{ex}");

                return rightClickMenu;
            }
        }

        public static void parseAction(Definitions.Game Game, string Action)
        {
            switch (Action.ToLowerInvariant())
            {
                default:
                    System.Diagnostics.Process.Start(string.Format(Action, Game.appID, Definitions.SLM.userSteamID64));
                    break;
                case "disk":
                    if (Game.commonPath.Exists)
                        System.Diagnostics.Process.Start(Game.commonPath.FullName);
                    break;
                case "acffile":
                    System.Diagnostics.Process.Start(Game.fullAcfPath.FullName);
                    break;
                case "deletegamefilesslm":

                    Game.deleteFiles();
                    Game.RemoveFromLibrary();

                    break;
            }
        }
    }
}
