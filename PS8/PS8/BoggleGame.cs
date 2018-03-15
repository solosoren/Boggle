using System;
using System.Windows.Forms;

namespace PS8
{
    public partial class BoggleGame : Form, IBoggleGame
    {
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


        public void UpdateBoard(Game game)
        {
            timeLeft.Invoke(new Action(() => timeLeft.Text = game.TimeLeft.ToString()));
            player1Score.Invoke(new Action(() => player1Score.Text = game.Player1.Score.ToString()));
            player2Score.Invoke(new Action(() => player2Score.Text = game.Player2.Score.ToString()));
        }

        private void Enter_Click(object sender, EventArgs e)
        {
            // check word
        }
    }
}