using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using SharpDX;


namespace MightyJayce
{
    class ConfigMenu : MainJayce
    {
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;
        public static readonly Obj_AI_Hero Player = ObjectManager.Player;
        public const string ChampName = "Jayce";
        public static Spell CannonQ, CannonW, CannonE, CannonQE;
        public static Spell HammerQ, HammerW, HammerE, R;

        public static void OnLoad(EventArgs args)
        {
            if (Player.ChampionName != ChampName)
                return;

            CannonQ = new Spell(SpellSlot.Q, 1050);
            CannonW = new Spell(SpellSlot.W);
            CannonE = new Spell(SpellSlot.E, 650);
            CannonQE = new Spell(SpellSlot.Q, 1600);

            HammerQ = new Spell(SpellSlot.Q, 600);
            HammerW = new Spell(SpellSlot.W, 285);
            HammerE = new Spell(SpellSlot.E, 240);

            R = new Spell(SpellSlot.R, 0);

            CannonQ.SetSkillshot(0.250f, 70f, 1500, true, SkillshotType.SkillshotCircle);
            CannonQE.SetSkillshot(0.300f, 140f, 3200, true, SkillshotType.SkillshotCircle);

            HammerQ.SetTargetted(HammerQ.Instance.SData.SpellCastTime, HammerQ.Instance.SData.MissileSpeed);
            HammerE.SetTargetted(HammerE.Instance.SData.SpellCastTime, HammerE.Instance.SData.MissileSpeed);

            Config = new Menu("MightyJayce", "MightyJayce", true);
            TargetSelector.AddToMenu(Config.AddSubMenu(new Menu("Target Selector", "Target Selector")));
            Orbwalker = new Orbwalking.Orbwalker(Config.AddSubMenu(new Menu("Orbwalker", "Orbwalker")));

            var combo = Config.AddSubMenu(new Menu("Combo Settings", "Combo Settings"));
            var harass = Config.AddSubMenu(new Menu("Harass Settings", "Harass Settings"));
            var killsteal = Config.AddSubMenu(new Menu("Killsteal Settings", "Killsteal Settings"));
            var laneclear = Config.AddSubMenu(new Menu("Laneclear Settings", "Laneclear Settings"));
            var lasthit = Config.AddSubMenu(new Menu("Lasthit Settings", "Lasthit Settings"));
            var jungleclear = Config.AddSubMenu(new Menu("Jungle Settings", "Jungle Settings"));
            var misc = Config.AddSubMenu(new Menu("Misc Settings", "Misc Settings"));
            var drawing = Config.AddSubMenu(new Menu("Draw Settings", "Draw Settings"));

            // Combo 
            var cannon = combo.AddSubMenu(new Menu("Cannon", "Ranged"));
            cannon.AddItem(new MenuItem("RangedQ", "Use Q").SetValue(true));
            cannon.AddItem(new MenuItem("RangedW", "Use W").SetValue(true));
            cannon.AddItem(new MenuItem("RangedE", "Use E").SetValue(true));

            var hammer = combo.AddSubMenu(new Menu("Hammer", "Hammer"));
            hammer.AddItem(new MenuItem("HammerQ", "Use Q").SetValue(true));
            hammer.AddItem(new MenuItem("HammerW", "Use W").SetValue(true));
            hammer.AddItem(new MenuItem("HammerE", "Use E").SetValue(true));

            combo.AddItem(new MenuItem("SwitchKill", "Only Switch Hammer if killable").SetValue(true));
            combo.AddItem(new MenuItem("Switch", "Auto Switch R").SetValue(true));

            // Killsteal
            var cannonks = killsteal.AddSubMenu(new Menu("Cannon", "Cannonks"));
            cannonks.AddItem(new MenuItem("RangedQKS", "Use Q").SetValue(true));
            cannonks.AddItem(new MenuItem("RangedWKS", "Use W").SetValue(true));
            cannonks.AddItem(new MenuItem("RangedEKS", "Use E").SetValue(true));

            var hammerks = killsteal.AddSubMenu(new Menu("Hammer", "Hammerks"));
            hammerks.AddItem(new MenuItem("HammerQKS", "Use Q").SetValue(true));
            hammerks.AddItem(new MenuItem("HammerWKS", "Use W").SetValue(true));
            hammerks.AddItem(new MenuItem("HammerEKS", "Use E").SetValue(true));

            killsteal.AddItem(new MenuItem("Switchks", "Auto Switch R").SetValue(true));

            // Laneclear
            var cannonclear = laneclear.AddSubMenu(new Menu("Cannon", "Cannonclear"));
            cannonclear.AddItem(new MenuItem("RangedClearQ", "Use Q").SetValue(true));
            cannonclear.AddItem(new MenuItem("RangedClearQHit", "If Minion Hit").SetValue(new Slider(2, 6, 0)));
            cannonclear.AddItem(new MenuItem("RangedClearW", "Use W").SetValue(true));
            cannonclear.AddItem(new MenuItem("RangedClearQE", "Use Gate for big minion wave").SetValue(true));

            var hammerclear = laneclear.AddSubMenu(new Menu("Hammer", "Hammerclear"));
            hammerclear.AddItem(new MenuItem("HammerClearQ", "Use Q").SetValue(true));
            hammerclear.AddItem(new MenuItem("HammerClearW", "Use W").SetValue(true));
            hammerclear.AddItem(new MenuItem("HammerClearE", "Use E").SetValue(true));

            laneclear.AddItem(new MenuItem("SwitchClear", "Auto Switch R").SetValue(true));

            // Lasthit
            lasthit.AddItem(new MenuItem("LasthitQ", "Lasthit Big Minion with Q").SetValue(false));          

            // Jungleclear
            var cannonjungle = jungleclear.AddSubMenu(new Menu("Cannon", "Cannonclear"));
            cannonjungle.AddItem(new MenuItem("CannonJungleQ", "Use Q").SetValue(true));
            cannonjungle.AddItem(new MenuItem("CannonJungleW", "Use W").SetValue(true));
            cannonjungle.AddItem(new MenuItem("CannonJungleE", "Use E").SetValue(true));

            var hammerjungle = jungleclear.AddSubMenu(new Menu("Hammer", "Hammerclear"));
            hammerjungle.AddItem(new MenuItem("HammerJungleQ", "Use Q").SetValue(true));
            hammerjungle.AddItem(new MenuItem("HammerJungleW", "Use W").SetValue(true));
            hammerjungle.AddItem(new MenuItem("HammerJungleE", "Use E").SetValue(true));

            jungleclear.AddItem(new MenuItem("SwitchJungle", "Auto Switch R").SetValue(true));

            Config.AddToMainMenu();
            Drawings.DrawEvent();
            Mechanics.EventLoader();         
            Game.OnUpdate += Game_OnGameUpdate;

        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.LaneClear:
                    if (!Mechanics.HammerMode)
                        Extensions.laneclear_Cannon();
                    if (Mechanics.HammerMode)
                        Extensions.laneclear_Hammer();
                    break;
            }
        }
    }
}