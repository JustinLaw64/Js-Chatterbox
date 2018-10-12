namespace JsChatterBox
{
    partial class ServerWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServerWindow));
            this.ShutdownButton = new System.Windows.Forms.Button();
            this.PeopleConnectedListBox = new System.Windows.Forms.TextBox();
            this.PeopleConnectedLabel = new System.Windows.Forms.Label();
            this.FormUpdateTimer = new System.Windows.Forms.Timer(this.components);
            this.LogTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.ClearLogButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ShutdownButton
            // 
            this.ShutdownButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ShutdownButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ShutdownButton.Location = new System.Drawing.Point(347, 226);
            this.ShutdownButton.Name = "ShutdownButton";
            this.ShutdownButton.Size = new System.Drawing.Size(75, 23);
            this.ShutdownButton.TabIndex = 4;
            this.ShutdownButton.Text = "Close Down";
            this.ShutdownButton.UseVisualStyleBackColor = true;
            this.ShutdownButton.Click += new System.EventHandler(this.ShutdownButton_Click);
            // 
            // PeopleConnectedListBox
            // 
            this.PeopleConnectedListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PeopleConnectedListBox.Location = new System.Drawing.Point(3, 16);
            this.PeopleConnectedListBox.Multiline = true;
            this.PeopleConnectedListBox.Name = "PeopleConnectedListBox";
            this.PeopleConnectedListBox.ReadOnly = true;
            this.PeopleConnectedListBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.PeopleConnectedListBox.Size = new System.Drawing.Size(137, 189);
            this.PeopleConnectedListBox.TabIndex = 1;
            // 
            // PeopleConnectedLabel
            // 
            this.PeopleConnectedLabel.AutoSize = true;
            this.PeopleConnectedLabel.Location = new System.Drawing.Point(0, 0);
            this.PeopleConnectedLabel.Name = "PeopleConnectedLabel";
            this.PeopleConnectedLabel.Size = new System.Drawing.Size(98, 13);
            this.PeopleConnectedLabel.TabIndex = 0;
            this.PeopleConnectedLabel.Text = "People Connected:";
            // 
            // FormUpdateTimer
            // 
            this.FormUpdateTimer.Interval = 200;
            this.FormUpdateTimer.Tick += new System.EventHandler(this.FormUpdateTimer_Tick);
            // 
            // LogTextBox
            // 
            this.LogTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LogTextBox.Location = new System.Drawing.Point(3, 16);
            this.LogTextBox.Multiline = true;
            this.LogTextBox.Name = "LogTextBox";
            this.LogTextBox.ReadOnly = true;
            this.LogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.LogTextBox.Size = new System.Drawing.Size(258, 189);
            this.LogTextBox.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(28, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Log:";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(12, 12);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.PeopleConnectedListBox);
            this.splitContainer1.Panel1.Controls.Add(this.PeopleConnectedLabel);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.LogTextBox);
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Size = new System.Drawing.Size(410, 208);
            this.splitContainer1.SplitterDistance = 142;
            this.splitContainer1.TabIndex = 7;
            // 
            // ClearLogButton
            // 
            this.ClearLogButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ClearLogButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ClearLogButton.Location = new System.Drawing.Point(266, 226);
            this.ClearLogButton.Name = "ClearLogButton";
            this.ClearLogButton.Size = new System.Drawing.Size(75, 23);
            this.ClearLogButton.TabIndex = 8;
            this.ClearLogButton.Text = "Clear Log";
            this.ClearLogButton.UseVisualStyleBackColor = true;
            this.ClearLogButton.Click += new System.EventHandler(this.ClearLogButton_Click);
            // 
            // ServerWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.ShutdownButton;
            this.ClientSize = new System.Drawing.Size(434, 261);
            this.Controls.Add(this.ClearLogButton);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.ShutdownButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(300, 200);
            this.Name = "ServerWindow";
            this.Text = "Js ChatterBox Server";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ServerWindow_FormClosed);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button ShutdownButton;
        private System.Windows.Forms.TextBox PeopleConnectedListBox;
        private System.Windows.Forms.Label PeopleConnectedLabel;
        private System.Windows.Forms.Timer FormUpdateTimer;
        private System.Windows.Forms.TextBox LogTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button ClearLogButton;
    }
}