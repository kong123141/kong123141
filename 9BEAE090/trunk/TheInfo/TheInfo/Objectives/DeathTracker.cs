using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace TheInfo.Objectives
{
    class DeathTracker
    {
        private readonly Obj_AI_Hero[] _tracking;
        private float _lastTick;
        public float[] RespawnTimes;

        public DeathTracker(GameObjectTeam team)
        {
            _tracking = HeroManager.AllHeroes.Where(hero => hero.Team == team).ToArray();
            RespawnTimes = new float[_tracking.Length];
            Game.OnUpdate += Game_OnUpdate;
        }

        void Game_OnUpdate(EventArgs args)
        {
            if(_lastTick > Game.Time)
            return;

            _lastTick = Game.Time + 0.5f; // exec below code once every 0.5 sec

            for (int i = 0; i < _tracking.Length; i++)
                if (_tracking[i].IsDead && RespawnTimes[i] < Game.Time)
                    RespawnTimes[i] = _tracking[i].DeathDuration + Game.Time;
        }

        public float GetTimeWhenAlive(int count)
        {
            if(count == 0 && RespawnTimes.All(time => time > Game.Time))
                return Game.Time;
            if (count < 0 || count > RespawnTimes.Length)
                return 0;

            return RespawnTimes.OrderBy(item => item).ToArray()[count - 1];
        }

        public int GetAliveCount(float time)
        {
            return RespawnTimes.Count(value => value < time);
        }
    }
}
