using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace HikiCarry_Graves_Reborn
{
    class Program
    {
        public const string ChampionName = "Graves";
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> SpellList = new List<Spell>();
        public static Menu Config;
        private static Obj_AI_Hero Player = ObjectManager.Player;
        private static SpellSlot Flash;
        public static int qRange = 0;

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != ChampionName) return;


            Q = new Spell(SpellSlot.Q, 720f);
            W = new Spell(SpellSlot.W, 850f);
            E = new Spell(SpellSlot.E, 425f);
            R = new Spell(SpellSlot.R, 1100f);

            Q.SetSkillshot(0.25f, 15f * (float)Math.PI / 180, 2000f, false, SkillshotType.SkillshotCone);
            W.SetSkillshot(0.25f, 250f, 1650f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.25f, 100f, 2100f, false, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            Config = new Menu("HikiCarry - Graves", "HikiCarry - Graves", true);

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Config.AddSubMenu(new Menu("Orbwalker Settings", "Orbwalker Settings"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker Settings"));

            Config.AddSubMenu(new Menu("Combo Settings", "Combo Settings"));
            Config.SubMenu("Combo Settings").AddItem(new MenuItem("qCombo", "Use Q").SetValue(true));
            Config.SubMenu("Combo Settings").AddItem(new MenuItem("wCombo", "Use W").SetValue(true));
            Config.SubMenu("Combo Settings").AddItem(new MenuItem("eCombo", "Use E").SetValue(true));
            Config.SubMenu("Combo Settings").AddItem(new MenuItem("rCombo", "Use R").SetValue(true));

            Config.AddSubMenu(new Menu("Harass Settings", "Harass Settings"));
            Config.SubMenu("Harass Settings").AddItem(new MenuItem("qHarass", "Use Q").SetValue(true));
            Config.SubMenu("Harass Settings").AddItem(new MenuItem("harassMana", "Harass Mana Percent").SetValue(new Slider(30, 1, 100)));

            Config.AddSubMenu(new Menu("LaneClear Settings", "LaneClear Settings"));
            Config.SubMenu("LaneClear Settings").AddItem(new MenuItem("qClear", "Use Q").SetValue(true));
            Config.SubMenu("LaneClear Settings").AddItem(new MenuItem("qMinions", "Q Minions Hit >=").SetValue(new Slider(3, 1, 5)));
            Config.SubMenu("LaneClear Settings").AddItem(new MenuItem("clearMana", "Harass Mana Percent").SetValue(new Slider(30, 1, 100)));

            Config.AddSubMenu(new Menu("Misc Settings", "Misc Settings"));
            Config.SubMenu("Misc Settings").AddSubMenu(new Menu("Scrying Orb Settings", "Scrying Orb Settings"));
            Config.SubMenu("Misc Settings").SubMenu("Scrying Orb Settings").AddItem(new MenuItem("bT", "Auto Scrying Orb Buy!").SetValue(true));
            Config.SubMenu("Misc Settings").SubMenu("Scrying Orb Settings").AddItem(new MenuItem("bluetrinketlevel", "Scrying Orb Buy Level").SetValue(new Slider(6, 0, 18)));
            Config.SubMenu("Misc Settings").AddItem(new MenuItem("agapcloser", "Anti-Gapcloser Active!").SetValue(true));
            Config.SubMenu("Misc Settings").AddItem(new MenuItem("ainterrupt", "Auto Interrupt Active!").SetValue(true));

            Config.AddSubMenu(new Menu("Items Settings", "Items Settings"));
            Config.SubMenu("Items Settings").AddSubMenu(new Menu("BOTRK Settings", "BOTRK Settings"));
            Config.SubMenu("Items Settings").SubMenu("BOTRK Settings").AddItem(new MenuItem("useBOTRK", "USE BOTRK").SetValue(true));
            Config.SubMenu("Items Settings").SubMenu("BOTRK Settings").AddItem(new MenuItem("myhp", "USE IF MY HP < %").SetValue(new Slider(20, 0, 100)));
            Config.SubMenu("Items Settings").SubMenu("BOTRK Settings").AddItem(new MenuItem("theirhp", "USE IF ENEMY HP < %").SetValue(new Slider(20, 0, 100)));
            Config.SubMenu("Items Settings").AddItem(new MenuItem("ghostblade", "USE GHOSTBLADE").SetValue(true));

            Config.AddSubMenu(new Menu("Immobile Target Settings", "Immobile Target Settings"));
            Config.SubMenu("Immobile Target Settings").AddItem(new MenuItem("iActive", "Inmobile Target Auto Cast Skill").SetValue(true));
            Config.SubMenu("Immobile Target Settings").AddItem(new MenuItem("qInmobile", "Use Q").SetValue(true));
            Config.SubMenu("Immobile Target Settings").AddItem(new MenuItem("wInmobile", "Use W").SetValue(true));

            Config.AddSubMenu(new Menu("Draw Settings", "Draw Settings"));
            Config.SubMenu("Draw Settings").AddItem(new MenuItem("qDraw", "Q Range").SetValue(new Circle(true, Color.SpringGreen)));
            Config.SubMenu("Draw Settings").AddItem(new MenuItem("wDraw", "W Range").SetValue(new Circle(true, Color.Yellow)));
            Config.SubMenu("Draw Settings").AddItem(new MenuItem("eDraw", "E Range").SetValue(new Circle(true, Color.Green)));
            Config.SubMenu("Draw Settings").AddItem(new MenuItem("rDraw", "R Range").SetValue(new Circle(true, Color.SkyBlue)));
            Config.SubMenu("Draw Settings").AddItem(new MenuItem("aaRangeDraw", "AA Range").SetValue(new Circle(true, Color.White)));
            

            Config.AddItem(new MenuItem("imgodandiknow", "                HikiCarry Graves Settings"));
            Config.AddItem(new MenuItem("qRange",  "[Q] Range").SetValue(new Slider(500, 1, 720)));
            Config.AddItem(new MenuItem("eLogic",  "[E] Logic").SetValue(new StringList(new[] { "Cursor Position" })));

            var drawDamageMenu = new MenuItem("rDamage", "R Damage").SetValue(true);
            var drawFill = new MenuItem("rDamageFill", "R Damage Fill").SetValue(new Circle(true, Color.Yellow));
            Config.SubMenu("Draw Settings").AddItem(drawDamageMenu);
            Config.SubMenu("Draw Settings").AddItem(drawFill);
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
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;
        }
        private static float GetComboDamage(Obj_AI_Hero hero)
        {
            float damage = 0;

            if (R.IsReady())
                damage += R.GetDamage(hero);


            if (damage > hero.Health && R.IsReady())
            {
                var yx = Drawing.WorldToScreen(hero.Position);
                Drawing.DrawText(yx[0], yx[1], System.Drawing.Color.Yellow, "ULT = KILL");
            }
            return damage;
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
                Jungle();
            }
            if (Config.Item("bT").GetValue<bool>())
            {
                BlueOrb();
            }
            if (Config.Item("iActive").GetValue<bool>())
            {
                immobileTarget();
            }
            
        }
        private static void immobileTarget()
        {           
            var useQ   = Config.Item("qInmobile").GetValue<bool>();
            var useW   = Config.Item("wInmobile").GetValue<bool>();
            if (Q.IsReady() && useQ)
            {
                foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range) && !CanMove(enemy)))
                {
                    Q.Cast(enemy.Position, true);
                    return;

                }
            }
            if (W.IsReady() && useW)
            {
                foreach (var enemy in HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(W.Range) && !CanMove(enemy)))
                {
                    W.Cast(enemy.Position, true);
                    return;

                }
            }
        }
        private static void BlueOrb()
        {
            if (Config.Item("bT").GetValue<bool>())
            {
                if (Player.Level >= Config.Item("bluetrinketlevel").GetValue<Slider>().Value && Player.InShop() && !(Items.HasItem(3342) || Items.HasItem(3363)))
                {
                    Player.BuyItem(ItemId.Scrying_Orb_Trinket);
                }
            }
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
        private static void Combo()
        {
            qRange     = Config.Item("qRange").GetValue<Slider>().Value;
            var useQ   = Config.Item("qCombo").GetValue<bool>();
            var useW   = Config.Item("wCombo").GetValue<bool>();
            var useE   = Config.Item("eCombo").GetValue<bool>();
            var useR   = Config.Item("rCombo").GetValue<bool>();

            if (Q.IsReady() && useQ)
            {
                foreach (
                   var en in
                       HeroManager.Enemies.Where(
                           hero =>
                               hero.IsValidTarget(qRange)))
                {
                    if (Q.CanCast(en) && Q.GetPrediction(en).Hitchance >= HitChance.High && !Player.IsWindingUp && !Player.IsDashing())

                        Q.Cast(en);
                }
            }
            if (W.IsReady() && useW)
            {
                foreach (
                   var en in
                       HeroManager.Enemies.Where(
                           hero =>
                               hero.IsValidTarget(W.Range)))
                {
                    if (W.CanCast(en) && W.GetPrediction(en).Hitchance >= HitChance.High && !Player.IsWindingUp && !Player.IsDashing())

                        W.Cast(en);
                }
            }
            if (E.IsReady() && useE)
            {
                foreach (
                   var en in
                       HeroManager.Enemies.Where(
                           hero =>
                               hero.IsValidTarget(Q.Range)))
                {
                    E.Cast(Game.CursorPos);
                }
            }
            if (R.IsReady() && useR)
            {
                foreach (var hero in
                    ObjectManager.Get<Obj_AI_Hero>().Where(
                    hero => hero.IsValidTarget(R.Range) 
                        && ObjectManager.Player.GetSpellDamage(hero, SpellSlot.R, 1) - 20 > hero.Health))
                {
                    if (R.CanCast(hero) && R.GetPrediction(hero).Hitchance >= HitChance.High)
                    {
                        R.Cast(hero);
                    }   
                }
            }
        }
        private static void Harass()
        {
            qRange = Config.Item("qRange").GetValue<Slider>().Value;
            var useQ = Config.Item("qHarass").GetValue<bool>();
            if (ObjectManager.Player.ManaPercent > Config.Item("harassMana").GetValue<Slider>().Value)
            {
                if (Q.IsReady() && useQ)
                {
                    foreach (
                       var en in
                           HeroManager.Enemies.Where(
                               hero =>
                                   hero.IsValidTarget(qRange)))
                    {
                        if (Q.CanCast(en) && Q.GetPrediction(en).Hitchance >= HitChance.High && !Player.IsWindingUp && !Player.IsDashing())

                            Q.Cast(en);
                    }
                }
            }
            
        }
        private static void Clear()
        {
            var useQ = Config.Item("qClear").GetValue<bool>();
            if (Q.IsReady() && useQ && ObjectManager.Player.ManaPercent > Config.Item("clearMana").GetValue<Slider>().Value)
            {
                var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All);
                var Qfarm = Q.GetCircularFarmLocation(allMinionsQ, 200);
                if (Qfarm.MinionsHit >= Config.Item("qMinions").GetValue<Slider>().Value && Q.IsReady())
                    Q.Cast(Qfarm.Position);
            }
        }
        private static void Jungle()
        {
            /*soon(tm)*/
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
            if (Config.Item("ghostblade").GetValue<bool>())
            {
                if (Items.CanUseItem(3142))
                {
                    Items.UseItem(3142);
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
                if (W.CanCast(sender))
                {
                    W.Cast(sender);
                }
            }
        }
        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Config.Item("agapcloser").GetValue<bool>())
            {
                if (gapcloser.Sender.IsValidTarget(1000))
                {
                    Render.Circle.DrawCircle(gapcloser.Sender.Position, gapcloser.Sender.BoundingRadius, Color.Gold, 5);
                    var targetpos = Drawing.WorldToScreen(gapcloser.Sender.Position);
                    Drawing.DrawText(targetpos[0] - 40, targetpos[1] + 20, Color.Gold, "Gapcloser");
                }
                if (E.CanCast(gapcloser.Sender))
                {
                    E.Cast(gapcloser.Sender);
                }
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            qRange = Config.Item("qRange").GetValue<Slider>().Value;
            var menuItem1 = Config.Item("qDraw").GetValue<Circle>();
            var menuItem2 = Config.Item("wDraw").GetValue<Circle>();
            var menuItem3 = Config.Item("eDraw").GetValue<Circle>();
            var menuItem4 = Config.Item("rDraw").GetValue<Circle>();
            var menuItem5 = Config.Item("aaRangeDraw").GetValue<Circle>();

            if (menuItem1.Active && Q.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, qRange, Color.SpringGreen);
            }
            if (menuItem2.Active && E.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.Crimson);
            }
            if (menuItem3.Active && W.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (menuItem4.Active && R.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.White);
            }
            if (menuItem5.Active)
            {
                Render.Circle.DrawCircle(Player.Position, 525, Color.White);
            }
           
        }
    }
}
