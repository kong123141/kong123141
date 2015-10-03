namespace ElUtilitySuite.Plugins
{
    using System;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    public class Nidalee
    {
        #region Static Fields

        private static readonly Menu Menu = InitializeMenu.Menu;

        private static Spell champSpell;

        private static double totalDamage;

        #endregion

        #region Public Methods and Operators

        public static void Load()
        {
            champSpell = new Spell(SpellSlot.Q, 1500f);

            var championMenu = Menu.AddSubMenu(new Menu("Champion spells", "Championspells"));
            {
                championMenu.AddItem(
                    new MenuItem(
                        "Enabled-" + Entry.Player.ChampionName,
                        Entry.Player.ChampionName + " - " + champSpell.Slot)).SetValue(true);
            }

            Game.OnUpdate += OnGameUpdate;
        }

        #endregion

        #region Methods

        private static void OnGameUpdate(EventArgs args)
        {
            if (InitializeMenu.Menu.Item("ElSmite.Activated").GetValue<KeyBind>().Active)
            {
                if (Smite.minion == null
                    || !InitializeMenu.Menu.Item(Smite.minion.CharData.BaseSkinName).GetValue<bool>()
                    || !(Vector3.Distance(Entry.Player.ServerPosition, Smite.minion.ServerPosition) <= champSpell.Range))
                {
                    return;
                }

                Console.WriteLine(Smite.minion.CharData.BaseSkinName);

                totalDamage = Entry.Player.GetSpellDamage(Smite.minion, champSpell.Slot) + Smite.SmiteDamage();

                var pred = champSpell.GetPrediction(Smite.minion);

                if (InitializeMenu.Menu.Item("Enabled-" + ObjectManager.Player.ChampionName).GetValue<bool>()
                    && champSpell.IsReady() && Entry.Player.Spellbook.CanUseSpell(Smite.smite.Slot) == SpellState.Ready)
                {
                    if (totalDamage >= Smite.minion.Health && pred.Hitchance >= HitChance.Medium)
                    {
                        champSpell.Cast(pred.CastPosition);
                    }
                }

                if (InitializeMenu.Menu.Item("Enabled-" + ObjectManager.Player.ChampionName).GetValue<bool>()
                    && champSpell.IsReady())
                {
                    if (Entry.Player.GetSpellDamage(Smite.minion, champSpell.Slot) >= Smite.minion.Health
                        && pred.Hitchance >= HitChance.Medium)
                    {
                        champSpell.Cast(pred.CastPosition);
                    }
                }
            }
        }

        #endregion
    }
}