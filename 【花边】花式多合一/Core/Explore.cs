using System;
using Lost = LeagueSharp.Hacks;
using LeagueSharp;
using LeagueSharp.Common;

namespace 花边_花式多合一.Core
{
    internal class Explore
    {

        private static void Game_OnUpdate(EventArgs args)
        {
            if (InitializeMenu.Menu.Item("zoom hack").IsActive())
            {
                Lost.ZoomHack = true;
            }
            else
            {
                Lost.ZoomHack = false;
            }

            if (InitializeMenu.Menu.Item("Disable Drawing").GetValue<KeyBind>().Active)
            {
                Lost.DisableDrawings = true;
            }
            else
            {
                Lost.DisableDrawings = false;
            }

            if (InitializeMenu.Menu.Item("disable say").GetValue<KeyBind>().Active)
            {
                Lost.DisableSay = true;
            }
            else
            {
                Lost.DisableSay = false;
            }

            if (InitializeMenu.Menu.Item("Tower Ranges").GetValue<KeyBind>().Active)
            {
                Lost.TowerRanges = true;
            }
            else
            {
                Lost.TowerRanges = false;
            }
        }

        internal static void Game_OnGameLoad(EventArgs args)
        {
            try
            {
                if (!InitializeMenu.Menu.Item("Explore").GetValue<bool>()) return;
                Game.OnUpdate += Game_OnUpdate;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Explore error occurred: '{0}'", ex);
            }
        }
    }
}