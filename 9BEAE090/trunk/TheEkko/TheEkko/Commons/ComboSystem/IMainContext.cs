using LeagueSharp.Common;

namespace TheBrand.ComboSystem
{
    /// <summary>
    /// The context needed by the ComboProvider, to access common data
    /// </summary>
    public interface IMainContext
    {
        Menu GetRootMenu();
        Orbwalking.Orbwalker GetOrbwalker();
    }
}
