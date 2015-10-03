namespace ElUtilitySuite
{
    using System;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    public class Heal
    {
        #region Static Fields

        public static Obj_AI_Hero AggroTarget;

        public static Obj_AI_Hero Attacker;

        public static bool Stealth;

        public static SpellSlot summonerHeal;

        private static Spell healSpell;

        private static float incomingDamage, minionDamage;

        private static SpellDataInst slot1;

        private static SpellDataInst slot2;

        #endregion

        #region Public Methods and Operators

        public static Obj_AI_Hero GetEnemy(string championname)
        {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .First(enemy => enemy.Team != Entry.Player.Team && enemy.ChampionName == championname);
        }

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

        private static void CheckHeal(float incdmg = 0)
        {
            var heal = Entry.Player.GetSpellSlot("summonerheal");
            if (heal == SpellSlot.Unknown)
            {
                return;
            }

            if (!InitializeMenu.Menu.Item("Heal.Activated").GetValue<bool>())
            {
                return;
            }

            if (Entry.Player.Spellbook.CanUseSpell(heal) != SpellState.Ready)
            {
                return;
            }

            var target = Entry.Allies();
            var iDamagePercent = (int)((incdmg / Entry.Player.MaxHealth) * 100);

            if (target.Distance(Entry.Player.ServerPosition) <= 700f && Entry.Player.CountEnemiesInRange(1000) > 0)
            {
                var aHealthPercent = (int)((target.Health / target.MaxHealth) * 100);
                if (aHealthPercent <= InitializeMenu.Menu.Item("Heal.HP").GetValue<Slider>().Value
                    && InitializeMenu.Menu.Item("healon" + target.ChampionName).GetValue<bool>()
                    && !Entry.Player.IsRecalling() && !Entry.Player.InFountain())
                {
                    if ((iDamagePercent >= 1 || incdmg >= target.Health) && AggroTarget.NetworkId == target.NetworkId)
                    {
                        Entry.Player.Spellbook.CastSpell(summonerHeal);
                    }
                }

                else if (iDamagePercent >= InitializeMenu.Menu.Item("Heal.Damage").GetValue<Slider>().Value
                         && InitializeMenu.Menu.Item("healon" + target.ChampionName).GetValue<bool>()
                         && AggroTarget.NetworkId == target.NetworkId && !Entry.Player.IsRecalling()
                         && !Entry.Player.InFountain())
                {
                    Entry.Player.Spellbook.CastSpell(summonerHeal);
                }
            }
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.Type == GameObjectType.obj_AI_Hero && sender.IsEnemy)
            {
                var heroSender = ObjectManager.Get<Obj_AI_Hero>().First(x => x.NetworkId == sender.NetworkId);
                if (heroSender.GetSpellSlot(args.SData.Name) == SpellSlot.Unknown
                    && args.Target.Type == Entry.Player.Type)
                {
                    AggroTarget = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(args.Target.NetworkId);
                    incomingDamage = (float)heroSender.GetAutoAttackDamage(AggroTarget);
                }

                if (heroSender.ChampionName == "Jinx" && args.SData.Name.Contains("JinxQAttack")
                    && args.Target.Type == Entry.Player.Type)
                {
                    AggroTarget = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(args.Target.NetworkId);
                    incomingDamage = (float)heroSender.GetAutoAttackDamage(AggroTarget);
                }
            }

            if (sender.Type == GameObjectType.obj_AI_Minion && sender.IsEnemy)
            {
                if (args.Target.NetworkId == Entry.Player.NetworkId)
                {
                    AggroTarget = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(args.Target.NetworkId);

                    minionDamage =
                        (float)
                        sender.CalcDamage(
                            AggroTarget,
                            Damage.DamageType.Physical,
                            sender.BaseAttackDamage + sender.FlatPhysicalDamageMod);
                }
            }

            if (sender.Type == GameObjectType.obj_AI_Turret && sender.IsEnemy)
            {
                if (args.Target.Type == Entry.Player.Type)
                {
                    if (sender.Distance(Entry.Allies().ServerPosition, true) <= 900 * 900)
                    {
                        AggroTarget = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(args.Target.NetworkId);

                        incomingDamage =
                            (float)
                            sender.CalcDamage(
                                AggroTarget,
                                Damage.DamageType.Physical,
                                sender.BaseAttackDamage + sender.FlatPhysicalDamageMod);
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
                CheckHeal(incomingDamage);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        #endregion
    }
}