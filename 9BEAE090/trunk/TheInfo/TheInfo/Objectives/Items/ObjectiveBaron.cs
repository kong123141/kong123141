using System.Linq;
using LeagueSharp;
using SharpDX;

namespace TheInfo.Objectives.Items
{
    class ObjectiveBaron : Objective
    {
        private int _respawnTime;
        private const int Tolerance = 10;
        public Obj_AI_Minion GameObject
        {
            get
            {
                if (_gameObject == null || !_gameObject.IsValid)
                    return null;
                return _gameObject;
            }
            set { _gameObject = value; }
        }
        private Obj_AI_Minion _gameObject;

        public ObjectiveBaron(Vector2 position)
            : base(position)
        {

        }

        protected override void LargeUpdate()
        {
            if (GameObject == null || GameObject.IsDead)
                GameObject = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(x => x.CampNumber > 0 && x.Name.Contains("Baron12.1.1") && x.IsVisible);

            if (CanBeDone() && ObjectiveCommons.HasAnyoneNashorBuff())
            {
                _respawnTime = (int)Game.Time + 7 * 60 - Tolerance;
                GameObject = null;
            }

            base.LargeUpdate();
        }


        public override int GetEstimatedGold()
        {
            return 300 * ObjectiveCommons.TeamSize;
        }

        public override int GetEstimatedExp()
        {
            return 900 * ObjectiveCommons.TeamSize;
        }

        public override float GetEstimatedDps(Obj_AI_Hero attacker)
        {
            //Todo: This is currently a shitty estimate
            return attacker.TotalAttackDamage * (1f / attacker.AttackDelay) + attacker.TotalMagicalDamage * 0.4f;
        }

        public override int GetImportance()
        {
            return 8;
        }

        public override bool CanBeDone()
        {
            return _respawnTime < Game.Time || (GameObject != null && !GameObject.IsDead);
        }

        public override bool HasBeenDone()
        {
            return !CanBeDone();
        }

        public override LeagueSharp.AttackableUnit GetGameObject()
        {
            return GameObject;
        }
    }
}
