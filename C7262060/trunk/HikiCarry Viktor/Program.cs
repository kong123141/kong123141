using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;
using LeagueSharp;
using HikiCarry_Viktor.SPrediction;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;
using Font = SharpDX.Direct3D9.Font;
using FontStyle = System.Drawing.FontStyle;

namespace HikiCarry_Viktor
{
    class Program
    {
        public static Menu Config;
        public static string cName = "Viktor";
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> SpellList = new List<Spell>();
        private static Obj_AI_Hero Player = ObjectManager.Player;

        public static string[] HitchanceNameArray = { "Low", "Medium", "High", "Very High", "Only Immobile" };
        public static HitChance[] HitchanceArray = { HitChance.Low, HitChance.Medium, HitChance.High, HitChance.VeryHigh, HitChance.Immobile };

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static int eRange = 525;
        public static SpellSlot Ignite;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.CharData.BaseSkinName != cName)
            {
                return;
            }
            Activator.Potion.hikiPotion = true;
            Activator.Zhonya.hikiZhonya = true;

            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 550 + eRange / 4f); 
            R = new Spell(SpellSlot.R, 700);

            Q.SetTargetted(0.25f, 2000);
            W.SetSkillshot(0.25f, 300, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.0f, 90, 1200, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.25f, 250, float.MaxValue, false, SkillshotType.SkillshotCircle);

            Ignite = Player.GetSpellSlot("summonerdot");

            Config = new Menu("HikiCarry - Viktor", "HikiCarry - Viktor", true).SetFontStyle(FontStyle.Bold, SharpDX.Color.Gold);
            TargetSelector.AddToMenu(Config.SubMenu("Target Selector Settings"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker Settings"));
            var comboMenu = new Menu("Combo Settings", "Combo Settings");
            {
                comboMenu.AddItem(new MenuItem("qCombo", "Use Q").SetValue(true));
                comboMenu.AddItem(new MenuItem("wCombo", "Use W").SetValue(true));
                comboMenu.AddItem(new MenuItem("eCombo", "Use E").SetValue(true));
                comboMenu.AddItem(new MenuItem("rCombo", "Use R").SetValue(true));
                comboMenu.AddItem(new MenuItem("minHitR", "Minimum Hit R").SetValue(new Slider(2, 1, 5)));
                Config.AddSubMenu(comboMenu);
            }
            var harassMenu = new Menu("Harass Settings", "Harass Settings");
            {
                harassMenu.AddItem(new MenuItem("qHarass", "Use Q").SetValue(true));
                harassMenu.AddItem(new MenuItem("eHarass", "Use E").SetValue(true));
                harassMenu.AddItem(new MenuItem("eToggle", "Use E [Toggle]").SetValue(true));
                harassMenu.AddItem(new MenuItem("tChance", "E Toggle Chance").SetValue<StringList>(new StringList(HitchanceNameArray, 3)));
                harassMenu.AddItem(new MenuItem("hMana", "Mana Manager").SetValue(new Slider(50, 0, 100)));
                Config.AddSubMenu(harassMenu);
            }

            var clearMenu = new Menu("Clear Settings", "Clear Settings");
            {
                clearMenu.AddItem(new MenuItem("eClear", "Use E").SetValue(true));
                clearMenu.AddItem(new MenuItem("eMinionCount", "E Minion Hit Count").SetValue(new Slider(3, 1, 5)));
                clearMenu.AddItem(new MenuItem("cMana", "Mana Manager").SetValue(new Slider(50, 0, 100)));
                Config.AddSubMenu(clearMenu);
            }

            var jungleMenu = new Menu("Jungle Settings", "Jungle Settings");
            {
                jungleMenu.AddItem(new MenuItem("qJungle", "Use Q").SetValue(true));
                jungleMenu.AddItem(new MenuItem("wJungle", "Use W").SetValue(true));
                jungleMenu.AddItem(new MenuItem("eJungle", "Use E").SetValue(true));
                jungleMenu.AddItem(new MenuItem("jMana", "Mana Manager").SetValue(new Slider(50, 0, 100)));
                Config.AddSubMenu(jungleMenu);
            }

            var lastMenu = new Menu("Lasthit Settings", "Lasthit Settings");
            {
                lastMenu.AddItem(new MenuItem("qLast", "Use Q [Siege Minions]").SetValue(true));
                lastMenu.AddItem(new MenuItem("lMana", "Mana Manager").SetValue(new Slider(50, 0, 100)));
                Config.AddSubMenu(lastMenu);
            }

            var itemMenu = new Menu("Item Settings", "Item Settings");
            {
                var zhonyaMenu = new Menu("Zhonya Settings", "Zhonya Settings");
                {
                    zhonyaMenu.AddItem(new MenuItem("useZhonya", "Use Zhonya").SetValue(true));
                    zhonyaMenu.AddItem(new MenuItem("zhonyaHP", "Use if my HP < %").SetValue(new Slider(20, 0, 100)));
                    itemMenu.AddSubMenu(zhonyaMenu);
                }
                var health = new Menu("Health Potion Settings", "Health Potion Settings");
                {
                    health.AddItem(new MenuItem("useHealth", "Use Health Potion").SetValue(true));
                    health.AddItem(new MenuItem("myhp", "Use if my HP < %").SetValue(new Slider(20, 0, 100)));
                    itemMenu.AddSubMenu(health);
                }
                var mana = new Menu("Mana Potion Settings", "Mana Potion Settings");
                {
                    mana.AddItem(new MenuItem("useMana", "Use Mana Potion").SetValue(true));
                    mana.AddItem(new MenuItem("mymana", "Use if my mana < %").SetValue(new Slider(20, 0, 100)));
                    itemMenu.AddSubMenu(mana);
                }
                Config.AddSubMenu(itemMenu);
            }

            var miscMenu = new Menu("Misc Settings", "Misc Settings");
            {
                miscMenu.AddItem(new MenuItem("aGapcloser", "AntiGapcloser[W]").SetValue(true));
                miscMenu.AddItem(new MenuItem("wInterrupter", "Interrupter[W]").SetValue(true));
                miscMenu.AddItem(new MenuItem("eKS", "Killsteal[E]").SetValue(true));
                miscMenu.AddItem(new MenuItem("wImmobile", "Auto W to Immobile Target").SetValue(true));
                Config.AddSubMenu(miscMenu);
            }
            var drawMenu = new Menu("Draw Settings", "Draw Settings");
            {
                drawMenu.AddItem(new MenuItem("qDraw", "Q Range").SetValue(new Circle(true, Color.White)));
                drawMenu.AddItem(new MenuItem("wDraw", "W Range").SetValue(new Circle(true, Color.DarkOrange)));
                drawMenu.AddItem(new MenuItem("eDraw", "E Range").SetValue(new Circle(true, Color.Green)));
                drawMenu.AddItem(new MenuItem("rDraw", "R Range").SetValue(new Circle(true, Color.Gold)));
                Config.AddSubMenu(drawMenu);
            }
            Config.AddItem(new MenuItem("useIgnite", "Smart Ignite").SetValue(true));
            Config.AddItem(new MenuItem("hChance", "Hit Chance").SetValue<StringList>(new StringList(HitchanceNameArray, 2)));
            var drawDamageMenu = new MenuItem("RushDrawEDamage", "E Damage").SetValue(true);
            var drawFill = new MenuItem("RushDrawEDamageFill", "E Damage Fill").SetValue(new Circle(true, Color.Gold));

            drawMenu.SubMenu("Damage Draws").AddItem(drawDamageMenu);
            drawMenu.SubMenu("Damage Draws").AddItem(drawFill);


            DamageIndicator.DamageToUnit = cDamage;
            DamageIndicator.Enabled = drawDamageMenu.GetValue<bool>();
            DamageIndicator.Fill = drawFill.GetValue<Circle>().Active;
            DamageIndicator.FillColor = drawFill.GetValue<Circle>().Color;

            drawDamageMenu.ValueChanged +=
            delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                DamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };

            drawFill.ValueChanged +=
            delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                DamageIndicator.Fill = eventArgs.GetNewValue<Circle>().Active;
                DamageIndicator.FillColor = eventArgs.GetNewValue<Circle>().Color;
            };

            SPrediction.Prediction.Initialize(Config);
            Config.AddToMainMenu();
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Config.Item("aGapcloser").GetValue<bool>())
            {
                if (gapcloser.Sender.IsValidTarget(1000))
                {
                    Render.Circle.DrawCircle(gapcloser.Sender.Position, gapcloser.Sender.BoundingRadius, Color.Gold, 5);
                    var targetpos = Drawing.WorldToScreen(gapcloser.Sender.Position);
                }
                if (E.CanCast(gapcloser.Sender))
                {
                    E.Cast(gapcloser.Sender);
                }
            }
        }
        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Config.Item("ainterrupt").GetValue<bool>())
            {
                if (sender.IsValidTarget(1000))
                {
                    Render.Circle.DrawCircle(sender.Position, sender.BoundingRadius, Color.Gold, 5);
                    var targetpos = Drawing.WorldToScreen(sender.Position);
                    Drawing.DrawText(targetpos[0] - 40, targetpos[1] + 20, Color.Gold, "Interrupt");
                }
                if (E.CanCast(sender))
                {
                    E.Cast(sender);
                }
            }
        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                Combo();
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                Harass();
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                Clear();
                JungleClear();
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
            {
                LastHit();
            }
            eToggle();
            eKS();
            immobileE();

        }
        public static bool CanMove(Obj_AI_Hero target)
        {
            return !(target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Knockup) ||
                target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Fear) || target.HasBuffOfType(BuffType.Knockback) ||
                target.HasBuffOfType(BuffType.Taunt) || target.HasBuffOfType(BuffType.Suppression) ||
                target.IsStunned || target.IsChannelingImportantSpell());
        }
        private static void Combo()
        {
            var useQ = Config.Item("qCombo").GetValue<bool>();
            var useW = Config.Item("wCombo").GetValue<bool>();
            var useE = Config.Item("eCombo").GetValue<bool>();
            var useR = Config.Item("rCombo").GetValue<bool>();
            var useIgnite = Config.Item("useIgnite").GetValue<bool>();
            HitChance hCance = HitchanceArray[Config.Item("hChance").GetValue<StringList>().SelectedIndex];
            byte minHit = (byte)Config.Item("minHitR").GetValue<Slider>().Value;

            if (Q.IsReady() && useQ)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && !x.IsDead && !x.IsZombie && x.IsValidTarget(Q.Range)))
                {
                    if (enemy.Distance(Player.Position) < Player.AttackRange)
                    {
                        Q.Cast(enemy);
                    }
                }
            }

            if (W.IsReady() && useW)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && !x.IsDead && !x.IsZombie && x.IsValidTarget(W.Range)))
                {
                    W.SPredictionCast(enemy, hCance);
                }
            }

            if (E.IsReady() && useE)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && !x.IsDead && !x.IsZombie && x.IsValidTarget(E.Range)))
                {
                    
                    if (Player.Distance(enemy.Position) < 550)
                    {
                        E.SPredictionCast(enemy, hCance);
                    }
                }
            }

            if (R.IsReady() && useR)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && !x.IsDead && !x.IsZombie && x.IsValidTarget(R.Range)))
                {
                    if (enemy.Health < cDamage(enemy))
                    {
                        R.SPredictionCast(enemy, hCance);
                    }
                    if (Player.CountEnemiesInRange(R.Range) > minHit)
                    {
                        R.SPredictionCast(enemy, hCance, 0, minHit);
                    }
                }
            }
            if (Ignite.IsReady() && useIgnite)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && !x.IsDead && !x.IsZombie
                    && x.IsValidTarget(550)))
                {
                    if (Player.GetSpellDamage(enemy, Ignite) + cDamage(enemy) > enemy.Health)
                    {
                        Player.Spellbook.CastSpell(Ignite, enemy);
                    }
                }
            }
        }
        private static float cDamage(Obj_AI_Base enemy)
        {
            var useQ = Config.Item("qCombo").GetValue<bool>();
            var useE = Config.Item("eCombo").GetValue<bool>();
            var useR = Config.Item("rCombo").GetValue<bool>();

            var qDAA = new Double[] { 20, 25, 30, 35, 40, 45, 50, 55, 60, 70, 80, 90, 110, 130, 150, 170, 190, 210 };
            float damage = 0;

            if (Q.IsReady() && useQ)
            {
                damage += Q.GetDamage(enemy);
            }
            if (Q.IsReady() || ObjectManager.Player.HasBuff("viktorpowertransferreturn") && useQ)
            {
                damage += (float)Player.CalcDamage(enemy, Damage.DamageType.Magical,
                    qDAA[Player.Level >= 18 ? 18 - 1 : Player.Level - 1] +
                    (Player.TotalMagicalDamage * .5) + Player.TotalAttackDamage());
            }
            if (E.IsReady() && useE)
            {
                if (Player.HasBuff("viktoreaug") || Player.HasBuff("viktorqeaug") || Player.HasBuff("viktorqweaug"))
                {
                    damage += E.GetDamage(enemy, 1);
                }
                else
                {
                    damage += E.GetDamage(enemy, 0);
                }
            }
            if (R.IsReady() && useR)
            {
                damage += R.GetDamage(enemy);
                damage += R.GetDamage(enemy, 2);
            }

            return (float)damage;
        }
        private static void Harass()
        {
            var useQ = Config.Item("qCombo").GetValue<bool>();
            var useE = Config.Item("eCombo").GetValue<bool>();
            HitChance hCance = HitchanceArray[Config.Item("hChance").GetValue<StringList>().SelectedIndex];
            var hMana = Config.Item("hMana").GetValue<Slider>().Value;

            if (ObjectManager.Player.ManaPercent > hMana)
            {
                if (Q.IsReady() && useQ)
                {
                    foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && !x.IsDead && !x.IsZombie && x.IsValidTarget(Q.Range)))
                    {
                        if (enemy.Distance(Player.Position) < Player.AttackRange)
                        {
                            Q.Cast(enemy);
                        }
                    }
                }
                if (E.IsReady() && useE)
                {
                    foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && !x.IsDead && !x.IsZombie && x.IsValidTarget(E.Range)))
                    {
                        E.SPredictionCast(enemy, hCance);
                    }
                }
            }
        }
        private static void eToggle()
        {
            var hMana = Config.Item("hMana").GetValue<Slider>().Value;
            if (ObjectManager.Player.ManaPercent > hMana)
            {
                var toggleE = Config.Item("eToggle").GetValue<bool>();
                HitChance tChance = HitchanceArray[Config.Item("tChance").GetValue<StringList>().SelectedIndex];

                if (E.IsReady() && toggleE)
                {
                    foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && !x.IsDead && !x.IsZombie && x.IsValidTarget(E.Range)))
                    {
                        E.SPredictionCast(enemy, tChance);
                    }
                }
            }
        }
        private static void Clear()
        {
            var useE = Config.Item("eClear").GetValue<bool>();
            var cMana = Config.Item("cMana").GetValue<Slider>().Value;
            var minionHit = Config.Item("eMinionCount").GetValue<Slider>().Value;
            if (Player.ManaPercent >= cMana)
            {
                if (E.IsReady() && useE)
                {
                    var eMinion = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range, MinionTypes.All);
                    var eFarmPos = E.GetCircularFarmLocation(eMinion, 200);

                    if (eFarmPos.MinionsHit >= minionHit && E.IsReady())
                        E.Cast(eFarmPos.Position);
                }
            }

        }
        private static void eKS()
        {
            var useE = Config.Item("eKS").GetValue<bool>();
            if (E.IsReady() && useE)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && !x.IsDead && !x.IsZombie
                    && x.IsValidTarget(E.Range) && E.GetDamage(x) > x.Health))
                {
                    HitChance hCance = HitchanceArray[Config.Item("hChance").GetValue<StringList>().SelectedIndex];
                    E.SPredictionCast(enemy, hCance);
                }
            }
        }
        private static void JungleClear()
        {
            var mob = MinionManager.GetMinions(Player.ServerPosition, Orbwalking.GetRealAutoAttackRange(Player) + 100, MinionTypes.All, MinionTeam.Neutral,MinionOrderTypes.MaxHealth);

            if (mob == null || (mob != null && mob.Count == 0))
            {
                return;
            }
            var useQ = Config.Item("qJungle").GetValue<bool>();
            var useW = Config.Item("wJungle").GetValue<bool>();
            var useE = Config.Item("eJungle").GetValue<bool>();
            var jMana = Config.Item("hMana").GetValue<Slider>().Value;

            if (ObjectManager.Player.ManaPercent > jMana)
            {
                if (Q.IsReady() && useQ)
                {
                    Q.Cast(mob[0]);
                }
                if (W.IsReady() && useW)
                {
                    W.Cast(mob[0].Position);
                }
                if (E.IsReady() && useE)
                {
                    E.Cast(mob[0].Position);
                }
            }
        }
        private static void immobileE()
        {
            var useE = Config.Item("eImmobile").GetValue<bool>();
            if (useE)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && !x.IsDead && !x.IsZombie
                   && x.IsValidTarget(E.Range) && CanMove(x)))
                {
                    HitChance hCance = HitchanceArray[Config.Item("hChance").GetValue<StringList>().SelectedIndex];
                    E.SPredictionCast(enemy, hCance);
                }
            }

        }
        private static void LastHit()
        {
            var lMana = Config.Item("lMana").GetValue<Slider>().Value;
            var useQ = Config.Item("qLast").GetValue<bool>();
            var qMinion = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Enemy);
            if (ObjectManager.Player.ManaPercent > lMana)
            {
                if (Q.IsReady() && useQ)
                {
                    foreach (var minyon in qMinion)
                    {
                        if (minyon.CharData.BaseSkinName.Contains("MinionSiege"))
                        {
                            if (Q.IsKillable(minyon))
                            {
                                Q.CastOnUnit(minyon);
                            }
                        }
                    }
                }
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            var menuItem1 = Config.Item("qDraw").GetValue<Circle>();
            var menuItem2 = Config.Item("wDraw").GetValue<Circle>();
            var menuItem3 = Config.Item("eDraw").GetValue<Circle>();
            var menuItem4 = Config.Item("rDraw").GetValue<Circle>();
            var killableDraws = Config.Item("killableDrew").GetValue<bool>();

            if (menuItem1.Active && Q.IsReady())
            {
                Render.Circle.DrawCircle(new Vector3(Player.Position.X, Player.Position.Y, Player.Position.Z), Q.Range, menuItem1.Color, 5);
            }
            if (menuItem2.Active && W.IsReady())
            {
                Render.Circle.DrawCircle(new Vector3(Player.Position.X, Player.Position.Y, Player.Position.Z), W.Range, menuItem2.Color, 5);
            }
            if (menuItem3.Active && E.IsReady())
            {
                Render.Circle.DrawCircle(new Vector3(Player.Position.X, Player.Position.Y, Player.Position.Z), E.Range + eRange - eRange / 4f, menuItem3.Color, 5);
            }
            if (menuItem4.Active && R.IsReady())
            {
                Render.Circle.DrawCircle(new Vector3(Player.Position.X, Player.Position.Y, Player.Position.Z), R.Range, menuItem4.Color, 5);
            }
            
        }
    }
}