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
    public partial class BoggleGame : Form, IBoggleGame
    {
        public BoggleGame(Game game)
        {
            InitializeComponent();
            Control control = new Control();
            char[] board = game.Board.ToCharArray();
            int currentCharacter = 0;
            for (int col = 0; col < 4; col++)
            {
                for (int row = 0; row < 4; row++)
                {
                    control = tableLayoutPanel1.GetControlFromPosition(col, row);
                    control.Text = board[currentCharacter].ToString();
                    currentCharacter++;
                }
            }

            player1Name.Text = game.Player1.NickName;
            player1Score.Text = game.Player1.Score.ToString();

            player2Name.Text = game.Player2.NickName;
            player2Score.Text = game.Player2.Score.ToString();
            timeLeft.Text = game.TimeLeft.ToString();
        }

        public void UpdateBoard(Game game)
        {
            timeLeft.Text = game.TimeLeft.ToString();
            player1Score.Text = game.Player1.Score.ToString();
            player2Score.Text = game.Player2.Score.ToString();
        }

        private void Enter_Click(object sender, EventArgs e)
        {
            // check word
        }
    }
}