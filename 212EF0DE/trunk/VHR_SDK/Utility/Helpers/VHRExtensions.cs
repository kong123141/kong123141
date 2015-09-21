namespace VHR_SDK.Utility.Helpers
{
    #region
    using System.Collections.Generic;
    using System.Linq;
    using LeagueSharp;
    using LeagueSharp.SDK.Core;
    using LeagueSharp.SDK.Core.Enumerations;
    using LeagueSharp.SDK.Core.Extensions;
    using LeagueSharp.SDK.Core.Extensions.SharpDX;
    using LeagueSharp.SDK.Core.UI.IMenu.Values;
    using LeagueSharp.SDK.Core.Wrappers;
    using SharpDX;
    using System;
    using LeagueSharp.SDK.Core.Utils;
    #endregion

    static class VHRExtensions
    {
        #region Fields
        private static IEnumerable<Obj_AI_Hero> EnemiesClose
        {
            get
            {
                return
                    GameObjects.EnemyHeroes.Where(
                        m =>
                            m.DistanceSquared(ObjectManager.Player) <= Math.Pow(1500, 2) && m.IsValidTarget(1500, false) &&
                            m.CountEnemiesInRange(m.IsMelee() ? m.AttackRange * 1.5f : m.AttackRange + 20 * 1.5f) > 0);
            }
        }

        public static IEnumerable<Obj_AI_Hero> MeleeEnemiesTowardsMe
        {
            get
            {
                return
                    GameObjects.EnemyHeroes.Where(
                        m => m.IsMelee && m.DistanceSquared(ObjectManager.Player) <= m.AttackRange * m.AttackRange
                            && (m.ServerPosition.ToVector2() + (m.BoundingRadius + 25f) * m.Direction.ToVector2().Perpendicular()).DistanceSquared(ObjectManager.Player.ServerPosition.ToVector2()) <= m.ServerPosition.DistanceSquared(ObjectManager.Player.ServerPosition)
                            && m.IsValidTarget(1500));
            }
        }
        #endregion

        #region Unit Extensions
        public static bool Has2WStacks(this Obj_AI_Hero target)
        {
            return target.GetBuffCount("vaynesilvereddebuff") == 2;
        }

        public static Obj_AI_Hero GetHeroWith2WStacks()
        {
            return GameObjects.EnemyHeroes.FirstOrDefault(target => target.IsValidTarget(ObjectManager.Player.AttackRange + 65f) && target.GetBuffCount("vaynesilvereddebuff") == 2);
        }

        #endregion

        #region Positional Helpers

        public static bool IsSafePosition(this Vector3 Position, bool considerAllyTurrets = true, bool considerLHEnemies = true)
        {
            if (Position.UnderTurret(true) && !ObjectManager.Player.UnderTurret(true))
            {
                return false;
            }

            var allies = Position.CountAlliesInRange(ObjectManager.Player.AttackRange);
            var enemies = Position.CountEnemiesInRange(ObjectManager.Player.AttackRange);
            var lhEnemies = considerLHEnemies ? Position.GetLhEnemiesNear(ObjectManager.Player.AttackRange, 15).Count() : 0;

            if (enemies <= 1) ////It's a 1v1, safe to assume I can Q
            {
                return true;
            }

            if (Position.UnderAllyTurret())
            {
                var nearestAllyTurret = GameObjects.AllyTurrets.OrderBy(d => d.DistanceSquared(Position)).FirstOrDefault();

                if (nearestAllyTurret != null)
                {
                    ////We're adding more allies, since the turret adds to the firepower of the team.
                    allies += 2;
                }
            }

            ////Adding 1 for my Player
            return (allies + 1 > enemies - lhEnemies);
        }

        /// <summary>
        /// Returns if this passes the 'No Q into enemies mode'
        /// </summary>
        /// <param name="Position">The Position</param>
        /// <returns></returns>
        public static bool PassesNoQIntoEnemiesCheck(this Vector3 Position)
        {
            if (!VHR.VHRMenu["dz191.vhr.misc"]["dz191.vhr.misc.tumble"]["noqenemies"].GetValue<MenuBool>().Value || VHR.VHRMenu["dz191.vhr.misc"]["dz191.vhr.misc.tumble"]["qspam"].GetValue<MenuBool>().Value)
            {
                return true;
            }

            if (!VHR.VHRMenu["dz191.vhr.misc"]["dz191.vhr.misc.general"]["lightweight"].GetValue<MenuBool>().Value)
            {
                var Vector2Position = Position.ToVector2();
                if (EnemiesClose.Count() <= 1)
                {
                    return true;
                }

                if (GetEnemyPoints().Contains(Vector2Position) &&
                    !VHR.VHRMenu["dz191.vhr.misc"]["dz191.vhr.misc.tumble"]["qspam"].GetValue<MenuBool>().Value)
                {
                    return false;
                }
                return true;
            }

            var closeEnemies = GameObjects.EnemyHeroes.Where(en => en.IsValidTarget(1500f)).OrderBy(en => en.Distance(Position));
            if (closeEnemies.Any())
            {
                if (closeEnemies.Count() <= 1)
                {
                    return true;
                }

                return closeEnemies.All(enemy => Position.CountEnemiesInRange(VHR.VHRMenu["dz191.vhr.misc"]["dz191.vhr.misc.tumble"]["dynamicqsafety"].GetValue<MenuBool>().Value ? enemy.AttackRange : 405f) < 1);
            }
            return true;
        }
        
        #endregion

        #region Positional Geometry
        public static List<Vector2> GetEnemyPoints()
        {
            var polygonsList = EnemiesClose.Select(enemy => new Geometry.Circle(enemy.ServerPosition.ToVector2(), (enemy.IsMelee ? enemy.AttackRange * 1.5f : enemy.AttackRange) + enemy.BoundingRadius + 20).ToPolygon()).ToList();
            var pathList = Geometry.ClipPolygons(polygonsList);
            var pointList = pathList.SelectMany(path => path, (path, point) => new Vector2(point.X, point.Y)).Where(currentPoint => !currentPoint.IsWall()).ToList();
            return pointList;
        }

        
        public static float GetRealAutoAttackRange(Obj_AI_Hero Attacker, AttackableUnit target)
        {
            var result = Attacker.AttackRange + Attacker.BoundingRadius;
            if (target.IsValidTarget())
            {
                return result + target.BoundingRadius;
            }
            return result;
        }

        #endregion

        #region Old Common/VHR Extensions

        public static List<Obj_AI_Hero> GetLhEnemiesNear(this Vector3 position, float range, float Healthpercent)
        {
            return GameObjects.EnemyHeroes.Where(hero => hero.IsValidTarget(range, true, position) && hero.HealthPercent <= Healthpercent).ToList();
        }

        public static bool UnderAllyTurret(this Vector3 Position)
        {
            return ObjectManager.Get<Obj_AI_Turret>().Any(t => t.IsAlly && !t.IsDead);
        }


        public static int CountEnemiesInRange(float range)
        {
            return ObjectManager.Player.CountEnemiesInRange(range);
        }

        /// <summary>
        ///     Counts the enemies in range of Unit.
        /// </summary>
        public static int CountEnemiesInRange(this Obj_AI_Base unit, float range)
        {
            return unit.ServerPosition.CountEnemiesInRange(range);
        }

        /// <summary>
        ///     Counts the enemies in range of point.
        /// </summary>
        public static int CountEnemiesInRange(this Vector3 point, float range)
        {
            return GameObjects.EnemyHeroes.Count(h => h.IsValidTarget(range, true, point));
        }

        // Use same interface as CountEnemiesInRange
        /// <summary>
        ///     Count the allies in range of the Player.
        /// </summary>
        public static int CountAlliesInRange(float range)
        {
            return ObjectManager.Player.CountAlliesInRange(range);
        }

        /// <summary>
        ///     Counts the allies in range of the Unit.
        /// </summary>
        public static int CountAlliesInRange(this Obj_AI_Base unit, float range)
        {
            return unit.ServerPosition.CountAlliesInRange(range);
        }

        /// <summary>
        ///     Counts the allies in the range of the Point.
        /// </summary>
        public static int CountAlliesInRange(this Vector3 point, float range)
        {
            return GameObjects.AllyHeroes
                .Count(x => x.IsValidTarget(range, false, point));
        }

        public static List<Obj_AI_Hero> GetAlliesInRange(this Obj_AI_Base unit, float range)
        {
            return GetAlliesInRange(unit.ServerPosition, range);
        }

        public static List<Obj_AI_Hero> GetAlliesInRange(this Vector3 point, float range)
        {
            return
                GameObjects.AllyHeroes
                    .Where(x => point.DistanceSquared(x.ServerPosition) <= range * range).ToList();
        }

        public static List<Obj_AI_Hero> GetEnemiesInRange(this Obj_AI_Base unit, float range)
        {
            return GetEnemiesInRange(unit.ServerPosition, range);
        }

        public static List<Obj_AI_Hero> GetEnemiesInRange(this Vector3 point, float range)
        {
            return
                GameObjects.EnemyHeroes
                    .Where(x => point.DistanceSquared(x.ServerPosition) <= range * range).ToList();
        }

        /// <summary>
        ///     Returns true if the unit is under tower range.
        /// </summary>
        public static bool UnderTurret(this Obj_AI_Base unit)
        {
            return UnderTurret(unit.Position, true);
        }

        /// <summary>
        ///     Returns true if the unit is under turret range.
        /// </summary>
        public static bool UnderTurret(this Obj_AI_Base unit, bool enemyTurretsOnly)
        {
            return UnderTurret(unit.Position, enemyTurretsOnly);
        }

        public static bool UnderTurret(this Vector3 position, bool enemyTurretsOnly)
        {
            return
                ObjectManager.Get<Obj_AI_Turret>().Any(turret => turret.IsValidTarget(950, enemyTurretsOnly, position));
        }
        #endregion

        #region Menu
        public static bool IsEnabledAndReady(this Spell spell, OrbwalkerMode mode)
        {
            var modeString = mode.ToString().ToLowerInvariant();
            var EnabledInMenu =
                VHR.VHRMenu[string.Format("dz191.vhr.{0}", modeString)][
                    string.Format("Use{0}", spell.Slot.GetStringFromSlot())].GetValue<MenuBool>().Value;

            var Ready = spell.IsReady();

            var ManaManagerCheck = ObjectManager.Player.ManaPercent >= VHR.VHRMenu[string.Format("dz191.vhr.{0}", modeString)][
                    string.Format("{0}Mana", spell.Slot.GetStringFromSlot())].GetValue<MenuSlider>().Value;

            return EnabledInMenu && Ready && ManaManagerCheck;
        }

        public static bool IsRunningAway(this Obj_AI_Hero target)
        {
            var runningDirection = (target.Position.ToVector2() +
             200 * target.Direction.Perpendicular().ToVector2()).ToVector3();
            if (ObjectManager.Player.ServerPosition.DistanceSquared(runningDirection) >
                ObjectManager.Player.ServerPosition.DistanceSquared(target.ServerPosition)
                && target.ServerPosition.DistanceSquared(runningDirection) > 200*200  &&
                !target.IsFacing(ObjectManager.Player))
            {
                return true;
            }
            return false;
        }

        public static string GetStringFromSlot(this SpellSlot slot)
        {
            switch (slot)
            {
                case SpellSlot.Q:
                    return "Q";
                case SpellSlot.E:
                    return "E";
                case SpellSlot.R:
                    return "R";
                case SpellSlot.Trinket:
                    return "Trinket";
                default:
                    return "Unk";
            }
        }
        #endregion
    }
}
