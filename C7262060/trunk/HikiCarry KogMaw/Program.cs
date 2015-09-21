using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Geometry = LeagueSharp.Common.Geometry; 
using Color = System.Drawing.Color; 

namespace HikiCarry_KogMaw
{
    class Program
    {
        public const string ChampionName = "KogMaw"; 
        public static Orbwalking.Orbwalker Orbwalker; 
        public static List<Spell> SpellList = new List<Spell>(); 

        public static Menu Config;

        public static Spell Q; 
        public static Spell W; 
        public static Spell E;
        public static Spell R;
        public static float wRange = 760 + 20 * ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Level;
        public static float rRange = 800 + 300 * ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Level;
        

        private static Obj_AI_Hero Player; 
       
        static void Main(string[] args)
        {

            CustomEvents.Game.OnGameLoad += Game_OnGameLoad; 
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.BaseSkinName != ChampionName) return;


            Q = new Spell(SpellSlot.Q, 980) ;
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 1200);
            R = new Spell(SpellSlot.R, 1800);

            Q.SetSkillshot(0.25f, 50f, 2000f, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 120f, 1400f, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(1.5f, 200f, float.MaxValue, false, SkillshotType.SkillshotCircle);


            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            Activators.BOTRK.hikiBOTRK = true;
            Activators.Bilgewater.hikiBilgewater = true;
            Activators.Ghostblade.hikiGhostBlade = true;
            Activators.Potion.hikiPotion = true;
            Activators.QSS.hikiQSS = true;

            Config = new Menu("HikiCarry - KogMaw", "HikiCarry - KogMaw", true);
            TargetSelector.AddToMenu(Config.SubMenu("Target Selector Settings"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker Settings"));

            var comboMenu = new Menu("Combo Settings", "Combo Settings");
            {
                comboMenu.AddItem(new MenuItem("qCombo", "Use Q").SetValue(true));
                comboMenu.AddItem(new MenuItem("wCombo", "Use W").SetValue(true));
                comboMenu.AddItem(new MenuItem("eCombo", "Use E").SetValue(true));
                comboMenu.AddItem(new MenuItem("rCombo", "Use R").SetValue(true));
                Config.AddItem(new MenuItem("wType", "W Type").SetValue(new StringList(new[] { "Normal", "Only enemy if Immobile" },0)));
                Config.AddSubMenu(comboMenu);
            }
            var harassMenu = new Menu("Harass Settings", "Harass Settings");
            {
                harassMenu.AddItem(new MenuItem("wHarass", "Use W").SetValue(true));
                harassMenu.AddItem(new MenuItem("eHarass", "Use E").SetValue(true));
                harassMenu.AddItem(new MenuItem("rHarass", "Use R").SetValue(true));
                harassMenu.AddItem(new MenuItem("manaHarass", "Harass Mana Manager").SetValue(new Slider(20, 0, 100)));
                Config.AddSubMenu(harassMenu);
            }

            var laneMenu = new Menu("LaneClear Settings", "LaneClear Settings");
            {
                laneMenu.AddItem(new MenuItem("wClear", "Use W").SetValue(true));
                laneMenu.AddItem(new MenuItem("manaClear", "Clear Mana Manager").SetValue(new Slider(20, 0, 100)));
                Config.AddSubMenu(laneMenu);
            }
            var itemMenu = new Menu("Item Settings", "Item Settings");
            {
                var qssMenu = new Menu("QSS Settings", "QSS Settings");
                {
                    qssMenu.AddItem(new MenuItem("use.qss", "Use QSS").SetValue(true));
                    qssMenu.AddItem(new MenuItem("clear.ignite", "Clear Ignite").SetValue(true));
                    qssMenu.AddItem(new MenuItem("clear.exhaust", "Clear Exhaust").SetValue(true));
                    qssMenu.AddItem(new MenuItem("clear.zedult", "Clear Zed R").SetValue(true));
                    qssMenu.AddItem(new MenuItem("clear.fizzult", "Clear Fizz R").SetValue(true));
                    qssMenu.AddItem(new MenuItem("clear.malzaharult", "Clear Malzahar R").SetValue(true));
                    qssMenu.AddItem(new MenuItem("clear.vladulti", "Clear Vladimir R").SetValue(true));
                    itemMenu.AddSubMenu(qssMenu);
                }

                var botrk = new Menu("BOTRK Settings", "BOTRK Settings");
                {
                    botrk.AddItem(new MenuItem("useBOTRK", "Use BOTRK").SetValue(true));
                    botrk.AddItem(new MenuItem("myhp", "Use if my HP < %").SetValue(new Slider(20, 0, 100)));
                    botrk.AddItem(new MenuItem("theirhp", "Use if enemy HP < %").SetValue(new Slider(20, 0, 100)));
                    itemMenu.AddSubMenu(botrk);
                }

                var ghostBlade = new Menu("GhostBlade Settings", "GhostBlade Settings");
                {
                    ghostBlade.AddItem(new MenuItem("gBlade", "Use GhostBlade").SetValue(true));
                    itemMenu.AddSubMenu(ghostBlade);
                }

                var bilgewater = new Menu("Bilgewater Settings", "Bilgewater Settings");
                {
                    bilgewater.AddItem(new MenuItem("useBilge", "Use BOTRK").SetValue(true));
                    bilgewater.AddItem(new MenuItem("myhpbilge", "Use if my HP < %").SetValue(new Slider(20, 0, 100)));
                    bilgewater.AddItem(new MenuItem("theirhpbilge", "Use if enemy HP < %").SetValue(new Slider(20, 0, 100)));
                    itemMenu.AddSubMenu(bilgewater);
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

            var ksMenu = new Menu("KillSteal Settings", "KillSteal Settings");
            {
                ksMenu.AddItem(new MenuItem("qKS", "Use Q").SetValue(true));
                ksMenu.AddItem(new MenuItem("eKS", "Use E").SetValue(true));
                ksMenu.AddItem(new MenuItem("rKS", "Use R").SetValue(true));
                Config.AddSubMenu(ksMenu);
            }

            var miscMenu = new Menu("Miscellaneous", "Miscellaneous");
            {
                miscMenu.AddItem(new MenuItem("rImmobile", "Auto R to Immobile Target").SetValue(true));
                Config.AddSubMenu(miscMenu);
            }

            var drawMenu = new Menu("Draw Settings", "Draw Settings");
            {
                drawMenu.AddItem(new MenuItem("qDraw", "Q Range").SetValue(new Circle(true, Color.White)));
                drawMenu.AddItem(new MenuItem("wDraw", "W Range").SetValue(new Circle(true, Color.Silver)));
                drawMenu.AddItem(new MenuItem("eDraw", "E Range").SetValue(new Circle(true, Color.Yellow)));
                drawMenu.AddItem(new MenuItem("rDraw", "R Range").SetValue(new Circle(true, Color.Gold)));
                Config.AddSubMenu(drawMenu);
            }

            var drawDamageMenu = new MenuItem("drawDamage", "Combo Damage").SetValue(true);
            var drawFill = new MenuItem("damageFill", "Combo Fill").SetValue(new Circle(true, Color.Gold));

            drawMenu.SubMenu("Damage Draws").AddItem(drawDamageMenu);
            drawMenu.SubMenu("Damage Draws").AddItem(drawFill);

            DamageIndicator.DamageToUnit = GetComboDamage;
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

            Config.AddToMainMenu();
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }
        private static void Killsteal()
        {
            var useR = Config.Item("rKS").GetValue<bool>();

            if (R.IsReady() && useR)
            {
                foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
                {
                    if (R.CanCast(target) && R.GetPrediction(target).Hitchance >= HitChance.VeryHigh
                        && R.IsKillable(target))
                    {
                        R.Cast(target);
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

            if (menuItem1.Active && Q.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, menuItem1.Color);
            }
            if (menuItem2.Active && W.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, menuItem2.Color);
            }
            if (menuItem3.Active && E.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, wRange, menuItem3.Color);
            }
            if (menuItem4.Active && R.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, rRange, menuItem4.Color);
            }
        }
        static float GetComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (R.IsReady())
            {
                damage += (float)R.GetDamage(enemy);
            }

            if (Items.HasItem(3057)) // Sheen Damage
            {
                damage += (float)Player.CalcDamage(enemy, Damage.DamageType.Physical, 0.5 * Player.BaseAttackDamage);
            }
            if (Items.HasItem(3100)) // Lich Damage
            {
                damage += (float)ObjectManager.Player.CalcDamage(enemy, Damage.DamageType.Magical, (0.75 * ObjectManager.Player.BaseAttackDamage) + (0.50 * ObjectManager.Player.FlatMagicDamageMod));
            }
            if (Items.HasItem(3285)) // Luden Damage
            {
                damage += (float)ObjectManager.Player.CalcDamage(enemy, Damage.DamageType.Magical, 100 + (0.1 * ObjectManager.Player.FlatMagicDamageMod));
            }
            if (Items.HasItem(3153) && Items.CanUseItem(3153)) // Botrk Damage
            {
                if (ObjectManager.Player.Distance(enemy.Position) < 550)
                {
                    damage += (float)ObjectManager.Player.GetItemDamage(enemy, Damage.DamageItems.Botrk);
                }
            }
            if (Items.HasItem(3144) && Items.CanUseItem(3144)) // Bilgewater Damage
            {
                if (ObjectManager.Player.Distance(enemy.Position) < 550)
                {
                    damage += (float)ObjectManager.Player.GetItemDamage(enemy, Damage.DamageItems.Bilgewater);
                }
            }
            return (float)damage;
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
            }
            Killsteal();
            AutoR();
        }
        private static void Combo()
        {
            var useQ = Config.Item("qCombo").GetValue<bool>();
            var useW = Config.Item("wCombo").GetValue<bool>();
            var useE = Config.Item("eCombo").GetValue<bool>();
            var useR = Config.Item("rCombo").GetValue<bool>();

            if (useE && E.IsReady())
            {
                foreach (var target in HeroManager.Enemies.Where(o => o.IsEnemy && o.IsValidTarget(E.Range) 
                    && !o.IsDead && !o.IsZombie))
                {
                    if (E.GetPrediction(target).Hitchance >= HitChance.High)
                    {
                        E.Cast(target);
                    }
                }
            }
            if (useQ && Q.IsReady())
            {
                foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(Q.Range) 
                    && !o.IsDead && !o.IsZombie))
                {
                    if (Q.GetPrediction(target).Hitchance >= HitChance.High)
                    {
                        Q.Cast(target);
                    }
                }
                
            }
            if (useW && W.IsReady())
            {
                foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(wRange-100) && !o.IsDead && !o.IsZombie))
                {
                    if (Config.Item("wType").GetValue<StringList>().SelectedIndex == 0) // Normal
                    {
                        W.Cast();
                    }
                    if (Config.Item("wType").GetValue<StringList>().SelectedIndex == 1 && immobileTarget(target)) // If Immobile
                    {
                        W.Cast();
                    }
                }
            }
            if (useR && R.IsReady())
            {
                foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(rRange)
                    && !o.IsDead && !o.IsZombie))
                {
                    if (R.GetPrediction(target).Hitchance >= HitChance.High)
                    {
                        R.Cast(target);
                    }
                }
            }
        }
        private static bool immobileTarget(Obj_AI_Base target)
        {
            if (target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Knockup) ||
                target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Fear) || target.HasBuffOfType(BuffType.Knockback) ||
                target.HasBuffOfType(BuffType.Taunt) || target.HasBuffOfType(BuffType.Suppression) ||
                target.IsStunned)
            {
                return true;
            }
            else
                return false;
        }
        private static void Clear()
        {
            var useW = Config.Item("wHarass").GetValue<bool>();
            var clearMana = Config.Item("manaHarass").GetValue<Slider>().Value;
            var allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, wRange, MinionTypes.All);
            if (ObjectManager.Player.ManaPercent >= clearMana)
            {
                if (W.IsReady() && useW && allMinionsW.Where(x => x.IsValidTarget(wRange)).Count() >= 3)
                {
                    W.Cast();
                }
            }
        }
        private static void Harass()
        {
            
            var useW = Config.Item("wHarass").GetValue<bool>();
            var useE = Config.Item("eHarass").GetValue<bool>();
            var useR = Config.Item("rHarass").GetValue<bool>();
            var harassMana = Config.Item("manaHarass").GetValue<Slider>().Value;
            if (ObjectManager.Player.ManaPercent > harassMana)
            {
                if (useW && W.IsReady())
                {
                    foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(wRange - 100)
                    && !o.IsDead && !o.IsZombie))
                    {
                        W.Cast();
                    }
                }
                if (useE && E.IsReady())
                {
                    if (useE && E.IsReady())
                    {
                        foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(E.Range)
                            && !o.IsDead && !o.IsZombie))
                        {
                            if (E.GetPrediction(target).Hitchance >= HitChance.High)
                            {
                                E.Cast(target);
                            }
                        }
                    }
                }
                if (useR && R.IsReady())
                {
                    foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(rRange)
                    && !o.IsDead && !o.IsZombie))
                    {
                        if (R.GetPrediction(target).Hitchance >= HitChance.High)
                        {
                            R.Cast(target);
                        }
                    }
                }
            }
        }
        private static void AutoR()
        {
            var useR = Config.Item("rImmobile").GetValue<bool>();

            if (useR && R.IsReady())
            {
                foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(rRange)
                    && !o.IsDead && !o.IsZombie))
                {
                    if (target != null && immobileTarget(target))
                    {
                        if (R.GetPrediction(target).Hitchance >= HitChance.High && Player.Buffs.Count(o => o.Name == "kogmawlivingartillery") < 6)
                        {
                            R.Cast(target);
                        }
                    }
                }
            }
        }
    }
}
