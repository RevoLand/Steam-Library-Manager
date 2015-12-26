namespace Steam_Library_Manager
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tab_InstalledGames = new System.Windows.Forms.TabPage();
            this.label_searchInLibrary = new System.Windows.Forms.Label();
            this.textBox_searchInGames = new System.Windows.Forms.TextBox();
            this.panel_LibraryList = new System.Windows.Forms.FlowLayoutPanel();
            this.panel_GameList = new System.Windows.Forms.FlowLayoutPanel();
            this.tab_Settings = new System.Windows.Forms.TabPage();
            this.groupBox_SLM = new System.Windows.Forms.GroupBox();
            this.button_changeDefaultTextEditor = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.SLM_defaultTextEditor = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.SLM_SortGamesBy = new System.Windows.Forms.ComboBox();
            this.checkbox_LogErrorsToFile = new System.Windows.Forms.CheckBox();
            this.SLM_archiveSizeCalcMethod = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SLM_sizeCalculationMethod = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox_Steam = new System.Windows.Forms.GroupBox();
            this.button_SelectSteamPath = new System.Windows.Forms.Button();
            this.linkLabel_SteamPath = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox_Version = new System.Windows.Forms.GroupBox();
            this.label_versionResult = new System.Windows.Forms.Label();
            this.checkbox_CheckForUpdatesAtStartup = new System.Windows.Forms.CheckBox();
            this.button_CheckForUpdates = new System.Windows.Forms.Button();
            this.label_LatestVersion = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label_CurrentVersion = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.fileDialog_SelectSteamPath = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowser_SelectNewLibraryPath = new System.Windows.Forms.FolderBrowserDialog();
            this.button_RefreshLibraries = new System.Windows.Forms.Button();
            this.button_newBackupLibrary = new System.Windows.Forms.Button();
            this.button_newSteamLibrary = new System.Windows.Forms.Button();
            this.fileDialog_defaultTextEditor = new System.Windows.Forms.OpenFileDialog();
            this.tabControl1.SuspendLayout();
            this.tab_InstalledGames.SuspendLayout();
            this.tab_Settings.SuspendLayout();
            this.groupBox_SLM.SuspendLayout();
            this.groupBox_Steam.SuspendLayout();
            this.groupBox_Version.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.Controls.Add(this.tab_InstalledGames);
            this.tabControl1.Controls.Add(this.tab_Settings);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 1;
            // 
            // tab_InstalledGames
            // 
            resources.ApplyResources(this.tab_InstalledGames, "tab_InstalledGames");
            this.tab_InstalledGames.Controls.Add(this.label_searchInLibrary);
            this.tab_InstalledGames.Controls.Add(this.textBox_searchInGames);
            this.tab_InstalledGames.Controls.Add(this.panel_LibraryList);
            this.tab_InstalledGames.Controls.Add(this.panel_GameList);
            this.tab_InstalledGames.Name = "tab_InstalledGames";
            this.tab_InstalledGames.UseVisualStyleBackColor = true;
            // 
            // label_searchInLibrary
            // 
            resources.ApplyResources(this.label_searchInLibrary, "label_searchInLibrary");
            this.label_searchInLibrary.Name = "label_searchInLibrary";
            // 
            // textBox_searchInGames
            // 
            resources.ApplyResources(this.textBox_searchInGames, "textBox_searchInGames");
            this.textBox_searchInGames.Name = "textBox_searchInGames";
            this.textBox_searchInGames.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBox_searchInGames_KeyUp);
            // 
            // panel_LibraryList
            // 
            resources.ApplyResources(this.panel_LibraryList, "panel_LibraryList");
            this.panel_LibraryList.Name = "panel_LibraryList";
            // 
            // panel_GameList
            // 
            resources.ApplyResources(this.panel_GameList, "panel_GameList");
            this.panel_GameList.Name = "panel_GameList";
            // 
            // tab_Settings
            // 
            resources.ApplyResources(this.tab_Settings, "tab_Settings");
            this.tab_Settings.Controls.Add(this.groupBox_SLM);
            this.tab_Settings.Controls.Add(this.groupBox_Steam);
            this.tab_Settings.Controls.Add(this.groupBox_Version);
            this.tab_Settings.Name = "tab_Settings";
            this.tab_Settings.UseVisualStyleBackColor = true;
            // 
            // groupBox_SLM
            // 
            resources.ApplyResources(this.groupBox_SLM, "groupBox_SLM");
            this.groupBox_SLM.Controls.Add(this.button_changeDefaultTextEditor);
            this.groupBox_SLM.Controls.Add(this.label6);
            this.groupBox_SLM.Controls.Add(this.SLM_defaultTextEditor);
            this.groupBox_SLM.Controls.Add(this.label5);
            this.groupBox_SLM.Controls.Add(this.SLM_SortGamesBy);
            this.groupBox_SLM.Controls.Add(this.checkbox_LogErrorsToFile);
            this.groupBox_SLM.Controls.Add(this.SLM_archiveSizeCalcMethod);
            this.groupBox_SLM.Controls.Add(this.label3);
            this.groupBox_SLM.Controls.Add(this.SLM_sizeCalculationMethod);
            this.groupBox_SLM.Controls.Add(this.label2);
            this.groupBox_SLM.Name = "groupBox_SLM";
            this.groupBox_SLM.TabStop = false;
            // 
            // button_changeDefaultTextEditor
            // 
            resources.ApplyResources(this.button_changeDefaultTextEditor, "button_changeDefaultTextEditor");
            this.button_changeDefaultTextEditor.Name = "button_changeDefaultTextEditor";
            this.button_changeDefaultTextEditor.UseVisualStyleBackColor = true;
            this.button_changeDefaultTextEditor.Click += new System.EventHandler(this.button_changeDefaultTextEditor_Click);
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // SLM_defaultTextEditor
            // 
            resources.ApplyResources(this.SLM_defaultTextEditor, "SLM_defaultTextEditor");
            this.SLM_defaultTextEditor.Name = "SLM_defaultTextEditor";
            this.SLM_defaultTextEditor.ReadOnly = true;
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // SLM_SortGamesBy
            // 
            resources.ApplyResources(this.SLM_SortGamesBy, "SLM_SortGamesBy");
            this.SLM_SortGamesBy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SLM_SortGamesBy.Items.AddRange(new object[] {
            resources.GetString("SLM_SortGamesBy.Items"),
            resources.GetString("SLM_SortGamesBy.Items1"),
            resources.GetString("SLM_SortGamesBy.Items2")});
            this.SLM_SortGamesBy.Name = "SLM_SortGamesBy";
            this.SLM_SortGamesBy.SelectedIndexChanged += new System.EventHandler(this.SLM_SortGamesBy_SelectedIndexChanged);
            // 
            // checkbox_LogErrorsToFile
            // 
            resources.ApplyResources(this.checkbox_LogErrorsToFile, "checkbox_LogErrorsToFile");
            this.checkbox_LogErrorsToFile.Name = "checkbox_LogErrorsToFile";
            this.checkbox_LogErrorsToFile.UseVisualStyleBackColor = true;
            this.checkbox_LogErrorsToFile.CheckedChanged += new System.EventHandler(this.checkbox_LogErrorsToFile_CheckedChanged);
            // 
            // SLM_archiveSizeCalcMethod
            // 
            resources.ApplyResources(this.SLM_archiveSizeCalcMethod, "SLM_archiveSizeCalcMethod");
            this.SLM_archiveSizeCalcMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SLM_archiveSizeCalcMethod.Items.AddRange(new object[] {
            resources.GetString("SLM_archiveSizeCalcMethod.Items"),
            resources.GetString("SLM_archiveSizeCalcMethod.Items1")});
            this.SLM_archiveSizeCalcMethod.Name = "SLM_archiveSizeCalcMethod";
            this.SLM_archiveSizeCalcMethod.SelectedIndexChanged += new System.EventHandler(this.SLM_archiveSizeCalcMethod_SelectedIndexChanged);
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // SLM_sizeCalculationMethod
            // 
            resources.ApplyResources(this.SLM_sizeCalculationMethod, "SLM_sizeCalculationMethod");
            this.SLM_sizeCalculationMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SLM_sizeCalculationMethod.Items.AddRange(new object[] {
            resources.GetString("SLM_sizeCalculationMethod.Items"),
            resources.GetString("SLM_sizeCalculationMethod.Items1")});
            this.SLM_sizeCalculationMethod.Name = "SLM_sizeCalculationMethod";
            this.SLM_sizeCalculationMethod.SelectedIndexChanged += new System.EventHandler(this.SLM_sizeCalculationMethod_SelectedIndexChanged);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // groupBox_Steam
            // 
            resources.ApplyResources(this.groupBox_Steam, "groupBox_Steam");
            this.groupBox_Steam.Controls.Add(this.button_SelectSteamPath);
            this.groupBox_Steam.Controls.Add(this.linkLabel_SteamPath);
            this.groupBox_Steam.Controls.Add(this.label1);
            this.groupBox_Steam.Name = "groupBox_Steam";
            this.groupBox_Steam.TabStop = false;
            // 
            // button_SelectSteamPath
            // 
            resources.ApplyResources(this.button_SelectSteamPath, "button_SelectSteamPath");
            this.button_SelectSteamPath.Name = "button_SelectSteamPath";
            this.button_SelectSteamPath.Click += new System.EventHandler(this.button_SelectSteamPath_Click);
            // 
            // linkLabel_SteamPath
            // 
            resources.ApplyResources(this.linkLabel_SteamPath, "linkLabel_SteamPath");
            this.linkLabel_SteamPath.Name = "linkLabel_SteamPath";
            this.linkLabel_SteamPath.TabStop = true;
            this.linkLabel_SteamPath.MouseClick += new System.Windows.Forms.MouseEventHandler(this.linkLabel_SteamPath_LinkClicked);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // groupBox_Version
            // 
            resources.ApplyResources(this.groupBox_Version, "groupBox_Version");
            this.groupBox_Version.Controls.Add(this.label_versionResult);
            this.groupBox_Version.Controls.Add(this.checkbox_CheckForUpdatesAtStartup);
            this.groupBox_Version.Controls.Add(this.button_CheckForUpdates);
            this.groupBox_Version.Controls.Add(this.label_LatestVersion);
            this.groupBox_Version.Controls.Add(this.label7);
            this.groupBox_Version.Controls.Add(this.label_CurrentVersion);
            this.groupBox_Version.Controls.Add(this.label4);
            this.groupBox_Version.Name = "groupBox_Version";
            this.groupBox_Version.TabStop = false;
            // 
            // label_versionResult
            // 
            resources.ApplyResources(this.label_versionResult, "label_versionResult");
            this.label_versionResult.Name = "label_versionResult";
            // 
            // checkbox_CheckForUpdatesAtStartup
            // 
            resources.ApplyResources(this.checkbox_CheckForUpdatesAtStartup, "checkbox_CheckForUpdatesAtStartup");
            this.checkbox_CheckForUpdatesAtStartup.Name = "checkbox_CheckForUpdatesAtStartup";
            this.checkbox_CheckForUpdatesAtStartup.UseVisualStyleBackColor = true;
            this.checkbox_CheckForUpdatesAtStartup.CheckedChanged += new System.EventHandler(this.checkbox_CheckForUpdatesAtStartup_CheckedChanged);
            // 
            // button_CheckForUpdates
            // 
            resources.ApplyResources(this.button_CheckForUpdates, "button_CheckForUpdates");
            this.button_CheckForUpdates.Name = "button_CheckForUpdates";
            this.button_CheckForUpdates.UseVisualStyleBackColor = true;
            this.button_CheckForUpdates.Click += new System.EventHandler(this.button_CheckForUpdates_Click);
            // 
            // label_LatestVersion
            // 
            resources.ApplyResources(this.label_LatestVersion, "label_LatestVersion");
            this.label_LatestVersion.Name = "label_LatestVersion";
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // label_CurrentVersion
            // 
            resources.ApplyResources(this.label_CurrentVersion, "label_CurrentVersion");
            this.label_CurrentVersion.Name = "label_CurrentVersion";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // fileDialog_SelectSteamPath
            // 
            resources.ApplyResources(this.fileDialog_SelectSteamPath, "fileDialog_SelectSteamPath");
            this.fileDialog_SelectSteamPath.FileOk += new System.ComponentModel.CancelEventHandler(this.fileDialog_SelectSteamPath_FileOk);
            // 
            // folderBrowser_SelectNewLibraryPath
            // 
            resources.ApplyResources(this.folderBrowser_SelectNewLibraryPath, "folderBrowser_SelectNewLibraryPath");
            this.folderBrowser_SelectNewLibraryPath.RootFolder = System.Environment.SpecialFolder.MyComputer;
            // 
            // button_RefreshLibraries
            // 
            resources.ApplyResources(this.button_RefreshLibraries, "button_RefreshLibraries");
            this.button_RefreshLibraries.Name = "button_RefreshLibraries";
            this.button_RefreshLibraries.UseVisualStyleBackColor = true;
            this.button_RefreshLibraries.Click += new System.EventHandler(this.button_RefreshLibraries_Click);
            // 
            // button_newBackupLibrary
            // 
            resources.ApplyResources(this.button_newBackupLibrary, "button_newBackupLibrary");
            this.button_newBackupLibrary.Name = "button_newBackupLibrary";
            this.button_newBackupLibrary.Tag = "true";
            this.button_newBackupLibrary.UseVisualStyleBackColor = true;
            this.button_newBackupLibrary.Click += new System.EventHandler(this.newLibrary_Click);
            // 
            // button_newSteamLibrary
            // 
            resources.ApplyResources(this.button_newSteamLibrary, "button_newSteamLibrary");
            this.button_newSteamLibrary.Name = "button_newSteamLibrary";
            this.button_newSteamLibrary.Tag = "false";
            this.button_newSteamLibrary.UseVisualStyleBackColor = true;
            this.button_newSteamLibrary.Click += new System.EventHandler(this.newLibrary_Click);
            // 
            // fileDialog_defaultTextEditor
            // 
            resources.ApplyResources(this.fileDialog_defaultTextEditor, "fileDialog_defaultTextEditor");
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.button_newSteamLibrary);
            this.Controls.Add(this.button_newBackupLibrary);
            this.Controls.Add(this.button_RefreshLibraries);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = global::Steam_Library_Manager.Properties.Resources.steam_icon;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.tabControl1.ResumeLayout(false);
            this.tab_InstalledGames.ResumeLayout(false);
            this.tab_InstalledGames.PerformLayout();
            this.tab_Settings.ResumeLayout(false);
            this.groupBox_SLM.ResumeLayout(false);
            this.groupBox_SLM.PerformLayout();
            this.groupBox_Steam.ResumeLayout(false);
            this.groupBox_Steam.PerformLayout();
            this.groupBox_Version.ResumeLayout(false);
            this.groupBox_Version.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion


        private System.Windows.Forms.TabPage tab_InstalledGames;
        private System.Windows.Forms.TabPage tab_Settings;
        private System.Windows.Forms.OpenFileDialog fileDialog_SelectSteamPath;
        private System.Windows.Forms.GroupBox groupBox_Steam;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.LinkLabel linkLabel_SteamPath;
        public System.Windows.Forms.FlowLayoutPanel panel_GameList;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.Button button_SelectSteamPath;
        public System.Windows.Forms.FlowLayoutPanel panel_LibraryList;
        private System.Windows.Forms.GroupBox groupBox_SLM;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.ComboBox SLM_sizeCalculationMethod;
        public System.Windows.Forms.FolderBrowserDialog folderBrowser_SelectNewLibraryPath;
        public System.Windows.Forms.ComboBox SLM_archiveSizeCalcMethod;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button_RefreshLibraries;
        private System.Windows.Forms.GroupBox groupBox_Version;
        private System.Windows.Forms.Button button_newBackupLibrary;
        private System.Windows.Forms.Button button_newSteamLibrary;
        public System.Windows.Forms.CheckBox checkbox_LogErrorsToFile;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label7;
        public System.Windows.Forms.Label label_CurrentVersion;
        public System.Windows.Forms.Label label_LatestVersion;
        private System.Windows.Forms.Button button_CheckForUpdates;
        public System.Windows.Forms.CheckBox checkbox_CheckForUpdatesAtStartup;
        public System.Windows.Forms.ComboBox SLM_SortGamesBy;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_searchInGames;
        private System.Windows.Forms.Label label_searchInLibrary;
        private System.Windows.Forms.OpenFileDialog fileDialog_defaultTextEditor;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button button_changeDefaultTextEditor;
        public System.Windows.Forms.TextBox SLM_defaultTextEditor;
        public System.Windows.Forms.Label label_versionResult;
    }
}

