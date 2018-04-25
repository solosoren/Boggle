using CustomNetworking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MyBoggleService
{
    class Program
    {
        static void Main(string[] args)
        {
            SSListener server = new SSListener(60000, Encoding.UTF8);
            server.Start();
            server.BeginAcceptSS(ConnectionMade, server);
            Console.ReadLine();
        }

        private static void ConnectionMade(SS ss, object payload)
        {
            SSListener server = (SSListener)payload;
            server.BeginAcceptSS(ConnectionMade, server);
            new RequestHandler(ss);
        }

        private class RequestHandler
        {
            private SS ss;

            private string firstLine;

            private int contentLength;

            private string gameID;

            // TODO: Regex for other service calls
            private static readonly Regex createUserPattern = new Regex(@"^POST /BoggleService.svc/users HTTP");
            private static readonly Regex joinGamePattern = new Regex(@"^POST /BoggleService.svc/games HTTP");
            private static readonly Regex gameStatusPattern = new Regex(@"^GET /BoggleService.svc/games/(.+) HTTP");
            private static readonly Regex cancelRequestPattern = new Regex(@"^PUT /BoggleService.svc/games HTTP");
            private static readonly Regex playWordPattern = new Regex(@"^PUT /BoggleService.svs/games[/]([\d]+) HTTP");
            private static readonly Regex contentLengthPattern = new Regex(@"^content-length: (\d+)", RegexOptions.IgnoreCase);

            public RequestHandler(SS ss)
            {
                this.ss = ss;
                this.contentLength = 0;
                ss.BeginReceive(ReadLines, null);
            }

            private void ReadLines(String line, object p)
            {
                // End of request
                if (line.Trim().Length == 0 && contentLength > 0)
                {
                    ss.BeginReceive(ProcessRequest, null, contentLength);
                }

                // No object at end of Request
                else if (line.Trim().Length == 0)
                {
                    ProcessRequest(null);
                }

                // Middle Line: looking for contentLength
                else if (firstLine != null)
                {
                    Match m = contentLengthPattern.Match(line);
                    if (m.Success)
                    {
                        contentLength = int.Parse(m.Groups[1].ToString());
                    }
                    ss.BeginReceive(ReadLines, null);
                }

                // First Line
                else
                {
                    firstLine = line;
                    ss.BeginReceive(ReadLines, null);
                }

            }

            private void ProcessRequest(string line, object p = null)
            {
                Console.WriteLine(line);
                //if (!line.Contains("}"))
                //{
                //    ss.BeginReceive(ProcessRequest, null, contentLength);
                //}
                if (createUserPattern.IsMatch(firstLine))
                {
                    CreateUserRequest(line);
                }
                else if (joinGamePattern.IsMatch(firstLine))
                {
                    JoinGameRequest(line);
                }
                else if (gameStatusPattern.IsMatch(firstLine))
                {
                    
                }
                else if (cancelRequestPattern.IsMatch(firstLine))
                {
                    CancelRequest(line);
                }
                else if (playWordPattern.IsMatch(firstLine))
                {
                    var v = playWordPattern.Match(firstLine);
                    gameID = v.Groups[1].ToString();

                    PlayWordRequest(line);
                }
            }

            private void CreateUserRequest(string line)
            {
                // Check
                Name n = JsonConvert.DeserializeObject<Name>(line);
                User u = new User(n);
                User user = new BoggleService().CreateUser(u, out HttpStatusCode status);
                string result = CreateResult(user, status);
                ss.BeginSend(result, (x, y) => { ss.Shutdown(System.Net.Sockets.SocketShutdown.Both); }, null);
            }

            private void JoinGameRequest(string line)
            {
                SetGame sg = JsonConvert.DeserializeObject<SetGame>(line);
                SetGame setGame = new BoggleService().JoinGame(sg, out HttpStatusCode status);
                string result = CreateResult(setGame, status);
                ss.BeginSend(result, (x, y) => { ss.Shutdown(System.Net.Sockets.SocketShutdown.Both); }, null);
            }

            // Serializes the object to send and creates result string
            private string CreateResult(object o, HttpStatusCode status)
            {
                string result = "HTTP/1.1 " + (int)status + " " + status + "\r\n";

                // Success Code
                if ((int)status / 100 == 2)
                {
                    string res = JsonConvert.SerializeObject(o);
                    result += "Content-Length: " + Encoding.UTF8.GetByteCount(res) + "\r\n\r\n";
                    result += res;
                }
                else
                {
                    result += "\r\n";
                }
                return result;
            }

            private void GetStatusRequest(string line)
            {

            }

            private void CancelRequest(string line)
            {
                CancelRequestDetails cRD = JsonConvert.DeserializeObject<CancelRequestDetails>(line);
                new BoggleService().CancelJoinRequest(cRD, out HttpStatusCode status);

                String result = CreateResult(null, status);
                ss.BeginSend(result, (x, y) => { ss.Shutdown(System.Net.Sockets.SocketShutdown.Both); }, null);
            }

            private void PlayWordRequest(string line)
            {
                PlayWordDetails playWordDetails = JsonConvert.DeserializeObject<PlayWordDetails>(line);
                PlayWordDetails wordDetail = new BoggleService().PlayWord(gameID, playWordDetails, out HttpStatusCode status);
                string result = CreateResult(wordDetail, status);
                ss.BeginSend(result, (x, y) => { ss.Shutdown(System.Net.Sockets.SocketShutdown.Both); }, null);
            }
        }
    }
}
