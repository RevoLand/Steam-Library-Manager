using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Directory = Alphaleonis.Win32.Filesystem.Directory;
using DirectoryInfo = Alphaleonis.Win32.Filesystem.DirectoryInfo;
using File = Alphaleonis.Win32.Filesystem.File;
using FileInfo = Alphaleonis.Win32.Filesystem.FileInfo;
using Path = Alphaleonis.Win32.Filesystem.Path;

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

        public override void UpdateAppList()
        {
            try
            {
                if (IsUpdatingAppList)
                    return;

                IsUpdatingAppList = true;

                Apps.Clear();

                if (!Directory.Exists(FullPath)) return;

                foreach (var directoryPath in Directory.EnumerateDirectories(FullPath, "*",
                    SearchOption.TopDirectoryOnly))
                {
                    var dirInfo = new DirectoryInfo(directoryPath);

                    if (!File.Exists(Path.Combine(dirInfo.FullName, "uplay_install.state")))
                    {
                        if (List.IgnoredJunkItems.Contains(dirInfo.FullName))
                        {
                            continue;
                        }

                        List.LcProgress.Report(new List.JunkInfo
                        {
                            FSInfo = dirInfo,
                            Size = Functions.FileSystem.FormatBytes(Functions.FileSystem.GetDirectorySize(dirInfo, true)),
                            Library = this,
                            Tag = Enums.JunkType.HeadlessFolder
                        });
                        continue;
                    }

                    Functions.Uplay.ParseAppDetails(dirInfo.Name, dirInfo, this);
                }

                foreach (var archivePath in Directory.EnumerateFiles(FullPath, "*.zip", SearchOption.TopDirectoryOnly))
                {
                    var fileInfo = new FileInfo(archivePath);
                    Functions.Uplay.ParseAppDetails(fileInfo.Name.Replace(".zip", ""), fileInfo.Directory, this, true);
                }

                if (SLM.CurrentSelectedLibrary != null && SLM.CurrentSelectedLibrary == this)
                {
                    Functions.App.UpdateAppPanel(this);
                }

                IsUpdatingAppList = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.Uplay_UpdateAppListError)), new { FullPath, ex }));
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