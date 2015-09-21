#region Todo
    //      Move to Controller
#endregion Todo

using LeagueSharp;
using LeagueSharp.Common;

namespace EndifsCreations.Tools
{
    class myHumazier
    {
        private static Menu tools;
       
        public static void AddToMenu(Menu Tools)
        {
            tools = Tools;
            var subs = new Menu("Humanizer", "myHumazier");
            {
                subs.AddItem(new MenuItem(ObjectManager.Player.ChampionName + "mhz_spells", "Spells").SetValue(new Slider(300, 0, 1000)));
                subs.AddItem(new MenuItem(ObjectManager.Player.ChampionName + "mhz_reaction", "Reaction").SetValue(new Slider(300, 0, 1000)));
            }
            Tools.AddSubMenu(subs);
        }
        public static int SpellDelay
        {
            get
            {
                return tools.Item(ObjectManager.Player.ChampionName + "mhz_spells").GetValue<Slider>().Value;
            }
        }
        public static int ReactionDelay
        {
            get
            {
                return tools.Item(ObjectManager.Player.ChampionName + "mhz_reaction").GetValue<Slider>().Value;
            }
        }
    }   
}

