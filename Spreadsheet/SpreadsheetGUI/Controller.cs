using SS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Formulas;
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

        }

        /// <summary>
        /// Handles request to add content to selected cell
        /// </summary>
        /// <param name="content"></param>
        private void HandleSetContent(string content)
        {
            foreach (string name in spreadsheet.SetContentsOfCell(spreadsheetView.GetCellName(), content))
            {
                int col = -65, row = -49;
                foreach (char character in name.ToCharArray())
                {
                    if (Regex.IsMatch(character.ToString(), @"[a-zA-Z]"))
                    {
                        col += character;
                    }
                    else
                    {
                        row += character;
                    }
                }
                spreadsheetView.SetCellContent(col, row, name, spreadsheet.GetCellValue(name).ToString());
            }

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


    }
}
