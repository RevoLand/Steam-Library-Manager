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
        private string _pathToCacheFolder;
        private bool _withoutCaching;
        private string _pathToCachedFile;
        private string _url;

        public PictureBoxWithCaching()
        {
            try
            {
                _pathToCacheFolder = Definitions.Directories.SLM.CacheDirectory;
                if (!Directory.Exists(_pathToCacheFolder))
                {
                    Directory.CreateDirectory(_pathToCacheFolder);
                }
                _bw.DoWork += DoWork;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                _withoutCaching = true;
            }
        }

        public new void LoadAsync(string url)
        {
            if (_withoutCaching)
            {
                base.LoadAsync(url);
                return;
            }
            _url = url;

            var urlAsMD5 = calculateMD5(url);
             _pathToCachedFile = Path.Combine(_pathToCacheFolder, urlAsMD5);
            
            if (!File.Exists(_pathToCachedFile))
            {
                _bw.RunWorkerAsync();
            }
            else
            {
                base.LoadAsync(_pathToCachedFile);
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

        private BackgroundWorker _bw = new BackgroundWorker();

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            var wc = new WebClient();
            wc.DownloadFileAsync(new Uri(_url), _pathToCachedFile);
            base.LoadAsync(_pathToCachedFile);
        }
    }
}
