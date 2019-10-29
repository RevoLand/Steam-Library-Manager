using Alphaleonis.Win32.Filesystem;
using Steam_Library_Manager.Definitions.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Steam_Library_Manager.Definitions
{
    public abstract class Library : INotifyPropertyChanged
    {
        public readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public LibraryType Type { get; set; }
        public bool IsMain { get; set; }
        public bool IsUpdatingAppList { get; set; }
        public DirectoryInfo DirectoryInfo { get; set; }
        private string _fullPath;

        public string FullPath
        {
            get => _fullPath;
            set
            {
                _fullPath = value;
                Functions.FileSystem.GetDiskFreeSpaceEx(_fullPath, out var freeSpace, out var totalSpace, out var totalFreeSpace);

                FreeSpace = (long)freeSpace;
                TotalSize = (long)totalSpace;
            }
        }

        public System.Collections.ObjectModel.ObservableCollection<dynamic> Apps { get; set; } = new System.Collections.ObjectModel.ObservableCollection<dynamic>();
        public Dictionary<string, DirectoryInfo> DirectoryList { get; set; } = new Dictionary<string, DirectoryInfo>();
        public List<LibraryType> AllowedAppTypes = new List<LibraryType>();

        public long FreeSpace { get; set; }
        public long TotalSize { get; set; }
        public string PrettyFreeSpace => DirectoryInfo.Exists && !DirectoryInfo.FullName.StartsWith(Path.DirectorySeparatorChar.ToString()) ? $"{Functions.FileSystem.FormatBytes(FreeSpace)} / {Functions.FileSystem.FormatBytes(TotalSize)}" : "";
        public int FreeSpacePerc => DirectoryInfo.Exists && !DirectoryInfo.FullName.StartsWith(Path.DirectorySeparatorChar.ToString()) ? 100 - ((int)Math.Round((double)(100 * FreeSpace) / TotalSize)) : 0;

        public List<FrameworkElement> ContextMenu => _contextMenuElements ?? (_contextMenuElements = GenerateCMenuItems());
        private List<FrameworkElement> _contextMenuElements;

        private List<FrameworkElement> GenerateCMenuItems()
        {
            var cMenu = new List<FrameworkElement>();
            try
            {
                foreach (var cMenuItem in List.LibraryCMenuItems.Where(x => x.IsActive && x.AllowedLibraryTypes.Contains(Type)).ToList())
                {
                    if (!cMenuItem.ShowToNormal && IsMain)
                    {
                        continue;
                    }

                    if (!DirectoryInfo.Exists && !cMenuItem.ShowToOffline)
                    {
                        continue;
                    }

                    if (cMenuItem.IsSeparator)
                    {
                        cMenu.Add(new Separator());
                    }
                    else
                    {
                        var menuItem = new MenuItem()
                        {
                            Tag = cMenuItem.Action,
                            Header = Framework.StringFormat.Format(cMenuItem.Header, new { LibraryFullPath = DirectoryInfo.FullName, FreeDiskSpace = PrettyFreeSpace }),
                            Icon = cMenuItem.Icon,
                            HorizontalContentAlignment = HorizontalAlignment.Left,
                            VerticalContentAlignment = VerticalAlignment.Center
                        };

                        menuItem.Click += Main.FormAccessor.LibraryCMenuItem_Click;

                        cMenu.Add(menuItem);
                    }
                }

                return cMenu;
            }
            catch (FormatException ex)
            {
                MessageBox.Show(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.FormatException)), new { ExceptionMessage = ex.Message }));
                return cMenu;
            }
        }

        public abstract void UpdateAppList();

        public abstract void ParseMenuItemActionAsync(string action);

        public abstract void RemoveLibraryAsync(bool withFiles);

        public abstract void UpdateJunks();

        public abstract void UpdateDupes();

        public void UpdateDiskDetails()
        {
            Functions.FileSystem.GetDiskFreeSpaceEx(_fullPath, out var freeSpace, out var totalSpace, out var totalFreeSpace);

            FreeSpace = (long)freeSpace;
            TotalSize = (long)totalSpace;

            OnPropertyChanged("DirectoryInfo");
            OnPropertyChanged("FreeSpace");
            OnPropertyChanged("PrettyFreeSpace");
            OnPropertyChanged("FreeSpacePerc");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
    }
}