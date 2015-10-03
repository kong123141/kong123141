using LeagueSharp;
using LeagueSharp.Common;

namespace EndifsCreations.Controller
{
    internal static class myHumazier
    {
        private static Menu Menu;
        public static void AddToMenu(Menu menu)
        {
            Menu = menu;
            Menu.AddItem(new MenuItem("EC." + ObjectManager.Player.ChampionName + ".HM.Spells", "Spells").SetValue(new Slider(300, 0, 1000)));
            Menu.AddItem(new MenuItem("EC." + ObjectManager.Player.ChampionName + ".HM.Reaction", "Reaction").SetValue(new Slider(300, 0, 1000)));
            Menu.AddItem(new MenuItem("EC." + ObjectManager.Player.ChampionName + ".HM.Movement", "Movement").SetValue(new Slider(100, 0, 1000)));
            Menu.AddItem(new MenuItem("EC." + ObjectManager.Player.ChampionName + ".HM.MovementRandomize", "Movement Randomize").SetValue(true));            
        }
        public static int SpellDelay
        {
            get
            {
                return Menu.Item("EC." + ObjectManager.Player.ChampionName + ".HM.Spells").GetValue<Slider>().Value;
            }
        }
        public static int ReactionDelay
        {
            get
            {
                return Menu.Item("EC." + ObjectManager.Player.ChampionName + ".HM.Reaction").GetValue<Slider>().Value;
            }
        }
        public static int MovementDelay
        {
            get
            {
                return Menu.Item("EC." + ObjectManager.Player.ChampionName + ".HM.Movement").GetValue<Slider>().Value;
            }
        }
        public static bool MovementRandomize
        {
            get
            {
                return Menu.Item("EC." + ObjectManager.Player.ChampionName + ".HM.MovementRandomize").GetValue<bool>();
            }
        }
    }   
}

