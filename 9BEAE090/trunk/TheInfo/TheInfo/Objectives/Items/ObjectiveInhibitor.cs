using System;
using System.Linq;
using LeagueSharp;
using SharpDX;

namespace TheInfo.Objectives.Items
{
    class ObjectiveInhibitor : Objective
    {
        private const int EstimatedPositionRange = 100;
        private Obj_BarracksDampener _inhibitor;
        private ObjectiveInhibitorTurret _tower;

        public ObjectiveInhibitor(Vector2 position, ObjectiveInhibitorTurret inhibTower) : base(position)
        {
            _tower = inhibTower;
            _inhibitor = ObjectManager.Get<Obj_BarracksDampener>().First(tower => Math.Abs(tower.Position.X - position.X) < EstimatedPositionRange && Math.Abs(tower.Position.Y - position.Y) < EstimatedPositionRange);
            RequiredObjectives.Add(inhibTower);
        }

        public override int GetEstimatedGold()
        {
            return 50;
        }

        public override int GetEstimatedExp()
        {
            return 0;
        }


        public override int GetImportance()
        {
            return 8;
        }

        public override bool CanBeDone()
        {
            return (_tower.HasBeenDone()) && _inhibitor.IsValid && _inhibitor.Health > 0;
        }

        public override float GetEstimatedDps(Obj_AI_Hero attacker)
        {
            return ObjectiveCommons.GetEstimatedTowerDamage(attacker) * attacker.AttackSpeedMod;
        }

        public override bool HasBeenDone()
        {
            return _inhibitor.Health <= 0 ||(!_inhibitor.IsValid) || _inhibitor.IsDead;
        }

        public override AttackableUnit GetGameObject()
        {
            return _inhibitor;
        }
    }
}
