namespace JsChatterBox
{
    partial class ConnectionManagerWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConnectionManagerWindow));
            this.ConnectButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.DisconnectButton = new System.Windows.Forms.Button();
            this.HostAddressBox = new System.Windows.Forms.TextBox();
            this.ClientNameLabel = new System.Windows.Forms.Label();
            this.ClientNameChangeButton = new System.Windows.Forms.Button();
            this.ClientNameTextBox = new System.Windows.Forms.TextBox();
            this.NoteLabel = new System.Windows.Forms.Label();
            this.CloseButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ConnectButton
            // 
            this.ConnectButton.Location = new System.Drawing.Point(12, 147);
            this.ConnectButton.Name = "ConnectButton";
            this.ConnectButton.Size = new System.Drawing.Size(80, 23);
            this.ConnectButton.TabIndex = 6;
            this.ConnectButton.Text = "C&onnect";
            this.ConnectButton.UseVisualStyleBackColor = true;
            this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 105);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Server HostName:";
            // 
            // DisconnectButton
            // 
            this.DisconnectButton.Location = new System.Drawing.Point(98, 147);
            this.DisconnectButton.Name = "DisconnectButton";
            this.DisconnectButton.Size = new System.Drawing.Size(80, 23);
            this.DisconnectButton.TabIndex = 7;
            this.DisconnectButton.Text = "&Disconnect";
            this.DisconnectButton.UseVisualStyleBackColor = true;
            this.DisconnectButton.Click += new System.EventHandler(this.DisconnectButton_Click);
            // 
            // HostAddressBox
            // 
            this.HostAddressBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.HostAddressBox.Location = new System.Drawing.Point(12, 121);
            this.HostAddressBox.Name = "HostAddressBox";
            this.HostAddressBox.Size = new System.Drawing.Size(260, 20);
            this.HostAddressBox.TabIndex = 5;
            this.HostAddressBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.HostAddressBox_KeyDown);
            // 
            // ClientNameLabel
            // 
            this.ClientNameLabel.AutoSize = true;
            this.ClientNameLabel.Location = new System.Drawing.Point(9, 66);
            this.ClientNameLabel.Name = "ClientNameLabel";
            this.ClientNameLabel.Size = new System.Drawing.Size(63, 13);
            this.ClientNameLabel.TabIndex = 1;
            this.ClientNameLabel.Text = "Your Name:";
            // 
            // ClientNameChangeButton
            // 
            this.ClientNameChangeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ClientNameChangeButton.Location = new System.Drawing.Point(208, 80);
            this.ClientNameChangeButton.Name = "ClientNameChangeButton";
            this.ClientNameChangeButton.Size = new System.Drawing.Size(64, 23);
            this.ClientNameChangeButton.TabIndex = 3;
            this.ClientNameChangeButton.Text = "Change";
            this.ClientNameChangeButton.UseVisualStyleBackColor = true;
            this.ClientNameChangeButton.Click += new System.EventHandler(this.ClientNameChangeButton_Click);
            // 
            // ClientNameTextBox
            // 
            this.ClientNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ClientNameTextBox.Location = new System.Drawing.Point(12, 82);
            this.ClientNameTextBox.Name = "ClientNameTextBox";
            this.ClientNameTextBox.Size = new System.Drawing.Size(190, 20);
            this.ClientNameTextBox.TabIndex = 2;
            this.ClientNameTextBox.Text = "Unnamed";
            this.ClientNameTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ClientNameTextBox_KeyDown);
            // 
            // NoteLabel
            // 
            this.NoteLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NoteLabel.Location = new System.Drawing.Point(9, 9);
            this.NoteLabel.Name = "NoteLabel";
            this.NoteLabel.Size = new System.Drawing.Size(263, 57);
            this.NoteLabel.TabIndex = 0;
            this.NoteLabel.Text = "The connection your client is on is managed here. Make sure the name you\'re using" +
    " is right as others will be able to see it. The server hostname is the address o" +
    "f the server you want to connect to.";
            // 
            // CloseButton
            // 
            this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CloseButton.Location = new System.Drawing.Point(192, 243);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(80, 23);
            this.CloseButton.TabIndex = 9;
            this.CloseButton.Text = "&Close";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // ConnectionManagerWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CloseButton;
            this.ClientSize = new System.Drawing.Size(284, 278);
            this.Controls.Add(this.ClientNameChangeButton);
            this.Controls.Add(this.ClientNameLabel);
            this.Controls.Add(this.NoteLabel);
            this.Controls.Add(this.ConnectButton);
            this.Controls.Add(this.ClientNameTextBox);
            this.Controls.Add(this.HostAddressBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.DisconnectButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(300, 300);
            this.Name = "ConnectionManagerWindow";
            this.Text = "Connection Manager";
            this.Load += new System.EventHandler(this.ConnectionManagerWindow_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ConnectButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button DisconnectButton;
        private System.Windows.Forms.TextBox HostAddressBox;
        private System.Windows.Forms.Label ClientNameLabel;
        private System.Windows.Forms.Button ClientNameChangeButton;
        private System.Windows.Forms.TextBox ClientNameTextBox;
        private System.Windows.Forms.Label NoteLabel;
        private System.Windows.Forms.Button CloseButton;
    }
}