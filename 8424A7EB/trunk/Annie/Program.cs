#region

/*
 * Credits to:
 * Eskor
 * Roach_
 * Both for helping me alot doing this Assembly and start On L# 
 * lepqm for cleaning my shit up
 * iMeh Code breaker 101
 */
using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

#endregion

namespace Annie
{
    internal class Program
    {
        public const string CharName = "Annie";
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Spell R1;

        public static float DoingCombo = 0;

        public static SpellSlot IgniteSlot;
        public static SpellSlot FlashSlot;

        public static Menu Config;

        private static int StunCount
        {
            get
            {
                foreach (var buff in
                    ObjectManager.Player.Buffs.Where(
                        buff => buff.Name == "pyromania" || buff.Name == "pyromania_particle"))
                {
                    switch (buff.Name)
                    {
                        case "pyromania":
                            return buff.Count;
                        case "pyromania_particle":
                            return 4;
                    }
                }

                return 0;
            }
        }

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != CharName)
            {
                return;
            }

            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
            FlashSlot = ObjectManager.Player.GetSpellSlot("SummonerFlash");

            Q = new Spell(SpellSlot.Q, 625f);
            W = new Spell(SpellSlot.W, 625f);
            E = new Spell(SpellSlot.E, float.MaxValue);
            R = new Spell(SpellSlot.R, 600f);
            R1 = new Spell(SpellSlot.R, 900f);

            W.SetSkillshot(0.60f, 625f, float.MaxValue, false, SkillshotType.SkillshotCone);
            R.SetSkillshot(0.20f, 200f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R1.SetSkillshot(0.25f, 200f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(R);
            SpellList.Add(R1);

            Config = new Menu(CharName, CharName, true);

            Config.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker"));

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);

            Config.AddSubMenu(targetSelectorMenu);
            Config.AddSubMenu(new Menu("Combo settings", "combo"));
            Config.SubMenu("combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));
            Config.SubMenu("combo").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("combo").AddItem(new MenuItem("autoult", "AutoUltimate!").SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
 
            Config.SubMenu("combo").AddItem(new MenuItem("comboItems", "Use Items")).SetValue(true);
            Config.SubMenu("combo")
                .AddItem(new MenuItem("flashCombo", "Targets needed to Flash -> R"))
                .SetValue(new Slider(4, 5, 1));

            Config.AddSubMenu(new Menu("Farm Settings", "lasthit"));
            Config.SubMenu("lasthit").AddItem(new MenuItem("qFarm", "Last hit with Disintegrate (Q)").SetValue(true));
            Config.SubMenu("lasthit").AddItem(new MenuItem("wFarm", "Lane Clear with Incinerate (W)").SetValue(true));
            Config.SubMenu("lasthit")
                .AddItem(new MenuItem("saveqStun", "Don't Last Hit with Q while stun is up").SetValue(true));
            Config.AddSubMenu(new Menu("Draw Settings", "draw"));
            Config.SubMenu("draw")
                .AddItem(
                    new MenuItem("QDraw", "Draw Disintegrate (Q) Range").SetValue(
                        new Circle(true, Color.FromArgb(128, 178, 0, 0))));
            Config.SubMenu("draw")
                .AddItem(
                    new MenuItem("WDraw", "Draw Incinerate (W) Range").SetValue(
                        new Circle(false, Color.FromArgb(128, 32, 178, 170))));
            Config.SubMenu("draw")
                .AddItem(
                    new MenuItem("RDraw", "Draw Tibbers (R) Range").SetValue(
                        new Circle(true, Color.FromArgb(128, 128, 0, 128))));
            Config.SubMenu("draw")
                .AddItem(
                    new MenuItem("R1Draw", "Draw Flash -> R combo Range").SetValue(
                        new Circle(true, Color.FromArgb(128, 128, 0, 128))));
            Config.AddItem(new MenuItem("PCast", "Packet Cast Spells").SetValue(true));
            Config.AddToMainMenu();

            Drawing.OnDraw += OnDraw;
            Game.OnUpdate += OnGameUpdate;
            GameObject.OnCreate += OnCreateObject;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;

            Game.PrintChat("Annie# Loaded");
        }

        private static void OnDraw(EventArgs args)
        {
            // Utility.DrawCircle(R1.GetPrediction(TargetSelector.GetTarget(900, TargetSelector.DamageType.Magical)).CastPosition, 250,
            //     Color.Aquamarine);
            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Draw").GetValue<Circle>();
                if (menuItem.Active)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spell.Range, menuItem.Color);
                }
            }
        }

        private static void OnCreateObject(GameObject sender, EventArgs args)
        {
            if (sender.IsAlly || !(sender is Obj_SpellMissile))
            {
                return;
            }

            var missile = (Obj_SpellMissile)sender;
            if (!(missile.SpellCaster is Obj_AI_Hero) || !(missile.Target.IsMe))
            {
                return;
            }

            if (E.IsReady())
            {
                E.Cast();
            }
            else if (!ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(missile.SpellCaster.NetworkId).IsMelee())
            {
                var ecd = (int)(ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).CooldownExpires - Game.Time) *
                          1000;
                if ((int)Vector3.Distance(missile.Position, ObjectManager.Player.ServerPosition) /
                    ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(missile.SpellCaster.NetworkId)
                        .BasicAttack.MissileSpeed * 1000 > ecd)
                {
                    Utility.DelayAction.Add(ecd, () => E.Cast());
                }
            }
        }

        private static void OnGameUpdate(EventArgs args)
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var flashRtarget = TargetSelector.GetTarget(900, TargetSelector.DamageType.Magical);
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo(target, flashRtarget, true);
            }
            if (Config.Item("HarassActive").GetValue<KeyBind>().Active)
            {
                Combo(target, flashRtarget, false);
            }
            if ((flashRtarget != null && R.IsReady() && GetEnemiesInRange(flashRtarget.ServerPosition, 250) >=
                            Config.Item("flashCombo").GetValue<Slider>().Value) && (Config.Item("autoult").GetValue<KeyBind>().Active))
            {
                FlashUlt(flashRtarget);
            }
        }
        private static void FlashUlt(Obj_AI_Hero target)
        {
            var position = R1.GetPrediction(target, true).CastPosition;
            if (StunCount == 3 && E.IsReady())
            {
                E.Cast();
            }
            else if (StunCount == 4)
            {
                if (ObjectManager.Player.Distance(position) > 600)
                {
                    ObjectManager.Player.Spellbook.CastSpell(FlashSlot, position);
                }
                R.Cast(target, false, true);
            }
        }
        private static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            args.Process = Environment.TickCount - DoingCombo > 500;
        }

       private static void Combo(Obj_AI_Base target, Obj_AI_Base flashRtarget, bool ulti)
        {
            Console.WriteLine("[" + Game.Time + "]Combo started");
            if ((target == null && flashRtarget == null) || Environment.TickCount - DoingCombo < 500 ||
                (!Q.IsReady() && !W.IsReady() && !R.IsReady()))
            {
                return;
            }

            Console.WriteLine("[" + Game.Time + "]Target acquired");
            if (Config.Item("comboItems").GetValue<bool>() && target != null)
            {
                Items.UseItem(3128, target);
            }

            switch (StunCount)
            {
                case 3:
                    Console.WriteLine("[" + Game.Time + "]Case 3");
                    if (Q.IsReady())
                    {
                        DoingCombo = Environment.TickCount;
                        Q.CastOnUnit(target, Config.Item("PCast").GetValue<bool>());
                         Utility.DelayAction.Add(
                            (int) (ObjectManager.Player.Distance(target.ServerPosition) / Q.Speed * 1000 - Game.Ping / 2.0)+250,
                            () =>
                            {
                                if (R.IsReady() &&
                                    !(ObjectManager.Player.GetSpellDamage(target, SpellSlot.R) > target.Health))
                                {
                                    R.Cast(target, false, true);
                                }
                            });

                    }
                    else if (W.IsReady())
                    {
                        DoingCombo = Environment.TickCount;
                    }

                    W.Cast(target, false, true); //stack only goes up after 650 secs


                    break;
                case 4:
                    Console.WriteLine("[" + Game.Time + "]Case 4");
                    if (R.IsReady() && ulti && !(ObjectManager.Player.GetSpellDamage(target, SpellSlot.R) * 0.6 > target.Health))
                        {
                            R.Cast(target, false, true);
                        }
                    if (W.IsReady())
                    {
                        W.Cast(target, false, true);
                    }

                    if (Q.IsReady())
                    {
                        Q.Cast(target, false, true);
                    }

                    break;
                default:
                    Console.WriteLine("[" + Game.Time + "]Case default");
                    if (Q.IsReady())
                    {
                        Q.CastOnUnit(target, Config.Item("PCast").GetValue<bool>());
                    }

                    if (W.IsReady())
                    {
                        W.Cast(target, false, true);
                    }

                    break;
            }

            if (IgniteSlot != SpellSlot.Unknown && target != null &&
                ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready &&
                ObjectManager.Player.Distance(target.ServerPosition) < 600 &&
                ObjectManager.Player.GetSpellDamage(target, IgniteSlot) > target.Health)

            {
                ObjectManager.Player.Spellbook.CastSpell(IgniteSlot, target);
            }
        }

       private static void Farm(bool laneclear)
       {
           var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
           var jungleMinions = MinionManager.GetMinions(
               ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral);
           minions.AddRange(jungleMinions);

           if (laneclear && Config.Item("wFarm").GetValue<bool>() && W.IsReady())
           {
               if (minions.Count > 0)
               {
                   W.Cast(W.GetLineFarmLocation(minions).Position.To3D());
               }
           }
           if (((!Config.Item("qFarm").GetValue<bool>() ||
                 !Orbwalker.ActiveMode.Equals(Orbwalking.OrbwalkingMode.LastHit)) &&
                (!Config.Item("qFarmHarass").GetValue<bool>() ||
                 !Orbwalker.ActiveMode.Equals(Orbwalking.OrbwalkingMode.Mixed)) &&
                !Orbwalker.ActiveMode.Equals(Orbwalking.OrbwalkingMode.LaneClear)) ||
               Config.Item("saveqStun").GetValue<bool>() && StunCount == 4 || !Q.IsReady())
           {
               return;
           }
           foreach (var minion in
               from minion in
                   minions.OrderByDescending(Minions => Minions.MaxHealth)
                       .Where(minion => minion.IsValidTarget(Q.Range))
               let predictedHealth = Q.GetHealthPrediction(minion)
               where predictedHealth < ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q) * 0.85 && predictedHealth > 0
               select minion)
           {
               Q.CastOnUnit(minion, Config.Item("PCast").GetValue<bool>());
           }
       }


        private static int GetEnemiesInRange(Vector3 pos, float range)
        {
            //var Pos = pos;
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(hero => hero.Team != ObjectManager.Player.Team)
                    .Count(hero => Vector3.Distance(pos, hero.ServerPosition) <= range);
        }
    }
}
