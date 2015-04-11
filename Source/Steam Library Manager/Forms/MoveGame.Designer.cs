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
            this.components = new System.ComponentModel.Container();
            this.comboBox_TargetLibrary = new System.Windows.Forms.ComboBox();
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
            this.label_TimeElapsed = new System.Windows.Forms.Label();
            this.timer_TimeElapsed = new System.Windows.Forms.Timer(this.components);
            this.pictureBox_GameImage = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_GameImage)).BeginInit();
            this.SuspendLayout();
            // 
            // comboBox_TargetLibrary
            // 
            this.comboBox_TargetLibrary.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_TargetLibrary.FormattingEnabled = true;
            this.comboBox_TargetLibrary.Location = new System.Drawing.Point(12, 175);
            this.comboBox_TargetLibrary.Name = "comboBox_TargetLibrary";
            this.comboBox_TargetLibrary.Size = new System.Drawing.Size(260, 21);
            this.comboBox_TargetLibrary.TabIndex = 2;
            this.comboBox_TargetLibrary.SelectedIndexChanged += new System.EventHandler(this.comboBox_TargetLibrary_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Segoe UI Semilight", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label1.Location = new System.Drawing.Point(8, 151);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(260, 21);
            this.label1.TabIndex = 3;
            this.label1.Text = "Target Library:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // checkbox_Validate
            // 
            this.checkbox_Validate.AutoSize = true;
            this.checkbox_Validate.Font = new System.Drawing.Font("Segoe UI Semilight", 12F);
            this.checkbox_Validate.Location = new System.Drawing.Point(188, 202);
            this.checkbox_Validate.Name = "checkbox_Validate";
            this.checkbox_Validate.Size = new System.Drawing.Size(84, 25);
            this.checkbox_Validate.TabIndex = 4;
            this.checkbox_Validate.Text = "Validate";
            this.checkbox_Validate.UseVisualStyleBackColor = true;
            // 
            // checkbox_RemoveOldFiles
            // 
            this.checkbox_RemoveOldFiles.AutoSize = true;
            this.checkbox_RemoveOldFiles.Font = new System.Drawing.Font("Segoe UI Semilight", 12F);
            this.checkbox_RemoveOldFiles.Location = new System.Drawing.Point(12, 202);
            this.checkbox_RemoveOldFiles.Name = "checkbox_RemoveOldFiles";
            this.checkbox_RemoveOldFiles.Size = new System.Drawing.Size(147, 25);
            this.checkbox_RemoveOldFiles.TabIndex = 5;
            this.checkbox_RemoveOldFiles.Text = "Remove Old Files";
            this.checkbox_RemoveOldFiles.UseVisualStyleBackColor = true;
            // 
            // button_Copy
            // 
            this.button_Copy.Font = new System.Drawing.Font("Segoe UI Semilight", 12F);
            this.button_Copy.Location = new System.Drawing.Point(12, 271);
            this.button_Copy.Name = "button_Copy";
            this.button_Copy.Size = new System.Drawing.Size(260, 40);
            this.button_Copy.TabIndex = 6;
            this.button_Copy.Text = "Copy";
            this.button_Copy.UseVisualStyleBackColor = true;
            this.button_Copy.Click += new System.EventHandler(this.button_Copy_Click);
            // 
            // progressBar_CopyStatus
            // 
            this.progressBar_CopyStatus.Location = new System.Drawing.Point(12, 414);
            this.progressBar_CopyStatus.Name = "progressBar_CopyStatus";
            this.progressBar_CopyStatus.Size = new System.Drawing.Size(260, 23);
            this.progressBar_CopyStatus.Step = 1;
            this.progressBar_CopyStatus.TabIndex = 7;
            // 
            // textBox_CopyLogs
            // 
            this.textBox_CopyLogs.Location = new System.Drawing.Point(12, 317);
            this.textBox_CopyLogs.Multiline = true;
            this.textBox_CopyLogs.Name = "textBox_CopyLogs";
            this.textBox_CopyLogs.Size = new System.Drawing.Size(260, 91);
            this.textBox_CopyLogs.TabIndex = 8;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Segoe UI Semilight", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label2.Location = new System.Drawing.Point(8, 99);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(260, 21);
            this.label2.TabIndex = 9;
            this.label2.Text = "Current Library:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // linkLabel_currentLibrary
            // 
            this.linkLabel_currentLibrary.Font = new System.Drawing.Font("Segoe UI Semilight", 12F);
            this.linkLabel_currentLibrary.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkLabel_currentLibrary.LinkColor = System.Drawing.Color.Blue;
            this.linkLabel_currentLibrary.Location = new System.Drawing.Point(8, 120);
            this.linkLabel_currentLibrary.Name = "linkLabel_currentLibrary";
            this.linkLabel_currentLibrary.Size = new System.Drawing.Size(260, 31);
            this.linkLabel_currentLibrary.TabIndex = 10;
            this.linkLabel_currentLibrary.TabStop = true;
            this.linkLabel_currentLibrary.Text = "N/A";
            this.linkLabel_currentLibrary.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabel_currentLibrary.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_currentLibrary_LinkClicked);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semilight", 10F);
            this.label3.Location = new System.Drawing.Point(8, 230);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(105, 19);
            this.label3.TabIndex = 11;
            this.label3.Text = "Available Space:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI Semilight", 10F);
            this.label4.Location = new System.Drawing.Point(8, 249);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(98, 19);
            this.label4.TabIndex = 12;
            this.label4.Text = "Needed Space:";
            // 
            // label_AvailableSpace
            // 
            this.label_AvailableSpace.Font = new System.Drawing.Font("Segoe UI Semilight", 10F);
            this.label_AvailableSpace.Location = new System.Drawing.Point(192, 230);
            this.label_AvailableSpace.Name = "label_AvailableSpace";
            this.label_AvailableSpace.Size = new System.Drawing.Size(80, 17);
            this.label_AvailableSpace.TabIndex = 13;
            this.label_AvailableSpace.Text = "N/A";
            this.label_AvailableSpace.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label_NeededSpace
            // 
            this.label_NeededSpace.Font = new System.Drawing.Font("Segoe UI Semilight", 10F);
            this.label_NeededSpace.Location = new System.Drawing.Point(192, 251);
            this.label_NeededSpace.Name = "label_NeededSpace";
            this.label_NeededSpace.Size = new System.Drawing.Size(80, 17);
            this.label_NeededSpace.TabIndex = 14;
            this.label_NeededSpace.Text = "N/A";
            this.label_NeededSpace.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label_TimeElapsed
            // 
            this.label_TimeElapsed.Font = new System.Drawing.Font("Segoe UI Semilight", 10F);
            this.label_TimeElapsed.Location = new System.Drawing.Point(8, 440);
            this.label_TimeElapsed.Name = "label_TimeElapsed";
            this.label_TimeElapsed.Size = new System.Drawing.Size(264, 19);
            this.label_TimeElapsed.TabIndex = 15;
            this.label_TimeElapsed.Text = "Time Elapsed: 0";
            this.label_TimeElapsed.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // timer_TimeElapsed
            // 
            this.timer_TimeElapsed.Tick += new System.EventHandler(this.timer_TimeElapsed_Tick);
            // 
            // pictureBox_GameImage
            // 
            this.pictureBox_GameImage.ErrorImage = global::Steam_Library_Manager.Properties.Resources.no_image_available;
            this.pictureBox_GameImage.Location = new System.Drawing.Point(12, 4);
            this.pictureBox_GameImage.Name = "pictureBox_GameImage";
            this.pictureBox_GameImage.Size = new System.Drawing.Size(260, 97);
            this.pictureBox_GameImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_GameImage.TabIndex = 16;
            this.pictureBox_GameImage.TabStop = false;
            this.pictureBox_GameImage.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBox_GameImage_MouseClick);
            // 
            // MoveGame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 461);
            this.Controls.Add(this.pictureBox_GameImage);
            this.Controls.Add(this.label_TimeElapsed);
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
            this.Controls.Add(this.comboBox_TargetLibrary);
            this.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MoveGame";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MoveGame";
            this.Load += new System.EventHandler(this.MoveGame_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_GameImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox_TargetLibrary;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkbox_Validate;
        private System.Windows.Forms.CheckBox checkbox_RemoveOldFiles;
        private System.Windows.Forms.Button button_Copy;
        private System.Windows.Forms.ProgressBar progressBar_CopyStatus;
        private System.Windows.Forms.TextBox textBox_CopyLogs;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.LinkLabel linkLabel_currentLibrary;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label_AvailableSpace;
        private System.Windows.Forms.Label label_NeededSpace;
        private System.Windows.Forms.Label label_TimeElapsed;
        private System.Windows.Forms.Timer timer_TimeElapsed;
        private System.Windows.Forms.PictureBox pictureBox_GameImage;
    }
}