using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
// ReSharper disable All

namespace ShrekSai.Utility
{
    static class Helpers
    {
        public static bool  IsPlayerBurrowed()
        {
            return ObjectManager.Player.HasBuff("RekSaiW"); //TODO Gotta Check this
        }

        public static bool IsFuryFull()
        {
            return ObjectManager.Player.ManaPercent > 99f; //Fuck you too Resharper
        }

        public static bool IsKnockedUp(this Obj_AI_Hero target)
        {
            return target.HasBuff("RekSaiKnockupImmune"); //TODO Gotta Check this
        }

        public static float GetKnockupEndTime(this Obj_AI_Hero target)
        {
            return (Game.Time - target.GetBuff("RekSaiKnockupImmune").EndTime) != 0f ? Game.Time - target.GetBuff("RekSaiKnockupImmune").EndTime : 0f; //TODO Gotta Check this
        }

        public static bool CanKnockup(this Obj_AI_Hero target)
        {
            return !target.IsKnockedUp() || target.GetKnockupEndTime() <= 0;
        }
    }
}
