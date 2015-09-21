using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;

namespace ElSmite
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                CustomEvents.Game.OnGameLoad += Entry.OnLoad;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }
    }
}
