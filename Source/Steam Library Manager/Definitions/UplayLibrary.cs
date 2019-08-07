using System;
using System.Collections.Async;
using System.Diagnostics;
using System.IO;
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

            AllowedAppTypes.Add(Enums.LibraryType.Uplay);
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

                await Directory.EnumerateDirectories(FullPath, "*", SearchOption.TopDirectoryOnly)
                    .ParallelForEachAsync(
                        async directoryPath =>
                        {
                            var dirInfo = new DirectoryInfo(directoryPath);
                            Functions.Uplay.ParseAppDetails(dirInfo.Name, dirInfo, this);
                        });

                await Directory.EnumerateFiles(FullPath, "*.zip", SearchOption.TopDirectoryOnly).ParallelForEachAsync(async originCompressedArchive =>
                {
                    Functions.Uplay.ParseAppDetails(originCompressedArchive.Replace(".zip", ""), new DirectoryInfo(originCompressedArchive), this, true);
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