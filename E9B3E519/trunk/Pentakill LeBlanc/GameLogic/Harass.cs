using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace Pentakill_LeBlanc.GameLogic {
    internal class Harass {


        private static int lastCast = 0;

        public static void performHarass() {
            bool useQ = Program.menuController.getMenu().Item("gates.menu.harass.useQ").GetValue<bool>();
            bool useW = Program.menuController.getMenu().Item("gates.menu.harass.useW").GetValue<bool>();
            bool wBack = Program.menuController.getMenu().Item("gates.menu.harass.wBack").GetValue<bool>();
            bool useE = Program.menuController.getMenu().Item("gates.menu.harass.useE").GetValue<bool>();
            int wDelay = Program.menuController.getMenu().Item("gates.menu.wDelay").GetValue<Slider>().Value;
            Obj_AI_Hero target = TargetSelector.GetTarget(Program.spells[SpellSlot.W].Range, TargetSelector.DamageType.Magical);

            if (target != null && target.IsValidTarget()) {
                if (useQ && Program.spells[SpellSlot.Q].IsReady()) {
                    Program.spells[SpellSlot.Q].CastOnUnit(target);
                }
                if (useW && Program.spells[SpellSlot.W].IsReady() && !Program.spells[SpellSlot.Q].IsReady() && !Utils.wActivated() && Utils.hasQBuff(target)) {
                    Program.spells[SpellSlot.W].Cast(target);
                    lastCast = Environment.TickCount;
                } else if (useW && Program.spells[SpellSlot.W].IsReady() && !Utils.wActivated() && !Program.spells[SpellSlot.Q].IsReady()) {
                    Program.spells[SpellSlot.W].Cast(target);
                    lastCast = Environment.TickCount;
                }
                if (useE && Program.spells[SpellSlot.E].IsReady() && !Program.spells[SpellSlot.Q].IsReady() && Utils.wActivated()) {
                    Program.spells[SpellSlot.E].CastIfHitchanceEquals(target, HitChance.High);
                }
                if (wBack && Utils.wActivated() && (Environment.TickCount - lastCast) >= wDelay && !target.HasBuffOfType(BuffType.Slow)) {
                    Program.spells[SpellSlot.W].Cast();
                }
            }
        }
    }
}
