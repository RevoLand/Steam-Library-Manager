using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Steam_Library_Manager.Definitions
{
    public abstract class LibraryBase
    {
        public readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public Library Library { get; set; }
        public bool IsMain { get; set; }
        public string FullPath { get; set; }
        public System.Collections.ObjectModel.ObservableCollection<dynamic> Apps { get; set; } = new System.Collections.ObjectModel.ObservableCollection<dynamic>();

        public List<FrameworkElement> ContextMenu => _contextMenuElements ?? (_contextMenuElements = GenerateCMenuItems());
        private List<FrameworkElement> _contextMenuElements;

        public List<FrameworkElement> GenerateCMenuItems()
        {
            var cMenu = new List<FrameworkElement>();
            try
            {
                foreach (var cMenuItem in List.LibraryCMenuItems.ToList().Where(x => x.IsActive && x.LibraryType == Library.Type))
                {
                    if (!cMenuItem.ShowToNormal)
                    {
                        continue;
                    }

                    if (cMenuItem.IsSeparator)
                    {
                        cMenu.Add(new Separator());
                    }
                    else
                    {
                        MenuItem SLMItem = new MenuItem()
                        {
                            Tag = cMenuItem.Action,
                            Header = Framework.StringFormat.Format(cMenuItem.Header, new { LibraryFullPath = Library.DirectoryInfo.FullName, FreeDiskSpace = Library.PrettyFreeSpace }),
                            Icon = Functions.FAwesome.GetAwesomeIcon(cMenuItem.Icon, cMenuItem.IconColor),
                            HorizontalContentAlignment = HorizontalAlignment.Left,
                            VerticalContentAlignment = VerticalAlignment.Center
                        };

                        SLMItem.Click += Main.FormAccessor.LibraryCMenuItem_Click;

                        cMenu.Add(SLMItem);
                    }
                }

                return cMenu;
            }
            catch (FormatException ex)
            {
                MessageBox.Show(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.SteamAppInfo_FormatException)), new { ExceptionMessage = ex.Message }));
                return cMenu;
            }
        }

        public abstract void UpdateAppListAsync();

        public abstract void ParseMenuItemActionAsync(string action);

        public abstract void RemoveLibraryAsync(bool withFiles);
    }
}