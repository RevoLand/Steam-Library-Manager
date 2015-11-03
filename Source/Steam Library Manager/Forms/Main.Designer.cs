namespace Steam_Library_Manager
{
    partial class Main
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tab_InstalledGames = new System.Windows.Forms.TabPage();
            this.label_searchInLibrary = new System.Windows.Forms.Label();
            this.textBox_searchInGames = new System.Windows.Forms.TextBox();
            this.panel_LibraryList = new System.Windows.Forms.FlowLayoutPanel();
            this.panel_GameList = new System.Windows.Forms.FlowLayoutPanel();
            this.tab_Settings = new System.Windows.Forms.TabPage();
            this.gameContextMenuItems = new System.Windows.Forms.DataGridView();
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
            this.itemName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.itemValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.itemEnabled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.tabControl1.SuspendLayout();
            this.tab_InstalledGames.SuspendLayout();
            this.tab_Settings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gameContextMenuItems)).BeginInit();
            this.groupBox_SLM.SuspendLayout();
            this.groupBox_Steam.SuspendLayout();
            this.groupBox_Version.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tab_InstalledGames);
            this.tabControl1.Controls.Add(this.tab_Settings);
            this.tabControl1.Location = new System.Drawing.Point(4, 33);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 1;
            this.tabControl1.Size = new System.Drawing.Size(853, 750);
            this.tabControl1.TabIndex = 0;
            // 
            // tab_InstalledGames
            // 
            this.tab_InstalledGames.Controls.Add(this.label_searchInLibrary);
            this.tab_InstalledGames.Controls.Add(this.textBox_searchInGames);
            this.tab_InstalledGames.Controls.Add(this.panel_LibraryList);
            this.tab_InstalledGames.Controls.Add(this.panel_GameList);
            this.tab_InstalledGames.Location = new System.Drawing.Point(4, 22);
            this.tab_InstalledGames.Name = "tab_InstalledGames";
            this.tab_InstalledGames.Padding = new System.Windows.Forms.Padding(3);
            this.tab_InstalledGames.Size = new System.Drawing.Size(845, 724);
            this.tab_InstalledGames.TabIndex = 1;
            this.tab_InstalledGames.Text = "Installed Games";
            this.tab_InstalledGames.UseVisualStyleBackColor = true;
            // 
            // label_searchInLibrary
            // 
            this.label_searchInLibrary.AutoSize = true;
            this.label_searchInLibrary.Location = new System.Drawing.Point(481, 701);
            this.label_searchInLibrary.Name = "label_searchInLibrary";
            this.label_searchInLibrary.Size = new System.Drawing.Size(167, 13);
            this.label_searchInLibrary.TabIndex = 4;
            this.label_searchInLibrary.Text = "Search in Library (Name or appID)";
            // 
            // textBox_searchInGames
            // 
            this.textBox_searchInGames.Font = new System.Drawing.Font("Segoe UI Semilight", 9.75F);
            this.textBox_searchInGames.Location = new System.Drawing.Point(654, 695);
            this.textBox_searchInGames.Multiline = true;
            this.textBox_searchInGames.Name = "textBox_searchInGames";
            this.textBox_searchInGames.Size = new System.Drawing.Size(188, 23);
            this.textBox_searchInGames.TabIndex = 3;
            this.textBox_searchInGames.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBox_searchInGames_KeyUp);
            // 
            // panel_LibraryList
            // 
            this.panel_LibraryList.AutoScroll = true;
            this.panel_LibraryList.Location = new System.Drawing.Point(2, 15);
            this.panel_LibraryList.Name = "panel_LibraryList";
            this.panel_LibraryList.Size = new System.Drawing.Size(839, 173);
            this.panel_LibraryList.TabIndex = 2;
            // 
            // panel_GameList
            // 
            this.panel_GameList.AutoScroll = true;
            this.panel_GameList.Location = new System.Drawing.Point(2, 192);
            this.panel_GameList.Margin = new System.Windows.Forms.Padding(10);
            this.panel_GameList.Name = "panel_GameList";
            this.panel_GameList.Size = new System.Drawing.Size(839, 499);
            this.panel_GameList.TabIndex = 0;
            // 
            // tab_Settings
            // 
            this.tab_Settings.Controls.Add(this.gameContextMenuItems);
            this.tab_Settings.Controls.Add(this.groupBox_SLM);
            this.tab_Settings.Controls.Add(this.groupBox_Steam);
            this.tab_Settings.Controls.Add(this.groupBox_Version);
            this.tab_Settings.Location = new System.Drawing.Point(4, 22);
            this.tab_Settings.Name = "tab_Settings";
            this.tab_Settings.Padding = new System.Windows.Forms.Padding(3);
            this.tab_Settings.Size = new System.Drawing.Size(845, 724);
            this.tab_Settings.TabIndex = 2;
            this.tab_Settings.Text = "Settings";
            this.tab_Settings.UseVisualStyleBackColor = true;
            // 
            // gameContextMenuItems
            // 
            this.gameContextMenuItems.AllowUserToOrderColumns = true;
            this.gameContextMenuItems.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.itemName,
            this.itemValue,
            this.itemEnabled});
            this.gameContextMenuItems.Location = new System.Drawing.Point(15, 227);
            this.gameContextMenuItems.Name = "gameContextMenuItems";
            this.gameContextMenuItems.Size = new System.Drawing.Size(343, 231);
            this.gameContextMenuItems.TabIndex = 4;
            // 
            // groupBox_SLM
            // 
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
            this.groupBox_SLM.Location = new System.Drawing.Point(15, 6);
            this.groupBox_SLM.Name = "groupBox_SLM";
            this.groupBox_SLM.Size = new System.Drawing.Size(283, 215);
            this.groupBox_SLM.TabIndex = 2;
            this.groupBox_SLM.TabStop = false;
            this.groupBox_SLM.Text = "Steam Library Manager Settings";
            // 
            // button_changeDefaultTextEditor
            // 
            this.button_changeDefaultTextEditor.Location = new System.Drawing.Point(216, 161);
            this.button_changeDefaultTextEditor.Name = "button_changeDefaultTextEditor";
            this.button_changeDefaultTextEditor.Size = new System.Drawing.Size(61, 23);
            this.button_changeDefaultTextEditor.TabIndex = 9;
            this.button_changeDefaultTextEditor.Text = "Change";
            this.button_changeDefaultTextEditor.UseVisualStyleBackColor = true;
            this.button_changeDefaultTextEditor.Click += new System.EventHandler(this.button_changeDefaultTextEditor_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 145);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(98, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "Default Text Editor:";
            // 
            // SLM_defaultTextEditor
            // 
            this.SLM_defaultTextEditor.Location = new System.Drawing.Point(9, 161);
            this.SLM_defaultTextEditor.Name = "SLM_defaultTextEditor";
            this.SLM_defaultTextEditor.ReadOnly = true;
            this.SLM_defaultTextEditor.Size = new System.Drawing.Size(201, 20);
            this.SLM_defaultTextEditor.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label5.Location = new System.Drawing.Point(6, 105);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(80, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = "Sort Games By:";
            // 
            // SLM_SortGamesBy
            // 
            this.SLM_SortGamesBy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SLM_SortGamesBy.IntegralHeight = false;
            this.SLM_SortGamesBy.ItemHeight = 13;
            this.SLM_SortGamesBy.Items.AddRange(new object[] {
            "appName",
            "appID",
            "sizeOnDisk"});
            this.SLM_SortGamesBy.Location = new System.Drawing.Point(9, 121);
            this.SLM_SortGamesBy.Name = "SLM_SortGamesBy";
            this.SLM_SortGamesBy.Size = new System.Drawing.Size(268, 21);
            this.SLM_SortGamesBy.TabIndex = 5;
            this.SLM_SortGamesBy.SelectedIndexChanged += new System.EventHandler(this.SLM_SortGamesBy_SelectedIndexChanged);
            // 
            // checkbox_LogErrorsToFile
            // 
            this.checkbox_LogErrorsToFile.Location = new System.Drawing.Point(6, 187);
            this.checkbox_LogErrorsToFile.Name = "checkbox_LogErrorsToFile";
            this.checkbox_LogErrorsToFile.Size = new System.Drawing.Size(175, 24);
            this.checkbox_LogErrorsToFile.TabIndex = 4;
            this.checkbox_LogErrorsToFile.Text = "Log errors to File";
            this.checkbox_LogErrorsToFile.UseVisualStyleBackColor = true;
            this.checkbox_LogErrorsToFile.CheckedChanged += new System.EventHandler(this.checkbox_LogErrorsToFile_CheckedChanged);
            // 
            // SLM_archiveSizeCalcMethod
            // 
            this.SLM_archiveSizeCalcMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SLM_archiveSizeCalcMethod.IntegralHeight = false;
            this.SLM_archiveSizeCalcMethod.ItemHeight = 13;
            this.SLM_archiveSizeCalcMethod.Items.AddRange(new object[] {
            "Uncompressed size (Slow, Accurate)",
            "Archive size (Fast, gets archive size)"});
            this.SLM_archiveSizeCalcMethod.Location = new System.Drawing.Point(9, 82);
            this.SLM_archiveSizeCalcMethod.Name = "SLM_archiveSizeCalcMethod";
            this.SLM_archiveSizeCalcMethod.Size = new System.Drawing.Size(268, 21);
            this.SLM_archiveSizeCalcMethod.TabIndex = 4;
            this.SLM_archiveSizeCalcMethod.SelectedIndexChanged += new System.EventHandler(this.SLM_archiveSizeCalcMethod_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label3.Location = new System.Drawing.Point(6, 65);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(124, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Archive Size Calculation:";
            // 
            // SLM_sizeCalculationMethod
            // 
            this.SLM_sizeCalculationMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SLM_sizeCalculationMethod.IntegralHeight = false;
            this.SLM_sizeCalculationMethod.ItemHeight = 13;
            this.SLM_sizeCalculationMethod.Items.AddRange(new object[] {
            "ACF - Fast, not Accurate",
            "Enum - Slow, Accurate"});
            this.SLM_sizeCalculationMethod.Location = new System.Drawing.Point(9, 41);
            this.SLM_sizeCalculationMethod.Name = "SLM_sizeCalculationMethod";
            this.SLM_sizeCalculationMethod.Size = new System.Drawing.Size(268, 21);
            this.SLM_sizeCalculationMethod.TabIndex = 1;
            this.SLM_sizeCalculationMethod.SelectedIndexChanged += new System.EventHandler(this.SLM_sizeCalculationMethod_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(6, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(151, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Game size calculation method:";
            // 
            // groupBox_Steam
            // 
            this.groupBox_Steam.Controls.Add(this.button_SelectSteamPath);
            this.groupBox_Steam.Controls.Add(this.linkLabel_SteamPath);
            this.groupBox_Steam.Controls.Add(this.label1);
            this.groupBox_Steam.Location = new System.Drawing.Point(603, 6);
            this.groupBox_Steam.Name = "groupBox_Steam";
            this.groupBox_Steam.Size = new System.Drawing.Size(225, 103);
            this.groupBox_Steam.TabIndex = 0;
            this.groupBox_Steam.TabStop = false;
            this.groupBox_Steam.Text = "Steam";
            // 
            // button_SelectSteamPath
            // 
            this.button_SelectSteamPath.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.button_SelectSteamPath.Location = new System.Drawing.Point(57, 63);
            this.button_SelectSteamPath.Name = "button_SelectSteamPath";
            this.button_SelectSteamPath.Size = new System.Drawing.Size(112, 32);
            this.button_SelectSteamPath.TabIndex = 3;
            this.button_SelectSteamPath.Text = "Select STEAM Path";
            this.button_SelectSteamPath.Click += new System.EventHandler(this.button_SelectSteamPath_Click);
            // 
            // linkLabel_SteamPath
            // 
            this.linkLabel_SteamPath.AutoSize = true;
            this.linkLabel_SteamPath.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.linkLabel_SteamPath.Location = new System.Drawing.Point(9, 38);
            this.linkLabel_SteamPath.Name = "linkLabel_SteamPath";
            this.linkLabel_SteamPath.Size = new System.Drawing.Size(27, 13);
            this.linkLabel_SteamPath.TabIndex = 2;
            this.linkLabel_SteamPath.TabStop = true;
            this.linkLabel_SteamPath.Text = "N/A";
            this.linkLabel_SteamPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.linkLabel_SteamPath.MouseClick += new System.Windows.Forms.MouseEventHandler(this.linkLabel_SteamPath_LinkClicked);
            // 
            // label1
            // 
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(9, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Steam path:";
            // 
            // groupBox_Version
            // 
            this.groupBox_Version.Controls.Add(this.label_versionResult);
            this.groupBox_Version.Controls.Add(this.checkbox_CheckForUpdatesAtStartup);
            this.groupBox_Version.Controls.Add(this.button_CheckForUpdates);
            this.groupBox_Version.Controls.Add(this.label_LatestVersion);
            this.groupBox_Version.Controls.Add(this.label7);
            this.groupBox_Version.Controls.Add(this.label_CurrentVersion);
            this.groupBox_Version.Controls.Add(this.label4);
            this.groupBox_Version.Location = new System.Drawing.Point(307, 6);
            this.groupBox_Version.Name = "groupBox_Version";
            this.groupBox_Version.Size = new System.Drawing.Size(290, 192);
            this.groupBox_Version.TabIndex = 3;
            this.groupBox_Version.TabStop = false;
            this.groupBox_Version.Text = "Version";
            // 
            // label_versionResult
            // 
            this.label_versionResult.Location = new System.Drawing.Point(7, 135);
            this.label_versionResult.Name = "label_versionResult";
            this.label_versionResult.Size = new System.Drawing.Size(277, 23);
            this.label_versionResult.TabIndex = 8;
            this.label_versionResult.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // checkbox_CheckForUpdatesAtStartup
            // 
            this.checkbox_CheckForUpdatesAtStartup.AutoSize = true;
            this.checkbox_CheckForUpdatesAtStartup.Location = new System.Drawing.Point(6, 167);
            this.checkbox_CheckForUpdatesAtStartup.Name = "checkbox_CheckForUpdatesAtStartup";
            this.checkbox_CheckForUpdatesAtStartup.Size = new System.Drawing.Size(164, 17);
            this.checkbox_CheckForUpdatesAtStartup.TabIndex = 7;
            this.checkbox_CheckForUpdatesAtStartup.Text = "Check for Updates at Startup";
            this.checkbox_CheckForUpdatesAtStartup.UseVisualStyleBackColor = true;
            this.checkbox_CheckForUpdatesAtStartup.CheckedChanged += new System.EventHandler(this.checkbox_CheckForUpdatesAtStartup_CheckedChanged);
            // 
            // button_CheckForUpdates
            // 
            this.button_CheckForUpdates.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.button_CheckForUpdates.Location = new System.Drawing.Point(97, 106);
            this.button_CheckForUpdates.Name = "button_CheckForUpdates";
            this.button_CheckForUpdates.Size = new System.Drawing.Size(104, 25);
            this.button_CheckForUpdates.TabIndex = 6;
            this.button_CheckForUpdates.Text = "Update";
            this.button_CheckForUpdates.UseVisualStyleBackColor = true;
            this.button_CheckForUpdates.Click += new System.EventHandler(this.button_CheckForUpdates_Click);
            // 
            // label_LatestVersion
            // 
            this.label_LatestVersion.Location = new System.Drawing.Point(3, 77);
            this.label_LatestVersion.Name = "label_LatestVersion";
            this.label_LatestVersion.Size = new System.Drawing.Size(168, 20);
            this.label_LatestVersion.TabIndex = 3;
            this.label_LatestVersion.Text = "N\\A";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 61);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(77, 13);
            this.label7.TabIndex = 2;
            this.label7.Text = "Latest Version:";
            // 
            // label_CurrentVersion
            // 
            this.label_CurrentVersion.Location = new System.Drawing.Point(3, 41);
            this.label_CurrentVersion.Name = "label_CurrentVersion";
            this.label_CurrentVersion.Size = new System.Drawing.Size(168, 20);
            this.label_CurrentVersion.TabIndex = 1;
            this.label_CurrentVersion.Text = "N\\A";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 25);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(82, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Current Version:";
            // 
            // fileDialog_SelectSteamPath
            // 
            this.fileDialog_SelectSteamPath.Filter = "Steam|steam.exe";
            this.fileDialog_SelectSteamPath.Title = "Select Steam.exe";
            this.fileDialog_SelectSteamPath.FileOk += new System.ComponentModel.CancelEventHandler(this.fileDialog_SelectSteamPath_FileOk);
            // 
            // folderBrowser_SelectNewLibraryPath
            // 
            this.folderBrowser_SelectNewLibraryPath.RootFolder = System.Environment.SpecialFolder.MyComputer;
            // 
            // button_RefreshLibraries
            // 
            this.button_RefreshLibraries.Location = new System.Drawing.Point(466, 2);
            this.button_RefreshLibraries.Name = "button_RefreshLibraries";
            this.button_RefreshLibraries.Size = new System.Drawing.Size(125, 25);
            this.button_RefreshLibraries.TabIndex = 1;
            this.button_RefreshLibraries.Text = "Refresh Library List";
            this.button_RefreshLibraries.UseVisualStyleBackColor = true;
            this.button_RefreshLibraries.Click += new System.EventHandler(this.button_RefreshLibraries_Click);
            // 
            // button_newBackupLibrary
            // 
            this.button_newBackupLibrary.Location = new System.Drawing.Point(728, 2);
            this.button_newBackupLibrary.Name = "button_newBackupLibrary";
            this.button_newBackupLibrary.Size = new System.Drawing.Size(125, 25);
            this.button_newBackupLibrary.TabIndex = 2;
            this.button_newBackupLibrary.Tag = "true";
            this.button_newBackupLibrary.Text = "New Backup Library";
            this.button_newBackupLibrary.UseVisualStyleBackColor = true;
            this.button_newBackupLibrary.Click += new System.EventHandler(this.newLibrary_Click);
            // 
            // button_newSteamLibrary
            // 
            this.button_newSteamLibrary.Location = new System.Drawing.Point(597, 2);
            this.button_newSteamLibrary.Name = "button_newSteamLibrary";
            this.button_newSteamLibrary.Size = new System.Drawing.Size(125, 25);
            this.button_newSteamLibrary.TabIndex = 3;
            this.button_newSteamLibrary.Tag = "false";
            this.button_newSteamLibrary.Text = "New Steam Library";
            this.button_newSteamLibrary.UseVisualStyleBackColor = true;
            this.button_newSteamLibrary.Click += new System.EventHandler(this.newLibrary_Click);
            // 
            // fileDialog_defaultTextEditor
            // 
            this.fileDialog_defaultTextEditor.Filter = "|*.exe";
            // 
            // itemName
            // 
            this.itemName.HeaderText = "Name";
            this.itemName.Name = "itemName";
            // 
            // itemValue
            // 
            this.itemValue.HeaderText = "Value";
            this.itemValue.Name = "itemValue";
            // 
            // itemEnabled
            // 
            this.itemEnabled.HeaderText = "Enabled";
            this.itemEnabled.Name = "itemEnabled";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(859, 786);
            this.Controls.Add(this.button_newSteamLibrary);
            this.Controls.Add(this.button_newBackupLibrary);
            this.Controls.Add(this.button_RefreshLibraries);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = global::Steam_Library_Manager.Properties.Resources.steam_icon;
            this.MaximizeBox = false;
            this.Name = "Main";
            this.Padding = new System.Windows.Forms.Padding(20, 30, 20, 20);
            this.Text = "Steam Library Manager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.tabControl1.ResumeLayout(false);
            this.tab_InstalledGames.ResumeLayout(false);
            this.tab_InstalledGames.PerformLayout();
            this.tab_Settings.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gameContextMenuItems)).EndInit();
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
        private System.Windows.Forms.DataGridViewTextBoxColumn itemName;
        private System.Windows.Forms.DataGridViewTextBoxColumn itemValue;
        private System.Windows.Forms.DataGridViewCheckBoxColumn itemEnabled;
        public System.Windows.Forms.DataGridView gameContextMenuItems;
    }
}

