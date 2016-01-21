﻿namespace Flowers_Utility.Common.Evade
{
    using System.Collections.Generic;
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;

    public static class Evader
    {
        public static List<Vector2> GetEvadePoints(int speed = -1, int delay = 0, bool isBlink = false, bool onlyGood = false)
        {
            speed = speed == -1 ? (int)ObjectManager.Player.MoveSpeed : speed;

            var goodCandidates = new List<Vector2>();
            var badCandidates = new List<Vector2>();

            var polygonList = new List<Geometry.Polygon>();

            var takeClosestPath = false;

            foreach (var skillshot in Pluging.Evade.DetectedSkillshots)
            {
                if (skillshot.Evade())
                {
                    if (skillshot.SpellData.TakeClosestPath && skillshot.IsDanger(Pluging.Evade.PlayerPosition))
                        takeClosestPath = true;

                    polygonList.Add(skillshot.EvadePolygon);
                }
            }

            var dangerPolygons = Geometry.ClipPolygons(polygonList).ToPolygons();
            var myPosition = Pluging.Evade.PlayerPosition;

            foreach (var poly in dangerPolygons)
            {
                for (var i = 0; i <= poly.Points.Count - 1; i++)
                {
                    var sideStart = poly.Points[i];
                    var sideEnd = poly.Points[(i == poly.Points.Count - 1) ? 0 : i + 1];
                    var originalCandidate = myPosition.ProjectOn(sideStart, sideEnd).SegmentPoint;
                    var distanceToEvadePoint = Vector2.DistanceSquared(originalCandidate, myPosition);

                    if (distanceToEvadePoint < 600 * 600)
                    {
                        var sideDistance = Vector2.DistanceSquared(sideEnd, sideStart);
                        var direction = (sideEnd - sideStart).Normalized();
                        var s = (distanceToEvadePoint < 200 * 200 && sideDistance > 90 * 90) ? Config.DiagonalEvadePointsCount : 0;

                        for (var j = -s; j <= s; j++)
                        {
                            var candidate = originalCandidate + j * Config.DiagonalEvadePointsStep * direction;
                            var pathToPoint = ObjectManager.Player.GetPath(candidate.To3D()).To2DList();

                            if (!isBlink)
                            {
                                if (Pluging.Evade.IsSafePath(pathToPoint, Config.EvadingFirstTimeOffset, speed, delay).IsSafe)
                                    goodCandidates.Add(candidate);

                                if (Pluging.Evade.IsSafePath(pathToPoint, Config.EvadingSecondTimeOffset, speed, delay).IsSafe && j == 0)
                                    badCandidates.Add(candidate);
                            }
                            else
                            {
                                if (Pluging.Evade.IsSafeToBlink(pathToPoint[pathToPoint.Count - 1], Config.EvadingFirstTimeOffset, delay))
                                    goodCandidates.Add(candidate);

                                if (Pluging.Evade.IsSafeToBlink(pathToPoint[pathToPoint.Count - 1], Config.EvadingSecondTimeOffset, delay))
                                    badCandidates.Add(candidate);
                            }
                        }
                    }
                }
            }

            if (takeClosestPath)
            {
                if (goodCandidates.Count > 0)
                {
                    goodCandidates = new List<Vector2>
                    {
                        goodCandidates.MinOrDefault(vector2 => ObjectManager.Player.Distance(vector2, true))
                    };
                }

                if (badCandidates.Count > 0)
                {
                    badCandidates = new List<Vector2>
                    {
                        badCandidates.MinOrDefault(vector2 => ObjectManager.Player.Distance(vector2, true))
                    };
                }
            }

            return (goodCandidates.Count > 0) ? goodCandidates : (onlyGood ? new List<Vector2>() : badCandidates);
        }

        public static Vector2 GetClosestOutsidePoint(Vector2 from, List<Geometry.Polygon> polygons)
        {
            var result = new List<Vector2>();

            foreach (var poly in polygons)
            {
                for (var i = 0; i <= poly.Points.Count - 1; i++)
                {
                    var sideStart = poly.Points[i];
                    var sideEnd = poly.Points[(i == poly.Points.Count - 1) ? 0 : i + 1];

                    result.Add(from.ProjectOn(sideStart, sideEnd).SegmentPoint);
                }
            }
            return result.MinOrDefault(vector2 => vector2.Distance(from));
        }

        public static List<Obj_AI_Base> GetEvadeTargets(SpellValidTargets[] validTargets, int speed, int delay, float range, bool isBlink = false, bool onlyGood = false, bool DontCheckForSafety = false)
        {
            var badTargets = new List<Obj_AI_Base>();
            var goodTargets = new List<Obj_AI_Base>();
            var allTargets = new List<Obj_AI_Base>();

            foreach (var targetType in validTargets)
            {
                switch (targetType)
                {
                    case SpellValidTargets.AllyChampions:
                        foreach (var ally in ObjectManager.Get<Obj_AI_Hero>())
                            if (ally.IsValidTarget(range, false) && !ally.IsMe && ally.IsAlly)
                                allTargets.Add(ally);
                        break;

                    case SpellValidTargets.AllyMinions:
                        allTargets.AddRange(MinionManager.GetMinions(ObjectManager.Player.Position, range, MinionTypes.All, MinionTeam.Ally));
                        break;

                    case SpellValidTargets.AllyWards:
                        foreach (var gameObject in ObjectManager.Get<Obj_AI_Minion>())
                            if (gameObject.Name.ToLower().Contains("ward") && gameObject.IsValidTarget(range, false) && gameObject.Team == ObjectManager.Player.Team)
                                allTargets.Add(gameObject);
                        break;

                    case SpellValidTargets.EnemyChampions:
                        foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>())
                            if (enemy.IsValidTarget(range))
                                allTargets.Add(enemy);
                        break;

                    case SpellValidTargets.EnemyMinions:
                        allTargets.AddRange(MinionManager.GetMinions(ObjectManager.Player.Position, range, MinionTypes.All, MinionTeam.NotAlly));
                        break;

                    case SpellValidTargets.EnemyWards:
                        foreach (var gameObject in ObjectManager.Get<Obj_AI_Minion>())
                            if (gameObject.Name.ToLower().Contains("ward") && gameObject.IsValidTarget(range))
                                allTargets.Add(gameObject);
                        break;
                }
            }

            foreach (var target in allTargets)
            {
                if (DontCheckForSafety || Pluging.Evade.IsSafe(target.ServerPosition.To2D()).IsSafe)
                {
                    if (isBlink)
                    {
                        if (Utils.TickCount - Pluging.Evade.LastWardJumpAttempt < 250 || Pluging.Evade.IsSafeToBlink(target.ServerPosition.To2D(), Config.EvadingFirstTimeOffset, delay))
                            goodTargets.Add(target);

                        if (Utils.TickCount - Pluging.Evade.LastWardJumpAttempt < 250 || Pluging.Evade.IsSafeToBlink(target.ServerPosition.To2D(), Config.EvadingSecondTimeOffset, delay))
                            badTargets.Add(target);
                    }
                    else
                    {
                        var pathToTarget = new List<Vector2>();

                        pathToTarget.Add(Pluging.Evade.PlayerPosition);
                        pathToTarget.Add(target.ServerPosition.To2D());

                        if (Utils.TickCount - Pluging.Evade.LastWardJumpAttempt < 250 || Pluging.Evade.IsSafePath(pathToTarget, Config.EvadingFirstTimeOffset, speed, delay).IsSafe)
                            goodTargets.Add(target);

                        if (Utils.TickCount - Pluging.Evade.LastWardJumpAttempt < 250 || Pluging.Evade.IsSafePath(pathToTarget, Config.EvadingSecondTimeOffset, speed, delay).IsSafe)
                            badTargets.Add(target);
                    }
                }
            }

            return (goodTargets.Count > 0) ? goodTargets : (onlyGood ? new List<Obj_AI_Base>() : badTargets);
        }
    }
}
