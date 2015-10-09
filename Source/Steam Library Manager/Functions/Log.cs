using System;
using System.IO;

namespace Steam_Library_Manager.Functions
{
    class Log
    {
        public static void ErrorsToFile(string ErrorType, string Error)
        {
            try
            {
                // Define error file path
                string FilePath = Path.Combine(Definitions.Directories.SLM.CurrentDirectory, ErrorType + ".txt");
                // using FileWriter, If file not exists Create a new text, else open the pre-generated file
                using (StreamWriter FileWriter = (!File.Exists(FilePath)) ? File.CreateText(FilePath) : File.AppendText(FilePath))
                {
                    // Write error to file
                    FileWriter.WriteLine(string.Format("[{0}] {1}", DateTime.Now, Error));
                }
            }
            catch (Exception ex)
            {
                // In case of an error, show the error to user as messagebox
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
        }
    }
}
