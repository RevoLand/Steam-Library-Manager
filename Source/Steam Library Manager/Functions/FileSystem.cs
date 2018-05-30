using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Steam_Library_Manager.Functions
{
    internal static class FileSystem
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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
            catch (DirectoryNotFoundException ex)
            {
                logger.Error(ex);
            }
            catch (IOException ex)
            {
                logger.Error(ex);
            }
            catch (AggregateException ex)
            {
                logger.Error(ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.Error(ex);
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
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
                logger.Error(ex);
                return 0;
            }
            // on error, return 0
            catch (Exception ex)
            {
                Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
                logger.Fatal(ex);
                return 0;
            }
        }

        // Source: http://stackoverflow.com/a/2082893
        public static string FormatBytes(long length)
        {
            try
            {
                long B = 0, KB = 1024, MB = KB * 1024, GB = MB * 1024, TB = GB * 1024;
                double size = length;
                string suffix = nameof(B);

                if (length >= TB)
                {
                    size = Math.Round((double)length / TB, 2);
                    suffix = nameof(TB);
                }
                else if (length >= GB)
                {
                    size = Math.Round((double)length / GB, 2);
                    suffix = nameof(GB);
                }
                else if (length >= MB)
                {
                    size = Math.Round((double)length / MB, 2);
                    suffix = nameof(MB);
                }
                else if (length >= KB)
                {
                    size = Math.Round((double)length / KB, 2);
                    suffix = nameof(KB);
                }

                return $"{size} {suffix}";
            }
            catch (Exception ex)
            {
                logger.Error(ex);
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
                logger.Fatal(ae);

                return 0;
            }
            catch (Exception ex)
            {
                Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
                logger.Fatal(ex);
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
                logger.Fatal(ae);

                return 0;
            }
            catch (Exception ex)
            {
                Definitions.SLM.RavenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
                logger.Fatal(ex);
                return 0;
            }
        }
    }
}