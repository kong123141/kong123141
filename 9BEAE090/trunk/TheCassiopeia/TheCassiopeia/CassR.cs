using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using TheCassiopeia.Commons;
using TheCassiopeia.Commons.ComboSystem;
using Color = System.Drawing.Color;

namespace TheCassiopeia
{
    class CassR : Skill
    {
        public bool UltOnKillable;
        public int MinTargetsFacing;
        public int MinTargetsNotFacing;
        public int GapcloserUltHp;
        public int MinHealth;
        public int PanicModeHealth;
        public MenuItem BurstMode;
        public bool MinEnemiesOnlyInCombo;

        public CassR(SpellSlot slot)
            : base(slot)
        {
            Range = 825f;
        }


        public override void Initialize(ComboProvider combo)
        {
            
            SetSkillshot(0.3f, (float)(80 * Math.PI / 180), float.MaxValue, false, SkillshotType.SkillshotCone);
            base.Initialize(combo);
        }

        public Commons.Prediction.PredictionOutput GetPrediction(Obj_AI_Hero target)
        {
            return Commons.Prediction.Prediction.GetPrediction(new Commons.Prediction.PredictionInput() { Aoe = true, Collision = false, Delay = Delay, From = ObjectManager.Player.ServerPosition, Radius = (float)(80 * Math.PI / 180), Range = 825f, Type = Commons.Prediction.SkillshotType.SkillshotCone, Unit = target, RangeCheckFrom = ObjectManager.Player.ServerPosition });
        }

        public override void Update(Orbwalking.OrbwalkingMode mode, ComboProvider combo, Obj_AI_Hero target)
        {
            if (!MinEnemiesOnlyInCombo && CanBeCast())
            {
                var pred = GetPrediction(target);
                if (pred.Hitchance < Commons.Prediction.HitChance.Low) return;

                var targets = HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(Range) && WillHit(enemy.Position, pred.CastPosition));
                var looking = targets.Count(trgt => trgt.IsFacingMe());
                if (looking >= MinTargetsFacing || targets.Count() >= MinTargetsNotFacing)
                    Cast(pred.CastPosition);

            }

            base.Update(mode, combo, target);
        }

        public override void Execute(Obj_AI_Hero target)
        {

            var pred = GetPrediction(target);
            if (pred.Hitchance < Commons.Prediction.HitChance.Low) return;

            var targets = HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(Range) && WillHit(enemy.Position, pred.CastPosition));
            var looking = targets.Count(trgt => trgt.IsFacingMe());

            if (looking >= MinTargetsFacing || targets.Count() >= MinTargetsNotFacing || UltOnKillable && Provider.GetComboDamage(target) > target.Health && target.IsFacingMe() && target.HealthPercent > MinHealth && target.IsValidTarget(Range) || PanicModeHealth > ObjectManager.Player.HealthPercent || BurstMode.IsActive())
            {
                Cast(pred.CastPosition);
            }
        }

        public override void Gapcloser(ComboProvider combo, ActiveGapcloser gapcloser)
        {
            if (ObjectManager.Player.HealthPercent < GapcloserUltHp && gapcloser.Sender.IsValidTarget(Range))
            {
                var pred = GetPrediction(gapcloser.Sender);
                if (pred.Hitchance < Commons.Prediction.HitChance.Low) return;

                Cast(pred.CastPosition);
            }
        }

        public override int GetPriority()
        {
            return 4;
        }
    }
}
