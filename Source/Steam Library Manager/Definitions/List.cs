using System;
using System.ComponentModel;

namespace Steam_Library_Manager.Definitions
{
    // Our Library and Game definitions exists there
    public class List
    {
        // Make a new list for Library details
        public static Framework.AsyncObservableCollection<Library> Libraries = new Framework.AsyncObservableCollection<Library>();
        public static Framework.AsyncObservableCollection<JunkInfo> Junks { get; set; } = new Framework.AsyncObservableCollection<JunkInfo>();

        public static Framework.AsyncObservableCollection<ContextMenuItem> LibraryCMenuItems = new Framework.AsyncObservableCollection<ContextMenuItem>();
        public static Framework.AsyncObservableCollection<ContextMenuItem> AppCMenuItems = new Framework.AsyncObservableCollection<ContextMenuItem>();

        public class TaskList : INotifyPropertyChanged
        {
            public AppInfo TargetApp { get; set; }
            public Library Library { get; set; }

            public bool ErrorHappened = false;
            public bool Moving = false;
            public bool Compress { get; set; } = Properties.Settings.Default.Global_Compress;
            public bool RemoveOldFiles { get; set; } = Properties.Settings.Default.Global_RemoveOldFiles;
            public bool ReportFileMovement { get; set; } = Properties.Settings.Default.Global_ReportFileMovement;
            public System.Diagnostics.Stopwatch ElapsedTime = new System.Diagnostics.Stopwatch();

            private double _TotalFileCount = 100;
            private long _MovedFileSize = 0;
            private long _TotalFileSize = 0;
            private bool _Completed = false;

            public string PrettyAvgSpeed => _MovedFileSize == 0 ? "" : $"{Math.Round(((_MovedFileSize / 1024f) / 1024f) / ElapsedTime.Elapsed.TotalSeconds, 3)} MB/sec";

            public double TotalFileCount
            {
                get => _TotalFileCount;
                set
                {
                    _TotalFileCount = value;
                    OnPropertyChanged("TotalFileCount");
                }
            }

            public long MovedFileSize
            {
                get => _MovedFileSize;
                set
                {
                    _MovedFileSize = value;
                    OnPropertyChanged("MovedFileSize");
                    OnPropertyChanged("ProgressBarPerc");
                    OnPropertyChanged("PrettyAvgSpeed");
                }
            }

            public long TotalFileSize
            {
                get => _TotalFileSize;
                set
                {
                    _TotalFileSize = value;
                    OnPropertyChanged("TotalFileSize");
                }
            }

            public double ProgressBarPerc
            {
                get
                {
                    double perc = Math.Ceiling((double)(100 * _MovedFileSize) / _TotalFileSize);
                    Main.FormAccessor.TaskbarItemInfo.ProgressValue = perc / 100;

                    if (perc == 100)
                    {
                        Main.FormAccessor.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                        Main.FormAccessor.TaskbarItemInfo.ProgressValue = 0;
                    }

                    return _MovedFileSize == 0 ? 0 : perc;
                }
            }

            public bool Completed
            {
                get => _Completed;
                set
                {
                    _Completed = value;
                    OnPropertyChanged("Completed");
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public class JunkInfo
        {
            public System.IO.FileSystemInfo FSInfo { get; set; }
            public Library Library { get; set; }
            public long Size { get; set; }
            public string PrettyFolderSize => Functions.FileSystem.FormatBytes(Size);
        }
    }
}
