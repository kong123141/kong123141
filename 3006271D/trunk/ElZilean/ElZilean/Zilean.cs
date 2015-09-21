using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ElZilean
{
    enum Spells
    {
        Q, W, E, R
    }

    /// <summary>
    ///     Handle all stuff what is going on with Rengar.
    /// </summary>
    internal class Zilean
    {
        private static String hero = "Zilean";
        public static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public static Orbwalking.Orbwalker _orbwalker;
        private static SpellSlot _ignite;
        public static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>()
        {
            { Spells.Q, new Spell(SpellSlot.Q, 900)},
            { Spells.W, new Spell(SpellSlot.W, 0)},
            { Spells.E, new Spell(SpellSlot.E, 700)},
            { Spells.R, new Spell(SpellSlot.R, 900)}
        };

        #region hitchance

        private static HitChance CustomHitChance
        {
            get { return GetHitchance(); }
        }

        private static HitChance GetHitchance()
        {
            switch (ZileanMenu._menu.Item("ElZilean.hitChance").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }

        #endregion

        #region Gameloaded 

        public static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != hero)
                return;

            Notifications.AddNotification("ElZilean by jQuery v1.0.1.5", 10000);
            spells[Spells.Q].SetSkillshot(0.30f, 210f, 2000f, false, SkillshotType.SkillshotCircle);
            _ignite = Player.GetSpellSlot("summonerdot");

            ZileanMenu.Initialize();
            Game.OnUpdate += OnGameUpdate;
            Drawing.OnDraw += Drawings.Drawing_OnDraw;
            Orbwalking.BeforeAttack += OrbwalkingBeforeAttack;
        }

        #endregion

        private static void OrbwalkingBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (ZileanMenu._menu.Item("ElZilean.SupportMode").GetValue<bool>())
            {
                if (args.Target is Obj_AI_Minion)
                {
                        args.Process = false;
                }
            } 
        }

        #region OnGameUpdate

        private static void OnGameUpdate(EventArgs args)
        {
            switch (_orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
            }

            UltAlly();
            SelfUlt();

            if (ZileanMenu._menu.Item("FleeActive").GetValue<KeyBind>().Active)
            {
                Flee();
            }

            if (ZileanMenu._menu.Item("ElZilean.AutoHarass").GetValue<KeyBind>().Active)
            {
                var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);
                if (target == null || !target.IsValid)
                    return;

                var q = ZileanMenu._menu.Item("ElZilean.UseQAutoHarass").GetValue<bool>();
                var e = ZileanMenu._menu.Item("ElZilean.UseEAutoHarass").GetValue<bool>();
                var mana = ZileanMenu._menu.Item("ElZilean.harass.mana").GetValue<Slider>().Value;

                if (!(Player.ManaPercent >= mana))
                {
                    return;
                }
                if (q && spells[Spells.Q].IsReady() && Player.Distance(target) <= spells[Spells.Q].Range)
                {
                    spells[Spells.Q].CastIfHitchanceEquals(target, CustomHitChance);
                }

                if (e && spells[Spells.E].IsReady() && Player.Distance(target) <= spells[Spells.E].Range)
                {
                    spells[Spells.E].Cast(target);
                }
            }
        }

        #endregion


        #region Flee

        private static void Flee()
        {
            if (spells[Spells.E].IsReady())
            {
                spells[Spells.E].Cast(Player);
            }

            if (spells[Spells.W].IsReady())
            {
                spells[Spells.W].Cast();
            }
        }

        #endregion

        #region selfult

        private static void SelfUlt()
        {
            var useSelftUlt = ZileanMenu._menu.Item("ElZilean.R").GetValue<bool>();
            var useSelftHp = ZileanMenu._menu.Item("ElZilean.HP").GetValue<Slider>().Value;

            if (Player.IsRecalling() || Player.InFountain())
                return;
            if (useSelftUlt && (Player.Health / Player.MaxHealth) * 100 <= useSelftHp && spells[Spells.R].IsReady() 
                && Player.CountEnemiesInRange(650) > 0)
            {
                spells[Spells.R].Cast(Player);
            }
        }

        #endregion

        #region ultAlly

        private static void UltAlly()
        {
            
            var useult = ZileanMenu._menu.Item("ElZilean.useult").GetValue<bool>();
            var allyMinHp = ZileanMenu._menu.Item("ElZilean.Ally.HP").GetValue<Slider>().Value;

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly && !hero.IsMe))
            {
                var getAllys = ZileanMenu._menu.Item("ElZilean.Cast.Ult.Ally" + hero.BaseSkinName);

                if (Player.IsRecalling() || Player.InFountain())
                    return;
                if (useult && ((hero.Health / hero.MaxHealth) * 100 <= allyMinHp) && spells[Spells.R].IsReady() &&
                    Player.CountEnemiesInRange(1000) > 0 &&
                    (hero.Distance(Player.ServerPosition) <= spells[Spells.R].Range))
                {
                    if (getAllys != null && getAllys.GetValue<bool>())
                    {
                        spells[Spells.R].Cast(hero);
                    }
                }
            }
        }

        #endregion

        #region Laneclear 

        private static void LaneClear()
        {
            var minion = MinionManager.GetMinions(Player.ServerPosition, spells[Spells.Q].Range).FirstOrDefault();
            if (minion == null || minion.Name.ToLower().Contains("ward")) return;

            var qWaveClear = ZileanMenu._menu.Item("ElZilean.Clear.Q").GetValue<bool>();
            var wWaveClear = ZileanMenu._menu.Item("ElZilean.Clear.W").GetValue<bool>();

            var bestFarmLocation = MinionManager.GetBestCircularFarmLocation(MinionManager.GetMinions(spells[Spells.Q].Range, MinionTypes.All, MinionTeam.Enemy).Select(m => m.ServerPosition.To2D()).ToList(), spells[Spells.Q].Width, spells[Spells.Q].Range);

            if (qWaveClear && minion.IsValidTarget() && spells[Spells.Q].IsReady())
            {
                //Spells.W.Cast();
                spells[Spells.Q].Cast(bestFarmLocation.Position);
            }

            if (wWaveClear && !spells[Spells.Q].IsReady())
            {
                spells[Spells.W].Cast();
            }
        }

        #endregion

        #region Harass

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValid)
                return;


            var qCombo = ZileanMenu._menu.Item("ElZilean.Harass.Q").GetValue<bool>();
            var eCombo = ZileanMenu._menu.Item("ElZilean.Harass.E").GetValue<bool>();


            if (qCombo && spells[Spells.Q].IsReady() && Player.Distance(target) <= spells[Spells.Q].Range)
            {
                var pred = spells[Spells.Q].GetPrediction(target);
                if (pred.Hitchance >= CustomHitChance)
                    spells[Spells.Q].Cast(target);
            }

            if (eCombo && spells[Spells.E].IsReady() && Player.Distance(target) <= spells[Spells.E].Range)
            {
                spells[Spells.E].Cast(target);
            }
        }


        #endregion

        #region Combo

        private static void Combo()
        {
            Obj_AI_Hero target;
            target = TargetSelector.GetSelectedTarget();
            if (target == null || !target.IsValidTarget())
            {
                target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);
            }

            if (TargetSelector.GetSelectedTarget() != null)
            {
                if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < spells[Spells.Q].Range)
                {
                    target = TargetSelector.GetSelectedTarget();
                }
            }

             if (target == null || !target.IsValid)
                return;

            var qCombo = ZileanMenu._menu.Item("ElZilean.Combo.Q").GetValue<bool>();
            var eCombo = ZileanMenu._menu.Item("ElZilean.Combo.E").GetValue<bool>();
            var wCombo = ZileanMenu._menu.Item("ElZilean.Combo.W").GetValue<bool>();
            var useIgnite = ZileanMenu._menu.Item("ElZilean.Combo.Ignite").GetValue<bool>();
 
            if (qCombo && spells[Spells.Q].IsReady() && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < spells[Spells.Q].Range)
            {
                var pred = spells[Spells.Q].GetPrediction(target);
                if (pred.Hitchance >= CustomHitChance)
                    spells[Spells.Q].Cast(target);
            }

            if (eCombo && spells[Spells.E].IsReady() && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < spells[Spells.E].Range)
            {
                spells[Spells.E].Cast(target);
            }

            if (target.HasBuff("ZileanQEnemyBomb") && wCombo)
            {
                spells[Spells.W].Cast();
            }

            if (Player.Distance(target) <= 600 && IgniteDamage(target) >= target.Health &&
                useIgnite)
            {
                Player.Spellbook.CastSpell(_ignite, target);
            }
        }

        #endregion

        #region GetComboDamage   

        public static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (spells[Spells.Q].IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
            }

            return (float)damage;
        }

        #endregion

        #region Ignite

        private static float IgniteDamage(Obj_AI_Hero target)
        {
            if (_ignite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(_ignite) != SpellState.Ready)
            {
                return 0f;
            }
            return (float)Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }

        #endregion
    }
}