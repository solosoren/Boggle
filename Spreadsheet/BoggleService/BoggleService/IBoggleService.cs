using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Boggle
{
    [ServiceContract]
    public interface IBoggleService
    {
        /// <summary>
        /// Returns the Token of the new User
        /// </summary>
        [WebInvoke(Method = "POST", UriTemplate = "/users")]
        string CreateUser(User user);

        /// <summary>
        /// Returns the string ID of the new game
        /// </summary>
        [WebInvoke(Method = "POST", UriTemplate = "/games")]
        string JoinGame(SetGame setGame);

        /// <summary>
        /// Cancel join game
        /// </summary>
        [WebInvoke(Method = "PUT", UriTemplate = "/games")]
        void CancelJoinRequest(CancelRequestDetails cancelRequestDetails);

        /// <summary>
        /// Returns a score for the played word
        /// </summary>
        [WebInvoke(Method = "PUT", UriTemplate = "/games/{GameID}")]
        PlayWordDetails PlayWord(string GameID, PlayWordDetails PlayWordDetails);

        /// <summary>
        /// Returns the GameState
        /// </summary>
        [WebInvoke(Method = "GET", UriTemplate = "/games/{GameID}?brief={brief}")]
        Game GameStatus(string gameID, string brief);
    }
}
