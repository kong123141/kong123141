using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace LeBlanc.Helper
{
    internal class Cast
    {
        private static readonly Dictionary<SpellSlot, Spell> Spell = Spells.Spell;
        private static readonly bool PacketCast = Config.LeBlanc.Item("apollo.leblanc.packetcast").GetValue<bool>();

        public static void Q(Obj_AI_Base enemy, SpellSlot slot = SpellSlot.Q)
        {
            if (Spell[slot].IsReady() && enemy.IsValidTarget(Spell[slot].Range))
                Spell[slot].CastOnUnit(enemy, PacketCast);
        }
        public static void W(Obj_AI_Base enemy, HitChance hit, SpellSlot slot = SpellSlot.W)
        {
            if (Spell[slot].IsReady() && enemy.IsValidTarget())
            {
                var pre = Spell[slot].GetPrediction(enemy, true);
                if (pre.Hitchance >= hit)
                    Spell[slot].Cast(pre.CastPosition, PacketCast);
            }
        }
        public static void W2(SpellSlot slot = SpellSlot.W)
        {
            if (Spell[slot].IsReady())
            {
                Spell[slot].Cast(PacketCast);
            }
        }
        public static void E(Obj_AI_Base enemy, HitChance hit, SpellSlot slot = SpellSlot.E)
        {
            if (Spell[slot].IsReady() && enemy.IsValidTarget())
            {
                var pre = Spell[slot].GetPrediction(enemy);
                if (pre.Hitchance >= hit)
                    Spell[slot].Cast(pre.CastPosition, PacketCast);
            }
        }
        public class R
        {
            public static void Q(Obj_AI_Base enemy)
            {
                Cast.Q(enemy, SpellSlot.R);
            }
            public static void W(Obj_AI_Base enemy, HitChance hit)
            {
                Cast.W(enemy, hit, SpellSlot.R);
            }
            public static void W2()
            {
                Cast.W2(SpellSlot.R);
            }
            public static void E(Obj_AI_Base enemy, HitChance hit)
            {
                Cast.E(enemy, hit, SpellSlot.R);
            }
        }
    }
}
