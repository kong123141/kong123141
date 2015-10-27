using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace 花边_花式多合一.Core
{
    class JunglePosition
    {
        internal static void Game_OnGameLoad(EventArgs args)
        {
            try
            {
                if (!InitializeMenu.Menu.Item("wushangdaye").GetValue<bool>()) return;
                Drawing.OnDraw += Drawing_OnDraw;
            }
            catch (Exception ex)
            {
                Console.WriteLine("JunglePosition error occurred: '{0}'", ex);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Game.MapId == (GameMapId)11 && InitializeMenu.Menu.Item("wushangdaye").GetValue<bool>())
            {
                const float circleRange = 100f;

                Render.Circle.DrawCircle(new Vector3(7461.018f, 3253.575f, 52.57141f), circleRange, System.Drawing.Color.Orange, 2); // blue team :red
                Render.Circle.DrawCircle(new Vector3(3511.601f, 8745.617f, 52.57141f), circleRange, System.Drawing.Color.Orange, 2); // blue team :blue
                Render.Circle.DrawCircle(new Vector3(7462.053f, 2489.813f, 52.57141f), circleRange, System.Drawing.Color.Orange, 2); // blue team :golems
                Render.Circle.DrawCircle(new Vector3(3144.897f, 7106.449f, 51.89026f), circleRange, System.Drawing.Color.Orange, 2); // blue team :wolfs
                Render.Circle.DrawCircle(new Vector3(7770.341f, 5061.238f, 49.26587f), circleRange, System.Drawing.Color.Orange, 2); // blue team :wariaths
                Render.Circle.DrawCircle(new Vector3(10930.93f, 5405.83f, -68.72192f), circleRange, System.Drawing.Color.Red, 2); // Dragon
                Render.Circle.DrawCircle(new Vector3(7326.056f, 11643.01f, 50.21985f), circleRange, System.Drawing.Color.Orange, 2); // red team :red
                Render.Circle.DrawCircle(new Vector3(11417.6f, 6216.028f, 51.00244f), circleRange, System.Drawing.Color.Orange, 2); // red team :blue
                Render.Circle.DrawCircle(new Vector3(7368.408f, 12488.37f, 56.47668f), circleRange, System.Drawing.Color.Orange, 2); // red team :golems
                Render.Circle.DrawCircle(new Vector3(10342.77f, 8896.083f, 51.72742f), circleRange, System.Drawing.Color.Orange, 2); // red team :wolfs
                Render.Circle.DrawCircle(new Vector3(7001.741f, 9915.717f, 54.02466f), circleRange, System.Drawing.Color.Orange, 2); // red team :wariaths                    
            }

        }
    }
}
