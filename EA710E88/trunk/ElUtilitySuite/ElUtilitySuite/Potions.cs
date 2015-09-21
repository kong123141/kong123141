namespace ElUtilitySuite
{
    using System;

    using LeagueSharp;
    using LeagueSharp.Common;

    using ItemData = LeagueSharp.Common.Data.ItemData;

    internal static class Potions
    {
        #region Public Methods and Operators

        public static void Load()
        {
            try
            {
                Game.OnUpdate += OnUpdate;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        #endregion

        #region Methods

        private static void OnUpdate(EventArgs args)
        {
            if (Entry.Player.IsDead)
            {
                return;
            }

            try
            {
                PotionManager();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        private static bool PotCheck()
        {
            return Entry.Player.HasBuff("FlaskOfCrystalWater") || Entry.Player.HasBuff("RegenerationPotion")
                   || Entry.Player.HasBuff("ItemCrystalFlask") || Entry.Player.HasBuff("ItemMiniRegenPotion");
        }

        private static void PotionManager()
        {
            if (!InitializeMenu.Menu.Item("Potions.Activated").GetValue<bool>() || Entry.Player.InFountain()
                || Entry.Player.IsRecalling() || PotCheck())
            {
                return;
            }

            var healthPotion = ItemData.Health_Potion.GetItem();
            var manaPotion = ItemData.Mana_Potion.GetItem();
            var crystallineFlask = ItemData.Crystalline_Flask.GetItem();
            var biscuit = ItemData.Total_Biscuit_of_Rejuvenation2.GetItem();

            if (Entry.Player.Health / Entry.Player.MaxHealth * 100
                < InitializeMenu.Menu.Item("Potions.Player.Health").GetValue<Slider>().Value)
            {
                if (healthPotion.IsOwned(Entry.Player) && healthPotion.IsReady()
                    && InitializeMenu.Menu.Item("Potions.Health").GetValue<bool>())
                {
                    healthPotion.Cast();
                }

                if (biscuit.IsOwned(Entry.Player) && InitializeMenu.Menu.Item("Potions.Biscuit").GetValue<bool>())
                {
                    biscuit.Cast();
                }
            }

            if (Entry.Player.ManaPercent <= InitializeMenu.Menu.Item("Potions.Player.Mana").GetValue<Slider>().Value)
            {
                if (manaPotion.IsOwned(Entry.Player) && InitializeMenu.Menu.Item("Potions.Mana").GetValue<bool>())
                {
                    manaPotion.Cast();
                }
            }

            if (Entry.Player.HealthPercent <= InitializeMenu.Menu.Item("Potions.Player.Health").GetValue<Slider>().Value
                && Entry.Player.ManaPercent <= InitializeMenu.Menu.Item("Potions.Player.Mana").GetValue<Slider>().Value
                || ObjectManager.Player.HealthPercent
                <= (InitializeMenu.Menu.Item("Potions.Player.Health").GetValue<Slider>().Value / 2)
                || ObjectManager.Player.ManaPercent
                <= (InitializeMenu.Menu.Item("Potions.Player.Mana").GetValue<Slider>().Value / 2)
                && InitializeMenu.Menu.Item("Potions.Flask").GetValue<bool>())
            {
                if (crystallineFlask.IsOwned(Entry.Player))
                {
                    crystallineFlask.Cast();
                }
            }
        }

        #endregion
    }
}