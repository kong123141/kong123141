using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Color = System.Drawing.Color;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
namespace NoMoreFreezes
{ 
    class Program
    {
        private static int counter = 0;
        private static bool InGame = false;
        private static string[] options = {"Flashing Loading", "Increasing Number"};
        private static Menu mainMenu = new Menu("NoMoreFreezes", "NoMoreFreezes", true);
        private static bool number = true;
        static void Main(string[] args)
        {
            Game.OnStart += OnGameLoad;
            CustomEvents.Game.OnGameLoad += OnGameLoad;
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += DrawNotFrozen;
            mainMenu.AddItem(new MenuItem("Style:", "Style:", false).SetValue(new StringList(options, 0)));
            mainMenu.AddToMainMenu();
            if (mainMenu.Item("Style:").GetValue<string>().Equals("Increasing Number"))
                number = false;
        }

        private static void DrawNotFrozen(EventArgs args)
        {
            if (InGame)
            return;
            else
            {
               if(number)
                Drawing.DrawText(10,10, Color.Red, "You are still in game if this is increasing:" + counter);
                else
                {
                    Color temp = ((int)Math.Truncate((double)counter/2)%2==0) ? Color.White : Color.Green;
                    Drawing.DrawText(10, 10, temp, "Loading");
                }
                counter++;
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            if (Game.Time > 0)
                InGame = true;
        }
        private static void OnGameLoad(EventArgs args)
        {
            InGame = true;
        }
    }
}
