namespace Pluging
{
    using Flowers_Utility.Common;
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    using System;
    using System.Globalization;

    public class TurretHealth
    {
        public static Menu Menu;

        public TurretHealth(Menu mainmenu)
        {
            Menu = mainmenu;

            Menu TurretHealthMenu = new Menu("[FL] 防御塔血量显示", "TurretHealth");

            TurretHealthMenu.AddItem(new MenuItem("TIHealth", "显示方式").SetValue(new StringList(new[] { "百分比", "数字" })));
            TurretHealthMenu.AddItem(new MenuItem("HealthActive", "启动").SetValue(true));

            Menu.AddSubMenu(TurretHealthMenu);

            Game.OnUpdate += OnUpdate;
            Drawing.OnEndScene += OnEndScene;
        }

        private void OnEndScene(EventArgs args)
        {
            try
            {
                var HealthActive = Menu.Item("HealthActive").GetValue<bool>();

                if(HealthActive)
                {
                    foreach (Obj_AI_Turret turret in ObjectManager.Get<Obj_AI_Turret>())
                    {
                        if ((turret.HealthPercent == 100))
                        {
                            continue;
                        }

                        int health = 0;

                        switch (Menu.Item("TIHealth").GetValue<StringList>().SelectedIndex)
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
                            Helper.HealthDrawText(Helper.HealthText, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.LimeGreen);
                        }
                        else if (perHealth < 75 && perHealth >= 50)
                        {
                            Helper.HealthDrawText(Helper.HealthText, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.YellowGreen);
                        }
                        else if (perHealth < 50 && perHealth >= 25)
                        {
                            Helper.HealthDrawText(Helper.HealthText, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.Orange);
                        }
                        else if (perHealth < 25)
                        {
                            Helper.HealthDrawText(Helper.HealthText, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.Red);
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
                                Helper.HealthDrawText(Helper.HealthText, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.LimeGreen);
                            }
                            else if (health < 75 && health >= 50)
                            {
                                Helper.HealthDrawText(Helper.HealthText, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.YellowGreen);
                            }
                            else if (health < 50 && health >= 25)
                            {
                                Helper.HealthDrawText(Helper.HealthText, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.Orange);
                            }
                            else if (health < 25)
                            {
                                Helper.HealthDrawText(Helper.HealthText, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.Red);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in TurretHealth.OnEndScene + " + ex);
            }
        }

        private void OnUpdate(EventArgs args)
        {
            try
            {
                var HealthActive = Menu.Item("HealthActive").GetValue<bool>();

                if(HealthActive)
                {
                    foreach (Obj_AI_Turret turret in ObjectManager.Get<Obj_AI_Turret>())
                    {
                        if ((turret.HealthPercent == 100))
                        {
                            continue;
                        }

                        int health = 0;

                        switch (Menu.Item("TIHealth").GetValue<StringList>().SelectedIndex)
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
                            Helper.HealthDrawText(Helper.HealthText, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.LimeGreen);
                        }
                        else if (perHealth < 75 && perHealth >= 50)
                        {
                            Helper.HealthDrawText(Helper.HealthText, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.YellowGreen);
                        }
                        else if (perHealth < 50 && perHealth >= 25)
                        {
                            Helper.HealthDrawText(Helper.HealthText, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.Orange);
                        }
                        else if (perHealth < 25)
                        {
                            Helper.HealthDrawText(Helper.HealthText, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.Red);
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
                                Helper.HealthDrawText(Helper.HealthText, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.LimeGreen);
                            }
                            else if (health < 75 && health >= 50)
                            {
                                Helper.HealthDrawText(Helper.HealthText, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.YellowGreen);
                            }
                            else if (health < 50 && health >= 25)
                            {
                                Helper.HealthDrawText(Helper.HealthText, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.Orange);
                            }
                            else if (health < 25)
                            {
                                Helper.HealthDrawText(Helper.HealthText, health.ToString(CultureInfo.InvariantCulture), (int)pos[0], (int)pos[1], SharpDX.Color.Red);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in TurretHealth.OnUpdate + " + ex);
            }
        }
    }
}