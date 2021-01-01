using Dasync.Collections;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Windows;
using Directory = Alphaleonis.Win32.Filesystem.Directory;
using DirectoryInfo = Alphaleonis.Win32.Filesystem.DirectoryInfo;

namespace Steam_Library_Manager.Definitions
{
    public class OriginLibrary : Library
    {
        public OriginLibrary(string fullPath, bool isMain = false)
        {
            FullPath = fullPath;
            IsMain = isMain;
            Type = Enums.LibraryType.Origin;
            DirectoryInfo = new DirectoryInfo(fullPath);

            AllowedAppTypes.Add(Enums.LibraryType.Origin);
        }

        public override async void UpdateAppList()
        {
            try
            {
                if (IsUpdatingAppList)
                    return;

                IsUpdatingAppList = true;

                Apps.Clear();

                if (!Directory.Exists(FullPath))
                {
                    IsUpdatingAppList = false;
                    return;
                }

                await Directory.EnumerateFiles(FullPath, "installerdata.xml", SearchOption.AllDirectories)
                    .ParallelForEachAsync(
                        async filePath =>
                        {
                            await Functions.Origin.ParseAppDetailsAsync(new StreamReader(filePath).BaseStream, filePath, this);
                        });

                await Directory.EnumerateFiles(FullPath, "*.zip", SearchOption.TopDirectoryOnly).ParallelForEachAsync(async originCompressedArchive =>
                {
                    using (var archive = ZipFile.OpenRead(originCompressedArchive))
                    {
                        if (archive.Entries.Count > 0)
                        {
                            foreach (var archiveEntry in archive.Entries.Where(x => x.Name.Contains("installerdata.xml")))
                            {
                                await Functions.Origin.ParseAppDetailsAsync(archiveEntry.Open(), originCompressedArchive, this, true);
                            }
                        }
                    }
                });

                if (SLM.CurrentSelectedLibrary != null && SLM.CurrentSelectedLibrary == this)
                {
                    Functions.App.UpdateAppPanel(this);
                }

                IsUpdatingAppList = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.OriginUpdateAppListException)), new { FullPath, ex }));
                Logger.Fatal(ex);
            }
        }

        public override void ParseMenuItemActionAsync(string action)
        {
            switch (action.ToLowerInvariant())
            {
                case "disk":
                    if (Directory.Exists(FullPath))
                    {
                        Process.Start(FullPath);
                    }
                    break;

                case "remove":
                    RemoveLibraryAsync(false);
                    break;
            }
        }

        public override void RemoveLibraryAsync(bool withFiles)
        {
            if (withFiles)
            {
                throw new NotImplementedException();
            }
            else
            {
                List.Libraries.Remove(this);
            }
        }

        public JObject GetGameLocalData(string gameId)
        {
            try
            {
                var client = new WebClient();

                return JObject.Parse(client.DownloadString($"https://api1.origin.com/ecommerce2/public/{gameId}/en_US"));
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
                Debug.WriteLine(ex);
                return null;
            }
        }

        public override void UpdateJunks()
        {
            throw new NotImplementedException();
        }

        public override void UpdateDupes()
        {
            throw new NotImplementedException();
        }
    }
}