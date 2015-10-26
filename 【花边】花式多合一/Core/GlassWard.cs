using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace 花边_花式多合一.Core
{
    internal class GlassWard
    {
        public static Vector3 positionWard;
        private static Obj_AI_Hero WardTarget;
        private static float WardTime = 0;
        public static Items.Item WardS = new Items.Item(2043, 600f);
        public static Items.Item WardN = new Items.Item(2044, 600f);
        public static Items.Item TrinketN = new Items.Item(3340, 600f);
        public static Items.Item SightStone = new Items.Item(2049, 600f);
        public static Items.Item ManaPotion = new Items.Item(2004, 0);
        private static Obj_AI_Hero Player;

        private static void Game_OnUpdate(EventArgs args)
        {
            if (InitializeMenu.Menu.Item("ward").GetValue<bool>() ||
                (InitializeMenu.Menu.Item("ward").GetValue<bool>() && InitializeMenu.Menu.Item("Combo").GetValue<KeyBind>().Active && InitializeMenu.Menu.Item("wardC").GetValue<bool>()))
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget(2000)))
                {
                    bool WallOfGrass = NavMesh.IsWallOfGrass(Prediction.GetPrediction(enemy, 0.3f).CastPosition, 0);
                    if (WallOfGrass)
                    {
                        positionWard = Prediction.GetPrediction(enemy, 0.3f).CastPosition;
                        WardTarget = enemy;
                        WardTime = Game.Time;
                    }
                }
                if (Player.Distance(positionWard) < 600 && !WardTarget.IsValidTarget() && Game.Time - WardTime < 5)
                {
                    WardTime = Game.Time - 6;
                    if (TrinketN.IsReady())
                        TrinketN.Cast(positionWard);
                    else if (SightStone.IsReady())
                        SightStone.Cast(positionWard);
                    else if (WardS.IsReady())
                        WardS.Cast(positionWard);
                    else if (WardN.IsReady())
                        WardN.Cast(positionWard);
                }
            }
        }

        internal static void Game_OnGameLoad(EventArgs args)
        {
            try
            {
                if (!InitializeMenu.Menu.Item("ward").GetValue<bool>()) return;
                Game.OnUpdate += Game_OnUpdate;
            }
            catch (Exception ex)
            {
                Console.WriteLine("GlassWard error occurred: '{0}'", ex);
            }
        }
    }
}