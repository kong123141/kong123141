using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace TheInfo.Objectives.Items
{
    public abstract class Objective
    {
        public string DisplayName;
        private const int RangedRange = 450; // ~ range of ranged characters
        public List<Objective> RequiredObjectives = new List<Objective>(); //Like inhibitor following inhib tower
        public bool RequireAll { get { return _requireAll; } protected set { _requireAll = value; } } // if all required objects are required or just one of them
        private bool _requireAll = true;
        public string SpecialText { get; set; }
        private float _largeUpdateTime;
        private readonly Dictionary<Objective, float> _distanceData = new Dictionary<Objective, float>();
        public Vector2 Position { get; private set; }


        protected Objective(Vector2 position)
        {
            Position = position;
        }

        public virtual void Update()
        {
            if (Game.Time - _largeUpdateTime > 1f)
            {
                _largeUpdateTime = Game.Time;
                LargeUpdate();
            }
        }

        protected virtual void LargeUpdate() { }

        public abstract int GetEstimatedGold();
        public abstract int GetEstimatedExp();
        public abstract float GetEstimatedDps(Obj_AI_Hero attacker);
        public abstract int GetImportance();
        public abstract bool CanBeDone();
        public abstract bool HasBeenDone();
        public abstract AttackableUnit GetGameObject();

        public float GetMeleeDistanceTo(Objective otherObjective)
        {
            if (_distanceData.ContainsKey(otherObjective))
                return _distanceData[otherObjective];
            return otherObjective._distanceData.ContainsKey(this) ? otherObjective._distanceData[this] : Position.Distance(otherObjective.Position);
        }

        public float GetRangedDistanceTo(Objective otherObjective)
        {
            return GetMeleeDistanceTo(otherObjective) - RangedRange * 2f;
        }

        public void SetDistanceTo(Objective otherObjective, float distance)
        {
            _distanceData.Add(otherObjective, distance);
        }

        public void OrientNonMappedOnNearestMapped(Objective[] objectives)
        {
            foreach (var objective in objectives)
            {

                if (_distanceData.ContainsKey(objective)) continue;
                var nearestObjective = objectives.Where(obj => _distanceData.ContainsKey(obj)).OrderBy(obj => obj.Position.Distance(objective.Position)).FirstOrDefault();
                if (nearestObjective != null) SetDistanceTo(objective, _distanceData[nearestObjective] + nearestObjective.Position.Distance(objective.Position));
            }
        }

        public string TryGenerateDisplayName() //Todo
        {
            DisplayName = this.GetType().Name;
            return DisplayName;
        }

        public override string ToString()
        {
            return "{ \"" + DisplayName + "\" / CanBeDone: " + CanBeDone() + " / HasBeenDone: " + HasBeenDone() + " }";
        }
    }
}
