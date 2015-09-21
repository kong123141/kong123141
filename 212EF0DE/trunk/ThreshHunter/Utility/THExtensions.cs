using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ThreshHunter.Utility
{
    internal static class THExtensions
    {


        public static bool IsEnabledAndReady(this Spell spell, Orbwalking.OrbwalkingMode mode)
        {
            var modeString = mode.ToString().ToLowerInvariant();
            var EnabledInMenu =
                GetItemValue<bool>(string.Format("dz191.thresh.{0}.use{1}", modeString, spell.Slot.GetStringFromSlot()));
            var Ready = spell.IsReady();

            return EnabledInMenu && Ready;
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

        public static T GetItemValue<T>(string item)
        {
            return Thresh.RootMenu.Item(item).GetValue<T>();
        }

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
                var nearestAllyTurret = ObjectManager.Get<Obj_AI_Turret>().Where(h => h.IsAlly).OrderBy(d => d.Distance(Position, true)).FirstOrDefault();

                if (nearestAllyTurret != null)
                {
                    ////We're adding more allies, since the turret adds to the firepower of the team.
                    allies += 2;
                }
            }

            ////Adding 1 for my Player
            return (allies + 1 > enemies - lhEnemies);
        }

        public static List<Obj_AI_Hero> GetLhEnemiesNear(this Vector3 position, float range, float Healthpercent)
        {
            return HeroManager.Enemies.Where(hero => hero.IsValidTarget(range, true, position) && hero.HealthPercent <= Healthpercent).ToList();
        }

        public static bool UnderAllyTurret(this Vector3 Position)
        {
            return ObjectManager.Get<Obj_AI_Turret>().Any(t => t.IsAlly && !t.IsDead);
        }

    }
}
