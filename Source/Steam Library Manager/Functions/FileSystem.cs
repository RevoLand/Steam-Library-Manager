using System;
using System.IO;

namespace Steam_Library_Manager.Functions
{
    class FileSystem
    {
        public static bool TestFile(string TestDirectory)
        {
            /*
             * This is an ugly way to check if we have the needed permissions but this should work without a problem 
             */
            try
            {
                string Testfile = TestDirectory + "SLM_TestFile.txt";

                if (!Directory.Exists(TestDirectory))
                    Directory.CreateDirectory(TestDirectory);

                if (File.Exists(Testfile))
                {
                    File.Delete(Testfile);
                    if (File.Exists(Testfile))
                        return false;
                    else
                        return TestFile(TestDirectory);
                }
                else
                {
                    File.CreateText(Testfile).Close();
                    if (File.Exists(Testfile))
                    {
                        File.Delete(Testfile);
                        return true;
                    }
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
                return false;
            }
        }

        public static long GetDirectorySize(string directoryPath, bool includeSub)
        {
            try
            {
                long directorySize = 0;

                foreach (Framework.FileData currentFile in Framework.FastDirectoryEnumerator.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories))
                {
                    directorySize += currentFile.Size;
                }

                return directorySize;
            }
            catch { return 0; }
        }

        public static byte[] GetFileMD5(string filePath)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    return md5.ComputeHash(stream);
                }
            }
        }

        // http://stackoverflow.com/a/2082893
        public static string FormatBytes(long bytes)
        {
            string[] Suffix = { "B", "KB", "MB", "GB", "TB" };
            int i;
            double dblSByte = bytes;
            for (i = 0; i < Suffix.Length && bytes >= 1024; i++, bytes /= 1024)
            {
                dblSByte = bytes / 1024.0;
            }

            return String.Format("{0:0.##} {1}", dblSByte, Suffix[i]);
        }

        public static long GetFreeSpace(string TargetFolder)
        {
            try
            {
                DriveInfo Disk = new DriveInfo(Path.GetPathRoot(TargetFolder));

                return Disk.AvailableFreeSpace;
            }
            catch { return 0; }
        }
    }
}
