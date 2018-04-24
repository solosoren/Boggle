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
            StringSocketListener server = new StringSocketListener(60000, Encoding.UTF8);
            server.Start();
            server.BeginAcceptStringSocket(ConnectionMade, server);
            Console.ReadLine();
        }

        private static void ConnectionMade(StringSocket ss, object payload)
        {
            StringSocketListener server = (StringSocketListener)payload;
            server.BeginAcceptStringSocket(ConnectionMade, server);
            new RequestHandler(ss);
        }

        private class RequestHandler
        {
            private StringSocket ss;

            private string firstLine;

            private int contentLength;

            // TODO: Regex for other service calls
            private static readonly Regex makeUserPattern = new Regex(@"^POST /BoggleService.svc/users HTTP");
            private static readonly Regex contentLengthPattern = new Regex(@"^content-length: (\d+)", RegexOptions.IgnoreCase);

            public RequestHandler(StringSocket ss)
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
                if (!line.Contains("}"))
                {
                    ss.BeginReceive(ProcessRequest, null, contentLength);
                }
                else if (makeUserPattern.IsMatch(firstLine))
                {
                    CreateUserRequest(line);
                }
            }

            private void CreateUserRequest(string line)
            {
                // Check
                Name n = JsonConvert.DeserializeObject<Name>(line);
                User u = new User(n);
                User user = new BoggleService().CreateUser(u, out HttpStatusCode status);
                string result = "HTTP/1.1" + (int)status + " " + status + "\r\n";

                // Success Code
                if ((int)status / 100 == 2)
                {
                    string res = JsonConvert.SerializeObject(user);
                    result += "Content-Length: " + Encoding.UTF8.GetByteCount(res) + "\r\n";
                    result += res;
                }
                ss.BeginSend(result, (x, y) => { ss.Shutdown(System.Net.Sockets.SocketShutdown.Both); }, null);
            }
        }
    }
}
