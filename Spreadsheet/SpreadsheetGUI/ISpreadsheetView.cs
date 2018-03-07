﻿using SSGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpreadsheetGUI
{
    /// <summary>
    /// Controllable interface for SpreadsheetGUI
    /// </summary>
    interface ISpreadsheetView
    {

        event Action<int, int, string> SetContentEvent;
        event Action CloseEvent;
        event Action NewEvent;
        event Action HelpEvent;
        event Action<int, int, TextBox, TextBox> SelectionChangeEvent;

        /// <summary>
        /// Sets spreadsheetPanel to given content at given location
        /// </summary>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <param name="content"></param>
        void SetCellValue(int column, int row, string content);
        /// <summary>
        /// Opens a new Window
        /// </summary>
        void OpenNew();

        /// <summary>
        /// Closes the window
        /// </summary>
        void CloseWindow();


    }
}