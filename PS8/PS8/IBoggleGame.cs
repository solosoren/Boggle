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
        void UpdateBoard(Game game);

        void EndGame();

    }
}
