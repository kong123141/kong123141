using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using LeBlanc.Helper;

namespace LeBlanc
{
    internal class Damages
    {
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        private static readonly Dictionary<SpellSlot, Spell> Spell = Spells.Spell;
        private static readonly int[] RqDmg = { 100, 200, 300 };
        private static readonly int[] RwDmg = { 150, 300, 450 };
        private static readonly int[] ReDmg = { 100, 200, 300 };

        private const Damage.DamageType DmgType = Damage.DamageType.Magical;
        public class R
        {
            public static float Q(Obj_AI_Base enemy)
            {
                return
                    (float)
                        Player.CalcDamage(enemy, DmgType, RqDmg[Spell[SpellSlot.R].Level - 1] + (Player.TotalMagicalDamage * .65));
            }

            public static float W(Obj_AI_Base enemy)
            {
                return
                    (float)
                        Player.CalcDamage(enemy, DmgType, RwDmg[Spell[SpellSlot.R].Level - 1] + (Player.TotalMagicalDamage * .975));
            }

            public static float E(Obj_AI_Base enemy)
            {
                return
                    (float)
                        Player.CalcDamage(enemy, DmgType, ReDmg[Spell[SpellSlot.R].Level - 1] + (Player.TotalMagicalDamage * .65));
            }
        }
        public static float ComboDamage(Obj_AI_Base enemy)
        {
            var dmg = 0d;
            var mana = Player.Mana;
            var usedMana = 0f;

            if (Spell[SpellSlot.Q].IsReady() && mana >= usedMana + Spell[SpellSlot.Q].Instance.ManaCost)
            {
                dmg += 2 * Player.GetSpellDamage(enemy, SpellSlot.Q);
                usedMana += Spell[SpellSlot.Q].Instance.ManaCost;
            }
            if (Spell[SpellSlot.W].IsReady() && mana >= usedMana + Spell[SpellSlot.W].Instance.ManaCost)
            {
                dmg += Player.GetSpellDamage(enemy, SpellSlot.W);
                usedMana += Spell[SpellSlot.W].Instance.ManaCost;
            }
            if (Spell[SpellSlot.E].IsReady() && mana >= usedMana + Spell[SpellSlot.E].Instance.ManaCost)
            {
                dmg += 2 * Player.GetSpellDamage(enemy, SpellSlot.E);
            }
            if (enemy.HasBuff("LeblancChaosOrbM", true))
            {
                dmg += R.Q(enemy);
            }
            if (enemy.HasBuff("LeblancChaosOrb", true))
            {
                dmg += Player.GetSpellDamage(enemy, SpellSlot.Q);
            }

            if (Spell[SpellSlot.R].IsReady() && !Spell[SpellSlot.R].IsSecond())
            {
                if (Spell[SpellSlot.R].HasStatus(SpellSlot.Q))
                    dmg += 2 * R.Q(enemy);
                if (Spell[SpellSlot.R].HasStatus(SpellSlot.W))
                    dmg += R.W(enemy);
                if (Spell[SpellSlot.R].HasStatus(SpellSlot.E))
                    dmg += 2 * R.E(enemy);
            }

            if (Mechanics.IgniteSlot != SpellSlot.Unknown &&
                Player.Spellbook.CanUseSpell(Mechanics.IgniteSlot) == SpellState.Ready)
            {
                dmg += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            }

            return (float)dmg;
        }
    }
}
