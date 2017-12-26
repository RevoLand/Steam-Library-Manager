using System;
using System.ComponentModel;

namespace Steam_Library_Manager.Definitions
{
    // Our Library and Game definitions exists there
    public class List
    {
        // Make a new list for Library details
        public static Framework.AsyncObservableCollection<Library> Libraries = new Framework.AsyncObservableCollection<Library>();
        public static Framework.AsyncObservableCollection<JunkInfo> LCItems { get; set; } = new Framework.AsyncObservableCollection<JunkInfo>();

        public static Framework.AsyncObservableCollection<ContextMenuItem> LibraryCMenuItems = new Framework.AsyncObservableCollection<ContextMenuItem>();
        public static Framework.AsyncObservableCollection<ContextMenuItem> AppCMenuItems = new Framework.AsyncObservableCollection<ContextMenuItem>();

        public class TaskInfo : INotifyPropertyChanged
        {
            public Enums.TaskType TaskType { get; set; }
            public AppInfo App { get; set; }
            public Library TargetLibrary { get; set; }

            public bool ErrorHappened, Active;
            public bool Compress { get; set; } = Properties.Settings.Default.Global_Compress;
            public bool RemoveOldFiles { get; set; } = Properties.Settings.Default.Global_RemoveOldFiles;
            public bool ReportFileMovement { get; set; } = Properties.Settings.Default.Global_ReportFileMovement;
            public System.Diagnostics.Stopwatch ElapsedTime = new System.Diagnostics.Stopwatch();

            private double _TotalFileCount = 100;
            private long _MovedFileSize, _TotalFileSize;
            private bool _Completed = false;
            private string _TaskStatusInfo;

            public string TaskProgressInfo => (_MovedFileSize) != 0 ? $"{_MovedFileSize}/{_TotalFileSize}" : "";

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
                    if (_MovedFileSize != 0)
                    {
                        double perc = Math.Floor((double)(100 * _MovedFileSize) / _TotalFileSize);
                        try
                        {
                            Main.FormAccessor.TaskbarItemInfo.ProgressValue = perc / 100;
                            if (perc == 100)
                            {

                                Main.FormAccessor.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                                Main.FormAccessor.TaskbarItemInfo.ProgressValue = 0;
                            }
                        }
                        catch { return 0; }

                        return _MovedFileSize == 0 ? 0 : perc;
                    }

                    return 0;
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
