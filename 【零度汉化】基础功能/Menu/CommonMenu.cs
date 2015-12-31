namespace LeagueSharp.Common
{
    /// <summary>
    ///     The LeagueSharp.Common official menu.
    /// </summary>
    internal class CommonMenu
    {
        #region Static Fields

        /// <summary>
        ///     The menu instance.
        /// </summary>
        internal static Menu Instance = new Menu("\u3010\u96F6\u5EA6\u6C49\u5316\u3011\u57FA\u7840\u529F\u80FD", "LeagueSharp.Common", true);

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a static instance of the <see cref="CommonMenu" /> class.
        /// </summary>
        static CommonMenu()
        {
            TargetSelector.Initialize();
            Prediction.Initialize();
            Flowers.Initialize();
            //Hacks.Initialize();
            FakeClicks.Initialize();

            Instance.AddToMainMenu();
        }

        #endregion
    }
}