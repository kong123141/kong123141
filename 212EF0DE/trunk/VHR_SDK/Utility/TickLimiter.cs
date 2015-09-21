namespace VHR_SDK.Utility
{
    using System.Collections.Generic;
    using System.Linq;
    using LeagueSharp.SDK.Core;

    class TickLimiter
    {
        public static Dictionary<TickLimiterKey, float> TickDictionary = new Dictionary<TickLimiterKey, float>();

        public static void Add(string name, float interval)
        {
            TickDictionary.Add(new TickLimiterKey{Interval = interval, Name = name}, 0f);
        }

        public static bool CanTick(string name)
        {
            var CurrentKey = TickDictionary.Select(k => k.Key).FirstOrDefault(h => h.Name == name);
            if (CurrentKey != null)
            {
                if (Variables.TickCount - TickDictionary[CurrentKey] >= CurrentKey.Interval)
                {
                    TickDictionary[CurrentKey] = Variables.TickCount;
                    return true;
                }
            }
            return false;
        }
    }

    class TickLimiterKey
    {
        public string Name { get; set; }
        public float Interval { get; set; }
    }
}
