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
    public partial class ClientWindow : Form
    {
        public ChatClient ClientInstance { get { return _ClientInstance; } }

        public ClientWindow()
        {
            InitializeComponent();

            _ClientInstance = new ChatClient();
            _ClientInstance.OnLineOutput += LogMessage;
            FormUpdateTimer.Start();
            UpdateGeneralControls();
        }

        private ChatClient _ClientInstance;
        private ConnectionManagerWindow _ConnectionWindow = null;

        private void LogMessage(String Message)
        {
            List<String> ChatLogLines = new List<String>(ChatLogTextBox.Lines);
            ChatLogLines.Insert(0, Message);
            ChatLogTextBox.Lines = ChatLogLines.ToArray();
        }
        private void ClearLog() { ChatLogTextBox.Lines = new String[0]; }
        private void SendMessageCommand()
        {
            if (_ClientInstance.IsConnected)
            {
                String MessageText = MessageTextBox.Text;
                MessageTextBox.Text = "";
                _ClientInstance.SendHumanMessage(MessageText);
            }
        }

        private void UpdatePeopleList()
        {
            GuestInfo[] people = _ClientInstance.GetGuestList();
            int peopleLength = people.Length;
            String[] NewLines = new String[peopleLength];
            for (int i = 0; i < peopleLength; i++)
			{
                NewLines[i] = people[i].ToString();
			}
            PeopleConnectedListBox.Lines = NewLines;
        }
        private void UpdateGeneralControls()
        {
            bool connected = _ClientInstance.IsConnected;

            SendMesageButton.Enabled = connected;
        }
        private void RepositionConnectionControlsDialog()
        {
            if (_ConnectionWindow != null)
            {
                Rectangle CurrentScreen = Screen.GetWorkingArea(this);

                _ConnectionWindow.Height = Math.Min(this.Height, CurrentScreen.Height);
                int MaxLeftPosition = CurrentScreen.Right - _ConnectionWindow.Width;
                int MaxTopPosition = CurrentScreen.Bottom - _ConnectionWindow.Height;
                _ConnectionWindow.Left = Math.Min(this.Right, MaxLeftPosition);
                _ConnectionWindow.Top = Math.Max(CurrentScreen.Top, Math.Min(this.Top, MaxTopPosition));
            }
        }

        private void SendMesageButton_Click(object sender, EventArgs e) { SendMessageCommand(); }
        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) SendMessageCommand(); }
        private void ClearLogMenuItem_Click(object sender, EventArgs e) { ClearLog(); }

        private void ConnectionSettingsMenuItem_Click(object sender, EventArgs e)
        {
            if (_ConnectionWindow == null)
            {
                ConnectionManagerWindow NewWindow = new ConnectionManagerWindow(_ClientInstance);
                _ConnectionWindow = NewWindow;
                RepositionConnectionControlsDialog();
                NewWindow.Show(this);
                RepositionConnectionControlsDialog();
                NewWindow.Refresh();

                ConnectionSettingsMenuItem.Enabled = false;
                FormClosedEventHandler eventResponder = null;
                eventResponder = (object sender2, FormClosedEventArgs e2) =>
                {
                    _ConnectionWindow = null;
                    NewWindow.FormClosed -= eventResponder;
                    ConnectionSettingsMenuItem.Enabled = true;
                };
                NewWindow.FormClosed += eventResponder;
            }
        }

        private void FormUpdateTimer_Tick(object sender, EventArgs e)
        {
            _ClientInstance.RunCycle();

            UpdatePeopleList();
            UpdateGeneralControls();
        }
        private void ChatForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _ClientInstance.OnLineOutput -= LogMessage;
            _ClientInstance.Dispose();
            _ClientInstance = null;
        }
    }
}
