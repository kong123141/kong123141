using System;
using LeagueSharp;
using SharpDX;
using Notification = LeagueSharp.Common.Notification;
using LeagueSharp.Common;

namespace 花边_花式多合一.Core
{
    class ShowWindUp
    {
        public static double windup;
        private static Notification _modeNotificationHandler;

        internal static void Game_OnGameLoad(EventArgs args)
        {
            try
            {
                if (!InitializeMenu.Menu.Item("WindUpEnable").GetValue<bool>()) return;
                Game.OnUpdate += Game_OnUpdate;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ShowWindUp error occurred: '{0}'", ex);
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!InitializeMenu.Menu.Item("WindUpEnable").GetValue<bool>())
            {

            }
            else
            {
                NotificationShowwindUp();

                LoadHuabianAAhouyao();
            }
        }

        private static void LoadHuabianAAhouyao()
        {
            var additional = 0;
            if (Game.Ping >= 100)
                additional = Game.Ping / 100 * 5;
            else if (Game.Ping > 40 && Game.Ping < 100)
                additional = Game.Ping / 100 * 10;
            else if (Game.Ping <= 40)
                additional = +15;

            var windUp = Game.Ping - 20 + additional;

            if (windUp < 40 && 20 < windUp)
                windUp = 36;
            else if (windUp < 20 && 10 < windUp)
                windUp = 14;
            else if (windUp < 10)
                windUp = 5;
        }

        private static void NotificationShowwindUp()
        {
            var text = "鑺辫竟鎺ㄨ崘璁剧疆: " + windup;

            if (_modeNotificationHandler == null)
            {
                _modeNotificationHandler = new Notification(text)
                {
                    TextColor = new ColorBGRA(124, 252, 0, 255)
                };
                Notifications.AddNotification("花边提示最佳AA后摇");
                Notifications.AddNotification(_modeNotificationHandler);
            }

            _modeNotificationHandler.Text = text;
        }
    }
}
