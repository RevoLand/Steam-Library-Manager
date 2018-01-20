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
        public OriginLibrary Origin { get; set; }
        public Framework.AsyncObservableCollection<FrameworkElement> ContextMenu
        {
            get
            {
                Framework.AsyncObservableCollection<FrameworkElement> CMenu = new Framework.AsyncObservableCollection<FrameworkElement>();
                try
                {
                    foreach (ContextMenuItem CMenuItem in List.LibraryCMenuItems.Where(x => x.IsActive && x.ShowToSLMBackup))
                    {
                        if (!CMenuItem.ShowToNormal && !CMenuItem.ShowToSLMBackup)
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
                                Tag = CMenuItem.Action,
                                Header = string.Format(CMenuItem.Header, DirectoryInfo.FullName, PrettyFreeSpace),
                                Icon = Functions.FAwesome.GetAwesomeIcon(CMenuItem.Icon, CMenuItem.IconColor),
                                HorizontalContentAlignment = HorizontalAlignment.Left,
                                VerticalContentAlignment = VerticalAlignment.Center
                            };

                            SLMItem.Click += Main.FormAccessor.LibraryCMenuItem_Click;

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
            else if (Type == Enums.LibraryType.Origin)
            {
                Origin.ParseMenuItemAction(Action);
            }
        }

        public long FreeSpace => DirectoryInfo.Exists && !DirectoryInfo.FullName.StartsWith(System.IO.Path.DirectorySeparatorChar.ToString()) ? Functions.FileSystem.GetAvailableFreeSpace(DirectoryInfo.FullName) : 0;

        public long TotalSize => DirectoryInfo.Exists && !DirectoryInfo.FullName.StartsWith(System.IO.Path.DirectorySeparatorChar.ToString()) ? Functions.FileSystem.GetAvailableTotalSpace(DirectoryInfo.FullName) : 0;

        public string PrettyFreeSpace => DirectoryInfo.Exists && !DirectoryInfo.FullName.StartsWith(System.IO.Path.DirectorySeparatorChar.ToString()) ? $"{Functions.FileSystem.FormatBytes(FreeSpace)} / {Functions.FileSystem.FormatBytes(TotalSize)}" : "";

        public int FreeSpacePerc => DirectoryInfo.Exists && !DirectoryInfo.FullName.StartsWith(System.IO.Path.DirectorySeparatorChar.ToString()) ? 100 - ((int)Math.Round((double)(100 * FreeSpace) / Functions.FileSystem.GetAvailableTotalSpace(DirectoryInfo.FullName))) : 0;
    }
}
