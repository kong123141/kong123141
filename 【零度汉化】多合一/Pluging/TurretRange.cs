namespace Pluging
{
    using LeagueSharp;
    using LeagueSharp.Common;
    using System;
    using System.Collections.Generic;
    using Color = System.Drawing.Color;

    public class TurretRange
    {
        public static Menu Menu;
        public static Dictionary<int, Obj_AI_Turret> turretCache = new Dictionary<int, Obj_AI_Turret>();


        public TurretRange(Menu mainMenu)
        {
            Menu = mainMenu;

            Menu TurretRangeMenu = new Menu("[FL] 防御塔范围", "TurretRange");

            TurretRangeMenu.AddItem(new MenuItem("TurretRangeActive", "启动").SetValue(true));

            Menu.AddSubMenu(TurretRangeMenu);

            InitializeCache();

            Drawing.OnDraw += OnDraw;
        }

        private void InitializeCache()
        {
            try
            {
                foreach (var obj in ObjectManager.Get<Obj_AI_Turret>())
                {
                    if (!turretCache.ContainsKey(obj.NetworkId))
                    {
                        turretCache.Add(obj.NetworkId, obj);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Rnage.InitilaizeCache + " + ex);
            }
        }

        private void OnDraw(EventArgs args)
        {
            try
            {
                if (!Menu.Item("TurretRangeActive").GetValue<bool>())
                    return;

                var turretRange = 800 + ObjectManager.Player.BoundingRadius;

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

                    var distToTurret = ObjectManager.Player.ServerPosition.Distance(turret.Position);

                    if (distToTurret < turretRange + 500)
                    {
                        var tTarget = turret.Target;

                        if (tTarget.IsValidTarget(float.MaxValue, false))
                        {
                            if (tTarget is Obj_AI_Hero)
                            {
                                Render.Circle.DrawCircle(tTarget.Position, tTarget.BoundingRadius + circlePadding, Color.FromArgb(255, 255, 0, 0), 10);
                            }
                            else
                            {
                                Render.Circle.DrawCircle(tTarget.Position, tTarget.BoundingRadius + circlePadding, Color.FromArgb(255, 0, 255, 0), 5);
                            }
                        }

                        if (tTarget != null && (tTarget.IsMe || (turret.IsAlly && tTarget is Obj_AI_Hero)))
                        {
                            Render.Circle.DrawCircle(turret.Position, turretRange, Color.FromArgb(255, 255, 0, 0), 10);
                        }
                        else
                        {
                            var alpha = distToTurret > turretRange ? (turretRange + 500 - distToTurret) / 2 : 250;

                            Render.Circle.DrawCircle(turret.Position, turretRange, Color.FromArgb((int)alpha, 0, 255, 0), 5);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Range.OnDraw + " + ex);
            }
        }
    }
}