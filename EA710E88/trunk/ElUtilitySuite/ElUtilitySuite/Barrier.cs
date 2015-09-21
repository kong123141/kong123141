namespace ElUtilitySuite
{
    using System;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    public class Barrier
    {
        #region Static Fields

        public static Spell barrierSpell;

        private static SpellDataInst slot1;

        private static SpellDataInst slot2;

        private static SpellSlot summonerBarrier;

        #endregion

        #region Public Methods and Operators

        public static void Load()
        {
            try
            {
                slot1 = Entry.Player.Spellbook.GetSpell(SpellSlot.Summoner1);
                slot2 = Entry.Player.Spellbook.GetSpell(SpellSlot.Summoner2);

                //Soon riot will introduce multiple heals, mark my words.
                var barrierNames = new[] { "summonerbarrier" };

                if (barrierNames.Contains(slot1.Name))
                {
                    barrierSpell = new Spell(SpellSlot.Summoner1);
                    summonerBarrier = SpellSlot.Summoner1;
                }
                else if (barrierNames.Contains(slot2.Name))
                {
                    barrierSpell = new Spell(SpellSlot.Summoner2);
                    summonerBarrier = SpellSlot.Summoner2;
                }
                else
                {
                    Console.WriteLine("You don't have barrier faggot");
                    return;
                }

                Game.PrintChat("<font color='#CC0000'>Sorry!</font> Barrier is not supported yet");
                //Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
                //DamagePrediction.OnTargettedSpellWillKill += DamagePrediction_OnTargettedSpellWillKill;
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
            if (InitializeMenu.Menu.Item("Barrier.Activated").GetValue<bool>())
            {
                if (!Entry.Player.InFountain() || !Entry.Player.IsRecalling())
                {
                    Entry.Player.Spellbook.CastSpell(summonerBarrier);
                }
            }
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!(sender is Obj_AI_Hero) || !(args.Target is Obj_AI_Hero) || sender.IsAlly)
            {
            }
        }

        #endregion
    }
}