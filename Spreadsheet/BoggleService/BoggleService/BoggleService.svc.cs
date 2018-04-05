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

        public string JoinGame(SetGame setGame)
        {

            if (setGame.UserToken == null)
            {
                SetStatus(Forbidden);
                return null;
            }

            if (setGame.TimeLimit < 5 || setGame.TimeLimit > 120)
            {
                SetStatus(Forbidden);
                return null;
            }

            using (SqlConnection connection = new SqlConnection(BoggleDB))
            {
                connection.Open();

                // Transaction for databse commands
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    // SQL command to run
                    // To check if user ID is valid
                    using (SqlCommand command = new SqlCommand(
                        "select UserID from Users where UserID = @UserID",
                        connection,
                        transaction))
                    {
                        command.Parameters.AddWithValue("@UserID", setGame.UserToken);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // User ID is invalid
                            if (!reader.HasRows)
                            {
                                SetStatus(Forbidden);
                                reader.Close();
                                transaction.Commit();
                                return null;
                            }
                        }
                    }

                    // SQL command to run
                    // To check if user is in game
                    using (SqlCommand command = new SqlCommand(
                        "select * from Games where Games.Player1 = @UserID or Games.Player2 = @UserID",
                        connection,
                        transaction))
                    {
                        command.Parameters.AddWithValue("@UserID", setGame.UserToken);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                SetStatus(Conflict);
                                reader.Close();
                                transaction.Commit();
                                return null;
                            }
                        }
                    }

                    // If there is a user waiting with the same time limit, add current user to game
                    using (SqlCommand command = new SqlCommand(
                        "select * from Games where Games.Player2 is NULL",
                        connection,
                        transaction))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // No pending games to join, make a new pending game
                            if (!reader.HasRows)
                            {
                                using (SqlCommand addCommand = new SqlCommand(
                                    "insert into Games(Player1) values (@Player1) output inserted.GameID",
                                    connection,
                                    transaction))
                                {
                                    addCommand.Parameters.AddWithValue("@Player1", setGame.UserToken);

                                    if (addCommand.ExecuteNonQuery() != 1)
                                    {
                                        throw new Exception("Query failed unexpectedly");
                                    }

                                    string gameID = addCommand.ExecuteScalar().ToString();
                                    SetStatus(Accepted);
                                    transaction.Commit();
                                    return gameID;
                                }
                            }
                            else
                            {
                                // reader consists of gameID and player1
                                // need to set Player2, Board, TimeLimit, StartTime
                                using (SqlCommand joinGameCommand = new SqlCommand(
                                    "update Games set Player2 = @Player2, Board = @Board, TimeLimit = @TimeLimit," +
                                    " StartTime = @StartTime where GameID = @GameID",
                                    connection,
                                    transaction))
                                {
                                    Game game = new Game();
                                    joinGameCommand.Parameters.AddWithValue("@Player2", setGame.UserToken);
                                    joinGameCommand.Parameters.AddWithValue("@Board", game.Board);
                                    joinGameCommand.Parameters.AddWithValue("@TimeLimit", setGame.TimeLimit);
                                    joinGameCommand.Parameters.AddWithValue("@StartTime", game.GetStartTime());
                                    joinGameCommand.Parameters.AddWithValue("@GameID", reader["GameID"].ToString());

                                    if (command.ExecuteNonQuery() == 0)
                                    {
                                        SetStatus(Forbidden);
                                    }
                                    else
                                    {
                                        SetStatus(Created);
                                    }

                                    return reader["GameID"].ToString();
                                }
                            }
                        }
                    }
                }
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
