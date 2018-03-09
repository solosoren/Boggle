using SpreadsheetGUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PS7Tester
{
    class SpreadsheetViewStub : ISpreadsheetView
    {
        public event Action<int, int, string> SetContentEvent;
        public event Action CloseEvent;
        public event Action NewEvent;
        public event Action OpenEvent;
        public event Action HelpSpreadsheetEvent;
        public event Action HelpFileEvent;
        public event Action<FileStream> SaveEvent;
        public event Action DidChangeEvent;
        public event Action<int, int, System.Windows.Forms.TextBox, System.Windows.Forms.TextBox> SelectionChangeEvent;


        public Boolean CalledCloseWindow = false;
        public Boolean CalledSave = false;
        public String displayMessage;
        public String setCellValue;
        public int setColumn;
        public int setRow;

        public void CloseWindow()
        {
            CalledCloseWindow = true;
        }

        public void OpenNew()
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            CalledSave = true;
        }

        public void SetCellValue(int column, int row, string content)
        {
            setCellValue = content;
            setColumn = column;
            setRow = row;
        }

        public void TestSetContentEvent(int column, int row, string content)
        {
            SetContentEvent(column, row, content);
        }

        public void DisplayMessage(string message)
        {
            displayMessage = message;
        }

        public void TestClose()
        {
            CloseEvent();
        }

        public void TestHandleSpreadsheetHelp()
        {
            HelpSpreadsheetEvent();
        }

        public void TestHandleFileHelp()
        {
            HelpFileEvent();
        }

        public void TestDidChange()
        {
            DidChangeEvent();
        }

        public void TestSave()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Spreadsheet (*.ss)|*.ss|All files (*.*)|*.*";
            saveFileDialog.Title = "Save Spreadsheet";
            saveFileDialog.ShowDialog();
            if (saveFileDialog.FileName != "")
            {
                FileStream fs = (FileStream)saveFileDialog.OpenFile();
                SaveEvent(fs);
            }
            
        }

        public void TestNew()
        {
            NewEvent();
        }

        public void TestOpen()
        {
            OpenEvent();
        }



    }
}
