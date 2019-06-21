﻿using System;
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
        public static ObservableCollection<Library> Libraries { get; set; } = new ObservableCollection<Library>();

        public static ObservableCollection<JunkInfo> LcItems { get; set; } = new ObservableCollection<JunkInfo>();

        public static readonly IProgress<Library> LibraryProgress = new Progress<Library>(library => Libraries.Add(library));
        public static readonly IProgress<JunkInfo> LCProgress = new Progress<JunkInfo>(junk => LcItems.Add(junk));

        public static ObservableCollection<ContextMenuItem> LibraryCMenuItems { get; set; } = new ObservableCollection<ContextMenuItem>();
        public static ObservableCollection<ContextMenuItem> AppCMenuItems { get; set; } = new ObservableCollection<ContextMenuItem>();

        public static readonly List<Tuple<string, string>> SteamUserIDList = new List<Tuple<string, string>>();
        public static readonly Dictionary<int, DateTime> SteamApps_LastPlayedDic = new Dictionary<int, DateTime>();

        public class TaskInfo : INotifyPropertyChanged
        {
            public Enums.TaskType TaskType { get; set; }
            public App App { get; set; }
            public Library TargetLibrary { get; set; }
            public Enums.CompactLevel CompactLevel { get; set; } = (Enums.CompactLevel)Enum.Parse(typeof(Enums.CompactLevel), Properties.Settings.Default.DefaultCompactLevel);
            public bool Compact { get; set; } = true;
            public bool ForceCompact { get; set; }

            public bool ErrorHappened, Active;
            public bool Compress { get; set; } = Properties.Settings.Default.Global_Compress;
            public bool RemoveOldFiles { get; set; } = Properties.Settings.Default.Global_RemoveOldFiles;
            public bool ReportFileMovement { get; set; } = Properties.Settings.Default.Global_ReportFileMovement;
            public System.Diagnostics.Stopwatch ElapsedTime = new System.Diagnostics.Stopwatch();
            public ManualResetEvent mre = new ManualResetEvent(!Functions.TaskManager.Paused);

            private long _movedFileSize;
            public string TaskProgressInfo => (_movedFileSize == 0) ? "" : $"{_movedFileSize / 1024000} MB / {TotalFileSize / 1024000} MB";

            public string TaskStatusInfo { get; set; }

            public double TotalFileCount { get; set; }

            public long MovedFileSize
            {
                get => _movedFileSize;
                set
                {
                    _movedFileSize = value;
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
                    if (_movedFileSize != 0)
                    {
                        perc = Math.Floor((double)(100 * _movedFileSize) / TotalFileSize);
                    }

                    return _movedFileSize == 0 ? 0 : perc;
                }
            }

            public bool Completed { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public class TmInfo : INotifyPropertyChanged
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
            public string JunkReason { get; set; }
        }
    }
}