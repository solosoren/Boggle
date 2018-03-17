using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PS8
{
    public partial class BoggleGame : Form, IBoggleGame
    {

        public event Action<string, Button> EnterPressed;
        public event Action GameClosed;
        public event Action HelpPressed;

        /// <summary>
        /// Creates a new BoggleGame with the given game
        /// </summary>
        /// <param name="game"></param>
        public BoggleGame(Game game)
        {
            InitializeComponent();
            Control control = new Control();
            char[] board = game.Board.ToCharArray();
            int currentCharacter = 0;
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    control = tableLayoutPanel1.GetControlFromPosition(col, row);
                    control.Text = board[currentCharacter].ToString();
                    currentCharacter++;
                }
            }

            player1Name.Text = game.Player1.Nickname;
            player1Score.Text = game.Player1.Score.ToString();

            player2Name.Text = game.Player2.Nickname;
            player2Score.Text = game.Player2.Score.ToString();
            timeLeft.Text = game.TimeLeft.ToString();
            this.Update();
        }

        /// <summary>
        /// updates the form
        /// </summary>
        /// <param name="game"></param>
        public void UpdateBoard(Game game)
        {
            this.Invoke((Action)(() =>
            {
                timeLeft.Text = game.TimeLeft.ToString();
                player1Score.Text = game.Player1.Score.ToString();
                player2Score.Text = game.Player2.Score.ToString();
                this.Update();
            }));
        }

        /// <summary>
        /// Play word button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Enter_Click(object sender, EventArgs e)
        {
            if (ValidWord())
            {
                EnterPressed?.Invoke(wordTextBox.Text, EnterButton);
            }
        }

        /// <summary>
        /// Checks whether the word is valid
        /// </summary>
        /// <returns></returns>
        private bool ValidWord()
        {
            if (wordTextBox.Text != null && wordTextBox.Text.Trim() != "" && wordTextBox.Text.Trim().Length < 30)
            {
                return true;
            }
            return false;
        }

        public void EndGame(List<string> player1Words, List<string> player2Words)
        {
            this.Invoke((Action)(() =>
            {
                EnterButton.Enabled = false;
                wordTextBox.Enabled = false;
                timeLeft.Text = "Finished";
                foreach (string word in player1Words)
                {
                    player1WordList.Items.Add(new Label().Text = word);
                }
                foreach (string word in player2Words)
                {
                    player2WordList.Items.Add(new Label().Text = word);
                }
                this.Update();
            }));

        }

        private void BoggleGame_FormClosing(object sender, FormClosingEventArgs e)
        {
            GameClosed?.Invoke();
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelpPressed?.Invoke();
        }

        private void wordTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode.Equals(Keys.Enter))
            {
                if (ValidWord())
                {
                    EnterPressed?.Invoke(wordTextBox.Text, EnterButton);
                }
            }
        }
    }
}