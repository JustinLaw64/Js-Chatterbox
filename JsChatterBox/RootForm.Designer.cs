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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RootForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.configurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearRecentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.HostNameTextBox = new System.Windows.Forms.TextBox();
            this.ConnectButton = new System.Windows.Forms.Button();
            this.HostPortTextBox = new System.Windows.Forms.TextBox();
            this.FavoriteToggleButton = new System.Windows.Forms.Button();
            this.FormUpdateTimer = new System.Windows.Forms.Timer(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.HostSelectionList = new System.Windows.Forms.ListBox();
            this.RequestAnswerer_ListenCheckBox = new System.Windows.Forms.CheckBox();
            this.RequestAnswerer_AnswerButton = new System.Windows.Forms.Button();
            this.RequestAnswerer_ListBox = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.createServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.createServerToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(384, 24);
            this.menuStrip1.TabIndex = 11;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.configurationToolStripMenuItem,
            this.clearRecentToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // configurationToolStripMenuItem
            // 
            this.configurationToolStripMenuItem.Name = "configurationToolStripMenuItem";
            this.configurationToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.configurationToolStripMenuItem.Text = "&Configuration...";
            this.configurationToolStripMenuItem.Click += new System.EventHandler(this.ConfigurationMenuItem_Click);
            // 
            // clearRecentToolStripMenuItem
            // 
            this.clearRecentToolStripMenuItem.Name = "clearRecentToolStripMenuItem";
            this.clearRecentToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.clearRecentToolStripMenuItem.Text = "Clear &Recent";
            this.clearRecentToolStripMenuItem.Click += new System.EventHandler(this.ClearRecentMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(154, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitMenuItem_Click);
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
            this.HostNameTextBox.Location = new System.Drawing.Point(12, 16);
            this.HostNameTextBox.Name = "HostNameTextBox";
            this.HostNameTextBox.Size = new System.Drawing.Size(136, 20);
            this.HostNameTextBox.TabIndex = 12;
            this.HostNameTextBox.TextChanged += new System.EventHandler(this.HostParameterTextBox_TextChanged);
            this.HostNameTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ConnectionParameterBox_KeyDown);
            // 
            // ConnectButton
            // 
            this.ConnectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ConnectButton.Location = new System.Drawing.Point(143, 152);
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
            this.HostPortTextBox.Location = new System.Drawing.Point(154, 16);
            this.HostPortTextBox.MaxLength = 6;
            this.HostPortTextBox.Name = "HostPortTextBox";
            this.HostPortTextBox.Size = new System.Drawing.Size(64, 20);
            this.HostPortTextBox.TabIndex = 16;
            this.HostPortTextBox.TextChanged += new System.EventHandler(this.HostParameterTextBox_TextChanged);
            this.HostPortTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ConnectionParameterBox_KeyDown);
            // 
            // FavoriteToggleButton
            // 
            this.FavoriteToggleButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.FavoriteToggleButton.Location = new System.Drawing.Point(62, 152);
            this.FavoriteToggleButton.Name = "FavoriteToggleButton";
            this.FavoriteToggleButton.Size = new System.Drawing.Size(75, 23);
            this.FavoriteToggleButton.TabIndex = 17;
            this.FavoriteToggleButton.Text = "F&avorite";
            this.FavoriteToggleButton.UseVisualStyleBackColor = true;
            this.FavoriteToggleButton.Click += new System.EventHandler(this.FavoriteToggleButton_Click);
            // 
            // FormUpdateTimer
            // 
            this.FormUpdateTimer.Interval = 500;
            this.FormUpdateTimer.Tick += new System.EventHandler(this.FormUpdateTimer_Tick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(163, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Who do you want to connect to?";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.HostPortTextBox);
            this.splitContainer1.Panel1.Controls.Add(this.FavoriteToggleButton);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.HostSelectionList);
            this.splitContainer1.Panel1.Controls.Add(this.HostNameTextBox);
            this.splitContainer1.Panel1.Controls.Add(this.ConnectButton);
            this.splitContainer1.Panel1MinSize = 200;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.RequestAnswerer_ListenCheckBox);
            this.splitContainer1.Panel2.Controls.Add(this.RequestAnswerer_AnswerButton);
            this.splitContainer1.Panel2.Controls.Add(this.RequestAnswerer_ListBox);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Panel2MinSize = 150;
            this.splitContainer1.Size = new System.Drawing.Size(384, 187);
            this.splitContainer1.SplitterDistance = 221;
            this.splitContainer1.TabIndex = 18;
            // 
            // HostSelectionList
            // 
            this.HostSelectionList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.HostSelectionList.FormattingEnabled = true;
            this.HostSelectionList.IntegralHeight = false;
            this.HostSelectionList.Location = new System.Drawing.Point(12, 42);
            this.HostSelectionList.Name = "HostSelectionList";
            this.HostSelectionList.ScrollAlwaysVisible = true;
            this.HostSelectionList.Size = new System.Drawing.Size(206, 104);
            this.HostSelectionList.TabIndex = 14;
            this.HostSelectionList.SelectedValueChanged += new System.EventHandler(this.HostSelectionList_SelectedValueChanged);
            this.HostSelectionList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ConnectionParameterBox_KeyDown);
            // 
            // RequestAnswerer_ListenCheckBox
            // 
            this.RequestAnswerer_ListenCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.RequestAnswerer_ListenCheckBox.AutoSize = true;
            this.RequestAnswerer_ListenCheckBox.Location = new System.Drawing.Point(3, 156);
            this.RequestAnswerer_ListenCheckBox.Name = "RequestAnswerer_ListenCheckBox";
            this.RequestAnswerer_ListenCheckBox.Size = new System.Drawing.Size(54, 17);
            this.RequestAnswerer_ListenCheckBox.TabIndex = 17;
            this.RequestAnswerer_ListenCheckBox.Text = "&Listen";
            this.RequestAnswerer_ListenCheckBox.UseVisualStyleBackColor = true;
            this.RequestAnswerer_ListenCheckBox.CheckedChanged += new System.EventHandler(this.RequestAnswerer_ListenCheckBox_CheckedChanged);
            // 
            // RequestAnswerer_AnswerButton
            // 
            this.RequestAnswerer_AnswerButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.RequestAnswerer_AnswerButton.Location = new System.Drawing.Point(72, 152);
            this.RequestAnswerer_AnswerButton.Name = "RequestAnswerer_AnswerButton";
            this.RequestAnswerer_AnswerButton.Size = new System.Drawing.Size(75, 23);
            this.RequestAnswerer_AnswerButton.TabIndex = 16;
            this.RequestAnswerer_AnswerButton.Text = "&Answer";
            this.RequestAnswerer_AnswerButton.UseVisualStyleBackColor = true;
            this.RequestAnswerer_AnswerButton.Click += new System.EventHandler(this.RequestAnswerer_AnswerButton_Click);
            // 
            // RequestAnswerer_ListBox
            // 
            this.RequestAnswerer_ListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RequestAnswerer_ListBox.FormattingEnabled = true;
            this.RequestAnswerer_ListBox.IntegralHeight = false;
            this.RequestAnswerer_ListBox.Location = new System.Drawing.Point(3, 16);
            this.RequestAnswerer_ListBox.Name = "RequestAnswerer_ListBox";
            this.RequestAnswerer_ListBox.ScrollAlwaysVisible = true;
            this.RequestAnswerer_ListBox.Size = new System.Drawing.Size(144, 130);
            this.RequestAnswerer_ListBox.TabIndex = 15;
            this.RequestAnswerer_ListBox.SelectedValueChanged += new System.EventHandler(this.RequestAnswerer_ListBox_SelectedValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(123, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Incoming Chat Requests";
            // 
            // createServerToolStripMenuItem
            // 
            this.createServerToolStripMenuItem.Name = "createServerToolStripMenuItem";
            this.createServerToolStripMenuItem.Size = new System.Drawing.Size(88, 20);
            this.createServerToolStripMenuItem.Text = "Create Server";
            this.createServerToolStripMenuItem.Click += new System.EventHandler(this.CreateServerMenuItem_Click);
            // 
            // RootForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 211);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(400, 250);
            this.Name = "RootForm";
            this.Text = "Js ChatterBox Version 0.4";
            this.Activated += new System.EventHandler(this.RootForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RootForm_FormClosing);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.TextBox HostNameTextBox;
        private System.Windows.Forms.Button ConnectButton;
        private System.Windows.Forms.TextBox HostPortTextBox;
        private System.Windows.Forms.Button FavoriteToggleButton;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearRecentToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.Timer FormUpdateTimer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox HostSelectionList;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button RequestAnswerer_AnswerButton;
        private System.Windows.Forms.ListBox RequestAnswerer_ListBox;
        private System.Windows.Forms.CheckBox RequestAnswerer_ListenCheckBox;
        private System.Windows.Forms.ToolStripMenuItem configurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createServerToolStripMenuItem;
    }
}