using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace TheInfo
{
    class Program3
    {
        private static Menu _main;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnStart;
        }

        static void Game_OnStart(EventArgs args)
        {

            GameObject.OnCreate += Obj_SpellMissile_OnCreate;

            _main = new Menu("The Info", "theinfo", true);

            var modules = new IModule[] { new ModuleObjectives(), new ModuleTeamfightOverview(), new ModuleComboTime() };

            foreach (var module in modules)
            {
                module.InitializeMenu(_main);
                module.Initialize();
            }

            _main.AddToMainMenu();

            //Game.PrintChat("initialized");
            Notifications.AddNotification(new Notification("The Info initialized", 1, true) { TextColor = new ColorBGRA(0, 255, 0, 255) });

        }


        static void Obj_SpellMissile_OnCreate(GameObject sender, EventArgs args)
        {
            //if (sender.Name != "missile")
            //    return;

            //var player = ObjectManager.Player;
            //var missile = (Obj_SpellMissile)sender;

            //if (missile.SpellCaster.GetType().Name == "Obj_AI_Turret" && missile.SpellCaster.IsEnemy)
            //{
            //    if (player.Position.Distance(missile.EndPosition) < player.BoundingRadius * 0.9f)
            //    {

            //        //  _hasTurretFocus = true;
            //        var _turret = (Obj_AI_Turret)missile.SpellCaster;
            //        _turretMissileTime = Game.Time;
            //    }

            //    //  Console.WriteLine(missile.EndPosition.Distance(player.ServerPosition) + " / " + player.BoundingRadius);
            //    //    Console.WriteLine(missile.EndPosition.Distance(player.Position) + " / " + player.BoundingRadius);
            //    //   player.BoundingRadius
            //}


        }
    }
}
