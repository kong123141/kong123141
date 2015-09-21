using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DZJayce.Utility;
using LeagueSharp;
using LeagueSharp.Common;

namespace DZJayce
{
    class JayceBootstrap
    {
        internal static void OnLoad()
        {
            if (ObjectManager.Player.ChampionName != "Jayce")
            {
                return;
            }

            Jayce.RootMenu = new Menu("DZJayce","dz191.jayce", true);
            MenuGenerator.OnLoad(Jayce.RootMenu);
            SpellHandler.OnLoad();
            Jayce.OnLoad();
        }
    }
}
