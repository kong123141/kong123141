namespace ElDiana
{
    using LeagueSharp.Common;

    internal class Program
    {
        #region Methods

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Diana.OnLoad;
        }

        #endregion
    }
}