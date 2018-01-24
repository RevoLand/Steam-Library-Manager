using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace Steam_Library_Manager.Definitions
{
    public class OriginLibrary : INotifyPropertyChanged
    {
        public Library Library => List.Libraries.First(x => x.Origin == this);
        public bool IsMain { get; set; }
        public string FullPath { get; set; }
        public Framework.AsyncObservableCollection<OriginAppInfo> Apps { get; set; } = new Framework.AsyncObservableCollection<OriginAppInfo>();
        public Framework.AsyncObservableCollection<FrameworkElement> ContextMenu => GenerateCMenuItems();
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        //-----
        public OriginLibrary(string _FullPath, bool _IsMain = false)
        {
            FullPath = (!_FullPath.EndsWith(Path.DirectorySeparatorChar.ToString())) ? _FullPath.Insert(_FullPath.Count(), Path.DirectorySeparatorChar.ToString()) : _FullPath;
            IsMain = _IsMain;
        }

        public void UpdateAppList()
        {
            try
            {
                Apps.Clear();

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                if (Directory.Exists(FullPath))
                {
                    foreach(var OriginApp in Directory.EnumerateFiles(FullPath, "installerdata.xml", SearchOption.AllDirectories))
                    {
                        if (new FileInfo(OriginApp).Directory.Parent.Parent.Name != new DirectoryInfo(FullPath).Name)
                            continue;

                        Debug.WriteLine(OriginApp);

                        var xml = XDocument.Load(OriginApp);
                        Version ManifestVersion = new Version((xml.Root.Name.LocalName == "game") ? xml.Root.Attribute("manifestVersion").Value : ((xml.Root.Name.LocalName == "DiPManifest") ? xml.Root.Attribute("version").Value : "1.0"));

                        if (ManifestVersion == new Version("4.0"))
                        {
                            Apps.Add(new OriginAppInfo(_Library: Library, _AppName: xml.Root.Element("gameTitles")?.Elements("gameTitle")?.First(x => x.Attribute("locale").Value == "en_US")?.Value,
                                _AppID: Convert.ToInt32(xml.Root.Element("contentIDs")?.Element("contentID")?.Value), _InstallationDirectory: new FileInfo(OriginApp).Directory.Parent,
                                _AppVersion: new Version(xml.Root.Element("buildMetaData")?.Element("gameVersion")?.Attribute("version")?.Value),
                                _Locales: xml.Root.Element("installMetaData")?.Element("locales")?.Value.Split(','),
                                _TouchupFile: xml.Root.Element("touchup")?.Element("filePath")?.Value, _InstallationParameter: xml.Root.Element("touchup")?.Element("parameters")?.Value,
                                _UpdateParameter: xml.Root.Element("touchup")?.Element("updateParameters")?.Value, _RepairParameter: xml.Root.Element("touchup")?.Element("repairParameters")?.Value));
                        }
                        else if (ManifestVersion == new Version("2.1"))
                        {
                            List<string> _locales = new List<string>();
                            foreach (var _locale in xml.Root.Element("metadata")?.Elements("localeInfo")?.Attributes()?.Where(x => x.Name == "locale"))
                            {
                                _locales.Add(_locale.Value);
                            }

                            Apps.Add(new OriginAppInfo(_Library: Library, _AppName: xml.Root.Element("metadata")?.Elements("localeInfo")?.First(x => x.Attribute("locale").Value == "en_US")?.Element("title").Value,
                                _AppID: Convert.ToInt32(xml.Root.Element("contentIDs")?.Element("contentID")?.Value.Replace("EAX", "")),
                                _InstallationDirectory: new FileInfo(OriginApp).Directory.Parent,
                                _AppVersion: new Version(xml.Root.Attribute("gameVersion").Value),
                                _Locales: _locales.ToArray(),
                                _TouchupFile: xml.Root.Element("executable")?.Element("filePath")?.Value,
                                _InstallationParameter: xml.Root.Element("executable")?.Element("parameters")?.Value));
                        }
                        else
                        {
                            MessageBox.Show($"Unknown Manifest Version from Origin game: {ManifestVersion}");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Origin games directory is not exists.");
                }
                stopwatch.Stop();
                Debug.WriteLine(stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        //-----

        public Framework.AsyncObservableCollection<FrameworkElement> GenerateCMenuItems()
        {
            Framework.AsyncObservableCollection<FrameworkElement> CMenu = new Framework.AsyncObservableCollection<FrameworkElement>();
            try
            {
                foreach (ContextMenuItem CMenuItem in List.LibraryCMenuItems.Where(x => x.IsActive && x.LibraryType == Enums.LibraryType.Origin))
                {
                    if (!CMenuItem.ShowToNormal && IsMain)
                    {
                        continue;
                    }

                    if (CMenuItem.IsSeparator)
                    {
                        CMenu.Add(new Separator());
                    }
                    else
                    {
                        MenuItem SLMItem = new MenuItem()
                        {
                            Tag = CMenuItem.Action,
                            Header = string.Format(CMenuItem.Header, FullPath, Library.PrettyFreeSpace),
                            Icon = Functions.FAwesome.GetAwesomeIcon(CMenuItem.Icon, CMenuItem.IconColor),
                            HorizontalContentAlignment = HorizontalAlignment.Left,
                            VerticalContentAlignment = VerticalAlignment.Center
                        };

                        SLMItem.Click += Main.FormAccessor.LibraryCMenuItem_Click;

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
                case "disk":
                    if (Directory.Exists(FullPath))
                    {
                        Process.Start(FullPath);
                    }

                    break;
                case "remove":
                    List.Libraries.Remove(List.Libraries.First(x => x == Library));
                    break;
            }
        }

    }
}
