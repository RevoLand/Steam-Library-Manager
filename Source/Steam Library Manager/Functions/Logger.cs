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
            App,
            Library,
            TaskManager
        }

        class AppLog
        {
            public string Message;
            public Definitions.AppInfo App;
        }

        static BlockingCollection<string> SLMLogs = new BlockingCollection<string>();
        static BlockingCollection<AppLog> AppLogs = new BlockingCollection<AppLog>();
        static BlockingCollection<string> LibraryLogs = new BlockingCollection<string>();
        static BlockingCollection<string> TaskManagerLogs = new BlockingCollection<string>();

        static DirectoryInfo SLMLogDirectory = new DirectoryInfo(Path.Combine(Definitions.Directories.SLM.Log, "SLM"));
        static DirectoryInfo LibraryLogDirectory = new DirectoryInfo(Path.Combine(Definitions.Directories.SLM.Log, "Libraries"));
        static DirectoryInfo TaskManagerLogDirectory = new DirectoryInfo(Path.Combine(Definitions.Directories.SLM.Log, "TaskManager"));

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
                        System.Threading.Thread.Sleep(1);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    MessageBox.Show(ex.ToString());
                }
            });
            #endregion

            #region App Logs
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                try
                {
                    while (true)
                    {
                        ProcessAppLogs(AppLogs.Take());
                        System.Threading.Thread.Sleep(1);
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
                        System.Threading.Thread.Sleep(1);
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
                        System.Threading.Thread.Sleep(1);
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
                {
                    SLMLogDirectory.Create();
                }

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

        static void ProcessAppLogs(AppLog Log)
        {
            try
            {
                DirectoryInfo AppLogDirectory = new DirectoryInfo(Path.Combine(Definitions.Directories.SLM.Log, "App", Log.App.InstallationPath.Name));

                if (!AppLogDirectory.Exists)
                {
                    AppLogDirectory.Create();
                }

                FileInfo Logfile = new FileInfo(Path.Combine(AppLogDirectory.FullName, $"{Process.GetCurrentProcess().StartTime.ToString("d.M - H.mm.ss")}.txt"));

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
                {
                    LibraryLogDirectory.Create();
                }

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
                {
                    TaskManagerLogDirectory.Create();
                }

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

        public static void LogToFile(LogType LogType, string LogMessage, Definitions.AppInfo App = null)
        {
            try
            {
                if (!Properties.Settings.Default.Advanced_Logging)
                {
                    return;
                }

                switch (LogType)
                {
                    case LogType.SLM:
                        SLMLogs.Add(($"[{DateTime.Now}] {LogMessage}"));
                        break;
                    case LogType.App:
                        AppLogs.Add(new AppLog { App = App, Message = $"[{DateTime.Now}] {LogMessage}" });
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
                Logger.LogToFile(LogType.TaskManager, ex.ToString());
            }
        }
    }
}
