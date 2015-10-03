namespace ElUtilitySuite
{
    using System;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    internal static class Defensive
    {
        #region Static Fields

        public static Obj_AI_Hero AggroTarget;

        private static float incomingDamage, minionDamage;

        #endregion

        #region Public Methods and Operators

        public static bool IsValidState(this Obj_AI_Hero target)
        {
            return !target.HasBuffOfType(BuffType.SpellShield) && !target.HasBuffOfType(BuffType.SpellImmunity)
                   && !target.HasBuffOfType(BuffType.Invulnerability);
        }

        public static void Load()
        {
            try
            {
                Game.OnUpdate += OnUpdate;
                Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        #endregion

        #region Methods

        private static int CountHerosInRange(this Obj_AI_Hero target, bool checkteam, float range = 1200f)
        {
            var objListTeam = ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(range, false));

            return objListTeam.Count(hero => checkteam ? hero.Team != target.Team : hero.Team == target.Team);
        }

        private static void DefensiveItemManager()
        {
            if (InitializeMenu.Menu.Item("usecombo").GetValue<KeyBind>().Active)
            {
                UseItemCount("Randuins", 3143, 450f);
            }

            UseItem("allyshieldlocket", "Locket", 3190, 600f);
            UseItem("allyshieldmountain", "Mountain", 3401, 700f);
            UseItem("selfshieldseraph", "Seraphs", 3040);

            if (Items.HasItem(3069) && Items.CanUseItem(3069)
                && InitializeMenu.Menu.Item("useTalisman").GetValue<bool>())
            {
                if (!InitializeMenu.Menu.Item("usecombo").GetValue<KeyBind>().Active
                    && InitializeMenu.Menu.Item("talismanMode").GetValue<StringList>().SelectedIndex == 1)
                {
                    return;
                }

                var target = Entry.Allies();
                if (target.Distance(Entry.Player.ServerPosition, true) > 600 * 600)
                {
                    return;
                }

                var lowTarget =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .OrderBy(ex => ex.Health / ex.MaxHealth * 100)
                        .First(x => x.IsValidTarget(1000));

                var aHealthPercent = target.Health / target.MaxHealth * 100;
                var eHealthPercent = lowTarget.Health / lowTarget.MaxHealth * 100;

                if (lowTarget.Distance(target.ServerPosition, true) <= 900 * 900
                    && (target.CountHerosInRange(false) > target.CountHerosInRange(true)
                        && eHealthPercent <= InitializeMenu.Menu.Item("useEnemyPct").GetValue<Slider>().Value))
                {
                    Items.UseItem(3069);
                }

                if (target.CountHerosInRange(false) > target.CountHerosInRange(true)
                    && aHealthPercent <= InitializeMenu.Menu.Item("useAllyPct").GetValue<Slider>().Value)
                {
                    Items.UseItem(3069);
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
                DefensiveItemManager();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        private static void UseItem(string menuvar, string name, int itemId, float itemRange = float.MaxValue)
        {
            if (!Items.HasItem(itemId) || !Items.CanUseItem(itemId))
            {
                return;
            }

            if (!InitializeMenu.Menu.Item("use" + name).GetValue<bool>())
            {
                return;
            }

            var target = itemRange > 5000 ? Entry.Player : Entry.Allies();
            if (target.Distance(Entry.Player.ServerPosition, true) > itemRange * itemRange || !target.IsValidState())
            {
                return;
            }

            var aHealthPercent = (int)((target.Health / target.MaxHealth) * 100);
            var iDamagePercent = (int)(incomingDamage / target.MaxHealth * 100);

            if (!InitializeMenu.Menu.Item("DefenseOn" + target.SkinName).GetValue<bool>())
            {
                return;
            }

            if (menuvar.Contains("shield"))
            {
                if (aHealthPercent <= InitializeMenu.Menu.Item("use" + name + "Pct").GetValue<Slider>().Value)
                {
                    if ((iDamagePercent >= 1 || incomingDamage >= target.Health))
                    {
                        if (AggroTarget.NetworkId == target.NetworkId)
                        {
                            Items.UseItem(itemId, target);
                        }
                    }

                    if (iDamagePercent >= InitializeMenu.Menu.Item("use" + name + "Dmg").GetValue<Slider>().Value)
                    {
                        if (AggroTarget.NetworkId == target.NetworkId)
                        {
                            Items.UseItem(itemId, target);
                        }
                    }
                }
            }
        }

        private static void UseItemCount(string name, int itemId, float itemRange)
        {
            if (!Items.HasItem(itemId) || !Items.CanUseItem(itemId))
            {
                return;
            }

            if (InitializeMenu.Menu.Item("use" + name).GetValue<bool>())
            {
                if (Entry.Player.CountHerosInRange(true, itemRange)
                    >= InitializeMenu.Menu.Item("use" + name + "Count").GetValue<Slider>().Value)
                {
                    Items.UseItem(itemId);
                }
            }
        }

        #endregion
    }
}