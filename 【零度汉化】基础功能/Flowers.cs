namespace LeagueSharp.Common
{
    using System;
    using System.Threading;
    using Lost = LeagueSharp.Hacks;

    class Flowers
    {
        public static bool duramk = false;
        public static float gameTime1 = 0;
        public static readonly Menu fl = new Menu("Flowers Utility", "Flowers Utility");

        internal static void Initilalize()
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            fl.AddItem(new MenuItem("Disable Drawing", "Screen display [I]").SetValue(new KeyBind('I', KeyBindType.Toggle, LeagueSharp.Hacks.DisableDrawings)));
            //fl.AddItem(new MenuItem("zoom hack", "Infinite horizon [danger]").SetValue(false)); 
            fl.AddItem(new MenuItem("disable say", "Ban said the script").SetValue(true));
            //fl.AddItem(new MenuItem("Tower Ranges", "Show enemy tower range").SetValue(false));
            fl.AddItem(new MenuItem("SaySomething", "Stop script loading information").SetValue(false));
            fl.AddItem(new MenuItem("SaySomething1", "By huabian"));
            CommonMenu.Config.AddSubMenu(fl);

            fl.Item("Disable Drawing").ValueChanged += Flowers_ValueChanged;

            if (fl.Item("SaySomething").GetValue<bool>())
            {
                Utility.DelayAction.Add(1000, () => {
                    Game.PrintChat("銆€");
                    Game.PrintChat("銆€");
                    Game.PrintChat("銆€");
                    Game.PrintChat("銆€");
                    Game.PrintChat("銆€");
                    Game.PrintChat("銆€");
                    Game.PrintChat("銆€");
                    Game.PrintChat("銆€");
                    Game.PrintChat("<font color=\"#FFA042\"><b>杈撳嚭/help鑾峰彇鍛戒护鍒楄〃</b></font>");
                });
            }

            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Flowers_ValueChanged(object sender, OnValueChangeEventArgs e)
        {
            if (fl.Item("Disable Drawing").GetValue<KeyBind>().Active)
            {
                LeagueSharp.Hacks.DisableDrawings = true;
            }
            else
            {
                LeagueSharp.Hacks.DisableDrawings = false;
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            /*if (fl.Item("zoom hack").IsActive())
            {
                Lost.ZoomHack = true;
            }
            else
            {
                Lost.ZoomHack = false;
            }*/

            if (fl.Item("disable say").GetValue<KeyBind>().Active)
            {
                Lost.DisableSay = true;
            }
            else
            {
                Lost.DisableSay = false;
            }

            /*if (fl.Item("Tower Ranges").GetValue<KeyBind>().Active)
            {
                Lost.TowerRanges = true;
            }
            else
            {
                Lost.TowerRanges = false;
            }*/
        }
    }
}