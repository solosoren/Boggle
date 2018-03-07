using SS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Formulas;

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
                spreadsheetView.SetCellContent(name, spreadsheet.GetCellValue(name).ToString());
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
