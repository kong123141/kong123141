using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using SharpDX;
using LeagueSharp;
using LeagueSharp.Common;

namespace EnemyHealth
{
    class Program
    {
        internal static Menu MyMenu;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            MyMenu = new Menu("EnemyHealth#","dz191.eh",true);
            {
                MyMenu.AddItem(new MenuItem("dz191.eh.enable", "Enabled").SetValue(true));
            }
            MyMenu.AddToMainMenu();
            Drawing.OnDraw += Drawing_OnDraw;
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (!MyMenu.Item("dz191.eh.enable").GetValue<bool>())
            {
                return;
            }

            foreach (var enemy in HeroManager.Enemies.Where(h => h.IsHPBarRendered))
            {
                var barPosition = enemy.HPBarPosition;
                var x = barPosition.X;
                var y = barPosition.Y;
                var h2 = Math.Round(enemy.Health, 0).ToString(CultureInfo.InvariantCulture);

                Drawing.DrawText(x - 20 - TextWidth(h2, new Font("Calibri", 4)), y + 5, System.Drawing.Color.Orange, h2.ToString(CultureInfo.InvariantCulture));
            }
        }

        public static float TextWidth(string text, Font f)
        {
            float textWidth;

            using (var bmp = new Bitmap(1, 1))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    textWidth = g.MeasureString(text, f).Width;
                }
            }

            return textWidth;
        }
    }
}
