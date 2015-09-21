using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace Pentakill_LeBlanc {
    static class Utils {

        public static float getComboDamage(Obj_AI_Hero target) {
            double damage = Program.player.GetAutoAttackDamage(target, true);
            int rLevel = Program.spells[SpellSlot.R].Level;

            if (Program.spells[SpellSlot.Q].IsReady()) {
                damage += Program.player.CalcDamage(target, Damage.DamageType.Magical, new double[] { 110, 160, 210, 260, 310 }[Program.spells[SpellSlot.Q].Level - 1] + (0.8f * Program.player.FlatMagicDamageMod));
            }
            if (Program.spells[SpellSlot.R].IsReady()) {
                damage += Program.player.CalcDamage(target, Damage.DamageType.Magical, new double[] { 200, 400, 600 }[Program.spells[SpellSlot.R].Level - 1] + (1.2f * Program.player.FlatMagicDamageMod));
            }
            if (Program.spells[SpellSlot.W].IsReady()) {
                damage += Program.player.CalcDamage(target, Damage.DamageType.Magical, new double[] { 85, 125, 165, 205, 245 }[Program.spells[SpellSlot.W].Level - 1] + (0.6f * Program.player.FlatMagicDamageMod));
            }
            if (Program.spells[SpellSlot.E].IsReady()) {
                damage += Program.player.CalcDamage(target, Damage.DamageType.Magical, new double[] { 80, 130, 180, 230, 280 }[Program.spells[SpellSlot.E].Level - 1] + (1.0f * Program.player.FlatMagicDamageMod));
            }
            if (Program.player.GetSpell(Program.ignite).IsReady()) {
                damage += Program.player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            }

            return (float)damage;
        }

        public static Spell getRSpell() {
            switch (Program.player.Spellbook.GetSpell(SpellSlot.R).Name) {
                case "LeblancChaosOrbM":
                    return Program.spells[SpellSlot.Q];
                case "LeblancSlideM":
                    return Program.spells[SpellSlot.W];
                case "LeblancSoulShackleM":
                    return Program.spells[SpellSlot.E];
            }
            return null;
        }

        public static bool wActivated() {
            return Program.player.Spellbook.GetSpell(SpellSlot.W).Name == "leblancslidereturn";
        }

        public static bool rActivated() {
            return Program.player.Spellbook.GetSpell(SpellSlot.R).Name == "leblancslidereturnm";
        }

        public static bool hasQBuff(Obj_AI_Hero target) {
            return target.HasBuff("LeblancChaosOrb");
        }

        public static bool hasQRBuff(Obj_AI_Hero target) {
            return target.HasBuff("LeblancChaosOrbM");
        }

        public static bool hasEBuff(Obj_AI_Hero target) {
            return target.HasBuff("LeblancSoulShackle") || target.HasBuff("LeblancSoulShackleM");
        }

        public static void autoLevel() {
            List<SpellSlot> SKILL_SEQUENCE;
            switch (Program.menuController.getMenu().Item("gates.menu.autoLevel").GetValue<StringList>().SelectedIndex) {
                case 0:
                    AutoLevel.Disable();
                    break;
                case 1: //R>Q>W>E
                    AutoLevel.Enable();
                    SKILL_SEQUENCE = new List<SpellSlot>() { SpellSlot.W, SpellSlot.Q, SpellSlot.E, SpellSlot.Q, SpellSlot.Q, SpellSlot.R, SpellSlot.Q, SpellSlot.W, SpellSlot.Q, SpellSlot.W, SpellSlot.R, SpellSlot.W, SpellSlot.W, SpellSlot.E, SpellSlot.E, SpellSlot.R, SpellSlot.E, SpellSlot.E };
                    AutoLevel.UpdateSequence(SKILL_SEQUENCE);
                    break;
                case 2: //R>Q>E>W
                    AutoLevel.Enable();
                    SKILL_SEQUENCE = new List<SpellSlot>() { SpellSlot.W, SpellSlot.Q, SpellSlot.E, SpellSlot.Q, SpellSlot.Q, SpellSlot.R, SpellSlot.Q, SpellSlot.E, SpellSlot.Q, SpellSlot.E, SpellSlot.R, SpellSlot.E, SpellSlot.E, SpellSlot.W, SpellSlot.W, SpellSlot.R, SpellSlot.W, SpellSlot.W };
                    AutoLevel.UpdateSequence(SKILL_SEQUENCE);
                    break;
                case 3: //R>W>Q>E
                    AutoLevel.Enable();
                    SKILL_SEQUENCE = new List<SpellSlot>() { SpellSlot.W, SpellSlot.Q, SpellSlot.E, SpellSlot.W, SpellSlot.W, SpellSlot.R, SpellSlot.W, SpellSlot.Q, SpellSlot.W, SpellSlot.Q, SpellSlot.R, SpellSlot.Q, SpellSlot.Q, SpellSlot.E, SpellSlot.E, SpellSlot.R, SpellSlot.E, SpellSlot.E };
                    AutoLevel.UpdateSequence(SKILL_SEQUENCE);
                    break;
                case 4: //R>W>E>Q
                    AutoLevel.Enable();
                    SKILL_SEQUENCE = new List<SpellSlot>() { SpellSlot.W, SpellSlot.Q, SpellSlot.E, SpellSlot.W, SpellSlot.W, SpellSlot.R, SpellSlot.W, SpellSlot.E, SpellSlot.W, SpellSlot.E, SpellSlot.R, SpellSlot.E, SpellSlot.E, SpellSlot.Q, SpellSlot.Q, SpellSlot.R, SpellSlot.Q, SpellSlot.Q };
                    AutoLevel.UpdateSequence(SKILL_SEQUENCE);
                    break;
                case 5: //R>E>Q>W
                    AutoLevel.Enable();
                    SKILL_SEQUENCE = new List<SpellSlot>() { SpellSlot.W, SpellSlot.Q, SpellSlot.E, SpellSlot.E, SpellSlot.E, SpellSlot.R, SpellSlot.E, SpellSlot.Q, SpellSlot.E, SpellSlot.Q, SpellSlot.R, SpellSlot.Q, SpellSlot.Q, SpellSlot.W, SpellSlot.W, SpellSlot.R, SpellSlot.W, SpellSlot.W };
                    AutoLevel.UpdateSequence(SKILL_SEQUENCE);
                    break;
                case 6: //R>E>W>Q
                    AutoLevel.Enable();
                    SKILL_SEQUENCE = new List<SpellSlot>() { SpellSlot.W, SpellSlot.Q, SpellSlot.E, SpellSlot.E, SpellSlot.E, SpellSlot.R, SpellSlot.E, SpellSlot.W, SpellSlot.E, SpellSlot.W, SpellSlot.R, SpellSlot.W, SpellSlot.W, SpellSlot.Q, SpellSlot.Q, SpellSlot.R, SpellSlot.Q, SpellSlot.Q };
                    AutoLevel.UpdateSequence(SKILL_SEQUENCE);
                    break;
            }
        }
    }
}
