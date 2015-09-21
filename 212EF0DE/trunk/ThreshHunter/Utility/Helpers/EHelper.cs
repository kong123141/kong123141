using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ThreshHunter.Utility.Helpers
{
    class EHelper
    {
        public static EMode GetEMode()
        {
            switch (Thresh.RootMenu.Item("dz191.thresh.misc.emode").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return EMode.Push;
                case 1:
                    return EMode.Pull;
                default:
                    return EMode.Pull;
            }
        }

        public static void CastFlayPush(Obj_AI_Hero target, Orbwalking.OrbwalkingMode Mode)
        {
            var finalPosition = ObjectManager.Player.ServerPosition.Extend(target.ServerPosition, 200f);
            Thresh.spells[SpellSlot.E].Cast(finalPosition);
        }

        public static void CastFlayPull(Obj_AI_Hero target, Orbwalking.OrbwalkingMode Mode)
        {
            var finalPosition = ObjectManager.Player.ServerPosition.Extend(target.ServerPosition, -200f);
            Thresh.spells[SpellSlot.E].Cast(finalPosition);
        }

        public static Geometry.Polygon getERectangle(Vector3 finalPosition, float BoundingRadius)
        {
            var halfERange = 150f;
            var eRectangle = new Geometry.Polygon(
                Geometry.Polygon.Rectangle(
                    ObjectManager.Player.ServerPosition.To2D(),
                    ObjectManager.Player.ServerPosition.To2D().Extend(finalPosition.To2D(), halfERange),
                    BoundingRadius)
                );
            return eRectangle;
        }
    }
}
