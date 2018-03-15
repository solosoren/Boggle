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

        private GameController gameController;

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
        private CancellationTokenSource cancelTokenSource;

        public Controller(IBoggleClient view)
        {
            this.view = view;
            domainAddress = "";
            playerName = "";
            userToken = "";
            cancelTokenSource = new CancellationTokenSource();

            pregameTimer = new System.Timers.Timer(1000);
            pregameTimer.Elapsed += new ElapsedEventHandler(PregameTimerElapsed);

            view.RegisterPressed += HandleRegister;
            view.RegisterCancelPressed += HandleRegisterCancel;
            view.JoinGameCancelPressed += HandleJoinGameCancel;
            view.JoinGamePressed += HandleJoinGame;
        }

        private void HandleRegisterCancel()
        {
            cancelTokenSource.Cancel();
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

                    cancelTokenSource = new CancellationTokenSource();
                    StringContent content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PutAsync("games", content, cancelTokenSource.Token);

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

                    cancelTokenSource = new CancellationTokenSource();
                    StringContent content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync("users", content, cancelTokenSource.Token);

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

                    cancelTokenSource = new CancellationTokenSource();
                    StringContent content = new StringContent(JsonConvert.SerializeObject(dynamic), Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync("games", content, cancelTokenSource.Token);

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
            BoggleGame board = new BoggleGame(game);
            gameController = new GameController(this, game, board);
            gameController.StartGameTimer();
            Application.Run(board);
        }

        public async void FetchGame(bool isStarted)
        {
            try
            {
                using (HttpClient client = CreateClient())
                {
                    // Compose and send the request
                    cancelTokenSource = new CancellationTokenSource();
                    String url = String.Format("games/{0}", game.GameID);

                    HttpResponseMessage response = await client.GetAsync(url, cancelTokenSource.Token);

                    // Deal with the response
                    if (response.IsSuccessStatusCode)
                    {
                        String result = response.Content.ReadAsStringAsync().Result;
                        dynamic dynamic = JsonConvert.DeserializeObject<dynamic>(result);
                        game.SetState((string)dynamic.GameState);
                        game.Board = (string)dynamic.Board;
                        if (!isStarted)
                        {
                            game.StartGame(dynamic);
                        }
                        else if (((string)dynamic.GameState).Equals("active"))
                        {
                            dynamic player1 = dynamic.Player1;
                            dynamic player2 = dynamic.Player2;
                            if (player1.Player1Score == null)
                            {
                                player1.Player1Score = 0;
                            }
                            if (player2.Player2Score == null)
                            {
                                player2.Player2Score = 0;
                            }
                            game.UpdateScore((int)player1.Player1Score, (int)player2.Player2Score);
                            game.UpdateTime((int)dynamic.TimeLeft);
                        }
                        else if (((string)dynamic.GameState).Equals("completed"))
                        {
                            MessageBox.Show("Completed.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Error loading game: " + response.StatusCode + "\n" + response.ReasonPhrase);
                    }
                }
            }
            catch (TaskCanceledException e)
            {

            }

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
        private void CheckIfStarted()
        {
            FetchGame(false);
            if (game.GameState == "active")
            {
                pregameTimer.Stop();
                view.IsInActiveGame = true;
                StartGame();
            }
        }
    }
}