using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JsChatterBox
{
    public partial class ConnectionManagerWindow : Form
    {
        public ConnectionManagerWindow(ChatClient ClientInstance)
        {
            InitializeComponent();

            _ClientInstance = ClientInstance;
        }
        private void ConnectionManagerWindow_Load(object sender, EventArgs e)
        {
            ClientNameTextBox.Text = _ClientInstance.ClientID.Name;
            HostAddressBox.Text = _ClientInstance.ConnectedHostName;
            UpdateConnectionControls();
        }

        private ChatClient _ClientInstance;

        private void ChangeNameCommand()
        {
            String NewName = ClientNameTextBox.Text;
            if (NewName != _ClientInstance.ClientID.Name)
                _ClientInstance.ChangeName(NewName);
        }
        private void ConnectCommand() { _ClientInstance.BeginConnect(HostAddressBox.Text, NetworkConstants.DefaultServerPort); UpdateConnectionControls(); }
        private void DisconnectCommand() { _ClientInstance.BeginDisconnect(); UpdateConnectionControls(); }

        private void UpdateConnectionControls()
        {
            bool connected = _ClientInstance.IsConnected;

            ConnectButton.Enabled = !connected;
            DisconnectButton.Enabled = connected;
            HostAddressBox.ReadOnly = connected;
        }

        private void ClientNameChangeButton_Click(object sender, EventArgs e) { ChangeNameCommand(); }
        private void ClientNameTextBox_KeyDown(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) ChangeNameCommand(); }
        private void ConnectButton_Click(object sender, EventArgs e) { ConnectCommand(); }
        private void DisconnectButton_Click(object sender, EventArgs e) { DisconnectCommand(); }
        private void HostAddressBox_KeyDown(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter && !HostAddressBox.ReadOnly) ConnectCommand(); }

        private void CloseButton_Click(object sender, EventArgs e) { this.Close(); }
    }
}
