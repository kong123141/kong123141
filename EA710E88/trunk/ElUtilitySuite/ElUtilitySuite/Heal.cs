namespace ElUtilitySuite
{
    using System;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    public class Heal
    {
        #region Static Fields

        public static SpellSlot summonerHeal;

        private static Spell healSpell;

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

                //Soon riot will introduce multiple heals, mark my words.
                var healNames = new[] { "summonerheal" };

                if (healNames.Contains(slot1.Name))
                {
                    healSpell = new Spell(SpellSlot.Summoner1, 550f);
                    summonerHeal = SpellSlot.Summoner1;
                }
                else if (healNames.Contains(slot2.Name))
                {
                    healSpell = new Spell(SpellSlot.Summoner2, 550f);
                    summonerHeal = SpellSlot.Summoner2;
                }
                else
                {
                    Console.WriteLine("You don't have heal faggot");
                    return;
                }

                DamagePrediction.OnTargettedSpellWillKill += DamagePrediction_OnTargettedSpellWillKill;
                Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
                Game.OnUpdate += OnUpdate;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        #endregion

        #region Methods

        private static void DamagePrediction_OnTargettedSpellWillKill(
            Obj_AI_Hero sender,
            Obj_AI_Hero target,
            SpellData sData)
        {
            var targetName = target.ChampionName;
            if (sender.IsAlly || Entry.Player.IsDead)
            {
                return;
            }

            /* if (targetName != Entry.Player.ChampionName && InitializeMenu.Menu.Item("Heal." + Entry.Player.ChampionName.ToLowerInvariant() + ".noult." + targetName).GetValue<bool>())
            {
                return;
            }*/

            if (InitializeMenu.Menu.Item("Heal.Activated").GetValue<bool>()
                && InitializeMenu.Menu.Item("Heal.Predicted").GetValue<bool>())
            {
                if (!Entry.Player.InFountain() || !Entry.Player.IsRecalling())
                {
                    Console.WriteLine("HEAL");
                    Entry.Player.Spellbook.CastSpell(summonerHeal);
                }
            }
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!(sender is Obj_AI_Hero) || !(args.Target is Obj_AI_Hero) || Entry.Player.IsDead)
            {
                return;
            }

            if (sender.IsEnemy)
            {
                if (InitializeMenu.Menu.Item("Heal.Activated").GetValue<bool>()
                    && !InitializeMenu.Menu.Item("Heal.Predicted").GetValue<bool>())
                {
                    if (Entry.Player.Health / Entry.Player.MaxHealth * 100
                        <= InitializeMenu.Menu.Item("Heal.HP").GetValue<Slider>().Value)
                    {
                        if (!Entry.Player.InFountain() || !Entry.Player.IsRecalling())
                        {
                            Entry.Player.Spellbook.CastSpell(summonerHeal);
                        }
                    }
                }
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
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        #endregion
    }
}