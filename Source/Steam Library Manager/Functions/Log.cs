using System;
using System.IO;

namespace Steam_Library_Manager.Functions
{
    class Log
    {
        public static void LogErrorsToFile(string ErrorType, string Error)
        {
            try
            {
                string FilePath = Definitions.Directories.SLM.CurrentDirectory + "\\" + ErrorType + ".txt";

                if (!File.Exists(FilePath))
                {
                    using (StreamWriter FileWriter = File.CreateText(FilePath))
                    {
                        FileWriter.WriteLine("!--------------------------------------------------" + DateTime.Now.ToString() + " --------------------------------------------------!");
                        FileWriter.WriteLine(Error);
                        FileWriter.Close();
                    }
                }
                else
                {
                    using (StreamWriter FileWriter = File.AppendText(FilePath))
                    {
                        FileWriter.WriteLine("!--------------------------------------------------" + DateTime.Now.ToString() + " --------------------------------------------------!");
                        FileWriter.WriteLine(Error);
                        FileWriter.Close();
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
        }
    }
}
