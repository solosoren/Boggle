using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS8
{
    /// <summary>
    /// Store everything that is apart of the game. Contains internal Player Struct.
    /// </summary>
    public class Game
    {
        public string GameID { get; private set; }
        public string GameState { get; private set; }
        public string Board { get; set; }
        public int? TimeLeft { get; private set; }
        public int? TimeLimit { get; private set; }
        public Player Player1 { get; private set; }
        public Player Player2 { get; private set; }

        /// <summary>
        /// Creates a new game with the GameID
        /// </summary>
        /// <param name="gameID"></param>
        public Game(string gameID)
        {
            GameID = gameID;
        }

        /// <summary>
        /// Set the game state
        /// </summary>
        /// <param name="gameState"></param>
        public void SetState(string gameState)
        {
            GameState = gameState;
        }

        /// <summary>
        /// Start a game by storing all the game properties
        /// </summary>
        /// <param name="d"> an object from the client containing the game properties</param>
        public void StartGame(dynamic d)
        {
            // handle null
      
            GameState = d.GameState;
            TimeLeft = d.TimeLeft ?? 0;
            TimeLimit = d.TimeLimit ?? 0;


            dynamic player1 = d.Player1;
            if(player1 == null)
            {
                player1.Score = 0;
            }
            Player1 = new Player(Convert.ToString(player1.Nickname), (int)player1.Score);

            dynamic player2 = d.Player2;
            if(player2 == null)
            {
                player2.Score = 0;
            }
            Player2 = new Player(Convert.ToString(player2.Nickname), (int)player2.Score);
        }

        public void UpdateTime(int time)
        {
            TimeLeft = time;
        }

        /// <summary>
        /// Update both scores
        /// </summary>
        /// <param name="player1Score"></param>
        /// <param name="player2Score"></param>
        public void UpdateScore(int player1Score, int player2Score)
        {
            Player1.SetScore(player1Score);
            Player2.SetScore(player2Score);
        }
    }
}
