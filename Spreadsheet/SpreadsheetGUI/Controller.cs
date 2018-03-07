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
using System.Threading;
using System.IO;

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
            spreadsheetView.NewEvent += HandleNew;
            spreadsheetView.DidChangeEvent += HandleDidChange;
            spreadsheetView.SaveEvent += HandleSave;
        }

        /// <summary>
        /// Handles request to add content to selected cell
        /// </summary>
        /// <param name="content"></param>
        private void HandleSetContent(int column, int row, string content)
        {
            foreach (string name in spreadsheet.SetContentsOfCell(getCellName(column, row), content))
            {
                int col = name.ToCharArray()[0] - 65;
                int ro = int.Parse(name.Substring(1));
                spreadsheetView.SetCellValue(col, ro - 1, spreadsheet.GetCellValue(name).ToString());
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
        /// checks whether the spreadsheet was changed and calls save() accordingly
        /// </summary>
        private void HandleDidChange()
        {
            if (spreadsheet.Changed)
            {
                spreadsheetView.Save();
            }
        }


        private void HandleSave(FileStream fs)
        {
            using (StreamWriter s = new StreamWriter(fs))
            {
                spreadsheet.Save(s);
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

        /// <summary>
        /// Handles a request to open a help dialog
        /// </summary>
        private void HandleHelp()
        {

        }


        /// <summary>
        /// Handles a request to open a new spreadsheet
        /// </summary>
        private void HandleNew()
        {
           
            Thread thread = new Thread(() =>
            {
                var context = SpreadsheetGUIContext.GetContext();
                SpreadsheetGUIContext.GetContext().RunNew();
                Application.Run(context);
            });

            thread.Start();

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
            object content = spreadsheet.GetCellContents(getCellName(column, row));
            if (content is Formula)
            {
                contentTextBox.Text = "=" + spreadsheet.GetCellContents(getCellName(column, row)).ToString();
            }
            else
            {
                contentTextBox.Text = spreadsheet.GetCellContents(getCellName(column, row)).ToString();
            }
        }

    }
}
