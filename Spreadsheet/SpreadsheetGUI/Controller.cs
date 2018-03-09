using SS;
using System;
using System.Linq;
using Formulas;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;

namespace SpreadsheetGUI
{
    public class Controller
    {
        private ISpreadsheetView spreadsheetView;
        private Spreadsheet spreadsheet;

        public Controller(ISpreadsheetView spreadsheetView)
        {
            this.spreadsheetView = spreadsheetView;
            spreadsheet = new Spreadsheet();
            spreadsheetView.SetContentEvent += HandleSetContent;
            spreadsheetView.CloseEvent += HandleClose;
            spreadsheetView.HelpSpreadsheetEvent += HandleSpreadsheetHelp;
            spreadsheetView.HelpFileEvent += HandleFileHelp;
            spreadsheetView.SelectionChangeEvent += HandleSelectionChange;
            spreadsheetView.NewEvent += HandleNew;
            spreadsheetView.DidChangeEvent += HandleDidChange;
            spreadsheetView.SaveEvent += HandleSave;
            spreadsheetView.OpenEvent += HandleOpen;
        }

        /// <summary>
        /// Handles request to add content to selected cell
        /// </summary>
        /// <param name="content"></param>
        private void HandleSetContent(int column, int row, string content)
        {
            Spreadsheet oldSpreadsheet = new Spreadsheet();
            foreach (string name in spreadsheet.GetNamesOfAllNonemptyCells())
            {
                oldSpreadsheet.SetContentsOfCell(name, spreadsheet.GetCellContents(name).ToString());
            }
            try
            {
                if (content != "")
                {
                    foreach (string name in spreadsheet.SetContentsOfCell(getCellName(column, row), content))
                    {
                        int col = name.ToCharArray()[0] - 65;
                        int ro = int.Parse(name.Substring(1));
                        object contentToCheck = spreadsheet.GetCellValue(name);
                        if (contentToCheck is FormulaError)
                        {
                            spreadsheetView.DisplayMessage(String.Format("Formula error {0}", contentToCheck.ToString()));
                            spreadsheet = oldSpreadsheet;
                            break;
                        }
                        spreadsheetView.SetCellValue(col, ro - 1, spreadsheet.GetCellValue(name).ToString());
                    }
                }

            }
            catch (FormulaFormatException e)
            {
               spreadsheetView.DisplayMessage("Formula format invalid");

            }
            catch (CircularException e)
            {
                spreadsheetView.DisplayMessage("Circular exception");
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
                if (MessageBox.Show("File isn't saved. Would you like to save the file before closing? ", "Save file", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    spreadsheetView.Save();
                    spreadsheetView.CloseWindow();
                }
                else
                {
                    spreadsheetView.CloseWindow();
                }
            }
            else
            {
                spreadsheetView.CloseWindow();
            }

        }

        /// <summary>
        /// Handles a request to open a help dialog for spreadsheet
        /// </summary>
        private void HandleSpreadsheetHelp()
        {
            Thread thread = new Thread(() =>
            {
                var context = SpreadsheetGUIContext.GetContext();
                SpreadsheetGUIContext.GetContext().RunSpreadsheetHelp();
                Application.Run(context);
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        /// <summary>
        /// Handles a request to open a help dialog for file
        /// </summary>
        private void HandleFileHelp()
        {
            Thread thread = new Thread(() =>
            {
                var context = SpreadsheetGUIContext.GetContext();
                SpreadsheetGUIContext.GetContext().RunFileHelp();
                Application.Run(context);
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
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
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

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
                try
                {
                    spreadsheet.Save(s);

                }
                catch (IOException e)
                {
                    spreadsheetView.DisplayMessage("Unkown error encountered while attempting to save file.");
                }
            }
        }

        private void HandleOpen()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    FileStream fs = (FileStream)openFileDialog.OpenFile();
                    using (StreamReader s = new StreamReader(fs))
                        spreadsheet = new Spreadsheet(s, new Regex(""));

                    resetView();
                    foreach (string name in spreadsheet.GetNamesOfAllNonemptyCells().ToList())
                    {
                        int col = name.ToCharArray()[0] - 65;
                        int row = int.Parse(name.Substring(1));
                        spreadsheetView.SetCellValue(col, row - 1, spreadsheet.GetCellValue(name).ToString());
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error reading file : " + e.Message);
            }

        }

        /// <summary>
        /// Resets all cell values to null in SpreadsheetPanel
        /// </summary>
        private void resetView()
        {
            for (int col = 0; col < 27; col++)
            {
                for (int row = 0; row < 99; row++)
                {
                    spreadsheetView.SetCellValue(col, row, "");
                }
            }
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

