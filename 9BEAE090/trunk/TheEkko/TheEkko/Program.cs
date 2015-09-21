using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp.Common;

namespace TheEkko
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += new Ekko().Load;
        }
    }
}
