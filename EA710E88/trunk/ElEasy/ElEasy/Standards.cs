namespace ElEasy
{
    using System.Drawing;

    using LeagueSharp;
    using LeagueSharp.Common;

    public enum Spells
    {
        Q,

        W,

        E,

        R
    }

    public class Standards
    {
        #region Static Fields

        public static Menu Menu;

        protected static SpellSlot Ignite;

        protected static int LastNotification = 0;

        protected static Orbwalking.Orbwalker Orbwalker;

        #endregion

        #region Properties

        protected static Obj_AI_Hero Player
        {
            get
            {
                return ObjectManager.Player;
            }
        }

        #endregion

        #region Methods

        protected static void ShowNotification(string message, Color color, int duration = -1, bool dispose = true)
        {
            Notifications.AddNotification(new Notification(message, duration, dispose).SetTextColor(color));
        }

        #endregion
    }
}