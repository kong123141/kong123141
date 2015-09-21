using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace PentakillOrianna.Util {
    class SpellDamage {

        public static float getComboDamage(Obj_AI_Hero target) {
            float damage = (float)Program.player.GetAutoAttackDamage(target, true) * 3;
            if (Program.menuController.getMenu().Item("comboQ").GetValue<bool>()) {
                if (Program.q.IsReady()) {
                    damage += Program.q.GetDamage(target);
                }
            }
            if (Program.menuController.getMenu().Item("comboW").GetValue<bool>()) {
                if (Program.w.IsReady()) {
                    damage += Program.w.GetDamage(target);
                }
            }
            if (Program.menuController.getMenu().Item("comboR").GetValue<bool>()) {
                if (Program.r.IsReady()) {
                    damage += Program.r.GetDamage(target);
                }
            }
            if (Program.menuController.getMenu().Item("useIgnite").GetValue<bool>()) {
                if (Program.ignite.IsReady()) {
                    damage += (float)Program.player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
                }
            }
            return damage;
        }

    }
}
