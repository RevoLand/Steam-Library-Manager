using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

// https://github.com/floydpink/CachedImage

namespace Steam_Library_Manager.Framework.CachedImage
{
    public static class FileCache
    {
        public enum CacheMode
        {
            WinINet,
            Dedicated
        }

        // Record whether a file is being written.
        private static readonly Dictionary<string, bool> IsWritingFile = new Dictionary<string, bool>();

        static FileCache()
        {
            // default cache directory - can be changed if needed from App.xaml
            AppCacheDirectory = Path.Combine(Definitions.SLM.selectedLibrary.steamAppsPath.FullName, "HeaderImages");
            AppCacheMode = CacheMode.Dedicated;
        }

        /// <summary>
        ///     Gets or sets the path to the folder that stores the cache file. Only works when AppCacheMode is
        ///     CacheMode.Dedicated.
        /// </summary>
        public static string AppCacheDirectory { get; set; }

        /// <summary>
        ///     Gets or sets the cache mode. WinINet is recommended, it's provided by .Net Framework and uses the Temporary Files
        ///     of IE and the same cache policy of IE.
        /// </summary>
        public static CacheMode AppCacheMode { get; set; }

        public static async Task<MemoryStream> HitAsync(Uri url)
        {
            if (!Directory.Exists(AppCacheDirectory))
            {
                Directory.CreateDirectory(AppCacheDirectory);
            }

            var uri = url;

            // /steam/apps/107410/header.jpg
            var fileName = uri.AbsolutePath.Replace("/steam/apps/", "").Replace("/header", "");
            var localFile = string.Format("{0}\\{1}", AppCacheDirectory, fileName);
            var memoryStream = new MemoryStream();

            FileStream fileStream = null;
            if (!IsWritingFile.ContainsKey(fileName) && File.Exists(localFile))
            {
                using (fileStream = new FileStream(localFile, FileMode.Open, FileAccess.Read))
                {
                    await fileStream.CopyToAsync(memoryStream);
                }
                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream;
            }

            var request = WebRequest.Create(uri);
            request.Timeout = 30;
            try
            {
                var response = await request.GetResponseAsync();
                var responseStream = response.GetResponseStream();
                if (responseStream == null)
                    return null;
                if (!IsWritingFile.ContainsKey(fileName))
                {
                    IsWritingFile[fileName] = true;
                    fileStream = new FileStream(localFile, FileMode.Create, FileAccess.Write);
                }

                using (responseStream)
                {
                    var bytebuffer = new byte[100];
                    int bytesRead;
                    do
                    {
                        bytesRead = await responseStream.ReadAsync(bytebuffer, 0, 100);
                        if (fileStream != null)
                            await fileStream.WriteAsync(bytebuffer, 0, bytesRead);
                        await memoryStream.WriteAsync(bytebuffer, 0, bytesRead);
                    } while (bytesRead > 0);
                    if (fileStream != null)
                    {
                        await fileStream.FlushAsync();
                        fileStream.Dispose();
                        IsWritingFile.Remove(fileName);
                    }
                }
                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream;
            }
            catch (WebException)
            {
                return null;
            }
        }
    }
}