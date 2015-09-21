using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Talon.Helper
{
    internal class Cast
    {
        private static readonly Dictionary<SpellSlot, Spell> Spell = Spells.Spell;
        public static void W(Obj_AI_Base unit, Vector2 unitPosition,bool packetcast, int minTargets = 0)
        {
            var points = new List<Vector2>();
            var hitBoxes = new List<int>();

            var startPoint = ObjectManager.Player.ServerPosition.To2D();
            var originalDirection = Spell[SpellSlot.W].Range * (unitPosition - startPoint).Normalized();

            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (enemy.IsValidTarget() && enemy.NetworkId != unit.NetworkId)
                {
                    var pos = Spell[SpellSlot.W].GetPrediction(enemy);
                    if (pos.Hitchance >= HitChance.Medium)
                    {
                        points.Add(pos.UnitPosition.To2D());
                        hitBoxes.Add((int)enemy.BoundingRadius);
                    }
                }
            }


            var posiblePositions = new List<Vector2>();

            for (var i = 0; i < 3; i++)
            {
                if (i == 0) posiblePositions.Add(unitPosition + originalDirection.Rotated(0));
                if (i == 1) posiblePositions.Add(startPoint + originalDirection.Rotated(Spells.Wangle));
                if (i == 2) posiblePositions.Add(startPoint + originalDirection.Rotated(-Spells.Wangle));
            }


            if (startPoint.Distance(unitPosition) < 900)
            {
                for (var i = 0; i < 3; i++)
                {
                    var pos = posiblePositions[i];
                    var direction = (pos - startPoint).Normalized().Perpendicular();
                    var k = (2f / 3 * (unit.BoundingRadius + Spell[SpellSlot.W].Width));
                    posiblePositions.Add(startPoint - k * direction);
                    posiblePositions.Add(startPoint + k * direction);
                }
            }

            var bestPosition = new Vector2();
            var bestHit = -1;

            foreach (var position in posiblePositions)
            {
                var hits = CountHits(position, points, hitBoxes);
                if (hits > bestHit)
                {
                    bestPosition = position;
                    bestHit = hits;
                }
            }

            if (bestHit + 1 <= minTargets)
                return;

            Spell[SpellSlot.W].Cast(bestPosition.To3D(), packetcast);
        }
        private static int CountHits(Vector2 position, List<Vector2> points, List<int> hitBoxes)
        {
            var result = 0;

            var startPoint = ObjectManager.Player.ServerPosition.To2D();
            var originalDirection = Spell[SpellSlot.W].Range * (position - startPoint).Normalized();
            var originalEndPoint = startPoint + originalDirection;

            for (var i = 0; i < points.Count; i++)
            {
                var point = points[i];

                for (var k = 0; k < 3; k++)
                {
                    var endPoint = new Vector2();
                    if (k == 0) endPoint = originalEndPoint;
                    if (k == 1) endPoint = startPoint + originalDirection.Rotated(Spells.Wangle);
                    if (k == 2) endPoint = startPoint + originalDirection.Rotated(Spells.Wangle);

                    if (point.Distance(startPoint, endPoint, true, true) <
                        (Spell[SpellSlot.W].Width + hitBoxes[i]) * (Spell[SpellSlot.W].Width + hitBoxes[i]))
                    {
                        result++;
                        break;
                    }
                }
            }

            return result;
        }
    }
}
