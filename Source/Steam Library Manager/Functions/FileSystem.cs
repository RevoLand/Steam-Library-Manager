using System;
using System.IO;
using System.Threading;

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
            catch { return false; }
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
    }
}
