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
    public partial class RootForm : Form
    {
        public RootForm()
        {
            InitializeComponent();

            HostList = Program.DataManager.HostList;
            HostPortTextBox.Text = NetworkConstants.DefaultServerPort.ToString();
            UpdateHostListControl();
            UpdateButtons();
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
        private void CreateClientCommand()
        {
            ClientWindow NewWindow = new ClientWindow();
            NewWindow.Show();
        }
        private void CreateServerCommand()
        {
            ServerWindow NewWindow = new ServerWindow();
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

        private void ExitMenuItem_Click(object sender, EventArgs e) { this.Close(); }
        private void CreateClientMenuItem_Click(object sender, EventArgs e) { CreateClientCommand(); }
        private void CreateServerMenuItem_Click(object sender, EventArgs e) { CreateServerCommand(); }
        private void HelpAboutMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox NewWindow = new AboutBox();
            NewWindow.ShowDialog(this);
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
                HostPortTextBox.Text = NetworkConstants.DefaultServerPort.ToString();
                HostNameTextBox.Text = "";
            }
        }

        private void RootForm_FormClosing(object sender, FormClosingEventArgs e) { Program.DataManager.Save(); }

        private void ClearRecentMenuItem_Click(object sender, EventArgs e)
        {
            HostList.RecentHosts.Clear();
            UpdateHostListControl();
        }

        private void RootForm_Activated(object sender, EventArgs e) { UpdateHostListControl(); }
    }
}
