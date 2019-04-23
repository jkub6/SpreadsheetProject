using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientGui
{
    public partial class OpenNetworkFileForm : Form
    {
        public string selectedSpreadsheet = "";
        public string username = "";
        public string password = "";
        public bool canceled = false;

        public OpenNetworkFileForm()
        {
            InitializeComponent();
        }

        public void UpdateList(object sender, List<string> sheets)
        {
            System.Object[] ItemObject = new System.Object[sheets.Count];
            for (int i = 0; i <= sheets.Count-1; i++)
            {
                ItemObject[i] = sheets.ElementAt(i);
            }

            MethodInvoker methodInvokerDelegate = delegate () { listBox1.Items.Clear(); listBox1.Items.AddRange(ItemObject); };

            if (InvokeRequired)
                Invoke(methodInvokerDelegate);
            else
                methodInvokerDelegate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count == 1)
            {
                selectedSpreadsheet = listBox1.SelectedItem.ToString();
                username = usernameTextBox.Text.ToString();
                password = passwordTextBox.Text.ToString();

                canceled = false;

                Close();
            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string fileName = filenameTextBox.Text;

            if (fileName == "") { } //no filename
            else if (listBox1.Items.Contains(fileName))
                MessageBox.Show("Spreadsheet " + fileName + " already exists. (Nothing has been changed)");
            else
            {
                selectedSpreadsheet = fileName;
                username = usernameTextBox.Text.ToString();
                password = passwordTextBox.Text.ToString();

                canceled = false;

                Close();
            }
        }

        private void OpenNetworkFileForm_Load(object sender, EventArgs e)
        {
            canceled = true;
        }

        private void OpenNetworkFileForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }
    }
}
