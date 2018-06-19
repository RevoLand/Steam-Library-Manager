using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;

namespace Steam_Library_Manager.Definitions
{
    // Our Library and Game definitions exists there
    public static class List
    {
        // Make a new list for Library details
        public static Framework.AsyncObservableCollection<Library> Libraries = new Framework.AsyncObservableCollection<Library>();

        public static Framework.AsyncObservableCollection<JunkInfo> LCItems { get; set; } = new Framework.AsyncObservableCollection<JunkInfo>();

        public static ObservableCollection<ContextMenuItem> LibraryCMenuItems = new ObservableCollection<ContextMenuItem>();
        public static ObservableCollection<ContextMenuItem> AppCMenuItems = new ObservableCollection<ContextMenuItem>();

        public class TaskInfo : INotifyPropertyChanged
        {
            public Enums.TaskType TaskType { get; set; }
            public SteamAppInfo SteamApp { get; set; }
            public OriginAppInfo OriginApp { get; set; }
            public Library TargetLibrary { get; set; }

            public bool ErrorHappened, Active;
            public bool Compress { get; set; } = Properties.Settings.Default.Global_Compress;
            public bool RemoveOldFiles { get; set; } = Properties.Settings.Default.Global_RemoveOldFiles;
            public bool ReportFileMovement { get; set; } = Properties.Settings.Default.Global_ReportFileMovement;
            public System.Diagnostics.Stopwatch ElapsedTime = new System.Diagnostics.Stopwatch();
            public ManualResetEvent mre = new ManualResetEvent(!Framework.TaskManager.Paused);

            private double _TotalFileCount = 100;
            private long _MovedFileSize, _TotalFileSize;
            private bool _Completed;
            private string _TaskStatusInfo;

            public string TaskProgressInfo => (_MovedFileSize == 0) ? "" : $"{_MovedFileSize / 1024000} MB / {_TotalFileSize / 1024000} MB";

            public string TaskStatusInfo
            {
                get => _TaskStatusInfo;
                set
                {
                    _TaskStatusInfo = value;
                    OnPropertyChanged("TaskStatusInfo");
                }
            }

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
                    OnPropertyChanged("TaskProgressInfo");
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
                    double perc = 0;
                    if (_MovedFileSize != 0)
                    {
                        perc = Math.Floor((double)(100 * _MovedFileSize) / _TotalFileSize);
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