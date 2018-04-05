using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Web;
using static System.Net.HttpStatusCode;

namespace Boggle
{
    public class BoggleService : IBoggleService
    {
        // Connection string for the Database
        private static string BoggleDB;
        public BoggleService()
        {
            // Fetches connection string web.config
            BoggleDB = ConfigurationManager.ConnectionStrings["BoggleDB"].ConnectionString;
        }

        private readonly static Dictionary<String, User> users = new Dictionary<string, User>();
        // Games only contain active and completed
        private readonly static Dictionary<String, Game> games = new Dictionary<string, Game>();
        private readonly static HashSet<Game> pendingGames = new HashSet<Game>();
        private readonly static HashSet<User> pendingUsers = new HashSet<User>();
        private readonly static HashSet<int> pendingTimeLimits = new HashSet<int>();
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
            if (!IsNicknameValid(user.Nickname))
            {
                SetStatus(Forbidden);
                return null;
            }

            // Connection to database
            using (SqlConnection connection = new SqlConnection(BoggleDB))
            {
                connection.Open();

                // Transaction for databse commands
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    // SQL command to run
                    using (SqlCommand command = new SqlCommand(
                        "insert into Users (UserID, Nickname) values(@UserID, @Nickname)",
                        connection,
                        transaction))
                    {
                        string userID = Guid.NewGuid().ToString();

                        command.Parameters.AddWithValue("@UserID", userID);
                        command.Parameters.AddWithValue("@Nickname", user.Nickname);

                        if (command.ExecuteNonQuery() != 1)
                        {
                            throw new Exception("Query failed unexpectedly");
                        }

                        SetStatus(Created);

                        // To avoid rollback after control has left the scope
                        transaction.Commit();
                        return userID;
                    }
                }
            }
        }

        public SetGame JoinGame(SetGame setGame)
        {
            lock (sync)
            {
                if (setGame.UserToken == null)
                {
                    SetStatus(Forbidden);
                    return null;
                }
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
                if (user.InGame())
                {
                    SetStatus(Conflict);
                    return null;
                }

                // If there is a user waiting with the same time limit, add current user to game
                if (pendingGames.Count != 0)
                {
                    Game newGame = pendingGames.First();

                    // start pending game
                    newGame.Player1 = pendingUsers.First();
                    user.IsInGame = true;
                    user.GameID = newGame.GameID;
                    newGame.Player2 = user;

                    newGame.TimeLimit = (pendingTimeLimits.First() + (setGame.TimeLimit ?? 0)) / 2;
                    newGame.SetStartTime();
                    newGame.GameID = newGame.Player1.GameID;
                    newGame.GameState = "active";

                    // Remove game from pendingGame
                    pendingGames.Remove(newGame);
                    pendingUsers.Remove(pendingUsers.First());
                    pendingTimeLimits.Remove(pendingTimeLimits.First());

                    SetStatus(Created);
                    SetGame sg = new SetGame(newGame.GameID);
                    return sg;
                }

                user.IsInGame = true;
                string GameID = (games.Count + 1).ToString();
                // May want to change this to a better way for getting game id
                user.GameID = GameID;
                pendingUsers.Add(user);

                // No game exists with user preferences, make new game
                Game game = new Game();
                game.GameState = "pending";
                game.GameID = GameID;

                pendingTimeLimits.Add(setGame.TimeLimit ?? 0);


                games.Add(game.GameID, game);
                pendingGames.Add(game);

                SetStatus(Accepted);
                SetGame set = new SetGame(game.GameID);
                return set;

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

                if (!user.InGame())
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
                pendingUsers.Remove(user);
                pendingTimeLimits.Remove(pendingTimeLimits.First());
                games.Remove(user.GameID);
                user.IsInGame = false;
                user.GameID = null;

                SetStatus(OK);
            }
        }

        public PlayWordDetails PlayWord(string GameID, PlayWordDetails PlayWordDetails)
        {
            lock (sync)
            {
                string word = PlayWordDetails.Word.Trim();
                PlayedWord playedWord = new PlayedWord(word);

                if (word == "" || word == null || word.Length > 30)
                {
                    SetStatus(Forbidden);
                    return null;
                }

                Game game = games[GameID];

                if (game.GameState != "active" || (int)(game.GetStartTime().AddSeconds((double)game.TimeLimit) - DateTime.Now).TotalSeconds <= 0)
                {
                    SetStatus(Conflict);
                    return null;
                }

                if (!games.ContainsKey(GameID) || !users.ContainsKey(PlayWordDetails.UserToken) || (game.Player1.UserToken != PlayWordDetails.UserToken && game.Player2.UserToken != PlayWordDetails.UserToken))
                {
                    SetStatus(Forbidden);
                    return null;
                }

                if (game.Player1.UserToken == PlayWordDetails.UserToken)
                {
                    // Add score here
                    if (game.Player1.WordsPlayed == null)
                    {
                        game.Player1.WordsPlayed = new List<PlayedWord>();
                    }

                    List<string> words = new List<string>();
                    foreach (PlayedWord played in game.Player1.WordsPlayed)
                    {
                        words.Add(played.Word);
                    }

                    playedWord.Score = GetWordScore(word, game.BoggleBoard, words);
                    if (!game.Player1.WordsPlayed.Contains(playedWord))
                    {
                        game.Player1.WordsPlayed.Add(playedWord);
                    }
                    game.Player1.Score += playedWord.Score;

                    PlayWordDetails playWordDetails = new PlayWordDetails(playedWord.Score);
                    return playWordDetails;
                }
                else
                {
                    if (game.Player2.WordsPlayed == null)
                    {
                        game.Player2.WordsPlayed = new List<PlayedWord>();
                    }

                    List<string> words = new List<string>();
                    foreach (PlayedWord played in game.Player2.WordsPlayed)
                    {
                        words.Add(played.Word);
                    }

                    playedWord.Score = GetWordScore(word, game.BoggleBoard, words);
                    if (!game.Player2.WordsPlayed.Contains(playedWord))
                    {
                        game.Player2.WordsPlayed.Add(playedWord);
                    }

                    game.Player2.Score += playedWord.Score;

                    PlayWordDetails playWordDetails = new PlayWordDetails(playedWord.Score);
                    return playWordDetails;
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
            PlayedWord playedWord = new PlayedWord(word);
            if (word.Length < 3)
            {
                return 0;
            }

            // Check if word can be formed
            if (!board.CanBeFormed(word))
            {
                return -1;
            }

            // Check if word is legal
            string contents = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"/dictionary.txt");

            if (contents.Contains(word))
            {
                if (playerWordList.Contains(playedWord.Word))
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


        public Game GameStatus(string gameID, string brief)
        {
            if (!games.ContainsKey(gameID))
            {
                SetStatus(Forbidden);
                return null;
            }
            Game game = games[gameID];

            if (pendingGames.Contains(game))
            {
                Game pendingGame = new Game();
                pendingGame.GameState = "pending";
                SetStatus(OK);
                return pendingGame;
            }
            if (brief == "yes")
            {
                if (game.GameState == "active")
                {
                    SetStatus(OK);
                    return game.BriefGame();
                }
            }
            if (brief != "yes")
            {
                if (game.GameState == "active")
                {
                    SetStatus(OK);
                    return game.ActiveStatusLong();
                }
                else
                {
                    SetStatus(OK);
                    return game.CompletedStatusLong();
                }

            }

            SetStatus(Forbidden);
            return null;
        }
    }
}
