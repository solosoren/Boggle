using SS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Formulas;
using System.Windows.Forms;
using SSGui;
using System.Text.RegularExpressions;

namespace SpreadsheetGUI
{
    class Controller
    {
        private ISpreadsheetView spreadsheetView;
        private Spreadsheet spreadsheet;

        public Controller(ISpreadsheetView spreadsheetView)
        {
            this.spreadsheetView = spreadsheetView;
            spreadsheet = new Spreadsheet();
            spreadsheetView.SetContentEvent += HandleSetContent;
            spreadsheetView.CloseEvent += HandleClose;
            spreadsheetView.HelpEvent += HandleHelp;
            spreadsheetView.SelectionChangeEvent += HandleSelectionChange;
        }

        /// <summary>
        /// Handles request to add content to selected cell
        /// </summary>
        /// <param name="content"></param>
        private void HandleSetContent(int column, int row, string content)
        {
            foreach (string name in spreadsheet.SetContentsOfCell(getCellName(column, row), content))
            {
                int col = -65, ro = -49;
                foreach (char character in name.ToCharArray())
                {
                    if (Regex.IsMatch(character.ToString(), @"[a-zA-Z]"))
                    {
                        col += character;
                    }
                    else
                    {
                        ro += character;
                    }
                }
                spreadsheetView.SetCellValue(col, ro, spreadsheet.GetCellValue(name).ToString());
            }
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
        /// Handles a request to close the window
        /// </summary>
        private void HandleClose()
        {
            if (spreadsheet.Changed)
            {
                // TODO: Display save? dialogue box

            }
            else
            {
                spreadsheetView.CloseWindow();
            }

        }

        /// <summary>
        /// Handles a request to open a help dialog
        /// </summary>
        private void HandleHelp()
        {

        }

        /// <summary>
        /// Changes the information in value and content textboxes
        /// </summary>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <param name="valueTextBox"></param>
        /// <param name="contentTextBox"></param>
        private void HandleSelectionChange(int column, int row, TextBox valueTextBox, TextBox contentTextBox)
        {
            // Displays cell name and value in value textbox
            valueTextBox.Text = String.Format("{0} : {1}", getCellName(column, row), spreadsheet.GetCellValue(getCellName(column, row)));
            // Displays cell value based on selection in content textbox
            contentTextBox.Text = spreadsheet.GetCellContents(getCellName(column, row)).ToString();
        }

    }
}
