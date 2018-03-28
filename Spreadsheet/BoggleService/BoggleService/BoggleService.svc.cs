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
        /// <summary>
        /// The most recent call to SetStatus determines the response code used when
        /// an http response is sent.
        /// </summary>
        /// <param name="status"></param>
        private static void SetStatus(HttpStatusCode status)
        {
            WebOperationContext.Current.OutgoingResponse.StatusCode = status;
        }

        /// <summary>
        /// Returns a Stream version of index.html.
        /// </summary>
        /// <returns></returns>
        public Stream API()
        {
            SetStatus(OK);
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            return File.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "index.html");
        }


        public User PostUser(string nickname)
        {
            if (nickname == null || nickname.Trim().Length == 0 || nickname.Trim().Length > 50)
            {
                SetStatus(Forbidden);
                return null;
            }
            
            //TODO: Return User Token 
            SetStatus(Created);
        }


        public Game PostGame(int timeLimit, string token)
        {
            throw new NotImplementedException();
        }

        public void CancelGame(string userToken)
        {
            throw new NotImplementedException();
        }

        public void PlayWord(string UserToken, string Word, string GameID)
        {
            throw new NotImplementedException();
        }

        public Game GetGameState(string GameID, string brief)
        {
            throw new NotImplementedException();
        }
    }
}
