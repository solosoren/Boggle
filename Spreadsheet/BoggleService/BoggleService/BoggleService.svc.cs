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
                            item.SecondPlayer = user;
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
                BoggleBoard board = new BoggleBoard();
                game.TimeLimit = setGame.TimeLimit;
                // May want to change this to a better way for getting game id
                game.GameID = games.Count + 1.ToString();
                game.FirstPlayer = user;
                game.FirstPlayerWords = null;
                game.SecondPlayerWords = null;

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

                if (!user.IsInGame)
                {
                    SetStatus(Forbidden);
                    return;
                }

                if (games[user.GameID].GameState != "pending")
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

        public int PlayWord(string GameID, PlayWordDetails PlayWordDetails)
        {
            lock (sync)
            {
                string word = PlayWordDetails.Word.Trim();

                if (word == "" || word == null || word.Length > 30)
                {
                    SetStatus(Forbidden);
                    return 0;
                }

                if (!games.ContainsKey(GameID) || !users.ContainsKey(PlayWordDetails.UserToken))
                {
                    SetStatus(Forbidden);
                    return 0;
                }

                Game game = games[GameID];

                if (game.GameState != "active")
                {
                    SetStatus(Conflict);
                    return 0;
                }

                if (game.FirstPlayer.UserToken == PlayWordDetails.UserToken)
                {
                    game.FirstPlayerWords.Add(word);
                    // Add score here
                    int score = GetWordScore(word, game.BoggleBoard, game.FirstPlayerWords);
                    game.FirstPlayer.Score += score;
                    return score;
                }
                else
                {
                    game.SecondPlayerWords.Add(word);
                    int score = GetWordScore(word, game.BoggleBoard, game.SecondPlayerWords);
                    game.SecondPlayer.Score += score;
                    return score;
                }
            }
        }

        /// <summary>
        /// Returns score of given word whilist accounting for words played
        /// </summary>
        /// <param name="word"></param>
        /// <param name="board"></param>
        /// <param name="playerWordList"></param>
        /// <returns></returns>
        private int GetWordScore(string word, BoggleBoard board, List<string> playerWordList)
        {
            // Check if word can be formed
            if (!board.CanBeFormed(word))
            {
                return 0;
            }

            // Check if word is legal
            string contents = File.ReadAllText("./dictionary.txt");
            if (contents.Contains(word))
            {
                if (word.Length < 3)
                {
                    return 0;
                }
                if (playerWordList.Contains(word))
                {
                    return 0;
                }
                switch (word.Length)
                {
                    case 3:
                    case 4:
                        return 1;
                    case 5:
                        return 2;
                    case 6:
                        return 3;
                    case 7:
                        return 5;
                    // In this case, default is being used for if word length > 7
                    // Must be a better way to do this
                    default:
                        return 11;
                }
            }

            return -1;

        }

        public Game GameStatus(string GameID, string brief)
        {
            throw new NotImplementedException();
        }
    }
}
