using System.IO;

namespace Steam_Library_Manager.Definitions
{
    public class Library
    {
        public bool Main { get; set; }
        public bool Backup { get; set; }
        public int GameCount { get; set; }
        public DirectoryInfo steamAppsPath, commonPath, downloadPath, workshopPath;
        public Framework.AsyncObservableCollection<System.Windows.FrameworkElement> contextMenu { get; set; }
        public string fullPath { get; set; }
        public string prettyFreeSpace { get; set; }
        public int freeSpacePerc { get; set; }
        public long freeSpace { get; set; }
        public Framework.AsyncObservableCollection<Game> Games = new Framework.AsyncObservableCollection<Game>();
    }
}
