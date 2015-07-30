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
            this.panel_LibraryList = new System.Windows.Forms.FlowLayoutPanel();
            this.panel_GameList = new System.Windows.Forms.FlowLayoutPanel();
            this.tab_Settings = new System.Windows.Forms.TabPage();
            this.groupBox_Version = new System.Windows.Forms.GroupBox();
            this.groupBox_SLM = new System.Windows.Forms.GroupBox();
            this.checkbox_LogErrorsToFile = new System.Windows.Forms.CheckBox();
            this.SLM_archiveSizeCalcMethod = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SLM_button_GameSizeCalcHelp = new System.Windows.Forms.Button();
            this.SLM_sizeCalculationMethod = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox_Steam = new System.Windows.Forms.GroupBox();
            this.button_SelectSteamPath = new System.Windows.Forms.Button();
            this.linkLabel_SteamPath = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.fileDialog_SelectSteamPath = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowser_SelectNewLibraryPath = new System.Windows.Forms.FolderBrowserDialog();
            this.button_RefreshLibraries = new System.Windows.Forms.Button();
            this.button_newBackupLibrary = new System.Windows.Forms.Button();
            this.button_newSteamLibrary = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tab_InstalledGames.SuspendLayout();
            this.tab_Settings.SuspendLayout();
            this.groupBox_SLM.SuspendLayout();
            this.groupBox_Steam.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tab_InstalledGames);
            this.tabControl1.Controls.Add(this.tab_Settings);
            this.tabControl1.Location = new System.Drawing.Point(4, 11);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 1;
            this.tabControl1.Size = new System.Drawing.Size(853, 745);
            this.tabControl1.TabIndex = 0;
            // 
            // tab_InstalledGames
            // 
            this.tab_InstalledGames.Controls.Add(this.panel_LibraryList);
            this.tab_InstalledGames.Controls.Add(this.panel_GameList);
            this.tab_InstalledGames.Location = new System.Drawing.Point(4, 22);
            this.tab_InstalledGames.Name = "tab_InstalledGames";
            this.tab_InstalledGames.Padding = new System.Windows.Forms.Padding(3);
            this.tab_InstalledGames.Size = new System.Drawing.Size(845, 719);
            this.tab_InstalledGames.TabIndex = 1;
            this.tab_InstalledGames.Text = "Installed Games";
            this.tab_InstalledGames.UseVisualStyleBackColor = true;
            // 
            // panel_LibraryList
            // 
            this.panel_LibraryList.AutoScroll = true;
            this.panel_LibraryList.Location = new System.Drawing.Point(2, 6);
            this.panel_LibraryList.Name = "panel_LibraryList";
            this.panel_LibraryList.Size = new System.Drawing.Size(839, 164);
            this.panel_LibraryList.TabIndex = 2;
            // 
            // panel_GameList
            // 
            this.panel_GameList.AutoScroll = true;
            this.panel_GameList.Location = new System.Drawing.Point(2, 173);
            this.panel_GameList.Margin = new System.Windows.Forms.Padding(10);
            this.panel_GameList.Name = "panel_GameList";
            this.panel_GameList.Size = new System.Drawing.Size(839, 539);
            this.panel_GameList.TabIndex = 0;
            // 
            // tab_Settings
            // 
            this.tab_Settings.Controls.Add(this.groupBox_Version);
            this.tab_Settings.Controls.Add(this.groupBox_SLM);
            this.tab_Settings.Controls.Add(this.groupBox_Steam);
            this.tab_Settings.Location = new System.Drawing.Point(4, 22);
            this.tab_Settings.Name = "tab_Settings";
            this.tab_Settings.Padding = new System.Windows.Forms.Padding(3);
            this.tab_Settings.Size = new System.Drawing.Size(967, 719);
            this.tab_Settings.TabIndex = 2;
            this.tab_Settings.Text = "Settings";
            this.tab_Settings.UseVisualStyleBackColor = true;
            // 
            // groupBox_Version
            // 
            this.groupBox_Version.Location = new System.Drawing.Point(645, 6);
            this.groupBox_Version.Name = "groupBox_Version";
            this.groupBox_Version.Size = new System.Drawing.Size(316, 149);
            this.groupBox_Version.TabIndex = 3;
            this.groupBox_Version.TabStop = false;
            this.groupBox_Version.Text = "Version Control";
            // 
            // groupBox_SLM
            // 
            this.groupBox_SLM.Controls.Add(this.checkbox_LogErrorsToFile);
            this.groupBox_SLM.Controls.Add(this.SLM_archiveSizeCalcMethod);
            this.groupBox_SLM.Controls.Add(this.label3);
            this.groupBox_SLM.Controls.Add(this.SLM_button_GameSizeCalcHelp);
            this.groupBox_SLM.Controls.Add(this.SLM_sizeCalculationMethod);
            this.groupBox_SLM.Controls.Add(this.label2);
            this.groupBox_SLM.Location = new System.Drawing.Point(6, 132);
            this.groupBox_SLM.Name = "groupBox_SLM";
            this.groupBox_SLM.Size = new System.Drawing.Size(363, 229);
            this.groupBox_SLM.TabIndex = 2;
            this.groupBox_SLM.TabStop = false;
            this.groupBox_SLM.Text = "SLM";
            // 
            // checkbox_LogErrorsToFile
            // 
            this.checkbox_LogErrorsToFile.Location = new System.Drawing.Point(158, 70);
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
            this.SLM_archiveSizeCalcMethod.Location = new System.Drawing.Point(158, 43);
            this.SLM_archiveSizeCalcMethod.Name = "SLM_archiveSizeCalcMethod";
            this.SLM_archiveSizeCalcMethod.Size = new System.Drawing.Size(175, 21);
            this.SLM_archiveSizeCalcMethod.TabIndex = 4;
            this.SLM_archiveSizeCalcMethod.SelectedIndexChanged += new System.EventHandler(this.SLM_archiveSizeCalcMethod_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label3.Location = new System.Drawing.Point(6, 46);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(124, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Archive Size Calculation:";
            // 
            // SLM_button_GameSizeCalcHelp
            // 
            this.SLM_button_GameSizeCalcHelp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SLM_button_GameSizeCalcHelp.Location = new System.Drawing.Point(338, 14);
            this.SLM_button_GameSizeCalcHelp.Name = "SLM_button_GameSizeCalcHelp";
            this.SLM_button_GameSizeCalcHelp.Size = new System.Drawing.Size(25, 22);
            this.SLM_button_GameSizeCalcHelp.TabIndex = 2;
            this.SLM_button_GameSizeCalcHelp.Text = "?";
            this.SLM_button_GameSizeCalcHelp.Click += new System.EventHandler(this.SLM_button_GameSizeCalcHelp_Click);
            // 
            // SLM_sizeCalculationMethod
            // 
            this.SLM_sizeCalculationMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SLM_sizeCalculationMethod.IntegralHeight = false;
            this.SLM_sizeCalculationMethod.ItemHeight = 13;
            this.SLM_sizeCalculationMethod.Items.AddRange(new object[] {
            "ACF - Fast, not Accurate",
            "Enum - Slow, Accurate"});
            this.SLM_sizeCalculationMethod.Location = new System.Drawing.Point(158, 15);
            this.SLM_sizeCalculationMethod.Name = "SLM_sizeCalculationMethod";
            this.SLM_sizeCalculationMethod.Size = new System.Drawing.Size(175, 21);
            this.SLM_sizeCalculationMethod.TabIndex = 1;
            this.SLM_sizeCalculationMethod.SelectedIndexChanged += new System.EventHandler(this.SLM_sizeCalculationMethod_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(6, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(116, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Game Size Calculation:";
            // 
            // groupBox_Steam
            // 
            this.groupBox_Steam.Controls.Add(this.button_SelectSteamPath);
            this.groupBox_Steam.Controls.Add(this.linkLabel_SteamPath);
            this.groupBox_Steam.Controls.Add(this.label1);
            this.groupBox_Steam.Location = new System.Drawing.Point(6, 6);
            this.groupBox_Steam.Name = "groupBox_Steam";
            this.groupBox_Steam.Size = new System.Drawing.Size(225, 120);
            this.groupBox_Steam.TabIndex = 0;
            this.groupBox_Steam.TabStop = false;
            this.groupBox_Steam.Text = "Steam";
            // 
            // button_SelectSteamPath
            // 
            this.button_SelectSteamPath.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.button_SelectSteamPath.Location = new System.Drawing.Point(56, 82);
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
            this.label1.Text = "Current Steam path:";
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
            this.button_RefreshLibraries.Location = new System.Drawing.Point(714, 2);
            this.button_RefreshLibraries.Name = "button_RefreshLibraries";
            this.button_RefreshLibraries.Size = new System.Drawing.Size(125, 25);
            this.button_RefreshLibraries.TabIndex = 1;
            this.button_RefreshLibraries.Text = "Refresh Library List";
            this.button_RefreshLibraries.UseVisualStyleBackColor = true;
            this.button_RefreshLibraries.Click += new System.EventHandler(this.button_RefreshLibraries_Click);
            // 
            // button_newBackupLibrary
            // 
            this.button_newBackupLibrary.Location = new System.Drawing.Point(583, 2);
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
            this.button_newSteamLibrary.Location = new System.Drawing.Point(452, 2);
            this.button_newSteamLibrary.Name = "button_newSteamLibrary";
            this.button_newSteamLibrary.Size = new System.Drawing.Size(125, 25);
            this.button_newSteamLibrary.TabIndex = 3;
            this.button_newSteamLibrary.Tag = "false";
            this.button_newSteamLibrary.Text = "New Steam Library";
            this.button_newSteamLibrary.UseVisualStyleBackColor = true;
            this.button_newSteamLibrary.Click += new System.EventHandler(this.newLibrary_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(859, 760);
            this.Controls.Add(this.button_newSteamLibrary);
            this.Controls.Add(this.button_newBackupLibrary);
            this.Controls.Add(this.button_RefreshLibraries);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.Name = "Main";
            this.Padding = new System.Windows.Forms.Padding(20, 30, 20, 20);
            this.Text = "Steam Library Manager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.tabControl1.ResumeLayout(false);
            this.tab_InstalledGames.ResumeLayout(false);
            this.tab_Settings.ResumeLayout(false);
            this.groupBox_SLM.ResumeLayout(false);
            this.groupBox_SLM.PerformLayout();
            this.groupBox_Steam.ResumeLayout(false);
            this.groupBox_Steam.PerformLayout();
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
        private System.Windows.Forms.Button SLM_button_GameSizeCalcHelp;
        public System.Windows.Forms.ComboBox SLM_sizeCalculationMethod;
        public System.Windows.Forms.FolderBrowserDialog folderBrowser_SelectNewLibraryPath;
        public System.Windows.Forms.ComboBox SLM_archiveSizeCalcMethod;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button_RefreshLibraries;
        private System.Windows.Forms.GroupBox groupBox_Version;
        private System.Windows.Forms.Button button_newBackupLibrary;
        private System.Windows.Forms.Button button_newSteamLibrary;
        public System.Windows.Forms.CheckBox checkbox_LogErrorsToFile;
    }
}

