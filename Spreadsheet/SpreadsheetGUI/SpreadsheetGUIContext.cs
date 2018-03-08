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
        /// Runs the and opens the Help window
        /// </summary>
        public void RunHelp()
        {
            HelpDialog helpDialog = new HelpDialog();

            windowCount++;

            helpDialog.FormClosed += (o, e) => { if (--windowCount <= 0) ExitThread(); };

            helpDialog.Show();
        }
    }
}
