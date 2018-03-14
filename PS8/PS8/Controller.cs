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
        private CancellationTokenSource tokenSource;

        public Controller(IBoggleClient view)
        {
            this.view = view;
            domainAddress = "";
            playerName = "";
            userToken = "";

            pregameTimer = new System.Timers.Timer(1000);
            pregameTimer.Elapsed += new ElapsedEventHandler(PregameTimerElapsed);

            view.RegisterPressed += HandleRegister;
            view.CancelPressed += HandleCancel;
            view.JoinGamePressed += HandleJoinGame;
        }

        private void HandleCancel()
        {
            Console.WriteLine("Cancelled");
            tokenSource.Cancel();
            view.SetControlState(true);

            // Just for debugging. Delete later.
            MessageBox.Show("Cancelled");
        }

        //

        private async void HandleRegister(string domainName, string playerName)
        {
            domainAddress = domainName;

            try
            {
                view.SetControlState(false);
                using (HttpClient client = CreateClient())
                {
                    dynamic user = new ExpandoObject();
                    user.Nickname = playerName;

                    tokenSource = new CancellationTokenSource();
                    StringContent content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync("users", content, tokenSource.Token);


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
            if (domainAddress.Equals(""))
            {
                domainAddress = "http://ice.eng.utah.edu";
            }

            //TODO(Kunaal) : Make domain address reselient to prefix and suffix.
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
                using (HttpClient client = CreateClient())
                {
                    dynamic dynamic = new ExpandoObject();
                    dynamic.TimeLimit = length;
                    dynamic.UserToken = userToken;

                    tokenSource = new CancellationTokenSource();
                    StringContent content = new StringContent(JsonConvert.SerializeObject(dynamic), Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync("games", content, tokenSource.Token);

                    if (response.IsSuccessStatusCode)
                    {
                        String result = await response.Content.ReadAsStringAsync();
                        string gameID = JsonConvert.DeserializeObject<dynamic>(result).GameID;
                        game = new Game(gameID);
                        pregameTimer.Start();
                    }
                    else
                    {
                        MessageBox.Show("Error Joining Game: " + response.StatusCode + "\n" + response.ReasonPhrase);
                    }
                }
            }
            finally
            {

            }
        }

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
                tokenSource = new CancellationTokenSource();
                String url = String.Format("games/{0}?Brief=no", game.GameID);

                HttpResponseMessage response = await client.GetAsync(url, tokenSource.Token);

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
                        MessageBox.Show("You have joined a game");
                    }
                }
                else
                {
                    MessageBox.Show("Error getting game: " + response.StatusCode + "\n" + response.ReasonPhrase);
                }
            }
        }


        class Game : Object
        {
            public string GameID { get; private set; }
            public string GameState { get; private set; }
            public int TimeLeft { get; private set; }
            public int TimeLimit { get; private set; }
            public Player Player1 { get; private set; }

            public Player Player2 { get; private set; }

            public Game(string gameID)
            {
                GameID = gameID;
            }

            public void SetState(string gameState)
            {
                GameState = gameState;
            }

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

            public void UpdateScore(int player1Score, int player2Score)
            {
                Player1.SetScore(player1Score);
                Player2.SetScore(player2Score);
            }


            public struct Player
            {
                public string NickName { get; private set; }
                public int Score { get; private set; }

                public Player(string nickName, int score)
                {
                    NickName = nickName;
                    Score = score;
                }

                public void SetScore(int score)
                {
                    Score = score;
                }
            }


        }
        

    }
}