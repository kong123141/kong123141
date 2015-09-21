using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.SDK;

namespace VayneHunter_Reborn_SDK
{
    class Program
    {
        static void Main(string[] args)
        {
            LeagueSharp.SDK.Core.Events.Load.OnLoad += (sender, eventArgs) =>
            { VayneHunterReborn.OnLoad(args); };
        }

    }
}
