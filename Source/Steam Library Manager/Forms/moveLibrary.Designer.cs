namespace Steam_Library_Manager.Forms
{
    partial class moveLibrary
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(moveLibrary));
            this.panel_gamesInLibrary = new System.Windows.Forms.FlowLayoutPanel();
            this.folderBrowser_selectNewLibraryPath = new System.Windows.Forms.FolderBrowserDialog();
            this.progressBar_libraryMoveProgress = new System.Windows.Forms.ProgressBar();
            this.groupBox_selectedLibrary = new System.Windows.Forms.GroupBox();
            this.checkbox_removeOldFiles = new System.Windows.Forms.CheckBox();
            this.label_neededSpace = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label_gamesInLibrary = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox_targetLibrary = new System.Windows.Forms.GroupBox();
            this.button_newLibraryButton = new System.Windows.Forms.Button();
            this.label_availableSpaceAtTargetLibrary = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label_gamesInTargetLibrary = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.combobox_libraryList = new System.Windows.Forms.ComboBox();
            this.button_moveLibrary = new System.Windows.Forms.Button();
            this.label_progressInformation = new System.Windows.Forms.Label();
            this.groupBox_selectedLibrary.SuspendLayout();
            this.groupBox_targetLibrary.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel_gamesInLibrary
            // 
            resources.ApplyResources(this.panel_gamesInLibrary, "panel_gamesInLibrary");
            this.panel_gamesInLibrary.Name = "panel_gamesInLibrary";
            // 
            // folderBrowser_selectNewLibraryPath
            // 
            resources.ApplyResources(this.folderBrowser_selectNewLibraryPath, "folderBrowser_selectNewLibraryPath");
            this.folderBrowser_selectNewLibraryPath.RootFolder = System.Environment.SpecialFolder.MyComputer;
            // 
            // progressBar_libraryMoveProgress
            // 
            resources.ApplyResources(this.progressBar_libraryMoveProgress, "progressBar_libraryMoveProgress");
            this.progressBar_libraryMoveProgress.Name = "progressBar_libraryMoveProgress";
            this.progressBar_libraryMoveProgress.Step = 1;
            this.progressBar_libraryMoveProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // groupBox_selectedLibrary
            // 
            resources.ApplyResources(this.groupBox_selectedLibrary, "groupBox_selectedLibrary");
            this.groupBox_selectedLibrary.Controls.Add(this.checkbox_removeOldFiles);
            this.groupBox_selectedLibrary.Controls.Add(this.label_neededSpace);
            this.groupBox_selectedLibrary.Controls.Add(this.label2);
            this.groupBox_selectedLibrary.Controls.Add(this.label_gamesInLibrary);
            this.groupBox_selectedLibrary.Controls.Add(this.label1);
            this.groupBox_selectedLibrary.Name = "groupBox_selectedLibrary";
            this.groupBox_selectedLibrary.TabStop = false;
            // 
            // checkbox_removeOldFiles
            // 
            resources.ApplyResources(this.checkbox_removeOldFiles, "checkbox_removeOldFiles");
            this.checkbox_removeOldFiles.Name = "checkbox_removeOldFiles";
            this.checkbox_removeOldFiles.UseVisualStyleBackColor = true;
            // 
            // label_neededSpace
            // 
            resources.ApplyResources(this.label_neededSpace, "label_neededSpace");
            this.label_neededSpace.Name = "label_neededSpace";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label_gamesInLibrary
            // 
            resources.ApplyResources(this.label_gamesInLibrary, "label_gamesInLibrary");
            this.label_gamesInLibrary.Name = "label_gamesInLibrary";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // groupBox_targetLibrary
            // 
            resources.ApplyResources(this.groupBox_targetLibrary, "groupBox_targetLibrary");
            this.groupBox_targetLibrary.Controls.Add(this.button_newLibraryButton);
            this.groupBox_targetLibrary.Controls.Add(this.label_availableSpaceAtTargetLibrary);
            this.groupBox_targetLibrary.Controls.Add(this.label5);
            this.groupBox_targetLibrary.Controls.Add(this.label_gamesInTargetLibrary);
            this.groupBox_targetLibrary.Controls.Add(this.label4);
            this.groupBox_targetLibrary.Controls.Add(this.combobox_libraryList);
            this.groupBox_targetLibrary.Name = "groupBox_targetLibrary";
            this.groupBox_targetLibrary.TabStop = false;
            // 
            // button_newLibraryButton
            // 
            resources.ApplyResources(this.button_newLibraryButton, "button_newLibraryButton");
            this.button_newLibraryButton.Name = "button_newLibraryButton";
            this.button_newLibraryButton.UseVisualStyleBackColor = true;
            this.button_newLibraryButton.Click += new System.EventHandler(this.button_newLibraryButton_Click);
            // 
            // label_availableSpaceAtTargetLibrary
            // 
            resources.ApplyResources(this.label_availableSpaceAtTargetLibrary, "label_availableSpaceAtTargetLibrary");
            this.label_availableSpaceAtTargetLibrary.Name = "label_availableSpaceAtTargetLibrary";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // label_gamesInTargetLibrary
            // 
            resources.ApplyResources(this.label_gamesInTargetLibrary, "label_gamesInTargetLibrary");
            this.label_gamesInTargetLibrary.Name = "label_gamesInTargetLibrary";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // combobox_libraryList
            // 
            resources.ApplyResources(this.combobox_libraryList, "combobox_libraryList");
            this.combobox_libraryList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.combobox_libraryList.FormattingEnabled = true;
            this.combobox_libraryList.Name = "combobox_libraryList";
            this.combobox_libraryList.SelectedIndexChanged += new System.EventHandler(this.combobox_libraryList_SelectedIndexChanged);
            // 
            // button_moveLibrary
            // 
            resources.ApplyResources(this.button_moveLibrary, "button_moveLibrary");
            this.button_moveLibrary.Name = "button_moveLibrary";
            this.button_moveLibrary.UseVisualStyleBackColor = true;
            this.button_moveLibrary.Click += new System.EventHandler(this.button_moveLibrary_Click);
            // 
            // label_progressInformation
            // 
            resources.ApplyResources(this.label_progressInformation, "label_progressInformation");
            this.label_progressInformation.Name = "label_progressInformation";
            // 
            // moveLibrary
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label_progressInformation);
            this.Controls.Add(this.button_moveLibrary);
            this.Controls.Add(this.groupBox_targetLibrary);
            this.Controls.Add(this.groupBox_selectedLibrary);
            this.Controls.Add(this.progressBar_libraryMoveProgress);
            this.Controls.Add(this.panel_gamesInLibrary);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.Name = "moveLibrary";
            this.Load += new System.EventHandler(this.moveLibrary_Load);
            this.groupBox_selectedLibrary.ResumeLayout(false);
            this.groupBox_selectedLibrary.PerformLayout();
            this.groupBox_targetLibrary.ResumeLayout(false);
            this.groupBox_targetLibrary.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel panel_gamesInLibrary;
        private System.Windows.Forms.FolderBrowserDialog folderBrowser_selectNewLibraryPath;
        private System.Windows.Forms.ProgressBar progressBar_libraryMoveProgress;
        private System.Windows.Forms.GroupBox groupBox_selectedLibrary;
        private System.Windows.Forms.GroupBox groupBox_targetLibrary;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label_gamesInLibrary;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label_neededSpace;
        private System.Windows.Forms.ComboBox combobox_libraryList;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label_gamesInTargetLibrary;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label_availableSpaceAtTargetLibrary;
        private System.Windows.Forms.CheckBox checkbox_removeOldFiles;
        private System.Windows.Forms.Button button_moveLibrary;
        private System.Windows.Forms.Label label_progressInformation;
        private System.Windows.Forms.Button button_newLibraryButton;
    }
}