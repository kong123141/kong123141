using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace LeBlanc.Helper
{
    internal static class MenuHelper
    {
        public static bool IsReadyAndActive(this Spell spell, Mode mode)
        {
            try
            {
                return spell.IsReady() &&
                       Config.LeBlanc.Item("apollo.leblanc." + mode.ToString().ToLower() + "." + SpellName(spell) + ".bool").GetValue<bool>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return false;
        }

        public static HitChance GetHitChance(this Spell spell, Mode mode, string rmode = "notr")
        {
            try
            {
                if (spell.Slot == SpellSlot.R)
                {
                    switch (rmode)
                    {
                        case "W":
                            return
                                (HitChance)
                                    (Config.LeBlanc.Item(
                                        "apollo.leblanc." + mode.ToString().ToLower() + "." + SpellName(spell) +
                                        ".w.pre").GetValue<StringList>().SelectedIndex + 3);
                        case "E":
                            return
                                (HitChance)
                                    (Config.LeBlanc.Item(
                                        "apollo.leblanc." + mode.ToString().ToLower() + "." + SpellName(spell) +
                                        ".e.pre").GetValue<StringList>().SelectedIndex + 3);
                    }
                }
                else
                {
                    return
                        (HitChance)
                            (Config.LeBlanc.Item(
                                "apollo.leblanc." + mode.ToString().ToLower() + "." + SpellName(spell) + ".pre")
                                .GetValue<StringList>()
                                .SelectedIndex + 3);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return HitChance.High;
        }

        public static void AddHitChance(this Menu menu, string path, int minimumHitchance = 2)
        {
            try
            {
                menu.AddItem(
                    new MenuItem("apollo.leblanc." + path, "Minimum HitChance").SetValue(
                        new StringList((new[] { "Low", "Medium", "High", "Very High" }), minimumHitchance)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void AddBool(this Menu menu, string path, string message, bool toggle = true)
        {
            try
            {
                menu.AddItem(new MenuItem("apollo.leblanc." + path, message).SetValue(toggle));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static bool GetBool(this Menu menu, string path)
        {
            try
            {
                return menu.Item("apollo.leblanc." + path).GetValue<bool>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return false;
        }

        public static void AddSlider(this Menu menu, string path, string message, int value, int from = 0, int to = 100)
        {
            try
            {
                menu.AddItem(
                    new MenuItem("apollo.leblanc." + path, message).SetValue(new Slider(value, from, to)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static int GetSlider(this Menu menu, string path)
        {
            try
            {
                return menu.Item("apollo.leblanc." + path).GetValue<Slider>().Value;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return 0;
        }

        public static void AddKeyBind(this Menu menu, string path, string text, string key, KeyBindType type)
        {
            try
            {
                menu.AddItem(
                    new MenuItem("apollo.leblanc." + path, text).SetValue(new KeyBind(key.ToCharArray()[0], type)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static KeyBind GetKeyBind(this Menu menu, string path)
        {
            return menu.Item("apollo.leblanc." + path).GetValue<KeyBind>();
        }

        public static void AddInterrupter(List<string> list)
        {
            var menu = new Menu("Interrupter", "apollo.leblanc.interrupter");
            foreach (var spell in list)
            {
                var spellMenu = new Menu(spell, "apollo.leblanc.interrupter." + spell.ToLower());
                spellMenu.AddItem(
                    new MenuItem("apollo.leblanc.interrupter." + spell.ToLower() + ".bool", "Use as Interrupter")
                        .SetValue(true));
                spellMenu.AddItem(
                    new MenuItem("apollo.leblanc.interrupter." + spell.ToLower() + ".dangerlvl", "Minimum DangerLvl")
                        .SetValue(new StringList((new[] { "Low", "Medium", "High" }), 2)));
                menu.AddSubMenu(spellMenu);
            }
            Config.LeBlanc.AddSubMenu(menu);
        }

        public static Interrupter2.DangerLevel GetInterrupterDangerLevel(this Menu menu, Spell spell)
        {
            try
            {
                return
                    (Interrupter2.DangerLevel)
                        (menu.Item("apollo.leblanc.interrupter." + SpellName(spell) + ".dangerlvl")
                            .GetValue<StringList>()
                            .SelectedIndex + 2);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return Interrupter2.DangerLevel.High;
        }
        public static void DrawSpell(List<Spell> spells)
        {
            var drawConfig = Config.Configs.Drawing;

            if (spells.Any())
            {
                try
                {
                    drawConfig.AddItem(
                        new MenuItem("apollo.leblanc.draw.cd", "Draw on CD").SetValue(new Circle(false, Color.DarkRed)));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            foreach (var spell in spells)
            {
                try
                {
                    drawConfig.AddItem(
                        new MenuItem("apollo.leblanc.draw." + SpellName(spell), "Draw " + SpellName(spell).ToUpper() + " Range").SetValue(
                            new Circle(true, Color.AntiqueWhite)));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                Drawing.OnDraw += args =>
                {
                    if (args == null || ObjectManager.Player.IsDead)
                        return;

                    var drawCd = drawConfig.Item("apollo.leblanc.draw.cd").GetValue<Circle>();

                    var cd = !spell.IsReady() && drawCd.Active;
                    var draw = drawConfig.Item("apollo.leblanc.draw." + SpellName(spell)).GetValue<Circle>();
                    if (draw.Active && spell.Level > 0)
                        Render.Circle.DrawCircle(ObjectManager.Player.Position, spell.Range, cd ? drawCd.Color : draw.Color);
                };
            }

            Config.LeBlanc.AddSubMenu(Config.Configs.Drawing);
        }
        private static string SpellName(Spell spell)
        {
            switch (spell.Slot)
            {
                case SpellSlot.Q:
                    return "q";
                case SpellSlot.W:
                    return "w";
                case SpellSlot.E:
                    return "e";
                case SpellSlot.R:
                    return "r";
            }

            return "rip";
        }
        internal enum Mode
        {
            Combo,
            Harass,
            Lasthit,
            Laneclear,
            Farm,
            Ks,
            Interrupter,
            Antigapcloser,
            Misc,
            Flee
        }
    }
}