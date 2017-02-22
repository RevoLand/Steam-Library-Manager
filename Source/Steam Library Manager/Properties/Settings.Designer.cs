﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Steam_Library_Manager.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.0.1.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.SettingsProviderAttribute(typeof(Steam_Library_Manager.Framework.PortableSettingsProvider))]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        [global::System.Configuration.SettingsManageabilityAttribute(global::System.Configuration.SettingsManageability.Roaming)]
        public string steamInstallationPath {
            get {
                return ((string)(this["steamInstallationPath"]));
            }
            set {
                this["steamInstallationPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.SettingsProviderAttribute(typeof(Steam_Library_Manager.Framework.PortableSettingsProvider))]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SettingsManageabilityAttribute(global::System.Configuration.SettingsManageability.Roaming)]
        public global::System.Collections.Specialized.StringCollection backupDirectories {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["backupDirectories"]));
            }
            set {
                this["backupDirectories"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.SettingsProviderAttribute(typeof(Steam_Library_Manager.Framework.PortableSettingsProvider))]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("appName")]
        [global::System.Configuration.SettingsManageabilityAttribute(global::System.Configuration.SettingsManageability.Roaming)]
        public string defaultGameSortingMethod {
            get {
                return ((string)(this["defaultGameSortingMethod"]));
            }
            set {
                this["defaultGameSortingMethod"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.SettingsProviderAttribute(typeof(Steam_Library_Manager.Framework.PortableSettingsProvider))]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ACF")]
        [global::System.Configuration.SettingsManageabilityAttribute(global::System.Configuration.SettingsManageability.Roaming)]
        public string gameSizeCalculationMethod {
            get {
                return ((string)(this["gameSizeCalculationMethod"]));
            }
            set {
                this["gameSizeCalculationMethod"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.SettingsProviderAttribute(typeof(Steam_Library_Manager.Framework.PortableSettingsProvider))]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Uncompressed")]
        [global::System.Configuration.SettingsManageabilityAttribute(global::System.Configuration.SettingsManageability.Roaming)]
        public string archiveSizeCalculationMethod {
            get {
                return ((string)(this["archiveSizeCalculationMethod"]));
            }
            set {
                this["archiveSizeCalculationMethod"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.SettingsProviderAttribute(typeof(Steam_Library_Manager.Framework.PortableSettingsProvider))]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("20")]
        [global::System.Configuration.SettingsManageabilityAttribute(global::System.Configuration.SettingsManageability.Roaming)]
        public long ParallelAfterSize {
            get {
                return ((long)(this["ParallelAfterSize"]));
            }
            set {
                this["ParallelAfterSize"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.SettingsProviderAttribute(typeof(Steam_Library_Manager.Framework.PortableSettingsProvider))]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("text=Play;;action=steam://run/{0};;icon=Play;;iconcolor=#FF000000;;showToNormal=V" +
            "isible;;showToSLMBackup=Visible;;showToSteamBackup=Visible;;showToCompressed=Vis" +
            "ible;;active=True;;separator=False|;;icon=None;;iconcolor=#FF000000;;showToNorma" +
            "l=Visible;;showToSLMBackup=Visible;;showToSteamBackup=Visible;;showToCompressed=" +
            "Visible;;active=True;;separator=True|text={0} ({1});;action=Disk;;icon=FolderOpe" +
            "n;;iconcolor=#FF000000;;showToNormal=Visible;;showToSLMBackup=Visible;;showToSte" +
            "amBackup=Visible;;showToCompressed=Visible;;active=True;;separator=False|text=Vi" +
            "ew ACF File;;action=acffile;;icon=PencilSquareOutline;;iconcolor=#FF000000;;show" +
            "ToNormal=Visible;;showToSLMBackup=Visible;;showToSteamBackup=Visible;;showToComp" +
            "ressed=Visible;;active=True;;separator=False|text=Game Hub;;action=steam://url/G" +
            "ameHub/{0};;icon=Book;;iconcolor=#FF000000;;showToNormal=Visible;;showToSLMBacku" +
            "p=Visible;;showToSteamBackup=Visible;;showToCompressed=Visible;;active=True;;sep" +
            "arator=False|;;icon=None;;iconcolor=#FF000000;;showToNormal=Visible;;showToSLMBa" +
            "ckup=Visible;;showToSteamBackup=Visible;;showToCompressed=Visible;;active=True;;" +
            "separator=True|text=Workshop;;action=steam://url/SteamWorkshopPage/{0};;icon=Cog" +
            ";;iconcolor=#FF000000;;showToNormal=Visible;;showToSLMBackup=Visible;;showToStea" +
            "mBackup=Visible;;showToCompressed=Visible;;active=True;;separator=False|text=Sub" +
            "scribed Workshop Items;;action=http://steamcommunity.com/profiles/{1}/myworkshop" +
            "files/?appid={0}&amp;browsefilter=mysubscriptions&amp;sortmethod=lastupdated;;ic" +
            "on=Cogs;;iconcolor=#FF000000;;showToNormal=Visible;;showToSLMBackup=Visible;;sho" +
            "wToSteamBackup=Visible;;showToCompressed=Visible;;active=True;;separator=False|t" +
            "ext=Size on disk: {2};;action=Disk;;icon=HddOutline;;iconcolor=#FF000000;;showTo" +
            "Normal=Visible;;showToSLMBackup=Visible;;showToSteamBackup=Visible;;showToCompre" +
            "ssed=Visible;;active=True;;separator=False|;;icon=None;;iconcolor=#FF000000;;sho" +
            "wToNormal=Visible;;showToSLMBackup=Visible;;showToSteamBackup=Visible;;showToCom" +
            "pressed=Visible;;active=True;;separator=True|text=Delete game files (SLM);;actio" +
            "n=deleteGameFilesSLM;;icon=Trash;;iconcolor=#FF000000;;showToNormal=Visible;;sho" +
            "wToSLMBackup=Visible;;showToSteamBackup=Visible;;showToCompressed=Visible;;activ" +
            "e=True;;separator=False|")]
        [global::System.Configuration.SettingsManageabilityAttribute(global::System.Configuration.SettingsManageability.Roaming)]
        public string gameContextMenu {
            get {
                return ((string)(this["gameContextMenu"]));
            }
            set {
                this["gameContextMenu"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.SettingsProviderAttribute(typeof(Steam_Library_Manager.Framework.PortableSettingsProvider))]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"text=Open library in explorer ({0});;action=Disk;;icon=FolderOpen;;iconcolor=#FF000000;;showToNormal=Visible;;showToSLMBackup=Visible;;separator=False;;active=True|;;icon=None;;iconcolor=#FF000000;;showToNormal=Visible;;showToSLMBackup=Visible;;separator=True;;active=True|text=Move library;;action=moveLibrary;;icon=Paste;;iconcolor=#FF000000;;showToNormal=Visible;;showToSLMBackup=Visible;;separator=False;;active=True|text=Delete library;;action=deleteLibrary;;icon=Trash;;iconcolor=#FF000000;;showToNormal=Visible;;showToSLMBackup=Visible;;separator=False;;active=True|text=Delete games in library;;action=deleteLibrarySLM;;icon=TrashOutline;;iconcolor=#FF000000;;showToNormal=Visible;;showToSLMBackup=Visible;;separator=False;;active=True|;;icon=None;;iconcolor=#FF000000;;showToNormal=NotVisible;;showToSLMBackup=Visible;;separator=True;;active=True|text=Remove from list;;action=RemoveFromList;;icon=Minus;;iconcolor=#FF000000;;showToNormal=NotVisible;;showToSLMBackup=Visible;;separator=False;;active=True|")]
        [global::System.Configuration.SettingsManageabilityAttribute(global::System.Configuration.SettingsManageability.Roaming)]
        public string libraryContextMenu {
            get {
                return ((string)(this["libraryContextMenu"]));
            }
            set {
                this["libraryContextMenu"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.SettingsProviderAttribute(typeof(Steam_Library_Manager.Framework.PortableSettingsProvider))]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        [global::System.Configuration.SettingsManageabilityAttribute(global::System.Configuration.SettingsManageability.Roaming)]
        public bool includeSearchResults {
            get {
                return ((bool)(this["includeSearchResults"]));
            }
            set {
                this["includeSearchResults"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.SettingsProviderAttribute(typeof(Steam_Library_Manager.Framework.PortableSettingsProvider))]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"&lt;?xml version=""1.0"" encoding=""utf-8""?&gt;&lt;WINDOWPLACEMENT xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""&gt;&lt;length&gt;44&lt;/length&gt;&lt;flags&gt;0&lt;/flags&gt;&lt;showCmd&gt;1&lt;/showCmd&gt;&lt;minPosition&gt;&lt;X&gt;-1&lt;/X&gt;&lt;Y&gt;-1&lt;/Y&gt;&lt;/minPosition&gt;&lt;maxPosition&gt;&lt;X&gt;-1&lt;/X&gt;&lt;Y&gt;-1&lt;/Y&gt;&lt;/maxPosition&gt;&lt;normalPosition&gt;&lt;Left&gt;253&lt;/Left&gt;&lt;Top&gt;198&lt;/Top&gt;&lt;Right&gt;1053&lt;/Right&gt;&lt;Bottom&gt;868&lt;/Bottom&gt;&lt;/normalPosition&gt;&lt;/WINDOWPLACEMENT&gt;")]
        [global::System.Configuration.SettingsManageabilityAttribute(global::System.Configuration.SettingsManageability.Roaming)]
        public string MainWindowPlacement {
            get {
                return ((string)(this["MainWindowPlacement"]));
            }
            set {
                this["MainWindowPlacement"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.SettingsProviderAttribute(typeof(Steam_Library_Manager.Framework.PortableSettingsProvider))]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        [global::System.Configuration.SettingsManageabilityAttribute(global::System.Configuration.SettingsManageability.Roaming)]
        public bool PlayASoundOnCompletion {
            get {
                return ((bool)(this["PlayASoundOnCompletion"]));
            }
            set {
                this["PlayASoundOnCompletion"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.SettingsProviderAttribute(typeof(Steam_Library_Manager.Framework.PortableSettingsProvider))]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Grid")]
        [global::System.Configuration.SettingsManageabilityAttribute(global::System.Configuration.SettingsManageability.Roaming)]
        public string LibraryStyle {
            get {
                return ((string)(this["LibraryStyle"]));
            }
            set {
                this["LibraryStyle"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.SettingsProviderAttribute(typeof(Steam_Library_Manager.Framework.PortableSettingsProvider))]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        [global::System.Configuration.SettingsManageabilityAttribute(global::System.Configuration.SettingsManageability.Roaming)]
        public string CheckforUpdatesAtStartup {
            get {
                return ((string)(this["CheckforUpdatesAtStartup"]));
            }
            set {
                this["CheckforUpdatesAtStartup"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.SettingsProviderAttribute(typeof(Steam_Library_Manager.Framework.PortableSettingsProvider))]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        [global::System.Configuration.SettingsManageabilityAttribute(global::System.Configuration.SettingsManageability.Roaming)]
        public bool Global_RemoveOldFiles {
            get {
                return ((bool)(this["Global_RemoveOldFiles"]));
            }
            set {
                this["Global_RemoveOldFiles"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.SettingsProviderAttribute(typeof(Steam_Library_Manager.Framework.PortableSettingsProvider))]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        [global::System.Configuration.SettingsManageabilityAttribute(global::System.Configuration.SettingsManageability.Roaming)]
        public bool Global_Compress {
            get {
                return ((bool)(this["Global_Compress"]));
            }
            set {
                this["Global_Compress"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.SettingsProviderAttribute(typeof(Steam_Library_Manager.Framework.PortableSettingsProvider))]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        [global::System.Configuration.SettingsManageabilityAttribute(global::System.Configuration.SettingsManageability.Roaming)]
        public bool Global_ReportFileMovement {
            get {
                return ((bool)(this["Global_ReportFileMovement"]));
            }
            set {
                this["Global_ReportFileMovement"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.SettingsProviderAttribute(typeof(Steam_Library_Manager.Framework.PortableSettingsProvider))]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        [global::System.Configuration.SettingsManageabilityAttribute(global::System.Configuration.SettingsManageability.Roaming)]
        public bool Global_StartTaskManagerOnStartup {
            get {
                return ((bool)(this["Global_StartTaskManagerOnStartup"]));
            }
            set {
                this["Global_StartTaskManagerOnStartup"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.SettingsProviderAttribute(typeof(Steam_Library_Manager.Framework.PortableSettingsProvider))]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        [global::System.Configuration.SettingsManageabilityAttribute(global::System.Configuration.SettingsManageability.Roaming)]
        public bool Advanced_Logging {
            get {
                return ((bool)(this["Advanced_Logging"]));
            }
            set {
                this["Advanced_Logging"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.SettingsProviderAttribute(typeof(Steam_Library_Manager.Framework.PortableSettingsProvider))]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        [global::System.Configuration.SettingsManageabilityAttribute(global::System.Configuration.SettingsManageability.Roaming)]
        public string CustomSoundFile {
            get {
                return ((string)(this["CustomSoundFile"]));
            }
            set {
                this["CustomSoundFile"] = value;
            }
        }
    }
}
