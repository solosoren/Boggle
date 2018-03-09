﻿using SpreadsheetGUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS7Tester
{
    class SpreadsheetViewStub : ISpreadsheetView
    {
        public event Action<int, int, string> SetContentEvent;
        public event Action CloseEvent;
        public event Action NewEvent;
        public event Action<System.Windows.Forms.TextBox, System.Windows.Forms.TextBox> OpenEvent;
        public event Action HelpSpreadsheetEvent;
        public event Action HelpFileEvent;
        public event Action<FileStream> SaveEvent;
        public event Action DidChangeEvent;
        public event Action<int, int, System.Windows.Forms.TextBox, System.Windows.Forms.TextBox> SelectionChangeEvent;

        public void CloseWindow()
        {
            throw new NotImplementedException();
        }

        public void OpenNew()
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public void SetCellValue(int column, int row, string content)
        {
            throw new NotImplementedException();
        }
    }
}