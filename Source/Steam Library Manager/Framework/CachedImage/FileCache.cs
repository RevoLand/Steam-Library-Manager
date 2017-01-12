using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;

// https://github.com/floydpink/CachedImage
// Highly modified over the original one

namespace Steam_Library_Manager.Framework.CachedImage
{
    public static class FileCache
    {
        public static async System.Threading.Tasks.Task<MemoryStream> HitAsync(Uri url)
        {
            try
            {
                string AppCacheDirectory = Path.Combine(Definitions.SLM.selectedLibrary.SteamAppsFolder.FullName, "HeaderImages");
                var localFile = $"{AppCacheDirectory}\\{url.AbsolutePath.Replace("/steam/apps/", "").Replace("/header", "")}";

                MemoryStream memoryStream = new MemoryStream();

                if (!File.Exists(localFile))
                {
                    if (!Directory.Exists(AppCacheDirectory))
                        Directory.CreateDirectory(AppCacheDirectory);

                    new WebClient().DownloadFileAsync(url, localFile);

                    await (await new WebClient().OpenReadTaskAsync(url)).CopyToAsync(memoryStream);
                }
                else
                {
                    await (new FileStream(localFile, FileMode.Open, FileAccess.Read)).CopyToAsync(memoryStream);
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream;
            }
            catch (WebException)
            {
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Debug.WriteLine(ex);

                return null;
            }
        }
    }
}