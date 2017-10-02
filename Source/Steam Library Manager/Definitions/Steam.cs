using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Steam_Library_Manager.Definitions
{
    public class Steam
    {
        public class Library : INotifyPropertyChanged
        {
            bool _Offline;
            FileSystemWatcher SteamFolderWD;
            FileSystemWatcher SLMFolderWD;

            public bool IsMain { get; set; }
            public bool IsBackup { get; set; }

            public DirectoryInfo SteamAppsFolder => new DirectoryInfo(Path.Combine(FullPath, "SteamApps"));

            public DirectoryInfo CommonFolder => new DirectoryInfo(Path.Combine(SteamAppsFolder.FullName, "common"));

            public DirectoryInfo DownloadFolder => new DirectoryInfo(Path.Combine(SteamAppsFolder.FullName, "downloading"));

            public DirectoryInfo WorkshopFolder => new DirectoryInfo(Path.Combine(SteamAppsFolder.FullName, "workshop"));

            public Framework.AsyncObservableCollection<FrameworkElement> ContextMenu => GenerateCMenuItems();

            public string FullPath { get; set; }
            public Framework.AsyncObservableCollection<AppInfo> Apps { get; set; } = new Framework.AsyncObservableCollection<AppInfo>();

            public bool IsOffline
            {
                get => _Offline;
                set
                {
                    _Offline = value;
                    OnPropertyChanged("IsOffline");
                }
            }

            public long FreeSpace
            {
                get => Functions.FileSystem.GetAvailableFreeSpace(FullPath);
            }

            public void UpdateDiskDetails()
            {
                OnPropertyChanged("FreeSpace");
                OnPropertyChanged("PrettyFreeSpace");
                OnPropertyChanged("FreeSpacePerc");
            }

            public string PrettyFreeSpace => Functions.FileSystem.FormatBytes(FreeSpace);

            public int FreeSpacePerc => 100 - ((int)Math.Round((double)(100 * FreeSpace) / Functions.FileSystem.GetTotalSize(FullPath)));

            public void UpdateAppList()
            {
                try
                {
                    if (!SteamAppsFolder.Exists)
                        SteamAppsFolder.Create();

                    if (Apps.Count > 0)
                        Apps.Clear();

                    // Foreach *.acf file found in library
                    Parallel.ForEach(SteamAppsFolder.EnumerateFiles("appmanifest_*.acf", SearchOption.TopDirectoryOnly), AcfFile =>
                    {
                        // Define a new value and call KeyValue
                        Framework.KeyValue KeyValReader = new Framework.KeyValue();

                        // Read the *.acf file as text
                        KeyValReader.ReadFileAsText(AcfFile.FullName);

                        // If key doesn't contains a child (value in acf file)
                        if (KeyValReader.Children.Count == 0)
                        {
                            List.Junks.Add(new List.JunkInfo
                            {
                                FSInfo = new FileInfo(AcfFile.FullName),
                                Size = AcfFile.Length,
                                Library = this
                            });

                            return;
                        }

                        Functions.App.AddNewApp(Convert.ToInt32(KeyValReader["appID"].Value), KeyValReader["name"].Value ?? KeyValReader["UserConfig"]["name"].Value, KeyValReader["installdir"].Value, this, Convert.ToInt64(KeyValReader["SizeOnDisk"].Value), Convert.ToInt64(KeyValReader["LastUpdated"].Value), false);
                    });

                    // Do a loop for each *.zip file in library
                    Parallel.ForEach(Directory.EnumerateFiles(SteamAppsFolder.FullName, "*.zip", SearchOption.TopDirectoryOnly), ArchiveFile =>
                    {
                        Functions.App.ReadDetailsFromZip(ArchiveFile, this);
                    });

                    // If library is backup library
                    if (IsBackup)
                    {
                        foreach (string SkuFile in Directory.EnumerateFiles(FullPath, "*.sis", SearchOption.AllDirectories))
                        {
                            Framework.KeyValue KeyValReader = new Framework.KeyValue();

                            KeyValReader.ReadFileAsText(SkuFile);

                            string[] AppNames = System.Text.RegularExpressions.Regex.Split(KeyValReader["name"].Value, " and ");

                            int i = 0;
                            long AppSize = Functions.FileSystem.GetDirectorySize(new DirectoryInfo(SkuFile).Parent, true);
                            foreach (Framework.KeyValue App in KeyValReader["apps"].Children)
                            {
                                if (Apps.Count(x => x.AppID == Convert.ToInt32(App.Value)) > 0)
                                    continue;

                                Functions.App.AddNewApp(Convert.ToInt32(App.Value), AppNames[i], Path.GetDirectoryName(SkuFile), this, AppSize, ((DateTimeOffset)new FileInfo(SkuFile).LastWriteTime).ToUnixTimeSeconds(), false, true);

                                if (AppNames.Count() > 1)
                                    i++;
                            }
                        }
                    }

                    if (SLM.CurrentSelectedLibrary == this)
                        Functions.App.UpdateAppPanel(this);

                    SteamFolderWD = new FileSystemWatcher()
                    {
                        Path = SteamAppsFolder.FullName,
                        Filter = "appmanifest_*.acf",
                        EnableRaisingEvents = true,
                    };

                    SteamFolderWD.Created += FolderWD_Created;
                    SteamFolderWD.Changed += FolderWD_Changed;
                    SteamFolderWD.Deleted += FolderWD_Deleted;

                    if (IsBackup)
                    {
                        SLMFolderWD = new FileSystemWatcher()
                        {
                            Path = SteamAppsFolder.FullName,
                            Filter = "*.zip",
                            EnableRaisingEvents = true
                        };

                        SLMFolderWD.Created += SLMWD_Created;
                        SLMFolderWD.Deleted += SLMWD_Deleted;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    Functions.Logger.LogToFile(Functions.Logger.LogType.Library, ex.ToString());
                }
            }

            private void SLMWD_Created(object sender, FileSystemEventArgs e)
            {
                try
                {
                    Functions.App.ReadDetailsFromZip(e.FullPath, this);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    Functions.Logger.LogToFile(Functions.Logger.LogType.Library, ex.ToString());
                }
            }

            private void SLMWD_Deleted(object sender, FileSystemEventArgs e)
            {
                try
                {
                    if (Apps.Count(x => x.AppID.ToString() == e.Name.Replace(".zip", "") && x.IsCompressed) > 0)
                    {
                        Apps.Remove(Apps.First(x => x.AppID.ToString() == e.Name.Replace(".zip", "") && x.IsCompressed));

                        if (SLM.CurrentSelectedLibrary == this)
                            Functions.App.UpdateAppPanel(this);

                        UpdateLibraryVisual();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    Functions.Logger.LogToFile(Functions.Logger.LogType.Library, ex.ToString());
                }
            }

            private void FolderWD_Created(object sender, FileSystemEventArgs e)
            {
                try
                {
                    // Define a new value and call KeyValue
                    Framework.KeyValue KeyValReader = new Framework.KeyValue();

                    // Read the *.acf file as text
                    KeyValReader.ReadFileAsText(e.FullPath);

                    // If key doesn't contains a child (value in acf file)
                    if (KeyValReader.Children.Count == 0)
                    {
                        List.Junks.Add(new List.JunkInfo
                        {
                            FSInfo = new FileInfo(e.FullPath),
                            Library = this
                        });

                        return;
                    }

                    if (Apps.Count(x => x.AppID == Convert.ToInt32(KeyValReader["appID"].Value)) > 0)
                        return;

                    Functions.App.AddNewApp(Convert.ToInt32(KeyValReader["appID"].Value), KeyValReader["name"].Value ?? KeyValReader["UserConfig"]["name"].Value, KeyValReader["installdir"].Value, this, Convert.ToInt64(KeyValReader["SizeOnDisk"].Value), Convert.ToInt64(KeyValReader["LastUpdated"].Value), false);

                    if (SLM.CurrentSelectedLibrary == this)
                        Functions.App.UpdateAppPanel(this);

                    UpdateLibraryVisual();
                }
                catch (FormatException FormatEx)
                {
                    Debug.WriteLine(FormatEx);
                }
                catch (Exception Ex)
                {
                    MessageBox.Show(Ex.ToString());
                    Functions.Logger.LogToFile(Functions.Logger.LogType.Library, Ex.ToString());
                }
            }

            private void FolderWD_Changed(object sender, FileSystemEventArgs e)
            {
                try
                {
                    FolderWD_Deleted(sender, e);
                    FolderWD_Created(sender, e);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    Functions.Logger.LogToFile(Functions.Logger.LogType.Library, ex.ToString());
                }
            }

            private void FolderWD_Deleted(object sender, FileSystemEventArgs e)
            {
                try
                {
                    if (Apps.Count(x => x.AcfName == e.Name) > 0)
                    {
                        AppInfo RemovenApp = Apps.First(x => x.AcfName == e.Name);
                        Apps.Remove(RemovenApp);

                        if (SLM.CurrentSelectedLibrary == this)
                            Functions.App.UpdateAppPanel(this);

                        UpdateLibraryVisual();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    Functions.Logger.LogToFile(Functions.Logger.LogType.Library, ex.ToString());
                }
            }

            public Framework.AsyncObservableCollection<FrameworkElement> GenerateCMenuItems()
            {
                Framework.AsyncObservableCollection<FrameworkElement> CMenu = new Framework.AsyncObservableCollection<FrameworkElement>();
                try
                {
                    foreach (ContextMenuItem CMenuItem in List.LibraryCMenuItems.Where(x => x.IsActive))
                    {
                        if (IsBackup && !CMenuItem.ShowToSLMBackup)
                            continue;
                        else if (!IsBackup && !CMenuItem.ShowToNormal)
                            continue;
                        else if (IsOffline && !CMenuItem.ShowToOffline)
                            continue;

                        if (CMenuItem.IsSeparator)
                            CMenu.Add(new Separator());
                        else
                        {
                            MenuItem SLMItem = new MenuItem()
                            {
                                Tag = this,
                                Header = string.Format(CMenuItem.Header, FullPath, PrettyFreeSpace)
                            };
                            SLMItem.Tag = CMenuItem.Action;
                            SLMItem.Icon = Functions.FAwesome.GetAwesomeIcon(CMenuItem.Icon, CMenuItem.IconColor);
                            SLMItem.HorizontalContentAlignment = HorizontalAlignment.Left;
                            SLMItem.VerticalContentAlignment = VerticalAlignment.Center;

                            CMenu.Add(SLMItem);
                        }
                    }

                    return CMenu;
                }
                catch (FormatException ex)
                {
                    MessageBox.Show($"An error happened while parsing context menu, most likely happened duo typo on color name.\n\n{ex}");

                    Functions.Logger.LogToFile(Functions.Logger.LogType.Library, ex.ToString());
                    return CMenu;
                }
            }

            public void ParseMenuItemAction(string Action)
            {
                switch (Action.ToLowerInvariant())
                {
                    // Opens game installation path in explorer
                    case "disk":
                        if (SteamAppsFolder.Exists)
                            Process.Start(SteamAppsFolder.FullName);
                        break;
                    case "deletelibrary":

                        MessageBoxResult MoveAppsBeforeDeletion = MessageBox.Show("Move apps in Library before deleting the library?", "Move apps first?", MessageBoxButton.YesNoCancel);

                        if (MoveAppsBeforeDeletion == MessageBoxResult.Yes)
                            //new Forms.moveLibrary(Library).Show();
                            MessageBox.Show("Function not implemented, process cancelled");
                        else if (MoveAppsBeforeDeletion == MessageBoxResult.No)
                            RemoveLibraryAsync(true);

                        break;
                    case "deletelibraryslm":

                        foreach (AppInfo App in Apps.ToList())
                        {
                            if (!App.DeleteFiles())
                            {
                                MessageBox.Show(string.Format("An unknown error happened while removing app files. {0}", FullPath));

                                return;
                            }
                        }

                        UpdateLibraryVisual();

                        MessageBox.Show(string.Format("All app files in library ({0}) successfully removed.", FullPath));
                        break;
                    // Removes a backup library from list
                    case "removefromlist":
                        if (IsBackup)
                        {
                            // Remove the library from our list
                            List.SteamLibraries.Remove(this);

                            if (SLM.CurrentSelectedLibrary == this)
                                Main.FormAccessor.AppPanel.ItemsSource = null;
                        }
                        break;
                }
            }

            public void UpdateLibraryVisual()
            {
                try
                {
                    Parallel.ForEach(List.SteamLibraries.Where(x => x.SteamAppsFolder.Root.FullName.ToLowerInvariant() == SteamAppsFolder.Root.FullName.ToLowerInvariant()), LibraryToUpdate =>
                    {
                        LibraryToUpdate.UpdateDiskDetails();
                    });
                }
                catch (Exception ex)
                {
                    Functions.Logger.LogToFile(Functions.Logger.LogType.Library, ex.ToString());
                }
            }

            public async void UpdateLibraryPathAsync(string NewLibraryPath)
            {
                try
                {
                    await Functions.Steam.CloseSteamAsync();

                    // Make a KeyValue reader
                    Framework.KeyValue Key = new Framework.KeyValue();

                    // Read vdf file
                    Key.ReadFileAsText(Global.Steam.vdfFilePath);

                    // Change old library path with new one
                    Key["Software"]["Valve"]["Steam"].Children.Find(key => key.Value.Contains(FullPath)).Value = NewLibraryPath;

                    // Update config.vdf file with changes
                    Key.SaveToFile(Global.Steam.vdfFilePath, false);

                    // Since this file started to interrupt us? 
                    // No need to bother with it since config.vdf is the real deal, just remove it and Steam client will handle with some magic.
                    if (File.Exists(Path.Combine(Properties.Settings.Default.steamInstallationPath, "steamapps", "libraryfolders.vdf")))
                        File.Delete(Path.Combine(Properties.Settings.Default.steamInstallationPath, "steamapps", "libraryfolders.vdf"));

                    Functions.Steam.RestartSteamAsync();
                }
                catch (Exception ex)
                {
                    Functions.Logger.LogToFile(Functions.Logger.LogType.Library, ex.ToString());
                }
            }

            public void DeleteFiles()
            {
                try
                {
                    if (SteamAppsFolder.Exists)
                        SteamAppsFolder.Delete(true);

                    if (WorkshopFolder.Exists)
                        WorkshopFolder.Delete(true);

                    if (DownloadFolder.Exists)
                        DownloadFolder.Delete(true);
                }
                catch (Exception Ex)
                {
                    Functions.Logger.LogToFile(Functions.Logger.LogType.SLM, Ex.ToString());
                    MessageBox.Show(Ex.ToString());
                }
            }

            public async void RemoveLibraryAsync(bool deleteFiles)
            {
                try
                {
                    if (deleteFiles)
                        DeleteFiles();

                    List.SteamLibraries.Remove(this);

                    if (IsBackup)
                    {
                        Functions.SLM.Settings.SaveSettings();
                    }
                    else
                    {
                        await Functions.Steam.CloseSteamAsync();

                        // Make a KeyValue reader
                        Framework.KeyValue KeyValReader = new Framework.KeyValue();

                        // Read vdf file
                        KeyValReader.ReadFileAsText(Global.Steam.vdfFilePath);

                        // Remove old library
                        KeyValReader["Software"]["Valve"]["Steam"].Children.RemoveAll(x => x.Value == FullPath);

                        int i = 1;
                        foreach (Framework.KeyValue Key in KeyValReader["Software"]["Valve"]["Steam"].Children.FindAll(x => x.Name.Contains("BaseInstallFolder")))
                        {
                            Key.Name = $"BaseInstallFolder_{i}";
                            i++;
                        }

                        // Update libraryFolders.vdf file with changes
                        KeyValReader.SaveToFile(Global.Steam.vdfFilePath, false);

                        // Since this file started to interrupt us? 
                        // No need to bother with it since config.vdf is the real deal, just remove it and Steam client will handle with some magic.
                        if (File.Exists(Path.Combine(Properties.Settings.Default.steamInstallationPath, "steamapps", "libraryfolders.vdf")))
                            File.Delete(Path.Combine(Properties.Settings.Default.steamInstallationPath, "steamapps", "libraryfolders.vdf"));

                        Functions.Steam.RestartSteamAsync();
                    }
                }
                catch (Exception ex)
                {
                    Functions.Logger.LogToFile(Functions.Logger.LogType.Library, ex.ToString());
                    MessageBox.Show(ex.ToString());
                }
            }

            public void UpdateJunks()
            {
                try
                {
                    if (CommonFolder.Exists)
                    {
                        foreach (DirectoryInfo DirInfo in CommonFolder.GetDirectories().Where(
                            x => Apps.Count(y => y.InstallationPath.Name.ToLowerInvariant() == x.Name.ToLowerInvariant()) == 0
                            && x.Name != "241100" // Steam controller configs
                            ).OrderByDescending(x => Functions.FileSystem.GetDirectorySize(x, true)))
                        {
                            List.JunkInfo Junk = new List.JunkInfo
                            {
                                FSInfo = DirInfo,
                                Size = Functions.FileSystem.GetDirectorySize(DirInfo, true),
                                Library = this
                            };


                            if (List.Junks.Count(x => x.FSInfo.FullName == Junk.FSInfo.FullName) == 0)
                                List.Junks.Add(Junk);
                        }
                    }

                    if (WorkshopFolder.Exists)
                    {
                        foreach (FileInfo FileDetails in WorkshopFolder.EnumerateFiles("*.acf", SearchOption.TopDirectoryOnly).Where(
                            x => Apps.Count(y => x.Name == y.WorkShopAcfName) == 0
                            && x.Name.ToLowerInvariant() != "appworkshop_241100.acf" // Steam Controller Configs
                            ))
                        {
                            List.JunkInfo Junk = new List.JunkInfo
                            {
                                FSInfo = FileDetails,
                                Size = FileDetails.Length,
                                Library = this
                            };


                            if (List.Junks.Count(x => x.FSInfo.FullName == Junk.FSInfo.FullName) == 0)
                                List.Junks.Add(Junk);
                        }

                        if (Directory.Exists(Path.Combine(WorkshopFolder.FullName, "content")))
                        {
                            foreach (DirectoryInfo DirInfo in new DirectoryInfo(Path.Combine(WorkshopFolder.FullName, "content")).GetDirectories().Where(
                                x => Apps.Count(y => y.AppID.ToString() == x.Name) == 0
                                && x.Name != "241100" // Steam controller configs
                                ).OrderByDescending(x => Functions.FileSystem.GetDirectorySize(x, true)))
                            {
                                List.JunkInfo Junk = new List.JunkInfo
                                {
                                    FSInfo = DirInfo,
                                    Size = Functions.FileSystem.GetDirectorySize(DirInfo, true),
                                    Library = this
                                };


                                if (List.Junks.Count(x => x.FSInfo.FullName == Junk.FSInfo.FullName) == 0)
                                    List.Junks.Add(Junk);
                            }
                        }

                        if (Directory.Exists(Path.Combine(WorkshopFolder.FullName, "downloads")))
                        {
                            foreach (FileInfo FileDetails in new DirectoryInfo(Path.Combine(WorkshopFolder.FullName, "downloads")).EnumerateFiles("*.patch", SearchOption.TopDirectoryOnly).Where(
                                x => Apps.Count(y => x.Name.Contains($"state_{y.AppID}_")) == 0 // Steam Controller Configs
                                ))
                            {
                                List.JunkInfo Junk = new List.JunkInfo
                                {
                                    FSInfo = FileDetails,
                                    Size = FileDetails.Length,
                                    Library = this
                                };


                                if (List.Junks.Count(x => x.FSInfo.FullName == Junk.FSInfo.FullName) == 0)
                                    List.Junks.Add(Junk);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Functions.Logger.LogToFile(Functions.Logger.LogType.Library, ex.ToString());
                    MessageBox.Show(ex.ToString());
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public class AppInfo
        {
            public int AppID { get; set; }
            public string AppName { get; set; }
            public DirectoryInfo InstallationPath;
            public long SizeOnDisk { get; set; }
            public bool IsCompressed { get; set; }
            public bool SteamBackup { get; set; }
            public Library Library { get; set; }
            public DateTime LastUpdated { get; set; }

            public string GameHeaderImage
            {
                get => $"http://cdn.akamai.steamstatic.com/steam/apps/{AppID}/header.jpg";
            }

            public string PrettyGameSize
            {
                get => Functions.FileSystem.FormatBytes(SizeOnDisk);
            }

            public DirectoryInfo CommonFolder
            {
                get => new DirectoryInfo(Path.Combine(Library.CommonFolder.FullName, InstallationPath.Name));
            }

            public DirectoryInfo DownloadFolder
            {
                get => new DirectoryInfo(Path.Combine(Library.DownloadFolder.FullName, InstallationPath.Name));
            }

            public DirectoryInfo WorkShopPath
            {
                get => new DirectoryInfo(Path.Combine(Library.WorkshopFolder.FullName, "content", AppID.ToString()));
            }

            public FileInfo CompressedArchiveName
            {
                get => new FileInfo(Path.Combine(Library.SteamAppsFolder.FullName, AppID + ".zip"));
            }

            public FileInfo FullAcfPath
            {
                get => new FileInfo(Path.Combine(Library.SteamAppsFolder.FullName, AcfName));
            }

            public FileInfo WorkShopAcfPath
            {
                get => new FileInfo(Path.Combine(Library.WorkshopFolder.FullName, WorkShopAcfName));
            }

            public string AcfName
            {
                get => $"appmanifest_{AppID}.acf";
            }

            public string WorkShopAcfName
            {
                get => $"appworkshop_{AppID}.acf";
            }

            public Framework.AsyncObservableCollection<FrameworkElement> ContextMenuItems
            {
                get => GenerateRightClickMenuItems();
            }

            public Framework.AsyncObservableCollection<FrameworkElement> GenerateRightClickMenuItems()
            {
                Framework.AsyncObservableCollection<FrameworkElement> rightClickMenu = new Framework.AsyncObservableCollection<FrameworkElement>();
                try
                {
                    foreach (ContextMenuItem cItem in List.AppCMenuItems.Where(x => x.IsActive))
                    {
                        if (SteamBackup && !cItem.ShowToSteamBackup)
                            continue;
                        else if (Library.IsBackup && !cItem.ShowToSLMBackup)
                            continue;
                        else if (IsCompressed && !cItem.ShowToCompressed)
                            continue;
                        else if (!cItem.ShowToNormal)
                            continue;

                        if (cItem.IsSeparator)
                            rightClickMenu.Add(new Separator());
                        else
                        {
                            MenuItem slmItem = new MenuItem()
                            {
                                Tag = this,
                                Header = string.Format(cItem.Header, AppName, AppID, Functions.FileSystem.FormatBytes(SizeOnDisk))
                            };
                            slmItem.Tag = cItem.Action;
                            slmItem.Icon = Functions.FAwesome.GetAwesomeIcon(cItem.Icon, cItem.IconColor);
                            slmItem.HorizontalContentAlignment = HorizontalAlignment.Left;
                            slmItem.VerticalContentAlignment = VerticalAlignment.Center;

                            rightClickMenu.Add(slmItem);
                        }
                    }

                    return rightClickMenu;
                }
                catch (FormatException ex)
                {
                    MessageBox.Show($"An error happened while parsing context menu, most likely happened duo typo on color name.\n\n{ex}");
                    Functions.Logger.LogToFile(Functions.Logger.LogType.SLM, $"[{AppName}][{AppID}][{AcfName}] {ex}");

                    return rightClickMenu;
                }
            }

            public void ParseMenuItemAction(string Action)
            {
                switch (Action.ToLowerInvariant())
                {
                    default:
                        if (string.IsNullOrEmpty(SLM.UserSteamID64))
                            return;

                        System.Diagnostics.Process.Start(string.Format(Action, AppID, SLM.UserSteamID64));
                        break;
                    case "disk":
                        if (CommonFolder.Exists)
                            System.Diagnostics.Process.Start(CommonFolder.FullName);
                        break;
                    case "acffile":
                        System.Diagnostics.Process.Start(FullAcfPath.FullName);
                        break;
                    case "deleteappfiles":

                        DeleteFiles();
                        break;
                }
            }

            public List<FileSystemInfo> GetFileList(bool includeDownloads = true, bool includeWorkshop = true)
            {
                List<FileSystemInfo> FileList = new List<FileSystemInfo>();

                if (IsCompressed)
                {
                    FileList.Add(CompressedArchiveName);
                }
                else
                {
                    if (CommonFolder.Exists)
                    {
                        FileList.AddRange(GetCommonFiles());
                    }

                    if (includeDownloads && DownloadFolder.Exists)
                    {
                        FileList.AddRange(GetDownloadFiles());
                        FileList.AddRange(GetPatchFiles());
                    }

                    if (includeWorkshop && WorkShopPath.Exists)
                    {
                        FileList.AddRange(GetWorkshopFiles());
                    }

                    if (FullAcfPath.Exists)
                    {
                        FileList.Add(FullAcfPath);
                    }

                    if (WorkShopAcfPath.Exists)
                    {
                        FileList.Add(WorkShopAcfPath);
                    }
                }

                return FileList;
            }

            public List<FileSystemInfo> GetCommonFiles()
            {
                return CommonFolder.EnumerateFileSystemInfos("*", SearchOption.AllDirectories).Where(x => x is FileInfo).ToList();
            }

            public List<FileSystemInfo> GetDownloadFiles()
            {
                return DownloadFolder.EnumerateFileSystemInfos("*", SearchOption.AllDirectories).Where(x => x is FileInfo).ToList();
            }

            public List<FileSystemInfo> GetPatchFiles()
            {
                return Library.DownloadFolder.EnumerateFileSystemInfos($"*{AppID}*.patch", SearchOption.TopDirectoryOnly).Where(x => x is FileInfo).ToList();
            }

            public List<FileSystemInfo> GetWorkshopFiles()
            {
                return WorkShopPath.EnumerateFileSystemInfos("*", SearchOption.AllDirectories).Where(x => x is FileInfo).ToList();
            }

            public void CopyGameFiles(List.TaskList CurrentTask, CancellationToken cancellationToken)
            {
                LogToTM($"[{AppName}] Populating file list, please wait");
                Functions.Logger.LogToFile(Functions.Logger.LogType.App, "Populating file list", this);

                ConcurrentBag<string> CopiedFiles = new ConcurrentBag<string>();
                ConcurrentBag<string> CreatedDirectories = new ConcurrentBag<string>();
                List<FileSystemInfo> GameFiles = GetFileList();
                CurrentTask.TotalFileCount = GameFiles.Count;

                try
                {
                    long TotalFileSize = 0;
                    ParallelOptions parallelOptions = new ParallelOptions()
                    {
                        CancellationToken = cancellationToken
                    };

                    Parallel.ForEach(GameFiles, parallelOptions, file =>
                    {
                        Interlocked.Add(ref TotalFileSize, (file as FileInfo).Length);
                    });

                    CurrentTask.TotalFileSize = TotalFileSize;
                    CurrentTask.ElapsedTime.Start();

                    LogToTM($"[{AppName}] File list populated, total files to move: {GameFiles.Count} - total size to move: {Functions.FileSystem.FormatBytes(TotalFileSize)}");
                    Functions.Logger.LogToFile(Functions.Logger.LogType.App, $"File list populated, total files to move: {GameFiles.Count} - total size to move: {Functions.FileSystem.FormatBytes(TotalFileSize)}", this);

                    // If the game is not compressed and user would like to compress it
                    if (!IsCompressed && CurrentTask.Compress)
                    {
                        FileInfo compressedArchive = new FileInfo(CompressedArchiveName.FullName.Replace(Library.SteamAppsFolder.FullName, CurrentTask.TargetLibrary.SteamAppsFolder.FullName));

                        if (compressedArchive.Exists)
                            compressedArchive.Delete();

                        using (ZipArchive compressed = ZipFile.Open(compressedArchive.FullName, ZipArchiveMode.Create))
                        {
                            CopiedFiles.Add(compressedArchive.FullName);

                            foreach (FileSystemInfo currentFile in GameFiles)
                            {
                                string newFileName = currentFile.FullName.Substring(Library.SteamAppsFolder.FullName.Length + 1);

                                compressed.CreateEntryFromFile(currentFile.FullName, newFileName, CompressionLevel.Optimal);

                                //CopiedFiles.Add(newFileName);
                                CurrentTask.MovenFileSize += (currentFile as FileInfo).Length;

                                if (CurrentTask.ReportFileMovement)
                                {
                                    LogToTM($"[{AppName}][{CopiedFiles.Count}/{CurrentTask.TotalFileCount}] Moven file: {newFileName}");
                                }

                                Functions.Logger.LogToFile(Functions.Logger.LogType.App, $"[{CopiedFiles.Count}/{CurrentTask.TotalFileCount}] Moven file: {newFileName}", this);

                                if (cancellationToken.IsCancellationRequested)
                                    throw new OperationCanceledException(cancellationToken);
                            }
                        }
                    }
                    // If the game is compressed and user would like to decompress it
                    else if (IsCompressed && !CurrentTask.Compress)
                    {
                        foreach (ZipArchiveEntry currentFile in ZipFile.OpenRead(CompressedArchiveName.FullName).Entries)
                        {
                            FileInfo newFile = new FileInfo(Path.Combine(CurrentTask.TargetLibrary.SteamAppsFolder.FullName, currentFile.FullName));

                            if (!newFile.Directory.Exists)
                            {
                                newFile.Directory.Create();
                                CreatedDirectories.Add(newFile.Directory.FullName);
                            }

                            currentFile.ExtractToFile(newFile.FullName, true);

                            CopiedFiles.Add(newFile.FullName);
                            CurrentTask.MovenFileSize += currentFile.Length;

                            if (CurrentTask.ReportFileMovement)
                            {
                                LogToTM($"[{AppName}][{CopiedFiles.Count}/{CurrentTask.TotalFileCount}] Moven file: {newFile.FullName}");
                            }

                            Functions.Logger.LogToFile(Functions.Logger.LogType.App, $"[{CopiedFiles.Count}/{CurrentTask.TotalFileCount}] Moven file: {newFile.FullName}", this);

                            if (cancellationToken.IsCancellationRequested)
                                throw new OperationCanceledException(cancellationToken);
                        }
                    }
                    // Everything else
                    else
                    {
                        parallelOptions.MaxDegreeOfParallelism = 1;

                        Parallel.ForEach(GameFiles.Where(x => (x as FileInfo).Length > Properties.Settings.Default.ParallelAfterSize * 1000000).OrderByDescending(x => (x as FileInfo).Length), parallelOptions, currentFile =>
                        {
                            FileInfo newFile = new FileInfo(currentFile.FullName.Replace(Library.SteamAppsFolder.FullName, CurrentTask.TargetLibrary.SteamAppsFolder.FullName));

                            if (!newFile.Exists || (newFile.Length != (currentFile as FileInfo).Length || newFile.LastWriteTime != (currentFile as FileInfo).LastWriteTime))
                            {
                                if (!newFile.Directory.Exists)
                                {
                                    newFile.Directory.Create();
                                    CreatedDirectories.Add(newFile.Directory.FullName);
                                }

                                (currentFile as FileInfo).CopyTo(newFile.FullName, true);
                            }

                            CopiedFiles.Add(newFile.FullName);
                            CurrentTask.MovenFileSize += (currentFile as FileInfo).Length;

                            if (CurrentTask.ReportFileMovement)
                            {
                                LogToTM($"[{AppName}][{CopiedFiles.Count}/{CurrentTask.TotalFileCount}] Moven file: {newFile.FullName}");
                            }

                            Functions.Logger.LogToFile(Functions.Logger.LogType.App, $"[{CopiedFiles.Count}/{CurrentTask.TotalFileCount}] Moven file: {newFile.FullName}", this);
                        });

                        parallelOptions.MaxDegreeOfParallelism = -1;

                        Parallel.ForEach(GameFiles.Where(x => (x as FileInfo).Length <= Properties.Settings.Default.ParallelAfterSize * 1000000).OrderByDescending(x => (x as FileInfo).Length), parallelOptions, currentFile =>
                        {
                            FileInfo newFile = new FileInfo(currentFile.FullName.Replace(Library.SteamAppsFolder.FullName, CurrentTask.TargetLibrary.SteamAppsFolder.FullName));

                            if (!newFile.Exists || (newFile.Length != (currentFile as FileInfo).Length || newFile.LastWriteTime != (currentFile as FileInfo).LastWriteTime))
                            {
                                if (!newFile.Directory.Exists)
                                {
                                    newFile.Directory.Create();
                                    CreatedDirectories.Add(newFile.Directory.FullName);
                                }

                                (currentFile as FileInfo).CopyTo(newFile.FullName, true);
                            }

                            CopiedFiles.Add(newFile.FullName);
                            CurrentTask.MovenFileSize += (currentFile as FileInfo).Length;

                            if (CurrentTask.ReportFileMovement)
                            {
                                LogToTM($"[{AppName}][{CopiedFiles.Count}/{CurrentTask.TotalFileCount}] Moven file: {newFile.FullName}");
                            }

                            Functions.Logger.LogToFile(Functions.Logger.LogType.App, $"[{CopiedFiles.Count}/{CurrentTask.TotalFileCount}] Moven file: {newFile.FullName}", this);
                        });

                    }

                    CurrentTask.ElapsedTime.Stop();

                    LogToTM($"[{AppName}] Time elapsed: {CurrentTask.ElapsedTime.Elapsed} - Average speed: {Math.Round(((TotalFileSize / 1024f) / 1024f) / CurrentTask.ElapsedTime.Elapsed.TotalSeconds, 3)} MB/sec - Average file size: {Functions.FileSystem.FormatBytes(TotalFileSize / (long)CurrentTask.TotalFileCount)}");
                    Functions.Logger.LogToFile(Functions.Logger.LogType.App, $"Movement completed in {CurrentTask.ElapsedTime.Elapsed} with Average Speed of {Math.Round(((TotalFileSize / 1024f) / 1024f) / CurrentTask.ElapsedTime.Elapsed.TotalSeconds, 3)} MB/sec - Average file size: {Functions.FileSystem.FormatBytes(TotalFileSize / (long)CurrentTask.TotalFileCount)}", this);
                }
                catch (OperationCanceledException)
                {
                    Framework.TaskManager.Stop();
                    CurrentTask.Moving = false;
                    CurrentTask.Completed = true;

                    MessageBoxResult removeMovenFiles = MessageBox.Show($"[{AppName}] Game movement cancelled. Would you like to remove files that already moven?", "Remove moven files?", MessageBoxButton.YesNo);

                    if (removeMovenFiles == MessageBoxResult.Yes)
                        Functions.FileSystem.RemoveGivenFiles(CopiedFiles, CreatedDirectories);

                    LogToTM($"[{AppName}] Operation cancelled by user. Time Elapsed: {CurrentTask.ElapsedTime.Elapsed}");
                    Functions.Logger.LogToFile(Functions.Logger.LogType.App, $"Operation cancelled by user. Time Elapsed: {CurrentTask.ElapsedTime.Elapsed}", this);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    MessageBoxResult removeMovenFiles = MessageBox.Show($"[{AppName}] An error happened while moving game files. Would you like to remove files that already moven?", "Remove moven files?", MessageBoxButton.YesNo);

                    if (removeMovenFiles == MessageBoxResult.Yes)
                        Functions.FileSystem.RemoveGivenFiles(CopiedFiles, CreatedDirectories);

                    Main.FormAccessor.TaskManager_Logs.Add($"[{AppName}] An error happened while moving game files. Time Elapsed: {CurrentTask.ElapsedTime.Elapsed}");
                    Functions.Logger.LogToFile(Functions.Logger.LogType.SLM, $"[{AppName}][{AppID}][{AcfName}] {ex}");
                }
            }

            public void LogToTM(string TextToLog)
            {
                try
                {
                    Main.FormAccessor.TaskManager_Logs.Add($"[{DateTime.Now}] {TextToLog}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    Functions.Logger.LogToFile(Functions.Logger.LogType.SLM, $"[{AppName}][{AppID}][{AcfName}] {ex}");
                }
            }

            public bool DeleteFiles()
            {
                try
                {
                    if (IsCompressed)
                    {
                        CompressedArchiveName.Delete();
                    }
                    else if (SteamBackup)
                    {
                        if (InstallationPath.Exists)
                            InstallationPath.Delete(true);
                    }
                    else
                    {
                        List<FileSystemInfo> gameFiles = GetFileList();

                        Parallel.ForEach(gameFiles, currentFile =>
                        {
                            if (currentFile.Exists)
                                currentFile.Delete();
                        }
                        );

                        // common folder, if exists
                        if (CommonFolder.Exists)
                            CommonFolder.Delete(true);

                        // downloading folder, if exists
                        if (DownloadFolder.Exists)
                            DownloadFolder.Delete(true);

                        // workshop folder, if exists
                        if (WorkShopPath.Exists)
                            WorkShopPath.Delete(true);

                        // game .acf file
                        if (FullAcfPath.Exists)
                            FullAcfPath.Delete();

                        // workshop .acf file
                        if (WorkShopAcfPath.Exists)
                            WorkShopAcfPath.Delete();
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    Functions.Logger.LogToFile(Functions.Logger.LogType.SLM, $"[{AppName}][{AppID}][{AcfName}] {ex}");

                    return false;
                }
            }
        }
    }
}
