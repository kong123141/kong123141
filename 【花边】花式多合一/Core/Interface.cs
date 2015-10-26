using System.Collections.Generic;
using LeagueSharp;
using SharpDX;
using Object = 花边_花式多合一.Core.junglerslack.Object;

namespace 花边_花式多合一.Core
{
    public static class Interface
    {
        internal static List<IDetector> detectors;
        public static Dictionary<int, Object> data = new Dictionary<int, Object>();

        public enum DetectorSetting { Preferred, Safe, AntiHumanizer }

        public interface IDetector
        {
            void Initialize(Obj_AI_Hero hero, DetectorSetting setting = DetectorSetting.Safe);
            void ApplySetting(DetectorSetting setting);
            void FeedData(Vector3 targetPos);
            int GetScriptDetections();
            string GetName();
        }

        public static void Add(Object unit)
        {
            data.Add(unit.@ref.Index, unit);
        }

        public static void Add(GameObject unit)
        {
            var @class = Object.Class(unit);
            if (@class == "useless") return;
            else if (@class == "creep") Add(new junglerslack.Creep(unit as Obj_AI_Minion));
            else if (@class == "creepSpawn") Add(new junglerslack.CreepSpawn(unit as NeutralMinionCamp));
            else if (@class == "player") Add(new junglerslack.Player(unit as Obj_AI_Hero));
            else if (unit is Obj_AI_Base) Add(new junglerslack.Unit(unit as Obj_AI_Base));
            else data.Add(unit.Index, new Object(unit));
        }

        public static void Remove(junglerslack.Unit unit)
        {
            Remove(unit.@ref);
        }

        public static void Remove(GameObject unit)
        {
            data.Remove(unit.Index);
        }

        static Interface()
        {
            GameObject.OnCreate += (obj, a) => Add(obj);
            GameObject.OnDelete += (obj, a) => Remove(obj);
            foreach (var obj in ObjectManager.Get<GameObject>()) Add(obj);
        }

        // extension
        public static T Data<T>(this GameObject unit) where T : Object
        {
            return (T)data[unit.Index];
        }

    }
}
