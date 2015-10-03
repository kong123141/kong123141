namespace ElUtilitySuite.Plugins
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
                if (Smite.minion == null
                    || !InitializeMenu.Menu.Item(Smite.minion.CharData.BaseSkinName).GetValue<bool>()
                    || !(Vector3.Distance(Entry.Player.ServerPosition, Smite.minion.ServerPosition) <= champSpell.Range))
                {
                    return;
                }

                spellDamage = GetQ2Dmg(Smite.minion);
                totalDamage = spellDamage + Smite.SmiteDamage();

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
                    if (spellDamage >= Smite.minion.Health && pred.Hitchance >= HitChance.Medium)
                    {
                        champSpell.Cast(pred.CastPosition);
                    }
                }
            }
        }

        #endregion
    }
}