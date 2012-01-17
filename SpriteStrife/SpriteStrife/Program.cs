using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SpriteStrife
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (GameLogic game = new GameLogic())
            {
                game.Run();
            }
        }
    }
#endif
}

