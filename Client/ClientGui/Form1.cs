using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SS;
using SpreadsheetUtilities;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Net.NetworkInformation;


namespace ClientGui
{
    public partial class Form1 : Form
    {
        Client.Client client;

        /// <summary>
        /// Keeps track of the current column 
        /// </summary>
        private int column = 0;
        private int row = 0;
        //private Spreadsheet spreadsheet;
        private string fileName;

        private bool connected = false;

        private NetworkConsoleForm networkConsoleForm;
        private OpenNetworkFileForm openNetworkFileForm;

        /// <summary>
        /// This constructor initializes a form as well as allows command line arguements 
        /// and run it as an executable. This also updates the cells in the new form if 
        /// a save file is loaded. 
        /// </summary>
        public Form1()
        {
            client = new Client.Client();
            client.PingCompleted += PingCompletedCallback;

            //networkConsoleForm.Show();
            //networkConsoleForm.Visible = false;
            client.FullSendRecieved += FullSendRecieved;
            client.ErrorRecieved += ErrorRecieved;


            InitializeComponent();

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length >= 2)
            {
                client.spreadsheet = new Spreadsheet(args[1], s => true, s => s.ToUpper(), "ps6");
                fileName = Path.GetFileName(args[1]);
            }
            else
            {
                client.spreadsheet = new Spreadsheet(s => true, s => s.ToUpper(), "ps6");
                fileName = "Untitled.sprd";
            }

            Text = fileName;

            foreach (string cellName in client.spreadsheet.GetNamesOfAllNonemptyCells())
                UpdateCellByName(cellName);

            SelectCell();
            spreadsheetPanel.SelectionChanged += CellSelectedEvent;

            openNetworkFileForm = new OpenNetworkFileForm();
            client.SpreadsheetsRecieved += openNetworkFileForm.UpdateList;
        }

        /// <summary>
        /// Updates the current coordinates of the spreadsheet. 
        /// </summary>
        private void UpdateCoordinates()
        {
            spreadsheetPanel.GetSelection(out column, out row);
        }

        /// <summary>
        /// Submits text when the selection is changed and executes the function 
        /// that refocuses.
        /// </summary>
        /// <param name="sender"></param>
        private void CellSelectedEvent(object sender)
        {
            SubmitText();
            SelectCell();
        }

        /// <summary>
        /// The select cell function finds the current cell and column and 
        /// focuses in on it, as well as updates the coordinates for other functions to use. 
        /// It wlso Sets the Cell name, content, and appends the contents if the contents are a
        /// formula to contain an equals sign. 
        /// </summary>
        private void SelectCell()
        {
            spreadsheetPanel.GetSelection(out int columnNew, out int rowNew);
            cellContentBox.Focus();

            UpdateCoordinates();
            String cellName = GetSelectedCellName();
            cellNameBox.Text = cellName;
            object cellContents = client.spreadsheet.GetCellContents(cellName);
            cellContentBox.Text = client.spreadsheet.GetCellContents(cellName).ToString();

            if (cellContents is Formula f)
                cellContentBox.Text = "=" + cellContentBox.Text;

            cellValueBox.Text = client.spreadsheet.GetCellValue(cellName).ToString();

            cellContentBox.SelectAll();
        }

        /// <summary>
        /// Converst the column and row to the variable name
        /// </summary>
        /// <returns></returns>
        private string GetSelectedCellName()
        {
            return ((char)(column + 65)) + (row + 1).ToString();
        }

        /// <summary>
        /// This adds functionality to the enter, tab and arrow keys. 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            spreadsheetPanel.GetSelection(out int column, out int row);

            switch (keyData)
            {
                //Shifts selection right when Tab/Right is pressed
                case Keys.Tab:
                    //case Keys.Right:
                    SubmitText();
                    spreadsheetPanel.SetSelection((column + 1) % 26, row);
                    SelectCell();
                    return true;
                //Shifts selection left when Shift Tab/Left is pressed
                case Keys.Tab | Keys.Shift:
                    //case Keys.Left:
                    SubmitText();
                    spreadsheetPanel.SetSelection((column - 1) % 26, row);
                    SelectCell();
                    return true;
                //Shifts selection down when Enter/Down is pressed
                case Keys.Enter:
                case Keys.Down:
                    SubmitText();
                    spreadsheetPanel.SetSelection(column, (row + 1) % 99);
                    SelectCell();
                    return true;
                //Shifts selection up when Shift Enter/Up is pressed
                case Keys.Enter | Keys.Shift:
                case Keys.Up:
                    SubmitText();
                    spreadsheetPanel.SetSelection(column, (row - 1) % 99);
                    SelectCell();
                    return true;

            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// This event occurs when the entry button is clicked. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EntryButtonClickEvent(object sender, EventArgs e)
        {
            SubmitText();
        }

        /// <summary>
        /// Calculates values of cells. 
        /// This function catches exceptions when calculating values of cells. 
        /// Added Functionality: Asterisk when unsaved data is present. 
        /// </summary>
        private void SubmitText()
        {
            string cellName = GetSelectedCellName();

            string contents = client.spreadsheet.GetCellContents(cellName).ToString();
            if (client.spreadsheet.GetCellContents(cellName) is Formula f)
                contents = "=" + contents;
            if (contents == cellContentBox.Text)
            {
                UpdateCellByName(cellName);
                return;
            }


            if (connected)
            {
                try
                {
                    client.SendEdit(cellName, cellContentBox.Text);
                    UpdateCellByName(cellName); //cross out to keep temp value
                                                //return;
                }
                catch (System.IO.IOException)
                {
                    MessageBox.Show("Server terminated Connection Unexpectedly");
                    Logout();
                }

            }

            try
            {
                ISet<string> toUpdate = client.spreadsheet.SetContentsOfCell(cellName, cellContentBox.Text);

                foreach (string name in toUpdate)
                    UpdateCellByName(name);

                spreadsheetPanel.GetValue(column, row, out string t);
                cellValueBox.Text = t;

                if (client.spreadsheet.Changed)
                    Text = "*" + fileName;
            }
            catch (FormulaFormatException e)
            {
                MessageBox.Show(e.Message, "Error in Formula");
            }
            catch (CircularException)
            {
                MessageBox.Show("There was a circular dependency in this formula. Try changing a variable", "Circular Exception Error");
            }
        }

        /// <summary>
        /// Updates cell by the name 
        /// Allows for use of GetListofNonEmpty cells from 
        /// spreadsheet to update all cell values resulting from changes. 
        /// </summary>
        /// <param InputString="name"></param>
        private void UpdateCellByName(string name, string tempText = null)
        {
            //Converts the column and row to the Cell name
            int col = name[0] - 65;
            int row = int.Parse(name.Substring(1).ToString()) - 1;

            object value = client.spreadsheet.GetCellValue(name);
            string text = value.ToString();
            if (value is FormulaError f)
                text = f.Reason;

            if (tempText != null)
                text = tempText;

            spreadsheetPanel.SetValue(col, row, text);

            if (GetSelectedCellName() == name && tempText == null)
            {
                object o = client.spreadsheet.GetCellContents(name);

                text = o.ToString();
                if (o is Formula formula)
                    text = "=" + text;

                SetCellContentBox(text);
            }
        }

        void SetCellContentBox(string text)
        {
            MethodInvoker methodInvokerDelegate = delegate ()
            { cellContentBox.Text = text; };

            //This will be true if Current thread is not UI thread.
            if (InvokeRequired)
                Invoke(methodInvokerDelegate);
            else
                methodInvokerDelegate();
        }

        /// <summary>
        /// Displays how to use Spreadsheet 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 m = new AboutBox1();
            m.Show();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (client.spreadsheet.Changed && !connected)
            {
                MessageBoxButtons buttons = MessageBoxButtons.YesNoCancel;
                var result = MessageBox.Show("You have unsaved data. Save file before closing?", "Unsaved Data", buttons);
                if (result == System.Windows.Forms.DialogResult.Cancel)
                    e.Cancel = true;
                else if (result == System.Windows.Forms.DialogResult.Yes)
                    if (!SaveFile())
                        e.Cancel = true;
            }
        }

        /// <summary>
        /// Event for the Exit button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /// <summary>
        /// Saves button event to save the file. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        /// <summary>
        /// Returns true if Saved. Catches exceptions if there is an 
        /// error saving. 
        /// </summary>
        /// <returns></returns>
        private bool SaveFile()
        {
            SaveFileDialog s = new SaveFileDialog();
            try
            {
                s.Filter = "Spreadsheet Files (*.sprd)|*.sprd|All files (*.*)|*.*";
                s.FileName = fileName;
                //s.OverwritePrompt = false;
                if (s.ShowDialog() == DialogResult.OK)
                {
                    client.spreadsheet.Save(s.FileName);
                    fileName = Path.GetFileName(s.FileName);
                    Text = fileName;
                    playSimpleSound();
                    return true;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error Loading");
            }
            finally
            {
                s.Dispose();
            }
            return false;
        }
        /// <summary>
        /// Opens a new application with the New buttons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Application.ExecutablePath);
        }
        /// <summary>
        /// Uses the Open file button to open a new file
        /// If anything goes wrong a message will be shown. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            try
            {
                openFileDialog.Filter = "Spreadsheet Files (*.sprd)|*.sprd|All files (*.*)|*.*";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                    Process.Start(Application.ExecutablePath, openFileDialog.FileName);
            }
            catch (Exception z)
            {
                MessageBox.Show(z.Message, "Error Opening File!");
            }
            finally
            {
                openFileDialog.Dispose();
            }
        }

        /// <summary>
        /// Changed the color 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeHighlightColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog c = new ColorDialog();
            if (c.ShowDialog() == DialogResult.OK)
                spreadsheetPanel.BackColor = c.Color;
        }

        /// <summary>
        /// Clears all cells
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearAllCellsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Spreadsheet old = client.spreadsheet;

            List<string> cells = new List<string>(client.spreadsheet.GetNamesOfAllNonemptyCells());

            foreach (string cellName in cells)
            {
                client.spreadsheet.SetContentsOfCell(cellName, "");

                UpdateCellByName(cellName);

                if (connected)
                {
                    try
                    {
                        client.SendEdit(cellName, cellContentBox.Text);
                        UpdateCellByName(cellName); //cross out to keep temp value
                                                    //return;
                    }
                    catch (System.IO.IOException)
                    {
                        MessageBox.Show("Server terminated Connection Unexpectedly");
                        Logout();
                    }
                }
            }

            SelectCell();
        }

        /// <summary>
        /// Makes a sound when you save successfully!
        /// </summary>
        private void playSimpleSound()
        {
            SoundPlayer audio = new SoundPlayer(global::ClientGui.Properties.Resources._1_person_cheering_Jett_Rifkin_1851518140);
            audio.Play();
        }

        private void cellContentBox_TextChanged(object sender, EventArgs e)
        {
            UpdateCellByName(GetSelectedCellName(), cellContentBox.Text);
        }

        //edit buttons
        private void cutToolStripMenuItem_Click(object sender, EventArgs e) => cellContentBox.Cut();
        private void copySelectedTextToolStripMenuItem_Click(object sender, EventArgs e) => cellContentBox.Copy();
        private void pasteToolStripMenuItem_Click(object sender, EventArgs e) => cellContentBox.Paste();
        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e) => cellContentBox.SelectAll();
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e) => cellContentBox.SelectedText = "";

        private void fullScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool fullscreen = (FormBorderStyle == System.Windows.Forms.FormBorderStyle.None);
            if (fullscreen)
            {
                WindowState = FormWindowState.Normal;
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            }
            else
            {
                WindowState = FormWindowState.Maximized;
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            }
        }

        private void newNetworkFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenNetworkFile();
        }

        private void OpenNetworkFile()
        {
            if (!connected)
            {
                ConnectForm loginForm = new ConnectForm(client);
                loginForm.ShowDialog();

                if (!loginForm.connected)
                    return; //return if connection failed.

                //Now connected
                connected = true;
                pingLabel.Visible = true;
                undoNetworkToolStripMenuItem.Enabled = true;
                revertNetworkToolStripMenuItem.Enabled = true;
                logoutToolStripMenuItem.Enabled = true;
            }



            Login();
        }

        private void Login()
        {
            openNetworkFileForm.ShowDialog();

            if (openNetworkFileForm.canceled)
                return;


            string spreadsheet = openNetworkFileForm.selectedSpreadsheet;
            string username = openNetworkFileForm.username;
            string password = openNetworkFileForm.password;

            try
            {
                client.SendOpen(spreadsheet, username, password);
                //save unsaved first
                client.spreadsheet = new Spreadsheet();
            }
            catch (System.IO.IOException)
            {
                MessageBox.Show("Server terminated Connection Unexpectedly");
                Logout();
            }



        }

        private void openNetworkFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenNetworkFile();
        }

        private void PingCompletedCallback(object sender, PingCompletedEventArgs e)
        {
            MethodInvoker methodInvokerDelegate = delegate ()
            { pingLabel.Text = "Ping: " + e.Reply.RoundtripTime + "ms"; };

            //This will be true if Current thread is not UI thread.
            if (InvokeRequired)
                Invoke(methodInvokerDelegate);
            else
                methodInvokerDelegate();
        }

        private void networkConsoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            networkConsoleForm = new NetworkConsoleForm();
            client.NetworkMessageRecieved += networkConsoleForm.getText;
            networkConsoleForm.SendingText += SendText;
            client.SendingText += networkConsoleForm.NetworkMessageSending;
            networkConsoleForm.Show();
        }

        private void undoNetworkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                client.SendUndo();
            }
            catch (System.IO.IOException)
            {
                MessageBox.Show("Server terminated Connection Unexpectedly");
                Logout();
            }

        }

        private void revertNetworkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                client.SendRevert(GetSelectedCellName());
            }
            catch (System.IO.IOException)
            {
                MessageBox.Show("Server terminated Connection Unexpectedly");
                Logout();
            }

        }

        private void SendText(object sender, string text)
        {
            try
            {
                client.SendNetworkMessage(text);
            }
            catch (System.IO.IOException)
            {
                MessageBox.Show("Server terminated Connection Unexpectedly");
                Logout();
            }

        }

        private void FullSendRecieved(object sender, List<string> updatedCells)
        {

            foreach (string cellName in client.spreadsheet.GetNamesOfAllNonemptyCells())
                UpdateCellByName(cellName);
                
        }

        private void ErrorRecieved(object sender, int errorNum)
        {
            if (errorNum == 1)
            {
                MessageBox.Show("Error logging in, incorrect credentials.");
                Login();
            }
            else if (errorNum == 2)
            {
                MessageBox.Show("Circular Error");
            }
        }

        private void serverAdminToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProcessStartInfo sInfo = new ProcessStartInfo("http://gotorocksadmin.tk");
            Process.Start(sInfo);
        }

        private void logoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logout();
        }

        private void Logout()
        {
            client.Disconnect();
            connected = false;
            pingLabel.Visible = false;
            undoNetworkToolStripMenuItem.Enabled = false;
            revertNetworkToolStripMenuItem.Enabled = false;
            logoutToolStripMenuItem.Enabled = false;
        }
    }
}