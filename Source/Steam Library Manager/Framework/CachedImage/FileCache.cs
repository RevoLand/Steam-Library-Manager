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
                var LocalFile = $"{Definitions.Directories.SLM.Cache}\\{Url.AbsolutePath.Replace("/steam/apps/", "").Replace("/header", "")}";
                MemoryStream MemStream = new MemoryStream();

                if (!File.Exists(LocalFile))
                {
                    if (!Directory.Exists(Definitions.Directories.SLM.Cache))
                    {
                        Directory.CreateDirectory(Definitions.Directories.SLM.Cache);
                    }

                    await (await new WebClient().OpenReadTaskAsync(Url).ConfigureAwait(false)).CopyToAsync(MemStream).ConfigureAwait(false);
                    using (FileStream fs = File.OpenWrite(LocalFile))
                    {
                        MemStream.Seek(0, SeekOrigin.Begin);
                        await MemStream.CopyToAsync(fs).ConfigureAwait(false);
                    }
                }
                else
                {
                    using (FileStream fs = File.Open(LocalFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        await fs.CopyToAsync(MemStream).ConfigureAwait(false);
                    }
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