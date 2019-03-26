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

namespace SpreadsheetGui
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// Keeps track of the current column 
        /// </summary>
        private int column = 0;
        /// <summary>
        /// Keeps track of the current row
        /// </summary>
        private int row = 0;
        /// <summary>
        /// Creates a new spreadsheet
        /// </summary>
        private Spreadsheet spreadsheet;
        /// <summary>
        /// Keeps track of the current or future file name. 
        /// </summary>
        private string fileName;
        /// <summary>
        /// This constructor initializes a form as well as allows command line arguements 
        /// and run it as an executable. This also updates the cells in the new form if 
        /// a save file is loaded. 
        /// </summary>
        public Form1()
        {
            InitializeComponent();

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length >= 2)
            {
                spreadsheet = new Spreadsheet(args[1], s => true, s => s.ToUpper(), "ps6");
                fileName = Path.GetFileName(args[1]);
            }
            else
            {
                spreadsheet = new Spreadsheet(s => true, s => s.ToUpper(), "ps6");
                fileName = "Untitled.sprd";
            }

            Text = fileName;

            foreach (string cellName in spreadsheet.GetNamesOfAllNonemptyCells())
                UpdateCellByName(cellName);

            SelectCell();
            spreadsheetPanel.SelectionChanged += CellSelectedEvent;
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
            object cellContents = spreadsheet.GetCellContents(cellName);
            cellContentBox.Text = spreadsheet.GetCellContents(cellName).ToString();

            if (cellContents is Formula f)
                cellContentBox.Text = "=" + cellContentBox.Text;

            spreadsheetPanel.GetValue(column, row, out string t);
            cellValueBox.Text = t;
        }
        /// <summary>
        /// Converst the column and row to the variable name
        /// </summary>
        /// <returns></returns>
        private string GetSelectedCellName()
        {
            return (char)(column + 65) + (row + 1).ToString();
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
                //Shifts selection right when Tab is pressed
                case Keys.Tab:
                    SubmitText();
                    spreadsheetPanel.SetSelection((column + 1) % 26, row);
                    SelectCell();
                    return true;
                //Shifts selection right when right arrow is pressed
                case Keys.Right:
                    SubmitText();
                    spreadsheetPanel.SetSelection((column+1)%26, row);
                    SelectCell();
                    return true;
                //Shifts selection left when Left arrow is pressed
                case Keys.Left:
                    SubmitText();
                    spreadsheetPanel.SetSelection((column-1)%26, row);
                    SelectCell();
                    return true;
                //Shifts selection down up when enter is pressed
                case Keys.Enter:
                    SubmitText();
                    spreadsheetPanel.SetSelection(column, (row+1)%99);
                    SelectCell();
                    return true;
                //Shifts selection down up when down is pressed
                case Keys.Down:
                    SubmitText();
                    spreadsheetPanel.SetSelection(column, (row + 1) % 99);
                    SelectCell();
                    return true;
                //Shifts Selection up when up is pressed
                case Keys.Up:
                    SubmitText();
                    spreadsheetPanel.SetSelection(column, (row-1)%99);
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
            try
            {
                ISet<string> toUpdate = spreadsheet.SetContentsOfCell(cellName, cellContentBox.Text);

                foreach (string name in toUpdate)
                    UpdateCellByName(name);

                spreadsheetPanel.GetValue(column, row, out string t);
                cellValueBox.Text = t;

                if (spreadsheet.Changed)
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
        private void UpdateCellByName(string name)
        {
            //Converts the column and row to the Cell name
            int col = name[0] - 65;
            int row = int.Parse(name[1].ToString()) - 1;

            object value = spreadsheet.GetCellValue(name);
            string text = value.ToString();
            if (value is FormulaError f)
                text = f.Reason;

            spreadsheetPanel.SetValue(col, row, text);
        }
        /// <summary>
        /// Displays how to use Spreadsheet 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Spreadsheet program by Alexis Koopmann and Jacob Larkin" 
                             + "\n" + "©BlippityBlahInc"
                             + "\n" + "Select different cells with arrow keys, enter, tab, or the mouse"
                             + "\n" + "Added Functionality:"
                             + "\n\t" + "You can change the color of the spreadsheet with the View."
                             + "\n\t" + "Pressing the Enter key calculates value."
                             + "\n\t" + "Exiting a cell will automatically calculate the value."
                             + "\n\t" + "Selecting a cell allows you to enter it's contents."
                             + "\n\t" + "Spreadsheet can be called from the command line"
                             + "\n\t" + "Tab calculates the value of a cell as well and moves right."
                             + "\n\t" + "Shift calculates the value of the cell and moves left."
                             + "\n\t" + "If a file is unsaved an asterisk is displayed next to its name"
                             + "\n\t" + "The default file name is Untitled.sprd."
                             + "\n\t" + "You can change the color of the Top Part of the spreadsheet."
                             + "\n\t" + "You can clear all the cells in the spreadsheet."
                             + "\n\t" + "Our spreadsheet cheers for you when you save!", "About");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (spreadsheet.Changed)
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
                    spreadsheet.Save(s.FileName);
                    fileName = Path.GetFileName(s.FileName);
                    Text = fileName;
                    playSimpleSound();
                    return true;
                }
            }
            catch(Exception e)
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
            catch(Exception z)
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
            Spreadsheet old = spreadsheet;
            spreadsheet = new Spreadsheet(s => true, s => s.ToUpper(), "ps6");

            foreach (string cellName in old.GetNamesOfAllNonemptyCells())
                UpdateCellByName(cellName);

            SelectCell();
        }
        /// <summary>
        /// Makes a sound when you save successfully!
        /// </summary>
        private void playSimpleSound()
        {
            SoundPlayer audio = new SoundPlayer(SpreadsheetGui.Properties.Resources._1_person_cheering_Jett_Rifkin_1851518140); 
            audio.Play();
        }
    }
}

