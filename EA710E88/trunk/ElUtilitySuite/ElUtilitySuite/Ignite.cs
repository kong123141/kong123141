namespace ElUtilitySuite
{
    using System;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    // ReSharper disable once ClassNeverInstantiated.Global
    public static class Ignite
    {
        #region Static Fields

        public static Spell igniteSpell;

        public static SpellSlot summonerDot;

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

                //Soon riot will introduce multiple ignote, mark my words.
                var igniteNames = new[] { "summonerdot" };

                if (igniteNames.Contains(slot1.Name))
                {
                    igniteSpell = new Spell(SpellSlot.Summoner1);
                    summonerDot = SpellSlot.Summoner1;
                }
                else if (igniteNames.Contains(slot2.Name))
                {
                    igniteSpell = new Spell(SpellSlot.Summoner2);
                    summonerDot = SpellSlot.Summoner2;
                }
                else
                {
                    Console.WriteLine("You don't have ignite faggot");
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

        private static bool IgniteCheck(this Obj_AI_Base hero)
        {
            if (InitializeMenu.Menu.Item("Ignite.shieldCheck").GetValue<bool>())
            {
                return !hero.HasBuff("summonerdot") || !hero.HasBuff("summonerbarrier") || !hero.HasBuff("BlackShield")
                       || !hero.HasBuff("SivirShield") || !hero.HasBuff("BansheesVeil")
                       || !hero.HasBuff("ShroudofDarkness");
            }

            return true;
        }

        private static void IgniteKs()
        {
            if (!InitializeMenu.Menu.Item("Ignite.Activated").GetValue<bool>())
            {
                return;
            }

            var kSableEnemy =
                HeroManager.Enemies.FirstOrDefault(
                    hero =>
                    hero.IsValidTarget(550) && IgniteCheck(hero) && !hero.IsZombie
                    && Entry.Player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite) >= hero.Health);

            if (kSableEnemy != null)
            {
                Entry.Player.Spellbook.CastSpell(summonerDot, kSableEnemy);
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            if (Entry.Player.IsDead)
            {
                return;
            }

            try
            {
                IgniteKs();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        #endregion
    }
}