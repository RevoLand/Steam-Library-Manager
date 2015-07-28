namespace Steam_Library_Manager.Forms
{
    partial class MoveGame
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
            this.label1 = new System.Windows.Forms.Label();
            this.checkbox_Validate = new System.Windows.Forms.CheckBox();
            this.checkbox_RemoveOldFiles = new System.Windows.Forms.CheckBox();
            this.button_Copy = new System.Windows.Forms.Button();
            this.progressBar_CopyStatus = new System.Windows.Forms.ProgressBar();
            this.textBox_CopyLogs = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.linkLabel_currentLibrary = new System.Windows.Forms.LinkLabel();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label_AvailableSpace = new System.Windows.Forms.Label();
            this.label_NeededSpace = new System.Windows.Forms.Label();
            this.pictureBox_GameImage = new System.Windows.Forms.PictureBox();
            this.linkLabel_TargetLibrary = new System.Windows.Forms.LinkLabel();
            this.checkbox_Compress = new System.Windows.Forms.CheckBox();
            this.checkbox_DeCompress = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_GameImage)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 223);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(275, 21);
            this.label1.TabIndex = 3;
            this.label1.Text = "Target Library:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // checkbox_Validate
            // 
            this.checkbox_Validate.AutoSize = true;
            this.checkbox_Validate.Location = new System.Drawing.Point(222, 282);
            this.checkbox_Validate.Name = "checkbox_Validate";
            this.checkbox_Validate.Size = new System.Drawing.Size(63, 17);
            this.checkbox_Validate.TabIndex = 4;
            this.checkbox_Validate.Text = "Validate";
            // 
            // checkbox_RemoveOldFiles
            // 
            this.checkbox_RemoveOldFiles.AutoSize = true;
            this.checkbox_RemoveOldFiles.Location = new System.Drawing.Point(12, 282);
            this.checkbox_RemoveOldFiles.Name = "checkbox_RemoveOldFiles";
            this.checkbox_RemoveOldFiles.Size = new System.Drawing.Size(105, 17);
            this.checkbox_RemoveOldFiles.TabIndex = 5;
            this.checkbox_RemoveOldFiles.Text = "Remove Old Files";
            // 
            // button_Copy
            // 
            this.button_Copy.Location = new System.Drawing.Point(12, 341);
            this.button_Copy.Name = "button_Copy";
            this.button_Copy.Size = new System.Drawing.Size(275, 40);
            this.button_Copy.TabIndex = 6;
            this.button_Copy.Text = "Copy";
            this.button_Copy.Click += new System.EventHandler(this.button_Copy_Click);
            // 
            // progressBar_CopyStatus
            // 
            this.progressBar_CopyStatus.Location = new System.Drawing.Point(12, 517);
            this.progressBar_CopyStatus.Name = "progressBar_CopyStatus";
            this.progressBar_CopyStatus.Size = new System.Drawing.Size(275, 23);
            this.progressBar_CopyStatus.Step = 1;
            this.progressBar_CopyStatus.TabIndex = 7;
            // 
            // textBox_CopyLogs
            // 
            this.textBox_CopyLogs.Location = new System.Drawing.Point(12, 387);
            this.textBox_CopyLogs.Multiline = true;
            this.textBox_CopyLogs.Name = "textBox_CopyLogs";
            this.textBox_CopyLogs.Size = new System.Drawing.Size(275, 124);
            this.textBox_CopyLogs.TabIndex = 8;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(12, 165);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(275, 21);
            this.label2.TabIndex = 9;
            this.label2.Text = "Current Library:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // linkLabel_currentLibrary
            // 
            this.linkLabel_currentLibrary.Location = new System.Drawing.Point(12, 189);
            this.linkLabel_currentLibrary.Name = "linkLabel_currentLibrary";
            this.linkLabel_currentLibrary.Size = new System.Drawing.Size(275, 31);
            this.linkLabel_currentLibrary.TabIndex = 10;
            this.linkLabel_currentLibrary.TabStop = true;
            this.linkLabel_currentLibrary.Text = "N/A";
            this.linkLabel_currentLibrary.Click += new System.EventHandler(this.linkLabel_currentLibrary_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 300);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Available Space:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 319);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(79, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Needed Space:";
            // 
            // label_AvailableSpace
            // 
            this.label_AvailableSpace.Location = new System.Drawing.Point(207, 302);
            this.label_AvailableSpace.Name = "label_AvailableSpace";
            this.label_AvailableSpace.Size = new System.Drawing.Size(80, 17);
            this.label_AvailableSpace.TabIndex = 13;
            this.label_AvailableSpace.Text = "N/A";
            this.label_AvailableSpace.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label_NeededSpace
            // 
            this.label_NeededSpace.Location = new System.Drawing.Point(207, 321);
            this.label_NeededSpace.Name = "label_NeededSpace";
            this.label_NeededSpace.Size = new System.Drawing.Size(80, 17);
            this.label_NeededSpace.TabIndex = 14;
            this.label_NeededSpace.Text = "N/A";
            this.label_NeededSpace.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pictureBox_GameImage
            // 
            this.pictureBox_GameImage.ErrorImage = global::Steam_Library_Manager.Properties.Resources.no_image_available;
            this.pictureBox_GameImage.Location = new System.Drawing.Point(12, 33);
            this.pictureBox_GameImage.Name = "pictureBox_GameImage";
            this.pictureBox_GameImage.Size = new System.Drawing.Size(275, 129);
            this.pictureBox_GameImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_GameImage.TabIndex = 16;
            this.pictureBox_GameImage.TabStop = false;
            this.pictureBox_GameImage.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBox_GameImage_MouseClick);
            // 
            // linkLabel_TargetLibrary
            // 
            this.linkLabel_TargetLibrary.Location = new System.Drawing.Point(12, 247);
            this.linkLabel_TargetLibrary.Name = "linkLabel_TargetLibrary";
            this.linkLabel_TargetLibrary.Size = new System.Drawing.Size(275, 31);
            this.linkLabel_TargetLibrary.TabIndex = 17;
            this.linkLabel_TargetLibrary.TabStop = true;
            this.linkLabel_TargetLibrary.Text = "N/A";
            this.linkLabel_TargetLibrary.Click += new System.EventHandler(this.linkLabel_TargetLibrary_Click);
            // 
            // checkbox_Compress
            // 
            this.checkbox_Compress.AutoSize = true;
            this.checkbox_Compress.Location = new System.Drawing.Point(123, 282);
            this.checkbox_Compress.Name = "checkbox_Compress";
            this.checkbox_Compress.Size = new System.Drawing.Size(72, 17);
            this.checkbox_Compress.TabIndex = 18;
            this.checkbox_Compress.Text = "Compress";
            this.checkbox_Compress.Visible = false;
            // 
            // checkbox_DeCompress
            // 
            this.checkbox_DeCompress.AutoSize = true;
            this.checkbox_DeCompress.Location = new System.Drawing.Point(123, 282);
            this.checkbox_DeCompress.Name = "checkbox_DeCompress";
            this.checkbox_DeCompress.Size = new System.Drawing.Size(90, 17);
            this.checkbox_DeCompress.TabIndex = 19;
            this.checkbox_DeCompress.Text = "De-Compress";
            this.checkbox_DeCompress.Visible = false;
            // 
            // MoveGame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(300, 546);
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
            this.Controls.Add(this.textBox_CopyLogs);
            this.Controls.Add(this.progressBar_CopyStatus);
            this.Controls.Add(this.button_Copy);
            this.Controls.Add(this.checkbox_RemoveOldFiles);
            this.Controls.Add(this.checkbox_Validate);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = global::Steam_Library_Manager.Properties.Resources.steam_icon;
            this.MaximizeBox = false;
            this.Name = "MoveGame";
            this.Padding = new System.Windows.Forms.Padding(20, 30, 20, 20);
            this.Text = "MoveGame";
            this.Load += new System.EventHandler(this.MoveGame_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_GameImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkbox_Validate;
        private System.Windows.Forms.CheckBox checkbox_RemoveOldFiles;
        private System.Windows.Forms.ProgressBar progressBar_CopyStatus;
        private System.Windows.Forms.TextBox textBox_CopyLogs;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.LinkLabel linkLabel_currentLibrary;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label_AvailableSpace;
        private System.Windows.Forms.Label label_NeededSpace;
        private System.Windows.Forms.PictureBox pictureBox_GameImage;
        private System.Windows.Forms.Button button_Copy;
        private System.Windows.Forms.LinkLabel linkLabel_TargetLibrary;
        private System.Windows.Forms.CheckBox checkbox_Compress;
        private System.Windows.Forms.CheckBox checkbox_DeCompress;
    }
}