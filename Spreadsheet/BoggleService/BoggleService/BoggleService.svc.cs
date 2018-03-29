using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Web;
using static System.Net.HttpStatusCode;

namespace Boggle
{
    public class BoggleService : IBoggleService
    {

        private readonly static Dictionary<String, User> users = new Dictionary<string, User>();
        private readonly static Dictionary<String, Game> games = new Dictionary<string, Game>();
        private readonly static HashSet<Game> pendingGames = new HashSet<Game>();
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

        /// <summary>
        /// Returns true or false depending on if the nickname is valid
        /// </summary>
        /// <param name="nickname"></param>
        /// <returns></returns>
        private bool IsNicknameValid(string nickname)
        {
            if (nickname == null || nickname.Trim().Length == 0 || nickname.Trim().Length > 50)
            {
                return false;
            }
            return true;
        }

        public string CreateUser(User user)
        {
            lock (sync)
            {
                if (!IsNicknameValid(user.Nickname))
                {
                    SetStatus(Forbidden);
                    return null;
                }

                string userID = Guid.NewGuid().ToString();

                //Setup user object
                user.UserToken = userID;
                user.Score = 0;
                user.IsInGame = false;

                users.Add(userID, user);
                SetStatus(Created);
                return user.UserToken;
            }
        }

        public string JoinGame(SetGame setGame)
        {
            lock (sync)
            {
                if (!users.ContainsKey(setGame.UserToken))
                {
                    SetStatus(Forbidden);
                    return null;
                }

                User user = users[setGame.UserToken];

                if (!IsNicknameValid(user.Nickname) || setGame.TimeLimit < 5 ||
                    setGame.TimeLimit > 120)
                {
                    SetStatus(Forbidden);
                    return null;
                }


                // Is user already in a game ?
                if (user.IsInGame)
                {
                    SetStatus(Conflict);
                    return null;
                }

                // If there is a user waiting with the same time limit, add current user to game
                if (pendingGames.Count != 0)
                {
                    foreach (Game item in pendingGames.ToList())
                    {
                        if (item.TimeLimit == setGame.TimeLimit)
                        {
                            // start pending game
                            item.secondPlayer = user;
                            user.IsInGame = true;
                            user.GameID = item.GameID;
                            item.StartTime = DateTime.Now;

                            // Remove game from pendingGame
                            pendingGames.Remove(item);

                            SetStatus(Created);
                            return item.GameID;
                        }
                    }
                }

                // No game exists with user preferences, make new game
                Game game = new Game();
                game.GameState = "pending";
                game.Board = new BoggleBoard().ToString();
                game.TimeLimit = setGame.TimeLimit;
                // May want to change this to a better way for getting game id
                game.GameID = games.Count + 1.ToString();
                game.firstPlayer = user;

                games.Add(game.GameID, game);
                pendingGames.Add(game);
                user.IsInGame = true;
                user.GameID = game.GameID;
                SetStatus(Accepted);
                return game.GameID;

            }
        }


        public void CancelJoinRequest(CancelRequestDetails cancelRequestDetails)
        {
            lock (sync)
            {
                if (!users.ContainsKey(cancelRequestDetails.UserToken))
                {
                    SetStatus(Forbidden);
                    return;
                }
                User user = users[cancelRequestDetails.UserToken];

                if(!user.IsInGame)
                {
                    SetStatus(Forbidden);
                    return;
                }

                if(games[user.GameID].GameState != "pending")
                {
                    SetStatus(Forbidden);
                    return;
                }
                
                pendingGames.Remove(games[user.GameID]);
                games.Remove(user.GameID);
                user.IsInGame = false;
                user.GameID = null;

                SetStatus(OK);
            }
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
