using System;
using Lost = LeagueSharp.Hacks;
using LeagueSharp;
using LeagueSharp.Common;

namespace 花边_花式多合一.Core
{
    class AutoDisableDrawing
    {
        static int 杀人数 = InitializeMenu.Menu.Item("已连杀人数").GetValue<Slider>().Value;

        private static void Game_OnStart(EventArgs args)
        {
            杀人数 = 0;
        }

        private static void Game_OnNotify(GameNotifyEventArgs args)
        {
            if (args.EventId == GameEventId.OnGameStart || args.EventId == GameEventId.OnEndGame)
            {
                杀人数 = 0;
            }

            if (args.EventId == GameEventId.OnKill && args.NetworkId == ObjectManager.Player.NetworkId)
            {

                杀人数 = 0;
            }

            var time = InitializeMenu.Menu.Item("多杀屏蔽时间").GetValue<Slider>().Value;

            if (args.NetworkId == ObjectManager.Player.NetworkId
                && (args.EventId == GameEventId.OnChampionTripleKill
                || args.EventId == GameEventId.OnChampionQuadraKill
                || args.EventId == GameEventId.OnChampionPentaKill
                || args.EventId == GameEventId.OnAce)
                && Lost.DisableDrawings == false)
            {
                if (InitializeMenu.Menu.Item("多杀屏蔽显示").GetValue<bool>())
                {
                    //int time = InitializeMenu.Menu.Item("多杀屏蔽时间").GetValue<Slider>().Value;
                    Lost.DisableDrawings = true;
                    DelayAction.Add(time * 1000, () =>
                    {
                        Lost.DisableDrawings = false;
                    });
                }
            }

            if (args.EventId == GameEventId.OnChampionDie && args.NetworkId == ObjectManager.Player.NetworkId)
            {
                杀人数 += InitializeMenu.Menu.Item("已连杀人数").GetValue<Slider>().Value + 1;

                if (杀人数 >= 8
                    && InitializeMenu.Menu.Item("超神屏蔽显示").GetValue<bool>()
                    && Lost.DisableDrawings == false)
                {
                    //int time = InitializeMenu.Menu.Item("多杀屏蔽时间").GetValue<Slider>().Value;

                    Lost.DisableDrawings = true;
                    DelayAction.Add(time * 1000, () =>
                    {
                        Lost.DisableDrawings = false;
                    });
                }
            }
        }

        internal static void Game_OnGameLoad(EventArgs args)
        {
            try
            {
                if (!InitializeMenu.Menu.Item("AutoDisableDrawingEnable").GetValue<bool>()) return;

                Game.OnNotify += Game_OnNotify;
                Game.OnStart += Game_OnStart;
            }
            catch (Exception ex)
            {
                Console.WriteLine("AutoDisableDrawing error occurred: '{0}'", ex);
            }
        }
    }
}
