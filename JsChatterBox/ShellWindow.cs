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
using JsChatterBox;

namespace JsChatterBox
{
    public partial class ShellWindow : Form
    {
        public ShellWindow()
        {
            InitializeComponent();
            
            _c = new ClientShell();
            FormUpdateTimer.Start();
            UpdateGeneralControls();
        }

        private ClientShell _c;
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
            String t = MessageTextBox.Text;
            MessageTextBox.Text = "";
            _c.IssueCommand(t);
        }

        private void UpdateGeneralControls()
        {
            foreach (string item in _c.Log_CollectOutput())
                LogMessage(item);
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
            
            UpdateGeneralControls();
        }
        private void ChatForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _c.Dispose();
            _c = null;
        }
    }
}
