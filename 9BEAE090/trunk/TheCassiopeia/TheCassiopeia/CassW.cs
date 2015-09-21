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
    class CassW: Skill
    {
        private CassQ _q;
        private CassR _r;
        public bool UseOnGapcloser;

        public CassW(SpellSlot slot)
            : base(slot)
        {
            SetSkillshot(0.5f, Instance.SData.CastRadius, Instance.SData.MissileSpeed, false, SkillshotType.SkillshotCircle);
            Range = 850;
            HarassEnabled = false;
        }

        public override void Initialize(ComboProvider combo)
        {
            _q = combo.GetSkill<CassQ>();
            _r = combo.GetSkill<CassR>();
            base.Initialize(combo);
        }

        public override void Execute(Obj_AI_Hero target)
        {
            if (_q.OnCooldown() && (!target.IsPoisoned() && !Provider.IsMarked(target)))
            {
                Cast(target);
            }
        }

        public override void Gapcloser(ComboProvider combo, ActiveGapcloser gapcloser)
        {
            if (UseOnGapcloser && (!_r.CanBeCast() || _r.GapcloserUltHp < ObjectManager.Player.HealthPercent))
            {
                Cast(gapcloser.Sender);
            }
        }

        public override float GetDamage(Obj_AI_Hero enemy)
        {
            return 0;
        }

        public override void LaneClear()
        {
            var minions = MinionManager.GetMinions(900, MinionTypes.All, MinionTeam.NotAlly);
            if(!_q.OnCooldown() || minions.Any(minion => minion.IsPoisoned())) return;
            var farmLocation = MinionManager.GetBestCircularFarmLocation(minions.Select(minion => minion.Position.To2D()).ToList(), Instance.SData.CastRadius, 850);

            if (farmLocation.MinionsHit > 0)
            {
                Cast(farmLocation.Position);
            }
            base.LaneClear();
        }

        public override int GetPriority()
        {
            return 1;
        }
    }
}
