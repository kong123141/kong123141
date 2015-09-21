using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace TheDamage
{
    class TheDamage
    {
        private const int Width = 103, Height = 9;
        private static readonly Vector2 Offset = new Vector2(34f, 8.5f);
        private static Menu _menu, _enemiesMenu;
        private static readonly Dictionary<SpellSlot, Render.Text> Text = new Dictionary<SpellSlot, Render.Text>();
        private static MenuItem _permashow;
        private static readonly SpellSlot[] SupportedSlots = { SpellSlot.R, SpellSlot.E, SpellSlot.W, SpellSlot.Q };
        private static Dictionary<string, SpellSlot[]> _blackList;
        private static Dictionary<SpellSlot, Color> _spellColors;

        static void Main(string[] args)
        {
            _blackList = new Dictionary<string, SpellSlot[]>
            {
                {"Nunu", new[]{ SpellSlot.Q}},
            };

            _spellColors = new Dictionary<SpellSlot, Color>
            {
                {SpellSlot.Q, Color.Green},
                {SpellSlot.W, Color.Blue},
                {SpellSlot.E, Color.Purple},
                {SpellSlot.R, Color.Red}
            };

            CustomEvents.Game.OnGameLoad += _ =>
            {
                _menu = new Menu("The Damage", "Thedamage", true);
                _enemiesMenu = new Menu("Enemies", _menu.Name + ".Enemies");

                foreach (var enemy in HeroManager.Enemies)
                {
                    var enemyMenu = new Menu(enemy.ChampionName, _enemiesMenu.Name + "." + enemy.ChampionName);
                    foreach (SpellSlot slot in SupportedSlots)
                    {
                        enemyMenu.AddItem(new MenuItem(enemyMenu.Name + "." + slot.ToString(), slot.ToString()).SetValue(!(_blackList.ContainsKey(enemy.ChampionName) && _blackList[enemy.ChampionName].Contains(slot))));
                    }
                    _enemiesMenu.AddSubMenu(enemyMenu);
                }
                _menu.AddSubMenu(_enemiesMenu);

                foreach (SpellSlot slot in SupportedSlots)
                {
                    _menu.AddItem(new MenuItem(_menu.Name + "." + slot.ToString() + "Drawing", slot.ToString() + " Drawing").SetValue(Color.FromArgb(150, _spellColors[slot])));
                    Text[slot] = new Render.Text(string.Empty, Vector2.Zero, 16, new ColorBGRA(0)) { Visible = false };
                    Text[slot].Add();
                }
                Text[SpellSlot.Unknown] = new Render.Text(string.Empty, Vector2.Zero, 16, new ColorBGRA(0)) { Visible = false };
                Text[SpellSlot.Unknown].Add();

                var miscMenu = new Menu("Misc", _menu.Name + ".Misc");
                miscMenu.AddItem(new MenuItem(_menu.Name + ".dontdrawoncd", "Don't draw when on cooldown").SetValue(true));
                miscMenu.AddItem(new MenuItem(_menu.Name + ".DrawAsOneOnClutter", "Draw only one bar when small").SetValue(true));
                miscMenu.AddItem(new MenuItem(_menu.Name + ".GeneralColor", "General Color").SetValue(Color.FromArgb(150, Color.OrangeRed)));
                var hidePermeshow = miscMenu.AddItem(new MenuItem(_menu.Name + ".showPermashow", "Hide Permashow").SetValue(false));

                hidePermeshow.ValueChanged += (sender, sargs) => _permashow.Permashow(!sargs.GetNewValue<bool>());


                var upvoted = miscMenu.AddItem(new MenuItem(_menu.Name + ".upvotedasmdb", "Upvoted on assemblydb").SetValue(false));
                _menu.AddSubMenu(miscMenu);

                _menu.AddItem(new MenuItem(_menu.Name + ".Enabled", "Enabled").SetValue(true));
                _menu.AddToMainMenu();

                _permashow = new MenuItem(_menu.Name + ".Target", "The Damage").SetValue(new StringList(new[] { "None" }));
                _permashow.Permashow(!hidePermeshow.GetValue<bool>());


                upvoted.ValueChanged += (sender, changedArgs) =>
                {
                    if (changedArgs.GetNewValue<bool>())
                        Notifications.AddNotification("Thank you! :)", 3);
                };
                if (upvoted.GetValue<bool>())
                    miscMenu.Items.Remove(upvoted);

                Drawing.OnDraw += Draw;

            };
        }

        private static void Draw(EventArgs args)
        {
            if (!_menu.Item(_menu.Name + ".Enabled").GetValue<bool>())
            {
                DisableText();
                return;
            }

            var target = TargetSelector.GetSelectedTarget();
            if (!target.IsValidTarget())
                target = HeroManager.Enemies.Where(enemy => enemy.IsValidTarget()).MinOrDefault(hero => hero.Distance(ObjectManager.Player, true));
            if (target == null || !ObjectManager.Player.IsHPBarRendered)
            {
                DisableText();
                _permashow.GetValue<StringList>().SList[0] = "None";
                return;
            }
            _permashow.GetValue<StringList>().SList[0] = target.ChampionName;

            var playerHealthPercent = ObjectManager.Player.HealthPercent / 100d;
            var prevPlayerHealthPercent = playerHealthPercent;
            var hasDrawn = false;

            if (GetRemainingIgniteDamage(ObjectManager.Player) > ObjectManager.Player.Health + ObjectManager.Player.HPRegenRate * 2.5f)
            {
                var fullHealthPercent = 1d;
                var drawLines = false;
                foreach (var text in Text)
                    if (text.Key != SpellSlot.Unknown)
                        text.Value.Visible = false;
                DrawDamageOnHealthbar(ObjectManager.Player, ObjectManager.Player.MaxHealth, ref fullHealthPercent, 1, ref drawLines, Color.FromArgb(150, Color.Red), "IGNITE KILLS", true, SpellSlot.Unknown, false);
                return;
            }

            if (_menu.Item(_menu.Name + ".DrawAsOneOnClutter").GetValue<bool>())
            {
                var sumdmg = SupportedSlots.Select(slot => target.GetSpellDamage(ObjectManager.Player, slot)).Sum();
                if (sumdmg < ObjectManager.Player.MaxHealth / 5)
                {
                    var spellColor = _menu.Item(_menu.Name + ".GeneralColor").GetValue<Color>();
                    DrawDamageOnHealthbar(ObjectManager.Player, sumdmg, ref playerHealthPercent, prevPlayerHealthPercent, ref hasDrawn, spellColor);
                    DisableText();
                    return;
                }

            }

            foreach (var spellSlot in SupportedSlots)
            {
                if (target.GetSpell(spellSlot).Level == 0 || (target.GetSpell(spellSlot).CooldownExpires > Game.Time && _menu.Item(_menu.Name + ".dontdrawoncd").GetValue<bool>()) || !_enemiesMenu.Item(_enemiesMenu.Name + "." + target.ChampionName + "." + spellSlot).GetValue<bool>())
                {
                    Text[spellSlot].Visible = false;
                    continue;
                }

                var spellColor = _menu.Item(_menu.Name + "." + spellSlot + "Drawing").GetValue<Color>();
                var damage = target.GetSpellDamage(ObjectManager.Player, spellSlot);

                DrawDamageOnHealthbar(ObjectManager.Player, damage, ref playerHealthPercent, prevPlayerHealthPercent, ref hasDrawn, spellColor, true, spellSlot);

                prevPlayerHealthPercent = playerHealthPercent;
            }
        }

        private static void DisableText()
        {
            foreach (var text in Text)
            {
                text.Value.Visible = false;
            }
        }

        private static void DrawDamageOnHealthbar(Obj_AI_Hero healthbar, double damage, ref double playerHealthPercent, double prevPlayerHealthPercent, ref bool shouldDrawLine, Color spellColor, bool drawtext = false, SpellSlot slot = SpellSlot.Unknown)
        {
            DrawDamageOnHealthbar(healthbar, damage, ref playerHealthPercent, prevPlayerHealthPercent, ref shouldDrawLine, spellColor, slot.ToString(), drawtext, slot);
        }


        private static void DrawDamageOnHealthbar(Obj_AI_Hero healthbar, double damage, ref double playerHealthPercent, double prevPlayerHealthPercent, ref bool shouldDrawLine, Color spellColor, string text, bool drawtext = false, SpellSlot slot = SpellSlot.Unknown, bool colorDown = true)
        {
            var position = healthbar.HPBarPosition + Offset;
            playerHealthPercent = Math.Max(playerHealthPercent - (damage) / ObjectManager.Player.MaxHealth, 0);
            var start = new Vector2((float)(position.X + playerHealthPercent * Width), position.Y);
            var end = new Vector2((float)((prevPlayerHealthPercent) * Width) + position.X, position.Y);

            if (end.X - start.X > 1)
            {
                Drawing.DrawLine(start, end, Height, spellColor);
                if (shouldDrawLine)
                {
                    Drawing.DrawLine(end, new Vector2(end.X, end.Y + Height), 2, Color.Black);
                }
                shouldDrawLine = true;
            }

            if (drawtext)
            {
                Text[slot].text = text;
                Text[slot].X = (int)(start.X + (end.X - start.X) / 2f - Text[slot].Width / 2f);
                Text[slot].Y = (int)(start.Y - 3.25f);
                if (colorDown)
                    Text[slot].Color = new SharpDX.Color(spellColor.R - 75, spellColor.G - 75, spellColor.B - 75);
                else
                    Text[slot].Color = new SharpDX.Color(spellColor.R + 75, spellColor.G + 75, spellColor.B + 75);
                Text[slot].Visible = (end.X - start.X >= Text[slot].Width);
            }

        }

        private static float GetIgniteDamage(Obj_AI_Hero souce)
        {
            return 50 + souce.Level * 20;
        }

        private static float GetRemainingIgniteDamage(Obj_AI_Base target)
        {
            var ignitebuff = target.GetBuff("summonerdot");
            if (ignitebuff == null) return 0;
            return (float)ObjectManager.Player.CalcDamage(target, Damage.DamageType.True, ((int)(ignitebuff.EndTime - Game.Time) + 1) * GetIgniteDamage(ignitebuff.Caster as Obj_AI_Hero) / 5);
        }

    }
}
