using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SpriteStrife
{
    struct TargetPattern
    {
        public bool[,] Pattern;
        public Point Center;

        public TargetPattern(bool[,] pattern, Point center)
        {
            Pattern = pattern;
            Center = center;
        }

        public TargetPattern() : this(new bool[1, 1], new Point(0, 0)) 
        {
            Pattern[0, 0] = true;
        }
    }
}
