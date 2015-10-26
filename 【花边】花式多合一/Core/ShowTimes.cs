using System;
using SharpDX.Direct3D9;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;
using Font = SharpDX.Direct3D9.Font;

namespace 花边_花式多合一.Core
{
    class ShowTimes
    {
        private static Font TimeFont;

        private static void Drawing_OnDraw(EventArgs args)
        {
            var Enable = InitializeMenu.Menu.Item("TimeEnable").GetValue<bool>();
            if (!Enable)
            {
                return;
            }
            //var SelectColor = InitializeMenu.Menu.Item("颜色").GetValue<Color>();
            /*Drawing.DrawText(
                Drawing.Width - InitializeMenu.Menu.Item("atRight").GetValue<Slider>().Value,
                InitializeMenu.Menu.Item("atTop").GetValue<Slider>().Value,
                SelectColor, "Now Time:" + DateTime.Now.ToShortTimeString());*/
            FlowersDrawing.DrawText(TimeFont, "现在时间:" + DateTime.Now.ToShortTimeString(),
                Drawing.Width - InitializeMenu.Menu.Item("atRight").GetValue<Slider>().Value,
                InitializeMenu.Menu.Item("atTop").GetValue<Slider>().Value,
                 SharpDX.Color.SkyBlue);
        }

        internal static void Game_OnGameLoad(EventArgs args)
        {
            try
            {
                var Enable = InitializeMenu.Menu.Item("TimeEnable").GetValue<bool>();
                if (!Enable)
                {
                    return;
                }
                TimeFont = new Font(Drawing.Direct3DDevice, new System.Drawing.Font("微软雅黑", 15));

                Drawing.OnDraw += Drawing_OnDraw;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ShowTimes error occurred: '{0}'", ex);
            }
        }
    }
}
