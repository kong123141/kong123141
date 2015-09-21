using System;
using System.Linq;
using LeagueSharp;
using SharpDX;

namespace TheInfo.Objectives.Items
{
    class ObjectiveBaseTurret : Objective
    {
        public Obj_AI_Turret Object;
        private readonly ObjectiveInhibitor _inhib1;
        private readonly ObjectiveInhibitor _inhib2;
        private readonly ObjectiveInhibitor _inhib3;

        public ObjectiveBaseTurret(Vector2 position, ObjectiveInhibitor inhib1, ObjectiveInhibitor inhib2, ObjectiveInhibitor inhib3)
            : base(position)
        {
            Object = ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(tower => Math.Abs(tower.Position.X - position.X) < ObjectiveOuterTurret.EstimatedPositionRange && Math.Abs(tower.Position.Y - position.Y) < ObjectiveOuterTurret.EstimatedPositionRange);
            _inhib1 = inhib1;
            _inhib2 = inhib2;
            _inhib3 = inhib3;
            RequireAll = false;
            RequiredObjectives.Add(inhib1);
            RequiredObjectives.Add(inhib2);
            RequiredObjectives.Add(inhib3);
        }

        public override int GetEstimatedGold()
        {
            return 50 * ObjectiveCommons.TeamSize;
        }

        public override int GetEstimatedExp()
        {
            return 0;
        }

        public override int GetImportance()
        {
            return 7;
        }

        public override bool CanBeDone()
        {
            return ((_inhib1.HasBeenDone()) || (_inhib2.HasBeenDone()) || (_inhib3.HasBeenDone())) && Object.IsValid && Object.Health > 0;
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
            return Object == null || (!Object.IsValid) || Object.IsDead;
        }
    }
}
