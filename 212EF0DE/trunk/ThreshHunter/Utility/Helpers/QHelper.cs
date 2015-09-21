using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace ThreshHunter.Utility.Helpers
{
    class QHelper
    {
        public static QStates GetQState()
        {
            if (!Thresh.spells[SpellSlot.Q].IsReady())
            {
                return QStates.NotReady;
            }

            switch (Thresh.spells[SpellSlot.Q].Instance.Name)
            {
                case "ThreshQ":
                    return QStates.Q1;
                case "threshqleap":
                    if (Thresh.HookedUnit != null)
                    {
                        return QStates.Q2;
                    }
                    return QStates.Q1;
                default:
                    return QStates.Unknown;
            }
        }
    }
}
