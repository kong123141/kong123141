using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;
using LeagueSharp;
using SharpDX;
using Color = System.Drawing.Color;

namespace HikiCarry_Twitch
{
    class Program
    {
        public static string cName = "Twitch";
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> SpellList = new List<Spell>();
        public static Menu Config;
        private static Obj_AI_Hero Player = ObjectManager.Player;

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        public static string[] Marksman = { "Kalista", "Jinx", "Lucian", "Quinn", "Draven",  "Varus", "Graves", "Vayne", 
                                            "Caitlyn","Urgot", "Ezreal", "KogMaw", "Ashe", "MissFortune", "Tristana", "Teemo", 
                                            "Sivir","Twitch", "Corki"};

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

            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 950);
            E = new Spell(SpellSlot.E, 1200);
            R = new Spell(SpellSlot.R);

            W.SetSkillshot(0.25f, 120f, 1400f, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            Config = new Menu("HikiCarry - Twitch", "HikiCarry - Twitch", true);
            TargetSelector.AddToMenu(Config.SubMenu("Target Selector Settings"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker Settings"));

            var comboMenu = new Menu("Combo Settings", "Combo Settings");
            {
                comboMenu.AddItem(new MenuItem("wCombo", "Use W").SetValue(true));
                comboMenu.AddItem(new MenuItem("eCombo", "Use E").SetValue(true));
                comboMenu.AddItem(new MenuItem("rCombo", "Use R").SetValue(true));
                Config.AddSubMenu(comboMenu);
            }

            var harassMenu = new Menu("Harass Settings", "Harass Settings");
            {
                harassMenu.AddItem(new MenuItem("wHarass", "Use W").SetValue(true));
                harassMenu.AddItem(new MenuItem("eHarass", "Use E").SetValue(true));
                harassMenu.AddItem(new MenuItem("eSpearCount", "If Enemy Poison Count >= ").SetValue(new Slider(6, 1, 6)));
                harassMenu.AddItem(new MenuItem("manaHarass", "Harass Mana Manager").SetValue(new Slider(20, 0, 100)));
                Config.AddSubMenu(harassMenu);
            }

            var laneMenu = new Menu("LaneClear Settings", "LaneClear Settings");
            {
                laneMenu.AddItem(new MenuItem("wClear", "Use W").SetValue(true));
                laneMenu.AddItem(new MenuItem("eClear", "Use E").SetValue(true));
                laneMenu.AddItem(new MenuItem("eClearCount", "If Can Kill Minion >= ").SetValue(new Slider(4, 1, 5)));
                laneMenu.AddItem(new MenuItem("manaClear", "Clear Mana Manager").SetValue(new Slider(20, 0, 100)));
                Config.AddSubMenu(laneMenu);
            }

            var jungMenu = new Menu("JungleClear Settings", "JungleClear Settings");
            {
                jungMenu.AddItem(new MenuItem("wJungle", "Use W").SetValue(true));
                jungMenu.AddItem(new MenuItem("eJungle", "Use E").SetValue(true));
                jungMenu.AddItem(new MenuItem("manaJungle", "Jungle Mana Manager").SetValue(new Slider(20, 0, 100)));
                Config.AddSubMenu(jungMenu);
            }

            var ksMenu = new Menu("KillSteal Settings", "KillSteal Settings");
            {
                ksMenu.AddItem(new MenuItem("eKS", "Use E").SetValue(true));
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
                mobSteal.AddItem(new MenuItem("eSteal", "Use E").SetValue(true));
                Config.AddSubMenu(mobSteal);
            }

            var drawMenu = new Menu("Draw Settings", "Draw Settings");
            {
                drawMenu.AddItem(new MenuItem("qDraw", "Q Range").SetValue(new Circle(true, Color.White)));
                drawMenu.AddItem(new MenuItem("wDraw", "W Range").SetValue(new Circle(true, Color.Silver)));
                drawMenu.AddItem(new MenuItem("eDraw", "E Range").SetValue(new Circle(true, Color.Yellow)));
                drawMenu.AddItem(new MenuItem("rDraw", "R Range").SetValue(new Circle(true, Color.Gold)));
                Config.AddSubMenu(drawMenu);
            }

            var drawDamageMenu = new MenuItem("RushDrawEDamage", "E Damage").SetValue(true);
            var drawFill = new MenuItem("RushDrawEDamageFill", "E Damage Fill").SetValue(new Circle(true, Color.Gold));

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
            KillSteal();
            stealJungle();
        }
        private static void stealJungle()
        {

            var useE = Config.Item("eSteal").GetValue<bool>();
            var stealDragon = Config.Item("stealDragon").GetValue<bool>();
            var stealBaron = Config.Item("stealBaron").GetValue<bool>();
            var stealRed = Config.Item("stealRed").GetValue<bool>();
            var stealBlue = Config.Item("stealBlue").GetValue<bool>();
            var mob = MinionManager.GetMinions(Player.ServerPosition, Orbwalking.GetRealAutoAttackRange(Player) + 100, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (E.IsReady() && useE)
            {
                if (stealDragon && mob[0].CharData.BaseSkinName.Contains("Dragon")
                    && mob[0].Health + 50 + (mob[0].HPRegenRate / 2) <= E.GetDamage(mob[0])) // Dragon E
                {
                    E.Cast(mob[0]);
                }

                if (stealBaron && mob[0].BaseSkinName.Contains("Baron")
                    && mob[0].Health + 50 + (mob[0].HPRegenRate / 2) <= E.GetDamage(mob[0])) // Baron E
                {
                    E.Cast();
                }

                if (stealBlue && mob[0].BaseSkinName.Contains("SRU_Blue")
                    && mob[0].Health + 50 + (mob[0].HPRegenRate / 2) <= E.GetDamage(mob[0])) // Blue E
                {
                    E.Cast();
                }

                if (stealRed && mob[0].BaseSkinName.Contains("SRU_Red")
                    && mob[0].Health + 50 + (mob[0].HPRegenRate / 2) <= E.GetDamage(mob[0])) // Red E
                {
                    E.Cast();
                }
            }


        } // RDY
        private static void Combo()
        {
            var useW = Config.Item("wCombo").GetValue<bool>();
            var useE = Config.Item("eCombo").GetValue<bool>();
            var useR = Config.Item("rCombo").GetValue<bool>();
            float rEnemyCount = Player.CountEnemiesInRange(1337);

            if (useW && W.IsReady())
            {
                foreach (var enemy in HeroManager.Enemies.Where(o => o.IsValidTarget(W.Range) && o.IsEnemy && o.IsVisible && !o.IsZombie && !o.IsDead))
                {
                   
                    if (W.GetPrediction(enemy).Hitchance >= HitChance.High)
                    {
                        W.Cast(enemy);
                    }
                }
            }
            if (useE && E.IsReady())
            {

                foreach (var enemy in HeroManager.Enemies.Where(o => o.IsValidTarget(E.Range) && o.IsEnemy && o.IsVisible && !o.IsZombie && !o.IsDead
                    && o.HasBuff("twitchdeadlyvenom")))
                {
                    if (E.GetDamage(enemy) > enemy.Health)
                    {
                        E.Cast();
                    }
                }
            }
            if (useR && R.IsReady())
            {
                foreach (var enemy in HeroManager.Enemies.Where(o => o.IsValidTarget(W.Range) && o.IsEnemy && o.IsVisible && !o.IsZombie && !o.IsDead))
                {
                    if (rEnemyCount == 1 && ObjectManager.Player.Distance(enemy.Position) < W.Range 
                        && enemy.TotalAttackDamage < ObjectManager.Player.TotalAttackDamage &&
                        Marksman.Contains(enemy.ChampionName))
                    {
                        R.Cast();
                    }
                }
            }


        } // RDY
        private static void Harass()
        {
            var useW = Config.Item("qHarass").GetValue<bool>();
            var useE = Config.Item("eHarass").GetValue<bool>();
            var eSpearCount = Config.Item("eSpearCount").GetValue<Slider>().Value;
            var manaSlider = Config.Item("manaHarass").GetValue<Slider>().Value;

            if (Player.ManaPercent >= manaSlider)
            {
                if (useW && W.IsReady())
                {
                    foreach (var enemy in ObjectManager.Get<Obj_AI_Base>().Where(o => o.IsValidTarget(W.Range) && o.IsEnemy && o.IsVisible && !o.IsZombie && !o.IsDead))
                    {
                        if (W.GetPrediction(enemy).Hitchance >= HitChance.High)
                        {
                            W.Cast(enemy);
                        }
                    }
                    
                }
                if (useE && E.IsReady())
                {
                    foreach (var enemy in ObjectManager.Get<Obj_AI_Base>().Where(o => o.IsValidTarget(E.Range) && o.IsEnemy && o.IsVisible && !o.IsZombie && !o.IsDead
                    && o.HasBuff("twitchdeadlyvenom")))
                    {
                        if (enemy.GetBuffCount("twitchdeadlyvenom") >= eSpearCount)
                        {
                            E.Cast();
                        }
                    }
                }
            }
        } // RDY
        private static void Clear()
        {
            var manaSlider = Config.Item("manaHarass").GetValue<Slider>().Value;
            var eClearCount = Config.Item("eClearCount").GetValue<Slider>().Value;
            var allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range, MinionTypes.All);
            var useW = Config.Item("wClear").GetValue<bool>();
            if (Player.ManaPercent >= manaSlider)
            {
                if (W.IsReady() && useW)
                {
                    var wfarm = W.GetCircularFarmLocation(allMinionsW, 200);

                    if (wfarm.MinionsHit >= 3 && W.IsReady())
                    {
                        W.Cast(wfarm.Position);
                    }
                }
            }
        } // RDY
        /*private static float eDamageCalculator (Obj_AI_Hero hero) // SOON TM
        {
            var x1 = new[] { 14, 19, 24, 29, 35 };
            var x2 = new[] { 20, 35, 50, 65, 80 };
            var stacks = hero.GetBuffCount("twitchdeadlyvenom");

            var total = stacks * x1[E.Level] + 0.2 * Player.FlatMagicDamageMod + 0.25 * Player.FlatPhysicalDamageMod + x2[E.Level];

            return (float) total;

        }*/
        private static void KillSteal()
        {
            var eKS = Config.Item("eKS").GetValue<bool>();
            if (E.IsReady() && eKS)
            {
                foreach (var target in HeroManager.Enemies.Where(hero => hero.IsValidTarget(E.Range)))
                {
                    if (target.Health < E.GetDamage(target))
                    {
                        E.Cast();
                    }

                }
            }
        } // RDY
        private static void JungleClear()
        {
            var useW = Config.Item("wJungle").GetValue<bool>();
            var useE = Config.Item("eJungle").GetValue<bool>();
            var manaSlider = Config.Item("manaJungle").GetValue<Slider>().Value;
            var mob = MinionManager.GetMinions(Player.ServerPosition, Orbwalking.GetRealAutoAttackRange(Player) + 100, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (mob == null || (mob != null && mob.Count == 0))
            {
                return;
            }

            if (Player.ManaPercent > manaSlider)
            {
                if (W.IsReady() && useW)
                {
                    W.Cast(mob[0]);
                }
                if (E.IsReady() && useE)
                {
                    if (mob[0].Health + (mob[0].HPRegenRate / 2) <= E.GetDamage(mob[0]))
                    {
                        E.Cast();
                    }
                }
            }
        } // RDY
        private static float GetComboDamage(Obj_AI_Hero hero)
        {
            float damage = 0;

            if (E.IsReady())
            {
                damage += E.GetDamage(hero);
            }
            return damage;
        }
        private static void Drawing_OnDraw(EventArgs args) // RDY
        {
            var menuItem1 = Config.Item("qDraw").GetValue<Circle>();
            var menuItem2 = Config.Item("wDraw").GetValue<Circle>();
            var menuItem3 = Config.Item("eDraw").GetValue<Circle>();
            var menuItem4 = Config.Item("rDraw").GetValue<Circle>();
            var ePercent = Config.Item("ePercent").GetValue<bool>();

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
                Render.Circle.DrawCircle(new Vector3(Player.Position.X, Player.Position.Y, Player.Position.Z), E.Range, menuItem3.Color, 5);
            }
            if (menuItem4.Active && R.IsReady())
            {
                Render.Circle.DrawCircle(new Vector3(Player.Position.X, Player.Position.Y, Player.Position.Z), R.Range, menuItem4.Color, 5);
            }
            
        }
    }
}