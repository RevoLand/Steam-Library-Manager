namespace Steam_Library_Manager.Definitions
{
    // Our Library and Game definitions exists there
    public class List
    {
        // Make a new list for Library details
        public static Framework.AsyncObservableCollection<Library> Libraries = new Framework.AsyncObservableCollection<Library>();
        public static Framework.AsyncObservableCollection<ContextMenu> libraryContextMenuItems = new Framework.AsyncObservableCollection<ContextMenu>();
        public static Framework.AsyncObservableCollection<ContextMenu> gameContextMenuItems = new Framework.AsyncObservableCollection<ContextMenu>();

        public class TaskList
        {
            public Game TargetGame { get; set; }
            public Library TargetLibrary { get; set; }
            public bool Compress { get; set; } = false;
            public bool RemoveOldFiles { get; set; } = false;
        }
    }
}
