using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace PS8
{
    class GameController
    {
        private Controller controller;
        private IBoggleGame board;
        private Game game;
        private System.Timers.Timer gameTimer;
        private CancellationTokenSource cancelTokenSource;

        public GameController(Controller controller, Game game, BoggleGame board)
        {
            this.controller = controller;
            this.game = game;
            this.board = board;
            gameTimer = new System.Timers.Timer(1000);
            gameTimer.Elapsed += new ElapsedEventHandler(GameTimerElapsed);

            board.EnterPressed += HandlePlayWord;
        }
        public void StartGameTimer()
        {
            gameTimer.Start();
        }

        private void GameTimerElapsed(object source, ElapsedEventArgs e)
        {
            UpdateGame();
        }

        private void UpdateGame()
        {
            controller.FetchGame(true);
            board.UpdateBoard(game);

            if (game.GameState == "completed")
            {
                gameTimer.Stop();
                // TODO: End Game
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="word"></param>
        private async void HandlePlayWord(string word)
        {
            try
            {
                using (HttpClient client = controller.CreateClient())
                {
                    dynamic dynamic = new ExpandoObject();
                    dynamic.Word = word;
                    dynamic.UserToken = controller.userToken;

                    String url = String.Format("games/{0}", game.GameID);
                    cancelTokenSource = new CancellationTokenSource();
                    StringContent content = new StringContent(JsonConvert.SerializeObject(dynamic), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PutAsync(url, content, cancelTokenSource.Token);
                    

                    if (response.IsSuccessStatusCode)
                    {
                        String result = await response.Content.ReadAsStringAsync();
                        int score = JsonConvert.DeserializeObject<dynamic>(result).Score;
                    }
                    else
                    {
                        MessageBox.Show("Error Playing Word: " + response.StatusCode + "\n" + response.ReasonPhrase);
                    }
                }
            }
            catch
            {
                MessageBox.Show("Error Playing Word");
            }
        }

    }
}
