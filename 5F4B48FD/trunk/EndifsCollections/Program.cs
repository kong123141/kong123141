using System;
using EndifsCollections.Controller;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace EndifsCollections
{
    class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }
        static void OnLoad(EventArgs args)
        {
            try
            {                
                var type = Type.GetType("EndifsCollections.Plugins." + ObjectManager.Player.ChampionName);
                if (type != null)
                {
                    myUtility.Notify("Endif's Collections - " + ObjectManager.Player.ChampionName, Color.White, 4000);
                    new PluginLoader();
                    return;
                }
                myUtility.Notify(ObjectManager.Player.ChampionName + " is not supported.", Color.White, 4000);
                InitializeTools();
            }
            catch
            {
            }
        }
        static void InitializeTools()
        {
            new myOrbwalkerMenu();
        }
    }
}
