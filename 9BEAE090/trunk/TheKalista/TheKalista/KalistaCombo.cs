using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using TheKalista.Commons.ComboSystem;
using SharpDX;

namespace TheKalista
{
    class KalistaCombo : ComboProvider
    {
        public KalistaCombo(float targetSelectorRange, IEnumerable<Skill> skills, Orbwalking.Orbwalker orbwalker)
            : base(targetSelectorRange, skills, orbwalker)
        {
        }

        public KalistaCombo(float targetSelectorRange, Orbwalking.Orbwalker orbwalker, params Skill[] skills)
            : base(targetSelectorRange, orbwalker, skills)
        {
        }

        //private static Vector2 FindIntersectionSpot(Obj_AI_Hero enemy, Obj_AI_Hero enemy2)
        //{
        //    if (enemy.NetworkId == enemy2.NetworkId) return Vector2.Zero;

        //    // Find the distance between the centers.
        //    float dx = enemy.Position.X - enemy2.Position.X;
        //    float dy = enemy.Position.Y - enemy2.Position.Y;
        //    double dist = Math.Sqrt(dx * dx + dy * dy);

        //    // See how many solutions there are.
        //    if (dist > ObjectManager.Player.AttackRange * 2)
        //    {
        //        return Vector2.Zero;
        //    }

        //    // Find a and h.
        //    double a = (dist * dist) / (2 * dist);
        //    double h = Math.Sqrt(ObjectManager.Player.AttackRange * ObjectManager.Player.AttackRange - a * a);

        //    // Find P2.
        //    double cx2 = enemy.Position.X + a * (enemy2.Position.X - enemy.Position.X) / dist;
        //    double cy2 = enemy.Position.Y + a * (enemy2.Position.Y - enemy.Position.Y) / dist;

        //    // Get the points P3.
        //    var intersection1 = new SharpDX.Vector2(
        //        (float)(cx2 + h * (enemy2.Position.Y - enemy.Position.Y) / dist),
        //        (float)(cy2 - h * (enemy2.Position.X - enemy.Position.X) / dist));
        //    var intersection2 = new SharpDX.Vector2(
        //        (float)(cx2 - h * (enemy2.Position.Y - enemy.Position.Y) / dist),
        //        (float)(cy2 + h * (enemy2.Position.X - enemy.Position.X) / dist));


        //    var mindst = float.MaxValue;
        //    var minspot = ObjectManager.Player.Position.To2D();

        //    if (HeroManager.Enemies.All(nmy => nmy == enemy || nmy == enemy2 || nmy.Position.To2D().Distance(intersection1, true) > ObjectManager.Player.AttackRange * ObjectManager.Player.AttackRange))
        //    {
        //        var dst = intersection1.Distance(ObjectManager.Player, true);
        //        if (dst < mindst)
        //        {
        //            mindst = dst;
        //            minspot = intersection1;
        //        }
        //    }

        //    if (HeroManager.Enemies.All(nmy => nmy == enemy || nmy == enemy2 || nmy.Position.To2D().Distance(intersection2, true) > ObjectManager.Player.AttackRange * ObjectManager.Player.AttackRange))
        //    {
        //        var dst = intersection1.Distance(ObjectManager.Player, true);
        //        if (dst < mindst)
        //        {
        //            return minspot;
        //        }
        //    }
        //    return minspot;
        //}

        //private static List<Vector2> FindKiteSpots(IEnumerable<Obj_AI_Hero> src)
        //{
        //    var minDst = float.MaxValue;
        //    var spots = new List<Vector2>();

        //    foreach (var enemy in src)
        //    {
        //        foreach (var enemy2 in HeroManager.Enemies)
        //        {
        //            spots.Add(FindIntersectionSpot(enemy, enemy2));
        //        }
        //    }
        //    return spots;
        //}

        //private static Vector2 FindBestKiteFleeSpot()
        //{
        //    var minSpot = Vector2.Zero;
        //    var minDst = float.MaxValue;
        //    foreach (var spot in FindKiteSpots(HeroManager.Enemies))
        //    {
        //        var currDst = spot.Distance(ObjectManager.Player, true);
        //        if (currDst < minDst)
        //        {
        //            minDst = currDst;
        //            minSpot = spot;
        //        }
        //    }
        //    return minSpot;
        //}


        //public void AutoKite()
        //{
        //    throw new NotImplementedException();

        //    Vector2 spot = ObjectManager.Player.Position.To2D();
        //    if (HeroManager.Enemies.Any(enemy => enemy.Position.Distance(ObjectManager.Player.Position, true) < ObjectManager.Player.AttackRange / 2 * ObjectManager.Player.AttackRange / 2))
        //    {
        //        spot = FindBestKiteFleeSpot();
        //    }
        //    else if (!Orbwalker.InAutoAttackRange(Target))
        //    {
        //        spot = ((Target.Position - ObjectManager.Player.Position).Normalized() * (ObjectManager.Player.AttackRange - 50) + Target.Position).To2D();
        //    }
        //    else
        //    {
        //        var sideSpots = FindKiteSpots(new Obj_AI_Hero[] { Target });

        //    }

        //    Orbwalker.ActiveMode = Orbwalking.OrbwalkingMode.Combo;
        //    Orbwalker.SetOrbwalkingPoint(spot.To3D2());

        //}

        protected override LeagueSharp.Obj_AI_Hero SelectTarget()
        {
            var target = Orbwalker.GetTarget();
            if (target != null && target.Type == LeagueSharp.GameObjectType.obj_AI_Hero)
                return (Obj_AI_Hero)target;
            return KalistaTargetSelector.GetTarget(TargetRange, (KalistaTargetSelector.DamageType)DamageType);
        }
    }
}
