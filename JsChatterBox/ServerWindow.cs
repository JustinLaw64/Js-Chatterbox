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
    public partial class ServerWindow : Form
    {
        public ChatServer ServerInstance { get { return _ServerInstance; } }

        public ServerWindow(int Port)
        {
            InitializeComponent();

            _ServerInstance = new ChatServer(Port);
            _ServerInstance.OnLineOutput += LogMessage;

            FormUpdateTimer.Start();
            UpdateClientListBox();

            _ServerInstance.Start();
        }

        private ChatServer _ServerInstance;

        private void LogMessage(String Message)
        {
            List<String> LogLines = new List<String>(LogTextBox.Lines);
            LogLines.Insert(0, Message);
            LogTextBox.Lines = LogLines.ToArray();
        }
        private void ClearLog() { LogTextBox.Lines = new String[0]; }
        private void ShutdownServer()
        {
            _ServerInstance.OnLineOutput -= LogMessage;
            _ServerInstance.Dispose();
            FormUpdateTimer.Stop();
        }
        private void UpdateClientListBox() { PeopleConnectedListBox.Lines = _ServerInstance.GetGuestList(); }

        private void FormUpdateTimer_Tick(object sender, EventArgs e)
        {
            _ServerInstance.RunCycle(FormUpdateTimer.Interval / 1000f);
            UpdateClientListBox();
        }
        private void ClearLogMenuItem_Click(object sender, EventArgs e) { ClearLog(); }
        private void HelpAboutMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox NewWindow = new AboutBox();
            NewWindow.ShowDialog(this);
        }
        private void ShutdownButton_Click(object sender, EventArgs e) { Close(); }

        private void ServerWindow_FormClosed(object sender, FormClosedEventArgs e) { ShutdownServer(); }

        private void ConnectLocalButton_Click(object sender, EventArgs e)
        {
            Program.OpenChatConnection("127.0.0.1", _ServerInstance.Port);
        }
    }
}
