namespace ElAlistarReborn
{
    using System;
    using System.Drawing;

    using LeagueSharp;
    using LeagueSharp.Common;

    internal class Drawings
    {
        #region Public Methods and Operators

        public static void OnDraw(EventArgs args)
        {
            var drawOff = ElAlistarMenu.Menu.Item("ElAlistar.Draw.off").GetValue<bool>();
            var drawQ = ElAlistarMenu.Menu.Item("ElAlistar.Draw.Q").GetValue<Circle>();
            var drawW = ElAlistarMenu.Menu.Item("ElAlistar.Draw.W").GetValue<Circle>();
            var drawE = ElAlistarMenu.Menu.Item("ElAlistar.Draw.E").GetValue<Circle>();

            if (drawOff)
            {
                return;
            }

            if (drawQ.Active)
            {
                if (Alistar.spells[Spells.Q].Level > 0)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Alistar.spells[Spells.Q].Range, Color.White);
                }
            }

            if (drawE.Active)
            {
                if (Alistar.spells[Spells.E].Level > 0)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Alistar.spells[Spells.E].Range, Color.White);
                }
            }

            if (drawW.Active)
            {
                if (Alistar.spells[Spells.W].Level > 0)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Alistar.spells[Spells.W].Range, Color.White);
                }
            }
        }

        #endregion
    }
}