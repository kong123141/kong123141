using System;
using System.Linq;
using LeagueSharp;
using TheInfo.Objectives.Items;

namespace TheInfo.Objectives
{
    class InhibitorTracker
    {
        public readonly float[] RespawnTime;
        private readonly ObjectiveInhibitor[] _inhib;
        private float _tickTime;

        public InhibitorTracker(ObjectiveInhibitor[] inhib)
        {
            _inhib = inhib;
            RespawnTime = new float[inhib.Length];
            Game.OnUpdate += Game_OnUpdate;
        }

        void Game_OnUpdate(EventArgs args)
        {
            if (_tickTime > Game.Time)
                return;
            _tickTime = Game.Time + 1f; // the below code only gets executed once per second, which is enough

            for (int i = 0; i < _inhib.Length; i++)
            {
                if (RespawnTime[i] < Game.Time && _inhib[i].HasBeenDone())
                {
                    RespawnTime[i] = Game.Time + 5 * 60;
                }
            }

        }

        public bool AllUpIn(float seconds)
        {
            return RespawnTime.All(time => time < Game.Time + seconds);
        }
    }
}
