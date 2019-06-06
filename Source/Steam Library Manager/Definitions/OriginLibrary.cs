using Newtonsoft.Json.Linq;
using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Xml.Linq;

namespace Steam_Library_Manager.Definitions
{
    public class OriginLibrary : LibraryBase
    {
        public OriginLibrary(string fullPath, Library library, bool isMain = false)
        {
            FullPath = fullPath;
            IsMain = isMain;
            Library = library;
        }

        public override async void UpdateAppListAsync()
        {
            try
            {
                Apps.Clear();

                if (!Directory.Exists(FullPath)) return;

                var appIds = new List<KeyValuePair<string, string>>();

                if (Directory.Exists(Directories.Origin.LocalContentDirectoy))
                {
                    //foreach (var originApp in Directory.EnumerateFiles(Directories.Origin.LocalContentDirectoy, "*.mfst", SearchOption.AllDirectories))
                    await Directory.EnumerateFiles(Directories.Origin.LocalContentDirectoy, "*.mfst",
                        SearchOption.AllDirectories).ParallelForEachAsync(
                        async originApp =>
                        {
                            var appId = Path.GetFileNameWithoutExtension(originApp);

                            if (!appId.StartsWith("Origin"))
                            {
                                // Get game id by fixing file via adding : before integer part of the name
                                // for example OFB-EAST52017 converts to OFB-EAST:52017
                                var match = System.Text.RegularExpressions.Regex.Match(appId, @"^(.*?)(\d+)$");
                                if (!match.Success)
                                {
                                    return;
                                }

                                appId = match.Groups[1].Value + ":" + match.Groups[2].Value;
                            }

                            appIds.Add(new KeyValuePair<string, string>(new FileInfo(originApp).Directory.Name, appId));
                        });
                }

                await Directory.EnumerateFiles(FullPath, "installerdata.xml", SearchOption.AllDirectories)
                    .ParallelForEachAsync(
                        async originApp =>
                        {
                            if (new FileInfo(originApp).Directory.Parent.Parent.Name !=
                                new DirectoryInfo(FullPath).Name)
                                return;

                            var installerLog = Path.Combine(Directory.GetParent(originApp).FullName, "InstallLog.txt");
                            var installedLocale = "en_US";

                            if (File.Exists(installerLog))
                            {
                                foreach (var line in File.ReadAllLines(installerLog))
                                {
                                    if (!line.Contains("Install Locale:")) continue;

                                    installedLocale = line.Split(new string[] { "Install Locale:" },
                                        StringSplitOptions.None)[1];
                                    break;
                                }

                                installedLocale = installedLocale.Replace(" ", "");
                            }

                            var xml = XDocument.Load(originApp);
                            var manifestVersion = new Version((xml.Root.Name.LocalName == "game")
                                ? xml.Root.Attribute("manifestVersion").Value
                                : ((xml.Root.Name.LocalName == "DiPManifest")
                                    ? xml.Root.Attribute("version").Value
                                    : "1.0"));

                            OriginAppInfo originAppInfo = null;

                            if (manifestVersion == new Version("4.0"))
                            {
                                originAppInfo = new OriginAppInfo(Library,
                                    xml.Root.Element("gameTitles")?.Elements("gameTitle")
                                        ?.First(x => x.Attribute("locale").Value == "en_US")?.Value,
                                    Convert.ToInt32(xml.Root.Element("contentIDs")?.Elements()
                                        .FirstOrDefault(x => int.TryParse(x.Value, out int appId))?.Value),
                                    new FileInfo(originApp).Directory.Parent,
                                    new Version(xml.Root.Element("buildMetaData")?.Element("gameVersion")
                                        ?.Attribute("version")?.Value),
                                    xml.Root.Element("installMetaData")?.Element("locales")?.Value.Split(','),
                                    installedLocale,
                                    xml.Root.Element("touchup")?.Element("filePath")?.Value,
                                    xml.Root.Element("touchup")?.Element("parameters")?.Value,
                                    xml.Root.Element("touchup")?.Element("updateParameters")?.Value,
                                    xml.Root.Element("touchup")?.Element("repairParameters")?.Value);
                            }
                            else if (manifestVersion >= new Version("1.1") && manifestVersion <= new Version("3.0"))
                            {
                                var locales = new List<string>();
                                foreach (var locale in xml.Root.Element("metadata")?.Elements("localeInfo")
                                    ?.Attributes()?.Where(x => x.Name == "locale"))
                                {
                                    locales.Add(locale.Value);
                                }

                                originAppInfo = new OriginAppInfo(Library,
                                    xml.Root.Element("metadata")?.Elements("localeInfo")
                                        ?.First(x => x.Attribute("locale").Value == "en_US")?.Element("title").Value,
                                    Convert.ToInt32(xml.Root.Element("contentIDs")?.Element("contentID")?.Value
                                        .Replace("EAX", "")),
                                    new FileInfo(originApp).Directory.Parent,
                                    new Version(xml.Root.Attribute("gameVersion").Value),
                                    locales.ToArray(),
                                    installedLocale,
                                    xml.Root.Element("executable")?.Element("filePath")?.Value,
                                    xml.Root.Element("executable")?.Element("parameters")?.Value);
                            }
                            else
                            {
                                MessageBox.Show(Framework.StringFormat.Format(
                                    Functions.SLM.Translate(nameof(Properties.Resources.OriginUnknownManifestFile)),
                                    new { ManifestVersion = manifestVersion, OriginApp = originApp }));
                                return;
                            }

                            if (appIds.Count(x => x.Key == originAppInfo.InstallationDirectory.Name) > 0)
                            {
                                var appId = appIds.First(x => x.Key == originAppInfo.InstallationDirectory.Name);

                                var appLocalData = GetGameLocalData(appId.Value);

                                if (appLocalData != null)
                                {
                                    originAppInfo.GameHeaderImage = string.Concat(
                                        appLocalData["customAttributes"]["imageServer"],
                                        appLocalData["localizableAttributes"]["packArtLarge"]);
                                }
                            }

                            Apps.Add(originAppInfo);
                        });

                if (SLM.CurrentSelectedLibrary != null && SLM.CurrentSelectedLibrary == Library)
                {
                    Functions.App.UpdateAppPanel(Library);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
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
                List.Libraries.Remove(Library);
            }
        }

        private JObject GetGameLocalData(string gameId)
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
    }
}