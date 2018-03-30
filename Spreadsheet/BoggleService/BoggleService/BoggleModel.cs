using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel.Web;
using static System.Net.HttpStatusCode;

namespace Boggle
{

    [DataContract]
    public class User
    {
        [DataMember(EmitDefaultValue = false)]
        public string Nickname { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string UserToken { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int Score { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsInGame { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string GameID { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Dictionary<string, int> WordsPlayed { get; set; }

        public User()
        {
            WordsPlayed = new Dictionary<string, int>();
        }

        public User BriefUser()
        {
            User user = new User();
            user.Score = this.Score;
            return user;
        }

        public User ActiveLongUser()
        {
            User user = new User();
            user.Nickname = this.Nickname;
            user.Score = this.Score;

            return user;
        }

        public User CompletedLongUser()
        {
            User user = ActiveLongUser();
            user.WordsPlayed = this.WordsPlayed;

            return user;
        }
    }



    [DataContract]
    public class Game
    {
        [DataMember]
        // Can be pending, active or completed
        public string GameState { get; set; }

        private DateTime StartTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int TimeLimit { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string GameID { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public User Player1 { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public User Player2 { get; set; }

        public BoggleBoard BoggleBoard { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string board { get; set; }

        public Game()
        {
            BoggleBoard = new BoggleBoard();
        }

        [DataMember(EmitDefaultValue = false)]
        public int TimeLeft
        {
            get
            {
                if (GameState == "Completed")
                {
                    return 0;
                }
                return (int)(StartTime - StartTime.AddSeconds((double)TimeLimit)).TotalSeconds;
            }
            set { }
        }

        public void SetStartTime()
        {
            this.StartTime = DateTime.Now;
        }

        public Game BriefGame()
        {
            Game game = new Game();
            game.GameState = GameState;
            game.TimeLeft = this.TimeLeft;
            game.Player1 = Player1.BriefUser();
            game.Player2 = Player2.BriefUser();
            return game;
        }

        public Game ActiveStatusLong()
        {
            Game game = new Game();
            game.GameState = GameState;
            game.board = BoggleBoard.ToString();
            game.TimeLimit = TimeLimit;
            game.TimeLeft = TimeLeft;

            game.Player1 = Player1.ActiveLongUser();
            game.Player2 = Player2.ActiveLongUser();

            return game;
        }

        public Game CompletedStatusLong()
        {
            Game game = ActiveStatusLong();
            game.Player1 = Player1.CompletedLongUser();
            game.Player2 = Player2.CompletedLongUser();

            return game;
        }

    }

    public class SetGame
    {
        public string UserToken { get; set; }
        public int TimeLimit { get; set; }
    }

    public class CancelRequestDetails
    {
        public string UserToken { get; set; }
    }

    public class PlayWordDetails
    {
        public string UserToken { get; set; }
        public string Word { get; set; }
    }

}