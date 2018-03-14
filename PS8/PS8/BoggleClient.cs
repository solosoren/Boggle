using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        public event Action<int> JoinGamePressed;

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
                if (playerNameTextBox.Text.Equals(null) || playerNameTextBox.Text.Trim().Equals(""))
                {
                    MessageBox.Show("Player name can't be empty.");
                    return;
                }

                if (playerNameTextBox.Text.Trim().Length > 50)
                {
                    MessageBox.Show("Player name must be less than 50 characters.");
                    return;
                }

                RegisterPressed?.Invoke(domainNameTextBox.Text, playerNameTextBox.Text);
            }
            else
            {
                CancelPressed?.Invoke();
            }
        }

        private void joinGameButton_Click(object sender, EventArgs e)
        {
            if (int.TryParse(gameDurationTextBox.Text, out int duration))
            {
                if (duration < 5)
                {
                    MessageBox.Show("Your game duration must be greater than 5 seconds.");
                    return;
                }
                else if (duration > 120)
                {
                    MessageBox.Show("Your game duration must be less than than 120 seconds.");
                    return;
                }

                JoinGamePressed.Invoke(duration);
            }
        }
    }
}
