using System;
using System.Collections.Generic;
using Color = System.Drawing.Color;
using LeagueSharp;
using LeagueSharp.Common;

namespace 花边_花式多合一.Core
{
    internal class TowerRange
    {
        public static Dictionary<int, Obj_AI_Turret> turretCache = new Dictionary<int, Obj_AI_Turret>();


        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!InitializeMenu.Menu.Item("RangeEnabled").GetValue<bool>())
            {
                return;
            }

            var turretRange = 800 + Huabian.Player.BoundingRadius;

            foreach (var entry in turretCache)
            {
                var turret = entry.Value;

                var circlePadding = 20;

                if (turret == null || !turret.IsValid || turret.IsDead)
                {
                    Utility.DelayAction.Add(1, () => turretCache.Remove(entry.Key));
                    continue;
                }

                if (turret.TotalAttackDamage > 800)
                {
                    continue;
                }

                var distToTurret = Huabian.Player.ServerPosition.Distance(turret.Position);
                if (distToTurret < turretRange + 500)
                {
                    var tTarget = turret.Target;
                    if (tTarget.IsValidTarget(float.MaxValue, false))
                    {
                        if (tTarget is Obj_AI_Hero)
                        {
                            Render.Circle.DrawCircle(tTarget.Position, tTarget.BoundingRadius + circlePadding,
                            Color.FromArgb(255, 255, 0, 0), 20);
                        }
                        else
                        {
                            Render.Circle.DrawCircle(tTarget.Position, tTarget.BoundingRadius + circlePadding,
                            Color.FromArgb(255, 0, 255, 0), 10);
                        }
                    }

                    if (tTarget != null && (tTarget.IsMe || (turret.IsAlly && tTarget is Obj_AI_Hero)))
                    {
                        Render.Circle.DrawCircle(turret.Position, turretRange,
                            Color.FromArgb(255, 255, 0, 0), 20);
                    }
                    else
                    {
                        var alpha = distToTurret > turretRange ? (turretRange + 500 - distToTurret) / 2 : 250;
                        Render.Circle.DrawCircle(turret.Position, turretRange,
                            Color.FromArgb((int)alpha, 0, 255, 0), 10);
                    }
                }
            }
        }

        internal static void Game_OnGameLoad(EventArgs args)
        {
            try
            {
                //if (InitializeMenu.Menu.Item("RangeEnabled").GetValue<bool>()) return;
                
                Drawing.OnDraw += Drawing_OnDraw;
                InitiatizeCache();
            }
            catch (Exception ex)
            {
                Console.WriteLine("TowerRange error occurred: '{0}'", ex);
            }
        }

        private static void InitiatizeCache()
        {
            foreach (var obj in ObjectManager.Get<Obj_AI_Turret>())
            {
                if (!turretCache.ContainsKey(obj.NetworkId))
                {
                    turretCache.Add(obj.NetworkId, obj);
                }
            }
        }
    }
}