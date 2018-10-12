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
    public partial class ConfigurationForm : Form
    {
        public ConfigurationForm()
        {
            InitializeComponent();

            PersistentDataManager DataSave = Program.DataManager;
            MyNameTextBox.Text = DataSave.UserName;
            WorkingPortTextBox.Text = DataSave.WorkingPort.ToString();
        }

        private void AcceptButton_Click(object sender, EventArgs e)
        {
            bool r = false;

            String NewName = MyNameTextBox.Text;
            int NewWorkingPort = 0;

            if (int.TryParse(WorkingPortTextBox.Text, out NewWorkingPort) && (NewWorkingPort >= System.Net.IPEndPoint.MinPort & NewWorkingPort <= System.Net.IPEndPoint.MaxPort))
            {
                if (NewName != "")
                    r = true;
                else
                    MessageBox.Show("You can't use a blank name!");
            }
            else
                MessageBox.Show(String.Concat(
                    "You can only type 5 integer numbers that is more than or equal to ", System.Net.IPEndPoint.MinPort, " and less than or equal to ", System.Net.IPEndPoint.MaxPort, " as a whole into the Working Port box."));

            if (r)
            {
                PersistentDataManager DataSave = Program.DataManager;
                DataSave.UserName = NewName;
                DataSave.WorkingPort = NewWorkingPort;
                DataSave.Save();

                Close();
            }
        }
        private void CancelButton_Click(object sender, EventArgs e) { Close(); }
    }
}
