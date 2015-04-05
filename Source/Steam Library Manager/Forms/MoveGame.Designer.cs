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
            this.label_GameToMove = new System.Windows.Forms.Label();
            this.linkLabel_GameName = new System.Windows.Forms.LinkLabel();
            this.comboBox_TargetLibrary = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkbox_Validate = new System.Windows.Forms.CheckBox();
            this.checkbox_RemoveOldFiles = new System.Windows.Forms.CheckBox();
            this.button_Copy = new System.Windows.Forms.Button();
            this.progressBar_CopyStatus = new System.Windows.Forms.ProgressBar();
            this.textBox_CopyLogs = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label_GameToMove
            // 
            this.label_GameToMove.Font = new System.Drawing.Font("Segoe UI Semilight", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label_GameToMove.Location = new System.Drawing.Point(3, 9);
            this.label_GameToMove.Name = "label_GameToMove";
            this.label_GameToMove.Size = new System.Drawing.Size(269, 21);
            this.label_GameToMove.TabIndex = 0;
            this.label_GameToMove.Text = "Game to Move:";
            this.label_GameToMove.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // linkLabel_GameName
            // 
            this.linkLabel_GameName.Font = new System.Drawing.Font("Segoe UI Semilight", 15F);
            this.linkLabel_GameName.Location = new System.Drawing.Point(2, 30);
            this.linkLabel_GameName.Name = "linkLabel_GameName";
            this.linkLabel_GameName.Size = new System.Drawing.Size(270, 31);
            this.linkLabel_GameName.TabIndex = 1;
            this.linkLabel_GameName.TabStop = true;
            this.linkLabel_GameName.Text = "N/A";
            this.linkLabel_GameName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabel_GameName.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_GameName_LinkClicked);
            // 
            // comboBox_TargetLibrary
            // 
            this.comboBox_TargetLibrary.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_TargetLibrary.FormattingEnabled = true;
            this.comboBox_TargetLibrary.Location = new System.Drawing.Point(12, 85);
            this.comboBox_TargetLibrary.Name = "comboBox_TargetLibrary";
            this.comboBox_TargetLibrary.Size = new System.Drawing.Size(260, 21);
            this.comboBox_TargetLibrary.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Segoe UI Semilight", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label1.Location = new System.Drawing.Point(3, 61);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(269, 21);
            this.label1.TabIndex = 3;
            this.label1.Text = "Target Library:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // checkbox_Validate
            // 
            this.checkbox_Validate.AutoSize = true;
            this.checkbox_Validate.Font = new System.Drawing.Font("Segoe UI Semilight", 12F);
            this.checkbox_Validate.Location = new System.Drawing.Point(183, 112);
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
            this.checkbox_RemoveOldFiles.Location = new System.Drawing.Point(12, 112);
            this.checkbox_RemoveOldFiles.Name = "checkbox_RemoveOldFiles";
            this.checkbox_RemoveOldFiles.Size = new System.Drawing.Size(147, 25);
            this.checkbox_RemoveOldFiles.TabIndex = 5;
            this.checkbox_RemoveOldFiles.Text = "Remove Old Files";
            this.checkbox_RemoveOldFiles.UseVisualStyleBackColor = true;
            // 
            // button_Copy
            // 
            this.button_Copy.Font = new System.Drawing.Font("Segoe UI Semilight", 12F);
            this.button_Copy.Location = new System.Drawing.Point(12, 143);
            this.button_Copy.Name = "button_Copy";
            this.button_Copy.Size = new System.Drawing.Size(260, 40);
            this.button_Copy.TabIndex = 6;
            this.button_Copy.Text = "Copy";
            this.button_Copy.UseVisualStyleBackColor = true;
            this.button_Copy.Click += new System.EventHandler(this.button_Copy_Click);
            // 
            // progressBar_CopyStatus
            // 
            this.progressBar_CopyStatus.Location = new System.Drawing.Point(12, 276);
            this.progressBar_CopyStatus.Name = "progressBar_CopyStatus";
            this.progressBar_CopyStatus.Size = new System.Drawing.Size(260, 23);
            this.progressBar_CopyStatus.Step = 1;
            this.progressBar_CopyStatus.TabIndex = 7;
            // 
            // textBox_CopyLogs
            // 
            this.textBox_CopyLogs.Location = new System.Drawing.Point(12, 189);
            this.textBox_CopyLogs.Multiline = true;
            this.textBox_CopyLogs.Name = "textBox_CopyLogs";
            this.textBox_CopyLogs.Size = new System.Drawing.Size(260, 81);
            this.textBox_CopyLogs.TabIndex = 8;
            // 
            // MoveGame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 311);
            this.Controls.Add(this.textBox_CopyLogs);
            this.Controls.Add(this.progressBar_CopyStatus);
            this.Controls.Add(this.button_Copy);
            this.Controls.Add(this.checkbox_RemoveOldFiles);
            this.Controls.Add(this.checkbox_Validate);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBox_TargetLibrary);
            this.Controls.Add(this.linkLabel_GameName);
            this.Controls.Add(this.label_GameToMove);
            this.Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MoveGame";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MoveGame";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.MoveGame_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_GameToMove;
        private System.Windows.Forms.LinkLabel linkLabel_GameName;
        private System.Windows.Forms.ComboBox comboBox_TargetLibrary;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkbox_Validate;
        private System.Windows.Forms.CheckBox checkbox_RemoveOldFiles;
        private System.Windows.Forms.Button button_Copy;
        private System.Windows.Forms.ProgressBar progressBar_CopyStatus;
        private System.Windows.Forms.TextBox textBox_CopyLogs;
    }
}