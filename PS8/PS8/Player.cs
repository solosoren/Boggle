using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS8
{
    /// <summary>
    /// Used to separate players and scores cleanly
    /// </summary>
    public struct Player
    {
        public string Nickname { get; private set; }
        public int? Score { get; private set; }

        /// <summary>
        /// Creates a new player
        /// </summary>
        /// <param name="nickName"></param>
        /// <param name="score"></param>
        public Player(string nickName, int? score)
        {
            Nickname = nickName;
            Score = score;
        }

        /// <summary>
        /// Sets the players score
        /// </summary>
        /// <param name="score"></param>
        public void SetScore(int score)
        {
            Score = score;
        }
    }
}
