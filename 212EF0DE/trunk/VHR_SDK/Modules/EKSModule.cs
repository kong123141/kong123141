namespace VHR_SDK.Modules
{
    using LeagueSharp;
    using LeagueSharp.SDK.Core.Extensions;
    using LeagueSharp.SDK.Core.UI.IMenu.Values;
    using Interfaces;
    using Utility;
    using LeagueSharp.SDK.Core;
    using LeagueSharp.SDK.Core.Wrappers;
    using Utility.Helpers;

    class EKSModule : IVHRModule
    {
        public bool ShouldBeLoaded()
        {
            return false;
        }

        public string GetModuleName()
        {
            return "E KS Module";
        }

        public void OnLoad()
        {
            VHRDebug.WriteInfo("E KS Module loaded successfully!");
        }

        public bool ShouldRun()
        {
            return VHR.spells[SpellSlot.E].IsReady() && !VHR.spells[SpellSlot.Q].IsReady() && VHR.VHRMenu["dz191.vhr.misc"]["dz191.vhr.misc.condemn"]["eks"].GetValue<MenuBool>().Value && !ObjectManager.Player.HasBuff("vaynetumblebonus");
        }

        public void Run()
        {
            var target = GameObjects.EnemyHeroes.Find(en => en.IsValidTarget(VHR.spells[SpellSlot.E].Range) && en.Has2WStacks());
            if (target != null && !target.IsInvulnerable(DamageType.Physical, false) && target.Health + 60 <= (ObjectManager.Player.GetSpellDamage(target, SpellSlot.E) + ObjectManager.Player.GetSpellDamage(target, SpellSlot.W)))
            {
                VHR.spells[SpellSlot.E].Cast(target);
            }
        }
    }
}
