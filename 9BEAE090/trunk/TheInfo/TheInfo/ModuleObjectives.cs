using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using TheInfo.Objectives;
using TheInfo.Objectives.Items;
using Color = System.Drawing.Color;

namespace TheInfo
{
    class ModuleObjectives : IModule
    {
        private Menu _objectives, _ending;
        private ObjectiveCalc _calc;
        private ObjectiveCalc.ObjectiveResult[] _clickedResults;
        private ObjectiveCalc.ObjectiveResult[] _endResults;
        private float _lastTick;
        private bool _fightWon;
        private bool _shouldEnd;

        private DeathTracker _deathTracker;
        private InhibitorTracker _inhibTracker;

        public void Initialize()
        {
            _calc = new ObjectiveCalc();
            _deathTracker = new DeathTracker(ObjectManager.Player.Team == GameObjectTeam.Chaos ? GameObjectTeam.Order : GameObjectTeam.Chaos);
            _inhibTracker = new InhibitorTracker(ObjectiveCalc.Objectives.Where(objective => objective.GetType() == typeof(ObjectiveInhibitor)).Cast<ObjectiveInhibitor>().ToArray());

            Drawing.OnDraw += Draw;
            Game.OnUpdate += Tick;
        }

        public void InitializeMenu(Menu rootMenu)
        {
            _objectives = new Menu("Objectives Helper", "objectiveshelper");

            _ending = new Menu("End Possibilities", "endpossibilities");

            _ending.AddItem(new MenuItem("displayendpossibilities", "Display end possibilities").SetValue(true));
            _ending.AddItem(new MenuItem("notification", "Notification").SetValue(true));
            _ending.AddItem(new MenuItem("drawtext", "Draw text").SetValue(true));

            _objectives.AddItem(new MenuItem("canclickobjectives", "Can click objectives").SetValue(true));
            _objectives.AddItem(new MenuItem("calculateexactinfo", "Drops FPS below hell:"));
            _objectives.AddItem(new MenuItem("calculateexact", "Calculate Exact").SetValue(false));

            _ending.Item("displayendpossibilities").ValueChanged += (sender, valArgs) => _endResults = null;
            _objectives.Item("canclickobjectives").ValueChanged += (sender, valArgs) => _clickedResults = null;
            _objectives.Item("calculateexact").ValueChanged += (sender, valArgs) => _calc.CalculateExact = valArgs.GetNewValue<bool>();

            _objectives.AddSubMenu(_ending);
            rootMenu.AddSubMenu(_objectives);


        }

        private void Draw(EventArgs args)
        {
            if (ObjectiveCalc.SelectedObjective != null && _objectives.Item("canclickobjectives").GetValue<bool>())
                Render.Circle.DrawCircle(new Vector3(ObjectiveCalc.SelectedObjective.Position.X, ObjectiveCalc.SelectedObjective.Position.Y, 0), 200, System.Drawing.Color.Blue, 10);
        }

        private void Tick(EventArgs args)
        {
            #region large tick
            if (_lastTick < Game.Time)
            {
                _lastTick = Game.Time + 0.10f;

                var deadEnemies = HeroManager.Enemies.Count(enemy => enemy.IsDead);
                var deadAllies = HeroManager.Allies.Count(ally => ally.IsDead);

                // var profiler1 = Stopwatch.StartNew();
                if (_ending.Item("displayendpossibilities").GetValue<bool>() && (deadEnemies > deadAllies + 2 || deadEnemies == HeroManager.Enemies.Count)) // -> if 3 more allies are alive than enemies or enemies aced
                {
                    var chain = _calc.GetObjectiveChainsTo(_calc.GetObjective<ObjectiveBase>());
                    _endResults = new ObjectiveCalc.ObjectiveResult[chain.Count];
                    for (int i = 0; i < chain.Count; i++)
                        _endResults[i] = _calc.GetNeededTime(chain[i], null);


                    if ((!_fightWon))
                    {
                        var orderedResults = _endResults.OrderBy(res => res.TotalTime);
                        var bestChain = orderedResults.First();

                        var deadEnemiesOnEnd = _deathTracker.RespawnTimes.Count(value => value - bestChain.TotalTime + 5f > Game.Time);
                        if (deadEnemiesOnEnd > deadAllies + 1 || deadEnemiesOnEnd == HeroManager.Enemies.Count)
                        {
                            _shouldEnd = true;
                            if(_ending.Item("notification").GetValue<bool>())
                            Notifications.AddNotification(new Notification("You could end (Chance: high) + ~" + _deathTracker.GetTimeWhenAlive(HeroManager.Allies.Count(ally => !ally.IsDead) - 3) + " sec.", 4, true) { TextColor = new ColorBGRA(0, 255, 0, 255) });
                        }
                        else if (deadEnemiesOnEnd > deadAllies + 3 || deadEnemiesOnEnd == HeroManager.Enemies.Count)
                        {
                            if(_ending.Item("notification").GetValue<bool>())
                            Notifications.AddNotification(new Notification("You could end (Chance: low) enemies: " + _deathTracker.GetAliveCount(Game.Time + bestChain.TotalTime), 4, true) { TextColor = new ColorBGRA(0, 0, 255, 255) });
                        }
                    }

                    _fightWon = true;
                }
                else
                    _fightWon = false;
                //   Console.WriteLine(profiler1.ElapsedTicks + " end " + (profiler1.ElapsedTicks / Stopwatch.Frequency * 1000f));


                //       profiler1.Restart();
                if (ObjectiveCalc.SelectedObjective != null && _objectives.Item("canclickobjectives").GetValue<bool>())
                {
                    if (ObjectiveCalc.SelectedObjective.HasBeenDone())
                        ObjectiveCalc.SelectedObjective = null;
                    else
                    {
                        try
                        {
                            var chain = _calc.GetObjectiveChainsTo(ObjectiveCalc.SelectedObjective);
                            _clickedResults = new ObjectiveCalc.ObjectiveResult[chain.Count];

                            // var stopwatch = Stopwatch.StartNew();
                            for (int i = 0; i < chain.Count; i++)
                            {
                                _clickedResults[i] = _calc.GetNeededTime(chain[i], null);
                            }
                            // Console.WriteLine(chain.Count + " " + (stopwatch.ElapsedTicks / (float)Stopwatch.Frequency * 1000f) + " elapse");

                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;

                            Console.WriteLine(ex);
                            Console.WriteLine(ex.Source);
                            Console.WriteLine(ex.StackTrace);
                            Console.WriteLine(ex.Data);
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                    }
                }
                //   Console.WriteLine(profiler1.ElapsedTicks+ " selected " + (profiler1.ElapsedTicks / (float)Stopwatch.Frequency * 1000f));
            }
            #endregion

            #region objective drawing
            if (_clickedResults != null && ObjectiveCalc.SelectedObjective != null)
            {
                if (!_clickedResults.Any(res => res.Objectives.Any(obj => obj == null)))
                {
                    var orderedResults = _clickedResults.OrderBy(res => res.TotalTime);
                    var bestChain = orderedResults.First();

                    var positionWorld2D = bestChain.Objectives.Last().Position;
                    var positionScreen2D = Drawing.WorldToScreen(new Vector3(positionWorld2D.X, positionWorld2D.Y, 0));

                    Drawing.DrawText(positionScreen2D.X, positionScreen2D.Y, Color.Red, string.Format("{0:0.0}", bestChain.TotalTime) + " seconds (" + string.Format("{0:0.0}", bestChain.ObjectiveTimes[bestChain.ObjectiveTimes.Length - 1])+")");
                    if (bestChain.Objectives.Count > 1)
                    {
                        var time = 0f;
                        for (int i = 0; i < bestChain.Objectives.Count - 1; i++)
                        {
                            positionWorld2D = bestChain.Objectives[i].Position;
                            positionScreen2D = Drawing.WorldToScreen(new Vector3(positionWorld2D.X, positionWorld2D.Y, 0));
                            time += bestChain.ObjectiveTimes[i];
                            Drawing.DrawText(positionScreen2D.X, positionScreen2D.Y, Color.Red, string.Format("{0:0.0}", time) +" seconds "+ " (" + string.Format("{0:0.0}", bestChain.ObjectiveTimes[i]) + ")");
                        }
                    }
                }
                else
                    _clickedResults = null;
            }

            if (_endResults != null && _fightWon && _ending.Item("drawtext").GetValue<bool>() && _shouldEnd)
            {

                var orderedResults = _endResults.OrderBy(res => res.TotalTime);
                var bestChain = orderedResults.First();

                var screenCoord = Drawing.WorldToScreen(ObjectManager.Player.Position);
                Drawing.DrawText(screenCoord.X - 75, screenCoord.Y, Color.Green, "Could end in " + string.Format("{0:0.0}", bestChain.TotalTime) + " sec.");
            }
            else
                _shouldEnd = false;
            #endregion
        }
    }
}
