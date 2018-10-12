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
    public partial class ServerWindow : Form
    {
        public ChatServer ServerInstance { get { return _ServerInstance; } }

        public ServerWindow()
        {
            InitializeComponent();

            this._ServerInstance = new ChatServer();
            _ServerInstance.OnLineOutput += LogMessage;

            FormUpdateTimer.Start();
            UpdateClientListBox();
        }

        private ChatServer _ServerInstance;

        private void LogMessage(String Message)
        {
            List<String> LogLines = new List<String>(LogTextBox.Lines);
            LogLines.Insert(0, Message);
            LogTextBox.Lines = LogLines.ToArray();
        }
        private void ClearLog()
        {
            LogTextBox.Lines = new String[0];
        }
        private void ShutdownServer()
        {
            _ServerInstance.OnLineOutput -= LogMessage;
            _ServerInstance.Dispose();
            FormUpdateTimer.Stop();
        }
        private void UpdateClientListBox()
        {
        //    GuestInfo[] guests = _ServerInstance.GetGuests();
        //    int guestsL = guests.Length;
        //    String[] lines = new String[guestsL];
        //    for (int i = 0; i < guestsL; i++)
        //    {
        //        GuestInfo guest = guests[i];
        //        lines[i] = String.Concat(guest.Name, " (ID: ", guest.GuestId, ")");
        //    }
        //    PeopleConnectedListBox.Lines = lines;


            PeopleConnectedListBox.Lines = _ServerInstance.GetGuestList();
        }

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
            Program.OpenChatConnection("127.0.0.1", NetworkConstants.DefaultServerPort);
        }
    }
}
