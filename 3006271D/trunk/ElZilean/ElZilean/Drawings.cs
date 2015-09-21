using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace ElZilean
{
    public class Drawings
    {
        public static void Drawing_OnDraw(EventArgs args)
        {

            var drawOff = ZileanMenu._menu.Item("ElZilean.Draw.off").GetValue<bool>();
            var drawQ = ZileanMenu._menu.Item("ElZilean.Draw.Q").GetValue<Circle>();
            var drawW = ZileanMenu._menu.Item("ElZilean.Draw.W").GetValue<Circle>();
            var drawE = ZileanMenu._menu.Item("ElZilean.Draw.E").GetValue<Circle>();
            var drawR = ZileanMenu._menu.Item("ElZilean.Draw.R").GetValue<Circle>();

            if (drawOff)
                return;

            if (drawQ.Active)
                if (Zilean.spells[Spells.Q].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Zilean.spells[Spells.Q].Range, Zilean.spells[Spells.Q].IsReady() ? Color.Green : Color.Red);

            if (drawW.Active)
                if (Zilean.spells[Spells.W].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Zilean.spells[Spells.W].Range, Zilean.spells[Spells.W].IsReady() ? Color.Green : Color.Red);

            if (drawE.Active)
                if (Zilean.spells[Spells.E].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Zilean.spells[Spells.E].Range, Zilean.spells[Spells.E].IsReady() ? Color.Green : Color.Red);

            if (drawR.Active)
                if (Zilean.spells[Spells.R].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Zilean.spells[Spells.R].Range, Zilean.spells[Spells.R].IsReady() ? Color.Green : Color.Red);
        }
    }
}