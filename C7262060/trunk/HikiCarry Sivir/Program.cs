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

namespace HikiCarry_Sivir
{
    class Program
    {
        public const string ChampionName = "Sivir";
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


            Q = new Spell(SpellSlot.Q, 1245f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 1000f);

            Q.SetSkillshot(0.25f, 90f, 1350f, false, SkillshotType.SkillshotLine);

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

            SpellListt.Add(new Spells { ChampionName = "Graves", SpellName = "GravesClusterShot", slot = SpellSlot.Q });   //Akali Graves

            //MENU
            Config = new Menu("HikiCarry - Sivir", "HikiCarry - Sivir", true);

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
            Config.SubMenu("Combo").AddItem(new MenuItem("RushRCombo", "Use R").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("RcomboEnemy", "R on x Enemy").SetValue(new Slider(3, 1, 5)));
            Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            //LANECLEAR
            Config.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("RushQClear", "Use Q").SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("RushWClear", "Use W").SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("clearmana", "Clear Mana Percent").SetValue(new Slider(30, 0, 100)));

            //HARASS
            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("RushQHarass", "Use Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("harassmana", "Haras Mana Percent").SetValue(new Slider(30, 0, 100)));
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


           
            //MISC
            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("useE", "Dodge with E").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("ksQ", "KillSteal Q!").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("AinQ", "Auto Q Inmobile Target!").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("bT", "Auto Buy Scrying Orb!").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("bluetrinketlevel", "Scrying Orb Buy Level").SetValue(new Slider(6, 0, 18)));

            //DRAWINGS
            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RushQRange", "Q Range").SetValue(new Circle(true, Color.SkyBlue)));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RushWRange", "W Range").SetValue(new Circle(true, Color.Yellow)));
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
            Obj_AI_Base.OnCreate += Obj_AI_Base_OnCreate;
            Drawing.OnDraw += Drawing_OnDraw;
           




        }

       
        static float GetComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage(enemy);

            return damage;
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

        private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Config.Item("useE").GetValue<bool>())
            {
                string[] Spells = {"AhriSeduce"
                                          , "InfernalGuardian"
                                          , "EnchantedCrystalArrow"
                                          , "InfernalGuardian"
                                          , "EnchantedCrystalArrow"
                                          , "RocketGrab"
                                          , "BraumQ"
                                          , "CassiopeiaPetrifyingGaze"
                                          , "DariusAxeGrabCone"
                                          , "DravenDoubleShot"
                                          , "DravenRCast"
                                          , "EzrealTrueshotBarrage"
                                          , "FizzMarinerDoom"
                                          , "GnarBigW"
                                          , "GnarR"
                                          , "GragasR"
                                          , "GravesChargeShot"
                                          , "GravesClusterShot"
                                          , "JarvanIVDemacianStandard"
                                          , "JinxW"
                                          , "JinxR"
                                          , "KarmaQ"
                                          , "KogMawLivingArtillery"
                                          , "LeblancSlide"
                                          , "LeblancSoulShackle"
                                          , "LeonaSolarFlare"
                                          , "LuxLightBinding"
                                          , "LuxLightStrikeKugel"
                                          , "LuxMaliceCannon"
                                          , "UFSlash"
                                          , "DarkBindingMissile"
                                          , "NamiQ"
                                          , "NamiR"
                                          , "OrianaDetonateCommand"
                                          , "RengarE"
                                          , "rivenizunablade"
                                          , "RumbleCarpetBombM"
                                          , "SejuaniGlacialPrisonStart"
                                          , "SionR"
                                          , "ShenShadowDash"
                                          , "SonaR"
                                          , "ThreshQ"
                                          , "ThreshEFlay"
                                          , "VarusQMissilee"
                                          , "VarusR"
                                          , "VeigarBalefulStrike"
                                          , "VelkozQ"
                                          , "Vi-q"
                                          , "Laser"
                                          , "xeratharcanopulse2"
                                          , "XerathArcaneBarrage2"
                                          , "XerathMageSpear"
                                          , "xerathrmissilewrapper"
                                          , "yasuoq3w"
                                          , "ZacQ"
                                          , "ZedShuriken"
                                          , "ZiggsQ"
                                          , "ZiggsW"
                                          , "ZiggsE"
                                          , "ZiggsR"
                                          , "ZileanQ"
                                          , "ZyraQFissure"
                                          , "ZyraGraspingRoots"
                                      };
                for (int i = 0; i <= 61; i++)
                {
                    if (args.SData.Name == Spells[i])
                    {
                        if (sender is Obj_AI_Hero && sender.IsEnemy && args.Target.IsMe && !args.SData.IsAutoAttack() && E.IsReady())
                        {
                            E.Cast();
                        }
                    }
                }
            }
           
           
            

            if (!Config.Item("Use").GetValue<KeyBind>().Active)
                return;

            if (!sender.IsEnemy || sender.IsDead || !(sender is Obj_AI_Hero))
                return;

            if (SpellListt.Exists(x => x.SpellName.Contains(args.SData.Name.ToLower())))
            {
                var _sender = sender as Obj_AI_Hero;

                if (!Config.Item(_sender.ChampionName.ToLower() + _sender.GetSpellSlot(args.SData.Name).ToString()).GetValue<bool>())
                    return;

                if (CheckSlot() == SpellSlot.Unknown)
                    return;

                if (CheckWard())
                    return;

                if (ObjectManager.Player.Distance(sender.Position) > 700)
                    return;

                if (Environment.TickCount - Delay > 1500 || Delay == 0)
                {
                    var pos = ObjectManager.Player.Distance(args.End) > 600 ? ObjectManager.Player.Position : args.End;
                    ObjectManager.Player.Spellbook.CastSpell(CheckSlot(), pos);
                    Delay = Environment.TickCount;
                }
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
        private static void Game_OnGameUpdate(EventArgs args)
        {
            Orbwalker.SetAttack(true);



            //COMBO
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
            if (Config.Item("ksQ").GetValue<bool>())
            {
                Killsteal();
            }

            if (Config.Item("bT").GetValue<bool>() && Player.Level >= Config.Item("bluetrinketlevel").GetValue<Slider>().Value && Player.InShop() && !(Items.HasItem(3342) || Items.HasItem(3363)))
            {
                Player.BuyItem(ItemId.Scrying_Orb_Trinket);
            }

            if (Config.Item("AinQ").GetValue<bool>())
            {
                foreach (Obj_AI_Hero target in HeroManager.Enemies.Where(x => x.IsValidTarget(Q.Range)))
                {
                    if (target != null)
                    {
                        if (Q.CanCast(target) && UnitIsImmobileUntil(target) >= Q.Delay - 0.5 && Q.GetPrediction(target).Hitchance >= HitChance.VeryHigh)
                            Q.Cast(target);
                    }
                }
            }

        }

        private static void Combo()
        {
            if (Q.IsReady() && Config.Item("RushQCombo").GetValue<bool>())
            {
                foreach (
                    var en in
                        HeroManager.Enemies.Where(
                            hero =>
                                hero.IsValidTarget(Q.Range)))
                {
                    var targetQ = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                    if (Q.CanCast(targetQ))
                    {
                        Q.Cast(targetQ);
                    }
                }
            }

            if (W.IsReady() && Config.Item("RushWCombo").GetValue<bool>())
            {
                foreach (
                    var en in
                        HeroManager.Enemies.Where(
                            hero =>
                                hero.IsValidTarget(500)))
                {
                        W.Cast();
                    
                }
            }
            
            if (R.IsReady() && Config.Item("RushRCombo").GetValue<bool>() && Player.CountEnemiesInRange(1000) >= Config.Item("RcomboEnemy").GetValue<Slider>().Value)
            {
                R.Cast();
            }
            
        }
        private static void Harass()
        {
            if (Q.IsReady() && Config.Item("RushQHarass").GetValue<bool>() && Player.ManaPercent >= Config.Item("harassmana").GetValue<Slider>().Value)
            {
                foreach (
                    var en in
                        HeroManager.Enemies.Where(
                            hero =>
                                hero.IsValidTarget(Q.Range)))
                {
                    var targetQ = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                    if (Q.GetPrediction(targetQ).Hitchance >= HitChance.VeryHigh)
                    {
                        Q.Cast(targetQ);
                    }
                }
            }
        }
        private static void Clear()
        {
            //THANKS SEBBY 
            if (Q.IsReady() && Config.Item("RushQClear").GetValue<bool>() && Player.ManaPercent >= Config.Item("clearmana").GetValue<Slider>().Value)
            {
                var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All);
                var Qfarm = Q.GetCircularFarmLocation(allMinionsQ, 200);
                if (Qfarm.MinionsHit >= 3 && Q.IsReady())
                    Q.Cast(Qfarm.Position);
            }

            if (W.IsReady() && Config.Item("RushWClear").GetValue<bool>() && Player.ManaPercent >= Config.Item("clearmana").GetValue<Slider>().Value)
            {
                var allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range, MinionTypes.All);
                var Wfarm = W.GetCircularFarmLocation(allMinionsW, 200);
                if (allMinionsW.Count >= 4)
                    W.Cast();
            }
        }





        private static void Drawing_OnDraw(EventArgs args)
        {
            var menuItem2 = Config.Item("RushQRange").GetValue<Circle>();
            var menuItem3 = Config.Item("RushWRange").GetValue<Circle>();




            if (Config.Item("RushQCombo").GetValue<bool>() && Q.IsReady())
            {
                if (menuItem2.Active) Utility.DrawCircle(Player.Position, Q.Range, Color.SkyBlue);
            }

            if (Config.Item("RushWCombo").GetValue<bool>() && W.IsReady())
            {
                if (menuItem3.Active) Utility.DrawCircle(Player.Position, W.Range, Color.Yellow);
            }

        }

        private static void Killsteal()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && Q.IsKillable(target))
                    Q.Cast(target);
            }
        }



    }
}