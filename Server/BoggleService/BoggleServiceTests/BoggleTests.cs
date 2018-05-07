using Microsoft.VisualStudio.TestTools.UnitTesting;
using static System.Net.HttpStatusCode;
using System.Diagnostics;
using System.Net.Http;
using System;
using System.Dynamic;
using System.IO;

namespace Boggle
{
    /// <summary>
    /// Provides a way to start and stop the IIS web server from within the test
    /// cases.  If something prevents the test cases from stopping the web server,
    /// subsequent tests may not work properly until the stray process is killed
    /// manually.
    /// </summary>
    public static class IISAgent
    {
        // Reference to the running process
        private static Process process = null;

        /// <summary>
        /// Starts IIS
        /// </summary>
        public static void Start(string arguments)
        {
            if (process == null)
            {
                ProcessStartInfo info = new ProcessStartInfo(Properties.Resources.IIS_EXECUTABLE, arguments);
                info.WindowStyle = ProcessWindowStyle.Minimized;
                info.UseShellExecute = false;
                process = Process.Start(info);
            }
        }

        /// <summary>
        ///  Stops IIS
        /// </summary>
        public static void Stop()
        {
            if (process != null)
            {
                process.Kill();
            }
        }
    }
    [TestClass]
    public class BoggleTests
    {
        /// <summary>
        /// This is automatically run prior to all the tests to start the server
        /// </summary>
        //[ClassInitialize()]
        //public static void StartIIS(TestContext testContext)
        //{
        //    IISAgent.Start(@"/site:""BoggleService"" /apppool:""Clr4IntegratedAppPool"" /config:""..\..\..\.vs\config\applicationhost.config""");
        //}

        /// <summary>
        /// This is automatically run when all tests have completed to stop the server
        /// </summary>
        //[ClassCleanup()]
        //public static void StopIIS()
        //{
        //    IISAgent.Stop();
        //}

        private RestTestClient client = new RestTestClient("http://localhost:60000/BoggleService.svc/");

        /// <summary>
        /// Note that DoGetAsync (and the other similar methods) returns a Response object, which contains
        /// the response Stats and the deserialized JSON response (if any).  See RestTestClient.cs
        /// for details.
        /// </summary>
        //[TestMethod]
        //public void TestMethod1()
        //{
        //    Response r = client.DoGetAsync("word?index={0}", "-5").Result;
        //    Assert.AreEqual(Forbidden, r.Status);

        //    r = client.DoGetAsync("word?index={0}", "5").Result;
        //    Assert.AreEqual(OK, r.Status);

        //    string word = (string) r.Data;
        //    Assert.AreEqual("AAL", word);
        //}

        static private string GameID;
        static private string UserToken1;
        static private string UserToken2;

        [TestMethod]
        public void TestCreateUserAndJoinGame()
        {
            dynamic dynamic = new ExpandoObject();
            dynamic.Nickname = "nate";
            
            Response r = client.DoPostAsync("users", dynamic).Result;
            UserToken1 = (string)r.Data;
            Assert.AreEqual(Created, r.Status);

            dynamic d = new ExpandoObject();
            d.UserToken = UserToken1;
            d.TimeLimit = 45;

            Response response = client.DoPostAsync("games", d).Result;
            GameID = response.Data;
            Assert.AreEqual(Accepted, response.Status);

            dynamic = new ExpandoObject();
            dynamic.Nickname = "dan";

            r = client.DoPostAsync("users", dynamic).Result;
            UserToken2 = r.Data;
            Assert.AreEqual(Created, r.Status);

            d = new ExpandoObject();
            d.UserToken = UserToken2;
            d.TimeLimit = 45;

            response = client.DoPostAsync("games", d).Result;
            Assert.AreEqual(Created, response.Status);

            TestGameStatus();

            using (StreamReader words = new StreamReader("C:\\Users\\Soren\\source\\repos\\NelsonAndKumar2\\Spreadsheet\\BoggleService\\BoggleService\\dictionary.txt"))
            {
                while (!words.EndOfStream)
                {
                    string word = words.ReadLine();
                    d = new ExpandoObject();
                    d.UserToken = UserToken1;
                    d.Word = word;

                    string url = "games/" + GameID;
                    Response res = client.DoPutAsync(d, url).Result;
                }
                
            }
        }

        [TestMethod]
        public void TestCreateUserForbidden()
        {
            dynamic dynamic = new ExpandoObject();
            dynamic.Nickname = " ";

            Response r = client.DoPostAsync("users", dynamic).Result;
            Assert.AreEqual(Forbidden, r.Status);
        }

        [TestMethod]
        public void TestCreateUserForbidden2()
        {
            dynamic dynamic = new ExpandoObject();

            Response r = client.DoPostAsync("users", dynamic).Result;
            Assert.AreEqual(Forbidden, r.Status);
        }

        [TestMethod]
        public void TestCreateUserForbidden3()
        {
            dynamic dynamic = new ExpandoObject();
            dynamic.Nickname = "jjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjj";

            Response r = client.DoPostAsync("users", dynamic).Result;
            Assert.AreEqual(Forbidden, r.Status);
        }

        [TestMethod]
        public void TestJoinGameForbidden()
        {
            dynamic dynamic = new ExpandoObject();
            dynamic.UserToken = UserToken1;
            dynamic.TimeLimit = 145;

            Response r = client.DoPostAsync("users", dynamic).Result;
            Assert.AreEqual(Forbidden, r.Status);
        }

        [TestMethod]
        public void TestJoinGameForbidden2()
        {
            dynamic dynamic = new ExpandoObject();
            dynamic.UserToken = " ";
            dynamic.TimeLimit = 50;

            Response r = client.DoPostAsync("users", dynamic).Result;
            Assert.AreEqual(Forbidden, r.Status);
        }

        [TestMethod]
        public void TestJoinGameForbidden3()
        {
            dynamic dynamic = new ExpandoObject();
            dynamic.UserToken = UserToken1;
            dynamic.TimeLimit = 45;

            Response r = client.DoPostAsync("games", dynamic).Result;
            Assert.AreEqual(Conflict, r.Status);
        }

        private void TestGameStatus()
        {

            Response r = client.DoGetAsync("games/{0}", GameID).Result;
            Assert.AreEqual(OK, r.Status);
        }

    }
}
