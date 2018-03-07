using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpreadsheetGUI
{
    /// <summary>
    /// Controllable interface for SpreadsheetGUI
    /// </summary>
    interface ISpreadsheetView
    {

        event Action<string> SetContentEvent;
        event Action CloseEvent;
        event Action NewEvent;

        /// <summary>
        /// Gets the cell name
        /// </summary>
        /// <returns></returns>
        string GetCellName();

        /// <summary>
        /// Sets the cell's content after a formula has been computed
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        void SetCellContent(int column, int row, string name, string content);

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
