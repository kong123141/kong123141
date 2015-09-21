using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace Talon
{
    internal class Damages
    {
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        private static readonly Dictionary<SpellSlot, Spell> Spell = Spells.Spell;

        public static float ComboDmg(Obj_AI_Base enemy)
        {
            var dmg = 0d;

            if (Spell[SpellSlot.W].IsReady())
            {
                dmg += Player.GetSpellDamage(enemy, SpellSlot.W);
            }
            if (Spell[SpellSlot.Q].IsReady())
            {
                dmg += Player.GetSpellDamage(enemy, SpellSlot.Q) + Player.GetAutoAttackDamage(enemy);
            }
            if (Spell[SpellSlot.R].IsReady())
            {
                dmg += Player.GetSpellDamage(enemy, SpellSlot.R);
            }

            dmg += Player.CalcDamage(enemy, Damage.DamageType.Physical, Player.TotalAttackDamage);

            if (Mechanics.IgniteSlot != SpellSlot.Unknown && Mechanics.IgniteSlot.IsReady())
            {
                dmg += Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            }

            return (float)dmg;
        }
    }
}
