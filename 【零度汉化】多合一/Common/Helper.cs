namespace Flowers_Utility.Common
{
    using System;
    using SharpDX;
    using SharpDX.Direct3D9;
    using LeagueSharp;
    using LeagueSharp.Common;
    public static class Helper
    {
        public static DateTime assemblyLoadTime = DateTime.Now;

        static Helper()
        {
            //ignore
        }

        public static MenuItem GetSlider(this Menu Menu, string name)
        {
            return Menu.Item(name + ObjectManager.Player.ChampionName);
        }

        public static void JungleTimeText(Font font, String text, int posX, int posY, Color color)
        {
            Rectangle rec = font.MeasureText(null, text, FontDrawFlags.Center);
            font.DrawText(null, text, posX + 1 + rec.X, posY + 1, Color.Black);
            font.DrawText(null, text, posX + rec.X, posY + 1, Color.Black);
            font.DrawText(null, text, posX - 1 + rec.X, posY - 1, Color.Black);
            font.DrawText(null, text, posX + rec.X, posY - 1, Color.Black);
            font.DrawText(null, text, posX + rec.X, posY, color);
        }

        public static string JungleFormatTime(double time)
        {
            TimeSpan t = TimeSpan.FromSeconds(time);
            if (t.Minutes > 0)
            {
                return string.Format("{0:D1}:{1:D2}", t.Minutes, t.Seconds);
            }
            return string.Format("{0:D}", t.Seconds);
        }

        public static void HealthDrawText(Font font, string text, int posX, int posY, Color color)
        {
            Rectangle rec = font.MeasureText(null, text, FontDrawFlags.Center);
            font.DrawText(null, text, posX + 1 + rec.X, posY + 1, Color.Black);
            font.DrawText(null, text, posX + rec.X, posY + 1, Color.Black);
            font.DrawText(null, text, posX - 1 + rec.X, posY - 1, Color.Black);
            font.DrawText(null, text, posX + rec.X, posY - 1, Color.Black);
            font.DrawText(null, text, posX + rec.X, posY, color);
        }

        public static Font HealthText = new Font(Drawing.Direct3DDevice, new FontDescription
        {
            FaceName = "Calibri",
            Height = 13,
            OutputPrecision = FontPrecision.Default,
            Quality = FontQuality.Default,
        });

        public static float TickCount
        {
            get
            {
                return (int)DateTime.Now.Subtract(assemblyLoadTime).TotalMilliseconds;
            }
        }
    }
}
