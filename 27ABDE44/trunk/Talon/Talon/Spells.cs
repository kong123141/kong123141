using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace Talon
{
    internal class Spells
    {
        public static float Wangle = 20 * (float)Math.PI / 180;
        public static readonly Dictionary<SpellSlot, Spell> Spell = new Dictionary<SpellSlot, Spell>
        {
            { SpellSlot.Q, new Spell(SpellSlot.Q) },
            { SpellSlot.W, new Spell(SpellSlot.W, 780f) },
            { SpellSlot.E, new Spell(SpellSlot.E, 700f) },
            { SpellSlot.R, new Spell(SpellSlot.R, 650f) }
        };
        public static void Init()
        {
            Spell[SpellSlot.W].SetSkillshot(0.25f, 75, 2300, false, SkillshotType.SkillshotLine);
            Spell[SpellSlot.E].SetTargetted(0, 0);
        }
    }
}
