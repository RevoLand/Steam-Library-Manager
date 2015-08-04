using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System;

namespace Steam_Library_Manager.Functions
{
    class FileSystem
    {
        // Get directory size from path, with or without sub directories
        public static long GetDirectorySize(string directoryPath, bool includeSub)
        {
            try
            {
                // Define a "long" for directory size
                long directorySize = 0;

                // For each file in the given directory
                foreach (Framework.FileData currentFile in Framework.FastDirectoryEnumerator.EnumerateFiles(directoryPath, "*", (includeSub) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                {
                    // add current file size to directory size
                    directorySize += currentFile.Size;
                }

                // and return directory size
                return directorySize;
            }
            // on error, return 0
            catch { return 0; }
        }

        public static byte[] GetFileMD5(string filePath)
        {
            // Create a new md5 function and using it
            using (var MD5 = System.Security.Cryptography.MD5.Create())
            {
                // Compute md5 hash of given file and return the hash value
                return MD5.ComputeHash(File.OpenRead(filePath));
            }
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

            // Format the string
            return string.Format("{0:0.##} {1}", dblSByte, Suffix[current]);
        }

        public static long GetFreeSpace(string TargetFolder)
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


        //Source : http://stackoverflow.com/questions/1766748/how-do-i-get-a-relative-path-from-one-path-to-another-in-c-sharp
        [DllImport("shlwapi.dll", EntryPoint = "PathRelativePathTo")]
        protected static extern bool PathRelativePathTo(StringBuilder lpszDst, string from, UInt32 attrFrom, string to, UInt32 attrTo);

        public static string GetRelativePath(string from, string to)
        {
            StringBuilder builder = new StringBuilder(1024);
            bool result = PathRelativePathTo(builder, from, 0, to, 0);
            return builder.ToString();
        }
    }
}
