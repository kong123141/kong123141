using LeagueSharp.SDK.Core;
using LeagueSharp.SDK.Core.Enumerations;
using LeagueSharp.SDK.Core.Utils;
using VHR_SDK.Utility.Helpers;

namespace VHR_SDK.Modules
{
    using LeagueSharp;
    using LeagueSharp.SDK.Core.Extensions;
    using LeagueSharp.SDK.Core.UI.IMenu.Values;
    using Interfaces;
    using Utility;

    class Focus2Stacks : IVHRModule
    {
        public bool ShouldBeLoaded()
        {
            return true;
        }

        public string GetModuleName()
        {
            return "Focus Targets with 2 W marks Module";
        }

        public void OnLoad()
        {
            VHRDebug.WriteInfo("Focus Targets with 2 W marks Module loaded successfully!");
        }

        public bool ShouldRun()
        {
            return VHR.VHRMenu["dz191.vhr.misc"]["dz191.vhr.misc.general"]["specialfocus"].GetValue<MenuBool>().Value && (Orbwalker.ActiveMode == OrbwalkerMode.Hybrid || Orbwalker.ActiveMode == OrbwalkerMode.Orbwalk);
        }

        public void Run()
        {
            var TwoStacksTarget = VHRExtensions.GetHeroWith2WStacks();
            if (TwoStacksTarget != null && TwoStacksTarget.IsValidTarget(ObjectManager.Player.AttackRange + (1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1)) &&
                TwoStacksTarget != Orbwalker.OrbwalkTarget && !ObjectManager.Player.IsWindingUp)
            {
                    if (TwoStacksTarget.IsValidTarget() && !ObjectManager.Player.IsWindingUp)
                    {
                        Orbwalker.Attack = false;
                        ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, TwoStacksTarget);
                        DelayAction.Add((int)(Game.Ping / 2f + ObjectManager.Player.AttackDelay * 1000 + 250 + 50), () =>
                        {
                            Orbwalker.Attack = true;
                        });
                    }
            }
        }
    }
}
