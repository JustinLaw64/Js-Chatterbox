namespace JsChatterBox
{
    partial class RootForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RootForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearRecentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.advancedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createChatWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.HostNameTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.HostSelectionList = new System.Windows.Forms.ListBox();
            this.ConnectButton = new System.Windows.Forms.Button();
            this.HostPortTextBox = new System.Windows.Forms.TextBox();
            this.FavoriteToggleButton = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.advancedToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(284, 24);
            this.menuStrip1.TabIndex = 11;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearRecentToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // clearRecentToolStripMenuItem
            // 
            this.clearRecentToolStripMenuItem.Name = "clearRecentToolStripMenuItem";
            this.clearRecentToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.clearRecentToolStripMenuItem.Text = "Clear &Recent";
            this.clearRecentToolStripMenuItem.Click += new System.EventHandler(this.ClearRecentMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(137, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitMenuItem_Click);
            // 
            // advancedToolStripMenuItem
            // 
            this.advancedToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createChatWindowToolStripMenuItem,
            this.createServerToolStripMenuItem});
            this.advancedToolStripMenuItem.Name = "advancedToolStripMenuItem";
            this.advancedToolStripMenuItem.Size = new System.Drawing.Size(72, 20);
            this.advancedToolStripMenuItem.Text = "&Advanced";
            // 
            // createChatWindowToolStripMenuItem
            // 
            this.createChatWindowToolStripMenuItem.Name = "createChatWindowToolStripMenuItem";
            this.createChatWindowToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.createChatWindowToolStripMenuItem.Text = "Create Chat Window";
            this.createChatWindowToolStripMenuItem.Click += new System.EventHandler(this.CreateClientMenuItem_Click);
            // 
            // createServerToolStripMenuItem
            // 
            this.createServerToolStripMenuItem.Name = "createServerToolStripMenuItem";
            this.createServerToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.createServerToolStripMenuItem.Text = "Create Server";
            this.createServerToolStripMenuItem.Click += new System.EventHandler(this.CreateServerMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.aboutToolStripMenuItem.Text = "&About...";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.HelpAboutMenuItem_Click);
            // 
            // HostNameTextBox
            // 
            this.HostNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.HostNameTextBox.Location = new System.Drawing.Point(12, 40);
            this.HostNameTextBox.Name = "HostNameTextBox";
            this.HostNameTextBox.Size = new System.Drawing.Size(185, 20);
            this.HostNameTextBox.TabIndex = 12;
            this.HostNameTextBox.TextChanged += new System.EventHandler(this.HostParameterTextBox_TextChanged);
            this.HostNameTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ConnectionParameterBox_KeyDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(163, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Who do you want to connect to?";
            // 
            // HostSelectionList
            // 
            this.HostSelectionList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.HostSelectionList.FormattingEnabled = true;
            this.HostSelectionList.IntegralHeight = false;
            this.HostSelectionList.Location = new System.Drawing.Point(12, 67);
            this.HostSelectionList.Name = "HostSelectionList";
            this.HostSelectionList.ScrollAlwaysVisible = true;
            this.HostSelectionList.Size = new System.Drawing.Size(260, 131);
            this.HostSelectionList.TabIndex = 14;
            this.HostSelectionList.SelectedValueChanged += new System.EventHandler(this.HostSelectionList_SelectedValueChanged);
            this.HostSelectionList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ConnectionParameterBox_KeyDown);
            // 
            // ConnectButton
            // 
            this.ConnectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ConnectButton.Location = new System.Drawing.Point(197, 204);
            this.ConnectButton.Name = "ConnectButton";
            this.ConnectButton.Size = new System.Drawing.Size(75, 23);
            this.ConnectButton.TabIndex = 15;
            this.ConnectButton.Text = "&Connect";
            this.ConnectButton.UseVisualStyleBackColor = true;
            this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_Click);
            // 
            // HostPortTextBox
            // 
            this.HostPortTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.HostPortTextBox.Location = new System.Drawing.Point(203, 40);
            this.HostPortTextBox.MaxLength = 6;
            this.HostPortTextBox.Name = "HostPortTextBox";
            this.HostPortTextBox.Size = new System.Drawing.Size(69, 20);
            this.HostPortTextBox.TabIndex = 16;
            this.HostPortTextBox.TextChanged += new System.EventHandler(this.HostParameterTextBox_TextChanged);
            this.HostPortTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ConnectionParameterBox_KeyDown);
            // 
            // FavoriteToggleButton
            // 
            this.FavoriteToggleButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.FavoriteToggleButton.Location = new System.Drawing.Point(116, 204);
            this.FavoriteToggleButton.Name = "FavoriteToggleButton";
            this.FavoriteToggleButton.Size = new System.Drawing.Size(75, 23);
            this.FavoriteToggleButton.TabIndex = 17;
            this.FavoriteToggleButton.Text = "F&avorite";
            this.FavoriteToggleButton.UseVisualStyleBackColor = true;
            this.FavoriteToggleButton.Click += new System.EventHandler(this.FavoriteToggleButton_Click);
            // 
            // RootForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 239);
            this.Controls.Add(this.FavoriteToggleButton);
            this.Controls.Add(this.HostPortTextBox);
            this.Controls.Add(this.ConnectButton);
            this.Controls.Add(this.HostSelectionList);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.HostNameTextBox);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(300, 250);
            this.Name = "RootForm";
            this.Text = "Js ChatterBox Version 0.2";
            this.Activated += new System.EventHandler(this.RootForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RootForm_FormClosing);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem advancedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createChatWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createServerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.TextBox HostNameTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox HostSelectionList;
        private System.Windows.Forms.Button ConnectButton;
        private System.Windows.Forms.TextBox HostPortTextBox;
        private System.Windows.Forms.Button FavoriteToggleButton;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearRecentToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    }
}