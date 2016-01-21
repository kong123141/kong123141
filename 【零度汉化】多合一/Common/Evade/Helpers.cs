namespace Flowers_Utility.Common.Evade
{
    using LeagueSharp;
    using LeagueSharp.Common;

    static class Helpers
    {
        public static bool IsSpellShielded(this Obj_AI_Hero unit)
        {
            if (ObjectManager.Player.HasBuffOfType(BuffType.SpellShield))
            {
                return true;
            }

            if (ObjectManager.Player.HasBuffOfType(BuffType.SpellImmunity))
            {
                return true;
            }

            if (unit.LastCastedSpellName() == "SivirE" && (LeagueSharp.Common.Utils.TickCount - unit.LastCastedSpellT()) < 300)
            {
                return true;
            }

            if (unit.LastCastedSpellName() == "BlackShield" && (LeagueSharp.Common.Utils.TickCount - unit.LastCastedSpellT()) < 300)
            {
                return true;
            }

            if (unit.LastCastedSpellName() == "NocturneShit" && (LeagueSharp.Common.Utils.TickCount - unit.LastCastedSpellT()) < 300)
            {
                return true;
            }

            return false;
        }
    }
}
