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

        event Action<string> SetConentEvent;

        string GetCellName();
    }
}
