namespace VHR_SDK.Modules
{
    using LeagueSharp;
    using LeagueSharp.SDK.Core.Extensions;
    using LeagueSharp.SDK.Core.UI.IMenu.Values;
    using Interfaces;
    using Utility;

    class AutoEModule : IVHRModule
    {
        public bool ShouldBeLoaded()
        {
            return true;
        }

        public string GetModuleName()
        {
            return "Auto E Module";
        }

        public void OnLoad()
        {
            VHRDebug.WriteInfo("Auto E Module loaded successfully!");
        }

        public bool ShouldRun()
        {
            return VHR.VHRMenu["dz191.vhr.misc"]["dz191.vhr.misc.condemn"]["autoe"].GetValue<MenuBool>().Value && VHR.spells[SpellSlot.E] != null && VHR.spells[SpellSlot.E].IsReady();
        }

        public void Run()
        {
            var condemnTarget = VHR.GetCondemnTarget(ObjectManager.Player.ServerPosition);
            if (condemnTarget.IsValidTarget())
            {
                VHR.spells[SpellSlot.E].Cast(condemnTarget);
            }
        }
    }
}
