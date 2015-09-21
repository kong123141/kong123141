using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace PentakillOrianna.Controller {
    class MenuController {

        private Menu menu;

        public MenuController() {
            menu = new Menu("Pentakill Orianna", "menu", true);
            Combo();
            Harass();
            LaneClear();
            LastHit();
            Misc();
            Drawing();
        }

        public void Combo() {
            Menu combo = menu.AddSubMenu(new Menu("Combo", "combo"));
            combo.AddItem(new MenuItem("comboQ", "Use Q")).SetValue<bool>(true);
            combo.AddItem(new MenuItem("comboW", "Use W")).SetValue<bool>(true);
            combo.AddItem(new MenuItem("comboE", "Use E")).SetValue<bool>(true);
            combo.AddItem(new MenuItem("comboR", "Use R")).SetValue<bool>(true);
            combo.AddItem(new MenuItem("minEnemies", "Minimum Enemies for R").SetValue(new Slider(2, 1, 5)));
            combo.AddItem(new MenuItem("useIgnite", "Smart Ignite").SetValue(true));
        }

        public void Harass() {
            Menu harass = menu.AddSubMenu(new Menu("Harass", "harass"));
            harass.AddItem(new MenuItem("harassQ", "Use Q")).SetValue<bool>(true);
            harass.AddItem(new MenuItem("harassW", "Use W")).SetValue<bool>(true);
            harass.AddItem(new MenuItem("harassManager", "Mena Menager").SetValue(new Slider(60, 1, 100)));
        }

        public void LaneClear() {
            Menu laneClear = menu.AddSubMenu(new Menu("Lane Clear", "laneClear"));
            laneClear.AddItem(new MenuItem("clearQ", "Use Q")).SetValue<bool>(true);
            laneClear.AddItem(new MenuItem("clearW", "Use W")).SetValue<bool>(true);
            laneClear.AddItem(new MenuItem("clearManager", "Mena Menager").SetValue(new Slider(30, 1, 100)));
        }

        public void LastHit() {
            //TODO
        }

        public void Misc() {
            menu.AddItem(new MenuItem("autoLevel", "Auto Level Spells")).SetValue(
                        new StringList(new[] { "Off", "R>Q>W>E", "R>Q>E>W" }));
        }

        public void Drawing() {
            Menu drawingsMenu = menu.AddSubMenu(new Menu("Drawings", "drawings"));
            drawingsMenu.AddItem(new MenuItem("drawQ", "Draw Q")).SetValue(true);
            drawingsMenu.AddItem(new MenuItem("drawW", "Draw W")).SetValue(true);
            drawingsMenu.AddItem(new MenuItem("drawR", "Draw R")).SetValue(true);
            drawingsMenu.AddItem(new MenuItem("drawBall", "Draw Ball")).SetValue(true);
            drawingsMenu.AddItem(new MenuItem("drawDmg", "Draw Damage")).SetValue(true);
        }

        public Menu getOrbwalkingMenu() {
            return menu.AddSubMenu(new Menu("Orbwalker", "orbwalker"));
        }

        public Menu getMenu() {
            return menu;
        }
    }
}
