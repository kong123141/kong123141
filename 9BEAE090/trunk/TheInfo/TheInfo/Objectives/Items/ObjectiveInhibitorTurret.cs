using System;
using System.Linq;
using LeagueSharp;
using SharpDX;

namespace TheInfo.Objectives.Items
{
    class ObjectiveInhibitorTurret : Objective
    {
        public Obj_AI_Turret Object;
        private readonly ObjectiveInnerTurret _requiredTurret;

        /// <summary>
        /// Give me the estimated position via SummonersRift.Base.TowerXYZ
        /// </summary>
        public ObjectiveInhibitorTurret(Vector2 position, ObjectiveInnerTurret requiredTurret) : base(position)
        {
            Object = ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(tower => Math.Abs(tower.Position.X - position.X) < ObjectiveOuterTurret.EstimatedPositionRange && Math.Abs(tower.Position.Y - position.Y) < ObjectiveOuterTurret.EstimatedPositionRange);
            _requiredTurret = requiredTurret;
            RequiredObjectives.Add(requiredTurret);
        }

        public override int GetEstimatedGold()
        {
            return 175 * ObjectiveCommons.TeamSize; //global
        }

        public override int GetEstimatedExp()
        {
            return 100 * ObjectiveCommons.TeamSize;
        }


        public override int GetImportance()
        {
            return 7;
        }

        public override bool CanBeDone()
        {
            return (_requiredTurret.HasBeenDone()) && Object.IsValid && Object.Health > 0;
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
