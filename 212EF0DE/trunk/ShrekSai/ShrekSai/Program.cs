using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace ShrekSai
{
    class Program
    {
        static void Main(string[] args)
        {
            try //hard
            {
                CustomEvents.Game.OnGameLoad += eventArgs =>
                {
                    SSBootstrap.OnInit();
                };
            }
            catch (Exception e)
            {
                Console.WriteLine("Loading Exception for Shrekt'Sai: {0}", e);
            }
            
        }
    }
}
