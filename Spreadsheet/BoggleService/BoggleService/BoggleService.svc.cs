using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel.Web;
using static System.Net.HttpStatusCode;

namespace Boggle
{
    public class BoggleService : IBoggleService
    {

        private readonly static Dictionary<String, User> users = new Dictionary<string, User>();
        private readonly static Dictionary<String, Game> games = new Dictionary<string, Game>();
        private static readonly object sync = new object();

        /// <summary>
        /// The most recent call to SetStatus determines the response code used when
        /// an http response is sent.
        /// </summary>
        /// <param name="status"></param>
        private static void SetStatus(HttpStatusCode status)
        {
            WebOperationContext.Current.OutgoingResponse.StatusCode = status;
        }

        public void CancelJoinRequest(string userToken)
        {
            throw new NotImplementedException();
        }

        public string CreateUser(User user)
        {
            lock (sync)
            {
                if (user.Nickname == null || user.Nickname.Trim().Length == 0 || user.Nickname.Trim().Length > 50)
                {
                    SetStatus(Forbidden);
                    return null;
                }

                string userID = Guid.NewGuid().ToString();
                users.Add(userID, user);
                //TODO: Return User Token 
                SetStatus(Created);
                return userID;
            }

        }


        public Game JoinGame(int timeLimit, string token)
        {
            throw new NotImplementedException();
        }

        public Game GameStatus(string GameID, string brief)
        {
            throw new NotImplementedException();
        }

        public void PlayWord(string UserToken, string Word, string GameID)
        {
            throw new NotImplementedException();
        }
    }
}
