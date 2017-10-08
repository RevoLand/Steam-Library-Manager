using System;
using System.Diagnostics;
using System.IO;
using System.Net;

// https://github.com/floydpink/CachedImage
// Highly modified over the original one

namespace Steam_Library_Manager.Framework.CachedImage
{
    public static class FileCache
    {
        public static async System.Threading.Tasks.Task<MemoryStream> HitAsync(Uri Url)
        {
            try
            {
                var LocalFile = $"{Definitions.Directories.SLM.HeaderImage}\\{Url.AbsolutePath.Replace("/steam/apps/", "").Replace("/header", "")}";

                MemoryStream MemStream = new MemoryStream();

                if (!File.Exists(LocalFile))
                {
                    if (!Directory.Exists(Definitions.Directories.SLM.HeaderImage))
                    {
                        Directory.CreateDirectory(Definitions.Directories.SLM.HeaderImage);
                    }

                    new WebClient().DownloadFileAsync(Url, LocalFile);

                    await (await new WebClient().OpenReadTaskAsync(Url)).CopyToAsync(MemStream);
                }
                else
                {
                    await (new FileStream(LocalFile, FileMode.Open, FileAccess.Read)).CopyToAsync(MemStream);
                }

                MemStream.Seek(0, SeekOrigin.Begin);
                return MemStream;
            }
            catch (WebException)
            {
                return null;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                Debug.WriteLine(ex);

                return null;
            }
        }
    }
}