namespace LeagueSharp.Common
{
    using System;
    using Lost = LeagueSharp.Hacks;

    class Flowers
    {
        public static bool duramk = false;
        public static float gameTime1 = 0;
        public static readonly Menu fl = new Menu("Flowers Utility", "Flowers Utility");

        internal static void Initialize()
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            fl.AddItem(new MenuItem("Disable Drawing", "Screen display [I]").SetValue(new KeyBind('I', KeyBindType.Toggle, Lost.DisableDrawings)));
            fl.AddItem(new MenuItem("disable say", "Ban said the script").SetValue(true));
            fl.AddItem(new MenuItem("SaySomething", "Stop script loading information").SetValue(false));
            fl.AddItem(new MenuItem("SaySomething1", "By huabian"));
            CommonMenu.Instance.AddSubMenu(fl);

            fl.Item("Disable Drawing").ValueChanged += (s, e) => 
            {
                Lost.DisableDrawings = e.GetNewValue<KeyBind>().Active;
            };

            fl.Item("disable say").ValueChanged += (s, e) => 
            {
                Lost.DisableSay = e.GetNewValue<bool>();
            };

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

        }
    }
}