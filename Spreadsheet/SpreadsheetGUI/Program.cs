using System;
using System.Windows.Forms;

namespace SpreadsheetGUI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var context = SpreadsheetGUIContext.GetContext();
            SpreadsheetGUIContext.GetContext().RunNew();
            Application.Run(context);
        }
    }
}
