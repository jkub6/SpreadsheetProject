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
    public partial class NetworkConsoleForm : Form
    {
        public event EventHandler<string> SendingText;

        public NetworkConsoleForm()
        {
            InitializeComponent();
        }

        public void SetConnectedState(bool state)
        {
            MethodInvoker methodInvokerDelegate = delegate () { sendButton.Enabled = state; };

            if (InvokeRequired)
                Invoke(methodInvokerDelegate);
            else
                methodInvokerDelegate();
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            sendText(textBox2.Text);
            textBox2.Clear();
        }

        public void sendText(string text)
        {
            if (newLineCheckbox.Checked)
                text = text.Replace("\\n", "\n");

            SendingText?.Invoke(this, text); 
        }

        public void NetworkMessageSending(object sender, string text)
        {
            if (inOutCheckbox.Checked)
                text = "<-" + text;
            if (showLineCheckbox.Checked)
                text = text.Replace("\n", "\r\n");

            MethodInvoker methodInvokerDelegate = delegate () { textBox1.AppendText(text); };
            if (InvokeRequired)
                Invoke(methodInvokerDelegate);
            else
                methodInvokerDelegate();
        }

        public void getText(object sender, string text)
        {
            if (inOutCheckbox.Checked)
                text = "->" + text;
            if (showLineCheckbox.Checked)
                text = text.Replace("\n", "\r\n");

            MethodInvoker methodInvokerDelegate = delegate () { textBox1.AppendText(text); };

            if (InvokeRequired)
                Invoke(methodInvokerDelegate);
            else
                methodInvokerDelegate();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (autoScrollCheckbox.Checked)
            {
                textBox1.SelectionStart = textBox1.Text.Length;
                textBox1.ScrollToCaret();
                textBox1.Refresh();
            }
        }
    }
}
