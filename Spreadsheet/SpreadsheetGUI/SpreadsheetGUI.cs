using SS;
using SSGui;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpreadsheetGUI
{
    public partial class SpreadsheetGUI : Form
    {

        /// <summary>
        /// Underlying spreadsheet which will contain all information.
        /// </summary>
        private Spreadsheet spreadsheet;

        /// <summary>
        /// Offsets for resizing so the SpreadsheetPanel does not get hidden under the GUI.
        /// </summary>
        private int panelWidthOffset, panelHeightOffset;

        public SpreadsheetGUI()
        {
            InitializeComponent();
            panelWidthOffset = this.Width - spreadsheetPanel1.Width;
            panelHeightOffset = this.Height - spreadsheetPanel1.Height;   
            KeyPreview = true;
            spreadsheetPanel1.SelectionChanged += displaySelection;
        }

        private void displaySelection(SpreadsheetPanel ss)
        {
            int column, row;
            string value;
            ss.GetSelection(out column, out row);
            ss.SetSelection(column, row);
            ss.GetValue(column, row, out value);

            // Displays cell name and value in value textbox
            cellValueTextBox.Text = getCellName(column, row) + " : " + value;

            // Displays cell value based on selection in content textbox
            cellContentTextBox.Text = value;
        }

        /// <summary>
        /// Converts given column and row integers to a string that matches the spreadsheet and returns it.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        private string getCellName(int column, int row)
        {
            Char c = (Char)(65 + column);
            string rowS = row.ToString();
            return c + (row + 1).ToString();
        }


        /// <summary>
        /// Closes SpreadsheetGUI
        /// </summary>
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }


        /// <summary>
        /// Allows to naviagte cells using arrow keys.
        /// </summary>
        private void SpreadsheetGUI_KeyDown(object sender, KeyEventArgs e)
        {
            int column, row;
            spreadsheetPanel1.GetSelection(out column, out row);
            switch (e.KeyCode)
            {
                case Keys.Down:
                    if (row == 98)
                    {
                        break;
                    }
                    spreadsheetPanel1.SetSelection(column, row + 1);
                    // displaySelection here may not be necessary
                    displaySelection(spreadsheetPanel1);
                    e.Handled = true;
                    break;
                case Keys.Up:
                    if (row == 0)
                    {
                        break;
                    }
                    spreadsheetPanel1.SetSelection(column, row - 1);
                    // displaySelection here may not be necessary
                    displaySelection(spreadsheetPanel1);
                    e.Handled = true;
                    break;
                case Keys.Left:
                    if (column == 0)
                    {
                        break;
                    }
                    spreadsheetPanel1.SetSelection(column - 1, row);
                    // displaySelection here may not be necessary
                    displaySelection(spreadsheetPanel1);
                    e.Handled = true;
                    break;
                case Keys.Right:
                    if (column == 26)
                    {
                        break;
                    }
                    spreadsheetPanel1.SetSelection(column + 1, row);
                    // displaySelection here may not be necessary
                    displaySelection(spreadsheetPanel1);
                    e.Handled = true;
                    break;
            }
        }

       
        /// <summary>
        /// Once enter is pressed while content text box is in focus,
        /// sets content value for selected cell to the text that is
        /// in the text box.
        /// </summary>
        private void cellContentTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode.Equals(Keys.Enter))
            {
                int column, row;
                spreadsheetPanel1.GetSelection(out column, out row);

                // Will need to add controller command here
                spreadsheetPanel1.SetValue(column, row, cellContentTextBox.Text);
            }
        }


        /// <summary>
        /// Resizes SpreadsheetPanel based on SpreadsheetGUI.
        /// </summary>
        private void SpreadsheetGUI_Resize(object sender, EventArgs e)
        {
            spreadsheetPanel1.Width = this.Width - panelWidthOffset;
            spreadsheetPanel1.Height = this.Height - panelHeightOffset;
   
        }


    }
}
