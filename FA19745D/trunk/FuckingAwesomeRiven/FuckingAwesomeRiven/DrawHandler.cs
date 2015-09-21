using System;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using LeagueSharp;
using LeagueSharp.Common;

namespace FuckingAwesomeRiven
{
    internal class DrawHandler
    {
        public static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public static void Draw(EventArgs args)
        {

            if (MenuHandler.Config.Item("DALL").GetValue<bool>())
                return;

            if (MenuHandler.Config.Item("drawCirclesforTest").GetValue<bool>())
            {
                JumpHandler.DrawCircles();
            }

            var drawQ = MenuHandler.Config.Item("DQ").GetValue<Circle>();
            var drawW = MenuHandler.Config.Item("DW").GetValue<Circle>();
            var drawE = MenuHandler.Config.Item("DE").GetValue<Circle>();
            var drawR = MenuHandler.Config.Item("DR").GetValue<Circle>();
            var drawBc = MenuHandler.Config.Item("DBC").GetValue<Circle>();

            var playerPos = Drawing.WorldToScreen(ObjectManager.Player.Position);
            var rBool = MenuHandler.Config.Item("forcedR").GetValue<KeyBind>().Active;
            Drawing.DrawText(
                playerPos.X - 70, playerPos.Y + 40, (rBool ? Color.GreenYellow : Color.Red), "Forced R: {0}",
                (rBool ? "Enabled" : "Disabled"));

            if (drawQ.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, SpellHandler.QRange, drawQ.Color);
            }

            if (drawW.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, SpellHandler.WRange, drawW.Color);
            }

            if (drawE.Active)
            {
                Render.Circle.DrawCircle(
                    ObjectManager.Player.Position, SpellHandler.Spells[SpellSlot.E].Range, drawE.Color);
            }

            if (drawR.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 900, drawR.Color);
            }

            if (drawBc.Active)
            {
                Render.Circle.DrawCircle(
                    ObjectManager.Player.Position, 400 + SpellHandler.Spells[SpellSlot.E].Range, drawR.Color);
            }

            if (MenuHandler.Config.Item("DER").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(
                    ObjectManager.Player.Position, SpellHandler.Spells[SpellSlot.W].IsReady() ? SpellHandler.Spells[SpellSlot.E].Range + SpellHandler.Spells[SpellSlot.W].Range : SpellHandler.Spells[SpellSlot.E].Range, MenuHandler.Config.Item("DER").GetValue<Circle>().Color);
            }

            if (!MenuHandler.Config.Item("debug").GetValue<bool>())
            {
                return;
            }

            Drawing.DrawText(100, 100 + (20*1), Color.White, "Can Q" + ": " + CheckHandler.CanQ);
            Drawing.DrawText(100, 100 + (20*2), Color.White, "Can W" + ": " + CheckHandler.CanW);
            Drawing.DrawText(100, 100 + (20*3), Color.White, "Can E" + ": " + CheckHandler.CanE);
            Drawing.DrawText(100, 100 + (20*4), Color.White, "Can R" + ": " + CheckHandler.CanR);
            Drawing.DrawText(100, 100 + (20*5), Color.White, "Can AA" + ": " + CheckHandler.CanAa);
            Drawing.DrawText(100, 100 + (20*6), Color.White, "Can Move" + ": " + CheckHandler.CanMove);
            Drawing.DrawText(100, 100 + (20*7), Color.White, "Can SR" + ": " + CheckHandler.CanSr);
            Drawing.DrawText(100, 100 + (20*8), Color.White, "Mid Q" + ": " + CheckHandler.MidQ);
            Drawing.DrawText(100, 100 + (20*9), Color.White, "Mid W" + ": " + CheckHandler.MidW);
            Drawing.DrawText(100, 100 + (20*10), Color.White, "Mid E" + ": " + CheckHandler.MidE);
            Drawing.DrawText(100, 100 + (20*11), Color.White, "Mid AA" + ": " + CheckHandler.MidAa);
            Drawing.DrawText(100, 100 + (20*12), Color.White, "TickCount" + ": " + Environment.TickCount);
            Drawing.DrawText(100, 100 + (20*13), Color.White, "lastQ" + ": " + CheckHandler.LastQ);
            Drawing.DrawText(100, 100 + (20*14), Color.White, "lastAA" + ": " + CheckHandler.LastAa);
            Drawing.DrawText(100, 100 + (20*15), Color.White, "lastE" + ": " + CheckHandler.LastE);
            Drawing.DrawText(100, 100 + (20 * 17), Color.White, "windingup" + ": " + ObjectManager.Player.IsWindingUp);

            var text2 = Queuer.Queue.Aggregate(string.Empty, (current, q) => current + q + "->");
            Drawing.DrawText(100, 100 + (20*16), Color.White, "queue" + ": " + text2);
        }
    }
}