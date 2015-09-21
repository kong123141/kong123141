using System;

using LeagueSharp;

namespace DisableCastIndicator
{
    class Program
    {
        static void Main(string[] args)
        {
            Game.OnStart += Game_OnStart;
        }

        static void Game_OnStart(EventArgs args)
        {
            Hacks.DisableCastIndicator = true;
        }
    }
}
