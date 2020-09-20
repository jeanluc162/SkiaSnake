using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SkiaSnake
{
    internal static class HighscoreHandler
    {
        private static String _FileName;
        public static Dictionary<String, Int32> Highscores
        {
            get
            {
                String[] split;
                Dictionary<String, Int32> highscores = new Dictionary<String, Int32>();
                foreach(String Line in File.ReadAllLines(_FileName))
                {
                    split = Line.Split(new char[] { '|' });
                    highscores.Add(split[0], Convert.ToInt32(split[1]));
                }
                return highscores;
            }
        }
        public static void AddHighscore(String Name, Int32 Highscore)
        {
            Dictionary<String, Int32> highscores = Highscores;
            highscores.Add(Name, Highscore);
            String[] Lines = new string[highscores.Count];
            for(int i = 0; i < highscores.Count; i++)
            {
                Lines[i] = highscores.Keys.ElementAt(i) + "|" + highscores.Values.ElementAt(i);
            }
            File.WriteAllLines(_FileName, Lines);
        }
        static HighscoreHandler()
        {
            _FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Highscores.txt");
            if (!File.Exists(_FileName)) File.Create(_FileName);
        }
    }
}
