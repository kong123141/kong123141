using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using TheKalista.Commons;
using TheKalista.Commons.ComboSystem;
using TheKalista.Commons.Debug;

namespace TheKalista
{
    public class KalistaE : Skill
    {
        public bool FarmAssist;
        public int FarmAssistMana;
        public int MinLaneclear;
        public bool StealJungle;
        public bool DrawCount;

        public KalistaE(SpellSlot slot, float range, TargetSelector.DamageType damageType)
            : base(slot, range, damageType)
        {
        }

        public KalistaE(SpellSlot slot)
            : base(slot)
        {
        }

        private readonly Dictionary<Obj_AI_Hero, MissileClient> _flyingAttacks = new Dictionary<Obj_AI_Hero, MissileClient>();
        public int MobilityType;
        public int Reduction;

        public override void Initialize(ComboProvider combo)
        {
            Obj_AI_Base recentTarget = null;
            var dead = false;

            Orbwalking.AfterAttack += (sender, target) =>
            {
                if (sender.IsMe)
                {
                    var targetObj = target as Obj_AI_Base;
                    if (targetObj == null) return;
                    recentTarget = targetObj;
                    dead = HealthPrediction.GetHealthPrediction(targetObj, 500) - ObjectManager.Player.GetAutoAttackDamage(targetObj) <= 0;
                }
            };
            Console.WriteLine(Delay + " ");
            Orbwalking.OnNonKillableMinion += minion =>
            {
                if (minion.NetworkId == recentTarget.NetworkId && dead) return;
                if (FarmAssist && IsKillable(minion as Obj_AI_Base) && (Provider.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo && Provider.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.None))
                {
                    if (ObjectManager.Player.ManaPercent > FarmAssistMana)
                        Cast();
                    else if (MinionManager.GetMinions(Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.None).Any(min => min.HasBuff("Kalistaexpungemarker") && IsKillable(min) && min.NetworkId != minion.NetworkId))
                        Cast();
                }
            };

            GameObject.OnCreate += (obj, args) =>
            {
                if (obj.Type != GameObjectType.MissileClient) return;
                var mc = (MissileClient)obj;
                if (!mc.SpellCaster.IsMe || mc.Target.Type != GameObjectType.obj_AI_Hero || (mc.SData.MissileSpeed != 2000 && mc.SData.MissileSpeed != 2600)) return;
                _flyingAttacks[(Obj_AI_Hero)mc.Target] = mc;
            };

            base.Initialize(combo);
        }

        public override void Update(Orbwalking.OrbwalkingMode mode, ComboProvider combo, Obj_AI_Hero target)
        {
            if (mode != Orbwalking.OrbwalkingMode.None && StealJungle && MinionManager.GetMinions(Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.None).Any(min => min.HasBuff("Kalistaexpungemarker") && IsKillable(min) && !min.Name.Contains("Mini") && !min.Name.Contains("Dragon") && !min.Name.Contains("Baron")))
                Cast();

            base.Update(mode, combo, target);
        }

        public override void Draw()
        {
            foreach (var enemy in HeroManager.Enemies)
            {
                if (enemy.IsValid && enemy.IsHPBarRendered)
                {
                    var pos = enemy.HPBarPosition;

                    Drawing.DrawText(pos.X + 145, pos.Y + 20, Color.White, "~" + (Instance.GetState() != SpellState.Cooldown && Instance.State != SpellState.NoMana ? Math.Ceiling((enemy.Health - GetDamage(enemy)) / KalistaWalker.GetDamageForOneAuto(enemy, Level)) : Math.Ceiling(enemy.Health / ObjectManager.Player.GetAutoAttackDamage(enemy))) + " AA");
                }
            }

        }


        public override void Execute(Obj_AI_Hero target)
        {
            KillAnyone();

            var enemyWithStacks = false;
            var enemyInAutoRange = false;
            var sumDamage = 0f;
            Profiler.StartEndSection("e_collectData");
            foreach (var enemy in HeroManager.Enemies)
            {
                var valid = enemy.IsValidTarget(Range);
                if (valid)
                {
                    if (!enemyInAutoRange && Provider.Orbwalker.InAutoAttackRange(enemy))
                        enemyInAutoRange = true;

                    if (enemy.IsMelee && enemy.AttackRange + ObjectManager.Player.BoundingRadius > ObjectManager.Player.Position.Distance(enemy.Position))
                    {
                        //enemyWithStacks = false;
                        SlowWithReset();
                        return;
                    }

                    if (MobilityType != 4 && enemy.HasBuff("Kalistaexpungemarker"))
                    {
                        enemyWithStacks = true;
                        sumDamage += GetDamage(enemy);
                    }
                }

            }

            Profiler.StartEndSection("e_useData");
            if (!enemyInAutoRange && enemyWithStacks)
                SlowWithReset();
            else if (sumDamage > KalistaWalker.GetDamageForOneAuto(target, Level) * Math.Max(1, (MobilityType + 1) * (MobilityType + 1)))
            {
                // Console.WriteLine(sumDamage + " > " + KalistaWalker.GetDamageForOneAuto(target, Level));
                SlowWithReset();
            }
        }

        public override float GetDamage(Obj_AI_Hero enemy)
        {
            return base.GetDamage(enemy) - Reduction;
        }

        public override void LaneClear()
        {
            if (TickLimiter.Limit(100, 2))
            {
                var minions = MinionManager.GetMinions(Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.None);
                if (minions.Count(min => min.HasBuff("Kalistaexpungemarker") && IsKillable(min)) >= MinLaneclear)
                    Cast();
                else if (minions.Any(min => min.HasBuff("Kalistaexpungemarker") && IsKillable(min) && (min.SkinName.ToLower().Contains("siege") || min.SkinName.ToLower().Contains("super"))))
                    Cast();
            }
        }

        public override void Harass(Obj_AI_Hero target)
        {
            KillAnyone();
            base.Harass(target);
        }



        private void KillAnyone()
        {
            foreach (var enemy in HeroManager.Enemies)
            {
                if (!enemy.IsValidTarget(Range) || KalistaTargetSelector.IsInvulnerable(enemy, KalistaTargetSelector.DamageType.Physical, false)) continue;
                var damage = GetDamage(enemy);
                if (_flyingAttacks.ContainsKey(enemy) && _flyingAttacks[enemy].IsValid)
                {
                    var flyingAuto = _flyingAttacks[enemy];
                    //   if(flyingAuto)
                    if (flyingAuto.Position.Distance(enemy.Position, true) < 300 * 300)
                        damage += KalistaWalker.GetDamageForOneAuto(enemy, Level, flyingAuto.SData.MissileSpeed < 2600);
                }
                if (damage > enemy.Health && !KalistaTargetSelector.IsInvulnerable(enemy, KalistaTargetSelector.DamageType.Physical, false))
                {
                    Cast();
                }
            }



            //if (HeroManager.Enemies.Any(enemy => enemy.IsValidTarget(Range) && IsKillable(enemy) && !enemy.HasSpellShield() && !TargetSelector.IsInvulnerable(enemy, TargetSelector.DamageType.Physical)))
            //    Cast();
        }

        private void SlowWithReset()
        {
            foreach (var objectType in MinionManager.GetMinions(Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.None))
            {
                var hPred = HealthPrediction.GetHealthPrediction(objectType, 250);
                if (objectType.HasBuff("Kalistaexpungemarker") && GetDamage(objectType) > hPred && hPred > 0)
                {
                    Cast();
                    return;
                }
            }

            //if (ObjectManager.Get<Obj_AI_Base>().Any(minion => minion.IsValidTarget(Range) && minion.HasBuff("Kalistaexpungemarker") && GetDamage(minion) > minion.Health + 10f && HealthPrediction.GetHealthPrediction(minion, (int)(Delay * 1000f)) > 0))
            //{
            //    Cast();
            //}
        }

        public override int GetPriority()
        {
            return 4;
        }
    }
}
