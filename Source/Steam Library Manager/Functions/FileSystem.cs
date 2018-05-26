using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Steam_Library_Manager.Functions
{
    internal static class FileSystem
    {
        public static void RemoveGivenFiles(ConcurrentBag<string> FileList, ConcurrentBag<string> DirectoryList = null, Definitions.List.TaskInfo CurrentTask = null)
        {
            try
            {
                Parallel.ForEach(FileList, currentFile =>
                {
                    FileInfo File = new FileInfo(currentFile);

                    if (File.Exists)
                    {
                        if (CurrentTask != null)
                        {
                            CurrentTask.TaskStatusInfo = $"Deleting: {File.Name} ({FormatBytes(File.Length)})";
                        }

                        System.IO.File.SetAttributes(File.FullName, FileAttributes.Normal);
                        File.Delete();
                    }
                });

                if (DirectoryList != null)
                {
                    Parallel.ForEach(DirectoryList, currentDirectory =>
                    {
                        DirectoryInfo Directory = new DirectoryInfo(currentDirectory);

                        if (Directory.Exists)
                        {
                            if (CurrentTask != null)
                            {
                                CurrentTask.TaskStatusInfo = $"Deleting directory: {Directory.Name}";
                            }

                            Directory.Delete();
                        }
                    });
                }
                if (CurrentTask != null)
                {
                    CurrentTask.TaskStatusInfo = "";
                }
            }
            catch (IOException ex)
            {
                Logger.LogToFile(Logger.LogType.SLM, ex.ToString());
            }
            catch (AggregateException ex)
            {
                Logger.LogToFile(Logger.LogType.SLM, ex.ToString());
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.LogToFile(Logger.LogType.SLM, ex.ToString());
            }
            catch (Exception ex)
            {
                Logger.LogToFile(Logger.LogType.SLM, ex.ToString());
                Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
            }
        }

        // Get directory size from path, with or without sub directories
        public static long GetDirectorySize(DirectoryInfo directoryPath, bool IncludeSubDirectories)
        {
            try
            {
                directoryPath.Refresh();

                if (!directoryPath.Exists || !new DriveInfo(Path.GetPathRoot(directoryPath.Root.FullName)).IsReady)
                {
                    return 0;
                }

                // Define a "long" for directory size
                long DirectorySize = 0;

                foreach (var CurrentFile in directoryPath.EnumerateFiles("*", (IncludeSubDirectories) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).Where(x => x is FileInfo))
                {
                    DirectorySize += CurrentFile.Length;
                }
                // and return directory size
                return DirectorySize;
            }
            catch (DirectoryNotFoundException ex)
            {
                Logger.LogToFile(Logger.LogType.SLM, ex.ToString());
                return 0;
            }
            // on error, return 0
            catch (Exception ex)
            {
                Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
                Logger.LogToFile(Logger.LogType.SLM, ex.ToString());
                return 0;
            }
        }

        // Source: http://stackoverflow.com/a/2082893
        public static string FormatBytes(long Bytes)
        {
            try
            {
                // definition of file size suffixes
                string[] Suffix = { "B", "KB", "MB", "GB", "TB" };
                int Current;
                double dblSByte = Bytes;

                for (Current = 0; Current < Suffix.Length && Bytes >= 1024; Current++, Bytes /= 1024)
                {
                    dblSByte = Bytes / 1024.0;
                }

                if (dblSByte < 0)
                {
                    dblSByte = 0;
                }

                // Format the string
                return $"{dblSByte:0.##} {Suffix[Current]}";
            }
            catch (Exception ex)
            {
                Logger.LogToFile(Logger.LogType.SLM, ex.ToString());
                return "0";
            }
        }

        public static long GetAvailableFreeSpace(string TargetFolder)
        {
            try
            {
                return new DriveInfo(Path.GetPathRoot(TargetFolder)).AvailableFreeSpace;
            }
            catch (ArgumentException ae)
            {
                ae.Data.Add("GetPathRootResult", Path.GetPathRoot(TargetFolder));
                ae.Data.Add("TargetFolder", TargetFolder);
                Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ae));
                Logger.LogToFile(Logger.LogType.SLM, ae.ToString());

                return 0;
            }
            catch (Exception ex)
            {
                Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
                Logger.LogToFile(Logger.LogType.SLM, ex.ToString());
                return 0;
            }
        }

        public static long GetAvailableTotalSpace(string TargetFolder)
        {
            try
            {
                return new DriveInfo(Path.GetPathRoot(TargetFolder)).TotalSize;
            }
            catch (ArgumentException ae)
            {
                ae.Data.Add("GetPathRootResult", Path.GetPathRoot(TargetFolder));
                ae.Data.Add("TargetFolder", TargetFolder);
                Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ae));
                Logger.LogToFile(Logger.LogType.SLM, ae.ToString());

                return 0;
            }
            catch (Exception ex)
            {
                Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
                Logger.LogToFile(Logger.LogType.SLM, ex.ToString());
                return 0;
            }
        }
    }
}