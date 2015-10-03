using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using HikiCarry_Sivir.Utils;
using Geometry = LeagueSharp.Common.Geometry;
using Color = System.Drawing.Color;

namespace HikiCarry_Sivir
{
    class Program
    {
        public const string ChampionName = "Sivir";
        public static Orbwalking.Orbwalker Orbwalker;

        public static Menu Config;

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        public static string[] HitchanceNameArray = { "Low", "Medium", "High", "Very High", "Only Immobile" };
        public static HitChance[] HitchanceArray = { HitChance.Low, HitChance.Medium, HitChance.High, HitChance.VeryHigh, HitChance.Immobile };

        static void Main(string[] args)
        {

            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != ChampionName) return;


            Q = new Spell(SpellSlot.Q, 1245f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 1000f);

            Q.SetSkillshot(0.25f, 90f, 1350f, false, SkillshotType.SkillshotLine);



            var comboMenu = new Menu("Combo Settings", "Combo Settings");
            {
                comboMenu.AddItem(new MenuItem("qCombo", "Use Q").SetValue(true));
                comboMenu.AddItem(new MenuItem("wCombo", "Use W").SetValue(true));
                comboMenu.AddItem(new MenuItem("rCombo", "Use R").SetValue(true));
                Config.AddSubMenu(comboMenu);
            }

            var ultiMenu = new Menu("R Settings", "R Settings");
            {
                ultiMenu.AddItem(new MenuItem("r.combo", "R on x Enemy").SetValue(new Slider(3, 1, 5)));
                Config.AddSubMenu(ultiMenu);
            }
            var DODGE = new Menu("E Settings", "E Settings");
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(o => o.IsEnemy))
                {
                    foreach (var skillshot in SpellDatabase.Spells.Where(x => x.charName == enemy.ChampionName)) // 2.5F DODGE
                    {
                        DODGE.AddItem(new MenuItem("hero." + skillshot.spellName, "Enemy: " + skillshot.charName + " | Skill: " + skillshot.spellKey + " | Spell Name: " + skillshot.name).SetValue(true));
                    }
                }
                Config.AddSubMenu(DODGE);
            }
            var harassMenu = new Menu("Harass Settings", "Harass Settings");
            {
                harassMenu.AddItem(new MenuItem("qHarass", "Use Q").SetValue(true));
                harassMenu.AddItem(new MenuItem("manaHarass", "Harass Mana Manager").SetValue(new Slider(20, 0, 100)));
                Config.AddSubMenu(harassMenu);
            }

            var toggleMenu = new Menu("Toggle Settings", "Toggle Settings");
            {
                toggleMenu.AddItem(new MenuItem("qToggle", "Use Q").SetValue(true));
                toggleMenu.AddItem(new MenuItem("manaToggle", "Toggle Mana Manager").SetValue(new Slider(20, 0, 100)));
                Config.AddSubMenu(toggleMenu);
            }

            var laneMenu = new Menu("LaneClear Settings", "LaneClear Settings");
            {
                laneMenu.AddItem(new MenuItem("qClear", "Use Q").SetValue(true));
                laneMenu.AddItem(new MenuItem("q.minion.hit", "Q Minion Hit >=").SetValue(new Slider(3, 0, 5)));
                laneMenu.AddItem(new MenuItem("manaClear", "Clear Mana Manager").SetValue(new Slider(20, 0, 100)));
                Config.AddSubMenu(laneMenu);
            }

            var jungMenu = new Menu("JungleClear Settings", "JungleClear Settings");
            {
                jungMenu.AddItem(new MenuItem("qJungle", "Use Q").SetValue(true));
                jungMenu.AddItem(new MenuItem("wJungle", "Use W").SetValue(true));
                jungMenu.AddItem(new MenuItem("manaJungle", "Jungle Mana Manager").SetValue(new Slider(20, 0, 100)));
                Config.AddSubMenu(jungMenu);
            }

            var ksMenu = new Menu("KillSteal Settings", "KillSteal Settings");
            {
                ksMenu.AddItem(new MenuItem("qKS", "Use Q").SetValue(true));
                Config.AddSubMenu(ksMenu);
            }

            var mobSteal = new Menu("Mob Steal Settings", "Mob Steal Settings");
            {
                var stealMobs = new Menu("Stealable Mobs", "Stealable Mobs");
                {
                    stealMobs.AddItem(new MenuItem("stealDragon", "Steal Dragon").SetValue(true));
                    stealMobs.AddItem(new MenuItem("stealBaron", "Steal Baron").SetValue(true));
                    stealMobs.AddItem(new MenuItem("stealRed", "Steal Red").SetValue(true));
                    stealMobs.AddItem(new MenuItem("stealBlue", "Steal Blue").SetValue(true));
                    mobSteal.AddSubMenu(stealMobs);
                }
                mobSteal.AddItem(new MenuItem("qSteal", "Use Q").SetValue(true));
                Config.AddSubMenu(mobSteal);
            }
            var drawSettings = new Menu("Draw Settings", "Draw Settings");
            {
                drawSettings.AddItem(new MenuItem("qDraw", "Q Range").SetValue(new Circle(true, Color.Yellow)));
                drawSettings.AddItem(new MenuItem("wDraw", "W Range").SetValue(new Circle(true, Color.Green)));
                drawSettings.AddItem(new MenuItem("eDraw", "E Range").SetValue(new Circle(true, Color.White)));
                drawSettings.AddItem(new MenuItem("rDraw", "R Range").SetValue(new Circle(true, Color.Brown)));
            }
            Config.AddItem(new MenuItem("hikiChance", "Hit Chance").SetValue<StringList>(new StringList(HitchanceNameArray, 2)));
            var drawDamageMenu = new MenuItem("RushDrawEDamage", "Q Damage").SetValue(true);
            var drawFill = new MenuItem("RushDrawEDamageFill", "Q Damage Fill").SetValue(new Circle(true, Color.SeaGreen));
            Config.SubMenu("Drawings").AddItem(drawDamageMenu);
            Config.SubMenu("Drawings").AddItem(drawFill);

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
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Drawing.OnDraw += Drawing_OnDraw;
        }
        static float GetComboDamage(Obj_AI_Base hero)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage(hero);


            if (damage > hero.Health && Q.IsReady())
            {
                var yx = Drawing.WorldToScreen(hero.Position);
                Drawing.DrawText(yx[0], yx[1], System.Drawing.Color.Yellow, "Q = KILL");
            }
            return damage;
        }

       
        private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs spell)
        {
            foreach (var evader in SpellDatabase.Spells.Where(o => o.spellName == spell.SData.Name))
            {
                if (Config.Item("hero." + evader.spellName).GetValue<bool>())
                {
                    switch (evader.spellType)
                    {
                        case SpellType.Cone:
                            if (ObjectManager.Player.Distance(spell.End) <= 250)
                            {
                                if (E.IsReady())
                                {
                                    E.Cast();
                                }
                            }
                            break;
                        case SpellType.Line:
                            if (ObjectManager.Player.Distance(spell.End) <= 100)
                            {
                                if (E.IsReady())
                                {
                                    E.Cast();
                                }
                            }
                            break;
                        case SpellType.Circular:
                            if (ObjectManager.Player.Distance(spell.End) <= 250)
                            {
                                if (E.IsReady())
                                {
                                    E.Cast();
                                }
                            }
                            break;
                    }
                }
            }
        
        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;

                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;

                case Orbwalking.OrbwalkingMode.LaneClear:
                    Clear();
                    Jungle();
                    break;
            }

          
        }

        private static void Jungle()
        {
            var useQ = Config.Item("qJungle").GetValue<bool>();
            var useW = Config.Item("wJungle").GetValue<bool>();
            var manaSlider = Config.Item("manaJungle").GetValue<Slider>().Value;
            var mob = MinionManager.GetMinions(Player.ServerPosition, Orbwalking.GetRealAutoAttackRange(Player) + 100, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (mob == null || (mob != null && mob.Count == 0))
            {
                return;
            }

            if (Player.ManaPercent > manaSlider)
            {
                if (Q.IsReady() && useQ)
                {
                    Q.Cast(mob[0]);
                }
                if (W.IsReady() && useW)
                {
                    W.Cast();
                }
            }
        }

        private static void Combo()
        {
            var useQ = Config.Item("qCombo").GetValue<bool>();
            var useW = Config.Item("wCombo").GetValue<bool>();
            var useR = Config.Item("rCombo").GetValue<bool>();
            var rxEnemy = Config.Item("r.combo").GetValue<Slider>().Value;
            var hikiChance = HitchanceArray[Config.Item("hikiChance").GetValue<StringList>().SelectedIndex];
            if (useQ && Q.IsReady())
            {
                foreach (var enemy in HeroManager.Enemies.Where(o => o.IsValidTarget(Q.Range) && o.IsEnemy && !o.IsZombie && !o.IsDead))
                {
                    if (Q.GetPrediction(enemy).Hitchance >= hikiChance)
                    {
                        Q.Cast(enemy);
                    }
                }
            }

            if (useW && W.IsReady())
            {
                foreach (var enemy in HeroManager.Enemies.Where(o => o.IsValidTarget(500) && o.IsEnemy && !o.IsZombie && !o.IsDead))
                {
                    W.Cast();
                }
            }
            if (useR && R.IsReady())
            {
                foreach (var enemy in HeroManager.Enemies.Where(o => o.CountEnemiesInRange(1000) > rxEnemy && o.IsEnemy && !o.IsZombie && !o.IsDead))
                {
                    R.Cast();
                }
            }
            
        }
        private static void StealJungle()
        {
            var useQ = Config.Item("qSteal").GetValue<bool>();
            var stealDragon = Config.Item("stealDragon").GetValue<bool>();
            var stealBaron = Config.Item("stealBaron").GetValue<bool>();
            var stealRed = Config.Item("stealRed").GetValue<bool>();
            var stealBlue = Config.Item("stealBlue").GetValue<bool>();
            var mob = MinionManager.GetMinions(Player.ServerPosition, Orbwalking.GetRealAutoAttackRange(Player) + 100, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (E.IsReady() && useQ)
            {
                if (stealDragon && mob[0].CharData.BaseSkinName.Contains("Dragon")
                    && mob[0].Health + 50 + (mob[0].HPRegenRate / 2) <= E.GetDamage(mob[0])) // Dragon E
                {
                    Q.Cast(mob[0]);
                }
                if (stealBaron && mob[0].CharData.BaseSkinName.Contains("Baron")
                    && mob[0].Health + 50 + (mob[0].HPRegenRate / 2) <= E.GetDamage(mob[0])) // Baron E
                {
                    Q.Cast();
                }
                if (stealBlue && mob[0].CharData.BaseSkinName.Contains("SRU_Blue")
                    && mob[0].Health + 50 + (mob[0].HPRegenRate / 2) <= E.GetDamage(mob[0])) // Blue E
                {
                    Q.Cast();
                }
                if (stealRed && mob[0].CharData.BaseSkinName.Contains("SRU_Red")
                    && mob[0].Health + 50 + (mob[0].HPRegenRate / 2) <= E.GetDamage(mob[0])) // Red E
                {
                    Q.Cast();
                }
            }
        }
        private static void ToggleQ()
        {
            var useQ = Config.Item("qToggle").GetValue<bool>();
            var manaSlider = Config.Item("manaToggle").GetValue<Slider>().Value;
            var hikiChance = HitchanceArray[Config.Item("hikiChance").GetValue<StringList>().SelectedIndex];
            if (Player.ManaPercent > manaSlider)
            {
                if (useQ && Q.IsReady())
                {
                    foreach (var enemy in HeroManager.Enemies.Where(o => o.IsValidTarget(Q.Range) && o.IsEnemy && !o.IsZombie && !o.IsDead))
                    {
                        if (Q.GetPrediction(enemy).Hitchance >= hikiChance)
                        {
                            Q.Cast(enemy);
                        }
                    }
                }
            }
        }
        private static void KillSteal()
        {
            var useQ = Config.Item("qKS").GetValue<bool>();
            if (useQ && Q.IsReady())
            {
                foreach (var enemy in HeroManager.Enemies.Where(o => o.IsValidTarget(Q.Range) && o.Health < Q.GetDamage(o)))
                {
                    Q.Cast(enemy);
                }
            }
        }
        private static void Harass()
        {
            var useQ = Config.Item("qHarass").GetValue<bool>();
            var manaSlider = Config.Item("manaHarass").GetValue<Slider>().Value;
            var hikiChance = HitchanceArray[Config.Item("hikiChance").GetValue<StringList>().SelectedIndex];

            if (Player.ManaPercent >= manaSlider)
            {
                if (useQ && Q.IsReady())
                {
                    foreach (var enemy in HeroManager.Enemies.Where(o => o.IsValidTarget(Q.Range) && o.IsEnemy && !o.IsZombie && !o.IsDead))
                    {
                        if (Q.GetPrediction(enemy).Hitchance >= hikiChance)
                        {
                            Q.Cast(enemy);
                        }
                    }
                }
            }
        }
        private static void Clear()
        {
            var qMinion = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Enemy);
            var useQ = Config.Item("qClear").GetValue<bool>();
            var manaSlider = Config.Item("manaClear").GetValue<Slider>().Value;
            var qHit = Config.Item("q.minion.hit").GetValue<Slider>().Value;
            if (Player.ManaPercent > manaSlider)
            {
                if (Q.IsReady() && useQ && Player.ManaPercent >= Config.Item("manaClear").GetValue<Slider>().Value)
                {
                    var QFarm = Q.GetCircularFarmLocation(qMinion, 200);
                    if (QFarm.MinionsHit >= qHit && Q.IsReady())
                    {
                        Q.Cast(QFarm.Position);
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
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, menuItem1.Color, 1);
            }
            if (menuItem2.Active && W.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, menuItem2.Color, 1);
            }
            if (menuItem3.Active && E.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, menuItem3.Color, 1);
            }
            if (menuItem4.Active && R.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, menuItem4.Color, 1);
            }
        }
    }
}