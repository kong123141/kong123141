using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace Pentakill_LeBlanc.GameLogic {
    internal class LaneClear {



        public static void performLaneClear() {
            bool useQ = Program.menuController.getMenu().Item("gates.menu.laneClear.useQ").GetValue<bool>();
            bool useW = Program.menuController.getMenu().Item("gates.menu.laneClear.useW").GetValue<bool>();
            bool useR = Program.menuController.getMenu().Item("gates.menu.laneClear.useR").GetValue<bool>();
            if (useQ && Program.spells[SpellSlot.Q].IsReady()) {
                useQOnMinion();
            }
            if (useW && Program.spells[SpellSlot.W].IsReady() && !Utils.wActivated()) {
                useWOnMinion();
            }
            if (useR && Program.spells[SpellSlot.R].IsReady() && Utils.getRSpell() == Program.spells[SpellSlot.W] &&
                !Utils.rActivated()) {
                useROnMinion();
            }
        }

        public static void useQOnMinion() {
            var minion = MinionManager.GetMinions(Program.spells[SpellSlot.Q].Range).FirstOrDefault();
            if (minion != null && minion.IsValidTarget())
                Program.spells[SpellSlot.Q].CastOnUnit(minion);
        }

        public static void useWOnMinion() {
            var castLocation = getWFarmLocation();
            if (castLocation.MinionsHit > 2) {
                Program.spells[SpellSlot.W].Cast(castLocation.Position);
            }
        }

        public static void useROnMinion() {
            var castLocation = getWFarmLocation();
            if (castLocation.MinionsHit > 2) {
                Program.spells[SpellSlot.R].Cast(castLocation.Position);
            }
        }

        public static MinionManager.FarmLocation getWFarmLocation() {
            return MinionManager.GetBestCircularFarmLocation(MinionManager.GetMinions(Program.spells[SpellSlot.W].Range).Select(minion => minion.Position.To2D()).ToList(), Program.spells[SpellSlot.W].Width, Program.spells[SpellSlot.W].Range);
        }
    }
}
