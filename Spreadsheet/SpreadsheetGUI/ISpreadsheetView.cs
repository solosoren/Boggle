using SSGui;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpreadsheetGUI
{
    /// <summary>
    /// Controllable interface for SpreadsheetGUI
    /// </summary>
    public interface ISpreadsheetView
    {

        event Action<int, int, string> SetContentEvent;
        event Action CloseEvent;
        event Action NewEvent;
        event Action <TextBox, TextBox>OpenEvent;
        event Action HelpSpreadsheetEvent;
        event Action HelpFileEvent;
        event Action<FileStream> SaveEvent;
        event Action DidChangeEvent;
        event Action<int, int, TextBox, TextBox> SelectionChangeEvent;

        /// <summary>
        /// Sets spreadsheetPanel to given content at given location
        /// </summary>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <param name="content"></param>
        void SetCellValue(int column, int row, string content);

        /// <summary>
        /// Closes the window
        /// </summary>
        void CloseWindow();

        /// <summary>
        /// Find the file to save
        /// </summary>
        void Save();


    }
}
