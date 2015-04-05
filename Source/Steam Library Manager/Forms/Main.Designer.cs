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
            this.tab_MoveLibrary = new System.Windows.Forms.TabPage();
            this.groupBox_GameLibraries = new System.Windows.Forms.GroupBox();
            this.button_gameLibraries_Refresh = new System.Windows.Forms.Button();
            this.button_gameLibraries_RemoveSelected = new System.Windows.Forms.Button();
            this.button_gameLibraries_MoveSelected = new System.Windows.Forms.Button();
            this.button_gameLibraries_AddNew = new System.Windows.Forms.Button();
            this.listBox_GameLibraries = new System.Windows.Forms.ListBox();
            this.groupBox_InstalledGames = new System.Windows.Forms.GroupBox();
            this.button_InstalledGames_MoveGame = new System.Windows.Forms.Button();
            this.listBox_InstalledGames = new System.Windows.Forms.ListBox();
            this.tab_Categories = new System.Windows.Forms.TabPage();
            this.tab_Settings = new System.Windows.Forms.TabPage();
            this.groupBox_Steam = new System.Windows.Forms.GroupBox();
            this.button_SelectSteamPath = new System.Windows.Forms.Button();
            this.linkLabel_SteamPath = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.fileDialog_SelectSteamPath = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowser_SelectNewLibraryPath = new System.Windows.Forms.FolderBrowserDialog();
            this.tabControl1.SuspendLayout();
            this.tab_MoveLibrary.SuspendLayout();
            this.groupBox_GameLibraries.SuspendLayout();
            this.groupBox_InstalledGames.SuspendLayout();
            this.tab_Settings.SuspendLayout();
            this.groupBox_Steam.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tab_MoveLibrary);
            this.tabControl1.Controls.Add(this.tab_Categories);
            this.tabControl1.Controls.Add(this.tab_Settings);
            this.tabControl1.Location = new System.Drawing.Point(4, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(778, 744);
            this.tabControl1.TabIndex = 0;
            // 
            // tab_MoveLibrary
            // 
            this.tab_MoveLibrary.Controls.Add(this.groupBox_GameLibraries);
            this.tab_MoveLibrary.Controls.Add(this.groupBox_InstalledGames);
            this.tab_MoveLibrary.Location = new System.Drawing.Point(4, 22);
            this.tab_MoveLibrary.Name = "tab_MoveLibrary";
            this.tab_MoveLibrary.Padding = new System.Windows.Forms.Padding(3);
            this.tab_MoveLibrary.Size = new System.Drawing.Size(770, 718);
            this.tab_MoveLibrary.TabIndex = 0;
            this.tab_MoveLibrary.Text = "Library";
            this.tab_MoveLibrary.UseVisualStyleBackColor = true;
            // 
            // groupBox_GameLibraries
            // 
            this.groupBox_GameLibraries.Controls.Add(this.button_gameLibraries_Refresh);
            this.groupBox_GameLibraries.Controls.Add(this.button_gameLibraries_RemoveSelected);
            this.groupBox_GameLibraries.Controls.Add(this.button_gameLibraries_MoveSelected);
            this.groupBox_GameLibraries.Controls.Add(this.button_gameLibraries_AddNew);
            this.groupBox_GameLibraries.Controls.Add(this.listBox_GameLibraries);
            this.groupBox_GameLibraries.Location = new System.Drawing.Point(6, 6);
            this.groupBox_GameLibraries.Name = "groupBox_GameLibraries";
            this.groupBox_GameLibraries.Size = new System.Drawing.Size(373, 382);
            this.groupBox_GameLibraries.TabIndex = 1;
            this.groupBox_GameLibraries.TabStop = false;
            this.groupBox_GameLibraries.Text = "Game Libraries";
            // 
            // button_gameLibraries_Refresh
            // 
            this.button_gameLibraries_Refresh.Location = new System.Drawing.Point(225, 345);
            this.button_gameLibraries_Refresh.Name = "button_gameLibraries_Refresh";
            this.button_gameLibraries_Refresh.Size = new System.Drawing.Size(136, 29);
            this.button_gameLibraries_Refresh.TabIndex = 4;
            this.button_gameLibraries_Refresh.Text = "Refresh List";
            this.button_gameLibraries_Refresh.UseVisualStyleBackColor = true;
            this.button_gameLibraries_Refresh.Click += new System.EventHandler(this.button_gameLibraries_Refresh_Click);
            // 
            // button_gameLibraries_RemoveSelected
            // 
            this.button_gameLibraries_RemoveSelected.Location = new System.Drawing.Point(225, 89);
            this.button_gameLibraries_RemoveSelected.Name = "button_gameLibraries_RemoveSelected";
            this.button_gameLibraries_RemoveSelected.Size = new System.Drawing.Size(136, 29);
            this.button_gameLibraries_RemoveSelected.TabIndex = 3;
            this.button_gameLibraries_RemoveSelected.Text = "Remove Selected Library";
            this.button_gameLibraries_RemoveSelected.UseVisualStyleBackColor = true;
            // 
            // button_gameLibraries_MoveSelected
            // 
            this.button_gameLibraries_MoveSelected.Location = new System.Drawing.Point(225, 54);
            this.button_gameLibraries_MoveSelected.Name = "button_gameLibraries_MoveSelected";
            this.button_gameLibraries_MoveSelected.Size = new System.Drawing.Size(136, 29);
            this.button_gameLibraries_MoveSelected.TabIndex = 2;
            this.button_gameLibraries_MoveSelected.Text = "Move Selected Library";
            this.button_gameLibraries_MoveSelected.UseVisualStyleBackColor = true;
            // 
            // button_gameLibraries_AddNew
            // 
            this.button_gameLibraries_AddNew.Location = new System.Drawing.Point(225, 19);
            this.button_gameLibraries_AddNew.Name = "button_gameLibraries_AddNew";
            this.button_gameLibraries_AddNew.Size = new System.Drawing.Size(136, 29);
            this.button_gameLibraries_AddNew.TabIndex = 1;
            this.button_gameLibraries_AddNew.Text = "Add New Library";
            this.button_gameLibraries_AddNew.UseVisualStyleBackColor = true;
            this.button_gameLibraries_AddNew.Click += new System.EventHandler(this.button_gameLibraries_AddNew_Click);
            // 
            // listBox_GameLibraries
            // 
            this.listBox_GameLibraries.FormattingEnabled = true;
            this.listBox_GameLibraries.Location = new System.Drawing.Point(6, 19);
            this.listBox_GameLibraries.Name = "listBox_GameLibraries";
            this.listBox_GameLibraries.Size = new System.Drawing.Size(213, 355);
            this.listBox_GameLibraries.TabIndex = 0;
            this.listBox_GameLibraries.SelectedIndexChanged += new System.EventHandler(this.listBox_GameLibraries_SelectedIndexChanged);
            // 
            // groupBox_InstalledGames
            // 
            this.groupBox_InstalledGames.Controls.Add(this.button_InstalledGames_MoveGame);
            this.groupBox_InstalledGames.Controls.Add(this.listBox_InstalledGames);
            this.groupBox_InstalledGames.Location = new System.Drawing.Point(385, 6);
            this.groupBox_InstalledGames.Name = "groupBox_InstalledGames";
            this.groupBox_InstalledGames.Size = new System.Drawing.Size(379, 382);
            this.groupBox_InstalledGames.TabIndex = 0;
            this.groupBox_InstalledGames.TabStop = false;
            this.groupBox_InstalledGames.Text = "Current Installed Games";
            // 
            // button_InstalledGames_MoveGame
            // 
            this.button_InstalledGames_MoveGame.Location = new System.Drawing.Point(225, 19);
            this.button_InstalledGames_MoveGame.Name = "button_InstalledGames_MoveGame";
            this.button_InstalledGames_MoveGame.Size = new System.Drawing.Size(136, 29);
            this.button_InstalledGames_MoveGame.TabIndex = 1;
            this.button_InstalledGames_MoveGame.Text = "Move Selected Game";
            this.button_InstalledGames_MoveGame.UseVisualStyleBackColor = true;
            this.button_InstalledGames_MoveGame.Click += new System.EventHandler(this.button_InstalledGames_MoveGame_Click);
            // 
            // listBox_InstalledGames
            // 
            this.listBox_InstalledGames.FormattingEnabled = true;
            this.listBox_InstalledGames.Location = new System.Drawing.Point(6, 19);
            this.listBox_InstalledGames.Name = "listBox_InstalledGames";
            this.listBox_InstalledGames.Size = new System.Drawing.Size(213, 355);
            this.listBox_InstalledGames.TabIndex = 0;
            // 
            // tab_Categories
            // 
            this.tab_Categories.Location = new System.Drawing.Point(4, 22);
            this.tab_Categories.Name = "tab_Categories";
            this.tab_Categories.Padding = new System.Windows.Forms.Padding(3);
            this.tab_Categories.Size = new System.Drawing.Size(770, 718);
            this.tab_Categories.TabIndex = 1;
            this.tab_Categories.Text = "Categories";
            this.tab_Categories.UseVisualStyleBackColor = true;
            // 
            // tab_Settings
            // 
            this.tab_Settings.Controls.Add(this.groupBox_Steam);
            this.tab_Settings.Location = new System.Drawing.Point(4, 22);
            this.tab_Settings.Name = "tab_Settings";
            this.tab_Settings.Padding = new System.Windows.Forms.Padding(3);
            this.tab_Settings.Size = new System.Drawing.Size(770, 718);
            this.tab_Settings.TabIndex = 2;
            this.tab_Settings.Text = "Settings";
            this.tab_Settings.UseVisualStyleBackColor = true;
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
            this.button_SelectSteamPath.Location = new System.Drawing.Point(50, 82);
            this.button_SelectSteamPath.Name = "button_SelectSteamPath";
            this.button_SelectSteamPath.Size = new System.Drawing.Size(112, 32);
            this.button_SelectSteamPath.TabIndex = 3;
            this.button_SelectSteamPath.Text = "Select STEAM Path";
            this.button_SelectSteamPath.UseVisualStyleBackColor = true;
            this.button_SelectSteamPath.Click += new System.EventHandler(this.button_SelectSteamPath_Click);
            // 
            // linkLabel_SteamPath
            // 
            this.linkLabel_SteamPath.AutoSize = true;
            this.linkLabel_SteamPath.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkLabel_SteamPath.Location = new System.Drawing.Point(6, 29);
            this.linkLabel_SteamPath.Name = "linkLabel_SteamPath";
            this.linkLabel_SteamPath.Size = new System.Drawing.Size(27, 13);
            this.linkLabel_SteamPath.TabIndex = 2;
            this.linkLabel_SteamPath.TabStop = true;
            this.linkLabel_SteamPath.Text = "N/A";
            this.linkLabel_SteamPath.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_SteamPath_LinkClicked);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
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
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(784, 761);
            this.Controls.Add(this.tabControl1);
            this.MaximizeBox = false;
            this.Name = "Main";
            this.Text = "Steam Library Manager";
            this.tabControl1.ResumeLayout(false);
            this.tab_MoveLibrary.ResumeLayout(false);
            this.groupBox_GameLibraries.ResumeLayout(false);
            this.groupBox_InstalledGames.ResumeLayout(false);
            this.tab_Settings.ResumeLayout(false);
            this.groupBox_Steam.ResumeLayout(false);
            this.groupBox_Steam.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tab_MoveLibrary;
        private System.Windows.Forms.TabPage tab_Categories;
        private System.Windows.Forms.TabPage tab_Settings;
        private System.Windows.Forms.OpenFileDialog fileDialog_SelectSteamPath;
        private System.Windows.Forms.GroupBox groupBox_Steam;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_SelectSteamPath;
        public System.Windows.Forms.LinkLabel linkLabel_SteamPath;
        private System.Windows.Forms.GroupBox groupBox_InstalledGames;
        public System.Windows.Forms.ListBox listBox_InstalledGames;
        private System.Windows.Forms.GroupBox groupBox_GameLibraries;
        public System.Windows.Forms.ListBox listBox_GameLibraries;
        private System.Windows.Forms.Button button_gameLibraries_AddNew;
        private System.Windows.Forms.Button button_gameLibraries_MoveSelected;
        private System.Windows.Forms.Button button_gameLibraries_RemoveSelected;
        private System.Windows.Forms.Button button_gameLibraries_Refresh;
        private System.Windows.Forms.FolderBrowserDialog folderBrowser_SelectNewLibraryPath;
        public System.Windows.Forms.Button button_InstalledGames_MoveGame;
    }
}

