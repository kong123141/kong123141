﻿using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using SharpDX;

namespace 花边_花式多合一.Core
{
    public static class CheckMoreL
    {
        public static Notification notification = new Notification("下面的人员有开挂的嫌疑！" + "" );
        public static bool Notificed = false;
        public static readonly Dictionary<int, List<Interface.IDetector>> _detectors = new Dictionary<int, List<Interface.IDetector>>();

        private static void Game_OnUpdate(EventArgs args)
        {
            string content = "";

            foreach (var detector in _detectors)
            {
                var maxValue = detector.Value.Max(item => item.GetScriptDetections());
                var hero = HeroManager.AllHeroes.First(h => h.NetworkId == detector.Key);

                //if (hero.IsMe) continue;

                if (maxValue > 0)
                {
                    if (!Notificed)
                    {
                        Notificed = true;
                        Notifications.AddNotification(notification);
                    }
                    string info = (hero.IsAlly ? "我方" : "敌方")
                        + hero.ChampionName
                        + ": " + detector.Value.First(itemId => itemId.GetScriptDetections() == maxValue).GetName()
                        + "(" + maxValue + ")"
                        + ";";
                    content += info + Environment.NewLine;

                    notification.Text = content;
                    notification.OnUpdate();


                }
            }
        }

        internal static void Game_OnGameLoad(EventArgs args)
        {
            try
            {
                if (!InitializeMenu.Menu.Item("CheckEnable").GetValue<bool>()) return;
                Obj_AI_Base.OnNewPath += Obj_AI_Hero_OnNewPath;
                Game.OnUpdate += Game_OnUpdate;
                notification.Draw = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("CheckMoreL error occurred: '{0}'", ex);
            }
        }

        private static void Obj_AI_Hero_OnNewPath(Obj_AI_Base sender, GameObjectNewPathEventArgs args)
        {
            if (!InitializeMenu.Menu.Item("CheckEnable").GetValue<bool>())
            {
                return;
            }

            if (!_detectors.ContainsKey(sender.NetworkId))
            {
                var detectors = new List<Interface.IDetector> { new SacOrbwalkerDetector(), new LeaguesharpOrbwalkDetector() };
                detectors.ForEach(detector => detector.Initialize((Obj_AI_Hero)sender));
                _detectors.Add(sender.NetworkId, Interface.detectors);
            }
            else
            {
                _detectors[sender.NetworkId].ForEach(detector => detector.FeedData(args.Path.Last()));
            }
        }

    }

    internal class LeaguesharpOrbwalkDetector : Interface.IDetector
    {
        private const float RandomizerFactor = 2f / 3f; // Even though the distance in l# can be set, it is only randomized from 0.8x distance to 1.2x distance, which is a 2/3 relationship
        private int _scripting;
        private Obj_AI_Hero _hero;
        private readonly List<DataSet> _recentData = new List<DataSet>();
        private readonly Dictionary<Interface.DetectorSetting, float[]> _settingValues = new Dictionary<Interface.DetectorSetting, float[]>() { { Interface.DetectorSetting.Safe, new[] { 0.125f, 90 } }, { Interface.DetectorSetting.AntiHumanizer, new[] { 0.25f, 80 } } };
        private Interface.DetectorSetting _currentSetting;
        public LeaguesharpOrbwalkDetector()
        {

        }
        struct DataSet
        {
            public Vector3 Position;
            public float Time;
            public double Distance;

            public static DataSet Create(Vector3 position)
            {
                return new DataSet { Position = position, Distance = Huabian.Player.Position.Distance(position), Time = Game.Time };
            }
        }

        public void FeedData(Vector3 targetPos)
        {
            if (_recentData.Count >= 5)
            {
                _recentData.RemoveAt(0);
                _recentData.Add(DataSet.Create(targetPos));
            }
            else
            {
                _recentData.Add(DataSet.Create(targetPos));
                return;
            }

            if (_recentData.Last().Time - _recentData.First().Time < _recentData.Count * _settingValues[_currentSetting][0]) // clicking intensifies, 0.125f doesnt catch movement if you just hold the right mouse button, but sometimes still catches orbwalker at 170 humanizer delay
            {

                var min = double.MaxValue;
                var max = double.MinValue;
                foreach (var data in _recentData)
                {
                    if (data.Distance < min)
                        min = data.Distance;
                    if (data.Distance > max)
                        max = data.Distance;
                }

                if (!(max * RandomizerFactor <= min))
                {
                    return;
                }

                var angleDiff = 0f;
                for (int i = 1; i < _recentData.Count - 1; i++)
                    angleDiff += GetAngleDifference(_recentData[i].Position - _recentData[i - 1].Position, _recentData[i + 1].Position - _recentData[i].Position);

                if (angleDiff > _settingValues[_currentSetting][1] * _recentData.Count) // since the length is randomized it clicks like a vibrating mouse ... results in lots of direction turns. A normal person does not change the click-direction 90° avg. per click -> 450° per 5 click, which means you changed your click direction ~3 times in a short time
                    _scripting++;
            }
        }

        private static float GetAngleDifference(Vector3 vec1, Vector3 vec2)
        {
            return (float)RadianToDegree(Math.Atan2(Vector3.Cross(vec1, vec2).Length(), Vector3.Dot(vec1, vec2)));

        }

        private static double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        public int GetScriptDetections()
        {
            return _scripting;
        }

        public string GetName()
        {
            return "L#";
        }

        public void Initialize(Obj_AI_Hero hero, Interface.DetectorSetting setting = Interface.DetectorSetting.Safe)
        {
            _hero = hero;
            ApplySetting(setting);
        }

        public void ApplySetting(Interface.DetectorSetting setting)
        {
            _currentSetting = setting == Interface.DetectorSetting.Preferred ? Interface.DetectorSetting.Safe : setting;
        }
    }

    internal class SacOrbwalkerDetector : Interface.IDetector
    {
        private int _scripting;
        private Obj_AI_Hero _hero;
        private readonly List<DataSet> _recentData = new List<DataSet>();
        private readonly Dictionary<Interface.DetectorSetting, float[]> _settingValues = new Dictionary<Interface.DetectorSetting, float[]> { { Interface.DetectorSetting.Safe, new[] { 0.125f, 90 } }, { Interface.DetectorSetting.AntiHumanizer, new[] { 0.5f, 80 } } };
        private Interface.DetectorSetting _currentSetting;

        struct DataSet
        {
            public Vector3 Position;
            public float Time;
            public double Distance;

            public static DataSet Create(Vector3 position, Obj_AI_Hero sender)
            {
                return new DataSet
                {
                    Position = position,
                    Distance = sender.Position.Distance(position),
                    Time = Game.Time
                };
            }
        }

        public void FeedData(Vector3 targetPos)
        {
            if (_recentData.Count >= 5)
            {
                _recentData.RemoveAt(0);
                _recentData.Add(DataSet.Create(targetPos, _hero));
            }
            else
            {
                _recentData.Add(DataSet.Create(targetPos, _hero));
                return;
            }

            if (_recentData.Last().Time - _recentData.First().Time < _recentData.Count * _settingValues[_currentSetting][0]) // clicking intensifies, 0.125f doesnt catch movement if you just hold the right mouse button, but sometimes still catches orbwalker at 170 humanizer delay
            {

                var min = double.MaxValue;
                var max = double.MinValue;
                foreach (var data in _recentData)
                {
                    if (data.Distance < min)
                        min = data.Distance;
                    if (data.Distance > max)
                        max = data.Distance;
                }

                if (min >= 445 && max <= 475 && max - min < 30) // sac always clicks in a certain range
                {
                    _scripting++;
                }
            }
        }

        public int GetScriptDetections()
        {
            return _scripting;
        }

        public string GetName()
        {
            return "BoL";
        }

        public void Initialize(Obj_AI_Hero hero, Interface.DetectorSetting setting = Interface.DetectorSetting.Preferred)
        {
            _hero = hero;
            ApplySetting(setting);
        }

        public void ApplySetting(Interface.DetectorSetting setting)
        {
            _currentSetting = setting == Interface.DetectorSetting.Preferred ? Interface.DetectorSetting.AntiHumanizer : setting;
        }
    }
}
