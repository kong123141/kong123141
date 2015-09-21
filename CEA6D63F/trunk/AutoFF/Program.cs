namespace AutoFF
{
    using System;

    using LeagueSharp;
    using LeagueSharp.Common;

    /// <summary>
    /// The program.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// The config.
        /// </summary>
        private static Menu config;

        /// <summary>
        /// The last surrender time.
        /// </summary>
        /// <returns>
        /// The surrender time.
        /// </returns>
        private static float lastSurrenderTime;

        /// <summary>
        /// Should I say Surrender?
        /// </summary>
        /// <param name="gameTime">
        /// Current Game Time.
        /// </param>
        /// <returns>
        /// Returns if I should surrender.
        /// </returns>
        private static bool Surrender(float gameTime)
        {
            return (gameTime + 30) >= lastSurrenderTime;
        }

        /// <summary>
        /// Called when Program Starts
        /// </summary>
        private static void Main()
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        /// <summary>
        /// Called when Game Loads
        /// </summary>
        /// <param name="args">
        /// The Args
        /// </param>
        private static void Game_OnGameLoad(EventArgs args)
        {
            config = new Menu("Auto Surrender", "menu", true);

            config.AddItem(new MenuItem("toggle", "Auto Surrender at Time Set").SetValue(true));
            config.AddItem(new MenuItem("time", "Set Time for Surrender").SetValue(new Slider(20, 15, 120)));
            config.AddToMainMenu();

            Game.PrintChat("<font color='#01DF3A'>Auto FF - Initialized</font>");
            Game.OnUpdate += Game_OnUpdate;
            Game.OnNotify += Game_OnNotify;
        }

        /// <summary>
        /// Called when the Game has a notification
        /// </summary>
        /// <param name="args">
        /// The Args
        /// </param>
        private static void Game_OnNotify(GameNotifyEventArgs args)
        {
            if (string.Equals(args.EventId.ToString(), "OnSurrenderVote") || args.EventId == GameEventId.OnSurrenderVote)
            {
                Game.Say("/ff");
            }
        }

        /// <summary>
        /// Called each time the Game Updates
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        private static void Game_OnUpdate(EventArgs args)
        {
            var time = config.Item("time").GetValue<Slider>().Value;

            if (Game.Time >= time * 60 && config.Item("toggle").GetValue<bool>() && Surrender(Game.Time))
            {
                Game.Say("/ff");
                lastSurrenderTime = Game.ClockTime;
            }
        }
    }
}
