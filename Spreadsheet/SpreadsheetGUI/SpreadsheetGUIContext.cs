using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpreadsheetGUI
{
    class SpreadsheetGUIContext : ApplicationContext
    {
        /// <summary>
        /// Number of open spreadsheets
        /// </summary>
        private int windowCount = 0;

        /// <summary>
        /// Singleton ApplicationContext
        /// </summary>
        private static SpreadsheetGUIContext context;

        public static SpreadsheetGUIContext GetContext()
        {
            if (context == null)
            {
                context = new SpreadsheetGUIContext();
            }
            return context;
        }

        /// <summary>
        /// Runs the GUI 
        /// </summary>
        public void RunNew()
        {
            // Create the window and the controller
            SpreadsheetGUI spreadsheetGUI = new SpreadsheetGUI();
            new Controller(spreadsheetGUI);

            windowCount++;

            spreadsheetGUI.FormClosed += (o, e) => { if (--windowCount <= 0) ExitThread(); };

            spreadsheetGUI.Show();
        }

        /// <summary>
        /// Runs and opens the help spreadsheet dialog
        /// </summary>
        public void RunSpreadsheetHelp()
        {
            HelpSpreadsheetDialog helpDialog = new HelpSpreadsheetDialog();

            windowCount++;

            helpDialog.FormClosed += (o, e) => { if (--windowCount <= 0) ExitThread(); };

            helpDialog.Show();
        }

        /// <summary>
        /// Runs and opens the help file dialog
        /// </summary>
        public void RunFileHelp()
        {
            HelpFileDialog helpDialog = new HelpFileDialog();

            windowCount++;

            helpDialog.FormClosed += (o, e) => { if (--windowCount <= 0) ExitThread(); };

            helpDialog.Show();
        }
    }
}
