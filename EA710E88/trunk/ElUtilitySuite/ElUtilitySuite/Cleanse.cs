namespace ElUtilitySuite
{
    using System;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using ItemData = LeagueSharp.Common.Data.ItemData;
    using SS = LeagueSharp.SpellSlot;

    public static class Cleanse
    {
        #region Static Fields

        public static SS summonerCleanse;

        private static Spell cleanseSpell;

        private static SpellDataInst slot1;

        private static SpellDataInst slot2;

        #endregion

        #region Public Methods and Operators

        public static void Load()
        {
            try
            {
                slot1 = Entry.Player.Spellbook.GetSpell(SpellSlot.Summoner1);
                slot2 = Entry.Player.Spellbook.GetSpell(SpellSlot.Summoner2);

                //Soon riot will introduce multiple cleanses, mark my words.
                var cleanseNames = new[] { "summonerboost" };

                if (cleanseNames.Contains(slot1.Name))
                {
                    cleanseSpell = new Spell(SpellSlot.Summoner1, 550f);
                    summonerCleanse = SpellSlot.Summoner1;
                }
                else if (cleanseNames.Contains(slot2.Name))
                {
                    cleanseSpell = new Spell(SpellSlot.Summoner2, 550f);
                    summonerCleanse = SpellSlot.Summoner2;
                }
                else
                {
                    Console.WriteLine("You don't have cleanse faggot");
                    return;
                }

                Game.OnUpdate += OnUpdate;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        #endregion

        #region Methods

        private static void AllyCleanse()
        {
            var mikaels = ItemData.Mikaels_Crucible.GetItem();
            var delay = InitializeMenu.Menu.Item("Cleanse.Delay").GetValue<Slider>().Value * 10;

            foreach (var unit in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        x =>
                        x.IsAlly && !x.IsMe && x.IsValidTarget(900, false)
                        && InitializeMenu.Menu.Item("Protect.Cleanse.Kappa" + x.SkinName).GetValue<bool>()
                        && InitializeMenu.Menu.Item("Protect.Cleanse.Mikeals.Activated").GetValue<bool>())
                    .OrderByDescending(xe => xe.Health / xe.MaxHealth * 100))
            {
                foreach (var b in unit.Buffs)
                {
                    if (mikaels.IsOwned(Entry.Player) && mikaels.IsReady())
                    {
                        if (InitializeMenu.Menu.Item("Protect.Cleanse.Slow.Ally").GetValue<bool>()
                            && b.Type == BuffType.Slow)
                        {
                            Utility.DelayAction.Add(delay, () => mikaels.Cast(unit));
                        }

                        if (InitializeMenu.Menu.Item("Protect.Cleanse.Stun.Ally").GetValue<bool>()
                            && b.Type == BuffType.Stun)
                        {
                            Utility.DelayAction.Add(delay, () => mikaels.Cast(unit));
                        }

                        if (InitializeMenu.Menu.Item("Protect.Cleanse.Charm.Ally").GetValue<bool>()
                            && b.Type == BuffType.Charm)
                        {
                            Utility.DelayAction.Add(delay, () => mikaels.Cast(unit));
                        }

                        if (InitializeMenu.Menu.Item("Protect.Cleanse.Taunt.Ally").GetValue<bool>()
                            && b.Type == BuffType.Taunt)
                        {
                            Utility.DelayAction.Add(delay, () => mikaels.Cast(unit));
                        }

                        if (InitializeMenu.Menu.Item("Protect.Cleanse.Fear").GetValue<bool>() && b.Type == BuffType.Fear)
                        {
                            Utility.DelayAction.Add(delay, () => mikaels.Cast(unit));
                        }

                        if (InitializeMenu.Menu.Item("Protect.Cleanse.Snare").GetValue<bool>()
                            && b.Type == BuffType.Snare)
                        {
                            Utility.DelayAction.Add(delay, () => mikaels.Cast(unit));
                        }

                        if (InitializeMenu.Menu.Item("Protect.Cleanse.Silence.Ally").GetValue<bool>()
                            && b.Type == BuffType.Silence)
                        {
                            Utility.DelayAction.Add(delay, () => mikaels.Cast(unit));
                        }

                        if (InitializeMenu.Menu.Item("Protect.Cleanse.Suppression.Ally").GetValue<bool>()
                            && b.Type == BuffType.Suppression)
                        {
                            Utility.DelayAction.Add(delay, () => mikaels.Cast(unit));
                        }

                        if (InitializeMenu.Menu.Item("Protect.Cleanse.Polymorph").GetValue<bool>()
                            && b.Type == BuffType.Polymorph)
                        {
                            Utility.DelayAction.Add(delay, () => mikaels.Cast(unit));
                        }

                        if (InitializeMenu.Menu.Item("Protect.Cleanse.Blind").GetValue<bool>()
                            && b.Type == BuffType.Blind)
                        {
                            Utility.DelayAction.Add(delay, () => mikaels.Cast(unit));
                        }

                        if (InitializeMenu.Menu.Item("Protect.Cleanse.Posion").GetValue<bool>()
                            && b.Type == BuffType.Poison)
                        {
                            Utility.DelayAction.Add(delay, () => mikaels.Cast(unit));
                        }
                    }
                }
            }
        }

        private static void CleanseItems()
        {
            var quicksilver = ItemData.Quicksilver_Sash.GetItem();
            var dervish = ItemData.Dervish_Blade.GetItem();
            var mercurial = ItemData.Mercurial_Scimitar.GetItem();
            var mikaels = ItemData.Mikaels_Crucible.GetItem();

            if (InitializeMenu.Menu.Item("usecombo").GetValue<KeyBind>().Active
                || InitializeMenu.Menu.Item("cmode").GetValue<StringList>().SelectedIndex != 1)
            {
                if (quicksilver.IsOwned(Entry.Player) && quicksilver.IsReady())
                {
                    quicksilver.Cast();
                }
                else if (dervish.IsOwned() && dervish.IsReady())
                {
                    dervish.Cast();
                }
                else if (mercurial.IsOwned(Entry.Player) && mercurial.IsReady())
                {
                    mercurial.Cast();
                }
                else if (mikaels.IsOwned(Entry.Player) && mikaels.IsReady())
                {
                    mikaels.Cast();
                }
            }
        }


        private static bool DangerousSpells()
        {
            return Entry.Player.HasBuffOfType(BuffType.Suppression) || Entry.Player.HasBuff("ZedR")
                   || Entry.Player.HasBuff("vladimirhemoplague") || Entry.Player.HasBuff("urgotswap2")
                   || Entry.Player.HasBuff("MordekaiserChildrenOfTheGrave")
                   || Entry.Player.HasBuff("fiorarmark");
        }

        private static bool IsCleanseReady()
        {
            return summonerCleanse != SpellSlot.Unknown
                   && Entry.Player.Spellbook.CanUseSpell(cleanseSpell.Slot) == SpellState.Ready;
        }

        private static void OnUpdate(EventArgs args)
        {
            try
            {
                if (Entry.Player.IsDead)
                {
                    return;
                }

                if (InitializeMenu.Menu.Item("Cleanse.Activated").GetValue<bool>())
                {
                    UseCleanse();
                    AllyCleanse();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        private static void UseCleanse()
        {
            if (Entry.Player.HasBuffOfType(BuffType.SpellShield) || Entry.Player.HasBuffOfType(BuffType.SpellImmunity))
            {
                return;
            }

            var delay = InitializeMenu.Menu.Item("Cleanse.Delay").GetValue<Slider>().Value * 10;

            foreach (var b in Entry.Player.Buffs)
            {
                if (InitializeMenu.Menu.Item("Protect.Cleanse.Slow").GetValue<bool>() && b.Type == BuffType.Slow)
                {
                    if (IsCleanseReady())
                    {
                        Utility.DelayAction.Add(
                            delay,
                            () => Entry.Player.Spellbook.CastSpell(cleanseSpell.Slot, Entry.Player));
                        return;
                    }

                    Utility.DelayAction.Add(delay, () => CleanseItems());
                }

                if (InitializeMenu.Menu.Item("Protect.Cleanse.Stun").GetValue<bool>() && b.Type == BuffType.Stun)
                {
                    if (IsCleanseReady())
                    {
                        Utility.DelayAction.Add(
                            delay,
                            () => Entry.Player.Spellbook.CastSpell(cleanseSpell.Slot, Entry.Player));
                        return;
                    }

                    Utility.DelayAction.Add(delay, () => CleanseItems());
                }

                if (InitializeMenu.Menu.Item("Protect.Cleanse.Charm").GetValue<bool>() && b.Type == BuffType.Charm)
                {
                    if (IsCleanseReady())
                    {
                        Utility.DelayAction.Add(
                            delay,
                            () => Entry.Player.Spellbook.CastSpell(cleanseSpell.Slot, Entry.Player));
                        return;
                    }

                    Utility.DelayAction.Add(delay, () => CleanseItems());
                }

                if (InitializeMenu.Menu.Item("Protect.Cleanse.Taunt").GetValue<bool>() && b.Type == BuffType.Taunt)
                {
                    if (IsCleanseReady())
                    {
                        Utility.DelayAction.Add(
                            delay,
                            () => Entry.Player.Spellbook.CastSpell(cleanseSpell.Slot, Entry.Player));
                        return;
                    }

                    Utility.DelayAction.Add(delay, () => CleanseItems());
                }

                if (InitializeMenu.Menu.Item("Protect.Cleanse.Fear").GetValue<bool>() && b.Type == BuffType.Fear)
                {
                    if (IsCleanseReady())
                    {
                        Utility.DelayAction.Add(
                            delay,
                            () => Entry.Player.Spellbook.CastSpell(cleanseSpell.Slot, Entry.Player));
                        return;
                    }

                    Utility.DelayAction.Add(delay, () => CleanseItems());
                }

                if (InitializeMenu.Menu.Item("Protect.Cleanse.Snare").GetValue<bool>() && b.Type == BuffType.Snare)
                {
                    if (IsCleanseReady())
                    {
                        Utility.DelayAction.Add(
                            delay,
                            () => Entry.Player.Spellbook.CastSpell(cleanseSpell.Slot, Entry.Player));
                        return;
                    }

                    Utility.DelayAction.Add(delay, () => CleanseItems());
                }

                if (InitializeMenu.Menu.Item("Protect.Cleanse.Silence").GetValue<bool>() && b.Type == BuffType.Silence)
                {
                    if (IsCleanseReady())
                    {
                        Utility.DelayAction.Add(
                            delay,
                            () => Entry.Player.Spellbook.CastSpell(cleanseSpell.Slot, Entry.Player));
                        return;
                    }

                    Utility.DelayAction.Add(delay, () => CleanseItems());
                }

                if (InitializeMenu.Menu.Item("Protect.Cleanse.Suppression").GetValue<bool>()
                    && b.Type == BuffType.Suppression)
                {
                    if (IsCleanseReady())
                    {
                        Utility.DelayAction.Add(
                            delay,
                            () => Entry.Player.Spellbook.CastSpell(cleanseSpell.Slot, Entry.Player));
                        return;
                    }

                    Utility.DelayAction.Add(delay, () => CleanseItems());
                }

                if (InitializeMenu.Menu.Item("Protect.Cleanse.Polymorph").GetValue<bool>()
                    && b.Type == BuffType.Polymorph)
                {
                    if (IsCleanseReady())
                    {
                        Utility.DelayAction.Add(
                            delay,
                            () => Entry.Player.Spellbook.CastSpell(cleanseSpell.Slot, Entry.Player));
                        return;
                    }

                    Utility.DelayAction.Add(delay, () => CleanseItems());
                }

                if (InitializeMenu.Menu.Item("Protect.Cleanse.Blind").GetValue<bool>() && b.Type == BuffType.Blind)
                {
                    if (IsCleanseReady())
                    {
                        Utility.DelayAction.Add(
                            delay,
                            () => Entry.Player.Spellbook.CastSpell(cleanseSpell.Slot, Entry.Player));
                        return;
                    }

                    Utility.DelayAction.Add(delay, () => CleanseItems());
                }

                if (InitializeMenu.Menu.Item("Protect.Cleanse.Posion").GetValue<bool>() && b.Type == BuffType.Poison)
                {
                    if (IsCleanseReady())
                    {
                        Utility.DelayAction.Add(
                            delay,
                            () => Entry.Player.Spellbook.CastSpell(cleanseSpell.Slot, Entry.Player));
                        return;
                    }

                    Utility.DelayAction.Add(delay, () => CleanseItems());
                }

                if (DangerousSpells())
                {
                    if (Entry.Player.HasBuff("ZedR"))
                    {
                        Utility.DelayAction.Add(delay + 1800, () => CleanseItems());
                        return;
                    }

                    if (Entry.Player.HasBuff("SkarnerR"))
                    {
                        Utility.DelayAction.Add(delay + 1000, () => CleanseItems());
                        return;
                    }

                    CleanseItems();
                    return;
                }
            }
        }

        #endregion
    }
}