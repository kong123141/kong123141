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
using Collision = LeagueSharp.Common.Collision;

namespace HikiCarry_Caitlyn
{
    class Program
    {
        public const string ChampionName = "Caitlyn";
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> SpellList = new List<Spell>();
        static List<Spells> SpellListt = new List<Spells>();
        static int Delay = 0;

        public static Menu Config;

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        private static Obj_AI_Hero Player;
        public struct Spells
        {
            public string ChampionName;
            public string SpellName;
            public SpellSlot slot;
        }
        static void Main(string[] args)
        {

            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.BaseSkinName != ChampionName) return;


            Q = new Spell(SpellSlot.Q, 1240);
            W = new Spell(SpellSlot.W, 820);
            E = new Spell(SpellSlot.E, 800);
            R = new Spell(SpellSlot.R, 2000);

            Q.SetSkillshot(0.25f, 60f, 2000f, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 80f, 1600f, true, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            SpellListt.Add(new Spells { ChampionName = "akali", SpellName = "akalismokebomb", slot = SpellSlot.W });   //Akali W
            SpellListt.Add(new Spells { ChampionName = "shaco", SpellName = "deceive", slot = SpellSlot.Q }); //Shaco Q
            SpellListt.Add(new Spells { ChampionName = "khazix", SpellName = "khazixr", slot = SpellSlot.R }); //Khazix R
            SpellListt.Add(new Spells { ChampionName = "khazix", SpellName = "khazixrlong", slot = SpellSlot.R }); //Khazix R Evolved
            SpellListt.Add(new Spells { ChampionName = "talon", SpellName = "talonshadowassault", slot = SpellSlot.R }); //Talon R
            SpellListt.Add(new Spells { ChampionName = "monkeyking", SpellName = "monkeykingdecoy", slot = SpellSlot.W }); //Wukong W

            //MENU
            Config = new Menu("HikiCarry - Caitlyn", "HikiCarry - Caitlyn", true);

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
            Config.SubMenu("Combo").AddItem(new MenuItem("RushECombo", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("RushWCombo", "Use W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("RushRCombo", "Use R").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("combotype", "Combo Mode").SetValue(new StringList(new[] { "AD", "AP" })));
            Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            //LANECLEAR
            Config.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("clearmana", "Clear Mana Percent").SetValue(new Slider(30, 0, 100)));

            //INVISIBLE KICKER
            Config.AddSubMenu(new Menu("Invisible Kicker", "Invisiblez"));
            Config.SubMenu("Invisiblez").AddItem(new MenuItem("Use", "Use Vision Ward On Combo").SetValue(new KeyBind(32, KeyBindType.Press)));

            {
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy))
                {
                    foreach (var spell in SpellListt.Where(x => x.ChampionName.ToLower() == hero.ChampionName.ToLower()))
                    {
                        Config.SubMenu("Invisiblez").AddItem(new MenuItem(hero.ChampionName.ToLower() + spell.slot.ToString(), hero.ChampionName + " - " + spell.slot.ToString()).SetValue(true));
                    }
                }

                if (HeroManager.Enemies.Any(x => x.ChampionName.ToLower() == "rengar"))
                {
                    Config.SubMenu("Invisiblez").AddItem(new MenuItem("RengarR", "Rengar R").SetValue(true));
                }


            }


            //HARASS
            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("RushQHarass", "Use Q", true).SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("harassmana", "Haras Mana Percent").SetValue(new Slider(30, 0, 100)));
            //MISC
            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddSubMenu(new Menu("Scrying Orb Settings", "orbset"));
            Config.SubMenu("Misc").SubMenu("orbset").AddItem(new MenuItem("bT", "Auto Scrying Orb Buy!").SetValue(true));
            Config.SubMenu("Misc").SubMenu("orbset").AddItem(new MenuItem("bluetrinketlevel", "Scrying Orb Buy Level").SetValue(new Slider(6, 0, 18)));
          
            Config.SubMenu("Misc").AddSubMenu(new Menu("Killsteal Settings", "killset"));
            Config.SubMenu("Misc").SubMenu("killset").AddItem(new MenuItem("ksQ", "KillSteal Q!").SetValue(true));
            Config.SubMenu("Misc").SubMenu("killset").AddItem(new MenuItem("ksR", "KillSteal R!").SetValue(true));

            Config.SubMenu("Misc").AddSubMenu(new Menu("Auto Cast Inmobile Settings", "inset"));
            Config.SubMenu("Misc").SubMenu("inset").AddItem(new MenuItem("AinQ", "Auto Q Inmobile Target!").SetValue(true));
            Config.SubMenu("Misc").SubMenu("inset").AddItem(new MenuItem("AinW", "Auto W Inmobile Target!").SetValue(true));

            Config.SubMenu("Misc").AddItem(new MenuItem("agapcloser", "Anti-Gapcloser Active!", true).SetValue(true));
           

            //DRAWINGS
            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RushQRange", "Q Range").SetValue(new Circle(true, Color.SkyBlue)));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RushWRange", "W Range").SetValue(new Circle(true, Color.Yellow)));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RushERange", "E Range").SetValue(new Circle(true, Color.SpringGreen)));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RushRRange", "R Damage Text").SetValue(new Circle(true, Color.Crimson)));
           





            Config.AddToMainMenu();
            
            Game.OnUpdate += Game_OnGameUpdate;
            Obj_AI_Base.OnCreate += Obj_AI_Base_OnCreate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;
           

        }
        private static float ComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;
           
            if (R.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);

            return (float)damage;
        }
        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Config.Item("agapcloser").GetValue<bool>() || Player.IsDead)
                return;

            if (E.CanCast(gapcloser.Sender))
                E.Cast(gapcloser.Sender);
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
            if (Config.Item("RushRCombo").GetValue<bool>() && R.IsReady())
            {
                if (menuItem4.Active)

                    foreach (
                    var enemyVisible in
                        ObjectManager.Get<Obj_AI_Hero>().Where(enemyVisible => enemyVisible.IsValidTarget()))

                        if (ComboDamage(enemyVisible) > enemyVisible.Health)
                        {
                            Drawing.DrawText(Drawing.WorldToScreen(enemyVisible.Position)[0] + 50,
                                Drawing.WorldToScreen(enemyVisible.Position)[1] - 40, Color.Red,
                                "R = Kill");
                        }
            }
           
        }
        
        private static void Game_OnGameUpdate(EventArgs args)
        {
            Orbwalker.SetAttack(true);



            //COMBO
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                Combo();


                if (Config.Item("AinQ").GetValue<bool>())
                {
                    foreach (Obj_AI_Hero target in HeroManager.Enemies.Where(x => x.IsValidTarget(W.Range)))
                    {
                        if (target != null)
                        {
                            if (Q.CanCast(target) && UnitIsImmobileUntil(target) >= Q.Delay - 0.5)
                                Q.Cast(target);
                        }
                    }
                }
                if (Config.Item("AinW").GetValue<bool>())
                {
                    foreach (Obj_AI_Hero target in HeroManager.Enemies.Where(x => x.IsValidTarget(W.Range)))
                    {
                        if (target != null)
                        {
                            if (W.CanCast(target) && UnitIsImmobileUntil(target) >= W.Delay - 0.5)
                                W.Cast(target);
                        }
                    }
                }
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                Harass();
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
               Clear();
            }
            if (Config.Item("ksR").GetValue<bool>())
            {
                Killsteal();
            }

            if (Config.Item("bT").GetValue<bool>() && Player.Level >= Config.Item("bluetrinketlevel").GetValue<Slider>().Value && Player.InShop() && !(Items.HasItem(3342) || Items.HasItem(3363)))
            {
                Player.BuyItem(ItemId.Scrying_Orb_Trinket);
            }
        }
        static Obj_AI_Base W_GetBestTarget()
        {
            return HeroManager.Enemies.Where(x => W.CanCast(x) && !x.HasBuffOfType(BuffType.SpellImmunity) && W.GetPrediction(x).Hitchance >= HitChance.High && !x.IsFacing(Player) && x.IsValidTarget(550)).OrderBy(x => x.Distance(Player, false)).FirstOrDefault();
        }
        static Obj_AI_Base W_GetBestTargetAP()
        {
            return HeroManager.Enemies.Where(x => W.CanCast(x) && !x.HasBuffOfType(BuffType.SpellImmunity) && W.GetPrediction(x).Hitchance >= HitChance.Medium && !x.IsFacing(Player) && x.IsValidTarget(550)).OrderBy(x => x.Distance(Player, false)).FirstOrDefault();
        }
        static float Get_Ult_Dmg(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (R.IsReady())
                damage += R.GetDamage(enemy);

            return damage;
        }
        static bool CollisionCheck(Obj_AI_Hero source, Obj_AI_Hero target, float width)
        {
            var input = new PredictionInput
            {
                Radius = width,
                Unit = source,
            };

            input.CollisionObjects[0] = CollisionableObjects.Heroes;
            input.CollisionObjects[1] = CollisionableObjects.YasuoWall;

            return !Collision.GetCollision(new List<Vector3> { target.ServerPosition }, input).Where(x => x.NetworkId != x.NetworkId).Any();
        }
        
        private static void Combo()
        {
            float uRangeCheck = ObjectManager.Player.CountEnemiesInRange(E.Range);
            
            switch (Config.Item("combotype").GetValue<StringList>().SelectedIndex)
            {
                case 0:

                    if (E.IsReady() && Config.Item("RushECombo").GetValue<bool>())
                    {
                        var targetE = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical, true);
                        if (E.CanCast(targetE) && E.GetPrediction(targetE).Hitchance >= HitChance.VeryHigh)
                            E.Cast(targetE);
                    }

                     if (Q.IsReady() && Config.Item("RushQCombo").GetValue<bool>())
                     {
                     foreach (var En in HeroManager.Enemies.Where(hero => hero.IsValidTarget(Q.Range)))
                     {
                     var targetQ = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical, true);
                     if (Q.CanCast(targetQ) && Q.GetPrediction(targetQ).Hitchance >= HitChance.VeryHigh)
                      Q.Cast(targetQ);
                     }
                     }
           
          if (W.IsReady() && Config.Item("RushWCombo").GetValue<bool>())
          {
               foreach (var En in HeroManager.Enemies.Where(hero => hero.IsValidTarget(W.Range)))
              {
              var targetW = W_GetBestTarget();

              if (W.CanCast(targetW))
                  W.Cast(targetW);
              }
          }

            
         
          if (R.IsReady() && Config.Item("RushRCombo").GetValue<bool>())
          {
              foreach (var Rtarget in HeroManager.Enemies.Where(t => t.IsValidTarget(1500 + (500 * R.Level)) && !t.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) && !t.HasBuffOfType(BuffType.Invulnerability) && !t.HasBuffOfType(BuffType.SpellShield)))
              {
                  if (Rtarget.Health + Rtarget.HPRegenRate <= R.GetDamage(Rtarget) && CollisionCheck(Player, Rtarget, 150))
                      R.Cast(Rtarget);
              }
                  
              
          }

                    break;
                case 1:

                    if (E.IsReady() && Config.Item("RushECombo").GetValue<bool>())
                    {
                        
                            var targetE = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical, true);

                            if (E.CanCast(targetE) && E.GetPrediction(targetE).Hitchance >= HitChance.VeryHigh)
                                E.Cast(targetE);

                    }
                    if (W.IsReady() && Config.Item("RushWCombo").GetValue<bool>())
                    {
                        foreach (var En in HeroManager.Enemies.Where(hero => hero.IsValidTarget(W.Range)))
                        {
                           

                            var targetW = W_GetBestTargetAP();

                            if (W.CanCast(targetW))
                                W.Cast(targetW);
                        }
                    }
                    if (Q.IsReady() && Config.Item("RushQCombo").GetValue<bool>())
                    {
                        foreach (var En in HeroManager.Enemies.Where(hero => hero.IsValidTarget(Q.Range)))
                        {
                            var targetQ = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical, true);

                            if (Q.CanCast(targetQ) && Q.GetPrediction(targetQ).Hitchance >= HitChance.VeryHigh)
                                Q.Cast(targetQ);
                        }

                    }

                    if (R.IsReady() && Config.Item("RushRCombo").GetValue<bool>())
                    {
                        foreach (var Rtarget in HeroManager.Enemies.Where(t => t.IsValidTarget(1500 + (500 * R.Level)) && !t.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) && !t.HasBuffOfType(BuffType.Invulnerability) && !t.HasBuffOfType(BuffType.SpellShield)))
                        {
                            if (Rtarget.Health + Rtarget.HPRegenRate <= R.GetDamage(Rtarget) && CollisionCheck(Player, Rtarget, 150))
                                R.Cast(Rtarget);
                        }
                    }

                    break;
            } 
              
         
            




            


            

        }
        static double UnitIsImmobileUntil(Obj_AI_Base unit)
        {
            var result =
                unit.Buffs.Where(
                    buff =>
                        buff.IsActive && Game.Time <= buff.EndTime &&
                        (buff.Type == BuffType.Charm || buff.Type == BuffType.Knockup || buff.Type == BuffType.Stun ||
                         buff.Type == BuffType.Suppression || buff.Type == BuffType.Snare))
                    .Aggregate(0d, (current, buff) => Math.Max(current, buff.EndTime));
            return (result - Game.Time);
        }
        private static void Harass()
        {
            if (Q.IsReady() && Config.Item("RushQCombo").GetValue<bool>() && Player.ManaPercent >= Config.Item("harassmana").GetValue<Slider>().Value)
            {
                 foreach (var En in HeroManager.Enemies.Where(hero => hero.IsValidTarget(Q.Range)))
                {
                var targetQ = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical, true);

                if (Q.CanCast(targetQ) && Q.GetPrediction(targetQ).Hitchance >= HitChance.VeryHigh)
                    Q.Cast(targetQ);
                }
            }
        }

        private static void Clear()
        {
           
        }

        private static void Killsteal()
        {
            if (Config.Item("ksQ").GetValue<bool>())
            {
                
                    foreach (var targetksQ in HeroManager.Enemies.Where(t => t.IsValidTarget(Q.Range) && !t.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player))))
                    {
                        if (targetksQ.Health + targetksQ.HPRegenRate <= R.GetDamage(targetksQ))
                            Q.Cast(targetksQ);
                    }
            }
            if (Config.Item("ksR").GetValue<bool>())
            {
                foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
                {
                    if (R.CanCast(target) && R.IsKillable(target))
                        R.CastOnUnit(target);
                }
            }
           
        }

       

        private static void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            if (!Config.Item("Use").GetValue<KeyBind>().Active)
                return;

            var Rengar = HeroManager.Enemies.Find(x => x.ChampionName.ToLower() == "rengar");

            if (Rengar == null)
                return;

            if (!Config.Item("RengarR").GetValue<bool>())
                return;

            if (ObjectManager.Player.Distance(sender.Position) < 1500)
            {
                Console.WriteLine("Sender : " + sender.Name);
            }

            if (sender.IsEnemy && sender.Name.Contains("Rengar_Base_R_Alert"))
            {
                if (ObjectManager.Player.HasBuff("rengarralertsound") &&
                !CheckWard() &&
                !Rengar.IsVisible &&
                !Rengar.IsDead &&
                    CheckSlot() != SpellSlot.Unknown)
                {
                    ObjectManager.Player.Spellbook.CastSpell(CheckSlot(), ObjectManager.Player.Position);
                }
            }
        }
        static SpellSlot CheckSlot()
        {
            SpellSlot slot = SpellSlot.Unknown;

            if (Items.CanUseItem(3362) && Items.HasItem(3362, ObjectManager.Player))
            {
                slot = SpellSlot.Trinket;
            }
            else if (Items.CanUseItem(2043) && Items.HasItem(2043, ObjectManager.Player))
            {
                slot = ObjectManager.Player.GetSpellSlot("VisionWard");
            }
            return slot;
        }

        static bool CheckWard()
        {
            var status = false;

            foreach (var a in ObjectManager.Get<Obj_AI_Minion>().Where(x => x.Name == "VisionWard"))
            {
                if (ObjectManager.Player.Distance(a.Position) < 450)
                {
                    status = true;
                }
            }

            return status;
        }

    }
}
