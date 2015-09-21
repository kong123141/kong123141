using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace TheKalista
{
    class KalistaWalker : Orbwalking.Orbwalker
    {
        private readonly SpellDataInst _e;
        private static readonly float[] Speerdmg = { 10, 14, 19, 25, 32 };
        private static readonly float[] Scaledmg = { 0.2f, 0.225f, 0.25f, 0.275f, 0.3f };

        public KalistaWalker(Menu attachToMenu)
            : base(attachToMenu)
        {
            _e = ObjectManager.Player.GetSpell(SpellSlot.E);
        }

        public override LeagueSharp.AttackableUnit GetTarget()
        {
            if ((ActiveMode == Orbwalking.OrbwalkingMode.Mixed || ActiveMode == Orbwalking.OrbwalkingMode.LaneClear) && HeroManager.Enemies.Any(enemy => enemy.IsValidTarget(1000) && enemy.HasBuff("Kalistaexpungemarker")))
            {
                if (ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                    return FindAutoPlusRendMinion(MinionManager.GetMinions(ObjectManager.Player.AttackRange, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.None)) ?? base.GetTarget();

                var entity = base.GetTarget();
                if (entity == null)
                {
                    return FindAutoPlusRendMinion(MinionManager.GetMinions(ObjectManager.Player.AttackRange, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.None));
                }
                return entity;
            }

            if (ActiveMode != Orbwalking.OrbwalkingMode.Combo)
                return base.GetTarget();


            var target = KalistaTargetSelector.GetTarget(-1, KalistaTargetSelector.DamageType.Physical);
            //if (!_e.IsReady()) return target;
            if (target == null && HeroManager.Enemies.Any(enemy => enemy.IsValidTarget(2000)))
            {
                var minions = MinionManager.GetMinions(ObjectManager.Player.AttackRange, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.None);
                var optimalMinion = FindAutoPlusRendMinion(minions);
                if (optimalMinion == null && ObjectManager.Player.AttackDelay < 1.10f)
                    return minions.FirstOrDefault();
                return optimalMinion;
            }
            return target;
        }

        private Obj_AI_Base FindAutoPlusRendMinion(List<Obj_AI_Base> minions)
        {
            var optimalMinion = minions.FirstOrDefault(minion =>
            {
                var healthAfterAuto = HealthPrediction.GetHealthPrediction(minion, (int)(ObjectManager.Player.AttackDelay * 1000f)) - ObjectManager.Player.GetAutoAttackDamage(minion);
                var speercount = minion.GetBuffCount("Kalistaexpungemarker") + 1;
                if (healthAfterAuto > 0 && ObjectManager.Player.CalcDamage(minion, Damage.DamageType.Physical, (1 + _e.Level) * 10 + ObjectManager.Player.TotalAttackDamage * 0.6f + speercount * Speerdmg[_e.Level - 1] + speercount * Scaledmg[_e.Level - 1] * ObjectManager.Player.TotalAttackDamage) > healthAfterAuto)
                    return true;
                return false;
            });

            return optimalMinion;
        }

        public static float GetDamageForOneAuto(Obj_AI_Base target, int eLevel, bool isHurricane = false)
        {
            var aadmg = (float)ObjectManager.Player.GetAutoAttackDamage(target);
            if (isHurricane)
                aadmg /= 2;
            if (eLevel == 0) return aadmg;
            return aadmg + (float)ObjectManager.Player.CalcDamage(target, Damage.DamageType.Physical, Speerdmg[eLevel - 1] + Scaledmg[eLevel - 1] * ObjectManager.Player.TotalAttackDamage);
        }
    }
}
