using System;
using LeagueSharp.SDK.Core.UI.IMenu;
using LeagueSharp.SDK.Core.Wrappers;
using VHR_SDK.Enumerations;

namespace VHR_SDK.Interfaces
{
    interface IVHRItem
    {
        bool ShouldBeLoaded();

        String GetItemName();

        Items.Item GetItem();

        void CreateMenu(Menu mainMenu);

        ItemTypes GetItemType();

        bool ShouldRun();

        bool Run();
    }
}
