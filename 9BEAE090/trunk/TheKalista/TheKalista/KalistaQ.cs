using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using TheKalista.Commons;
using TheKalista.Commons.ComboSystem;
using Color = System.Drawing.Color;

namespace TheKalista
{
    class KalistaQ : Skill
    {
        public int MinLaneclear;

        public KalistaQ(SpellSlot slot, float range, TargetSelector.DamageType damageType)
            : base(slot, range, damageType)
        {
        }

        public KalistaQ(SpellSlot slot)
            : base(slot)
        {
        }

        public override void Initialize(ComboProvider combo)
        {
            SetSkillshot(0.25f, 40f, 1200f, true, SkillshotType.SkillshotLine); // orig width : 40

            base.Initialize(combo);
        }

        public override void Execute(Obj_AI_Hero target)
        {
            if (!ObjectManager.Player.IsDashing() && !ObjectManager.Player.IsWindingUp)
            {
                var pred = GetPrediction(target, collisionable: new[] { CollisionableObjects.Minions, CollisionableObjects.YasuoWall });
                if (pred.Hitchance >= (Provider.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo ? MinComboHitchance : MinHarassHitchance))
                {
                    Cast(pred.CastPosition);
                }
                //else if( && 
                else if (pred.Hitchance == HitChance.Collision && pred.UnitPosition.Distance(ObjectManager.Player.Position, true) - target.BoundingRadius * target.BoundingRadius < RangeSqr  && pred.CollisionObjects.All(obj => IsKillable(obj)))
                    Cast(pred.CastPosition);
            }
        }

        //public override void Draw()
        //{
        //    if (!Provider.Target.IsValidTarget()) return;
        //    var pred = GetPrediction(Provider.Target, collisionable: new[] { CollisionableObjects.Minions, CollisionableObjects.YasuoWall });
        //    if (pred.Hitchance >= MinComboHitchance)
        //        Render.Circle.DrawCircle(Provider.Target.Position, 150, Color.Green);
        //    else if (pred.Hitchance == HitChance.Collision)
        //    {
        //        Render.Circle.DrawCircle(Provider.Target.Position, 150, Color.Yellow);
        //    }
        //    else
        //        Render.Circle.DrawCircle(Provider.Target.Position, 150, Color.Red);
        //    var prevPos = ObjectManager.Player.Position;

            
        //    foreach (var collisionObject in pred.CollisionObjects.OrderBy(obj => obj.Position.Distance(ObjectManager.Player.Position, true)))
        //    {

        //        Render.Circle.DrawCircle(collisionObject.Position, 100, IsKillable(collisionObject) ? Color.Green : Color.Red);
        //        if (prevPos.Distance(collisionObject.Position, true) > 200 * 200)
        //            Drawing.DrawLine(Drawing.WorldToScreen(prevPos.Extend(collisionObject.Position, 100)), Drawing.WorldToScreen(collisionObject.Position.Extend(prevPos, 100)), 5, Color.Yellow);
        //        prevPos = collisionObject.Position;
        //    }
        //    Drawing.DrawLine(Drawing.WorldToScreen(prevPos.Extend(Provider.Target.Position, 100)), Drawing.WorldToScreen(Provider.Target.Position.Extend(prevPos, 100)), 5, Color.Yellow);
        //}

        public override void LaneClear()
        {
            if (TickLimiter.Limit(100, 1))
            {
                var minions = MinionManager.GetMinions(Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.None);
                foreach (var minion in minions.Where(x => IsKillable(x)))
                {
                    var killcount = 0;

                    foreach (var colminion in GetCollidingMinions(ObjectManager.Player, ObjectManager.Player.Position.Extend(minion.Position, Range)))
                    {
                        if (IsKillable(colminion))
                            killcount++;
                        else
                            break;
                    }

                    if (killcount >= MinLaneclear && !ObjectManager.Player.IsWindingUp && !ObjectManager.Player.IsDashing())
                    {
                        Cast(minion.ServerPosition);
                        break;
                    }
                }

                //var farmPos = MinionManager.GetBestLineFarmLocation(MinionManager.GetMinionsPredictedPositions(MinionManager.GetMinions(Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.None), Delay, Width, Speed, ObjectManager.Player.Position, Range, true, SkillshotType.SkillshotLine), Width, Range);
                //if (farmPos.MinionsHit >= MinLaneclear)
                //    Cast(farmPos.Position);
            }
        }

        /// <summary>
        /// Author: JQuery!
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        private IEnumerable<Obj_AI_Base> GetCollidingMinions(Obj_AI_Base source, Vector3 pos)
        {
            return LeagueSharp.Common.Collision.GetCollision(new List<Vector3> { pos }, new PredictionInput
            {
                Unit = source,
                Radius = Width,
                Delay = Delay,
                Speed = Speed
            }).OrderBy(obj => obj.Distance(source));
        }

        public override int GetPriority()
        {
            return 2;
        }
    }
}
