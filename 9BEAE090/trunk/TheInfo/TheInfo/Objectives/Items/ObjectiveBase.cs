using System;
using System.Linq;
using LeagueSharp;
using SharpDX;

namespace TheInfo.Objectives.Items
{
    class ObjectiveBase : Objective
    {
        public Obj_HQ Object;
        private readonly ObjectiveBaseTurret _turret1;
        private readonly ObjectiveBaseTurret _turret2;

        public ObjectiveBase(Vector2 position, ObjectiveBaseTurret turret1, ObjectiveBaseTurret turret2)
            : base(position)
        {
            Object = ObjectManager.Get<Obj_HQ>().First(tower => Math.Abs(tower.Position.X - position.X) < ObjectiveOuterTurret.EstimatedPositionRange && Math.Abs(tower.Position.Y - position.Y) < ObjectiveOuterTurret.EstimatedPositionRange);
            _turret1 = turret1;
            _turret2 = turret2;
            RequiredObjectives.Add(turret1);
            RequiredObjectives.Add(turret2);
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
            return 1337 + 420 + 69;
        }

        public override bool CanBeDone()
        {
            return (_turret1.HasBeenDone()) && (_turret2.HasBeenDone()) && Object.IsValid && Object.Health > 0;
        }

        public override float GetEstimatedDps(Obj_AI_Hero attacker)
        {
            return ObjectiveCommons.GetEstimatedTowerDamage(attacker) * attacker.AttackSpeedMod;
        }

        public override AttackableUnit GetGameObject()
        {
            return Object;
        }

        public override bool HasBeenDone()
        {
            return (!Object.IsValid) || Object.IsDead;
        }
    }
}
