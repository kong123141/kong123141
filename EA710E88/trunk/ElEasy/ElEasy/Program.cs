namespace ElEasy
{
    using System;

    using LeagueSharp;
    using LeagueSharp.Common;

    internal class Program
    {
        #region Methods

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }

        private static void OnLoad(EventArgs args)
        {
            try
            {
                Base.Load(ObjectManager.Player.ChampionName);
                Notifications.AddNotification("ElEasy - " + ObjectManager.Player.ChampionName + " 1.0.1.8", 8000);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion
    }
}