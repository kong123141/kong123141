namespace Flowers_Utility
{
    using System;
    using LeagueSharp;
    using LeagueSharp.Common;
    using Flowers_Utility.Common;

    class Program
    {
        public static Menu Menu;
        public static Menu LoadPlugingMenu;
        public static Pluging.GrassWard GrassWard;
        public static Pluging.TurretHealth TurretHealth;
        public static Pluging.TurretRange TurretRange;
        public static Pluging.GankAlerter GankAlerter;
        public static Pluging.TrackerCoolDown TrackerCoolDown;
        public static Pluging.JungleTime JungleTime;
        public static Pluging.AutoLevels AutoLevels;
        public static Pluging.SharedExperience SharedExperience;
        public static Pluging.ShadowTracker ShadowTracker;
        public static Pluging.Evade Evade;
        public static Pluging.Activator Activator;

        static void Main(string[] args)
        {
            DelayAction.Add(0, () =>
            {
                if(Game.Mode == GameMode.Running)
                {
                    OnLoad(new EventArgs());
                }
                else
                {
                    Game.OnStart += OnLoad;
                }
            });
        }

        private static void OnLoad(EventArgs args)
        {
            try
            {
                Menu = new Menu("Flowers - 多合一", "NightMoon", true).SetFontStyle(System.Drawing.FontStyle.Regular, SharpDX.Color.RoyalBlue);

                Menu.AddToMainMenu();

                LoadPlugingMenu = new Menu("[FL] 多合一插件加载", "nightmoon.loadpluging", true).SetFontStyle(System.Drawing.FontStyle.Regular, SharpDX.Color.YellowGreen);
                LoadPlugingMenu.AddItem(new MenuItem("LoadGrassWard", "加载 进草插眼 插件").SetValue(false));
                LoadPlugingMenu.AddItem(new MenuItem("LoadTowerHealth", "加载 防御塔血量显示 插件").SetValue(false));
                LoadPlugingMenu.AddItem(new MenuItem("LoadTowerRange", "加载 防御塔范围显示 插件").SetValue(false));
                LoadPlugingMenu.AddItem(new MenuItem("LoadJungleTime", "加载 打野计时 插件").SetValue(false));
                LoadPlugingMenu.AddItem(new MenuItem("LoadGankAlerter", "加载 Gank提示 插件").SetValue(false));
                LoadPlugingMenu.AddItem(new MenuItem("LoadTrackerCoolDown", "加载 眼位CD 插件").SetValue(false));
                LoadPlugingMenu.AddItem(new MenuItem("LoadAutoLevels", "加载 自动加点 插件").SetValue(false));
                LoadPlugingMenu.AddItem(new MenuItem("LoadSharedExperience", "加载 经验分流 插件").SetValue(false));
                LoadPlugingMenu.AddItem(new MenuItem("LoadShadowTracker", "加载 真身显示 插件").SetValue(false));
                LoadPlugingMenu.AddItem(new MenuItem("LoadEvade", "加载 躲避 插件").SetValue(false));
                LoadPlugingMenu.AddItem(new MenuItem("LoadActivator", "加载 自动嗑药 插件").SetValue(false));
                LoadPlugingMenu.AddItem(new MenuItem("LoadLoadLoad", "开关插件需要F5"));

                LoadPlugingMenu.AddToMainMenu();


                //Menu.AddSubMenu(LoadPluging);



                LoadPlugingEvents();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Program.OnLoad + " + ex);
            }
        }

        private static void LoadPlugingEvents()
        {
            try
            {
                if (LoadPlugingMenu.Item("LoadTrackerCoolDown").GetValue<bool>())
                    TrackerCoolDown = new Pluging.TrackerCoolDown(Menu);

                if (LoadPlugingMenu.Item("LoadTowerHealth").GetValue<bool>())
                    TurretHealth = new Pluging.TurretHealth(Menu);

                if (LoadPlugingMenu.Item("LoadTowerRange").GetValue<bool>())
                    TurretRange = new Pluging.TurretRange(Menu);

                if (LoadPlugingMenu.Item("LoadGrassWard").GetValue<bool>())
                    GrassWard = new Pluging.GrassWard(Menu);
                
                if (LoadPlugingMenu.Item("LoadJungleTime").GetValue<bool>())
                    JungleTime = new Pluging.JungleTime(Menu);

                if (LoadPlugingMenu.Item("LoadGankAlerter").GetValue<bool>())
                    GankAlerter = new Pluging.GankAlerter(Menu);

                if (LoadPlugingMenu.Item("LoadSharedExperience").GetValue<bool>())
                    SharedExperience = new Pluging.SharedExperience(Menu);

                if (LoadPlugingMenu.Item("LoadShadowTracker").GetValue<bool>())
                    ShadowTracker = new Pluging.ShadowTracker(Menu);

                if (LoadPlugingMenu.Item("LoadEvade").GetValue<bool>())
                    Evade = new Pluging.Evade(Menu);

                if (LoadPlugingMenu.Item("LoadActivator").GetValue<bool>())
                    Activator = new Pluging.Activator(Menu); 

                if (LoadPlugingMenu.Item("LoadAutoLevels").GetValue<bool>())
                    AutoLevels = new Pluging.AutoLevels(Menu);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Program.LoadPlugingEvents + " + ex);
            }
        }
    }
}
