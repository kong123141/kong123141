using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace DZJayce.Utility
{
    static class Helper
    {
        public static bool IsSpellReady(this Spells spell, Orbwalking.OrbwalkingMode Mode)
        {
            return SpellHandler.GetCooldown(spell) <= 0f && GetMenuValue<bool>(string.Format("dz191.jayce.{0}.{1}",Mode.ToString().ToLower(), spell.ToString().ToLower()));
        }
        public static bool IsSpellReady(this Spells spell)
        {
            return SpellHandler.GetCooldown(spell) <= 0f;
        }

        public static bool IsRangedForm(this Obj_AI_Hero player)
        {
            return SpellHandler.Q.Instance.Name != "JayceToTheSkies";
        }

        public static bool IsMeleeForm()
        {
            return !ObjectManager.Player.IsRangedForm();
        }
        private static T GetMenuValue<T>(string menuItem)
        {
            return Jayce.RootMenu.Item(menuItem).GetValue<T>();
        }

        public static Vector3 getGatePosition(Vector3 targetPosition)
        {
            var currentGateMode = GetMenuValue<StringList>("dz191.jayce.misc.gatemode").SelectedIndex;
            var gatePosition = Vector3.Zero;
            switch (currentGateMode)
            {
                case 0:
                    //Horizontal
                    gatePosition = ObjectManager.Player.ServerPosition.Extend(
                        targetPosition, SpellHandler.spells[Spells.ERanged].Range);
                    break;
                case 1:
                    //Vertical
                    gatePosition = ObjectManager.Player.ServerPosition.Extend(targetPosition, 1);
                    break;
            }
            return gatePosition;
        }
    }
}
