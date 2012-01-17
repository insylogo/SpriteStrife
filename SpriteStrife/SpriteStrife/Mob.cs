using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpriteStrife
{
    interface Mob
    {
        void Move(int newX, int newY, Map dMap);

    }
}
