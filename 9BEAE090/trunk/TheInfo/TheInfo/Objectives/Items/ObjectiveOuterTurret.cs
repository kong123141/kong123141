using System;
using System.Linq;
using LeagueSharp;
using SharpDX;

namespace TheInfo.Objectives.Items
{
    class ObjectiveOuterTurret : Objective
    {
        public Obj_AI_Turret Object;
        public const int EstimatedPositionRange = 100;


        /// <summary>
        /// Give me the estimated position via SummonersRift.Base.TowerXYZ
        /// </summary>
        /// <param name="position"></param>
        public ObjectiveOuterTurret(Vector2 position) : base(position)
        {
            Object = ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(tower => Math.Abs(tower.Position.X - position.X) < EstimatedPositionRange && Math.Abs(tower.Position.Y - position.Y) < EstimatedPositionRange);
        }


        public override int GetEstimatedGold()
        {
            return 125 * ObjectiveCommons.TeamSize + 150; // global + local gold
        }

        public override int GetEstimatedExp()
        {
            return 0;
        }

        public override int GetImportance()
        {
            return 2;
        }

        public override bool CanBeDone()
        {
            return Object.IsValid && Object.Health > 0;
        }

        public override AttackableUnit GetGameObject()
        {
            return Object;
        }

        public override float GetEstimatedDps(Obj_AI_Hero attacker)
        {
            return ObjectiveCommons.GetEstimatedTowerDamage(attacker) * (1f / attacker.AttackDelay);
        }

        public override bool HasBeenDone()
        {
            return Object == null || (!Object.IsValid) || Object.IsDead;
        }
    }
}
