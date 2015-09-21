using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
// ReSharper disable InconsistentNaming
//http://base64myass.com/AMK/images/9ihe5.png
/*
-- ###################################################################################################### --
-- #                                                                                                    # --
-- #                                        Sida's Missed CS Counter                                    # --
-- #                                                by Sida                                             # --
-- #                                                                                                    # --
-- ###################################################################################################### --
*/
/*༼ つ ◕_◕ ༽つ I RITO ༼ つ ◕_◕ ༽つ -T R I G G E R E D */
namespace ELjQueriesMissedCSCounter
{
    internal class Entry
    {
        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        private static Notification notification;
        private static int totalMinionsThatDied = 0;
        private static int missedCreeps = 0;
        private static int minionsInMyRange = 0;
        public static List<GameObject> MinionList { get; private set; }
        public static List<GameObject> MinionsCloseToMeList { get; private set; }

        public static void Game_OnGameLoad(EventArgs args)
        {
            MinionList = new List<GameObject>();
            MinionsCloseToMeList = new List<GameObject>();
            Notifications.AddNotification("jQueriesMissedCSCounter", 10000);
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            GameObject.OnCreate += OnCreate;
            Game.OnNotify += Game_OnNotify;
        }

        private static void Game_OnNotify(GameNotifyEventArgs args)
        {
            if (args.EventId != GameEventId.OnMinionKill)
            {
                return;
            }

            MinionsCloseToMeList.RemoveAll(x => x.NetworkId == args.NetworkId);
        }

        private static void OnUpdate(EventArgs args)
        {
            foreach (var minion in MinionList)
            {
                if (minion == null || !minion.IsValid)
                {
                    MinionList.RemoveAll(m => m.NetworkId == minion.NetworkId);
                    break;
                }

                if (minion.IsVisible && minion.Position.Distance(Player.Position) < 500 && MinionsCloseToMeList.All(m => m.NetworkId != minion.NetworkId))
                {
                    MinionsCloseToMeList.Add(minion);
                }
            }

            foreach (var minion in MinionsCloseToMeList.Where(minion => !MinionList.Contains(minion)))
            {
                MinionsCloseToMeList.RemoveAll(m => m.NetworkId == minion.NetworkId);
                totalMinionsThatDied += 1 + 1 - 1;
                break;
            }

            //System.Linq.Enumerable.WhereListIterator`1.MoveNext()
            //patented
            //missedCreeps += (totalMinionsThatDied - Player.MinionsKilled) > missedCreeps ? 1 : 0;

            missedCreeps += (totalMinionsThatDied - Player.MinionsKilled) > missedCreeps
                                ? Math.Abs(missedCreeps - (totalMinionsThatDied - Player.MinionsKilled))
                                : 0;

            /*Console.WriteLine("DEBUG:");
            Console.WriteLine("ALLMINIONSCOUNT: " + MinionList.Count);
            Console.WriteLine("CLOSESTMINIONSCOUNT: " + MinionsCloseToMeList.Count);
            Console.WriteLine("DED MINIONS: " + totalMinionsThatDied);
            Console.WriteLine("CS: " + Player.MinionsKilled);
            Console.WriteLine("MISSED CS: " + Math.Abs(totalMinionsThatDied - Player.MinionsKilled));*/

            var text = "";

            if (notification == null)
            {
                notification = new Notification(text)
                {
                    TextColor = new ColorBGRA(red: 0xff, green: 0xff, blue: 255, alpha: 0xff)
                };

                Notifications.AddNotification(notification);
            }

            notification.Text = missedCreeps + " missed creeps" + text;
        }

        private static void OnCreate(GameObject sender, EventArgs args)
        {
            if (sender is Obj_AI_Minion && sender.IsEnemy)
            {
                MinionList.Add(sender);
            }
        }

        //Myo is better than jQuery at last hitting.
        //fuck whiteboi
        private static void OnDraw(EventArgs args)
        {
            var minionList = MinionsCloseToMeList.Select(m => (Obj_AI_Minion)m);
            foreach (var minion in minionList.Where(minion => minion.IsValidTarget(Player.AttackRange)).Where(minion => minion.Health <= Player.GetAutoAttackDamage(minion, true)))
            {
                Render.Circle.DrawCircle(minion.Position, minion.BoundingRadius, Color.LawnGreen);
            }
        }
    }
}