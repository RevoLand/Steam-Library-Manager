using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DirectoryInfo = Alphaleonis.Win32.Filesystem.DirectoryInfo;
using DriveInfo = Alphaleonis.Win32.Filesystem.DriveInfo;
using FileInfo = Alphaleonis.Win32.Filesystem.FileInfo;
using Path = Alphaleonis.Win32.Filesystem.Path;

namespace Steam_Library_Manager.Functions
{
    internal static class FileSystem
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static async Task RemoveGivenFilesAsync(IEnumerable<string> FileList, List<string> DirectoryList = null, Definitions.List.TaskInfo CurrentTask = null)
        {
            try
            {
                if (CurrentTask != null)
                {
                    await Task.Delay(5000);
                }

                Parallel.ForEach(FileList, currentFile =>
                {
                    FileInfo File = new FileInfo(currentFile);

                    if (File.Exists)
                    {
                        if (CurrentTask != null)
                        {
                            CurrentTask.TaskStatusInfo = Framework.StringFormat.Format(SLM.Translate(nameof(Properties.Resources.TaskStatus_DeletingFile)), new { FileName = File.Name, FormattedFileSize = FormatBytes(File.Length) });
                        }

                        Alphaleonis.Win32.Filesystem.File.SetAttributes(File.FullName, FileAttributes.Normal);
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
                                CurrentTask.TaskStatusInfo = Framework.StringFormat.Format(SLM.Translate(nameof(Properties.Resources.TaskStatus_DeletingDirectory)), new { DirectoryName = Directory.Name });
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
            }
        }

        // Get directory size from path, with or without sub directories
        public static long GetDirectorySize(DirectoryInfo directoryPath, bool IncludeSubDirectories)
        {
            try
            {
                /*
                dynamic fso = Activator.CreateInstance(Type.GetTypeFromProgID("Scripting.FileSystemObject"));
                var fldr = fso.GetFolder(directoryPath.FullName);

                return Convert.ToInt64((ulong)fldr.size);
                */

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

        // Pinvoke for API function
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetDiskFreeSpaceEx(string lpDirectoryName,
            out ulong lpFreeBytesAvailable,
            out ulong lpTotalNumberOfBytes,
            out ulong lpTotalNumberOfFreeBytes);
    }
}