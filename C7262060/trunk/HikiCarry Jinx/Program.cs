using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;
using LeagueSharp;
using SharpDX;
using Color = System.Drawing.Color;
using Collision = LeagueSharp.Common.Collision;

namespace HikiCarry_Jinx
{
    class Program
    {
        public const string ChampionName = "Jinx";
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> SpellList = new List<Spell>();
        public static Menu Config;
        private static Obj_AI_Hero Player = ObjectManager.Player;
        private static SpellSlot Flash;

        public static Spell Q;
        public static Spell Qx2;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        static bool hCannon()
        {
            return Player.AttackRange > 525;
        }

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.BaseSkinName != ChampionName) return;


            Q = new Spell(SpellSlot.Q);
            Qx2 = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 1450f); 
            E = new Spell(SpellSlot.E, 900f);
            R = new Spell(SpellSlot.R, 2500f); 

            W.SetSkillshot(0.6f, 60f, 3300f, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(1.2f, 1f, 1750f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.5f, 140f, 1700f, false, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(Qx2);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            Config = new Menu("HikiCarry - Jinx", "HikiCarry - Jinx", true);

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Config.AddSubMenu(new Menu("Orbwalker Settings", "Orbwalker Settings"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker Settings"));

            //COMBO
            Config.AddSubMenu(new Menu("Combo Settings", "Combo Settings"));
            Config.SubMenu("Combo Settings").AddItem(new MenuItem("qCombo", "Use Q").SetValue(true)); 
            Config.SubMenu("Combo Settings").AddItem(new MenuItem("wCombo", "Use W").SetValue(true));
            Config.SubMenu("Combo Settings").AddItem(new MenuItem("eCombo", "Use E").SetValue(true));
            Config.SubMenu("Combo Settings").AddItem(new MenuItem("rCombo", "Use R").SetValue(true));
            Config.SubMenu("Combo Settings").AddItem(new MenuItem("maxR", "Max R Distance").SetValue(new Slider(1500, 0, 3000)));

            //HARASS
            Config.AddSubMenu(new Menu("Harass Settings", "Harass Settings"));
            Config.SubMenu("Harass Settings").AddItem(new MenuItem("qHarass", "Use Q").SetValue(true));
            Config.SubMenu("Harass Settings").AddItem(new MenuItem("wHarass", "Use W").SetValue(true));
            Config.SubMenu("Harass Settings").AddItem(new MenuItem("harassmana", "Haras Mana Percent").SetValue(new Slider(30, 0, 100)));

            //LANECLEAR
            Config.AddSubMenu(new Menu("LaneClear Settings", "LaneClear Settings"));
            Config.SubMenu("LaneClear Settings").AddItem(new MenuItem("qClear", "Use Q").SetValue(true));
            Config.SubMenu("Harass Settings").AddItem(new MenuItem("qCount", "Q Minion Hit").SetValue(new Slider(3, 1, 5)));
            Config.SubMenu("LaneClear Settings").AddItem(new MenuItem("clearmana", "Clear Mana Percent", true).SetValue(new Slider(2, 1, 5)));


            Config.AddSubMenu(new Menu("Item Settings", "Item Settings"));
            Config.SubMenu("Item Settings").AddSubMenu(new Menu("BOTRK Settings", "BOTRKset"));
            Config.SubMenu("Item Settings").SubMenu("BOTRKset").AddItem(new MenuItem("useBOTRK", "Use BOTRK").SetValue(true));
            Config.SubMenu("Item Settings").SubMenu("BOTRKset").AddItem(new MenuItem("myhp", "Use if my HP < %").SetValue(new Slider(20, 0, 100)));
            Config.SubMenu("Item Settings").SubMenu("BOTRKset").AddItem(new MenuItem("theirhp", "Use if enemy HP < %").SetValue(new Slider(20, 0, 100)));
            Config.SubMenu("Item Settings").AddSubMenu(new Menu("Ghostblade Settings", "GBladeset"));
            Config.SubMenu("Item Settings").SubMenu("GBladeset").AddItem(new MenuItem("useGBLADE", "Use Ghostblade").SetValue(true));

            //MISC
            Config.AddSubMenu(new Menu("Miscellaneous", "Miscellaneous"));
            Config.SubMenu("Miscellaneous").AddItem(new MenuItem("inmobileE", "Auto Inmobile Target E").SetValue(true));
            Config.SubMenu("Miscellaneous").AddItem(new MenuItem("antigapcloser", "Anti-Gapcloser with E").SetValue(true));
            Config.SubMenu("Miscellaneous").AddSubMenu(new Menu("Scrying Orb Settings", "orbset"));
            Config.SubMenu("Miscellaneous").SubMenu("orbset").AddItem(new MenuItem("bT", "Auto Scrying Orb Buy!").SetValue(true));
            Config.SubMenu("Miscellaneous").SubMenu("orbset").AddItem(new MenuItem("bluetrinketlevel", "Scrying Orb Buy Level").SetValue(new Slider(6, 0, 18)));
            Config.SubMenu("Miscellaneous").AddSubMenu(new Menu("KillSteal Settings", "killsetset"));
            Config.SubMenu("Miscellaneous").SubMenu("killsetset").AddItem(new MenuItem("ksW", "KillSteal W!").SetValue(true));

            //DRAWINGS
            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("qRange", "Q Range").SetValue(new Circle(true, Color.SkyBlue)));
            Config.SubMenu("Drawings").AddItem(new MenuItem("wRange", "W Range").SetValue(new Circle(true, Color.Yellow)));
            Config.SubMenu("Drawings").AddItem(new MenuItem("eRange", "E Range").SetValue(new Circle(true, Color.SpringGreen)));
            Config.SubMenu("Drawings").AddItem(new MenuItem("rRange", "R Range").SetValue(new Circle(true, Color.Crimson)));
            

            var drawDamageMenu = new MenuItem("RushDrawEDamage", "R Damage").SetValue(true);
            var drawFill = new MenuItem("RushDrawEDamageFill", "R Damage Fill").SetValue(new Circle(true, Color.SeaGreen));
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
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Config.Item("inmobileE").GetValue<bool>())
            {
                if (gapcloser.Sender.IsValidTarget(E.Range))
                {
                    Render.Circle.DrawCircle(gapcloser.Sender.Position, gapcloser.Sender.BoundingRadius, Color.Gold, 5);
                    var targetpos = Drawing.WorldToScreen(gapcloser.Sender.Position);
                    Drawing.DrawText(targetpos[0] - 40, targetpos[1] + 20, Color.Gold, "Gapcloser");
                }

                if (E.IsReady() && Player.Distance(gapcloser.End, false) <= 200)
                    E.Cast(gapcloser.End);
            }
           
        }
        private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var tar = (Obj_AI_Hero)target;
            if (Config.Item("useBOTRK").GetValue<bool>() && ((tar.Health / tar.MaxHealth) < Config.Item("theirhp").GetValue<Slider>().Value) && ((ObjectManager.Player.Health / ObjectManager.Player.MaxHealth) < Config.Item("myhp").GetValue<Slider>().Value))
            {
                if (Items.CanUseItem(3153))
                {
                    Items.UseItem(3153, tar);
                }
            }
            if (Config.Item("useGBLADE").GetValue<bool>())
            {
                if (Items.CanUseItem(3142))
                {
                    Items.UseItem(3142);
                }
            }
        }
        static float GetComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (R.IsReady())
                damage += R.GetDamage(enemy);

            if (damage > enemy.Health && R.IsReady())
            {
                var yx = Drawing.WorldToScreen(enemy.Position);
                Drawing.DrawText(yx[0], yx[1], System.Drawing.Color.SpringGreen, "ULT = KILL");
            }
            

            return damage;
        }
        public static bool CanMove(Obj_AI_Hero target)
        {
            if (target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Knockup) ||
                target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Fear) || target.HasBuffOfType(BuffType.Knockback) ||
                target.HasBuffOfType(BuffType.Taunt) || target.HasBuffOfType(BuffType.Suppression) ||
                target.IsStunned || target.IsChannelingImportantSpell())
            {
                return false;
            }
            else
                return true;
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
                LaneClear();
            }
            KillSteal();
            BlueOrb();
            instaEImmobile();
        }
        private static void LaneClear()
        {
            if (ObjectManager.Player.ManaPercent > Config.Item("clearmana").GetValue<Slider>().Value)
            {
                var minionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Enemy);
                if (Q.IsReady() && Config.Item("qClear").GetValue<bool>())
                {
                    foreach (var minyon in minionQ)
                    {
                        float dZ = Player.Position.Distance(minyon.Position);
                        if (hCannon())
                        {
                            if (dZ <= 600 && minionQ.Count >= Config.Item("qCount").GetValue<Slider>().Value)
                            {
                                Q.Cast();
                            }
                        }
                    }
                }
            }
            
        }
        private static void instaEImmobile()
        {
            if (Config.Item("inmobileE").GetValue<bool>() && E.IsReady())
            {
                foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(E.Range) && !CanMove(enemy)))
                {
                    E.Cast(enemy.Position, true);
                    return;

                }
            }
        }
        private static void BlueOrb()
        {
            if (Config.Item("bT").GetValue<bool>() && Player.Level >= Config.Item("bluetrinketlevel").GetValue<Slider>().Value && Player.InShop() && !(Items.HasItem(3342) || Items.HasItem(3363)))
            {
                Player.BuyItem(ItemId.Scrying_Orb_Trinket);
            }
        }
        private static void KillSteal()
        {
            if (Config.Item("ksW").GetValue<bool>())
            {
                foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
                {
                    if (W.CanCast(target) && W.IsKillable(target))
                        W.Cast(target);
                }
            }
        }
        private static void Combo()
        {
            if (Q.IsReady() && Config.Item("qCombo").GetValue<bool>())
            {
                foreach (var enemy in HeroManager.Enemies.Where(hero => hero.IsValidTarget(Q.Range)))
                {
                    float dZ = Player.Position.Distance(enemy.Position);
                    if (hCannon())
                    {
                        if (dZ <= 600)
                        {
                            Q.Cast();
                        }
                    }
                    else
                    {
                        if (dZ > 600)
                        {
                            Q.Cast();
                        }
                    }
                }
            }

            if (W.IsReady() && Config.Item("wCombo").GetValue<bool>())
            {
                foreach (var enemyW in HeroManager.Enemies.Where(hero => hero.IsValidTarget(W.Range)))
                {
                    if (W.GetPrediction(enemyW).Hitchance >= HitChance.Medium && !Player.IsWindingUp && !Player.IsDashing())
                    {
                        W.Cast(enemyW);
                    }
                }
            }
            if (E.IsReady() && Config.Item("eCombo").GetValue<bool>())
            {
                foreach (var enemyE in HeroManager.Enemies.Where(hero => hero.IsValidTarget(E.Range)))
                {
                    if (!CanMove(enemyE))
                    {
                       E.Cast(enemyE.Position, true);
                    }
                }
            }
            if (R.IsReady() && Config.Item("rCombo").GetValue<bool>())
            {
                var rDistance = Config.Item("maxR").GetValue<Slider>().Value;
                foreach (var enemyR in HeroManager.Enemies.Where(hero => CollisionCheckerino(Player, hero, R.Width) && hero.IsValidTarget(rDistance)))
                {
                    if (!enemyR.IsZombie && enemyR.Health < R.GetDamage(enemyR))
                    {
                        R.Cast(enemyR);
                    }
                }
            }
 
        }
        private static void Harass()
        {
            if (ObjectManager.Player.ManaPercent > Config.Item("harassmana").GetValue<Slider>().Value)
            {
                if (Q.IsReady() && Config.Item("qHarass").GetValue<bool>())
                {
                    foreach (var enemy in HeroManager.Enemies.Where(hero => hero.IsValidTarget(Q.Range)))
                    {
                        float dZ = Player.Position.Distance(enemy.Position);
                        if (hCannon())
                        {
                            if (dZ <= 600)
                            {
                                Q.Cast();
                            }
                        }
                        else
                        {
                            if (dZ > 600)
                            {
                                Q.Cast();
                            }
                        }
                    }
                }
                if (W.IsReady() && Config.Item("wHarass").GetValue<bool>())
                {
                    foreach (var enemyW in HeroManager.Enemies.Where(hero => hero.IsValidTarget(W.Range)))
                    {
                        if (W.GetPrediction(enemyW).Hitchance >= HitChance.Medium && !Player.IsWindingUp && !Player.IsDashing())
                        {
                            W.Cast(enemyW);
                        }
                    }
                }
            }
        }
        static bool CollisionCheckerino(Obj_AI_Hero source, Obj_AI_Hero target, float width)
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
        private static void Drawing_OnDraw(EventArgs args)
        {
            var menuItem1 = Config.Item("qRange").GetValue<Circle>();
            var menuItem2 = Config.Item("wRange").GetValue<Circle>();
            var menuItem3 = Config.Item("eRange").GetValue<Circle>();
            var menuItem4 = Config.Item("rRange").GetValue<Circle>();

            if (menuItem1.Active)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.SkyBlue);
            }
            if (menuItem2.Active)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.Yellow);
            }
            if (menuItem3.Active)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.SpringGreen);
            }
            if (menuItem4.Active)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.Crimson);
            }
        }
    }
}