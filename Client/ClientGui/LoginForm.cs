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
    public partial class LoginForm : Form
    {
        public string host;
        public int port;
        public string username;
        public string password;
        public Client.Client client;
        public bool connected = false;

        public LoginForm()
        {
            InitializeComponent();   
        }

        public LoginForm(Client.Client client) : this()
        {
            this.client = client;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            host = hostTextBox.Text;
            port = int.Parse(portTextBox.Text);
            username = usernameTextBox.Text;
            password = passwordTextBox.Text;

            BackgroundWorker Worker = new BackgroundWorker();
            Worker.RunWorkerCompleted += Done;
            Worker.DoWork += Connect;

            Worker.RunWorkerAsync();
            Application.UseWaitCursor = true;
        }

        private void Connect(object sender, DoWorkEventArgs e)
        {
            try
            {
                client.Connect(host, port);
                connected = true;
                //try password next
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Error connecting to \"" + host + "\" at port " + port.ToString());
            }
        }

        private void Done(object sender, RunWorkerCompletedEventArgs e)
        {
            Application.UseWaitCursor = false;
            if (connected)
                Close();
        }
    }
}
