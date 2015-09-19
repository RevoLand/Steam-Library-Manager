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
            label1 = new System.Windows.Forms.Label();
            checkbox_Validate = new System.Windows.Forms.CheckBox();
            checkbox_RemoveOldFiles = new System.Windows.Forms.CheckBox();
            button_Copy = new System.Windows.Forms.Button();
            progressBar_CopyStatus = new System.Windows.Forms.ProgressBar();
            textBox_Logs = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            linkLabel_currentLibrary = new System.Windows.Forms.LinkLabel();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label_AvailableSpace = new System.Windows.Forms.Label();
            label_NeededSpace = new System.Windows.Forms.Label();
            pictureBox_GameImage = new System.Windows.Forms.PictureBox();
            linkLabel_TargetLibrary = new System.Windows.Forms.LinkLabel();
            checkbox_Compress = new System.Windows.Forms.CheckBox();
            checkbox_DeCompress = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(pictureBox_GameImage)).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Location = new System.Drawing.Point(12, 223);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(275, 21);
            label1.TabIndex = 3;
            label1.Text = "Target Library:";
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // checkbox_Validate
            // 
            checkbox_Validate.AutoSize = true;
            checkbox_Validate.Location = new System.Drawing.Point(222, 282);
            checkbox_Validate.Name = "checkbox_Validate";
            checkbox_Validate.Size = new System.Drawing.Size(63, 17);
            checkbox_Validate.TabIndex = 4;
            checkbox_Validate.Text = "Validate";
            // 
            // checkbox_RemoveOldFiles
            // 
            checkbox_RemoveOldFiles.AutoSize = true;
            checkbox_RemoveOldFiles.Location = new System.Drawing.Point(12, 282);
            checkbox_RemoveOldFiles.Name = "checkbox_RemoveOldFiles";
            checkbox_RemoveOldFiles.Size = new System.Drawing.Size(105, 17);
            checkbox_RemoveOldFiles.TabIndex = 5;
            checkbox_RemoveOldFiles.Text = "Remove Old Files";
            // 
            // button_Copy
            // 
            button_Copy.Location = new System.Drawing.Point(12, 341);
            button_Copy.Name = "button_Copy";
            button_Copy.Size = new System.Drawing.Size(275, 40);
            button_Copy.TabIndex = 6;
            button_Copy.Text = "Copy";
            button_Copy.Click += new System.EventHandler(button_Copy_Click);
            // 
            // progressBar_CopyStatus
            // 
            progressBar_CopyStatus.Location = new System.Drawing.Point(12, 517);
            progressBar_CopyStatus.Name = "progressBar_CopyStatus";
            progressBar_CopyStatus.Size = new System.Drawing.Size(275, 23);
            progressBar_CopyStatus.Step = 1;
            progressBar_CopyStatus.TabIndex = 7;
            // 
            // textBox_CopyLogs
            // 
            textBox_Logs.Location = new System.Drawing.Point(12, 387);
            textBox_Logs.Multiline = true;
            textBox_Logs.Name = "textBox_CopyLogs";
            textBox_Logs.Size = new System.Drawing.Size(275, 124);
            textBox_Logs.TabIndex = 8;
            // 
            // label2
            // 
            label2.Location = new System.Drawing.Point(12, 165);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(275, 21);
            label2.TabIndex = 9;
            label2.Text = "Current Library:";
            label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // linkLabel_currentLibrary
            // 
            linkLabel_currentLibrary.Location = new System.Drawing.Point(12, 189);
            linkLabel_currentLibrary.Name = "linkLabel_currentLibrary";
            linkLabel_currentLibrary.Size = new System.Drawing.Size(275, 31);
            linkLabel_currentLibrary.TabIndex = 10;
            linkLabel_currentLibrary.TabStop = true;
            linkLabel_currentLibrary.Text = "N/A";
            linkLabel_currentLibrary.Click += new System.EventHandler(linkLabel_currentLibrary_Click);
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(12, 300);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(83, 13);
            label3.TabIndex = 11;
            label3.Text = "Available Space:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(12, 319);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(79, 13);
            label4.TabIndex = 12;
            label4.Text = "Needed Space:";
            // 
            // label_AvailableSpace
            // 
            label_AvailableSpace.Location = new System.Drawing.Point(207, 302);
            label_AvailableSpace.Name = "label_AvailableSpace";
            label_AvailableSpace.Size = new System.Drawing.Size(80, 17);
            label_AvailableSpace.TabIndex = 13;
            label_AvailableSpace.Text = "N/A";
            label_AvailableSpace.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label_NeededSpace
            // 
            label_NeededSpace.Location = new System.Drawing.Point(207, 321);
            label_NeededSpace.Name = "label_NeededSpace";
            label_NeededSpace.Size = new System.Drawing.Size(80, 17);
            label_NeededSpace.TabIndex = 14;
            label_NeededSpace.Text = "N/A";
            label_NeededSpace.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pictureBox_GameImage
            // 
            pictureBox_GameImage.Location = new System.Drawing.Point(12, 33);
            pictureBox_GameImage.Name = "pictureBox_GameImage";
            pictureBox_GameImage.Size = new System.Drawing.Size(275, 129);
            pictureBox_GameImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox_GameImage.TabIndex = 16;
            pictureBox_GameImage.TabStop = false;
            pictureBox_GameImage.MouseClick += new System.Windows.Forms.MouseEventHandler(pictureBox_GameImage_MouseClick);
            // 
            // linkLabel_TargetLibrary
            // 
            linkLabel_TargetLibrary.Location = new System.Drawing.Point(12, 247);
            linkLabel_TargetLibrary.Name = "linkLabel_TargetLibrary";
            linkLabel_TargetLibrary.Size = new System.Drawing.Size(275, 31);
            linkLabel_TargetLibrary.TabIndex = 17;
            linkLabel_TargetLibrary.TabStop = true;
            linkLabel_TargetLibrary.Text = "N/A";
            linkLabel_TargetLibrary.Click += new System.EventHandler(linkLabel_TargetLibrary_Click);
            // 
            // checkbox_Compress
            // 
            checkbox_Compress.AutoSize = true;
            checkbox_Compress.Location = new System.Drawing.Point(123, 282);
            checkbox_Compress.Name = "checkbox_Compress";
            checkbox_Compress.Size = new System.Drawing.Size(72, 17);
            checkbox_Compress.TabIndex = 18;
            checkbox_Compress.Text = "Compress";
            checkbox_Compress.Visible = false;
            // 
            // checkbox_DeCompress
            // 
            checkbox_DeCompress.AutoSize = true;
            checkbox_DeCompress.Location = new System.Drawing.Point(123, 282);
            checkbox_DeCompress.Name = "checkbox_DeCompress";
            checkbox_DeCompress.Size = new System.Drawing.Size(90, 17);
            checkbox_DeCompress.TabIndex = 19;
            checkbox_DeCompress.Text = "De-Compress";
            checkbox_DeCompress.Visible = false;
            // 
            // MoveGame
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(300, 546);
            Controls.Add(checkbox_DeCompress);
            Controls.Add(checkbox_Compress);
            Controls.Add(linkLabel_TargetLibrary);
            Controls.Add(pictureBox_GameImage);
            Controls.Add(label_NeededSpace);
            Controls.Add(label_AvailableSpace);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(linkLabel_currentLibrary);
            Controls.Add(label2);
            Controls.Add(textBox_Logs);
            Controls.Add(progressBar_CopyStatus);
            Controls.Add(button_Copy);
            Controls.Add(checkbox_RemoveOldFiles);
            Controls.Add(checkbox_Validate);
            Controls.Add(label1);
            Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            MaximizeBox = false;
            Name = "MoveGame";
            Padding = new System.Windows.Forms.Padding(20, 30, 20, 20);
            Text = "MoveGame";
            Load += new System.EventHandler(MoveGame_Load);
            ((System.ComponentModel.ISupportInitialize)(pictureBox_GameImage)).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkbox_Validate;
        private System.Windows.Forms.CheckBox checkbox_RemoveOldFiles;
        private System.Windows.Forms.ProgressBar progressBar_CopyStatus;
        private System.Windows.Forms.TextBox textBox_Logs;
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