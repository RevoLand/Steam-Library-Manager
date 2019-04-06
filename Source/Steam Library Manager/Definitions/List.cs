using System;
using System.Collections.Generic;
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

        public static List<Tuple<string, string>> SteamUserIDList = new List<Tuple<string, string>>();
        public static Dictionary<int, DateTime> SteamApps_LastPlayedDic = new Dictionary<int, DateTime>();

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

            private long _MovedFileSize;
            public string TaskProgressInfo => (_MovedFileSize == 0) ? "" : $"{_MovedFileSize / 1024000} MB / {TotalFileSize / 1024000} MB";

            public string TaskStatusInfo { get; set; }

            public double TotalFileCount { get; set; }

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

            public long TotalFileSize { get; set; }

            public double ProgressBarPerc
            {
                get
                {
                    double perc = 0;
                    if (_MovedFileSize != 0)
                    {
                        perc = Math.Floor((double)(100 * _MovedFileSize) / TotalFileSize);
                    }

                    return _MovedFileSize == 0 ? 0 : perc;
                }
            }

            public bool Completed { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public class TMInfo : INotifyPropertyChanged
        {
            public int PendingTasks { get; set; }
            public int CompletedTasks { get; set; }
            public int TotalTasks { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;
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