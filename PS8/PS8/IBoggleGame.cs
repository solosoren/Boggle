﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS8
{
    interface IBoggleGame
    {

        event Action<string> EnterPressed;

        void UpdateBoard(Game game);


    }
}
