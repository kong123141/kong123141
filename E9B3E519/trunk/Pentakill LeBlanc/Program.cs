using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Pentakill_LeBlanc {
    class Program {

        #region Declaration
        public static Dictionary<SpellSlot, Spell> spells = new Dictionary<SpellSlot, Spell>();
        public static SpellSlot ignite;
        public static Obj_AI_Hero player;
        public static MenuController menuController;
        public static Orbwalking.Orbwalker orbwalker;
        public static string status = "Idle";
        #endregion

        #region OnGameLoad
        static void Game_OnGameLoad(EventArgs args) {
            player = ObjectManager.Player;

            spells.Add(SpellSlot.Q, new Spell(SpellSlot.Q, 700));
            spells[SpellSlot.Q].SetTargetted(.401f, 2000);
            spells.Add(SpellSlot.W, new Spell(SpellSlot.W, 600));
            spells[SpellSlot.W].SetSkillshot(.5f, 100, 2000, false, SkillshotType.SkillshotCircle);
            spells.Add(SpellSlot.E, new Spell(SpellSlot.E, 950));
            spells[SpellSlot.E].SetSkillshot(.366f, 70, 1600, true, SkillshotType.SkillshotLine);
            spells.Add(SpellSlot.R, new Spell(SpellSlot.R));
            ignite = player.GetSpellSlot("summonerdot");

            menuController = new MenuController();
            menuController.getMenu().AddToMainMenu();
            orbwalker = new Orbwalking.Orbwalker(menuController.attachOrbwalker());
            TargetSelector.AddToMenu(menuController.attachTargetSelector());

            if (player.ChampionName != "Leblanc")
                return;

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;

            Game.PrintChat("<font color ='#33FFFF'>Pentakill LeBlanc</font> by <font color = '#FFFF00'>GoldenGates</font> loaded, enjoy!");
        }
        #endregion

        #region OnUpdate
        private static void Game_OnUpdate(EventArgs args) {
            if (player.IsDead) {
                status = "Dead";
                return;
            }
            Utils.autoLevel();
            switch (orbwalker.ActiveMode) {
                case Orbwalking.OrbwalkingMode.Combo:
                    GameLogic.Combo.performCombo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    status = "Harass/Last Hit";
                    int harassManaManager = menuController.getMenu().Item("gates.menu.harass.mana").GetValue<Slider>().Value;
                    if (player.ManaPercent > harassManaManager)
                        GameLogic.Harass.performHarass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    status = "Lane Clear";
                    int laneClearManaManager = menuController.getMenu().Item("gates.menu.laneClear.mana").GetValue<Slider>().Value;
                    if (player.ManaPercent > laneClearManaManager)
                        GameLogic.LaneClear.performLaneClear();
                    break;
                case Orbwalking.OrbwalkingMode.None:
                    status = "Idle";
                    if (menuController.getMenu().Item("gates.menu.combo.2chainz").GetValue<KeyBind>().Active) {
                        status = "2 Chainz ft. Gates";
                        GameLogic.Combo.perform2Chainz();
                    }
                    break;
            }
        }
        #endregion

        #region Drawing
        private static void Drawing_OnDraw(EventArgs args) {
            if (menuController.getMenu().Item("gates.menu.drawing.drawQ").GetValue<bool>()) {
                Render.Circle.DrawCircle(player.Position, spells[SpellSlot.Q].Range, System.Drawing.Color.Yellow);
            }
            if (menuController.getMenu().Item("gates.menu.drawing.drawW").GetValue<bool>()) {
                Render.Circle.DrawCircle(player.Position, spells[SpellSlot.W].Range, System.Drawing.Color.Green);
            }
            if (menuController.getMenu().Item("gates.menu.drawing.drawE").GetValue<bool>()) {
                Render.Circle.DrawCircle(player.Position, spells[SpellSlot.E].Range, System.Drawing.Color.IndianRed);
            }
            if (menuController.getMenu().Item("gates.menu.drawing.drawStatus").GetValue<bool>()) {
                Drawing.DrawText(Drawing.WorldToScreen(player.Position)[0] + 50, Drawing.WorldToScreen(player.Position)[1] - 40, System.Drawing.Color.White, "Status: " + status);
            }
            if (menuController.getMenu().Item("gates.menu.drawing.drawDamage").GetValue<bool>()) {
                DrawHPBarDamage();
            }
        }
        static readonly Render.Text text = new Render.Text(
                                             0, 0, "", 11, new ColorBGRA(255, 0, 0, 255), "monospace");
        static void DrawHPBarDamage() {
            const int XOffset = 10;
            const int YOffset = 20;
            const int Width = 103;
            const int Height = 8;
            foreach (var unit in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValid && h.IsHPBarRendered && h.IsEnemy)) {
                var barPos = unit.HPBarPosition;
                float damage = Utils.getComboDamage(unit);
                float percentHealthAfterDamage = Math.Max(0, unit.Health - damage) / unit.MaxHealth;
                float yPos = barPos.Y + YOffset;
                float xPosDamage = barPos.X + XOffset + Width * percentHealthAfterDamage;
                float xPosCurrentHp = barPos.X + XOffset + Width * unit.Health / unit.MaxHealth;

                if (damage > unit.Health) {
                    text.X = (int)barPos.X + XOffset;
                    text.Y = (int)barPos.Y + YOffset - 13;
                    text.text = ((int)(unit.Health - damage)).ToString();
                    text.OnEndScene();
                }
                Drawing.DrawLine(xPosDamage, yPos, xPosDamage, yPos + Height, 2, System.Drawing.Color.Yellow);
            }
        }
        #endregion

        #region Main
        static void Main(string[] args) {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        #endregion
    }
}
