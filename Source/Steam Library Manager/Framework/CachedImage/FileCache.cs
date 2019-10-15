using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Directory = Alphaleonis.Win32.Filesystem.Directory;
using File = Alphaleonis.Win32.Filesystem.File;
using Path = Alphaleonis.Win32.Filesystem.Path;

// https://github.com/frogcrush/CachedImage

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

        static FileCache() => AppCacheMode = CacheMode.Dedicated;

        /// <summary>
        ///     Gets or sets the cache mode. WinINet is recommended, it's provided by .Net Framework and uses the Temporary Files
        ///     of IE and the same cache policy of IE.
        /// </summary>
        public static CacheMode AppCacheMode { get; set; }

        public static async Task<MemoryStream> HitAsync(string url, string filename = null)
        {
            if (!Directory.Exists(Definitions.Directories.SLM.Cache))
            {
                Directory.CreateDirectory(Definitions.Directories.SLM.Cache);
            }

            var uri = new Uri(url);
            var fileNameBuilder = new StringBuilder();
            using (var sha1 = new SHA1Managed())
            {
                var canonicalUrl = uri.ToString();
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(canonicalUrl));
                fileNameBuilder.Append(BitConverter.ToString(hash).Replace("-", "").ToLower());
                if (Path.HasExtension(canonicalUrl))
                    fileNameBuilder.Append(Path.GetExtension(canonicalUrl.Split('?')[0]));
            }

            var fileName = fileNameBuilder.ToString();
            string localFile;
            if (string.IsNullOrEmpty(filename))
            {
                if (url.Contains("origin"))
                {
                    localFile = $"{Definitions.Directories.SLM.Cache}\\{uri.Segments[2].Replace("/", "")}.jpg";
                }
                else if (url.Contains("http"))
                {
                    localFile = $"{Definitions.Directories.SLM.Cache}\\{uri.Segments[3].Replace("/", "")}.jpg";
                }
                else
                {
                    localFile = uri.LocalPath;
                }
            }
            else
            {
                localFile = $"{Definitions.Directories.SLM.Cache}\\{filename}.jpg";
            }

            var memoryStream = new MemoryStream();

            FileStream fileStream = null;
            if (!IsWritingFile.ContainsKey(fileName) && File.Exists(localFile))
            {
                using (fileStream = new FileStream(localFile, FileMode.Open, FileAccess.Read))
                {
                    await fileStream.CopyToAsync(memoryStream).ConfigureAwait(false);
                }
                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream;
            }

            var request = WebRequest.Create(uri);
            request.Timeout = 10;
            try
            {
                var response = await request.GetResponseAsync().ConfigureAwait(false);
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
                    var bytebuffer = new byte[1024];
                    int bytesRead;
                    do
                    {
                        bytesRead = await responseStream.ReadAsync(bytebuffer, 0, 1024).ConfigureAwait(false);
                        if (fileStream != null)
                            await fileStream.WriteAsync(bytebuffer, 0, bytesRead).ConfigureAwait(false);
                        await memoryStream.WriteAsync(bytebuffer, 0, bytesRead).ConfigureAwait(false);
                    } while (bytesRead > 0);
                    if (fileStream != null)
                    {
                        await fileStream.FlushAsync().ConfigureAwait(false);
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