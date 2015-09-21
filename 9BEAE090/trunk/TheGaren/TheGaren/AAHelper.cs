using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace TheGaren
{
    // ReSharper disable once InconsistentNaming
    static class AAHelper
    {
        public static bool JustFinishedAutoattack;
        public static bool WillAutoattackSoon;
        private static float _lastAaTime;
        private static bool _windingUp;
        private static readonly Obj_AI_Hero Player;

        static AAHelper()
        {
            Player = ObjectManager.Player;
            Game.OnUpdate += Update;
        }

        private static void Update(EventArgs args)
        {
            JustFinishedAutoattack = _windingUp && !Player.IsWindingUp;
            if (JustFinishedAutoattack)
                _lastAaTime = Game.Time;
            WillAutoattackSoon = (_windingUp = Player.IsWindingUp) || Game.Time - _lastAaTime > Player.AttackDelay / 2f;
        }
    }
}
