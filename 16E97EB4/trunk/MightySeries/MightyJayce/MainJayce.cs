using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;


namespace MightyJayce
{
    class MainJayce
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += ConfigMenu.OnLoad;
        }
        public static void Printchat(string message)
        {

            Game.PrintChat(
                "<font color='#FFB90F'>[Console]:</font> <font color='#FFFFFF'>" + message + "</font>");
        }
        public static void Printmsg(string message)
        {
            Game.PrintChat(
                "<font color='#00ff00'>[Cheerleader Lux]:</font> <font color='#FFFFFF'>" + message + "</font>");
        }
    }
}
