using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using TheInfo.Objectives.Items;

namespace TheInfo.Objectives
{
    class ObjectiveCalc
    {
        public static Objective[] Objectives;
        public static Objective SelectedObjective;

        public bool CalculateExact; //will drop FPS below hell

        private class Player
        {
            public Obj_AI_Hero Hero;
            public float DistanceTime;

            public float GetPossibleDmgTo(Objective obj, float time)
            {
                time -= DistanceTime;
                if (time <= 0)
                    return 0;

                return obj.GetEstimatedDps(Hero) * time;
            }
        }

        public struct ObjectiveResult
        {
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public List<Objective> Objectives;
            public float TotalTime;
            public float[] ObjectiveTimes;
        }

        public ObjectiveCalc()
        {
            try
            {
                Objectives = SetupObjectives();
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch { }

            Game.OnWndProc += GameWndProc;
        }

        private static Objective[] SetupObjectives()
        {
            ObjectiveOuterTurret topr = new ObjectiveOuterTurret(SummonersRift.TopLane.Red_Outer_Turret), topb = new ObjectiveOuterTurret(SummonersRift.TopLane.Blue_Outer_Turret), midr = new ObjectiveOuterTurret(SummonersRift.MidLane.Red_Outer_Turret), midb = new ObjectiveOuterTurret(SummonersRift.MidLane.Blue_Outer_Turret), botr = new ObjectiveOuterTurret(SummonersRift.BottomLane.Red_Outer_Turret), botb = new ObjectiveOuterTurret(SummonersRift.BottomLane.Blue_Outer_Turret);
            ObjectiveInnerTurret topr2 = new ObjectiveInnerTurret(SummonersRift.TopLane.Red_Inner_Turret, topr), topb2 = new ObjectiveInnerTurret(SummonersRift.TopLane.Blue_Inner_Turret, topb), midr2 = new ObjectiveInnerTurret(SummonersRift.MidLane.Red_Inner_Turret, midr), midb2 = new ObjectiveInnerTurret(SummonersRift.MidLane.Blue_Inner_Turret, midb), botr2 = new ObjectiveInnerTurret(SummonersRift.BottomLane.Red_Inner_Turret, botr), botb2 = new ObjectiveInnerTurret(SummonersRift.BottomLane.Blue_Inner_Turret, botb);
            ObjectiveInhibitorTurret topr3 = new ObjectiveInhibitorTurret(SummonersRift.Base.Red_Top_Turret, topr2), topb3 = new ObjectiveInhibitorTurret(SummonersRift.Base.Blue_Top_Turret, topb2), midr3 = new ObjectiveInhibitorTurret(SummonersRift.Base.Red_Mid_Turret, midr2), midb3 = new ObjectiveInhibitorTurret(SummonersRift.Base.Blue_Mid_Turret, midb2), botr3 = new ObjectiveInhibitorTurret(SummonersRift.Base.Red_Bottom_Turret, botr2), botb3 = new ObjectiveInhibitorTurret(SummonersRift.Base.Blue_Bottom_Turret, botb2);
            ObjectiveInhibitor topr4 = new ObjectiveInhibitor(SummonersRift.Base.Red_Top_Inhibitor, topr3), topb4 = new ObjectiveInhibitor(SummonersRift.Base.Blue_Top_Inhibitor, topb3), midr4 = new ObjectiveInhibitor(SummonersRift.Base.Red_Mid_Inhibitor, midr3), midb4 = new ObjectiveInhibitor(SummonersRift.Base.Blue_Mid_Inhibitor, midb3), botr4 = new ObjectiveInhibitor(SummonersRift.Base.Red_Bottom_Inhibitor, botr3), botb4 = new ObjectiveInhibitor(SummonersRift.Base.Blue_Bottom_Inhibitor, botb3);
            ObjectiveBaseTurret topr5 = new ObjectiveBaseTurret(SummonersRift.Base.Red_Top_Nexus_Turret, topr4, midr4, botr4), topb5 = new ObjectiveBaseTurret(SummonersRift.Base.Blue_Top_Nexus_Turret, topb4, midb4, botb4), botr5 = new ObjectiveBaseTurret(SummonersRift.Base.Red_Bottom_Nexus_Turret, topr4, midr4, botr4), botb5 = new ObjectiveBaseTurret(SummonersRift.Base.Blue_Bottom_Nexus_Turret, topb4, midb4, botb4);
            ObjectiveBase midr6 = new ObjectiveBase(SummonersRift.Base.Red_Nexus, topr5, botr5), midb6 = new ObjectiveBase(SummonersRift.Base.Blue_Nexus, topb5, botb5);
            ObjectiveDragon drag = new ObjectiveDragon(SummonersRift.River.Dragon);
            ObjectiveBaron baron = new ObjectiveBaron(SummonersRift.River.Baron);

            Objective[] objectives;
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (ObjectManager.Player.Team == GameObjectTeam.Order)
            {
                objectives = new Objective[] { topr, midr, botr, topr2, midr2, botr2, topr3, midr3, botr3, topr4, midr4, botr4, topr5, botr5, midr6, drag, baron };
            }
            else
            {
                objectives = new Objective[] { topb, midb, botb, topb2, midb2, botb2, topb3, midb3, botb3, topb4, midb4, botb4, topb5, botb5, midb6, drag, baron };
            }

            // Using hardcoded Distance values because you can't use pathfinding from X to Y at runtime, just from player to X. And you can't use the straight distance, because jungle ...
            #region Baron
            baron.SetDistanceTo(midb, 4200);
            baron.SetDistanceTo(topb, 4500);
            baron.SetDistanceTo(botb, 10900);
            baron.SetDistanceTo(topb2, 4700);
            baron.SetDistanceTo(midb2, 4900);
            baron.SetDistanceTo(botb2, 9400);
            baron.SetDistanceTo(topb3, 6800);
            baron.SetDistanceTo(midb3, 6400);
            baron.SetDistanceTo(botb3, 9100);

            baron.SetDistanceTo(topr, 4200);
            baron.SetDistanceTo(midr, 4200);
            baron.SetDistanceTo(botr, 11400);
            baron.SetDistanceTo(topr2, 6300);
            baron.SetDistanceTo(midr2, 5400);
            baron.SetDistanceTo(botr2, 10100);
            baron.SetDistanceTo(topr3, 8100);
            baron.SetDistanceTo(midr3, 7300);
            baron.SetDistanceTo(botr3, 9700);

            baron.SetDistanceTo(drag, 7900);
            baron.OrientNonMappedOnNearestMapped(objectives);
            #endregion

            #region Dragon
            drag.SetDistanceTo(midb, 4600);
            drag.SetDistanceTo(topb, 11100);
            drag.SetDistanceTo(botb, 4200);
            drag.SetDistanceTo(topb2, 9900);
            drag.SetDistanceTo(midb2, 5400);
            drag.SetDistanceTo(botb2, 6200);
            drag.SetDistanceTo(topb3, 9500);
            drag.SetDistanceTo(midb3, 7100);
            drag.SetDistanceTo(botb3, 8200);

            drag.SetDistanceTo(topr, 11100);
            drag.SetDistanceTo(midr, 4300);
            drag.SetDistanceTo(botr, 4000);
            drag.SetDistanceTo(topr2, 9900);
            drag.SetDistanceTo(midr2, 5200);
            drag.SetDistanceTo(botr2, 4700);
            drag.SetDistanceTo(topr3, 9200);
            drag.SetDistanceTo(midr3, 6400);
            drag.SetDistanceTo(botr3, 6700);

            drag.SetDistanceTo(baron, 7900);
            drag.OrientNonMappedOnNearestMapped(objectives);
            #endregion

            foreach (var objective in objectives)
            {
                objective.TryGenerateDisplayName();
            }

            return objectives;
        }

        public ObjectiveResult GetNeededTime(Objective objective)
        {
            return GetNeededTime(new[] { objective }.ToList(), null);
        }

        public ObjectiveResult GetNeededTime(List<Objective> objectives, List<Obj_AI_Hero> allies)
        {

            var result = new ObjectiveResult { ObjectiveTimes = new float[objectives.Count], Objectives = objectives };

            List<Player> players;
            if (allies == null)
                players = HeroManager.Allies.Select(item => new Player { Hero = item, DistanceTime = (CalculateExact ? ObjectiveCommons.GetNeededMoveTimeHeavy(item, objectives.First().GetGameObject().Position) : ObjectiveCommons.GetNeededMoveTime(item, objectives.First().GetGameObject().Position)) - item.AttackRange / item.MoveSpeed }).ToList();
            else
                players = allies.Select(item => new Player { Hero = item, DistanceTime = (CalculateExact ? ObjectiveCommons.GetNeededMoveTimeHeavy(item, objectives.First().GetGameObject().Position) : ObjectiveCommons.GetNeededMoveTime(item, objectives.First().GetGameObject().Position)) - item.AttackRange / item.MoveSpeed }).ToList();

            for (int i = 0; i < objectives.Count; i++)
            {
                players = players.OrderBy(player => player.DistanceTime).ToList();

                var go = objectives[i].GetGameObject();
                var health = go.Health;
                var objTime = 0f;

                //Console.WriteLine(players[0].Hero.ChampionName);

                for (int j = 0; j < players.Count; j++)
                {
                    var dmg = players.Sum(player => player.GetPossibleDmgTo(objectives[i], players[j].DistanceTime));

                    if (dmg > health)
                    {
                        try
                        {
                            var sumdps = players.Sum(player => player.DistanceTime < players[j].DistanceTime ? objectives[i].GetEstimatedDps(player.Hero) : 0);
                            objTime = players[j - 1].DistanceTime + (health) / sumdps;
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;

                            Console.WriteLine(ex);
                            Console.ForegroundColor = ConsoleColor.Gray;

                        }


                    }
                    else
                    {

                        health -= dmg;

                        if (j + 1 == players.Count)
                        {
                            var sumdps = players.Sum(player => objectives[i].GetEstimatedDps(player.Hero));
                            objTime = players[j].DistanceTime + health / sumdps;
                        }
                    }
                }

                result.ObjectiveTimes[i] = objTime;
                result.TotalTime += objTime;

                if (i + 1 < objectives.Count)
                {
                    players.ForEach(player => player.DistanceTime = Math.Max(0, player.DistanceTime - objTime) + objectives[i].GetMeleeDistanceTo(objectives[i + 1]) / player.Hero.MoveSpeed);
                }


            }



            return result;
        }

        public List<List<Objective>> GetObjectiveChainsTo(Objective objective)
        {
            if (objective.HasBeenDone())
                return null;
            var allChains = new List<List<Objective>>();
            var allChainsEnded = true;
            allChains.Add(new[] { objective }.ToList());

            do
            {
                for (int i = 0; i < allChains.Count; i++)
                {
                    var currentChain = allChains[i];
                    allChainsEnded = true;
                    Objective currentObj = currentChain.Last();
                    if (currentObj.RequiredObjectives.Count == 0 || (currentObj.RequireAll ? currentObj.RequiredObjectives.All(obj => obj.HasBeenDone()) : currentObj.RequiredObjectives.Any(obj => obj.HasBeenDone())))
                        continue;
                    allChainsEnded = false;
                    if (currentObj.RequireAll)
                        currentChain.AddRange(currentObj.RequiredObjectives.Where(requiredObjective => !requiredObjective.HasBeenDone()));
                    else
                    {
                        if (!currentChain.Any(obj => obj.CanBeDone()))
                            allChains.Remove(currentChain);
                        foreach (var requiredObjective in currentObj.RequiredObjectives)
                        {
                            var newChain = new List<Objective>();
                            newChain.AddRange(currentChain.ToArray());
                            newChain.Add(requiredObjective);
                            allChains.Add(newChain);
                        }
                    }
                }
            } while (!allChainsEnded);

            allChains.ForEach(chain => chain.Reverse());
            return allChains;
        }

        public Objective GetObjective<T>()
        {
            return Objectives.FirstOrDefault(obj => obj.GetType() == typeof(T));
        }

        private static void GameWndProc(WndEventArgs args)
        {
            if (args.Msg != (uint)WindowsMessages.WM_LBUTTONDOWN)
                return;

            if (Objectives != null)
                SelectedObjective = Objectives.FirstOrDefault(obj => (!obj.HasBeenDone()) && obj.Position.Distance(Game.CursorPos) < 200);
        }

    }
}
