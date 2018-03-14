using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;

namespace PS8
{

    /// <summary>
    /// Controller for the BoggleClient
    /// </summary>
    class Controller
    {
        /// <summary>
        /// The Boggle client view controlled by this controller
        /// </summary>
        private IBoggleClient view;

        /// <summary>
        /// The domain address provided by the view
        /// </summary>
        private string domainAddress;

        /// <summary>
        /// The player name provided by the view
        /// </summary>
        private string playerName;

        private string userToken;

        private System.Timers.Timer pregameTimer;

        private Game game;

        /// <summary>
        /// For cancelling the current operation
        /// </summary>
        private CancellationToken cancelToken;

        public Controller(IBoggleClient view)
        {
            this.view = view;
            domainAddress = "";
            playerName = "";
            userToken = "";
            cancelToken = new CancellationToken();

            pregameTimer = new System.Timers.Timer(1000);
            pregameTimer.Elapsed += new ElapsedEventHandler(PregameTimerElapsed);

            view.RegisterPressed += HandleRegister;
            view.RegisterCancelPressed += HandleRegisterCancel;
            view.JoinGameCancelPressed += HandleJoinGameCancel;
            view.JoinGamePressed += HandleJoinGame;
        }

        private void HandleRegisterCancel()
        {
            cancelToken.ThrowIfCancellationRequested();
            view.SetControlState(true);

            // Just for debugging. Delete later.
            MessageBox.Show("Cancelled registration");
        }

        private async void HandleJoinGameCancel()
        {
            try
            {
                using (HttpClient client = CreateClient())
                {
                    dynamic user = new ExpandoObject();
                    user.UserToken = userToken;

                    cancelToken = new CancellationToken();
                    StringContent content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PutAsync("games", content, cancelToken);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Cancelled game request");
                        view.SetJoinGameControlState(true);
                    }
                }
            }
            catch { }
        }

        private async void HandleRegister(string domainName, string playerName)
        {
            domainAddress = domainName;

            try
            {
                view.SetControlState(false);
                using (HttpClient client = CreateClient())
                {
                    dynamic user = new ExpandoObject();
                    user.Nickname = playerName.Trim();

                    cancelToken = new CancellationToken();
                    StringContent content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync("users", content, cancelToken);

                    if (response.IsSuccessStatusCode)
                    {
                        String result = await response.Content.ReadAsStringAsync();
                        userToken = JsonConvert.DeserializeObject<dynamic>(result).UserToken;
                        view.IsUserRegistered = true;
                    }
                    else
                    {
                        MessageBox.Show("Error registering : " + response.StatusCode);
                    }
                }
            }
            catch (TaskCanceledException)
            {

            }
            finally
            {
                view.SetControlState(true);
            }

        }

        /// <summary>
        /// Creates an HttpClient for communicating with the server
        /// </summary>
        /// <returns></returns>
        private HttpClient CreateClient()
        {
            HttpClient client = new HttpClient();
            // Added for debugging purposes
            if (domainAddress.Trim().Equals(""))
            {
                domainAddress = "http://ice.eng.utah.edu";
            }

            client.BaseAddress = new Uri(domainAddress + "/BoggleService.svc/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            return client;
        }

        /// <summary>
        /// Joins a game using the given length of time
        /// </summary>
        /// <param name="length"></param>
        private async void HandleJoinGame(int length)
        {
            try
            {
                view.SetJoinGameControlState(false);
                using (HttpClient client = CreateClient())
                {
                    dynamic dynamic = new ExpandoObject();
                    dynamic.TimeLimit = length;
                    dynamic.UserToken = userToken;

                    cancelToken = new CancellationToken();
                    StringContent content = new StringContent(JsonConvert.SerializeObject(dynamic), Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync("games", content, cancelToken);

                    if (response.IsSuccessStatusCode)
                    {
                        String result = await response.Content.ReadAsStringAsync();
                        string gameID = JsonConvert.DeserializeObject<dynamic>(result).GameID;
                        game = new Game(gameID);
                        pregameTimer.Start();
                    }
                    else
                    {
                        view.SetJoinGameControlState(true);
                        MessageBox.Show("Error Joining Game: " + response.StatusCode + "\n" + response.ReasonPhrase);
                    }
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// Game will start here along with the gui
        /// </summary>
        private void StartGame()
        {
            // Just a place holder
            MessageBox.Show(view.IsInActiveGame.ToString());
            Application.Run(new BoggleGame());
        }

        /// <summary>
        /// fired every second from the pregame timer when waiting for a new game
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void PregameTimerElapsed(object source, ElapsedEventArgs e)
        {
            CheckIfStarted();
        }


        /// <summary>
        /// loads the initial game
        /// </summary>
        /// <returns>returns true if the game has started</returns>
        private async void CheckIfStarted()
        {
            using (HttpClient client = CreateClient())
            {
                // Compose and send the request
                cancelToken = new CancellationToken();
                String url = String.Format("games/{0}?Brief=no", game.GameID);

                HttpResponseMessage response = await client.GetAsync(url, cancelToken);

                // Deal with the response
                if (response.IsSuccessStatusCode)
                {
                    String result = response.Content.ReadAsStringAsync().Result;
                    dynamic dynamic = JsonConvert.DeserializeObject<dynamic>(result);
                    game.SetState((string)dynamic.GameState);

                    if (game.GameState == "active")
                    {
                        game.StartGame(dynamic);
                        pregameTimer.Stop();
                        view.IsInActiveGame = true;
                        MessageBox.Show("You have joined a game");
                        StartGame();
                    }
                }
                else
                {
                    MessageBox.Show("Error getting game: " + response.StatusCode + "\n" + response.ReasonPhrase);
                }
            }
        }

        /// <summary>
        /// Store everything that is apart of the game. Contains internal Player Struct.
        /// </summary>
        class Game
        {
            public string GameID { get; private set; }
            public string GameState { get; private set; }
            public int TimeLeft { get; private set; }
            public int TimeLimit { get; private set; }
            public Player Player1 { get; private set; }

            public Player Player2 { get; private set; }

            /// <summary>
            /// Creates a new game with the GameID
            /// </summary>
            /// <param name="gameID"></param>
            public Game(string gameID)
            {
                GameID = gameID;
            }

            /// <summary>
            /// Set the game state
            /// </summary>
            /// <param name="gameState"></param>
            public void SetState(string gameState)
            {
                GameState = gameState;
            }

            /// <summary>
            /// Start a game by storing all the game properties
            /// </summary>
            /// <param name="d"> an object from the client containing the game properties</param>
            public void StartGame(dynamic d)
            {
                GameState = d.GameState;
                TimeLeft = d.TimeLeft;
                TimeLimit = d.TimeLimit;

                dynamic player1 = d.Player1;
                Player1 = new Player((string)player1.Nickname, (int)player1.Score);

                dynamic player2 = d.Player2;
                Player2 = new Player((string)player2.Nickname, (int)player2.Score);
            }

            /// <summary>
            /// Update both scores
            /// </summary>
            /// <param name="player1Score"></param>
            /// <param name="player2Score"></param>
            public void UpdateScore(int player1Score, int player2Score)
            {
                Player1.SetScore(player1Score);
                Player2.SetScore(player2Score);
            }

            /// <summary>
            /// Used to separate players and scores cleanly
            /// </summary>
            public struct Player
            {
                public string NickName { get; private set; }
                public int Score { get; private set; }

                /// <summary>
                /// Creates a new player
                /// </summary>
                /// <param name="nickName"></param>
                /// <param name="score"></param>
                public Player(string nickName, int score)
                {
                    NickName = nickName;
                    Score = score;
                }

                /// <summary>
                /// Sets the players score
                /// </summary>
                /// <param name="score"></param>
                public void SetScore(int score)
                {
                    Score = score;
                }
            }


        }


    }
}