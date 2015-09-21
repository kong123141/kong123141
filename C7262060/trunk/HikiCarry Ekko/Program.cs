using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Geometry = LeagueSharp.Common.Geometry;
using Color = System.Drawing.Color;

namespace HikiCarry___Ekko
{
    class Program
    {
        public const string ChampionName = "Ekko";
        public static Orbwalking.Orbwalker Orbwalker;
        public static Menu TargetSelectorMenu;
        public static List<Spell> SpellList = new List<Spell>();

        static int Delay = 0;

        public static Menu Config;

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        private static Obj_AI_Hero Player;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.BaseSkinName != ChampionName) return;


            Q = new Spell(SpellSlot.Q, 1075);
            W = new Spell(SpellSlot.W, 1600);
            E = new Spell(SpellSlot.E, 400f);


            Q.SetSkillshot(0.25f, 70f, 2200f, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(3f, 90f, float.MaxValue, false, SkillshotType.SkillshotCircle);


            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            //MENU
            Config = new Menu("HikiCarry Ekko", "HikiCarry - Ekko", true);

            //TARGET SELECTOR
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //ORBWALKER
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //INFO
            Config.AddSubMenu(new Menu("Info", "Info"));
            Config.SubMenu("Info").AddItem(new MenuItem("Author", "@Hikigaya"));

            //COMBO
            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("RushQCombo", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("RushWCombo", "Use W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("RushECombo", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("RushRCombo", "Use R").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("MinEnemyW", "Min enemys for W").SetValue(new Slider(3, 5, 1)));
            Config.SubMenu("Combo").AddItem(new MenuItem("hpToR", "% Hp to R").SetValue(new Slider(30, 0, 100)));
            Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            //LANECLEAR
            Config.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("laneclearUseQ", "Use Q", true).SetValue(true));
           

            Config.AddSubMenu(new Menu("JungleClear", "JungleClear"));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("jungleclearUseQ", "Use Q", true).SetValue(true));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("jungleclearUseW", "Use W", true).SetValue(true));
            Config.SubMenu("JungleClear").AddItem(new MenuItem("jungleclearUseE", "Use E", true).SetValue(true));
           

            //HARASS
            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("RushQHarass", "Use Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("harassmana", "Haras Mana Percent").SetValue(new Slider(30, 0, 100)));

            //MISC
            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("agapcloser", "Anti-Gapcloser with E!").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("wStunQ", "Enemy is Stunned - Cast Q!").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("vsMode", "1v1 Enabled").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("twostackQ", "If enemy have 2 stack Auto Q").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("ksQ", "KillSteal Q!").SetValue(true));
           

            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RushQRange", "Q Range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RushWRange", "W Range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RushERange", "E Range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RushRRange", "Killable Text").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Config.Item("agapcloser").GetValue<bool>() || Player.IsDead)
                return;

            if (E.CanCast(gapcloser.Sender))
                E.Cast(gapcloser.Sender);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
               
               Combo();
               

            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                Harass();

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                Laneclear();
                
            }
            if (Config.Item("wStunQ").GetValue<bool>() && Q.IsReady())
            {
                var target0 = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                if (target0.Buffs.Any(buff => buff.Name == "ekkowstun"))
                {
                        if (Q.CanCast(target0) && Q.GetPrediction(target0).Hitchance >= HitChance.VeryHigh)
                            Q.Cast(target0);
                }
            }

            if (Config.Item("twostackQ").GetValue<bool>() && Q.IsReady())
            {
                var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                if (t.Buffs.Any(buff => buff.Name == "EkkoStacks" && buff.Count == 2))
                    //   {
                    foreach (var En in HeroManager.Enemies.Where(hero => hero.IsValidTarget(Q.Range)))
                    {
                        if (Q.CanCast(t))
                            Q.Cast(t);
                    }
            }
           
           

           
        }
        
        private static float ComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);
            if (Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

            return (float)damage;
        }
        private static int GetEnemys(Obj_AI_Hero target)
        {
            int Enemys = 0;
            foreach (Obj_AI_Hero enemys in ObjectManager.Get<Obj_AI_Hero>())
            {
                var pred = W.GetPrediction(enemys, true);
                if (pred.Hitchance >= HitChance.High && !enemys.IsMe && enemys.IsEnemy && Vector3.Distance(Player.Position, pred.UnitPosition) <= W.Range)
                {
                    Enemys = Enemys + 1;
                }
            }
            return Enemys;
        }
        private static void Combo()
        {
            var numOfEnemies = Config.Item("MinEnemyW").GetValue<Slider>().Value;

            float RangeEnemyCheck = ObjectManager.Player.CountEnemiesInRange(1000);

            
            
            
            if (RangeEnemyCheck == 1  && Config.Item("vsMode").GetValue<bool>())
            {
                if (Q.IsReady() && Config.Item("RushQCombo").GetValue<bool>())
                {
                    var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                    foreach (var En in HeroManager.Enemies.Where(hero => hero.IsValidTarget(Q.Range)))
                    {
                        if (Q.CanCast(t) && Q.GetPrediction(t).Hitchance >= HitChance.VeryHigh)
                            Q.Cast(t);
                    }
                }

                if (E.IsReady() && Config.Item("RushECombo").GetValue<bool>())
                {
                  
                    foreach (var En in HeroManager.Enemies.Where(hero => hero.IsValidTarget(550)))
                    {
                        E.Cast(Game.CursorPos);
                    }
                       

                }

                float healthPercent2 = Player.Health / Player.MaxHealth * 100;
                if (healthPercent2 <= Config.Item("hpToR").GetValue<Slider>().Value && R.IsReady())
                {
                    R.Cast();
                }

                if (W.IsReady() && Config.Item("RushWCombo").GetValue<bool>())
                {
                    var t = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
                    foreach (var En in HeroManager.Enemies.Where(hero => hero.IsValidTarget(Q.Range)))
                    {
                        if (W.CanCast(t) && W.GetPrediction(t).Hitchance >= HitChance.VeryHigh)
                            W.Cast(t);
                    }
                }
            }
            // BASIC COMBO
            if (Q.IsReady() && Config.Item("RushQCombo").GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                foreach (var En in HeroManager.Enemies.Where(hero => hero.IsValidTarget(Q.Range)))
                {
                    if (Q.CanCast(t) && Q.GetPrediction(t).Hitchance >= HitChance.VeryHigh)
                        Q.Cast(t);
                }
            }

            if (E.IsReady() && Config.Item("RushECombo").GetValue<bool>())
            {

                foreach (var En in HeroManager.Enemies.Where(hero => hero.IsValidTarget(550)))
                {
                    E.Cast(Game.CursorPos);
                }


            }

            float healthPercent = Player.Health / Player.MaxHealth * 100;
            if (healthPercent <= Config.Item("hpToR").GetValue<Slider>().Value && R.IsReady())
            {
                R.Cast();
            }

            if (W.IsReady() && Config.Item("RushWCombo").GetValue<bool>())
            {
                var t2 = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
                var Wcount = Utility.CountEnemiesInRange(Player, W.Range);
                var ReqWcount = Config.Item("MinEnemyW").GetValue<Slider>().Value;
                if (ReqWcount <= Wcount)
                {
                    W.Cast(t2);
                }
            }
             


          
        }

        private static void Harass()
        {
            
            if (Q.IsReady() && Config.Item("RushQHarass").GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                foreach (var En in HeroManager.Enemies.Where(hero => hero.IsValidTarget(Q.Range)))
                {
                    if (Q.CanCast(t) && Q.GetPrediction(t).Hitchance >= HitChance.VeryHigh)
                        Q.Cast(t);
                }
            }

            
        }

        private static void Laneclear()
        {
            if (Config.Item("laneclearUseQ", true).GetValue<Boolean>())
            {
                var Minions = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Enemy);
                var farmloc = Q.GetLineFarmLocation(Minions);


                Q.Cast(farmloc.Position);
            }
           

            if (Config.Item("jungleclearUseQ", true).GetValue<Boolean>())
            {
                var Mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

                if (Mobs.Count <= 0)
                    return;

                if (Q.CanCast(Mobs[0]))
                    Q.Cast(Mobs[0]);
            }
            if (Config.Item("jungleclearUseW", true).GetValue<Boolean>())
            {
                var Mobs = MinionManager.GetMinions(Player.ServerPosition, W.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

                if (Mobs.Count <= 0)
                    return;

                if (W.CanCast(Mobs[0]))
                    W.Cast(Mobs[0]);
            }
            if (Config.Item("jungleclearUseE", true).GetValue<Boolean>())
            {
                var Mobs = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

                if (Mobs.Count <= 0)
                    return;

                if (E.CanCast(Mobs[0]))
                    E.Cast(Game.CursorPos);
            }
           
           
        }

       

        private static void Drawing_OnDraw(EventArgs args)
        {
            var menuItem1 = Config.Item("RushQRange").GetValue<Circle>();
            var menuItem2 = Config.Item("RushWRange").GetValue<Circle>();
            var menuItem3 = Config.Item("RushERange").GetValue<Circle>();
            var menuItem4 = Config.Item("RushRRange").GetValue<Circle>();





            if (Config.Item("RushQCombo").GetValue<bool>() && Q.IsReady())
            {
                if (menuItem1.Active) Utility.DrawCircle(Player.Position, Q.Range, Color.SkyBlue);
            }

            if (Config.Item("RushWCombo").GetValue<bool>() && W.IsReady())
            {
                if (menuItem2.Active) Utility.DrawCircle(Player.Position, W.Range, Color.Yellow);
            }
            if (Config.Item("RushECombo").GetValue<bool>() && E.IsReady())
            {
                if (menuItem3.Active) Utility.DrawCircle(Player.Position, E.Range, Color.SpringGreen);

            }
            if (Config.Item("RushQCombo").GetValue<bool>() && Config.Item("RushECombo").GetValue<bool>() && Q.IsReady() && E.IsReady())
            {
                if (menuItem4.Active)

                    foreach (
                    var enemyVisible in
                        ObjectManager.Get<Obj_AI_Hero>().Where(enemyVisible => enemyVisible.IsValidTarget()))

                        if (ComboDamage(enemyVisible) > enemyVisible.Health)
                        {
                            Drawing.DrawText(Drawing.WorldToScreen(enemyVisible.Position)[0] + 50,
                                Drawing.WorldToScreen(enemyVisible.Position)[1] - 40, Color.Red,
                                "Combo[E+Q] = Kill");
                        }
            }

        }
    }
}
