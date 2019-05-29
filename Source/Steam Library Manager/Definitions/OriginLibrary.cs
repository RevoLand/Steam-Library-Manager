using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace Steam_Library_Manager.Definitions
{
    public class OriginLibrary
    {
        public Library Library;
        public bool IsMain { get; set; }
        public string FullPath { get; set; }
        public ObservableCollection<OriginAppInfo> Apps { get; set; } = new ObservableCollection<OriginAppInfo>();
        public List<FrameworkElement> ContextMenu => GenerateCMenuItems();

        //-----
        public OriginLibrary(string _FullPath, Library library, bool _IsMain = false)
        {
            FullPath = (!_FullPath.EndsWith(Path.DirectorySeparatorChar.ToString())) ? _FullPath.Insert(_FullPath.Length, Path.DirectorySeparatorChar.ToString()) : _FullPath;
            IsMain = _IsMain;
            Library = library;
        }

        public void UpdateAppList()
        {
            try
            {
                Apps.Clear();

                if (Directory.Exists(FullPath))
                {
                    var AppIds = new List<KeyValuePair<string, string>>();

                    if (Directory.Exists(Directories.Origin.LocalContentDirectoy))
                    {
                        foreach (string OriginApp in Directory.EnumerateFiles(Directories.Origin.LocalContentDirectoy, "*.mfst", SearchOption.AllDirectories))
                        {
                            var appId = Path.GetFileNameWithoutExtension(OriginApp);

                            if (!appId.StartsWith("Origin"))
                            {
                                // Get game id by fixing file via adding : before integer part of the name
                                // for example OFB-EAST52017 converts to OFB-EAST:52017
                                var match = System.Text.RegularExpressions.Regex.Match(appId, @"^(.*?)(\d+)$");
                                if (!match.Success)
                                {
                                    continue;
                                }

                                appId = match.Groups[1].Value + ":" + match.Groups[2].Value;
                            }

                            AppIds.Add(new KeyValuePair<string, string>(new FileInfo(OriginApp).Directory.Name, appId));
                        }
                    }

                    foreach (var OriginApp in Directory.EnumerateFiles(FullPath, "installerdata.xml", SearchOption.AllDirectories))
                    {
                        if (new FileInfo(OriginApp).Directory.Parent.Parent.Name != new DirectoryInfo(FullPath).Name)
                            continue;

                        string installerLog = Path.Combine(Directory.GetParent(OriginApp).FullName, "InstallLog.txt");
                        string installedLocale = "en_US";

                        if (File.Exists(installerLog))
                        {
                            foreach (var line in File.ReadAllLines(installerLog))
                            {
                                if (line.Contains("Install Locale:"))
                                {
                                    installedLocale = line.Split(new string[] { "Install Locale:" }, StringSplitOptions.None)[1];
                                    break;
                                }
                            }

                            installedLocale = installedLocale.Replace(" ", "");
                        }

                        var xml = XDocument.Load(OriginApp);
                        Version ManifestVersion = new Version((xml.Root.Name.LocalName == "game") ? xml.Root.Attribute("manifestVersion").Value : ((xml.Root.Name.LocalName == "DiPManifest") ? xml.Root.Attribute("version").Value : "1.0"));

                        OriginAppInfo originApp = null;

                        if (ManifestVersion == new Version("4.0"))
                        {
                            originApp = new OriginAppInfo(_Library: Library, _AppName: xml.Root.Element("gameTitles")?.Elements("gameTitle")?.First(x => x.Attribute("locale").Value == "en_US")?.Value,
                                _AppID: Convert.ToInt32(xml.Root.Element("contentIDs")?.Elements().FirstOrDefault(x => int.TryParse(x.Value, out int appId))?.Value), _InstallationDirectory: new FileInfo(OriginApp).Directory.Parent,
                                _AppVersion: new Version(xml.Root.Element("buildMetaData")?.Element("gameVersion")?.Attribute("version")?.Value),
                                _Locales: xml.Root.Element("installMetaData")?.Element("locales")?.Value.Split(','),
                                _InstalledLocale: installedLocale,
                                _TouchupFile: xml.Root.Element("touchup")?.Element("filePath")?.Value, _InstallationParameter: xml.Root.Element("touchup")?.Element("parameters")?.Value,
                                _UpdateParameter: xml.Root.Element("touchup")?.Element("updateParameters")?.Value, _RepairParameter: xml.Root.Element("touchup")?.Element("repairParameters")?.Value);
                        }
                        else if (ManifestVersion >= new Version("1.1") && ManifestVersion <= new Version("3.0"))
                        {
                            List<string> _locales = new List<string>();
                            foreach (var _locale in xml.Root.Element("metadata")?.Elements("localeInfo")?.Attributes()?.Where(x => x.Name == "locale"))
                            {
                                _locales.Add(_locale.Value);
                            }

                            originApp = new OriginAppInfo(_Library: Library, _AppName: xml.Root.Element("metadata")?.Elements("localeInfo")?.First(x => x.Attribute("locale").Value == "en_US")?.Element("title").Value,
                                _AppID: Convert.ToInt32(xml.Root.Element("contentIDs")?.Element("contentID")?.Value.Replace("EAX", "")),
                                _InstallationDirectory: new FileInfo(OriginApp).Directory.Parent,
                                _AppVersion: new Version(xml.Root.Attribute("gameVersion").Value),
                                _Locales: _locales.ToArray(),
                                _InstalledLocale: installedLocale,
                                _TouchupFile: xml.Root.Element("executable")?.Element("filePath")?.Value,
                                _InstallationParameter: xml.Root.Element("executable")?.Element("parameters")?.Value);
                        }
                        else
                        {
                            MessageBox.Show(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.OriginUnknownManifestFile)), new { ManifestVersion, OriginApp }));
                            continue;
                        }

                        if (AppIds.Count(x => x.Key == originApp.InstallationDirectory.Name) > 0)
                        {
                            var appId = AppIds.First(x => x.Key == originApp.InstallationDirectory.Name);

                            JObject appLocalData = GetGameLocalData(appId.Value);

                            if (appLocalData != null)
                            {
                                originApp.GameHeaderImage = string.Concat(appLocalData["customAttributes"]["imageServer"], appLocalData["localizableAttributes"]["packArtLarge"]);
                            }
                        }

                        Apps.Add(originApp);
                    } // foreach

                    if (SLM.CurrentSelectedLibrary != null && SLM.CurrentSelectedLibrary == Library)
                    {
                        Functions.App.UpdateAppPanel(Library);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public static JObject GetGameLocalData(string gameId)
        {
            try
            {
                WebClient client = new WebClient();

                return JObject.Parse(client.DownloadString($"https://api1.origin.com/ecommerce2/public/{gameId}/en_US"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }

        //-----

        public List<FrameworkElement> GenerateCMenuItems()
        {
            var CMenu = new List<FrameworkElement>();
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
                            Header = Framework.StringFormat.Format(CMenuItem.Header, new { LibraryFullPath = Library.DirectoryInfo.FullName, FreeDiskSpace = Library.PrettyFreeSpace }),
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
                MessageBox.Show(Framework.StringFormat.Format(Functions.SLM.Translate(nameof(Properties.Resources.OriginAppInfo_FormatException)), new { ExceptionMessage = ex.Message }));
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
                    List.Libraries.Remove(Library);
                    break;
            }
        }
    }
}