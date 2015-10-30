#region 引用
using System;
using LeagueSharp;
using 花边_花式多合一.Core;
using 我花边不服 = LeagueSharp.Common.CustomEvents.Game;
#endregion

namespace 花边_花式多合一
{
    class Program
    {

        static void Main(string[] args)
        {
            我花边不服.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            try
            {
                InitializeMenu.Load多合一Menu();

                new WhereDidHeGo.Load();
                new TurnAround.Load();
                我花边不服.OnGameLoad += AutoZhongya.Game_OnGameLoad;
                我花边不服.OnGameLoad += AutoSmite.Game_OnGameLoad;
                我花边不服.OnGameLoad += AutoSmite.Game_OnGameLoad;
                我花边不服.OnGameLoad += TrackEnemySpells.Game_OnGameLoad;
                我花边不服.OnGameLoad += TrackWards.Game_OnGameLoad;
                我花边不服.OnGameLoad += TowerRange.Game_OnGameLoad;
                我花边不服.OnGameLoad += TowerHealth.Game_OnGameLoad;
                我花边不服.OnGameLoad += ShowTimes.Game_OnGameLoad;
                我花边不服.OnGameLoad += JungleTimer.Game_OnGameLoad;
                我花边不服.OnGameLoad += GlassWard.Game_OnGameLoad;
                我花边不服.OnGameLoad += Explore.Game_OnGameLoad;
                我花边不服.OnGameLoad += AutoLantern.Game_OnGameLoad;
                我花边不服.OnGameLoad += AutoKillSteal.Game_OnGameLoad;
                我花边不服.OnGameLoad += AutoDisableDrawing.Game_OnGameLoad;
                我花边不服.OnGameLoad += ShowWindUp.Game_OnGameLoad;
                我花边不服.OnGameLoad += WardJump.Game_OnGameLoad;
                我花边不服.OnGameLoad += Songrentou.Game_OnGameLoad;
                我花边不服.OnGameLoad += AutoLevels.Game_OnGameLoad;
                我花边不服.OnGameLoad += SharpExperience.Game_OnGameLoad;
                我花边不服.OnGameLoad += JungleTracker.Game_OnGameLoad;
                我花边不服.OnGameLoad += Activators.Game_OnGameLoad;
                我花边不服.OnGameLoad += CheckVersion.Game_OnGameLoad;



                //我花边不服.OnGameLoad += CheckMoreL.Game_OnGameLoad;
                //我花边不服.OnGameLoad += Humanizer.Game_OnGameLoad;
                //我花边不服.OnGameLoad += Junglest.Game_OnGameLoad;
                //我花边不服.OnGameLoad += JunglePosition.Game_OnGameLoad;

                if (InitializeMenu.Menu.Item("Lost").GetValue<bool>())
                {
                    PrintChat.LoadNotifications();
                    PrintChat.LoadPrintChat();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Loaded error occurred: '{0}'", ex);
            }
        }
    }
}
