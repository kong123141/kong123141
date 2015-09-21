using LeagueSharp;
using LeagueSharp.Common;

namespace CheerleaderLux
{
    class Loader : Addons.Lux
    {
        static void Main(string[] args)
        {          
            CustomEvents.Game.OnGameLoad += OnLoad;
        }
    }
}
