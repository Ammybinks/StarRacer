using System;

namespace StarRacer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (StarRacer game = new StarRacer())
            {
                game.Run();
            }
        }
    }
}

