using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using TheCassiopeia.Commons;
using TheCassiopeia.Commons.ComboSystem;

namespace TheCassiopeia
{
    class CassE : Skill
    {
        public bool Farm;
        public int FarmNonPoisonedPercent;
        private int _recentAttacked;
        public bool OnlyKillNonPIn1V1;

        public CassE(SpellSlot slot)
            : base(slot)
        {
            Range = 700;
            SetTargetted(0.2f, float.MaxValue);
            Orbwalking.AfterAttack += AfterAutoAttack;
            UseManaManager = false;


        }

        private void AfterAutoAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe)
                _recentAttacked = target.NetworkId;
        }

        public override void Lasthit()
        {
            var killableMinion = MinionManager.GetMinions(700, MinionTypes.All, MinionTeam.NotAlly).Where(minion => minion.IsPoisoned()).FirstOrDefault(minion => IsKillable(minion) || minion.Team == GameObjectTeam.Neutral);
            if (killableMinion == null && FarmNonPoisonedPercent > ObjectManager.Player.ManaPercent)
                killableMinion = MinionManager.GetMinions(700, MinionTypes.All, MinionTeam.NotAlly).FirstOrDefault(minion => IsKillable(minion));

            if (killableMinion == null) return;

            var hPred = HealthPrediction.GetHealthPrediction(killableMinion, (int)(Delay * 2000), 0);
            if ((killableMinion.NetworkId != _recentAttacked || hPred - ObjectManager.Player.GetAutoAttackDamage(killableMinion) > 0) && hPred > 0)
                Cast(killableMinion);
        }

        public override void Execute(Obj_AI_Hero target)
        {
            if ((Provider.IsMarked(target) || target.IsPoisoned() && target.GetPoisonedTime() > Delay || (IsKillable(target) && (!OnlyKillNonPIn1V1 || ObjectManager.Player.CountEnemiesInRange(1500) == 1))) && !target.HasSpellShield() && !target.IsBehindWindWall())
            {
                Cast(target);
            }
        }

        public override void Harass(Obj_AI_Hero target)
        {
            if (ManaManager.CanUseMana(Orbwalking.OrbwalkingMode.Mixed))
                base.Harass(target);
        }

        public override void LaneClear()
        {
            if (!ManaManager.CanUseMana(Orbwalking.OrbwalkingMode.LaneClear)) return;

            var clearMinion = MinionManager.GetMinions(700, MinionTypes.All, MinionTeam.NotAlly).FirstOrDefault(minion => minion.IsPoisoned());

            if (clearMinion != null)
                Cast(clearMinion);
        }

        public override float GetDamage(Obj_AI_Hero enemy)
        {
            return base.GetDamage(enemy) * 4;
        }

        public override int GetPriority()
        {
            return 3;
        }
    }
}
