using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;

namespace 花边_花式多合一.Core
{
    class FlowersDrawing
    {
        public static string FormatTime(double time)
        {
            TimeSpan t = TimeSpan.FromSeconds(time);
            if (t.Minutes > 0)
            {
                return string.Format("{0:D1}:{1:D2}", t.Minutes, t.Seconds);
            }
            return string.Format("{0:D}", t.Seconds);
        }

        public static void DrawText(Font font, String text, int posX, int posY, Color color)
        {
            Rectangle rec = font.MeasureText(null, text, FontDrawFlags.Center);
            font.DrawText(null, text, posX + 1 + rec.X, posY + 1, Color.Black);
            font.DrawText(null, text, posX + rec.X, posY + 1, Color.Black);
            font.DrawText(null, text, posX - 1 + rec.X, posY - 1, Color.Black);
            font.DrawText(null, text, posX + rec.X, posY - 1, Color.Black);
            font.DrawText(null, text, posX + rec.X, posY, color);
        }

        public static void DrawText1(Font font, String text, int posX, int posY, Color color)
        {
            Rectangle rec = font.MeasureText(null, text, FontDrawFlags.Center);
            font.DrawText(null, text, posX + 1 + rec.X, posY + 1, Color.Black);
            font.DrawText(null, text, posX + rec.X, posY + 1, Color.Black);
            font.DrawText(null, text, posX - 1 + rec.X, posY - 1, Color.Black);
            font.DrawText(null, text, posX + rec.X, posY - 1, Color.Black);
            font.DrawText(null, text, posX + rec.X, posY, color);
        }

        public static Font Text1 = new Font(Drawing.Direct3DDevice, new FontDescription
        {
            FaceName = "Calibri",
            Height = 13,
            OutputPrecision = FontPrecision.Default,
            Quality = FontQuality.Default,
        });
    }

    class TextUtils
    {

        public static void DrawText(float x, float y, System.Drawing.Color c, string text)
        {
            if (text != null)
            {
                Drawing.DrawText(x, y, c, text);
            }
        }

        public static System.Drawing.Size GetTextExtent(string text)
        {
            if (text != null)
            {
                return Drawing.GetTextExtent(text);
            }
            else
            {
                return Drawing.GetTextExtent("A");
            }
        }

        public static string FormatTime(double time)
        {
            if (time > 0)
            {
                return Utils.FormatTime(time);
            }
            else
            {
                return "00:00";
            }
        }
    }

    abstract class RenderObject
    {
        public float endTime = 0;
        public float startTime = 0;

        abstract public void Draw();
    }

    class RenderObjects
    {
        private static List<RenderObject> objects = new List<RenderObject>();

        static RenderObjects()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            Render();
        }

        private static void Render()
        {
            foreach (RenderObject obj in objects)
            {
                if (obj.endTime - DelayAction.TickCount > 0)
                {
                    obj.Draw();
                }
                else
                {
                    DelayAction.Add(1, () => objects.Remove(obj));
                }
            }
        }

        public static void Add(RenderObject obj)
        {
            objects.Add(obj);
        }
    }
    class RenderText : RenderObject
    {
        public Vector2 renderPosition = new Vector2(0, 0);
        public string text = "";

        public System.Drawing.Color color = System.Drawing.Color.White;

        public RenderText(string text, Vector2 renderPosition, float renderTime)
        {
            this.startTime = DelayAction.TickCount;
            this.endTime = this.startTime + renderTime;
            this.renderPosition = renderPosition;

            this.text = text;
        }

        public RenderText(string text, Vector2 renderPosition, float renderTime,
            System.Drawing.Color color)
        {
            this.startTime = DelayAction.TickCount;
            this.endTime = this.startTime + renderTime;
            this.renderPosition = renderPosition;

            this.color = color;

            this.text = text;
        }

        override public void Draw()
        {
            if (renderPosition.IsOnScreen())
            {
                var textDimension = TextUtils.GetTextExtent(text);
                var wardScreenPos = Drawing.WorldToScreen(renderPosition.To3D());

                TextUtils.DrawText(wardScreenPos.X - textDimension.Width / 2, wardScreenPos.Y, color, text);
            }
        }
    }

    class CooldownBar : RenderObject
    {
        public float duration = 0;
        public Obj_AI_Base obj;
        public Vector2 position = Vector2.Zero;
        public float extraHeight = 0;

        public CooldownBar(Vector2 position, float duration, float extraHeight = 0, float startTime = 0)
        {
            this.startTime = startTime == 0 ? DelayAction.TickCount : startTime;
            this.endTime = this.startTime + duration;
            this.position = position;

            this.duration = duration;
            this.extraHeight = extraHeight;
        }

        public CooldownBar(Obj_AI_Base obj, float duration, float extraHeight = 0, float startTime = 0)
        {
            this.startTime = startTime == 0 ? DelayAction.TickCount : startTime;
            this.endTime = this.startTime + duration;
            this.obj = obj;

            this.duration = duration;
            this.extraHeight = extraHeight;
        }

        override public void Draw()
        {
            var pos = position != Vector2.Zero ? position : obj.Position.To2D();

            if (pos.IsOnScreen())
            {
                pos = Drawing.WorldToScreen(pos.To3D());
                pos.X -= 25;

                var percent = (endTime - DelayAction.TickCount) / duration;

                if (percent > 0)
                {
                    Drawing.DrawLine(new Vector2(pos.X - 1, pos.Y - 1 + extraHeight),
                                 new Vector2(pos.X + 1 + 50 * percent, pos.Y - 1 + extraHeight),
                                 7, System.Drawing.Color.Black);

                    Drawing.DrawLine(new Vector2(pos.X, pos.Y + extraHeight),
                                     new Vector2(pos.X + 50 * percent, pos.Y + extraHeight),
                                     5, System.Drawing.Color.LightGray);
                }
            }
        }
    }
}
