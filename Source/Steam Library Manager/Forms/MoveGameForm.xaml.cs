using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Steam_Library_Manager.Forms
{
    /// <summary>
    /// Interaction logic for MoveGameForm.xaml
    /// </summary>
    /// 

    public partial class MoveGameForm : Window
    {
        // Define our game
        Definitions.Game Game { get; set; }

        // Define our library from LatestDropLibrary
        Definitions.Library targetLibrary;

        // Define cancellation token
        System.Threading.CancellationTokenSource cancellationToken;

        // Define task
        System.Threading.Tasks.Task currentTask;

        ProgressReport pr = new ProgressReport();

        class ProgressReport : INotifyPropertyChanged
        {
            int progressBar;
            public int ProgressBar
            {
                get { return progressBar; }
                set
                {
                    progressBar = value;
                    NotifyPropertyChanged("ProgressBar");
                }
            }

            int movenFileCount;
            public int MovenFileCount
            {
                get { return movenFileCount; }
                set
                {
                    movenFileCount = value;
                    NotifyPropertyChanged("MovenFileCount");
                }
            }

            long movenFileSize;
            public long MovenFileSize
            {
                get { return movenFileSize; }
                set
                {
                    movenFileSize = value;
                    NotifyPropertyChanged("MovenFileSize");
                }
            }

            string progressLabel;
            public string ProgressLabel
            {
                get { return progressLabel; }
                set
                {
                    progressLabel = value;
                    NotifyPropertyChanged("ProgressLabel");
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            private void NotifyPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public Framework.AsyncObservableCollection<string> formLogs = new Framework.AsyncObservableCollection<string>();

        public MoveGameForm(Definitions.Game gameToMove, Definitions.Library libraryToMove)
        {
            InitializeComponent();

            Game = gameToMove;
            targetLibrary = libraryToMove;

            textBox.ItemsSource = formLogs;
        }

        private void MoveGameForm_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = Game;
            progressReportGrid.DataContext = pr;

            targetLibraryText.Content = targetLibrary.FullPath;
        }

        private void MoveGameForm_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.moveGameFormPlacement = Framework.NativeMethods.WindowPlacement.GetPlacement(this);

            if (cancellationToken != null && cancellationToken.Token.CanBeCanceled)
                cancellationToken.Cancel();
        }

        private void MoveGameForm_SourceInitialized(object sender, EventArgs e)
        {
            Framework.NativeMethods.WindowPlacement.SetPlacement(this, Properties.Settings.Default.moveGameFormPlacement);
        }

        public void ReportFileMovement(string movenFileName, int totalFileCount, long movenFileSize, long totalFileSize)
        {
            pr.MovenFileCount += 1;
            pr.MovenFileSize += movenFileSize;

            pr.ProgressBar = ((int)Math.Round((double)(100 * pr.MovenFileSize) / totalFileSize));
            pr.ProgressLabel = $"{Functions.FileSystem.FormatBytes(totalFileSize - pr.MovenFileSize)} left - {Functions.FileSystem.FormatBytes(pr.MovenFileSize)} / {Functions.FileSystem.FormatBytes(totalFileSize)}";

            formLogs.Add($"[{pr.MovenFileCount}/{totalFileCount}] {movenFileName}\n");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (currentTask == null)
            {
                bool removeOldGame = removeOldFiles.IsChecked.Value;
                bool compressGame = compress.IsChecked.Value;

                formLogs.Clear();
                button.Content = "Cancel";

                cancellationToken = new System.Threading.CancellationTokenSource();

                currentTask = new System.Threading.Tasks.TaskFactory(cancellationToken.Token).StartNew(() =>
                {
                    try
                    {
                        Game.CopyGameFiles(this, targetLibrary, cancellationToken, compressGame);

                        if (!cancellationToken.IsCancellationRequested)
                        {
                            // If game is not exists in the target library
                            if (targetLibrary.Games.Count(x => x.AcfName == Game.AcfName && compressGame == x.IsCompressed) == 0)
                            {
                                // Add game to new library
                                Functions.Games.AddNewGame(Game.FullAcfPath.FullName.Replace(Game.InstalledLibrary.steamAppsPath.FullName, targetLibrary.steamAppsPath.FullName), Game.AppID, Game.AppName, Game.InstallationPath.Name, targetLibrary, Game.SizeOnDisk, compressGame);

                                // Update library details
                                targetLibrary.UpdateLibraryVisual();
                            }

                            if (removeOldGame)
                            {
                                if (Game.DeleteFiles())
                                {
                                    if (Definitions.SLM.selectedLibrary == Game.InstalledLibrary)
                                        Functions.Games.UpdateMainForm(Game.InstalledLibrary);

                                    Game.RemoveFromLibrary();
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                });
            }
            else if (button.Content.ToString() != "Close")
            {
                button.Content = "Copy";
                cancellationToken.Cancel();
                currentTask = null;
            }
            else
            {
                Close();
            }
        }
    }
}
