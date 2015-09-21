using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace EndifsCreations
{
    class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }
        static void OnLoad(EventArgs args)
        {
            var type = Type.GetType("EndifsCreations.Plugins." + ObjectManager.Player.ChampionName);
            if (type != null)
            {
                new PluginLoader();
            }
        }
    }
}
