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
                string FilePath = Definitions.Directories.SLM.CurrentDirectory + "\\" + ErrorType + ".txt";

                // If file not exists
                if (!File.Exists(FilePath))
                {
                    // Create file
                    using (StreamWriter FileWriter = File.CreateText(FilePath))
                    {
                        // Write current time to text
                        FileWriter.WriteLine("!--------------------------------------------------" + DateTime.Now.ToString() + " --------------------------------------------------!");

                        // Write error to file
                        FileWriter.WriteLine(Error);

                        // Close file writer
                        FileWriter.Close();
                    }
                }
                // If file exists
                else
                {
                    // Open file writer in append mode
                    using (StreamWriter FileWriter = File.AppendText(FilePath))
                    {
                        // Writer current time to text
                        FileWriter.WriteLine("!--------------------------------------------------" + DateTime.Now.ToString() + " --------------------------------------------------!");

                        // Write error to file
                        FileWriter.WriteLine(Error);

                        // Close file writer
                        FileWriter.Close();
                    }
                }
            }
            catch (System.Exception ex)
            {
                // In case of an error, show the error to user as messagebox
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
        }
    }
}
