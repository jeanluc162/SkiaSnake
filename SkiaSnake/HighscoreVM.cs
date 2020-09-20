using System;
using System.Collections.Generic;
using System.Text;

namespace SkiaSnake
{
    class HighscoreVM
    {
        public Dictionary<String,Int32> Highscores
        {
            get
            {
                return HighscoreHandler.Highscores;
            }
        }
    }
}
