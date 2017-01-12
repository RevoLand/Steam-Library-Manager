using System;
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
            TaskManager
        }

        public static void LogToFile(LogType LogType, string LogMessage, Definitions.Game Game = null)
        {
            try
            {
                if (!Properties.Settings.Default.Advanced_Logging)
                    return;

                DirectoryInfo LogDirectory;

                switch (LogType)
                {
                    default:
                    case LogType.SLM:
                        LogDirectory = new DirectoryInfo(Path.Combine(Definitions.Directories.SLM.LogDirectory, "SLM"));

                        break;
                    case LogType.Game:
                        LogDirectory = new DirectoryInfo(Path.Combine(Definitions.Directories.SLM.LogDirectory, "Game", Game.AppName));

                        break;
                    case LogType.TaskManager:
                        LogDirectory = new DirectoryInfo(Path.Combine(Definitions.Directories.SLM.LogDirectory, "TaskManager"));

                        break;
                }

                if (!LogDirectory.Exists)
                    LogDirectory.Create();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
