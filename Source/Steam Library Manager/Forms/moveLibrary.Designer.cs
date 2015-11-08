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
            this.panel_gamesInLibrary.AutoScroll = true;
            this.panel_gamesInLibrary.Location = new System.Drawing.Point(12, 12);
            this.panel_gamesInLibrary.Name = "panel_gamesInLibrary";
            this.panel_gamesInLibrary.Size = new System.Drawing.Size(506, 214);
            this.panel_gamesInLibrary.TabIndex = 2;
            // 
            // folderBrowser_selectNewLibraryPath
            // 
            this.folderBrowser_selectNewLibraryPath.Description = "Select a new path for library. This can\'t be root nor existing library";
            this.folderBrowser_selectNewLibraryPath.RootFolder = System.Environment.SpecialFolder.MyComputer;
            // 
            // progressBar_libraryMoveProgress
            // 
            this.progressBar_libraryMoveProgress.Location = new System.Drawing.Point(12, 453);
            this.progressBar_libraryMoveProgress.Name = "progressBar_libraryMoveProgress";
            this.progressBar_libraryMoveProgress.Size = new System.Drawing.Size(506, 39);
            this.progressBar_libraryMoveProgress.Step = 1;
            this.progressBar_libraryMoveProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar_libraryMoveProgress.TabIndex = 3;
            // 
            // groupBox_selectedLibrary
            // 
            this.groupBox_selectedLibrary.Controls.Add(this.checkbox_removeOldFiles);
            this.groupBox_selectedLibrary.Controls.Add(this.label_neededSpace);
            this.groupBox_selectedLibrary.Controls.Add(this.label2);
            this.groupBox_selectedLibrary.Controls.Add(this.label_gamesInLibrary);
            this.groupBox_selectedLibrary.Controls.Add(this.label1);
            this.groupBox_selectedLibrary.Location = new System.Drawing.Point(12, 232);
            this.groupBox_selectedLibrary.Name = "groupBox_selectedLibrary";
            this.groupBox_selectedLibrary.Size = new System.Drawing.Size(247, 177);
            this.groupBox_selectedLibrary.TabIndex = 4;
            this.groupBox_selectedLibrary.TabStop = false;
            this.groupBox_selectedLibrary.Text = "Details";
            // 
            // checkbox_removeOldFiles
            // 
            this.checkbox_removeOldFiles.AutoSize = true;
            this.checkbox_removeOldFiles.Location = new System.Drawing.Point(6, 154);
            this.checkbox_removeOldFiles.Name = "checkbox_removeOldFiles";
            this.checkbox_removeOldFiles.Size = new System.Drawing.Size(119, 17);
            this.checkbox_removeOldFiles.TabIndex = 6;
            this.checkbox_removeOldFiles.Text = "Remove Old Library";
            this.checkbox_removeOldFiles.UseVisualStyleBackColor = true;
            // 
            // label_neededSpace
            // 
            this.label_neededSpace.Location = new System.Drawing.Point(164, 27);
            this.label_neededSpace.Name = "label_neededSpace";
            this.label_neededSpace.Size = new System.Drawing.Size(77, 13);
            this.label_neededSpace.TabIndex = 3;
            this.label_neededSpace.Text = "0";
            this.label_neededSpace.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Needed Space:";
            // 
            // label_gamesInLibrary
            // 
            this.label_gamesInLibrary.Location = new System.Drawing.Point(164, 14);
            this.label_gamesInLibrary.Name = "label_gamesInLibrary";
            this.label_gamesInLibrary.Size = new System.Drawing.Size(77, 13);
            this.label_gamesInLibrary.TabIndex = 1;
            this.label_gamesInLibrary.Text = "0";
            this.label_gamesInLibrary.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Games in Library:";
            // 
            // groupBox_targetLibrary
            // 
            this.groupBox_targetLibrary.Controls.Add(this.button_newLibraryButton);
            this.groupBox_targetLibrary.Controls.Add(this.label_availableSpaceAtTargetLibrary);
            this.groupBox_targetLibrary.Controls.Add(this.label5);
            this.groupBox_targetLibrary.Controls.Add(this.label_gamesInTargetLibrary);
            this.groupBox_targetLibrary.Controls.Add(this.label4);
            this.groupBox_targetLibrary.Controls.Add(this.combobox_libraryList);
            this.groupBox_targetLibrary.Location = new System.Drawing.Point(271, 232);
            this.groupBox_targetLibrary.Name = "groupBox_targetLibrary";
            this.groupBox_targetLibrary.Size = new System.Drawing.Size(247, 177);
            this.groupBox_targetLibrary.TabIndex = 5;
            this.groupBox_targetLibrary.TabStop = false;
            this.groupBox_targetLibrary.Text = "Target Library";
            // 
            // button_newLibraryButton
            // 
            this.button_newLibraryButton.Location = new System.Drawing.Point(167, 14);
            this.button_newLibraryButton.Name = "button_newLibraryButton";
            this.button_newLibraryButton.Size = new System.Drawing.Size(74, 23);
            this.button_newLibraryButton.TabIndex = 9;
            this.button_newLibraryButton.Text = "New Library";
            this.button_newLibraryButton.UseVisualStyleBackColor = true;
            this.button_newLibraryButton.Click += new System.EventHandler(this.button_newLibraryButton_Click);
            // 
            // label_availableSpaceAtTargetLibrary
            // 
            this.label_availableSpaceAtTargetLibrary.Location = new System.Drawing.Point(164, 53);
            this.label_availableSpaceAtTargetLibrary.Name = "label_availableSpaceAtTargetLibrary";
            this.label_availableSpaceAtTargetLibrary.Size = new System.Drawing.Size(77, 13);
            this.label_availableSpaceAtTargetLibrary.TabIndex = 8;
            this.label_availableSpaceAtTargetLibrary.Text = "0";
            this.label_availableSpaceAtTargetLibrary.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 53);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(87, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Available Space:";
            // 
            // label_gamesInTargetLibrary
            // 
            this.label_gamesInTargetLibrary.Location = new System.Drawing.Point(164, 40);
            this.label_gamesInTargetLibrary.Name = "label_gamesInTargetLibrary";
            this.label_gamesInTargetLibrary.Size = new System.Drawing.Size(77, 13);
            this.label_gamesInTargetLibrary.TabIndex = 6;
            this.label_gamesInTargetLibrary.Text = "0";
            this.label_gamesInTargetLibrary.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 40);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(88, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Games in Library:";
            // 
            // combobox_libraryList
            // 
            this.combobox_libraryList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.combobox_libraryList.FormattingEnabled = true;
            this.combobox_libraryList.Location = new System.Drawing.Point(6, 16);
            this.combobox_libraryList.Name = "combobox_libraryList";
            this.combobox_libraryList.Size = new System.Drawing.Size(155, 21);
            this.combobox_libraryList.TabIndex = 0;
            this.combobox_libraryList.SelectedIndexChanged += new System.EventHandler(this.combobox_libraryList_SelectedIndexChanged);
            // 
            // button_moveLibrary
            // 
            this.button_moveLibrary.Location = new System.Drawing.Point(179, 415);
            this.button_moveLibrary.Name = "button_moveLibrary";
            this.button_moveLibrary.Size = new System.Drawing.Size(173, 25);
            this.button_moveLibrary.TabIndex = 7;
            this.button_moveLibrary.Text = "Move";
            this.button_moveLibrary.UseVisualStyleBackColor = true;
            this.button_moveLibrary.Click += new System.EventHandler(this.button_moveLibrary_Click);
            // 
            // label_progressInformation
            // 
            this.label_progressInformation.Location = new System.Drawing.Point(306, 495);
            this.label_progressInformation.Name = "label_progressInformation";
            this.label_progressInformation.Size = new System.Drawing.Size(212, 13);
            this.label_progressInformation.TabIndex = 8;
            this.label_progressInformation.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // moveLibrary
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(530, 517);
            this.Controls.Add(this.label_progressInformation);
            this.Controls.Add(this.button_moveLibrary);
            this.Controls.Add(this.groupBox_targetLibrary);
            this.Controls.Add(this.groupBox_selectedLibrary);
            this.Controls.Add(this.progressBar_libraryMoveProgress);
            this.Controls.Add(this.panel_gamesInLibrary);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.Name = "moveLibrary";
            this.Text = "moveLibrary";
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