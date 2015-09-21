using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using TheKalista.Commons.ComboSystem;

namespace TheKalista
{
    class KalistaR : Skill
    {
        public bool Balista;
        public int AllyHealth;
        public int MyHealth;
        public bool SmartPeel;
        public int HitCount;
        public int BalistaDistance;

        public KalistaR(SpellSlot slot, float range, TargetSelector.DamageType damageType)
            : base(slot, range, damageType)
        {
        }

        public KalistaR(SpellSlot slot)
            : base(slot)
        {
        }

        public override void Initialize(ComboProvider combo)
        {
            //OnlyUpdateIfTargetValid = false;
            SetSkillshot(0.50f, 1500f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            base.Initialize(combo);
        }

        public override void Execute(Obj_AI_Hero target)
        {
            if (Kalista.Soulbound != null && !Kalista.Soulbound.IsDead && Kalista.Soulbound.Position.Distance(ObjectManager.Player.Position, true) < RangeSqr)
            {
                if (AllyHealth > Kalista.Soulbound.HealthPercent && !Kalista.Soulbound.IsRecalling() || MyHealth > ObjectManager.Player.HealthPercent) //  || GetHitCount(MinComboHitchance) >= HitCount
                    Cast(Kalista.Soulbound);

                if (Kalista.Soulbound.ChampionName == "Blitzcrank" && Balista)
                {
                    var dst = Kalista.Soulbound.Distance(ObjectManager.Player, true);
                    if (dst < BalistaDistance * BalistaDistance) return;

                    foreach (var enemy in HeroManager.Enemies)
                    {
                        var buff = enemy.GetBuff("rocketgrab2");
                        if (buff != null && buff.IsActive)
                        {
                            if (enemy.Distance(ObjectManager.Player, true) > dst)
                                Cast(Kalista.Soulbound);
                            return;
                        }
                    }
                }
            }
        }

        public override float GetDamage(Obj_AI_Hero enemy)
        {
            return 0;//return base.GetDamage(enemy);
        }

        public override int GetPriority()
        {
            return 3;
        }
    }
}
