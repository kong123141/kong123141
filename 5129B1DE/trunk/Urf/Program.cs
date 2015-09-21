using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Urf
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += eventArgs =>
            {
                switch (ObjectManager.Player.ChampionName)
                {
                    case "Karma":
                        Karma();
                        break;
                }
            };
        }

        static void Karma()
        {
            Game.PrintChat("Karma supported");
            var shield = new Spell(SpellSlot.E, 400);
            Game.OnUpdate += eventArgs =>
            {
                if (shield.IsReady() && ObjectManager.Player.CountEnemiesInRange(1500) > 0)
                {
                    shield.CastOnUnit(ObjectManager.Player);
                }
            }; 
        }
    }
}
