using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace FuckingAwesomeRiven
{
    public class JumpPosition
    {
        public Vector3 DirectionPos;
        public Vector3 JumpPos;

        public JumpPosition(Vector3 direction, Vector3 position)
        {
            DirectionPos = direction;
            JumpPos = position;
        }
    }

    internal static class JumpHandler
    {
        public static List<Vector3> JumpPositionsList2 = new List<Vector3>();
        public static List<JumpPosition> AllJumpPos = new List<JumpPosition>();
        public static bool InitJump;
        public static bool Jumping;
        public static JumpPosition SelectedPos;

        public static void Load()
        {
            AllJumpPos = new List<JumpPosition>
            {
                new JumpPosition(new Vector3(3900f, 1210f, 95.74805f), new Vector3(4142f, 1234f, 95.74805f)),
                new JumpPosition(new Vector3(4490f, 1272f, 95.74807f), new Vector3(4444f, 1254f, 95.74805f)),
                new JumpPosition(new Vector3(6666f, 1474f, 49.986f), new Vector3(6790f, 1482f, 49.59703f)),
                new JumpPosition(new Vector3(7314f, 1486f, 49.4455f), new Vector3(7052f, 1486f, 49.44687f)),
                new JumpPosition(new Vector3(7720f, 1604f, 49.44771f), new Vector3(7724f, 1722f, 49.4488f)),
                new JumpPosition(new Vector3(7744f, 2220f, 51.14706f), new Vector3(7756f, 2072f, 51.1414f))
            };
        }

        public static void Jump()
        {
            if (CheckHandler.QCount != 2)
            {
                SpellHandler.CastQ();
                return;
            }

            if (InitJump && SelectedPos != null && !Jumping)
            {
                Jumping = true;
                if (!SpellHandler.Spells[SpellSlot.E].IsReady())
                {
                    SmoothMouse.addMouseEvent(SelectedPos.DirectionPos, true);
                    ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, SelectedPos.DirectionPos);
                    Utility.DelayAction.Add(
                        100 + Game.Ping/2,
                        () => { SmoothMouse.addMouseEvent(SelectedPos.JumpPos, true); ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, SelectedPos.JumpPos); });
                    Utility.DelayAction.Add(
                        300 + Game.Ping/2, () =>
                        {
                            SpellHandler.CastQ();
                            Jumping = false;
                        });
                }
                else
                {
                    SmoothMouse.addMouseEvent(SelectedPos.DirectionPos);
                    ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, SelectedPos.DirectionPos);
                    Utility.DelayAction.Add(100 + Game.Ping / 2, () => { SpellHandler.CastE(SelectedPos.JumpPos); });
                    Utility.DelayAction.Add(
                        200 + Game.Ping/2, () =>
                        {
                            SpellHandler.CastQ();
                            Jumping = false;
                        });
                }

                InitJump = false;
            }

            if (InitJump || Jumping)
            {
                return;
            }

            SelectedPos = null;
            foreach (var jumpPos in AllJumpPos.Where(jumpPos => ObjectManager.Player.Distance(jumpPos.JumpPos) < 80))
            {
                SelectedPos = jumpPos;
                InitJump = true;
                break;
            }
        }

        public static void OnDraw()
        {
            foreach (var pos in AllJumpPos)
            {
                Render.Circle.DrawCircle(pos.DirectionPos, 30, Color.White);
                Drawing.DrawLine(
                    Drawing.WorldToScreen(pos.DirectionPos), Drawing.WorldToScreen(pos.JumpPos), 2, Color.White);
                Render.Circle.DrawCircle(pos.JumpPos, 30, Color.White);
            }
        }

        public static void DrawCircles()
        {
            foreach (var pos in AllJumpPos)
            {
                Render.Circle.DrawCircle(pos.DirectionPos, 30, Color.White);
                Drawing.DrawLine(
                    Drawing.WorldToScreen(pos.DirectionPos), Drawing.WorldToScreen(pos.JumpPos), 2, Color.White);
                Render.Circle.DrawCircle(pos.JumpPos, 30, Color.White);
            }

            for (var i = 0; i == JumpPositionsList2.Count - 1; i++)
            {
                Render.Circle.DrawCircle(JumpPositionsList2[i], 30, Color.Blue);
            }
        }

        public static void AddPos()
        {
            JumpPositionsList2.Add(ObjectManager.Player.Position);
            if (JumpPositionsList2.Count == 2)
            {
                AllJumpPos.Add(new JumpPosition(JumpPositionsList2[0], JumpPositionsList2[1]));
                JumpPositionsList2 = new List<Vector3>();
                Game.PrintChat("Added new Jump Position {0} ", Game.Time);
            }
            else if (JumpPositionsList2.Count > 2)
            {
                JumpPositionsList2 = new List<Vector3>();
                Game.PrintChat("Error: recreate jump pos");
            }
        }

        public static void ClearPrevious()
        {
            if (AllJumpPos.Count == 0)
            {
                return;
            }

            AllJumpPos.Remove(AllJumpPos[AllJumpPos.Count - 1]);
            Game.PrintChat("Removed Previous");
        }

        public static void ClearCurrent()
        {
            JumpPositionsList2 = new List<Vector3>();
            Game.PrintChat("Cleared Current Position!");
        }

        public static void PrintToConsole()
        {
            Console.Clear();
            foreach (var j in AllJumpPos)
            {
                Console.WriteLine(
                    "new jumpPosition(new Vector3({0}f, {1}f, {2}f), new Vector3({3}f, {4}f, {5}f)), ", j.DirectionPos.X,
                    j.DirectionPos.Y, j.DirectionPos.Z, j.JumpPos.X, j.JumpPos.Y, j.JumpPos.Z);
            }
        }

        public static bool IsFacing(this Vector3 source, Vector3 target)
        {
            if (!source.IsValid() || !target.IsValid())
            {
                return false;
            }

            const float angle = 90;
            return source.To2D().AngleBetween((target - source).To2D()) < angle;
        }
    }
}