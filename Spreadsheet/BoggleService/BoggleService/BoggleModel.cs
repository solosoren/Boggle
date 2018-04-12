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
        public int? Score { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsInGame { private get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string GameID { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<PlayedWord> WordsPlayed { get; set; }

        public User() { }

        public User CreatedUser()
        {
            User user = new User();
            user.UserToken = this.UserToken;
            return user;
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
            if (this.WordsPlayed == null)
            {
                WordsPlayed = new List<PlayedWord>();
            }
            user.WordsPlayed = this.WordsPlayed;

            return user;
        }

        public bool InGame()
        {
            return IsInGame;
        }
    }

    [DataContract]
    public class PlayedWord
    {
        [DataMember]
        public string Word;

        [DataMember]
        public int Score;

        public PlayedWord(string word)
        {
            Word = word;
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
        public string Board { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int? TimeLeft { get; set; }

        public Game()
        {
            BoggleBoard = new BoggleBoard();
        }

        public void SetStartTime()
        {
            this.StartTime = DateTime.Now;
        }

        public DateTime GetStartTime()
        {
            return StartTime;
        }

        public Game BriefGame()
        {
            Game game = new Game();

            int left = (int)(StartTime.AddSeconds((double)TimeLimit) - DateTime.Now).TotalSeconds;
            if (GameState == "completed" || left <= 0)
            {
                TimeLeft = 0;
                GameState = "completed";
            }
            else
            {
                TimeLeft = left;
            }

            game.TimeLeft = TimeLeft;
            game.GameState = GameState;
            game.Player1 = Player1.BriefUser();
            game.Player2 = Player2.BriefUser();
            return game;
        }

        public Game ActiveStatusLong()
        {
            Game game = new Game();
            game.Board = BoggleBoard.ToString();
            game.TimeLimit = TimeLimit;

            game.Player1 = Player1.ActiveLongUser();
            game.Player2 = Player2.ActiveLongUser();

            int left = (int)(StartTime.AddSeconds((double)TimeLimit) - DateTime.Now).TotalSeconds;
            if (GameState == "completed" || left <= 0)
            {
                TimeLeft = 0;
                GameState = "completed";
                game.Player1 = Player1.CompletedLongUser();
                game.Player2 = Player2.CompletedLongUser();
            }
            else
            {
                TimeLeft = left;
            }

            game.TimeLeft = TimeLeft;
            game.GameState = GameState;
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

    [DataContract]
    public class SetGame
    {
        [DataMember(EmitDefaultValue = false)]
        public string UserToken { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int? TimeLimit { get; set; }

        [DataMember]
        public string GameID { get; set; }

        public SetGame(string GameID)
        {
            UserToken = null;
            TimeLimit = null;
            this.GameID = GameID;
        }

    }

    public class CancelRequestDetails
    {
        public string UserToken { get; set; }
    }

    [DataContract]
    public class PlayWordDetails
    {
        [DataMember(EmitDefaultValue = false)]
        public string UserToken { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Word { get; set; }

        [DataMember]
        public int Score { get; set; }

        public PlayWordDetails(int score)
        {
            Score = score;
        }
    }

}