using LeagueSharp;
using LeagueSharp.Common;

namespace Pentakill_LeBlanc {
    class MenuController {

        private Menu menu;

        public MenuController() {
            menu = new Menu("Pentakill Leblanc", "menu", true);
            combo();
            harass();
            laneClear();
            drawing();
            misc();
        }

        public Menu getMenu() {
            return menu;
        }

        public Menu attachOrbwalker() {
            return menu.AddSubMenu(new Menu("Orbwalker", "gates.menu.orbwalker"));
        }

        public Menu attachTargetSelector() {
            return menu.AddSubMenu(new Menu("Target Selector", "gates.menu.targetSelector"));
        }

        #region SubMenu Methods
        private void combo() {
            Menu combo = menu.AddSubMenu(new Menu("Combo", "gates.menu.combo"));
            combo.AddItem(new MenuItem("gates.menu.combo.useQ", "Use Q")).SetValue<bool>(true);
            combo.AddItem(new MenuItem("gates.menu.combo.useW", "Use W")).SetValue<bool>(true);
            combo.AddItem(new MenuItem("gates.menu.combo.wBack", "W Back (Off for manual)")).SetValue<bool>(false);
            combo.AddItem(new MenuItem("gates.menu.combo.useE", "Use E")).SetValue<bool>(true);
            combo.AddItem(new MenuItem("gates.menu.combo.useR", "Use R")).SetValue<bool>(true);
            combo.AddItem(new MenuItem("gates.menu.combo.useIgnite", "Use Ignite")).SetValue<bool>(true);
            combo.AddItem(new MenuItem("gates.menu.combo.2chainz", "2 Chainz Ft. Gates (Soon™)")).SetValue(new KeyBind(86, KeyBindType.Press));
        }

        private void harass() {
            Menu harass = menu.AddSubMenu(new Menu("Harass", "gates.menu.harass"));
            harass.AddItem(new MenuItem("gates.menu.harass.useQ", "Use Q")).SetValue<bool>(true);
            harass.AddItem(new MenuItem("gates.menu.harass.useW", "Use W")).SetValue<bool>(true);
            harass.AddItem(new MenuItem("gates.menu.harass.wBack", "W Back (Off for manual)")).SetValue<bool>(true);
            harass.AddItem(new MenuItem("gates.menu.harass.useE", "Use E")).SetValue<bool>(false);
            harass.AddItem(new MenuItem("gates.menu.harass.mana", "Mana Manager")).SetValue(new Slider(50, 0, 100));
        }

        private void laneClear() {
            Menu laneClear = menu.AddSubMenu(new Menu("Lane Clear", "gates.menu.laneClear"));
            laneClear.AddItem(new MenuItem("gates.menu.laneClear.useQ", "Use Q")).SetValue<bool>(false);
            laneClear.AddItem(new MenuItem("gates.menu.laneClear.useW", "Use W")).SetValue<bool>(true);
            laneClear.AddItem(new MenuItem("gates.menu.laneClear.useR", "Use R (W)")).SetValue<bool>(false);
            laneClear.AddItem(new MenuItem("gates.menu.laneClear.mana", "Mana Manager")).SetValue(new Slider(25, 0, 100));
        }

        private void drawing() {
            Menu drawing = menu.AddSubMenu(new Menu("Drawing", "gates.menu.drawing"));
            drawing.AddItem(new MenuItem("gates.menu.drawing.drawQ", "Draw Q")).SetValue<bool>(true);
            drawing.AddItem(new MenuItem("gates.menu.drawing.drawW", "Draw W")).SetValue<bool>(true);
            drawing.AddItem(new MenuItem("gates.menu.drawing.drawE", "Draw E")).SetValue<bool>(true);
            drawing.AddItem(new MenuItem("gates.menu.drawing.drawDamage", "Draw Damage")).SetValue<bool>(true);
            drawing.AddItem(new MenuItem("gates.menu.drawing.drawStatus", "Draw Status")).SetValue<bool>(true);
        }
        private void misc() {
            menu.AddItem(new MenuItem("gates.menu.autoBuySweeper", "Switch to Red Sweeper at 6")).SetValue<bool>(true);
            menu.AddItem(new MenuItem("gates.menu.wEnemies", "Don't W if X Enemies Around")).SetValue(new Slider(3, 1, 5));
            menu.AddItem(new MenuItem("gates.menu.wDelay", "Minimum W Back Delay (ms)")).SetValue(new Slider(250, 0, 1000));
            menu.AddItem(new MenuItem("gates.menu.autoLevel", "Auto Level")).SetValue(new StringList(new[] { "Off", "R>Q>W>E", "R>Q>E>W", "R>W>Q>E", "R>W>E>Q", "R>E>Q>W", "R>E>W>Q" }));
        }
        #endregion
    }
}