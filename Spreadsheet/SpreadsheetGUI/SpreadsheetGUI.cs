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

        public SpreadsheetGUI()
        {
            InitializeComponent();

            spreadsheetPanel1.SelectionChanged += displaySelection;
        }

        private void displaySelection(SpreadsheetPanel ss)
        {
            int column, row;
            ss.GetSelection(out column, out row);
            ss.SetSelection(column, row);
            ss.SetValue(column, row, getCellName(column, row));
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
        /// Closes the window
        /// </summary>
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }



    }
}
