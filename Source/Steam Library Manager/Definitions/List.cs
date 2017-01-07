using System.ComponentModel;

namespace Steam_Library_Manager.Definitions
{
    // Our Library and Game definitions exists there
    public class List
    {
        // Make a new list for Library details
        public static Framework.AsyncObservableCollection<Library> Libraries = new Framework.AsyncObservableCollection<Library>();
        public static Framework.AsyncObservableCollection<ContextMenu> libraryContextMenuItems = new Framework.AsyncObservableCollection<ContextMenu>();
        public static Framework.AsyncObservableCollection<ContextMenu> gameContextMenuItems = new Framework.AsyncObservableCollection<ContextMenu>();

        public class TaskList : INotifyPropertyChanged
        {
            public Game TargetGame { get; set; }
            public Library TargetLibrary { get; set; }
            public bool Moving = false;
            public bool Compress { get; set; } = Properties.Settings.Default.Global_Compress;
            public bool RemoveOldFiles { get; set; } = Properties.Settings.Default.Global_RemoveOldFiles;
            public bool ReportFileMovement { get; set; } = Properties.Settings.Default.Global_ReportFileMovement;

            private double _ProgressBar = 0;
            public double ProgressBar
            {
                get => _ProgressBar;
                set
                {
                    _ProgressBar = value;
                    OnPropertyChanged("ProgressBar");
                }
            }
            private double _ProgressBarMax = 100;
            public double ProgressBarMax
            {
                get => _ProgressBarMax;
                set
                {
                    _ProgressBarMax = value;
                    OnPropertyChanged("ProgressBarMax");
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string info)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
