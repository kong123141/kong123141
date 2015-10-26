using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System;
using System.Globalization;

namespace 花边_花式多合一.Core
{
    class TowerHealth
    {

        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (InitializeMenu.Menu.Item("HealthActive").GetValue<bool>())
            {
                foreach (Obj_AI_Turret turret in ObjectManager.Get<Obj_AI_Turret>())
                {
                    if ((turret.HealthPercent == 100))
                    {
                        continue;
                    }
                    int health = 0;
                    switch (InitializeMenu.Menu.Item("TIHealth").GetValue<StringList>().SelectedIndex)
                    {
                        case 0:
                            health = (int)turret.HealthPercent;
                            break;

                        case 1:
                            health = (int)turret.Health;
                            break;
                    }
                    Vector2 pos = Drawing.WorldToMinimap(turret.Position);
                    var perHealth = (int)turret.HealthPercent;
                    if (perHealth >= 75)
                    {
                        FlowersDrawing.DrawText1(FlowersDrawing.Text1, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.LimeGreen);
                    }
                    else if (perHealth < 75 && perHealth >= 50)
                    {
                        FlowersDrawing.DrawText1(FlowersDrawing.Text1, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.YellowGreen);
                    }
                    else if (perHealth < 50 && perHealth >= 25)
                    {
                        FlowersDrawing.DrawText1(FlowersDrawing.Text1, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.Orange);
                    }
                    else if (perHealth < 25)
                    {
                        FlowersDrawing.DrawText1(FlowersDrawing.Text1, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.Red);
                    }
                }
                foreach (Obj_BarracksDampener inhibitor in ObjectManager.Get<Obj_BarracksDampener>())
                {
                    if (inhibitor.Health != 0 && (inhibitor.Health / inhibitor.MaxHealth) * 100 != 100)
                    {
                        Vector2 pos = Drawing.WorldToMinimap(inhibitor.Position);
                        var health = (int)((inhibitor.Health / inhibitor.MaxHealth) * 100);
                        if (health >= 75)
                        {
                            FlowersDrawing.DrawText1(FlowersDrawing.Text1, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.LimeGreen);
                        }
                        else if (health < 75 && health >= 50)
                        {
                            FlowersDrawing.DrawText1(FlowersDrawing.Text1, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.YellowGreen);
                        }
                        else if (health < 50 && health >= 25)
                        {
                            FlowersDrawing.DrawText1(FlowersDrawing.Text1, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.Orange);
                        }
                        else if (health < 25)
                        {
                            FlowersDrawing.DrawText1(FlowersDrawing.Text1, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.Red);
                        }
                    }
                }
            }
        }

        internal static void Game_OnGameLoad(EventArgs args)
        {
            try
            {
                if(!InitializeMenu.Menu.Item("HealthActive").GetValue<bool>()) return;
                Game.OnUpdate += Game_OnUpdate;
                Drawing.OnEndScene += Drawing_OnEndScene;
            }
            catch (Exception ex)
            {
                Console.WriteLine("TowerHealth error occurred: '{0}'", ex);
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (InitializeMenu.Menu.Item("HealthActive").GetValue<bool>())
            {
                foreach (Obj_AI_Turret turret in ObjectManager.Get<Obj_AI_Turret>())
                {
                    if ((turret.HealthPercent == 100))
                    {
                        continue;
                    }
                    int health = 0;
                    switch (InitializeMenu.Menu.Item("TIHealth").GetValue<StringList>().SelectedIndex)
                    {
                        case 0:
                            health = (int)turret.HealthPercent;
                            break;

                        case 1:
                            health = (int)turret.Health;
                            break;
                    }
                    Vector2 pos = Drawing.WorldToMinimap(turret.Position);
                    var perHealth = (int)turret.HealthPercent;
                    if (perHealth >= 75)
                    {
                        FlowersDrawing.DrawText1(FlowersDrawing.Text1, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.LimeGreen);
                    }
                    else if (perHealth < 75 && perHealth >= 50)
                    {
                        FlowersDrawing.DrawText1(FlowersDrawing.Text1, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.YellowGreen);
                    }
                    else if (perHealth < 50 && perHealth >= 25)
                    {
                        FlowersDrawing.DrawText1(FlowersDrawing.Text1, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.Orange);
                    }
                    else if (perHealth < 25)
                    {
                        FlowersDrawing.DrawText1(FlowersDrawing.Text1, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.Red);
                    }
                }
                foreach (Obj_BarracksDampener inhibitor in ObjectManager.Get<Obj_BarracksDampener>())
                {
                    if (inhibitor.Health != 0 && (inhibitor.Health / inhibitor.MaxHealth) * 100 != 100)
                    {
                        Vector2 pos = Drawing.WorldToMinimap(inhibitor.Position);
                        var health = (int)((inhibitor.Health / inhibitor.MaxHealth) * 100);
                        if (health >= 75)
                        {
                            FlowersDrawing.DrawText1(FlowersDrawing.Text1, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.LimeGreen);
                        }
                        else if (health < 75 && health >= 50)
                        {
                            FlowersDrawing.DrawText1(FlowersDrawing.Text1, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.YellowGreen);
                        }
                        else if (health < 50 && health >= 25)
                        {
                            FlowersDrawing.DrawText1(FlowersDrawing.Text1, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.Orange);
                        }
                        else if (health < 25)
                        {
                            FlowersDrawing.DrawText1(FlowersDrawing.Text1, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.Red);
                        }
                    }
                }
            }
        }
    }
}
