using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JsChatterBox.Networking;

namespace JsChatterBox
{
    public partial class ChatWindow : Form
    {
        public PeerConnection Connection { get { return _c; } }
        public bool OwnsConnection = false; // Can I automatically dispose the connection?

        public ChatWindow(PeerConnection Connection)
        {
            InitializeComponent();

            _TitleBarBaseName = this.Text;
            _c = Connection;
            FormUpdateTimer.Start();
            UpdateGeneralControls();
        }
        public ChatWindow() : this(new PeerConnection(Program.DataManager.UserName)) { OwnsConnection = true; }
        public ChatWindow(String Hostname, int Port) : this()
        {
            _c.BeginConnect(Hostname, Port);
        }

        private PeerConnection _c;
        private String _TitleBarBaseName;

        private void LogMessage(String Message)
        {
            List<String> ChatLogLines = new List<String>(ChatLogTextBox.Lines);
            ChatLogLines.Insert(0, Message);
            ChatLogTextBox.Lines = ChatLogLines.ToArray();
        }
        private void ClearLog() { ChatLogTextBox.Lines = new String[0]; }
        private void SendMessageCommand()
        {
            if (_c.IsConnected)
            {
                String MessageText = MessageTextBox.Text;
                MessageTextBox.Text = "";
                _c.SendHumanMessage(MessageText);
            }
        }
        
        private void UpdateGeneralControls()
        {
            PeerConnection c = _c;
            bool connected = c.IsConnected;

            SendMesageButton.Enabled = connected;

            String NewTitleBarName = String.Concat(
                _TitleBarBaseName, " - ", 
                (connected ? c.OtherPeerDisplayName : "Not Connected"));
            this.Text = NewTitleBarName;
        }

        // Form Events
        private void SendMesageButton_Click(object sender, EventArgs e) { SendMessageCommand(); }
        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) SendMessageCommand(); }
        private void ClearLogMenuItem_Click(object sender, EventArgs e) { ClearLog(); }
        private void DisconnectMenuItem_Click(object sender, EventArgs e) { if (_c.IsConnected) _c.BeginDisconnect(); }
        private void HelpAboutMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox NewWindow = new AboutBox();
            NewWindow.ShowDialog(this);
        }
        private void FormUpdateTimer_Tick(object sender, EventArgs e)
        {
            _c.RunCycle(FormUpdateTimer.Interval / 1000f);
            foreach (string m in _c.Log_CollectOutput())
                LogMessage(m);
            
            UpdateGeneralControls();
        }
        private void ChatForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (OwnsConnection)
                _c.Dispose();
            _c = null;
        }
    }
}
