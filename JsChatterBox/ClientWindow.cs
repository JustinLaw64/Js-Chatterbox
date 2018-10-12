using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JsChatterBoxNetworking;
using JsChatterBoxNetworking.Implementations;

namespace JsChatterBox
{
    public partial class ClientWindow : Form
    {
        public PeerConnection Connection { get { return _c; } }
        public bool OwnsConnection = false; // Can I automatically dispose the connection?

        public ClientWindow(PeerConnection Connection)
        {
            InitializeComponent();

            _TitleBarBaseName = this.Text;
            _c = Connection;
            _c.OnHumanLogOutput += LogMessage;
            FormUpdateTimer.Start();
            UpdateGeneralControls();
        }
        public ClientWindow() : this(new PeerConnection(new PeerIdentity(0, Program.DataManager.UserName)))
        {
            OwnsConnection = true;
        }
        public ClientWindow(String Hostname, int Port) : this()
        {
            _c.BeginConnect(Hostname, Port);
        }

        private PeerConnection _c;
        private String _TitleBarBaseName;

        private void LogMessage(PeerConnection Sender, String Message)
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
        
        private void UpdatePeopleList()
        {
            Dictionary<int, PeerIdentity> people = _c.GetGuestList();
            int peopleLength = people.Count;
            String[] NewLines = new String[peopleLength];
            int i = -1;
            foreach (var pair in people)
            {
                i++;
                NewLines[i] = String.Concat(pair.Value.Name, " (", pair.Key, ")");
            }
            PeopleConnectedListBox.Lines = NewLines;
        }
        private void UpdateGeneralControls()
        {
            PeerConnection c = _c;
            bool connected = c.IsConnected;

            SendMesageButton.Enabled = connected;
            MainSplitContainer.Panel1Collapsed = !(c.OtherPeerID.HasValue ? (c.OtherPeerID.Value.PeerType == 1) : false);

            String NewTitleBarName = String.Concat(
                _TitleBarBaseName, " - ", 
                (connected ? c.OtherPeerDisplayName : "Not Connected"));
            this.Text = NewTitleBarName;
        }

        // Form Events
        private void SendMesageButton_Click(object sender, EventArgs e) { SendMessageCommand(); }
        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) SendMessageCommand(); }
        private void ClearLogMenuItem_Click(object sender, EventArgs e) { ClearLog(); }
        private void HelpAboutMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox NewWindow = new AboutBox();
            NewWindow.ShowDialog(this);
        }
        private void FormUpdateTimer_Tick(object sender, EventArgs e)
        {
            _c.RunCycle(FormUpdateTimer.Interval / 1000f);

            UpdatePeopleList();
            UpdateGeneralControls();
        }
        private void ChatForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _c.OnHumanLogOutput -= LogMessage;
            if (OwnsConnection)
                _c.Dispose();
            _c = null;
        }
    }
}
