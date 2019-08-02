using System;
using System.Collections.Async;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;

namespace Steam_Library_Manager.Definitions
{
    public class UplayLibrary : Library
    {
        public UplayLibrary(string fullPath, bool isMain = false)
        {
            FullPath = fullPath;
            IsMain = isMain;
            Type = Enums.LibraryType.Uplay;
            DirectoryInfo = new DirectoryInfo(fullPath);
        }

        public override async void UpdateAppListAsync()
        {
            try
            {
                if (IsUpdatingAppList)
                    return;

                IsUpdatingAppList = true;

                Apps.Clear();

                if (!Directory.Exists(FullPath)) return;

                await Directory.EnumerateFiles(FullPath, "uplay_install.state", SearchOption.AllDirectories)
                    .ParallelForEachAsync(
                        async filePath =>
                        {
                            await Functions.Uplay.ParseAppDetailsAsync(new StreamReader(filePath).BaseStream, filePath, this);
                        });

                await Directory.EnumerateFiles(FullPath, "*.zip", SearchOption.TopDirectoryOnly).ParallelForEachAsync(async originCompressedArchive =>
                {
                    using (var archive = ZipFile.OpenRead(originCompressedArchive))
                    {
                        if (archive.Entries.Count > 0)
                        {
                            foreach (var archiveEntry in archive.Entries.Where(x => x.Name.Contains("uplay_install.state")))
                            {
                                await Functions.Uplay.ParseAppDetailsAsync(archiveEntry.Open(), originCompressedArchive, this, true);
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
                MessageBox.Show($"An error happened while updating game list for Uplay library: {FullPath}\n{ex}");
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