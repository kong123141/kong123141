using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.SDK.Core.Enumerations;
using LeagueSharp.SDK.Core.Wrappers;
using SharpDX;

namespace VHR_SDK.Utility.Helpers
{
    class VHRHelpers
    {
        public static float LastMoveC;

        public static bool IsSummonersRift()
        {
            var map = Map.GetMap();
            if (map != null && map.Type == MapType.SummonersRift)
            {
                return true;
            }
            return false;
        }

        public static void MoveToLimited(Vector3 where)
        {
            if (Environment.TickCount - LastMoveC < 80)
            {
                return;
            }

            LastMoveC = Environment.TickCount;

            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, where);
        }
    }
}
