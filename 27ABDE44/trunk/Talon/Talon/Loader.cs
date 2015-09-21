using LeagueSharp;
using LeagueSharp.Common;

namespace Talon
{
    internal class Loader
    {
        static void Main(string[] args)
        {
            if (args != null)
            {
                CustomEvents.Game.OnGameLoad += eventArgs =>
                {
                    if (ObjectManager.Player.ChampionName != "Talon")
                        return;

                    //Helper.Updater.Init("Apollo16/LeagueSharp/master/Talon/Talon");
                    Spells.Init();
                    Config.Init();
                    Helper.AssassinManager.Init();
                    Mechanics.Init();
                    Config.ShowNotification(
                        "Apollo's " + ObjectManager.Player.ChampionName + " Loaded", Config.NotificationColor, 7000);
                };
            }
        }
    }
}
