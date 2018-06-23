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
using JsChatterBox.Networking.Implementations;

namespace JsChatterBox
{
    public partial class RootForm : Form
    {
        public RootForm()
        {
            InitializeComponent();

            HostList = Program.DataManager.HostList;
            HostPortTextBox.Text = Program.DataManager.WorkingPort.ToString();

            RequestAnswerer_Constructor();

            UpdateHostListControl();
            UpdateButtons();

            FormUpdateTimer.Start();
        }
        private void RootForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            FormUpdateTimer.Stop();
            RequestAnswerer_Destructor();

            Program.DataManager.Save();
        }

        private UserHostList HostList;

        private void ConnectCommand()
        {
            String HostName = HostNameTextBox.Text;
            int? Port = PortBoxToNumber();
            if (Port.HasValue)
            {
                if (HostName != "")
                {
                    Program.OpenChatConnection(HostName, Port.Value);
                    UpdateHostListControl();
                }
                else
                    System.Windows.Forms.MessageBox.Show(this, "The HostName field is blank. The computer won't know who to talk to if this is blank.", "Error");
            }
            else
                System.Windows.Forms.MessageBox.Show(this, "The port field is not a number. It also doesn't accept decimals. Use only numbers!", "Error");
        }
        private void CreateServerCommand()
        {
            ServerWindow NewWindow = new ServerWindow(Program.DataManager.WorkingPort);
            NewWindow.Show();
        }
        private void UpdateHostListControl()
        {
            ListBox.ObjectCollection list = HostSelectionList.Items;
            list.Clear();
            list.Add("Favorites:");
            foreach (var item in HostList.FavoriteHosts.ToArray())
                list.Add(item);
            list.Add("Recent:");
            foreach (var item in HostList.RecentHosts.ToArray())
                list.Add(item);
        }
        private void UpdateButtons()
        {
            String HostName = HostNameTextBox.Text;
            int? Port = PortBoxToNumber();
            bool IsValid = HostParamsValid();
            bool IsFavorited = HostParamsFavorited();
            FavoriteToggleButton.Enabled = IsValid;
            FavoriteToggleButton.Text = (IsFavorited ? "Unf&avorite" : "Favorite");
        }
        private int? PortBoxToNumber()
        {
            int r;
            return (Int32.TryParse(HostPortTextBox.Text, out r) ? (int?)r : null);
        }
        private bool HostParamsValid()
        {
            String HostName = HostNameTextBox.Text;
            int? Port = PortBoxToNumber();
            return (Port.HasValue & HostName != "");
        }
        private bool HostParamsFavorited()
        {
            String HostName = HostNameTextBox.Text;
            int? Port = PortBoxToNumber();
            if (HostParamsValid())
            {
                int ExistingIndex = HostList.FavoriteHosts.IndexOf(HostParamsToInfo().Value);
                return (ExistingIndex > -1);
            }
            else
                return false;
        }
        private HostInformation? HostParamsToInfo()
        {
            String HostName = HostNameTextBox.Text;
            int? Port = PortBoxToNumber();
            return (HostParamsValid() ? (HostInformation?)(new HostInformation(HostName,Port.Value)) : null);
        }

        private void ClearRecentMenuItem_Click(object sender, EventArgs e)
        {
            HostList.RecentHosts.Clear();
            UpdateHostListControl();
        }
        private void ExitMenuItem_Click(object sender, EventArgs e) { this.Close(); }
        private void CreateServerMenuItem_Click(object sender, EventArgs e) { CreateServerCommand(); }
        private void HelpAboutMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox NewWindow = new AboutBox();
            NewWindow.ShowDialog(this);
        }
        private void ConfigurationMenuItem_Click(object sender, EventArgs e)
        {
            (new ConfigurationForm()).ShowDialog(this);
            int NewPort = Program.DataManager.WorkingPort;
            if (NewPort != RequestAnswerer_Listener.Port)
            {
                RequestAnswerer_SetState(false, true);
                RequestAnswerer_Listener.Port = NewPort;
            }
            RequestAnswerer_Listener.LocalIdentity = Program.DataManager.GetPeerIdentity();
        }

        private void FavoriteToggleButton_Click(object sender, EventArgs e)
        {
            if (HostParamsValid())
            {
                if (HostParamsFavorited())
                    HostList.FavoriteHosts.Remove(HostParamsToInfo().Value);
                else
                    HostList.FavoriteHosts.Add(HostParamsToInfo().Value);
                UpdateHostListControl();
                UpdateButtons();
            }
        }
        private void ConnectButton_Click(object sender, EventArgs e) { ConnectCommand(); }

        private void ConnectionParameterBox_KeyDown(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) ConnectCommand(); }
        private void HostParameterTextBox_TextChanged(object sender, EventArgs e) { UpdateButtons(); }
        private void HostSelectionList_SelectedValueChanged(object sender, EventArgs e)
        {
            HostInformation? s = (HostSelectionList.SelectedItem as HostInformation?);
            if (s.HasValue)
            {
                HostInformation s2 = s.Value;
                HostNameTextBox.Text = s2.HostName;
                HostPortTextBox.Text = s2.Port.ToString();
            }
            else
            {
                HostPortTextBox.Text = Program.DataManager.WorkingPort.ToString();
                HostNameTextBox.Text = "";
            }
        }

        private void FormUpdateTimer_Tick(object sender, EventArgs e)
        {
            RequestAnswerer_RunCycle(FormUpdateTimer.Interval / 1000f);
        }

        private void RootForm_Activated(object sender, EventArgs e) { UpdateHostListControl(); }

        #region RequestAnswerer

        private void RequestAnswerer_Constructor()
        {
            RequestAnswerer_Listener = new ConnectionRequestListener(Program.DataManager.GetPeerIdentity(), Program.DataManager.WorkingPort);

            RequestAnswerer_UpdateControls();
        }
        private void RequestAnswerer_Destructor()
        {
            RequestAnswerer_Listener.Dispose();
            RequestAnswerer_Listener = null;
        }

        private ConnectionRequestListener RequestAnswerer_Listener;

        private void RequestAnswerer_SetState(bool NewState, bool AffectControls)
        {
            if (NewState)
            {
                try { RequestAnswerer_Listener.Start(); }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(String.Concat("The system could not start listening because of an error. Message: \"", e.Message, "\""));
                    RequestAnswerer_ListenCheckBox.Checked = false;
                }
            }
            else
                RequestAnswerer_Listener.Stop();

            if (AffectControls)
            {
                RequestAnswerer_ListenCheckBox.Checked = NewState;
                RequestAnswerer_UpdateControls();
            }
        }
        private PeerConnection RequestAnswerer_GetSelection()
        {
            return (PeerConnection)RequestAnswerer_ListBox.SelectedItem;
        }
        private bool RequestAnswerer_SelectionValid()
        {
            return RequestAnswerer_GetSelection() != null;
        }

        private void RequestAnswerer_UpdateControls()
        {
            RequestAnswerer_UpdateList();
            RequestAnswerer_UpdateAcceptButton();
        }
        private void RequestAnswerer_UpdateList()
        {
            RequestAnswerer_ListBox.Enabled = RequestAnswerer_Listener.IsActive;

            PeerConnection[] l1, l2;

            l1 = RequestAnswerer_Listener.GetPendingRequests();
            List<PeerConnection> l1l = new List<PeerConnection>(l1);
            ListBox.ObjectCollection l2r = RequestAnswerer_ListBox.Items;
            l2 = new PeerConnection[l2r.Count];

            // Copy l2r to l2.
            for (int i = 0; i < l2r.Count; i++)
                l2[i] = (PeerConnection)l2r[i];

            // Remove
            foreach (PeerConnection item in l2)
                if (!l1l.Contains(item))
                    l2r.Remove(item);

            // Add
            for (int i = 0; i < l1l.Count; i++)
            {
                PeerConnection l1o = l1[i];
                PeerConnection l2o = i <= l2.GetUpperBound(0) ? l2[i] : null;
                if (l1o.GreetingReceived && (l2o == null || l1o != l2o))
                {
                    l2r.Add(l1o);
                    if (i == l1l.Count - 1)
                        RequestAnswerer_ListBox.SelectedItem = l1o;
                }
            }
        }
        private void RequestAnswerer_UpdateAcceptButton() {
            RequestAnswerer_AnswerButton.Enabled = RequestAnswerer_Listener.IsActive && RequestAnswerer_SelectionValid();
        }
        private void RequestAnswerer_RunCycle(float DeltaTime)
        {
            RequestAnswerer_Listener.RunCycle(DeltaTime);
            RequestAnswerer_UpdateControls();
        }

        private void RequestAnswerer_ListenCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            RequestAnswerer_SetState(RequestAnswerer_ListenCheckBox.Checked, false);
        }
        private void RequestAnswerer_AnswerButton_Click(object sender, EventArgs e)
        {
            PeerConnection s = RequestAnswerer_GetSelection();
            if (s != null)
            {
                Program.OpenChatConnection(s);
                RequestAnswerer_Listener.AcceptConnection(s);
            }
        }
        private void RequestAnswerer_ListBox_SelectedValueChanged(object sender, EventArgs e)
        {
            RequestAnswerer_UpdateAcceptButton();
        }

        #endregion
    }
}
