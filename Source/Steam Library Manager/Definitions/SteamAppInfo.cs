using System.IO;
using DirectoryInfo = Alphaleonis.Win32.Filesystem.DirectoryInfo;
using FileInfo = Alphaleonis.Win32.Filesystem.FileInfo;
using Path = Alphaleonis.Win32.Filesystem.Path;

namespace Steam_Library_Manager.Definitions
{
    public class SteamAppInfo : App
    {
        public bool IsSteamBackup { get; set; }

        public SteamAppInfo(int appId, Library library, DirectoryInfo installationDirectory)
        {
            AppId = appId;
            Library = library;
            InstallationDirectory = installationDirectory;
            GameHeaderImage = $"http://cdn.akamai.steamstatic.com/steam/apps/{AppId}/header.jpg";

            CompressedArchivePath = new FileInfo(Path.Combine(Library.DirectoryList["SteamApps"].FullName, AppId + ".zip"));

            AdditionalDirectories.Add((new DirectoryInfo(Path.Combine(Library.DirectoryList["Download"].FullName, InstallationDirectory.Name)), "*", SearchOption.AllDirectories));
            AdditionalDirectories.Add((new DirectoryInfo(Path.Combine(Library.DirectoryList["Workshop"].FullName, "content", AppId.ToString())), "*", SearchOption.AllDirectories));
            AdditionalDirectories.Add((Library.DirectoryList["Download"], $"*{AppId}*.patch", SearchOption.TopDirectoryOnly));

            AdditionalFiles.Add(new FileInfo(Path.Combine(Library.DirectoryList["SteamApps"].FullName, $"appmanifest_{AppId}.acf")));
            AdditionalFiles.Add(new FileInfo(Path.Combine(Library.DirectoryList["Workshop"].FullName, $"appworkshop_{AppId}.acf")));
        }
    }
}