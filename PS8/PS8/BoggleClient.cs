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
        public bool IsInActiveGame { get; set; }

        public event Action<string, string> RegisterPressed;
        public event Action RegisterCancelPressed;
        public event Action<int> JoinGamePressed;
        public event Action JoinGameCancelPressed;

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
                registerButton.Enabled = false;
                gameDurationLabel.Enabled = true;
                gameDurationTextBox.Enabled = true;
                joinGameButton.Enabled = true;
                this.Update();
            }
            else
            {
                domainNameTextBox.Enabled = state;
                playerNameTextBox.Enabled = state;
                gameDurationLabel.Enabled = false;
                gameDurationTextBox.Enabled = false;
                joinGameButton.Enabled = false;
                registerButton.Text = state == true ? "Register" : "Cancel";
                this.Update();
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
                RegisterCancelPressed?.Invoke();
            }
        }

        private void joinGameButton_Click(object sender, EventArgs e)
        {
            if (joinGameButton.Text.Equals("Join Game"))
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
                else
                {
                    MessageBox.Show("Enter an integer ranging from 5 - 120.");
                }
            }
            else
            {
                JoinGameCancelPressed?.Invoke();
            }

        }

        public void SetJoinGameControlState(bool state)
        {
            if (IsInActiveGame)
            {
                gameDurationLabel.Enabled = false;
                gameDurationTextBox.Enabled = false;
                joinGameButton.AutoSize = true;
                joinGameButton.Invoke(new Action(() => joinGameButton.Text = "Joined Game"));
                joinGameButton.Invoke(new Action(() => joinGameButton.Update()));
            }

            this.Invoke((Action)(() =>
         {
             gameDurationLabel.Enabled = state;
             gameDurationTextBox.Enabled = state;
             joinGameButton.AutoSize = true;
             joinGameButton.Text = state == true ? "Join Game" : "Cancel game request";
             joinGameButton.Update();
         }));
        }
    }
}
