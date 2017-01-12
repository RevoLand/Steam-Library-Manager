using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Steam_Library_Manager.Functions
{
    class FileSystem
    {
        public static void RemoveGivenFiles(ConcurrentBag<string> fileList, ConcurrentBag<string> directoryList = null)
        {
            Parallel.ForEach(fileList, currentFile =>
            {
                try
                {
                    FileInfo file = new FileInfo(currentFile);

                    if (file.Exists)
                        file.Delete();
                }
                catch { }
            });

            if (directoryList != null)
            {
                Parallel.ForEach(directoryList, currentDirectory =>
                {
                    try
                    {
                        DirectoryInfo directory = new DirectoryInfo(currentDirectory);

                        if (directory.Exists)
                            directory.Delete();
                    }
                    catch { }
                });
            }
        }

        // Get directory size from path, with or without sub directories
        public static long GetDirectorySize(DirectoryInfo directoryPath, bool includeSub)
        {
            try
            {
                if (!Directory.Exists(directoryPath.FullName))
                    return 0;

                // Define a "long" for directory size
                long directorySize = 0;

                foreach (FileInfo currentFile in directoryPath.GetFileSystemInfos("*", (includeSub) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).Where(x => x is FileInfo))
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
                // And return available free space from defined drive info
                return new DriveInfo(Path.GetPathRoot(TargetFolder)).AvailableFreeSpace;
            }
            catch { return 0; }
        }

        public static long GetTotalSize(string TargetFolder)
        {
            try
            {
                return new DriveInfo(Path.GetPathRoot(TargetFolder)).TotalSize;
            }
            catch { return 0; }
        }

        public static void DeleteOldLibrary(Definitions.Library Library)
        {
            try
            {
                if (Library.SteamAppsFolder.Exists)
                    Library.SteamAppsFolder.Delete(true);

                if (Library.WorkshopFolder.Exists)
                    Library.WorkshopFolder.Delete(true);

                if (Library.DownloadFolder.Exists)
                    Library.DownloadFolder.Delete(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
