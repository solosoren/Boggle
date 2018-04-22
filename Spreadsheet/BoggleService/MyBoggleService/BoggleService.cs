using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using static System.Net.HttpStatusCode;

namespace MyBoggleService
{
    public class BoggleService
    {
        // Connection string for the Database
        private static string BoggleDB;
        public BoggleService()
        {
            // Fetches connection string web.config
            string dbFolder = System.IO.Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            BoggleDB = String.Format(@"Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename = {0}\BoggleDB.mdf; Integrated Security = True", dbFolder);
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
        //private static void SetStatus(HttpStatusCode status)
        //{
        //    WebOperationContext.Current.OutgoingResponse.StatusCode = status;
        //}

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

        public User CreateUser(User user, out HttpStatusCode status)
        {
            if (!IsNicknameValid(user.Nickname))
            {
                status = Forbidden;
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

                        status = Created;

                        // To avoid rollback after control has left the scope
                        transaction.Commit();

                        return User.CreatedUser(userID);
                    }
                }
            }
        }

        public SetGame JoinGame(SetGame setGame, out HttpStatusCode status)
        {

            if (setGame.UserToken == null)
            {
                status = Forbidden;
                return null;
            }

            if (setGame.TimeLimit < 5 || setGame.TimeLimit > 120)
            {
                status = Forbidden;
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
                                status = Forbidden;
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
                            if (reader.HasRows)
                            {
                                status = Conflict;
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
                                    "insert into Games(Player1, TimeLimit) output inserted.GameID  values (@Player1, @TimeLimit)",
                                    connection,
                                    transaction))
                                {
                                    addCommand.Parameters.AddWithValue("@Player1", setGame.UserToken);
                                    addCommand.Parameters.AddWithValue("@TimeLimit", setGame.TimeLimit);

                                    string ID = addCommand.ExecuteScalar().ToString();
                                    if (ID == null)
                                    {
                                        throw new Exception("Query failed unexpectedly");
                                    }

                                    status = Accepted;
                                    SetGame sg = new SetGame(ID);
                                    reader.Close();
                                    transaction.Commit();
                                    return sg;
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
                                    reader.Read();
                                    Game game = new Game();
                                    int? newTimeLimit = setGame.TimeLimit;
                                    if (reader["TimeLimit"] != null)
                                    {
                                        newTimeLimit = ((int)reader["TimeLimit"] + newTimeLimit) / 2;
                                    }

                                    string id = reader["GameID"].ToString();

                                    joinGameCommand.Parameters.AddWithValue("@Player2", setGame.UserToken);
                                    joinGameCommand.Parameters.AddWithValue("@Board", game.Board);
                                    joinGameCommand.Parameters.AddWithValue("@TimeLimit", newTimeLimit);
                                    joinGameCommand.Parameters.AddWithValue("@StartTime", DateTime.Now);
                                    joinGameCommand.Parameters.AddWithValue("@GameID", id);

                                    reader.Close();

                                    SetGame sg = new SetGame(id);
                                    if (joinGameCommand.ExecuteNonQuery() == 0)
                                    {
                                        status = Forbidden;
                                    }
                                    else
                                    {
                                        status = Created;
                                    }
                                    transaction.Commit();
                                    return sg;
                                }
                            }
                        }
                    }
                }
            }
        }

        // TODO: TEST & Add GameState to Game
        public void CancelJoinRequest(CancelRequestDetails cancelRequestDetails, out HttpStatusCode status)
        {
            lock (sync)
            {
                using (SqlConnection connection = new SqlConnection(BoggleDB))
                {
                    connection.Open();

                    // Transaction for databse commands
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        using (SqlCommand command = new SqlCommand(
                            "select * from Games where Player1 = @UserID and Player2 is NULL",
                            connection,
                            transaction))
                        {
                            command.Parameters.AddWithValue("@UserID", cancelRequestDetails.UserToken);

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (!reader.HasRows)
                                {
                                    // No pending game
                                    status = Forbidden;
                                    reader.Close();
                                    transaction.Commit();
                                    return;
                                }
                            }
                        }

                        using (SqlCommand command = new SqlCommand(
                           "delete from Games where Player1 = @UserID",
                           connection,
                           transaction))
                        {
                            command.Parameters.AddWithValue("@UserID", cancelRequestDetails.UserToken);

                            if (command.ExecuteNonQuery() != 1)
                            {
                                throw new Exception("Query failed unexpectedly");
                            }
                        }

                        status = OK;
                        transaction.Commit();
                    }
                }
            }
        }

        public PlayWordDetails PlayWord(string GameID, PlayWordDetails PlayWordDetails, out HttpStatusCode status)
        {
            string word = PlayWordDetails.Word.Trim();
            PlayedWord playedWord = new PlayedWord(word);

            if (word == "" || word == null || word.Length > 30)
            {
                status = Forbidden;
                return null;
            }

            lock (sync)
            {
                using (SqlConnection connection = new SqlConnection(BoggleDB))
                {
                    connection.Open();
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        // Check if game exists
                        using (SqlCommand command = new SqlCommand(
                                "select * from Games where Games.GameID = @GameID",
                                connection,
                                transaction))
                        {
                            command.Parameters.AddWithValue("@GameID", GameID);

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (!reader.HasRows)
                                {
                                    status = Forbidden;
                                    reader.Close();
                                    transaction.Commit();
                                    return null;
                                }

                                // Game exists, Pending
                                reader.Read();
                                if (reader["Player2"].ToString() == "")
                                {
                                    status = Conflict;
                                    reader.Close();
                                    transaction.Commit();
                                    return null;
                                }

                                // completed
                                DateTime start = (DateTime)reader["StartTime"];
                                int timeLimit = (int)reader["TimeLimit"];
                                if ((start.AddSeconds((double)timeLimit) - DateTime.Now).TotalSeconds <= 0)
                                {
                                    status = Conflict;
                                    reader.Close();
                                    transaction.Commit();
                                    return null;
                                }


                                // User is in this game
                                if (reader["Player1"].ToString() == PlayWordDetails.UserToken ||
                                    reader["Player2"].ToString() == PlayWordDetails.UserToken)
                                {
                                    BoggleBoard board = new BoggleBoard(reader["Board"].ToString());
                                    reader.Close();

                                    List<string> words = new List<string>();

                                    // Get all words
                                    using (SqlCommand wordCommand = new SqlCommand(
                                        "select Word from Words where Player = @Player",
                                        connection,
                                        transaction))
                                    {
                                        wordCommand.Parameters.AddWithValue("@Player", PlayWordDetails.UserToken);

                                        // Get list of words played
                                        using (SqlDataReader wordReader = wordCommand.ExecuteReader())
                                        {
                                            while (wordReader.Read())
                                            {
                                                words.Add(wordReader["Word"].ToString());
                                            }
                                            wordReader.Close();
                                        }

                                        int score = GetWordScore(PlayWordDetails.Word, board, words);

                                        // Add word to word table
                                        using (SqlCommand addWordCommand = new SqlCommand(
                                            "insert into Words (Word, GameID, Player, Score) " +
                                            "values(@Word, @GameID, @Player, @Score)",
                                            connection,
                                            transaction))
                                        {
                                            addWordCommand.Parameters.AddWithValue("@Word", PlayWordDetails.Word);
                                            addWordCommand.Parameters.AddWithValue("@GameID", GameID);
                                            addWordCommand.Parameters.AddWithValue("@Player", PlayWordDetails.UserToken);
                                            addWordCommand.Parameters.AddWithValue("@Score", score);

                                            if (addWordCommand.ExecuteNonQuery() != 1)
                                            {
                                                throw new Exception("Query failed unexpectedly");
                                            }
                                            status = OK;
                                            transaction.Commit();
                                            return new PlayWordDetails(score);
                                        }
                                    }
                                }

                                // User is not in this game
                                else
                                {
                                    status = Forbidden;
                                    reader.Close();
                                    transaction.Commit();
                                    return null;
                                }
                            }
                        }
                    }
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

        public Game GameStatus(string gameID, string brief, out HttpStatusCode status)
        {
            lock (sync)
            {
                using (SqlConnection connection = new SqlConnection(BoggleDB))
                {
                    connection.Open();
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        using (SqlCommand command = new SqlCommand(
                            "select * from Games where GameID = @GameID",
                            connection,
                            transaction))
                        {
                            command.Parameters.AddWithValue("@GameID", gameID);

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (!reader.HasRows)
                                {
                                    // No game with that ID
                                    status = Forbidden;
                                    reader.Close();
                                    return null;
                                }

                                while (reader.Read())
                                {
                                    // pending
                                    if (reader["Player2"].ToString() == "")
                                    {
                                        Game pendingGame = Game.PendingGame();
                                        status = OK;
                                        reader.Close();
                                        transaction.Commit();
                                        return pendingGame;
                                    }

                                    Game game = new Game();
                                    game.TimeLimit = (int)reader["TimeLimit"];
                                    game.SetStartTime((DateTime)reader["StartTime"]);

                                    bool isCompleted = false;
                                    game.GameState = "active";
                                    int left = (int)(game.GetStartTime().AddSeconds((double)game.TimeLimit) - DateTime.Now).TotalSeconds;
                                    if (left <= 0)
                                    {
                                        isCompleted = true;
                                        game.GameState = "completed";
                                    }

                                    string player1ID = (string)reader["Player1"];
                                    string player2ID = (string)reader["Player2"];
                                    User player1 = GetPlayer(player1ID, connection, transaction, brief, isCompleted);
                                    User player2 = GetPlayer(player2ID, connection, transaction, brief, isCompleted);
                                    game.Player1 = player1;
                                    game.Player2 = player2;

                                    if (brief == "yes")
                                    {
                                        status = OK;
                                        reader.Close();
                                        transaction.Commit();
                                        return game.BriefGame();
                                    }

                                    game.Board = (string)reader["Board"];
                                    status = OK;
                                    reader.Close();
                                    transaction.Commit();
                                    return game.StatusLong();
                                }
                            }
                        }
                    }
                }
            }

            status = Forbidden;
            return null;
        }

        /// <summary>
        /// Returns the User from the DataBase
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="brief">If "yes" then only score is set</param>
        /// <param name="isCompleted">If false then only score and nickname is set otherwise score, nickname, and words are set</param>
        /// <returns></returns>
        private User GetPlayer(string userID, SqlConnection connection, SqlTransaction transaction, string brief, Boolean isCompleted)
        {
            User player = new User();
            using (SqlCommand command = new SqlCommand(
                            "select * from Users where UserID = @UserID",
                            connection, transaction))
            {
                command.Parameters.AddWithValue("@UserID", userID);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        throw new Exception("Query failed unexpectedly");
                    }

                    player.Score = 0;
                    List<PlayedWord> words = GetWords(userID, connection, transaction);
                    if (words.Count() > 0)
                    {
                        foreach (PlayedWord word in words)
                        {
                            player.Score += word.Score;
                        }
                        if (isCompleted)
                        {
                            player.WordsPlayed = words;
                        }
                    }

                    while (reader.Read())
                    {
                        if (brief != "yes")
                        {
                            player.Nickname = reader["Nickname"].ToString();
                        }
                    }
                }
            }

            return player;
        }

        /// <summary>
        /// Returns the Played words for the UserID Given
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private List<PlayedWord> GetWords(string userID, SqlConnection connection, SqlTransaction transaction)
        {
            List<PlayedWord> words = new List<PlayedWord>();
            using (SqlCommand command = new SqlCommand(
                            "select * from Words where Player = @UserID",
                            connection, transaction))
            {
                command.Parameters.AddWithValue("@UserID", userID);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PlayedWord word = new PlayedWord(reader["Word"].ToString());
                        word.Score = (int)reader["Score"];
                        words.Add(word);
                    }
                }
            }

            return words;
        }


    }

}

