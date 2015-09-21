using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DZBraum.Utility;
using LeagueSharp;
using LeagueSharp.Common;

namespace DZBraum
{
    class BraumBootstrap
    {
        public static void OnLoad()
        {
            if (ObjectManager.Player.ChampionName != "Braum")
            {
                return;
            }
            Braum.BraumMenu = new Menu("Braum - Something","dz191.braum", true);
            MenuGenerator.OnLoad(Braum.BraumMenu);
            Braum.OnLoad();
        }
    }
}
