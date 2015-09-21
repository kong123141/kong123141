using LeagueSharp;
using LeagueSharp.SDK.Core.Extensions;
using LeagueSharp.SDK.Core.Extensions.SharpDX;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using SharpDX;
using VHR_SDK.Interfaces;
using VHR_SDK.Utility;
using VHR_SDK.Utility.Helpers;

namespace VHR_SDK.Modules
{
    class WallTumbleModule : IVHRModule
    {
        public bool ShouldBeLoaded()
        {
            return true;
        }

        public string GetModuleName()
        {
            return "WallTumble Module";
        }

        public void OnLoad()
        {
            VHRDebug.WriteDebug("WallTumble Module loaded successfully!");
        }

        public bool ShouldRun()
        {
            return VHR.VHRMenu["dz191.vhr.misc"]["dz191.vhr.misc.tumble"]["walltumble"].GetValue<MenuKeyBind>().Active && VHR.spells[SpellSlot.Q].IsReady() && VHRHelpers.IsSummonersRift();
        }

        public void Run()
        {
            Vector2 drakeWallQPos = new Vector2(11514, 4462);

            if (ObjectManager.Player.Position.X < 12000 || ObjectManager.Player.Position.X > 12070 || ObjectManager.Player.Position.Y < 4800 ||
                ObjectManager.Player.Position.Y > 4872)
            {
                VHRHelpers.MoveToLimited(new Vector2(12050, 4827).ToVector3());
            }
            else
            {
                VHRHelpers.MoveToLimited(new Vector2(12050, 4827).ToVector3());
                VHR.spells[SpellSlot.Q].Cast(drakeWallQPos);
            }
        }
    }
}
