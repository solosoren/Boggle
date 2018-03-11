using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PS8
{
    public partial class BoggleClient : Form, IBoggleClient
    {
        public BoggleClient()
        {
            InitializeComponent();
        }

        public bool IsUserRegistered { get; set; }

        public event Action<string, string> RegisterPressed;
        public event Action CancelPressed;

        /// <summary>
        /// If state == true, enables all controls
        /// If state == false, disables all controls and changes register to Cancel
        /// </summary>
        /// <param name="state"></param>
        public void EnableControls(bool state)
        {
            domainNameTextBox.Enabled = state;
            playerNameTextBox.Enabled = state;
            registerButton.Text = state == true ? "Register" : "Cancel";
        }

        private void registerButton_Click(object sender, EventArgs e)
        {
            if (registerButton.Text.Equals("Register"))
            {
                RegisterPressed?.Invoke(domainNameTextBox.Text, playerNameTextBox.Text);
            }
            else
            {
                CancelPressed?.Invoke();
            }
        }
    }
}
