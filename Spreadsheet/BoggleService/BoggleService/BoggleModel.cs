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
        public int PlayerNumber { get; set; }
        public int Score { get; set; }

    }

    public class Game
    {
        public Boolean GameState { get; set; }
        public string Board { get; set; }
        public DateTime StartTime { get; set; }
        public int TimeLimit { get; set; }
        public string GameID { get; set; }

    }
}