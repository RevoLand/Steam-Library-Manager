using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Steam_Library_Manager.Functions
{
    class FileSystem
    {
        public static void RemoveGivenFiles(ConcurrentBag<string> fileList, ConcurrentBag<string> directoryList = null)
        {
            try
            {
                Parallel.ForEach(fileList, currentFile =>
                {
                    FileInfo file = new FileInfo(currentFile);

                    if (file.Exists)
                        file.Delete();
                });

                if (directoryList != null)
                {
                    Parallel.ForEach(directoryList, currentDirectory =>
                    {
                        DirectoryInfo directory = new DirectoryInfo(currentDirectory);

                        if (directory.Exists)
                            directory.Delete();
                    });
                }
            }
            catch { }
        }

        // Get directory size from path, with or without sub directories
        public static long GetDirectorySize(string directoryPath, bool includeSub)
        {
            try
            {
                // Define a "long" for directory size
                long directorySize = 0;

                foreach (FileInfo currentFile in new DirectoryInfo(directoryPath).GetFileSystemInfos("*", (includeSub) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                {
                    directorySize += currentFile.Length;
                }
                // and return directory size
                return directorySize;
            }
            // on error, return 0
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return 0;
            }
        }

        public static long GetFileSize(string filePath)
        {
            return new FileInfo(filePath).Length;
        }

        // Source: http://stackoverflow.com/a/2082893
        public static string FormatBytes(long bytes)
        {
            // definition of file size suffixes
            string[] Suffix = { "B", "KB", "MB", "GB", "TB" };
            int current;
            double dblSByte = bytes;

            for (current = 0; current < Suffix.Length && bytes >= 1024; current++, bytes /= 1024)
            {
                dblSByte = bytes / 1024.0;
            }

            if (dblSByte < 0)
                dblSByte = 0;

            // Format the string
            return $"{dblSByte:0.##} {Suffix[current]}";
        }

        public static long GetAvailableFreeSpace(string TargetFolder)
        {
            try
            {
                // Define a drive info
                DriveInfo Disk = new DriveInfo(Path.GetPathRoot(TargetFolder));

                // And return available free space from defined drive info
                return Disk.AvailableFreeSpace;
            }
            catch { return 0; }
        }

        public static long GetUsedSpace(string TargetFolder)
        {
            try
            {
                // Define a drive info
                DriveInfo Disk = new DriveInfo(Path.GetPathRoot(TargetFolder));

                // And return available free space from defined drive info
                return Disk.TotalSize;
            }
            catch { return 0; }
        }

        public static void DeleteOldLibrary(Definitions.Library Library)
        {
            try
            {
                if (Library.steamAppsPath.Exists)
                    Library.steamAppsPath.Delete(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
