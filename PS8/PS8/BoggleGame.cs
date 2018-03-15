using System;
using System.Windows.Forms;

namespace PS8
{
    public partial class BoggleGame : Form, IBoggleGame
    {

        public event Action<string> EnterPressed;

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
        }

        /// <summary>
        /// updates the form
        /// </summary>
        /// <param name="game"></param>
        public void UpdateBoard(Game game)
        {
            timeLeft.Invoke((Action)(() => timeLeft.Text = game.TimeLeft.ToString()));
            player1Score.Invoke((Action)(() => player1Score.Text = game.Player1.Score.ToString()));
            player2Score.Invoke((Action)(() => player2Score.Text = game.Player2.Score.ToString()));

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
                EnterPressed?.Invoke(wordTextBox.Text);
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
    }
}