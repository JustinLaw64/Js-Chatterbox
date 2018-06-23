namespace JsChatterBox
{
    partial class ClientWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ClientWindow));
            this.OutputBoxLabel = new System.Windows.Forms.Label();
            this.MessageTextBox = new System.Windows.Forms.TextBox();
            this.SendMesageButton = new System.Windows.Forms.Button();
            this.ChatLogTextBox = new System.Windows.Forms.TextBox();
            this.FormUpdateTimer = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.ClearLogMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // OutputBoxLabel
            // 
            this.OutputBoxLabel.AutoSize = true;
            this.OutputBoxLabel.Location = new System.Drawing.Point(9, 24);
            this.OutputBoxLabel.Name = "OutputBoxLabel";
            this.OutputBoxLabel.Size = new System.Drawing.Size(28, 13);
            this.OutputBoxLabel.TabIndex = 4;
            this.OutputBoxLabel.Text = "Log:";
            // 
            // MessageTextBox
            // 
            this.MessageTextBox.AcceptsReturn = true;
            this.MessageTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MessageTextBox.Location = new System.Drawing.Point(12, 249);
            this.MessageTextBox.Name = "MessageTextBox";
            this.MessageTextBox.Size = new System.Drawing.Size(370, 20);
            this.MessageTextBox.TabIndex = 6;
            this.MessageTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MessageTextBox_KeyDown);
            // 
            // SendMesageButton
            // 
            this.SendMesageButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SendMesageButton.Location = new System.Drawing.Point(388, 247);
            this.SendMesageButton.Name = "SendMesageButton";
            this.SendMesageButton.Size = new System.Drawing.Size(64, 23);
            this.SendMesageButton.TabIndex = 7;
            this.SendMesageButton.Text = "Send";
            this.SendMesageButton.UseVisualStyleBackColor = true;
            this.SendMesageButton.Click += new System.EventHandler(this.SendMesageButton_Click);
            // 
            // ChatLogTextBox
            // 
            this.ChatLogTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ChatLogTextBox.Location = new System.Drawing.Point(12, 40);
            this.ChatLogTextBox.Multiline = true;
            this.ChatLogTextBox.Name = "ChatLogTextBox";
            this.ChatLogTextBox.ReadOnly = true;
            this.ChatLogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ChatLogTextBox.Size = new System.Drawing.Size(440, 201);
            this.ChatLogTextBox.TabIndex = 5;
            // 
            // FormUpdateTimer
            // 
            this.FormUpdateTimer.Interval = 500;
            this.FormUpdateTimer.Tick += new System.EventHandler(this.FormUpdateTimer_Tick);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ClearLogMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(464, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // ClearLogMenuItem
            // 
            this.ClearLogMenuItem.Name = "ClearLogMenuItem";
            this.ClearLogMenuItem.Size = new System.Drawing.Size(69, 20);
            this.ClearLogMenuItem.Text = "Clear &Log";
            this.ClearLogMenuItem.Click += new System.EventHandler(this.ClearLogMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.aboutToolStripMenuItem.Text = "&About...";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.HelpAboutMenuItem_Click);
            // 
            // ClientWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(464, 281);
            this.Controls.Add(this.SendMesageButton);
            this.Controls.Add(this.MessageTextBox);
            this.Controls.Add(this.OutputBoxLabel);
            this.Controls.Add(this.ChatLogTextBox);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(480, 320);
            this.Name = "ClientWindow";
            this.Text = "Js ChatterBox";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ChatForm_FormClosed);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox ChatLogTextBox;
        private System.Windows.Forms.TextBox MessageTextBox;
        private System.Windows.Forms.Button SendMesageButton;
        private System.Windows.Forms.Timer FormUpdateTimer;
        private System.Windows.Forms.Label OutputBoxLabel;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem ClearLogMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
    }
}

