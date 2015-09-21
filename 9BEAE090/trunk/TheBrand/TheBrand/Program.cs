using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using LeagueSharp.Common;

namespace TheBrand
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += new Brand().Load;
        }
    }
}
