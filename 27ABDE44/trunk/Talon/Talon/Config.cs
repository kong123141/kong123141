using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using Talon.Helper;
using Color = System.Drawing.Color;

namespace Talon
{
    internal class Config
    {
        public static readonly Color NotificationColor = Color.FromArgb(136, 207, 240);
        public static Menu Talon;
        public static Orbwalking.Orbwalker Orbwalker;
        public class Configs
        {
            public static Menu TargetSelector;
            public static Menu Orbwalker;
            public static Menu Combo;
            public static Menu Harass;
            public static Menu LaneClear;
            public static Menu KillSteal;
            public static Menu Drawing;
            public static Menu Misc;
        }

        public static void Init()
        {
            Talon = new Menu("Apollo's " + ObjectManager.Player.ChampionName, "apollo.talon", true);

            //Orbwalker
            Configs.Orbwalker = new Menu("Orbwalker", "apollo.talon.orbwalker");
            Talon.AddSubMenu(Configs.Orbwalker);
            Orbwalker = new Orbwalking.Orbwalker(Configs.Orbwalker);

            //Targetselector
            Configs.TargetSelector = new Menu("Target Selector", "apollo.talon.targettelector");
            TargetSelector.AddToMenu(Configs.TargetSelector);
            Talon.AddSubMenu(Configs.TargetSelector);

            //Drawings
            Configs.Drawing = new Menu("Drawings", "apollo.talon.draw");

            //Combo
            Configs.Combo = new Menu("Combo", "apollo.talon.combo");
            {
                var q = new Menu("Q", "apollo.talon.combo.q");
                q.AddBool("combo.q.bool", "Use in Combo");
                Configs.Combo.AddSubMenu(q);

                var w = new Menu("W", "apollo.talon.combo.w");
                w.AddBool("combo.w.bool", "Use in Combo");
                w.AddHitChance("combo.w.pre");
                Configs.Combo.AddSubMenu(w);

                var e = new Menu("E", "apollo.talon.combo.e");
                e.AddBool("combo.e.bool", "Use in Combo");
                Configs.Combo.AddSubMenu(e);

                var r = new Menu("R", "apollo.talon.combo.r");
                r.AddBool("combo.r.bool", "Use in Combo");
                r.AddSlider("combo.r.hit", "Minimum Hit", 3, 1, 5);
                Configs.Combo.AddSubMenu(r);

                Configs.Combo.AddBool("combo.ignite.bool", "Use Ignite");
            }

            //Harass
            Configs.Harass = new Menu("Harass", "apollo.talon.harass");
            {
                Configs.Harass.AddBool("harass.q.bool", "Use Q");

                Configs.Harass.AddBool("harass.w.bool", "Use W");
                Configs.Harass.AddHitChance("harass.w.pre", 3, "Minimum HitChance W");

                Configs.Harass.AddBool("harass.e.bool", "Use E", false);

                Configs.Harass.AddSlider("harass.mana", "Minimum Mana%", 30);
                Configs.Harass.AddKeyBind("harass.key", "Harass ToggleKey", "T", KeyBindType.Toggle);
            }

            //Laneclear
            Configs.LaneClear = new Menu("Laneclear", "apollo.talon.laneclear");
            {
                Configs.LaneClear.AddBool("laneclear.q.bool", "Use Q");
                Configs.LaneClear.AddBool("laneclear.w.bool", "Use W");
                Configs.LaneClear.AddSlider("laneclear.w.hit", "Minimum Hit W", 3, 1, 10);
                Configs.LaneClear.AddSlider("laneclear.mana", "Minimum Mana%", 30);
            }

            //Killsteal
            Configs.KillSteal = new Menu("Killsteal", "apollo.talon.ks");
            {
                Configs.KillSteal.AddBool("ks.w.bool", "Use W");
            }

            //Misc
            Configs.Misc = new Menu("Misc", "apollo.talon.misc");
            {
                Configs.Misc.AddItem(
                    new MenuItem("apollo.talon.misc.q.mode", "Mode").SetValue(
                        new StringList((new[] { "After AA", "Before AA" }), 1)));
            }

            Talon.AddSubMenu(Configs.Combo);
            Talon.AddSubMenu(Configs.Harass);
            Talon.AddSubMenu(Configs.LaneClear);
            Talon.AddSubMenu(Configs.KillSteal);
            MenuHelper.DrawSpell(
                new List<Spell>(
                    new[] { Spells.Spell[SpellSlot.W], Spells.Spell[SpellSlot.E], Spells.Spell[SpellSlot.R] }));
            DamageIndicator.Init(Damages.ComboDmg);
            Talon.AddSubMenu(Configs.Misc);
            //Packets
            Talon.AddBool("packets.bool", "Packet Cast (Doesnt Work)", false);

            Talon.AddToMainMenu();
        }

        public static Notification ShowNotification(string message, Color color, int duration = -1, bool dispose = true)
        {
            var notif = new Notification(message).SetTextColor(color);
            Notifications.AddNotification(notif);

            if (dispose)
            {
                Utility.DelayAction.Add(duration, () => notif.Dispose());
            }

            return notif;
        }
    }
}
