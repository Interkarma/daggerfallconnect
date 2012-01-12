using System;

namespace RoHD_Playground
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Game.Main game = new Game.Main())
            {
                game.Run();
            }
        }
    }
#endif
}

