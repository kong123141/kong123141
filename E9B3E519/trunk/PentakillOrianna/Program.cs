/*
 * Too lazy to comment, will do some other lifetime
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using PentakillOrianna.Controller;
using PentakillOrianna.Util;
using SharpDX;

namespace PentakillOrianna {
    class Program {

        public static MenuController menuController;
        public static Orbwalking.Orbwalker orbwalker;
        public static Obj_AI_Hero player;
        public static Spell q;
        public static Spell w;
        public static Spell e;
        public static Spell r;
        public static SpellSlot ignite;
        public static Ball ball;
        public static bool onAlly = true;

        static void OnGameLoad(EventArgs args) {

            //Assigning objects used in later parts
            menuController = new MenuController();
            orbwalker = new Orbwalking.Orbwalker(menuController.getOrbwalkingMenu());
            player = ObjectManager.Player;
            ball = new Ball(player.Position);

            //Initiating spells
            q = new Spell(SpellSlot.Q, 825);
            q.SetSkillshot(0.25f, 80, 1300, false, SkillshotType.SkillshotLine);
            w = new Spell(SpellSlot.W, 250);
            w.SetSkillshot(0f, 250, float.MaxValue, false, SkillshotType.SkillshotCircle);
            e = new Spell(SpellSlot.E, 1095);
            e.SetSkillshot(0.25f, 145, 1700, false, SkillshotType.SkillshotLine);
            r = new Spell(SpellSlot.R, 370);
            r.SetSkillshot(0.60f, 370, float.MaxValue, false, SkillshotType.SkillshotCircle);
            ignite = player.GetSpellSlot("summonerdot");

            //Add menu to main menu
            menuController.getMenu().AddToMainMenu();
            //Check if our Champion is Orianna
            if (player.ChampionName != "Orianna") {
                return;
            }


            Game.OnUpdate += OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast += OnSpellCast;
            Drawing.OnDraw += OnDraw;
            Game.PrintChat("<font color ='#33FFFF'>Pentakill Orianna</font> by <font color = '#FFFF00'>GoldenGates</font> loaded, enjoy!");

        }

        static void OnGameUpdate(EventArgs args) {
            UpdateBallPosition();
            GameLogic.AutoLevelUp();
            if (player.IsDead)
                return;
            switch (orbwalker.ActiveMode) {
                case Orbwalking.OrbwalkingMode.Combo:
                    GameLogic.performCombo();
                    GameLogic.performEShield();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (menuController.getMenu().Item("harassManager").GetValue<Slider>().Value < player.ManaPercent) {
                        GameLogic.performHarass();
                    }
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    if (menuController.getMenu().Item("lastHitManager").GetValue<Slider>().Value < player.ManaPercent) {
                    }
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    if (menuController.getMenu().Item("clearManager").GetValue<Slider>().Value < player.ManaPercent) {
                        GameLogic.performLaneClear();
                    }
                    break;
            }
        }

        static void OnSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args) {
            if (unit.IsMe && player.GetSpellSlot(args.SData.Name) == SpellSlot.Q) {
                ball.setPosition(args.End);
                onAlly = false;
            }
        }

        static void UpdateBallPosition() {
            foreach (Obj_AI_Hero unit in ObjectManager.Get<Obj_AI_Hero>()) {
                if (unit.HasBuff("orianaghost", true) || unit.HasBuff("OrianaGhostSelf", true)) {
                    if (onAlly) {
                        onAlly = true;
                        ball.setPosition(unit.Position);
                    }
                }
            }
        }

        #region Drawing
        static readonly Render.Text text = new Render.Text(
                                               0, 0, "", 11, new ColorBGRA(255, 0, 0, 255), "monospace");
        static void OnDraw(EventArgs args) {
            if (menuController.getMenu().Item("drawQ").GetValue<bool>())
                Render.Circle.DrawCircle(player.Position, q.Range, System.Drawing.Color.Yellow);
            if (menuController.getMenu().Item("drawW").GetValue<bool>())
                Render.Circle.DrawCircle(ball.getPosition(), e.Range, System.Drawing.Color.Green);
            if (menuController.getMenu().Item("drawR").GetValue<bool>())
                Render.Circle.DrawCircle(player.Position, r.Range, System.Drawing.Color.IndianRed);
            if (menuController.getMenu().Item("drawBall").GetValue<bool>())
                Render.Circle.DrawCircle(ball.getPosition(), 100, System.Drawing.Color.White);
            if (menuController.getMenu().Item("drawDmg").GetValue<bool>())
                DrawHPBarDamage();
        }
        static void DrawHPBarDamage() {
            const int XOffset = 10;
            const int YOffset = 20;
            const int Width = 103;
            const int Height = 8;
            foreach (var unit in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValid && h.IsHPBarRendered && h.IsEnemy)) {
                var barPos = unit.HPBarPosition;
                float damage = SpellDamage.getComboDamage(unit);
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

        static void Main(string[] args) {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }
    }
}
