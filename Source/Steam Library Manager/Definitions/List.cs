using System.ComponentModel;

namespace Steam_Library_Manager.Definitions
{
    // Our Library and Game definitions exists there
    public class List
    {
        // Make a new list for Library details
        public static Framework.AsyncObservableCollection<Library> Libraries = new Framework.AsyncObservableCollection<Library>();
        public static Framework.AsyncObservableCollection<JunkInfo> JunkStuff { get; set; } = new Framework.AsyncObservableCollection<JunkInfo>();

        public static Framework.AsyncObservableCollection<ContextMenuItem> LibraryCMenuItems = new Framework.AsyncObservableCollection<ContextMenuItem>();
        public static Framework.AsyncObservableCollection<ContextMenuItem> GameCMenuItems = new Framework.AsyncObservableCollection<ContextMenuItem>();

        public class TaskList : INotifyPropertyChanged
        {
            public Game TargetGame { get; set; }
            public Library TargetLibrary { get; set; }
            public bool Moving = false;
            public bool Compress { get; set; } = Properties.Settings.Default.Global_Compress;
            public bool RemoveOldFiles { get; set; } = Properties.Settings.Default.Global_RemoveOldFiles;
            public bool ReportFileMovement { get; set; } = Properties.Settings.Default.Global_ReportFileMovement;

            private double _ProgressBar = 0;
            private double _ProgressBarMax = 100;
            private bool _Completed = false;

            public double ProgressBar
            {
                get => _ProgressBar;
                set
                {
                    _ProgressBar = value;
                    OnPropertyChanged("ProgressBar");
                }
            }

            public double ProgressBarMax
            {
                get => _ProgressBarMax;
                set
                {
                    _ProgressBarMax = value;
                    OnPropertyChanged("ProgressBarMax");
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
            protected void OnPropertyChanged(string info)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
            }
        }

        public class JunkInfo
        {
            public System.IO.FileSystemInfo FileSystemInfo { get; set; }
            public Library Library { get; set; }
            public long FolderSize { get; set; }
            public string PrettyFolderSize
            {
                get => Functions.FileSystem.FormatBytes(FolderSize);
            }
        }
    }
}
