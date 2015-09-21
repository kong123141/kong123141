using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace Prince_Urgot
{
    internal class ItemClass
    {
        public static SpellSlot IgniteSlot;
        private static Menu ItemMenu;
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public ItemClass(Menu itemMenu)
        {
            IgniteSlot = Player.GetSpellSlot("SummonerDot");
            ItemMenu = itemMenu;
            Menu();
            Game.OnUpdate += Game_OnGameUpdate;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {

            if (Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && IgniteSlot != SpellSlot.Unknown && IgniteSlot.IsReady() && ItemMenu.Item("useIgnite").GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(1200, TargetSelector.DamageType.Physical);
                var dmg = Player.GetSummonerSpellDamage(t, Damage.SummonerSpell.Ignite);
                if (t.Health < dmg && Player.Distance(t) < 600)
                    Player.Spellbook.CastSpell(IgniteSlot, t);
            }
        }

        private static void Menu()
        {
            ItemMenu.AddSubMenu(new Menu("Items", "items"));
            ItemMenu.SubMenu("items").AddItem(new MenuItem("useIgnite", "Use Ignite").SetValue(true));
        }
    }
}
