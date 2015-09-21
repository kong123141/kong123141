using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace Pentakill_LeBlanc.GameLogic {
    internal class Combo {

        /**
         * 
         *TODO: Different Combos and Methods 
         * 
         **/

        private static int lastCast = 0;

        public static void performCombo() {
            bool useQ = Program.menuController.getMenu().Item("gates.menu.combo.useQ").GetValue<bool>();
            bool useW = Program.menuController.getMenu().Item("gates.menu.combo.useW").GetValue<bool>();
            bool wBack = Program.menuController.getMenu().Item("gates.menu.combo.wBack").GetValue<bool>();
            bool useE = Program.menuController.getMenu().Item("gates.menu.combo.useE").GetValue<bool>();
            bool useR = Program.menuController.getMenu().Item("gates.menu.combo.useR").GetValue<bool>();
            bool useIgnite = Program.menuController.getMenu().Item("gates.menu.combo.useIgnite").GetValue<bool>();
            int wDelay = Program.menuController.getMenu().Item("gates.menu.wDelay").GetValue<Slider>().Value;

            Obj_AI_Hero target = TargetSelector.GetTarget(Program.spells[SpellSlot.W].Range, TargetSelector.DamageType.Magical);
            if (target != null && target.IsValidTarget()) {
                if (Program.player.ManaPercent > 25) {
                    performFullCombo(target, useQ, useW, wBack, useE, useR, useIgnite, wDelay);
                }
                if (wBack && Utils.wActivated() && (Environment.TickCount - lastCast) >= wDelay && !target.HasBuffOfType(BuffType.Slow)) {
                    Program.spells[SpellSlot.W].Cast();
                }
            }

        }

        private static void performFullCombo(Obj_AI_Hero target, bool useQ, bool useW, bool wBack, bool useE, bool useR, bool useIgnite, int wDelay) {
            if (Utils.getComboDamage(target) > target.Health) {
                Program.status = "Igniting";
                Program.player.Spellbook.GetSpell(Program.ignite).IsReady();
            }
            if (useQ) {
                Program.status = "Casting Q";
                castQ(target);
            }
            if (useR && !Program.spells[SpellSlot.Q].IsReady() && Program.spells[SpellSlot.R].IsReady()) {
                Program.status = "Casting R";
                castQR(target, true);
            }
            if (useW && !Program.spells[SpellSlot.Q].IsReady() && !Program.spells[SpellSlot.R].IsReady() && Program.spells[SpellSlot.W].IsReady()) {
                Program.status = "Casting W";
                if (Utils.hasQRBuff(target))
                    castW(target, false, true);
                else
                    castW(target, false, false);
            }
            if (useE && !Program.spells[SpellSlot.Q].IsReady() && !Program.spells[SpellSlot.R].IsReady() && Program.spells[SpellSlot.E].IsReady() && Utils.wActivated()) {
                Program.status = "Casting E";
                castE(target, false, false);
            }
        }

        public static void perform2Chainz() {
            Obj_AI_Hero target = TargetSelector.GetTarget(Program.spells[SpellSlot.Q].Range, TargetSelector.DamageType.Magical);
            if (Program.player.IsDashing())
                return;
            if (Program.player.CanMove) {
                Program.player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            }
            if (Program.player.CanAttack) {
                Program.player.IssueOrder(GameObjectOrder.AutoAttack, target);
            }
            castQ(target);
            castE(target, true, false);
            castER(target, false);
            if (!Program.spells[SpellSlot.E].IsReady() && !Program.spells[SpellSlot.R].IsReady())
                castW(target, false, false);

        }

        private static bool castQ(Obj_AI_Hero target) {
            if (Program.spells[SpellSlot.Q].IsReady())
                return Program.spells[SpellSlot.Q].CastOnUnit(target);
            return false;
        }

        private static bool castW(Obj_AI_Hero target, bool needQBuff, bool needRBuff) {
            if (needQBuff) {
                if (!Utils.wActivated() && Program.spells[SpellSlot.W].IsReady() && Utils.hasQBuff(target)) {
                    if (Program.spells[SpellSlot.W].Cast(target.Position)) {
                        lastCast = Environment.TickCount;
                        return true;
                    }
                }
            } else if (needRBuff) {
                if (!Utils.wActivated() && Program.spells[SpellSlot.W].IsReady() && Utils.hasQRBuff(target)) {
                    if (Program.spells[SpellSlot.W].Cast(target.Position)) {
                        lastCast = Environment.TickCount;
                        return true;
                    }
                }
            } else {
                if (!Utils.wActivated() && Program.spells[SpellSlot.W].IsReady()) {
                    if (Program.spells[SpellSlot.W].Cast(target.Position)) {
                        lastCast = Environment.TickCount;
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool castE(Obj_AI_Hero target, bool needQBuff, bool needRBuff) {
            if (needQBuff) {
                if (Program.spells[SpellSlot.E].IsReady() && Utils.hasQBuff(target)) {
                    return Program.spells[SpellSlot.E].CastIfHitchanceEquals(target, HitChance.High);
                }
            } else if (needRBuff) {
                if (Program.spells[SpellSlot.E].IsReady() && Utils.hasQRBuff(target)) {
                    return Program.spells[SpellSlot.E].CastIfHitchanceEquals(target, HitChance.High);
                }
            } else {
                if (Program.spells[SpellSlot.E].IsReady()) {
                    return Program.spells[SpellSlot.E].CastIfHitchanceEquals(target, HitChance.High);
                }
            }
            return false;
        }

        private static bool castQR(Obj_AI_Hero target, bool needQBuff) {
            if (Utils.getRSpell() == Program.spells[SpellSlot.Q]) {
                if (needQBuff) {
                    if (Program.spells[SpellSlot.R].IsReady() && Utils.hasQBuff(target)) {
                        return Program.spells[SpellSlot.R].CastOnUnit(target);
                    }
                } else {
                    if (Program.spells[SpellSlot.R].IsReady()) {
                        return Program.spells[SpellSlot.R].CastOnUnit(target);
                    }
                }
            }
            return false;
        }

        private static bool castWR(Obj_AI_Hero target, bool needQBuff) {
            if (Utils.getRSpell() == Program.spells[SpellSlot.W]) {
                if (needQBuff) {
                    if (!Utils.rActivated() && Program.spells[SpellSlot.R].IsReady() && Utils.hasQBuff(target)) {
                        if (Program.spells[SpellSlot.R].Cast(target.Position)) {
                            lastCast = Environment.TickCount;
                            return true;
                        }
                    }
                } else {
                    if (!Utils.rActivated() && Program.spells[SpellSlot.R].IsReady()) {
                        if (Program.spells[SpellSlot.R].Cast(target.Position)) {
                            lastCast = Environment.TickCount;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static bool castER(Obj_AI_Hero target, bool needQBuff) {
            if (Utils.getRSpell() == Program.spells[SpellSlot.E]) {
                if (needQBuff) {
                    if (Program.spells[SpellSlot.R].IsReady() && Utils.hasQBuff(target)) {
                        return Program.spells[SpellSlot.R].CastIfHitchanceEquals(target, HitChance.High);
                    }
                } else {
                    if (Program.spells[SpellSlot.R].IsReady()) {
                        return Program.spells[SpellSlot.R].CastIfHitchanceEquals(target, HitChance.High);
                    }

                }
            }
            return false;
        }

    }
}
