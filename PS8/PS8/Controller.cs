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
            view.RegisterPressed += HandleRegister;
            view.CancelPressed += HandleCancel;
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
    }
}