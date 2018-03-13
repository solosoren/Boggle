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
        public void SetControlState(bool state)
        {
            if (IsUserRegistered)
            {
                domainNameLabel.Enabled = false;
                domainNameTextBox.Enabled = false;
                playerNameLabel.Enabled = false;
                playerNameTextBox.Enabled = false;
                registerButton.AutoSize = true;
                registerButton.Text = "Registered";
                registerButton.Update();
                registerButton.Enabled = false;
                gameDurationLabel.Enabled = true;
                gameDurationTextBox.Enabled = true;
                joinGameButton.Enabled = true;
            }
            else
            {
                domainNameTextBox.Enabled = state;
                playerNameTextBox.Enabled = state;
                gameDurationLabel.Enabled = false;
                gameDurationTextBox.Enabled = false;
                joinGameButton.Enabled = false;
                registerButton.Text = state == true ? "Register" : "Cancel";
                registerButton.Update();
            }

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
