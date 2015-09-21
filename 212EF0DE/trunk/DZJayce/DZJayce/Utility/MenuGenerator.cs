using LeagueSharp.Common;

namespace DZJayce.Utility
{
    class MenuGenerator
    {
        public static void OnLoad(Menu RootMenu)
        {
            var OrbwalkerMenu = new Menu("Orbwalker", "dz191.jayce.orbwalker");
            Jayce.Orbwalker = new Orbwalking.Orbwalker(OrbwalkerMenu);
            RootMenu.AddSubMenu(OrbwalkerMenu);

            var TSMenu = new Menu("TargetSelector", "dz191.jayce.ts");

            TargetSelector.AddToMenu(TSMenu);

            RootMenu.AddSubMenu(TSMenu);

            var ComboMenu = new Menu("Combo","dz191.jayce.combo");
            {
                ComboMenu.AddItem(new MenuItem("dz191.jayce.combo.qranged", "Use Q Ranged").SetValue(true));
                ComboMenu.AddItem(new MenuItem("dz191.jayce.combo.wranged", "Use W Ranged").SetValue(true));
                ComboMenu.AddItem(new MenuItem("dz191.jayce.combo.eranged", "Use E Ranged").SetValue(true));
                ComboMenu.AddItem(new MenuItem("dz191.jayce.combo.qmelee", "Use Q Melee").SetValue(true));
                ComboMenu.AddItem(new MenuItem("dz191.jayce.combo.wmelee", "Use W Melee").SetValue(true));
                ComboMenu.AddItem(new MenuItem("dz191.jayce.combo.emelee", "Use E Melee").SetValue(true));
                ComboMenu.AddItem(new MenuItem("dz191.jayce.combo.r", "Use R").SetValue(true));
                RootMenu.AddSubMenu(ComboMenu);
            }

            var MiscMenu = new Menu("MiscMenu", "dz191.jayce.misc");
            {
                MiscMenu.AddItem(new MenuItem("dz191.jayce.misc.gatemode", "Gate Mode").SetValue(new StringList(new []{"Horizontal", "Vertical"})));
                MiscMenu.AddItem(new MenuItem("dz191.jayce.misc.castqe", "Cast QE to best target").SetValue(new KeyBind('T', KeyBindType.Press)));
                MiscMenu.AddItem(new MenuItem("dz191.jayce.misc.castqemouse", "Cast QE to mouse").SetValue(new KeyBind('Z', KeyBindType.Press)));
                RootMenu.AddSubMenu(MiscMenu);
            }

            RootMenu.AddToMainMenu();
        }
    }
}
