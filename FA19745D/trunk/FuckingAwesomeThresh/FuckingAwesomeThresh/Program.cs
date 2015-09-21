using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace FuckingAwesomeThresh
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameStart;
        }

        public static Orbwalking.Orbwalker Orbwalker;
        public static Menu Config;

        private static void Game_OnGameStart(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "Thresh")
            {
                Notifications.AddNotification(new Notification("not thresh sin huh? wanna go m9?", 2));
                return;
            }
            Config = new Menu("Fucking Awesome Thresh", "you-stealing-me-src-m29?", true);

            Orbwalker = new Orbwalking.Orbwalker(Config.AddSubMenu(new Menu("Orbwalker", "Orbwalker")));
            TargetSelector.AddToMenu(Config.AddSubMenu(new Menu("Target Selector", "Target Selector")));

            var combo = Config.AddSubMenu(new Menu("Combo", "Combo"));
            combo.AddItem(new MenuItem("CQ", "Use Q").SetValue(true));
            combo.AddItem(new MenuItem("CW", "Use W").SetValue(true));
            combo.AddItem(new MenuItem("CE", "Use E").SetValue(true));
            combo.AddItem(new MenuItem("CR", "Use R").SetValue(false));

            var allyMenu = combo.AddSubMenu(new Menu("W Priority (engage combo)", "WPrior"));
            allyMenu.AddItem(
                new MenuItem("kepe", "1 is lowest"));

            foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(a => a.IsAlly))
            {
                allyMenu.AddItem(
                    new MenuItem("priority" + ally.ChampionName, ally.ChampionName).SetValue(new Slider(5, 1, 5)));
            }

            var harass = Config.AddSubMenu(new Menu("Harass", "Harass"));
            harass.AddItem(new MenuItem("HQ", "Use Q").SetValue(true));
            harass.AddItem(new MenuItem("HE", "Use E").SetValue(false));

            var draw = Config.AddSubMenu(new Menu("Draw", "Draw"));
            draw.AddItem(new MenuItem("DQ", "Draw Q Range").SetValue(new Circle(false, Color.White)));
            draw.AddItem(new MenuItem("DW", "Draw W Range").SetValue(new Circle(false, Color.White)));
            draw.AddItem(new MenuItem("DE", "Draw E Range").SetValue(new Circle(false, Color.White)));
            draw.AddItem(new MenuItem("DR", "Draw R Range").SetValue(new Circle(false, Color.White)));
            draw.AddItem(new MenuItem("DES", "Draw Escape Spots").SetValue(true));

            var escape = Config.AddSubMenu(new Menu("Misc", "Misc"));
            escape.AddItem(new MenuItem("LanternAlly", "Throw Lantern At Ally").SetValue(true));
            escape.AddItem(new MenuItem("Escape", "Escape").SetValue(new KeyBind('Z', KeyBindType.Press)));

            var keys = Config.AddSubMenu(new Menu("Keys", "Kays"));
            keys.AddItem(new MenuItem("engageCombo", "Engage Combo").SetValue(new KeyBind('M', KeyBindType.Press)));
            keys.AddItem(new MenuItem("pull", "Pull").SetValue(new KeyBind('T', KeyBindType.Press)));
            keys.AddItem(new MenuItem("push", "Push").SetValue(new KeyBind('Y', KeyBindType.Press)));
            keys.AddItem(new MenuItem("flashPull", "Flash Pull").SetValue(new KeyBind('K', KeyBindType.Press)));
            keys.AddItem(new MenuItem("shieldAlly", "Shield Ally").SetValue(new KeyBind('L', KeyBindType.Press)));

            var info = Config.AddSubMenu(new Menu("Information", "info"));
            info.AddItem(new MenuItem("Msddsds", "if you would like to donate via paypal"));
            info.AddItem(new MenuItem("Msdsddsd", "you can do so by sending money to:"));
            info.AddItem(new MenuItem("Msdsadfdsd", "jayyeditsdude@gmail.com"));

            Config.AddItem(new MenuItem("Mgdgdfgsd", "Version: 0.0.1-2 BETA"));
            Config.AddItem(new MenuItem("Msd", "Made By FluxySenpai"));


            Config.AddToMainMenu();

            CheckHandler.Spells[SpellSlot.Q].SetSkillshot(0.5f, 70f, 1900, true, SkillshotType.SkillshotLine);

            Game.OnUpdate += Game_OnUpdate;
            CheckHandler.Init();

            Notifications.AddNotification(new Notification("Fucking Awesome Thresh", 2));

        }

        static void Game_OnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    ComboStateHandler.Combo();
                    break;
            }
            if (Config.Item("engageCombo").GetValue<KeyBind>().Active) ComboStateHandler.EngageCombo();
            if (Config.Item("pull").GetValue<KeyBind>().Active) ComboStateHandler.Pull();
            if (Config.Item("push").GetValue<KeyBind>().Active) ComboStateHandler.Push();
            if (Config.Item("flashPull").GetValue<KeyBind>().Active) ComboStateHandler.FlashPull();
        }
    }
}
