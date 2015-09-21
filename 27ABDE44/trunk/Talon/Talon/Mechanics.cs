using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Talon.Helper;

namespace Talon
{
    internal class Mechanics
    {
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        private static readonly Dictionary<SpellSlot, Spell> Spell = Spells.Spell;
        private static readonly bool PacketCast = Config.Talon.GetBool("packets.bool");
        public static readonly SpellSlot IgniteSlot = Player.GetSpellSlot("SummonerDot");

        public static void Init()
        {
            Game.OnUpdate += OnUpdate;
            Orbwalking.BeforeAttack += BeforeAttack;
            Orbwalking.AfterAttack += AfterAttack;
        }
        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead || Player.IsRecalling() || args == null)
                return;

            Killsteal();

            if (Config.Talon.GetKeyBind("harass.key").Active)        
                Harass();


            switch (Config.Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                {
                    Combo();
                    break;
                }
                case Orbwalking.OrbwalkingMode.LaneClear:
                {
                    Laneclear();
                    Jungleclear();
                    break;
                }
                case Orbwalking.OrbwalkingMode.Mixed:
                {
                    Harass();
                    break;
                }
            }
        }
        private static void Combo()
        {
            var t = GetTarget(Spell[SpellSlot.W].Range);

            if (t == null)
                return;

            if (IgniteSlot != SpellSlot.Unknown && IgniteSlot.IsReady() && Config.Talon.GetBool("combo.ignite.bool") &&
                t.Health < Damages.ComboDmg(t))
            {
                Player.Spellbook.CastSpell(IgniteSlot, t);
            }

            if (Spell[SpellSlot.W].IsReadyAndActive(MenuHelper.Mode.Combo))
            {
                var pred = Spell[SpellSlot.W].GetPrediction(t);
                if (pred.Hitchance >= Spell[SpellSlot.W].GetHitChance(MenuHelper.Mode.Combo))
                {
                    Cast.W(t, pred.UnitPosition.To2D(), PacketCast);
                }
            }

            if (Spell[SpellSlot.E].IsReadyAndActive(MenuHelper.Mode.Combo) && t.IsValidTarget(Spell[SpellSlot.E].Range))
            {
                Spell[SpellSlot.E].CastOnUnit(t, PacketCast);
            }

            if (Spell[SpellSlot.R].IsReadyAndActive(MenuHelper.Mode.Combo))
            {
                if (Damages.ComboDmg(t) > t.Health && t.IsValidTarget(Spell[SpellSlot.R].Range))
                    Spell[SpellSlot.R].Cast(PacketCast);
                else if (Player.CountEnemiesInRange(Spell[SpellSlot.R].Range - 20) >=
                         Config.Talon.GetSlider("combo.r.hit"))
                    Spell[SpellSlot.R].Cast(PacketCast);
            }
        }
        private static void Harass()
        {
            var t = TargetSelector.GetTarget(Player, Spell[SpellSlot.W].Range, TargetSelector.DamageType.Physical);
            var mana = Player.ManaPercent > Config.Talon.GetSlider("harass.mana");

            if (t == null || !mana)
                return;

            if (Spell[SpellSlot.W].IsReadyAndActive(MenuHelper.Mode.Harass))
            {
                var pre = Spell[SpellSlot.W].GetPrediction(t);
                if (pre.Hitchance >= Spell[SpellSlot.W].GetHitChance(MenuHelper.Mode.Harass))
                    Cast.W(t, pre.UnitPosition.To2D(), PacketCast);
            }
            if (Spell[SpellSlot.E].IsReadyAndActive(MenuHelper.Mode.Harass))
            {
                Spell[SpellSlot.E].CastOnUnit(t, PacketCast);
            }
        }
        private static void Killsteal()
        {
            var t =
                HeroManager.Enemies.Where(
                    h =>
                        h.IsValidTarget(Spell[SpellSlot.W].Range) &&
                        h.Health + 15 < Player.GetSpellDamage(h, SpellSlot.W))
                    .OrderBy(h => h.Health)
                    .FirstOrDefault(h => h.Health > 1);

            if (t == null)
                return;

            Spell[SpellSlot.W].CastIfHitchanceEquals(t, HitChance.High, PacketCast);
        }
        private static void Laneclear()
        {
            var minions = MinionManager.GetMinions(Player.ServerPosition, Spell[SpellSlot.W].Range);
            var mana = Player.ManaPercent > Config.Talon.GetSlider("laneclear.mana");

            if (minions == null || !mana)
                return;

            if (Spell[SpellSlot.W].IsReadyAndActive(MenuHelper.Mode.Laneclear))
            {
                var minHit = Config.Talon.GetSlider("laneclear.w.hit");
                var lineFarm =
                    MinionManager.GetBestCircularFarmLocation(
                        minions.Select(m => m.ServerPosition.To2D()).ToList(), Spell[SpellSlot.W].Width * 3,
                        Spell[SpellSlot.W].Range);

                if (lineFarm.MinionsHit >= minHit)
                    Spell[SpellSlot.W].Cast(lineFarm.Position, PacketCast);
            }
        }
        private static void Jungleclear()
        {
            var minions = MinionManager.GetMinions(
                Player.ServerPosition, Spell[SpellSlot.W].Range, MinionTypes.All, MinionTeam.Neutral,
                MinionOrderTypes.MaxHealth).FirstOrDefault();
            var mana = Player.ManaPercent > Config.Talon.GetSlider("laneclear.mana");

            if (minions == null || !mana)
                return;

            if (Spell[SpellSlot.W].IsReadyAndActive(MenuHelper.Mode.Laneclear))
            {
                Spell[SpellSlot.W].Cast(minions, PacketCast);
            }
        }

        private static Obj_AI_Hero GetTarget(float vDefaultRange = 0,
            TargetSelector.DamageType vDefaultDamageType = TargetSelector.DamageType.Physical)
        {
            if (Math.Abs(vDefaultRange) < 0.00001)
                vDefaultRange = Spell[SpellSlot.W].Range;

            if (!Config.Talon.Item("AssassinActive").GetValue<bool>())
                return TargetSelector.GetTarget(vDefaultRange, vDefaultDamageType);

            var assassinRange = Config.Talon.Item("AssassinSearchRange").GetValue<Slider>().Value;

            var vEnemy =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        enemy =>
                            enemy.Team != ObjectManager.Player.Team && !enemy.IsDead && enemy.IsVisible &&
                            Config.Talon.Item("Assassin" + enemy.ChampionName) != null &&
                            Config.Talon.Item("Assassin" + enemy.ChampionName).GetValue<bool>() &&
                            Vector3.Distance(Player.ServerPosition, enemy.ServerPosition) < assassinRange);

            if (Config.Talon.Item("AssassinSelectOption").GetValue<StringList>().SelectedIndex == 1)
            {
                vEnemy = (from vEn in vEnemy select vEn).OrderByDescending(vEn => vEn.MaxHealth);
            }

            Obj_AI_Hero[] objAiHeroes = vEnemy as Obj_AI_Hero[] ?? vEnemy.ToArray();

            Obj_AI_Hero t = !objAiHeroes.Any()
                ? TargetSelector.GetTarget(vDefaultRange, vDefaultDamageType)
                : objAiHeroes[0];

            return t;
        }

        private static void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (Config.Talon.Item("apollo.talon.misc.q.mode").GetValue<StringList>().SelectedIndex != 1 ||
                !args.Unit.IsMe)
                return;

            if (Config.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo &&
                Spell[SpellSlot.Q].IsReadyAndActive(MenuHelper.Mode.Combo))
            {
                Spell[SpellSlot.Q].Cast(PacketCast);
            }
            else if ((Config.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed ||
                      Config.Talon.GetKeyBind("harass.key").Active) &&
                     Spell[SpellSlot.Q].IsReadyAndActive(MenuHelper.Mode.Harass) &&
                     Player.ManaPercent > Config.Talon.GetSlider("harass.mana"))
            {
                Spell[SpellSlot.Q].Cast(PacketCast);
            }
        }

        private static void AfterAttack(AttackableUnit sender, AttackableUnit target)
        {
            if (!sender.IsMe)
                return;

            if (Config.Talon.Item("apollo.talon.misc.q.mode").GetValue<StringList>().SelectedIndex == 0)
            {
                if (Config.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo &&
                    Spell[SpellSlot.Q].IsReadyAndActive(MenuHelper.Mode.Combo))
                {
                    Spell[SpellSlot.Q].Cast(PacketCast);
                }
                else if ((Config.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed ||
                          Config.Talon.GetKeyBind("harass.key").Active) &&
                         Spell[SpellSlot.Q].IsReadyAndActive(MenuHelper.Mode.Harass) &&
                         Player.ManaPercent > Config.Talon.GetSlider("harass.mana"))
                {
                    Spell[SpellSlot.Q].Cast(PacketCast);
                }
            }
            else if (Config.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear &&
                     Spell[SpellSlot.Q].IsReadyAndActive(MenuHelper.Mode.Laneclear))
            {
                var laneMinions =
                    MinionManager.GetMinions(Player.ServerPosition, 300)
                        .Where(
                            h =>
                                h.IsValidTarget(Orbwalking.GetRealAutoAttackRange(h)) &&
                                h.Health < Player.GetSpellDamage(h, SpellSlot.Q) + Player.GetAutoAttackDamage(h))
                        .FirstOrDefault(h => h.Health > Player.GetAutoAttackDamage(h));
                if (laneMinions != null)
                {
                    Player.IssueOrder(GameObjectOrder.AttackUnit, laneMinions);
                    Spell[SpellSlot.Q].Cast(PacketCast);
                }
                else if (
                    MinionManager.GetMinions(Player.ServerPosition, 300, MinionTypes.All, MinionTeam.Neutral)
                        .Any(h => h.IsValidTarget(Orbwalking.GetRealAutoAttackRange(h))))
                {
                    Spell[SpellSlot.Q].Cast(PacketCast);
                }
            }
        }
    }
}
