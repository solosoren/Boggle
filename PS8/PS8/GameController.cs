using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace PS8
{
    class GameController: IBoggleGame
    {
        private Controller controller;
        private IBoggleGame board;
        private Game game;
        private System.Timers.Timer gameTimer;

        public GameController(Controller controller, Game game, BoggleGame board)
        {
            this.controller = controller;
            this.game = game;
            this.board = board;
            gameTimer = new System.Timers.Timer(1000);
            gameTimer.Elapsed += new ElapsedEventHandler(GameTimerElapsed);
        }

        public void StartGameTimer()
        {
            gameTimer.Start();
        }

        private void GameTimerElapsed(object source, ElapsedEventArgs e)
        {
            UpdateGame();
        }

        private void UpdateGame()
        {
            controller.FetchGame(true);
            if (game.GameState == "active")
            {
                UpdateBoard(game);
                // TODO: update everything
            }
            else if (game.GameState == "completed")
            {
                gameTimer.Stop();
                // TODO: End Game
            }

        }


        public void UpdateBoard(Game game)
        {
            board.UpdateBoard(game);
        }

    }
}
