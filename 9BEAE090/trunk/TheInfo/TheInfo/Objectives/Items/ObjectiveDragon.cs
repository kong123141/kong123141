using System;
using System.Linq;
using LeagueSharp;
using SharpDX;

namespace TheInfo.Objectives.Items
{
    class ObjectiveDragon : Objective
    {
        private int _respawnTime;
        private int _allyStacks;
        private int _enemyStacks;
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

        public ObjectiveDragon(Vector2 position)
            : base(position)
        {

        }

        protected override void LargeUpdate()
        {
            if (GameObject == null || GameObject.IsDead)
                GameObject = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(x => x.CampNumber > 0 && x.Name.Contains("Dragon") && x.IsVisible);

            if (CanBeDone())
            {
                var enemyStacks = ObjectiveCommons.GetEnemyDragonStacks();
                var allyStacks = ObjectiveCommons.GetAllyDragonStacks();
                Console.WriteLine(enemyStacks);
                if (enemyStacks > _enemyStacks || allyStacks > _allyStacks)
                {
                    _enemyStacks = enemyStacks;
                    _allyStacks = allyStacks;
                    _respawnTime = (int)Game.Time + 6 * 60 - Tolerance;
                    GameObject = null;
                }
            }

            base.LargeUpdate();
        }

        public override int GetEstimatedGold()
        {
            return 25;
        }

        public override int GetEstimatedExp()
        {
            return 200 * ObjectiveCommons.TeamSize; //Todo: not accurate!
        }

        public override int GetImportance()
        {
            switch (_allyStacks)
            {
                case 0:
                    return 2;
                case 1:
                    return 1;
                case 2:
                    return 2;
                case 3:
                    return 1;
                case 4:
                    return 5;
            }
            return 0;
        }


        public override bool CanBeDone()
        {
            //  Console.WriteLine("Can be done: (GameObject != null && !GameObject.IsDead) " + (GameObject != null && !GameObject.IsDead)+" "+GameObject.Health);
            return _respawnTime < Game.Time || (GameObject != null && !GameObject.IsDead);
        }

        public override float GetEstimatedDps(Obj_AI_Hero attacker)
        {
            //Todo: This is currently a shitty estimate
            return attacker.TotalAttackDamage * (1f / attacker.AttackDelay) + attacker.TotalMagicalDamage * 0.4f;
        }

        public override bool HasBeenDone()
        {
            return !CanBeDone();
        }

        public override AttackableUnit GetGameObject()
        {
            return GameObject;
        }
    }
}
