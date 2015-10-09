using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

/*
Copyright for this file goes to white_ghost, Thanks!

http://steamcommunity.com/profiles/76561197991469081

*/

namespace Steam_Library_Manager.Framework
{
    class PictureBoxWithCaching:PictureBox
    {
        private bool _withoutCaching = false;
        private string _pathToCachedFile, _url;
        private BackgroundWorker _bw = new BackgroundWorker();

        public PictureBoxWithCaching()
        {
            try
            {
                if (!Directory.Exists(Definitions.Directories.SLM.CacheDirectory))
                {
                    Directory.CreateDirectory(Definitions.Directories.SLM.CacheDirectory);
                }

                _bw.DoWork += DoWork;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                _withoutCaching = true;
            }
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (!File.Exists(_pathToCachedFile) || new FileInfo(_pathToCachedFile).Length == 0)
                {
                    WebClient wc = new WebClient();
                    wc.DownloadFile(new Uri(_url), _pathToCachedFile);
                    base.LoadAsync(_pathToCachedFile);
                }
            }
            catch (WebException)
            {
                base.LoadAsync("SLM"); // so it will load "no image available"
                //_withoutCaching = true;
            }
        }

        public new void LoadAsync(string url)
        {
            try
            {
                if (_withoutCaching)
                {
                    base.LoadAsync(url);
                    return;
                }
                _url = url;

                var urlAsMD5 = calculateMD5(url);
                _pathToCachedFile = Path.Combine(Definitions.Directories.SLM.CacheDirectory, urlAsMD5);

                if (!File.Exists(_pathToCachedFile) || new FileInfo(_pathToCachedFile).Length == 0)
                    _bw.RunWorkerAsync();
                else
                    base.LoadAsync(_pathToCachedFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                _withoutCaching = true;
            }
        }

        private string calculateMD5(string input)
        {
            var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] inputStringAsByteArray = Encoding.Default.GetBytes(input);
            byte[] md5AsByteArray = md5.ComputeHash(inputStringAsByteArray);

            var sb = new StringBuilder();
            foreach (byte b in md5AsByteArray)
            {
                sb.Append(b.ToString("x2").ToLower());
            }

            return sb.ToString();
        }
    }
}
