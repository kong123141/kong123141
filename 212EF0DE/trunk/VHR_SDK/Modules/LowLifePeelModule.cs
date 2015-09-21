namespace VHR_SDK.Modules
{
    using LeagueSharp;
    using LeagueSharp.SDK.Core.Extensions;
    using LeagueSharp.SDK.Core.UI.IMenu.Values;
    using Interfaces;
    using Utility;
    using LeagueSharp.SDK.Core.Wrappers;
    using System.Linq;
    using Utility.Helpers;

    class LowLifePeel : IVHRModule
    {
        public bool ShouldBeLoaded()
        {
            return true;
        }

        public string GetModuleName()
        {
            return "Low Life Peel Module";
        }

        public void OnLoad()
        {
            VHRDebug.WriteInfo("Low Life Peel Module loaded successfully!");
        }

        public bool ShouldRun()
        {
            return VHR.spells[SpellSlot.E].IsReady() && !VHR.spells[SpellSlot.Q].IsReady() && VHR.VHRMenu["dz191.vhr.misc"]["dz191.vhr.misc.condemn"]["lowlifepeel"].GetValue<MenuBool>().Value && (ObjectManager.Player.HealthPercent <= 25);
        }

        public void Run()
        {
            var meleeEnemies = ObjectManager.Player.GetEnemiesInRange(400f).FindAll(m => m.IsMelee);

            if (meleeEnemies.Any())
            {
                var mostDangerous = meleeEnemies.OrderByDescending(m => m.GetAutoAttackDamage(ObjectManager.Player)).First();
                    VHR.spells[SpellSlot.E].Cast(mostDangerous);
            }
        }
    }
}
