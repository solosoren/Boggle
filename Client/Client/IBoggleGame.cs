using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PS8
{
    interface IBoggleGame
    {

        event Action<string, Button> EnterPressed;
        event Action GameClosed;
        event Action HelpPressed;
        void UpdateBoard(Game game);

        void EndGame(List<string> player1Words, List<string> player2Words);

    }
}
