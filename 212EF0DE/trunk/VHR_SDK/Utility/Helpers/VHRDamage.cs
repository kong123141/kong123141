using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.SDK.Core.Extensions;
using LeagueSharp.SDK.Core.Wrappers;

namespace VHR_SDK.Utility.Helpers
{
    static class VHRDamage
    {
        public static float GetVHRSpellDamage(this SpellSlot spell, Obj_AI_Base target)
        {
            var realDamage = 0f;
            var dmgType = DamageType.Physical;
            var level = ObjectManager.Player.Spellbook.GetSpell(spell).Level;

            if (level == 0)
            {
                return 0f;
            }

            switch (spell)
            {
                case SpellSlot.Q:

                    realDamage = new float[] { 30, 35, 40, 45, 50 }[level] / 100 * (ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod) - 5f;
                    break;
                case SpellSlot.W:
                         dmgType = DamageType.True;
                         realDamage = new float[] { 20, 30, 40, 50, 60 }[level] +
                                 (new float[] { 4, 5, 6, 7, 8 }[level] / 100) * target.MaxHealth - 30f;
                    break;
                case SpellSlot.E:
                    realDamage = new float[] { 45, 80, 115, 150, 185 }[level] + 0.5f * ObjectManager.Player.FlatPhysicalDamageMod - 30f;
                    break;
            }

            if (realDamage == 0)
            {
                return 0f;
            }

            return (float)ObjectManager.Player.CalculateDamage(target, dmgType, realDamage);
        }
    }
}
