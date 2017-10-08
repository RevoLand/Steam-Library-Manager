using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Steam_Library_Manager.Functions
{
    class FileSystem
    {
        public static void RemoveGivenFiles(ConcurrentBag<string> FileList, ConcurrentBag<string> DirectoryList = null)
        {
            Parallel.ForEach(FileList, currentFile =>
            {
                try
                {
                    FileInfo File = new FileInfo(currentFile);

                    if (File.Exists)
                    {
                        File.Delete();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogToFile(Logger.LogType.SLM, ex.ToString());
                }
            });

            if (DirectoryList != null)
            {
                Parallel.ForEach(DirectoryList, currentDirectory =>
                {
                    try
                    {
                        DirectoryInfo Directory = new DirectoryInfo(currentDirectory);

                        if (Directory.Exists)
                        {
                            Directory.Delete();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogToFile(Logger.LogType.SLM, ex.ToString());
                    }
                });
            }
        }

        // Get directory size from path, with or without sub directories
        public static long GetDirectorySize(DirectoryInfo directoryPath, bool IncludeSubDirectories)
        {
            try
            {
                if (!Directory.Exists(directoryPath.FullName))
                {
                    return 0;
                }

                // Define a "long" for directory size
                long DirectorySize = 0;

                foreach (FileInfo CurrentFile in directoryPath.EnumerateFileSystemInfos("*", (IncludeSubDirectories) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).Where(x => x is FileInfo))
                {
                    DirectorySize += CurrentFile.Length;
                }
                // and return directory size
                return DirectorySize;
            }
            // on error, return 0
            catch (Exception ex)
            {
                Logger.LogToFile(Logger.LogType.SLM, ex.ToString());
                return 0;
            }
        }

        public static long GetFileSize(string FilePath)
        {
            return new FileInfo(FilePath).Length;
        }

        // Source: http://stackoverflow.com/a/2082893
        public static string FormatBytes(long Bytes)
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

        public static long GetAvailableFreeSpace(string TargetFolder)
        {
            try
            {
                if (!Directory.Exists(Path.GetPathRoot(TargetFolder)))
                {
                    return 0;
                }

                // And return available free space from defined drive info
                return new DriveInfo(Path.GetPathRoot(TargetFolder)).AvailableFreeSpace;
            }
            catch (Exception ex)
            {
                Logger.LogToFile(Logger.LogType.SLM, ex.ToString());
                return 0;
            }
        }

        public static long GetTotalSize(string TargetFolder)
        {
            try
            {
                if (!Directory.Exists(Path.GetPathRoot(TargetFolder)))
                {
                    return 0;
                }

                return new DriveInfo(Path.GetPathRoot(TargetFolder)).TotalSize;
            }
            catch (Exception ex)
            {
                Logger.LogToFile(Logger.LogType.SLM, ex.ToString());
                return 0;
            }
        }
    }
}
