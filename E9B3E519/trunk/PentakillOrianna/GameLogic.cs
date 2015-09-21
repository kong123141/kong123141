using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace PentakillOrianna {
    class GameLogic {

        public static void performCombo() {
            Obj_AI_Hero target = TargetSelector.GetTarget(Program.q.Range, TargetSelector.DamageType.Physical);
            if (target != null) {
                if (Program.menuController.getMenu().Item("comboQ").GetValue<bool>()) {
                    if (Program.q.IsReady()) {
                        Program.q.CastIfHitchanceEquals(target, HitChance.High, true);
                    }
                }
                if (Program.menuController.getMenu().Item("comboW").GetValue<bool>()) {
                    if (Program.w.IsReady()) {
                        if (Program.ball.getPosition().Distance(target.Position) < Program.w.Range) {
                            Program.w.Cast();
                        }
                    }
                }
                if (Program.menuController.getMenu().Item("comboR").GetValue<bool>()) {
                    if (Program.r.IsReady()) {
                        Program.r.CastIfWillHit(target, Program.menuController.getMenu().Item("minEnemies").GetValue<Slider>().Value);
                        if (target.Health < Program.r.GetDamage(target) && target.Distance(Program.ball.getPosition()) < Program.r.Range) {
                            Program.r.Cast();
                        }
                    }
                }
            }
            if (Program.player.HealthPercent < 80) {
                if (Program.menuController.getMenu().Item("comboE").GetValue<bool>()) {
                    if (Program.e.IsReady()) {
                        Program.e.Cast(Program.player);
                        Program.ball.setPosition(Program.player.Position);
                    }
                }
            }
        }

        public static void performEShield() {
            Obj_AI_Hero ally = ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsAlly && x.HealthPercent < 15 && !x.HasBuff("orianaghost", true)).FirstOrDefault();
            if (Program.menuController.getMenu().Item("comboE").GetValue<bool>()) {
                if (Program.e.IsReady()) {
                    Program.e.Cast(ally);
                    Program.ball.setPosition(ally.Position);
                }
            }
        }

        public static void performHarass() {
            Obj_AI_Hero target = TargetSelector.GetTarget(Program.q.Range, TargetSelector.DamageType.Physical);
            if (target != null) {
                if (Program.menuController.getMenu().Item("harassQ").GetValue<bool>()) {
                    if (Program.q.IsReady()) {
                        Program.q.CastIfHitchanceEquals(target, HitChance.High, true);
                    }
                }
                if (Program.menuController.getMenu().Item("harassW").GetValue<bool>()) {
                    if (Program.w.IsReady()) {
                        if (Program.ball.getPosition().Distance(target.Position) < Program.w.Range) {
                            Program.w.Cast();
                        }
                    }
                }
            }
        }


        public static void performLaneClear() {
            List<Obj_AI_Base> minionList = MinionManager.GetMinions(Program.q.Range);
            if (Program.menuController.getMenu().Item("clearQ").GetValue<bool>())
                if (Program.q.IsReady()) {
                    var wCast = MinionManager.GetBestCircularFarmLocation(minionList.Select(minion => minion.Position.To2D()).ToList(), Program.w.Width, Program.q.Range);
                    if (wCast.MinionsHit > 2)
                        if (Program.q.Cast(wCast.Position))
                            if (Program.menuController.getMenu().Item("clearW").GetValue<bool>())
                                if (Program.w.IsReady())
                                    Program.w.Cast();
                }
        }
        public static void AutoLevelUp() {
            List<SpellSlot> SKILL_SEQUENCE;
            switch (Program.menuController.getMenu().Item("autoLevel").GetValue<StringList>().SelectedIndex) {
                case 0:
                    AutoLevel.Disable();
                    break;
                case 1: //R>Q>W>E 
                    AutoLevel.Enable();
                    SKILL_SEQUENCE = new List<SpellSlot>() { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.Q, SpellSlot.Q, SpellSlot.R, SpellSlot.Q, SpellSlot.W, SpellSlot.Q, SpellSlot.W, SpellSlot.R, SpellSlot.W, SpellSlot.W, SpellSlot.E, SpellSlot.E, SpellSlot.R, SpellSlot.E, SpellSlot.E };
                    AutoLevel.UpdateSequence(SKILL_SEQUENCE);
                    break;

                case 2: //R>Q>E>W 
                    AutoLevel.Enable();
                    SKILL_SEQUENCE = new List<SpellSlot>() { SpellSlot.Q, SpellSlot.E, SpellSlot.W, SpellSlot.W, SpellSlot.W, SpellSlot.R, SpellSlot.W, SpellSlot.Q, SpellSlot.W, SpellSlot.Q, SpellSlot.R, SpellSlot.Q, SpellSlot.Q, SpellSlot.E, SpellSlot.E, SpellSlot.R, SpellSlot.E, SpellSlot.E };
                    AutoLevel.UpdateSequence(SKILL_SEQUENCE);
                    break;
            }
        }
    }
}
