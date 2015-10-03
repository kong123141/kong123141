namespace ElSmite.Plugins
{
    using System;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    public class LeeSin
    {
        #region Static Fields

        private static readonly Menu Menu = InitializeMenu.Menu;

        private static Spell champSpell;

        private static double spellDamage;

        private static double totalDamage;

        #endregion

        #region Public Methods and Operators

        public static void Load()
        {
            champSpell = new Spell(SpellSlot.Q, 1100f);

            var championMenu = Menu.AddSubMenu(new Menu("Champion spells", "Championspells"));
            {
                championMenu.AddItem(
                    new MenuItem(
                        "Enabled-" + Entry.Player.ChampionName,
                        Entry.Player.ChampionName + " - " + champSpell.Slot)).SetValue(false);
            }

            Game.OnUpdate += OnGameUpdate;
        }

        #endregion

        #region Methods

        private static double GetQ2Dmg(Obj_AI_Base target)
        {
            int[] dmgQ = { 50, 80, 110, 140, 170 };

            var damage = ObjectManager.Player.CalcDamage(
                target,
                Damage.DamageType.Physical,
                dmgQ[champSpell.Level - 1] + 0.9 * ObjectManager.Player.FlatPhysicalDamageMod
                + 0.08 * (target.MaxHealth - target.Health));

            return damage > 400 ? 400 : damage;
        }

        private static void OnGameUpdate(EventArgs args)
        {
            if (InitializeMenu.Menu.Item("ElSmite.Activated").GetValue<KeyBind>().Active)
            {
                if (Entry.minion == null
                    || !InitializeMenu.Menu.Item(Entry.minion.CharData.BaseSkinName).GetValue<bool>()
                    || !(Vector3.Distance(Entry.Player.ServerPosition, Entry.minion.ServerPosition) <= champSpell.Range))
                {
                    return;
                }

                spellDamage = GetQ2Dmg(Entry.minion);
                totalDamage = spellDamage + Entry.SmiteDamage();

                var pred = champSpell.GetPrediction(Entry.minion);

                if (InitializeMenu.Menu.Item("Enabled-" + ObjectManager.Player.ChampionName).GetValue<bool>()
                    && champSpell.IsReady() && Entry.Player.Spellbook.CanUseSpell(Entry.smite.Slot) == SpellState.Ready)
                {
                    if (totalDamage >= Entry.minion.Health && pred.Hitchance >= HitChance.Medium)
                    {
                        champSpell.Cast(pred.CastPosition);
                    }
                }

                if (InitializeMenu.Menu.Item("Enabled-" + ObjectManager.Player.ChampionName).GetValue<bool>()
                    && champSpell.IsReady())
                {
                    if (spellDamage >= Entry.minion.Health && pred.Hitchance >= HitChance.Medium)
                    {
                        champSpell.Cast(pred.CastPosition);
                    }
                }
            }
        }

        #endregion
    }
}