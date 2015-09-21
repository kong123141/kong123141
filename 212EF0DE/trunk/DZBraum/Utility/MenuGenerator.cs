using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;

namespace DZBraum.Utility
{
    class MenuGenerator
    {
        public static void OnLoad(Menu RootMenu)
        {
            var OrbwalkerMenu = new Menu("Orbwalker", "dz191.braum.orbwalker");
            Braum.Orbwalker = new Orbwalking.Orbwalker(OrbwalkerMenu);
            RootMenu.AddSubMenu(OrbwalkerMenu);

            var TSMenu = new Menu("TargetSelector", "dz191.braum.ts");

            TargetSelector.AddToMenu(TSMenu);

            RootMenu.AddSubMenu(TSMenu);

            var comboMenu = new Menu("Combo","dz191.braum.combo");
            {
                comboMenu.AddItem(new MenuItem("dz191.braum.combo.useQ", "Use Q").SetValue(true));
                comboMenu.AddItem(new MenuItem("dz191.braum.combo.useW", "Use W").SetValue(true));
                comboMenu.AddItem(new MenuItem("dz191.braum.combo.useE", "Use E").SetValue(true));
                comboMenu.AddItem(new MenuItem("dz191.braum.combo.useR", "Use R").SetValue(true));
                comboMenu.AddItem(new MenuItem("dz191.braum.combo.minR", "Min. Enemies for R").SetValue(new Slider(2,1,5)));
            }
            RootMenu.AddSubMenu(comboMenu);

            RootMenu.AddToMainMenu();
        }
    }
}
