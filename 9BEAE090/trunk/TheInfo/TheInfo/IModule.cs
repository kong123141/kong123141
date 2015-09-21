using LeagueSharp.Common;

namespace TheInfo
{
    interface IModule
    {
        void Initialize();
        void InitializeMenu(Menu rootMenu);
    }
}
