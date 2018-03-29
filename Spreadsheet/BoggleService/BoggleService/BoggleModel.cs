using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel.Web;
using static System.Net.HttpStatusCode;

namespace Boggle
{
    public class User
    {
        public string Nickname { get; set; }
        public string UserToken { get; set; }
        //public int PlayerNumber { get; set; }
        public int Score { get; set; }
        public bool IsInGame { get; set; }
        public string GameID { get; set; }
    }

    public class Game
    {
        // Can be pending, active or completed
        public string GameState { get; set; }
        public string Board { get; set; }
        public DateTime StartTime { get; set; }
        public int TimeLimit { get; set; }
        public string GameID { get; set; }
        public User firstPlayer { get; set; }
        public User secondPlayer { get; set; }

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
}