namespace Flowers_Utility.Common
{
    using System;
    using System.Collections.Generic;
    using LeagueSharp;

    class DelayAction
    {
        public delegate void Callback();
        public static List<Action> ActionList = new List<Action>();

        static DelayAction()
        {
            Game.OnUpdate += OnUpdate;
        }

        private static void OnUpdate(EventArgs args)
        {
            for (var i = ActionList.Count - 1; i >= 0; i--)
            {
                if (ActionList[i].Time <= Helper.TickCount)
                {
                    try
                    {
                        if (ActionList[i].CallbackObject != null)
                        {
                            ActionList[i].CallbackObject();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error in DelayAction + " + ex);
                    }

                    ActionList.RemoveAt(i);
                }
            }
        }

        public struct Action
        {
            public Callback CallbackObject;
            public int Time;

            public Action(int time, Callback callback)
            {
                Time = time + (int)Helper.TickCount;
                CallbackObject = callback;
            }
        }

        public static void Add(int time, Callback func)
        {
            var action = new Action(time, func);
            ActionList.Add(action);
        }
    }
}
