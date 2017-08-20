using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Steam_Library_Manager.Functions
{
    public class Logger
    {
        public enum LogType
        {
            SLM,
            Game,
            Library,
            TaskManager
        }

        class GameLog
        {
            public string Message;
            public Definitions.Game Game;
        }

        static BlockingCollection<string> SLMLogs = new BlockingCollection<string>();
        static BlockingCollection<GameLog> GameLogs = new BlockingCollection<GameLog>();
        static BlockingCollection<string> LibraryLogs = new BlockingCollection<string>();
        static BlockingCollection<string> TaskManagerLogs = new BlockingCollection<string>();

        static DirectoryInfo SLMLogDirectory = new DirectoryInfo(Path.Combine(Definitions.Directories.SLM.LogDirectory, "SLM"));
        static DirectoryInfo LibraryLogDirectory = new DirectoryInfo(Path.Combine(Definitions.Directories.SLM.LogDirectory, "Libraries"));
        static DirectoryInfo TaskManagerLogDirectory = new DirectoryInfo(Path.Combine(Definitions.Directories.SLM.LogDirectory, "TaskManager"));

        public static void StartLogger()
        {
            #region SLM Logs
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                try
                {
                    while (true)
                    {
                        ProcessSLMLogs(SLMLogs.Take());
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    MessageBox.Show(ex.ToString());
                }
            });
            #endregion

            #region Game Logs
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                try
                {
                    while (true)
                    {
                        ProcessGameLogs(GameLogs.Take());
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    MessageBox.Show(ex.ToString());
                }
            });
            #endregion

            #region Library Logs
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                try
                {
                    while (true)
                    {
                        ProcessLibraryLogs(LibraryLogs.Take());
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    MessageBox.Show(ex.ToString());
                }
            });
            #endregion

            #region Task Manager Logs
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                try
                {
                    while (true)
                    {
                        ProcessTaskManagerLogs(TaskManagerLogs.Take());
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    MessageBox.Show(ex.ToString());
                }
            });
            #endregion
        }

        static void ProcessSLMLogs(string LogMessage)
        {
            try
            {
                if (!SLMLogDirectory.Exists)
                    SLMLogDirectory.Create();

                FileInfo Logfile = new FileInfo(Path.Combine(SLMLogDirectory.FullName, $"{Process.GetCurrentProcess().StartTime.ToString("d.M - H.mm.ss")}.txt"));

                using (StreamWriter FileWriter = (!Logfile.Exists) ? File.CreateText(Logfile.FullName) : Logfile.AppendText())
                {
                    FileWriter.WriteLine(LogMessage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Debug.WriteLine(ex);
            }
        }

        static void ProcessGameLogs(GameLog Log)
        {
            try
            {
                DirectoryInfo GameLogDirectory = new DirectoryInfo(Path.Combine(Definitions.Directories.SLM.LogDirectory, "Game", Log.Game.InstallationPath.Name));

                if (!GameLogDirectory.Exists)
                    GameLogDirectory.Create();

                FileInfo Logfile = new FileInfo(Path.Combine(GameLogDirectory.FullName, $"{Process.GetCurrentProcess().StartTime.ToString("d.M - H.mm.ss")}.txt"));

                using (StreamWriter FileWriter = (!Logfile.Exists) ? File.CreateText(Logfile.FullName) : Logfile.AppendText())
                {
                    FileWriter.WriteLine(Log.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Debug.WriteLine(ex);
            }
        }

        static void ProcessLibraryLogs(string LogMessage)
        {
            try
            {
                if (!LibraryLogDirectory.Exists)
                    LibraryLogDirectory.Create();

                FileInfo Logfile = new FileInfo(Path.Combine(LibraryLogDirectory.FullName, $"{Process.GetCurrentProcess().StartTime.ToString("d.M - H.mm.ss")}.txt"));

                using (StreamWriter FileWriter = (!Logfile.Exists) ? File.CreateText(Logfile.FullName) : Logfile.AppendText())
                {
                    FileWriter.WriteLine(LogMessage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Debug.WriteLine(ex);
            }
        }

        static void ProcessTaskManagerLogs(string LogMessage)
        {
            try
            {
                if (!TaskManagerLogDirectory.Exists)
                    TaskManagerLogDirectory.Create();

                FileInfo Logfile = new FileInfo(Path.Combine(TaskManagerLogDirectory.FullName, $"{Process.GetCurrentProcess().StartTime.ToString("d.M - H.mm.ss")}.txt"));

                using (StreamWriter FileWriter = (!Logfile.Exists) ? File.CreateText(Logfile.FullName) : Logfile.AppendText())
                {
                    FileWriter.WriteLine(LogMessage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Debug.WriteLine(ex);
            }
        }

        public static void LogToFile(LogType LogType, string LogMessage, Definitions.Game Game = null)
        {
            try
            {
                if (!Properties.Settings.Default.Advanced_Logging)
                    return;

                switch (LogType)
                {
                    case LogType.SLM:
                        SLMLogs.Add(($"[{DateTime.Now}] {LogMessage}"));
                        break;
                    case LogType.Game:
                        GameLogs.Add(new GameLog { Game = Game, Message = $"[{DateTime.Now}] {LogMessage}" });
                        break;
                    case LogType.Library:
                        LibraryLogs.Add(($"[{DateTime.Now}] {LogMessage}"));
                        break;
                    case LogType.TaskManager:
                        TaskManagerLogs.Add(($"[{DateTime.Now}] {LogMessage}"));
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show(ex.ToString());
                Logger.LogToFile(Functions.Logger.LogType.TaskManager, ex.ToString());
            }
        }
    }
}
