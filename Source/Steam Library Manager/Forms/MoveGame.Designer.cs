namespace Steam_Library_Manager.Forms
{
    partial class moveGame
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
                processCancelation.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(moveGame));
            this.label1 = new System.Windows.Forms.Label();
            this.checkbox_Validate = new System.Windows.Forms.CheckBox();
            this.checkbox_RemoveOldFiles = new System.Windows.Forms.CheckBox();
            this.button_Copy = new System.Windows.Forms.Button();
            this.progressBar_CopyStatus = new System.Windows.Forms.ProgressBar();
            this.textBox_Logs = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.linkLabel_currentLibrary = new System.Windows.Forms.LinkLabel();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label_AvailableSpace = new System.Windows.Forms.Label();
            this.label_NeededSpace = new System.Windows.Forms.Label();
            this.linkLabel_TargetLibrary = new System.Windows.Forms.LinkLabel();
            this.checkbox_Compress = new System.Windows.Forms.CheckBox();
            this.checkbox_DeCompress = new System.Windows.Forms.CheckBox();
            this.pictureBox_GameImage = new Steam_Library_Manager.Framework.PictureBoxWithCaching();
            this.label_movedFileSize = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_GameImage)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // checkbox_Validate
            // 
            resources.ApplyResources(this.checkbox_Validate, "checkbox_Validate");
            this.checkbox_Validate.Name = "checkbox_Validate";
            // 
            // checkbox_RemoveOldFiles
            // 
            resources.ApplyResources(this.checkbox_RemoveOldFiles, "checkbox_RemoveOldFiles");
            this.checkbox_RemoveOldFiles.Name = "checkbox_RemoveOldFiles";
            // 
            // button_Copy
            // 
            resources.ApplyResources(this.button_Copy, "button_Copy");
            this.button_Copy.Name = "button_Copy";
            this.button_Copy.Click += new System.EventHandler(this.button_Copy_Click);
            // 
            // progressBar_CopyStatus
            // 
            resources.ApplyResources(this.progressBar_CopyStatus, "progressBar_CopyStatus");
            this.progressBar_CopyStatus.Name = "progressBar_CopyStatus";
            this.progressBar_CopyStatus.Step = 1;
            this.progressBar_CopyStatus.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // textBox_Logs
            // 
            this.textBox_Logs.BackColor = System.Drawing.SystemColors.ControlLightLight;
            resources.ApplyResources(this.textBox_Logs, "textBox_Logs");
            this.textBox_Logs.Name = "textBox_Logs";
            this.textBox_Logs.ReadOnly = true;
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // linkLabel_currentLibrary
            // 
            resources.ApplyResources(this.linkLabel_currentLibrary, "linkLabel_currentLibrary");
            this.linkLabel_currentLibrary.Name = "linkLabel_currentLibrary";
            this.linkLabel_currentLibrary.TabStop = true;
            this.linkLabel_currentLibrary.Click += new System.EventHandler(this.linkLabel_currentLibrary_Click);
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // label_AvailableSpace
            // 
            resources.ApplyResources(this.label_AvailableSpace, "label_AvailableSpace");
            this.label_AvailableSpace.Name = "label_AvailableSpace";
            // 
            // label_NeededSpace
            // 
            resources.ApplyResources(this.label_NeededSpace, "label_NeededSpace");
            this.label_NeededSpace.Name = "label_NeededSpace";
            // 
            // linkLabel_TargetLibrary
            // 
            resources.ApplyResources(this.linkLabel_TargetLibrary, "linkLabel_TargetLibrary");
            this.linkLabel_TargetLibrary.Name = "linkLabel_TargetLibrary";
            this.linkLabel_TargetLibrary.TabStop = true;
            this.linkLabel_TargetLibrary.Click += new System.EventHandler(this.linkLabel_TargetLibrary_Click);
            // 
            // checkbox_Compress
            // 
            resources.ApplyResources(this.checkbox_Compress, "checkbox_Compress");
            this.checkbox_Compress.Name = "checkbox_Compress";
            // 
            // checkbox_DeCompress
            // 
            resources.ApplyResources(this.checkbox_DeCompress, "checkbox_DeCompress");
            this.checkbox_DeCompress.Name = "checkbox_DeCompress";
            // 
            // pictureBox_GameImage
            // 
            resources.ApplyResources(this.pictureBox_GameImage, "pictureBox_GameImage");
            this.pictureBox_GameImage.Name = "pictureBox_GameImage";
            this.pictureBox_GameImage.TabStop = false;
            this.pictureBox_GameImage.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBox_GameImage_MouseClick);
            // 
            // label_movedFileSize
            // 
            resources.ApplyResources(this.label_movedFileSize, "label_movedFileSize");
            this.label_movedFileSize.Name = "label_movedFileSize";
            // 
            // moveGame
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label_movedFileSize);
            this.Controls.Add(this.checkbox_DeCompress);
            this.Controls.Add(this.checkbox_Compress);
            this.Controls.Add(this.linkLabel_TargetLibrary);
            this.Controls.Add(this.pictureBox_GameImage);
            this.Controls.Add(this.label_NeededSpace);
            this.Controls.Add(this.label_AvailableSpace);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.linkLabel_currentLibrary);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_Logs);
            this.Controls.Add(this.progressBar_CopyStatus);
            this.Controls.Add(this.button_Copy);
            this.Controls.Add(this.checkbox_RemoveOldFiles);
            this.Controls.Add(this.checkbox_Validate);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = global::Steam_Library_Manager.Properties.Resources.steam_icon;
            this.MaximizeBox = false;
            this.Name = "moveGame";
            this.Load += new System.EventHandler(this.MoveGame_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_GameImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkbox_Validate;
        private System.Windows.Forms.CheckBox checkbox_RemoveOldFiles;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.LinkLabel linkLabel_currentLibrary;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label_AvailableSpace;
        private System.Windows.Forms.Label label_NeededSpace;
        private Framework.PictureBoxWithCaching pictureBox_GameImage;
        private System.Windows.Forms.Button button_Copy;
        private System.Windows.Forms.LinkLabel linkLabel_TargetLibrary;
        private System.Windows.Forms.CheckBox checkbox_Compress;
        private System.Windows.Forms.CheckBox checkbox_DeCompress;
        public System.Windows.Forms.ProgressBar progressBar_CopyStatus;
        public System.Windows.Forms.TextBox textBox_Logs;
        public System.Windows.Forms.Label label_movedFileSize;
    }
}