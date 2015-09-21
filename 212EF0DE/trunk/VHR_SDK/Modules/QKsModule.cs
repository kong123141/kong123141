using LeagueSharp;
using LeagueSharp.SDK.Core;
using LeagueSharp.SDK.Core.Enumerations;
using LeagueSharp.SDK.Core.Extensions;
using LeagueSharp.SDK.Core.Extensions.SharpDX;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.Wrappers;
using SharpDX;
using VHR_SDK.Interfaces;
using VHR_SDK.Utility;
using VHR_SDK.Utility.Helpers;

namespace VHR_SDK.Modules
{
    class QKsModule : IVHRModule
    {
        private float RealAARange = ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius;
        public bool ShouldBeLoaded()
        {
            return true;
        }

        public string GetModuleName()
        {
            return "Q KS Module";
        }

        public void OnLoad()
        {
            VHRDebug.WriteDebug("Q KS Module loaded successfully!");
        }

        public bool ShouldRun()
        {
            return VHR.VHRMenu["dz191.vhr.misc"]["dz191.vhr.misc.tumble"]["qinrange"].GetValue<MenuBool>().Value 
                && VHR.spells[SpellSlot.Q].IsReady()
                && (Orbwalker.ActiveMode == OrbwalkerMode.Orbwalk);
        }

        public void Run()
        {
            var currentTarget = TargetSelector.GetTarget(RealAARange + 240f);
            if (!currentTarget.IsValidTarget())
            {
                return;
            }

            if (currentTarget.ServerPosition.Distance(ObjectManager.Player.ServerPosition) <= RealAARange)
            {
                return;
            }
            var WHealth = currentTarget.Has2WStacks() ? SpellSlot.W.GetVHRSpellDamage(currentTarget) : 0;
            if (currentTarget.Health + 35 < ObjectManager.Player.GetAutoAttackDamage(currentTarget) + SpellSlot.Q.GetVHRSpellDamage(currentTarget) + WHealth)
            {
                var extendedPosition = ObjectManager.Player.ServerPosition.Extend(
                    currentTarget.ServerPosition, 300f);
                if (extendedPosition.IsSafePosition() && extendedPosition.PassesNoQIntoEnemiesCheck())
                {
                    VHR.Tumble(extendedPosition, currentTarget);
                }
            }
        }
    }
}
