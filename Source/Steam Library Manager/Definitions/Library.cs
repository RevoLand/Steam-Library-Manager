using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Steam_Library_Manager.Definitions
{
    public class Library
    {
        public Enums.LibraryType Type { get; set; }
        public System.IO.DirectoryInfo DirectoryInfo { get; set; }

        public SteamLibrary Steam { get; set; }

        public Framework.AsyncObservableCollection<FrameworkElement> ContextMenu => GenerateCMenuItems();

        public Framework.AsyncObservableCollection<FrameworkElement> GenerateCMenuItems()
        {
            Framework.AsyncObservableCollection<FrameworkElement> CMenu = new Framework.AsyncObservableCollection<FrameworkElement>();
            try
            {
                foreach (ContextMenuItem CMenuItem in List.LibraryCMenuItems.Where(x => x.IsActive && x.ShowToSLMBackup))
                {
                    if (!CMenuItem.ShowToNormal)
                    {
                        continue;
                    }

                    if (CMenuItem.IsSeparator)
                    {
                        CMenu.Add(new Separator());
                    }
                    else
                    {
                        MenuItem SLMItem = new MenuItem()
                        {
                            Tag = this,
                            Header = string.Format(CMenuItem.Header, DirectoryInfo.FullName, PrettyFreeSpace)
                        };

                        SLMItem.Tag = CMenuItem.Action;
                        SLMItem.Icon = Functions.FAwesome.GetAwesomeIcon(CMenuItem.Icon, CMenuItem.IconColor);
                        SLMItem.HorizontalContentAlignment = HorizontalAlignment.Left;
                        SLMItem.VerticalContentAlignment = VerticalAlignment.Center;

                        CMenu.Add(SLMItem);
                    }
                }

                return CMenu;
            }
            catch (FormatException ex)
            {
                MessageBox.Show($"An error happened while parsing context menu, most likely happened duo typo on color name.\n\n{ex}");

                Functions.Logger.LogToFile(Functions.Logger.LogType.Library, ex.ToString());
                return CMenu;
            }
        }

        public void ParseMenuItemAction(string Action)
        {
            if (Type == Enums.LibraryType.SLM)
            {
                switch (Action.ToLowerInvariant())
                {
                    // Opens game installation path in explorer
                    case "disk":
                        if (DirectoryInfo.Exists)
                        {
                            Process.Start(DirectoryInfo.FullName);
                        }

                        break;
                    // Removes a backup library from list
                    case "removefromlist":
                        try
                        {
                            if (Type == Enums.LibraryType.SLM)
                            {
                                List.Libraries.Remove(this);

                                if (SLM.CurrentSelectedLibrary == this)
                                    Main.FormAccessor.AppPanel.ItemsSource = null;
                            }
                        }
                        catch (Exception ex)
                        {
                            Functions.Logger.LogToFile(Functions.Logger.LogType.Library, ex.ToString());
                        }
                        break;
                }
            }
            else if (Type == Enums.LibraryType.Steam)
            {
                Steam.ParseMenuItemActionAsync(Action);
            }
        }
        

        public long FreeSpace
        {
            get => Functions.FileSystem.GetAvailableFreeSpace(DirectoryInfo.FullName);
        }

        public string PrettyFreeSpace => Functions.FileSystem.FormatBytes(FreeSpace);

        public int FreeSpacePerc => 100 - ((int)Math.Round((double)(100 * FreeSpace) / Functions.FileSystem.GetTotalSize(DirectoryInfo.FullName)));
    }
}
