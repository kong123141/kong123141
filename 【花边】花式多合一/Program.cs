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

                #region 由于太过花式不使用这个了
                /*
                  if (InitializeMenu.Menu.Item("AutoDisableDrawingEnable").GetValue<bool>())
                  {
                      我花边不服.OnGameLoad += AutoDisableDrawing.Game_OnGameLoad;
                  }
                  else
                  {
                      我花边不服.OnGameLoad -= AutoDisableDrawing.Game_OnGameLoad;
                  }

                  if (InitializeMenu.Menu.Item("AutoKs").GetValue<bool>())
                  {
                      我花边不服.OnGameLoad += AutoKillSteal.Game_OnGameLoad;
                  }
                  else
                  {
                      我花边不服.OnGameLoad -= AutoKillSteal.Game_OnGameLoad;
                  }

                  if (!InitializeMenu.Menu.Item("AutoLanter").GetValue<bool>())
                  {
                      我花边不服.OnGameLoad -= AutoLantern.Game_OnGameLoad;
                  }
                  else
                  {
                      我花边不服.OnGameLoad += AutoLantern.Game_OnGameLoad;
                  }

                  if (!InitializeMenu.Menu.Item("CheckEnable").GetValue<bool>())
                  {
                      我花边不服.OnGameLoad -= CheckMoreL.Game_OnGameLoad;
                  }
                  else
                  {
                      我花边不服.OnGameLoad += CheckMoreL.Game_OnGameLoad;
                  }

                  if (!InitializeMenu.Menu.Item("Explore").GetValue<bool>())
                  {
                      我花边不服.OnGameLoad -= Explore.Game_OnGameLoad;
                  }
                  else
                  {
                      我花边不服.OnGameLoad += Explore.Game_OnGameLoad;
                  }

                  if (!InitializeMenu.Menu.Item("ward").GetValue<bool>())
                  {
                      我花边不服.OnGameLoad -= GlassWard.Game_OnGameLoad;
                  }
                  else
                  {
                      我花边不服.OnGameLoad += GlassWard.Game_OnGameLoad;
                  }

                  if (!InitializeMenu.Menu.Item("Humanizer").GetValue<bool>())
                  {
                      我花边不服.OnGameLoad -= Humanizer.Game_OnGameLoad;
                  }
                  else
                  {
                      我花边不服.OnGameLoad += Humanizer.Game_OnGameLoad;
                  }

                  if (!InitializeMenu.Menu.Item("wushangdaye").GetValue<bool>())
                  {
                      我花边不服.OnGameLoad -= JunglePosition.Game_OnGameLoad;
                  }
                  else
                  {
                      我花边不服.OnGameLoad += JunglePosition.Game_OnGameLoad;
                  }

                  if (!InitializeMenu.Menu.Item("JungleActive").GetValue<bool>())
                  {
                      我花边不服.OnGameLoad -= JungleTimer.Game_OnGameLoad;
                  }
                  else
                  {
                      我花边不服.OnGameLoad += JungleTimer.Game_OnGameLoad;
                  }

                  if (!InitializeMenu.Menu.Item("TimeEnable").GetValue<bool>())
                  {
                      我花边不服.OnGameLoad -= ShowTimes.Game_OnGameLoad;
                  }
                  else
                  {
                      我花边不服.OnGameLoad += ShowTimes.Game_OnGameLoad;
                  }

                  if (InitializeMenu.Menu.Item("HealthActive").GetValue<bool>())
                  {
                      我花边不服.OnGameLoad += TowerHealth.Game_OnGameLoad;
                  }
                  else
                  {
                      我花边不服.OnGameLoad -= TowerHealth.Game_OnGameLoad;
                  }

                  if (!InitializeMenu.Menu.Item("RangeEnabled").GetValue<bool>())
                  {
                      我花边不服.OnGameLoad -= TowerRange.Game_OnGameLoad;
                  }
                  else
                  {
                      我花边不服.OnGameLoad += TowerRange.Game_OnGameLoad;
                  }

                  if (InitializeMenu.Menu.Item("TrackEnemyWards").GetValue<bool>())
                  {
                      我花边不服.OnGameLoad += TrackWards.Game_OnGameLoad;
                  }
                  else
                  {
                      我花边不服.OnGameLoad -= TrackWards.Game_OnGameLoad;
                  }

                  if (InitializeMenu.Menu.Item("TrackEnemySpells").GetValue<bool>())
                  {
                      我花边不服.OnGameLoad += TrackEnemySpells.Game_OnGameLoad;
                  }
                  else
                  {
                      我花边不服.OnGameLoad -= TrackEnemySpells.Game_OnGameLoad;
                  }

                  if (!InitializeMenu.Menu.Item("AutoSmite").GetValue<bool>())
                  {
                      我花边不服.OnGameLoad -= AutoSmite.Game_OnGameLoad;
                  }
                  else
                  {
                      我花边不服.OnGameLoad += AutoSmite.Game_OnGameLoad;
                  }

                  if (!InitializeMenu.Menu.Item("AutoZhongya").GetValue<bool>())
                  {
                      我花边不服.OnGameLoad -= AutoZhongya.Game_OnGameLoad;
                  }
                  else
                  {
                      我花边不服.OnGameLoad += AutoZhongya.Game_OnGameLoad;
                  }

                  if (!InitializeMenu.Menu.Item("AutoTurnAround").GetValue<bool>())
                  {
                      我花边不服.OnGameLoad -= TurnAround.Game_OnGameLoad;
                  }
                  else
                  {
                      我花边不服.OnGameLoad += TurnAround.Game_OnGameLoad;
                  }

                  if (InitializeMenu.Menu.Item("ENABLEWDHG").GetValue<bool>() || InitializeMenu.Menu.Item("Fanyin").GetValue<bool>())
                  {
                      我花边不服.OnGameLoad += WhereDidHeGo.Game_OnGameLoad;
                  }
                  else
                  {
                      我花边不服.OnGameLoad -= WhereDidHeGo.Game_OnGameLoad;
                  }
                  
                  if (!InitializeMenu.Menu.Item("WindUpEnable").GetValue<bool>())
                  {
                      我花边不服.OnGameLoad -= WindUpEnable.Game_OnGameLoad;
                  }
                  else
                  {
                      我花边不服.OnGameLoad += WindUpEnable.Game_OnGameLoad;
                  }

                  if (!InitializeMenu.Menu.Item("WardJumpEnable").GetValue<bool>())
                  {
                      我花边不服.OnGameLoad -= WardJump.Game_OnGameLoad;
                  }
                  else
                  {
                      我花边不服.OnGameLoad += WardJump.Game_OnGameLoad;
                  }

                  if (!InitializeMenu.Menu.Item("SongrentoupEnable").GetValue<bool>())
                  {
                      我花边不服.OnGameLoad -= Songrentou.Game_OnGameLoad;
                  }
                  else
                  {
                      我花边不服.OnGameLoad += Songrentou.Game_OnGameLoad;
                  }
                  
                  if (!InitializeMenu.Menu.Item("AutoLevelsEnable").GetValue<bool>())
                  {
                      我花边不服.OnGameLoad -= AutoLevels.Game_OnGameLoad;
                  }
                  else
                  {
                      我花边不服.OnGameLoad += AutoLevels.Game_OnGameLoad;
                  }
                */

                #endregion

                #region  改用了如此不花式如此丑逼

                我花边不服.OnGameLoad += WhereDidHeGo.Game_OnGameLoad;
                我花边不服.OnGameLoad += TurnAround.Game_OnGameLoad;
                我花边不服.OnGameLoad += AutoZhongya.Game_OnGameLoad;
                我花边不服.OnGameLoad += AutoSmite.Game_OnGameLoad;
                我花边不服.OnGameLoad += AutoSmite.Game_OnGameLoad;
                我花边不服.OnGameLoad += TrackEnemySpells.Game_OnGameLoad;
                我花边不服.OnGameLoad += TrackWards.Game_OnGameLoad;
                我花边不服.OnGameLoad += TowerRange.Game_OnGameLoad;
                我花边不服.OnGameLoad += TowerHealth.Game_OnGameLoad;
                我花边不服.OnGameLoad += ShowTimes.Game_OnGameLoad;
                我花边不服.OnGameLoad += JungleTimer.Game_OnGameLoad;
                我花边不服.OnGameLoad += JunglePosition.Game_OnGameLoad;
                我花边不服.OnGameLoad += GlassWard.Game_OnGameLoad;
                我花边不服.OnGameLoad += Explore.Game_OnGameLoad;
                我花边不服.OnGameLoad += AutoLantern.Game_OnGameLoad;
                我花边不服.OnGameLoad += AutoKillSteal.Game_OnGameLoad;
                我花边不服.OnGameLoad += AutoDisableDrawing.Game_OnGameLoad;
                我花边不服.OnGameLoad += CheckMoreL.Game_OnGameLoad;
                我花边不服.OnGameLoad += ShowWindUp.Game_OnGameLoad;
                我花边不服.OnGameLoad += WardJump.Game_OnGameLoad;
                我花边不服.OnGameLoad += Songrentou.Game_OnGameLoad;
                我花边不服.OnGameLoad += AutoLevels.Game_OnGameLoad;
                我花边不服.OnGameLoad += SharpExperience.Game_OnGameLoad;
                我花边不服.OnGameLoad += JungleTracker.Game_OnGameLoad;
                我花边不服.OnGameLoad += Activators.Game_OnGameLoad;
                我花边不服.OnGameLoad += CheckVersion.Game_OnGameLoad;

                //我花边不服.OnGameLoad += Humanizer.Game_OnGameLoad;
                //我花边不服.OnGameLoad += Junglest.Game_OnGameLoad;

                #endregion

                if (InitializeMenu.Menu.Item("Lost").GetValue<bool>())
                {
                    PrintChat.LoadNotifications();
                    PrintChat.LoadPrintChat();
                }

                //Game.OnUpdate += Game_OnUpdate;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Loaded error occurred: '{0}'", ex);
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (InitializeMenu.Menu.Item("AutoDisableDrawingEnable").GetValue<bool>())
            {
                我花边不服.OnGameLoad += AutoDisableDrawing.Game_OnGameLoad;
            }

            if (!InitializeMenu.Menu.Item("AutoDisableDrawingEnable").GetValue<bool>())
            {
                我花边不服.OnGameLoad -= AutoDisableDrawing.Game_OnGameLoad;
            }

            if (InitializeMenu.Menu.Item("JungleslackEnable").GetValue<bool>())
            {
                我花边不服.OnGameLoad += Junglest.Game_OnGameLoad;
            }

            if (!InitializeMenu.Menu.Item("JungleslackEnable").GetValue<bool>())
            {
                我花边不服.OnGameLoad -= Junglest.Game_OnGameLoad;
            }

            if (InitializeMenu.Menu.Item("AutoKs").GetValue<bool>())
            {
                我花边不服.OnGameLoad += AutoKillSteal.Game_OnGameLoad;
            }

            if (!InitializeMenu.Menu.Item("AutoKs").GetValue<bool>())
            {
                我花边不服.OnGameLoad -= AutoKillSteal.Game_OnGameLoad;
            }

            if (!InitializeMenu.Menu.Item("AutoLanter").GetValue<bool>())
            {
                我花边不服.OnGameLoad -= AutoLantern.Game_OnGameLoad;
            }

            if (InitializeMenu.Menu.Item("AutoLanter").GetValue<bool>())
            {
                我花边不服.OnGameLoad += AutoLantern.Game_OnGameLoad;
            }

            if (!InitializeMenu.Menu.Item("CheckEnable").GetValue<bool>())
            {
                我花边不服.OnGameLoad -= CheckMoreL.Game_OnGameLoad;
            }

            if (InitializeMenu.Menu.Item("CheckEnable").GetValue<bool>())
            {
                我花边不服.OnGameLoad += CheckMoreL.Game_OnGameLoad;
            }

            if (!InitializeMenu.Menu.Item("Explore").GetValue<bool>())
            {
                我花边不服.OnGameLoad -= Explore.Game_OnGameLoad;
            }

            if (InitializeMenu.Menu.Item("Explore").GetValue<bool>())
            {
                我花边不服.OnGameLoad += Explore.Game_OnGameLoad;
            }

            if (!InitializeMenu.Menu.Item("ward").GetValue<bool>())
            {
                我花边不服.OnGameLoad -= GlassWard.Game_OnGameLoad;
            }

            if (InitializeMenu.Menu.Item("ward").GetValue<bool>())
            {
                我花边不服.OnGameLoad += GlassWard.Game_OnGameLoad;
            }

            if (!InitializeMenu.Menu.Item("Humanizer").GetValue<bool>())
            {
                我花边不服.OnGameLoad -= Humanizer.Game_OnGameLoad;
            }

            if (InitializeMenu.Menu.Item("Humanizer").GetValue<bool>())
            {
                我花边不服.OnGameLoad += Humanizer.Game_OnGameLoad;
            }

            if (!InitializeMenu.Menu.Item("wushangdaye").GetValue<bool>())
            {
                我花边不服.OnGameLoad -= JunglePosition.Game_OnGameLoad;
            }

            if (InitializeMenu.Menu.Item("wushangdaye").GetValue<bool>())
            {
                我花边不服.OnGameLoad += JunglePosition.Game_OnGameLoad;
            }

            if (!InitializeMenu.Menu.Item("JungleActive").GetValue<bool>())
            {
                我花边不服.OnGameLoad -= JungleTimer.Game_OnGameLoad;
            }

            if (InitializeMenu.Menu.Item("JungleActive").GetValue<bool>())
            {
                我花边不服.OnGameLoad += JungleTimer.Game_OnGameLoad;
            }

            if (!InitializeMenu.Menu.Item("TimeEnable").GetValue<bool>())
            {
                我花边不服.OnGameLoad -= ShowTimes.Game_OnGameLoad;
            }

            if (InitializeMenu.Menu.Item("TimeEnable").GetValue<bool>())
            {
                我花边不服.OnGameLoad += ShowTimes.Game_OnGameLoad;
            }

            if (InitializeMenu.Menu.Item("HealthActive").GetValue<bool>())
            {
                我花边不服.OnGameLoad += TowerHealth.Game_OnGameLoad;
            }

            if (!InitializeMenu.Menu.Item("HealthActive").GetValue<bool>())
            {
                我花边不服.OnGameLoad -= TowerHealth.Game_OnGameLoad;
            }

            if (!InitializeMenu.Menu.Item("RangeEnabled").GetValue<bool>())
            {
                我花边不服.OnGameLoad -= TowerRange.Game_OnGameLoad;
            }

            if (InitializeMenu.Menu.Item("RangeEnabled").GetValue<bool>())
            {
                我花边不服.OnGameLoad += TowerRange.Game_OnGameLoad;
            }

            if (InitializeMenu.Menu.Item("TrackEnemyWards").GetValue<bool>())
            {
                我花边不服.OnGameLoad += TrackWards.Game_OnGameLoad;
            }

            if (!InitializeMenu.Menu.Item("TrackEnemyWards").GetValue<bool>())
            {
                我花边不服.OnGameLoad -= TrackWards.Game_OnGameLoad;
            }

            if (InitializeMenu.Menu.Item("TrackEnemySpells").GetValue<bool>())
            {
                我花边不服.OnGameLoad += TrackEnemySpells.Game_OnGameLoad;
            }

            if (!InitializeMenu.Menu.Item("TrackEnemySpells").GetValue<bool>())
            {
                我花边不服.OnGameLoad -= TrackEnemySpells.Game_OnGameLoad;
            }

            if (!InitializeMenu.Menu.Item("AutoSmite").GetValue<bool>())
            {
                我花边不服.OnGameLoad -= AutoSmite.Game_OnGameLoad;
            }

            if (InitializeMenu.Menu.Item("AutoSmite").GetValue<bool>())
            {
                我花边不服.OnGameLoad += AutoSmite.Game_OnGameLoad;
            }

            if (!InitializeMenu.Menu.Item("AutoZhongya").GetValue<bool>())
            {
                我花边不服.OnGameLoad -= AutoZhongya.Game_OnGameLoad;
            }

            if (InitializeMenu.Menu.Item("AutoZhongya").GetValue<bool>())
            {
                我花边不服.OnGameLoad += AutoZhongya.Game_OnGameLoad;
            }

            if (!InitializeMenu.Menu.Item("AutoTurnAround").GetValue<bool>())
            {
                我花边不服.OnGameLoad -= TurnAround.Game_OnGameLoad;
            }

            if (InitializeMenu.Menu.Item("AutoTurnAround").GetValue<bool>())
            {
                我花边不服.OnGameLoad += TurnAround.Game_OnGameLoad;
            }

            if (InitializeMenu.Menu.Item("ENABLEWDHG").GetValue<bool>() || InitializeMenu.Menu.Item("Fanyin").GetValue<bool>())
            {
                我花边不服.OnGameLoad += WhereDidHeGo.Game_OnGameLoad;
            }

            if (!InitializeMenu.Menu.Item("ENABLEWDHG").GetValue<bool>() || !InitializeMenu.Menu.Item("Fanyin").GetValue<bool>())
            {
                我花边不服.OnGameLoad -= WhereDidHeGo.Game_OnGameLoad;
            }

            if (!InitializeMenu.Menu.Item("WindUpEnable").GetValue<bool>())
            {
                我花边不服.OnGameLoad -= ShowWindUp.Game_OnGameLoad;
            }

            if (InitializeMenu.Menu.Item("WindUpEnable").GetValue<bool>())
            {
                我花边不服.OnGameLoad += ShowWindUp.Game_OnGameLoad;
            }

            if (!InitializeMenu.Menu.Item("WardJumpEnable").GetValue<bool>())
            {
                我花边不服.OnGameLoad -= WardJump.Game_OnGameLoad;
            }
            if (InitializeMenu.Menu.Item("WardJumpEnable").GetValue<bool>())
            {
                我花边不服.OnGameLoad += WardJump.Game_OnGameLoad;
            }

            if (!InitializeMenu.Menu.Item("SongrentoupEnable").GetValue<bool>())
            {
                我花边不服.OnGameLoad -= Songrentou.Game_OnGameLoad;
            }

            if (InitializeMenu.Menu.Item("SongrentoupEnable").GetValue<bool>())
            {
                我花边不服.OnGameLoad += Songrentou.Game_OnGameLoad;
            }

            if (!InitializeMenu.Menu.Item("AutoLevelsEnable").GetValue<bool>())
            {
                我花边不服.OnGameLoad -= AutoLevels.Game_OnGameLoad;
            }

            if (InitializeMenu.Menu.Item("AutoLevelsEnable").GetValue<bool>())
            {
                我花边不服.OnGameLoad += AutoLevels.Game_OnGameLoad;
            }

        }
    }
}
