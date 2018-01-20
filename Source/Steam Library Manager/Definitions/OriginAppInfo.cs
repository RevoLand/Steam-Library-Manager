using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Steam_Library_Manager.Definitions
{
    public class OriginAppInfo
    {
        public Library Library { get; set; }
        public string AppName { get; set; } // gameTitle
        public int AppID { get; set; } // contentID
        public string[] Locales { get; set; } // pt_BR,en_US,de_DE,es_ES,fr_FR,it_IT,es_MX,nl_NL,pl_PL,ru_RU,ar_SA,cs_CZ,da_DK,no_NO,pt_PT,zh_TW,sv_SE,tr_TR
        public DirectoryInfo InstallationDirectory; // D:\Oyunlar\Origin Games\FIFA 17\
        public string TouchupFile { get; set; }
        public string InstallationParameter{ get; set; }
        public string UpdateParameter { get; set; }
        public string RepairParameter { get; set; }
        public Version AppVersion { get; set; }

        public OriginAppInfo(Library _Library, string _AppName, int _AppID, DirectoryInfo _InstallationDirectory, Version _AppVersion, string[] _Locales, string _TouchupFile, string _InstallationParameter, string _UpdateParameter = null, string _RepairParameter = null)
        {
            Library = _Library;
            AppName = _AppName;
            AppID = _AppID;
            Locales = _Locales;
            InstallationDirectory = _InstallationDirectory;
            TouchupFile = _TouchupFile;
            InstallationParameter = _InstallationParameter;
            UpdateParameter = _UpdateParameter;
            RepairParameter = _RepairParameter;
            AppVersion = _AppVersion;
        }

        //-----
        public Framework.AsyncObservableCollection<FrameworkElement> ContextMenuItems
        {
            get
            {
                Framework.AsyncObservableCollection<FrameworkElement> rightClickMenu = new Framework.AsyncObservableCollection<FrameworkElement>();
                try
                {
                    foreach (ContextMenuItem cItem in List.AppCMenuItems.Where(x => x.IsActive && x.LibraryType == Enums.LibraryType.Origin))
                    {
                        if (!cItem.ShowToNormal)
                        {
                            continue;
                        }

                        if (cItem.IsSeparator)
                        {
                            rightClickMenu.Add(new Separator());
                        }
                        else
                        {
                            MenuItem SLMItem = new MenuItem()
                            {
                                Tag = this,
                                Header = string.Format(cItem.Header, AppName, AppID)
                            };
                            SLMItem.Tag = cItem.Action;
                            SLMItem.Icon = Functions.FAwesome.GetAwesomeIcon(cItem.Icon, cItem.IconColor);
                            SLMItem.HorizontalContentAlignment = HorizontalAlignment.Left;
                            SLMItem.VerticalContentAlignment = VerticalAlignment.Center;
                            SLMItem.Click += Main.FormAccessor.AppCMenuItem_Click;

                            rightClickMenu.Add(SLMItem);
                        }
                    }

                    return rightClickMenu;
                }
                catch (FormatException ex)
                {
                    MessageBox.Show($"An error happened while parsing context menu, most likely happened duo typo on color name.\n\n{ex}");
                    Functions.Logger.LogToFile(Functions.Logger.LogType.SLM, $"[{AppName}][{AppID}] {ex}");

                    return rightClickMenu;
                }
            }
        }

        public void ParseMenuItemAction(string Action)
        {
            try
            {
                switch (Action.ToLowerInvariant())
                {
                    case "disk":
                        InstallationDirectory.Refresh();

                        if (InstallationDirectory.Exists)
                        {
                            System.Diagnostics.Process.Start(InstallationDirectory.FullName);
                        }

                        break;
                    case "deleteappfiles":
                        //await Task.Run(() => DeleteFilesAsync());

                        /*
                        Library.Origin.Apps.Remove(this);
                        if (SLM.CurrentSelectedLibrary == Library)
                            Functions.App.UpdateAppPanel(Library);
                        */
                        MessageBox.Show("Not implemented");
                        break;
                    case "deleteappfilestm":
                        //Framework.TaskManager.AddTask(new List.TaskInfo
                        //{
                        //    App = this,
                        //    TaskType = Enums.TaskType.Delete
                        //});
                        MessageBox.Show("Not implemented");
                        break;
                }
            }
            catch (Exception ex)
            {
                Functions.Logger.LogToFile(Functions.Logger.LogType.App, ex.ToString());
            }
        }

    }
}
