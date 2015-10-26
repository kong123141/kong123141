using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace 花边_花式多合一.Core
{
    class Activators
    {
        public static damageHandler dHandler;
        public static SpellSlot CleanseSpellSlot = ObjectManager.Player.GetSpellSlot("summonerboost");
        public static SpellSlot HealSpellSlot = ObjectManager.Player.GetSpellSlot("summonerheal");
        public static SpellSlot IgniteSpellSlot = ObjectManager.Player.GetSpellSlot("summonerdot");
        public static SpellSlot BarrierSpellSlot = ObjectManager.Player.GetSpellSlot("summonerbarrier");
        public static SpellSlot ClaritySpellSlot = ObjectManager.Player.GetSpellSlot("summonermana");
        public static SpellSlot ExhaustSpellSlot = ObjectManager.Player.GetSpellSlot("summonerexhaust");
        public static SpellSlot GhostSpellSlot = ObjectManager.Player.GetSpellSlot("summonerhaste");
        public static Obj_AI_Hero target = TargetSelector.GetTarget(850f, TargetSelector.DamageType.Physical);
        public static Obj_AI_Base banTarget = ObjectManager.Get<Obj_AI_Base>().Where(x => x.IsMinion && x.IsValidTarget(1200f, false) && x.CharData.BaseSkinName.Contains("siege")).FirstOrDefault();
        public static Obj_AI_Base healTarget = ObjectManager.Get<Obj_AI_Base>().Where(x => x.IsValidTarget(850f, false)).FirstOrDefault();

        internal static void Game_OnGameLoad(EventArgs args)
        {

            try
            {
                if (!InitializeMenu.Menu.Item("Autoactivator").GetValue<bool>()) return;

                Game.OnUpdate += Game_OnGameUpdate;
                Orbwalking.AfterAttack += Orbwalking_AfterAttack;
                Obj_AI_Base.OnDoCast += Obj_AI_Base_OnDoCast;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Activators error occurred: '{0}'", ex);
            }
        }

        public static bool IsHealthPotRunning()
        {
            return (ObjectManager.Player.HasBuff("ItemMiniRegenPotion")
                || ObjectManager.Player.HasBuff("ItemCrystalFlask")
                || ObjectManager.Player.HasBuff("RegenerationPotion"));
        }

        public static bool IsManaPotRunning()
        {
            return (ObjectManager.Player.HasBuff("ItemCrystalFlask")
                || ObjectManager.Player.HasBuff("FlaskOfCrystalWater"));
        }

        public static bool HasNoProtection(Obj_AI_Hero target)
        {
            return (!target.HasBuffOfType(BuffType.SpellShield)
                && !target.HasBuffOfType(BuffType.SpellImmunity));
        }

        public static bool ShouldUseCleanse(Obj_AI_Hero target)
        {
            return (target.HasBuffOfType(BuffType.Charm)
                || target.HasBuffOfType(BuffType.Flee)
                || target.HasBuffOfType(BuffType.Polymorph)
                || target.HasBuffOfType(BuffType.Snare)
                || target.HasBuffOfType(BuffType.Stun)
                || target.HasBuffOfType(BuffType.Taunt)
                || target.HasBuff("summonerexhaust")
                || target.HasBuff("summonerdot"));
        }

        #region 物品 召唤师技能 使用

        public static void UseCleanse()
        {
            Utility.DelayAction.Add(WeightedRandom.Next(100, 200),
                () =>
                {
                    ObjectManager.Player.Spellbook.CastSpell(CleanseSpellSlot, ObjectManager.Player);
                    return;
                }
            );
        }

        public static void UseClarity()
        {
            if (ObjectManager.Player.CountEnemiesInRange(1000) > 0)
            {
                ObjectManager.Player.Spellbook.CastSpell(ClaritySpellSlot, ObjectManager.Player);
                return;
            }
        }

        public static void UseGhost()
        {
            if ((target.CountEnemiesInRange(700) <= ObjectManager.Player.CountAlliesInRange(700)) && (ObjectManager.Player.IsFacing(target) && !target.IsFacing(ObjectManager.Player))
                || (target.CountEnemiesInRange(700) > ObjectManager.Player.CountAlliesInRange(700)) && (!ObjectManager.Player.IsFacing(target) && target.IsFacing(ObjectManager.Player)))
            {
                ObjectManager.Player.Spellbook.CastSpell(GhostSpellSlot, ObjectManager.Player);
                return;
            }
        }

        public static void UseIgnite()
        {
            if (target.IsValidTarget(600f))
            {
                ObjectManager.Player.Spellbook.CastSpell(IgniteSpellSlot, target);
                return;
            }
        }

        public static void UseDervishBlade()
        {
            Utility.DelayAction.Add(ObjectManager.Player.HasBuff("zedulttargetmark") ? 1500 : WeightedRandom.Next(100, 200),
                () =>
                {
                    Items.UseItem(3137, ObjectManager.Player);
                    return;
                }
            );
        }

        public static void UseMercurialScimitar()
        {
            Utility.DelayAction.Add(ObjectManager.Player.HasBuff("zedulttargetmark") ? 1500 : WeightedRandom.Next(100, 200),
                () =>
                {
                    Items.UseItem(3139, ObjectManager.Player);
                    return;
                }
            );
        }

        public static void UseQuicksilverSash()
        {
            Utility.DelayAction.Add(ObjectManager.Player.HasBuff("zedulttargetmark") ? 1500 : WeightedRandom.Next(100, 200),
                () =>
                {
                    Items.UseItem(3140, ObjectManager.Player);
                    return;
                }
            );
        }

        public static void UseMikaelsCrucible()
        {
            foreach (var Ally in ObjectManager.Get<Obj_AI_Hero>()
                .Where(h => h.IsValidTarget(750f, false)))
            {
                if (ShouldUseCleanse(Ally)
                    && HasNoProtection(Ally))
                {
                    Utility.DelayAction.Add(WeightedRandom.Next(100, 200),
                        () =>
                        {
                            Items.UseItem(3222, Ally);
                            return;
                        }
                    );
                }
            }

            if (ShouldUseCleanse(ObjectManager.Player)
                && HasNoProtection(ObjectManager.Player))
            {
                Utility.DelayAction.Add(WeightedRandom.Next(100, 200),
                    () =>
                    {
                        Items.UseItem(3222, ObjectManager.Player);
                        return;
                    }
                );
            }
        }

        public static bool ShouldUseCleanser()
        {
            return (ObjectManager.Player.HasBuff("zedulttargetmark")
                || ObjectManager.Player.HasBuff("VladimirHemoplague")
                || ObjectManager.Player.HasBuff("MordekaiserChildrenOfTheGrave")
                || ObjectManager.Player.HasBuff("PoppyDiplomaticImmunity")
                || ObjectManager.Player.HasBuff("FizzMarinerDoom")
                || ObjectManager.Player.HasBuffOfType(BuffType.Suppression));
        }

        public static bool IsCleanseAvailable()
        {
            return (!(CleanseSpellSlot == SpellSlot.Unknown)
                && ObjectManager.Player.Spellbook.CanUseSpell(CleanseSpellSlot) == SpellState.Ready);
        }

        public static bool IsClarityAvailable()
        {
            return (!(ClaritySpellSlot == SpellSlot.Unknown)
                && ObjectManager.Player.Spellbook.CanUseSpell(ClaritySpellSlot) == SpellState.Ready);
        }

        public static bool IsGhostAvailable()
        {
            return (!(GhostSpellSlot == SpellSlot.Unknown)
                && ObjectManager.Player.Spellbook.CanUseSpell(GhostSpellSlot) == SpellState.Ready);
        }

        public static bool IsIgniteAvailable()
        {
            return (!(IgniteSpellSlot == SpellSlot.Unknown)
                && ObjectManager.Player.Spellbook.CanUseSpell(IgniteSpellSlot) == SpellState.Ready);
        }

        public static void UseFlask()
        {
            if ((ObjectManager.Player.HealthPercent <= InitializeMenu.Menu.Item("nabbactivator.menu.potions.on_health_percent").GetValue<Slider>().Value
                && ObjectManager.Player.ManaPercent <= InitializeMenu.Menu.Item("nabbactivator.menu.potions.on_mana_percent").GetValue<Slider>().Value)
                || ObjectManager.Player.HealthPercent <= (InitializeMenu.Menu.Item("nabbactivator.menu.potions.on_health_percent").GetValue<Slider>().Value / 2)
                || ObjectManager.Player.ManaPercent <= (InitializeMenu.Menu.Item("nabbactivator.menu.potions.on_mana_percent").GetValue<Slider>().Value / 2))
            {
                Items.UseItem(2041);
                return;
            }
        }

        public static void UseHealthPotions()
        {
            if (ObjectManager.Player.HealthPercent <= InitializeMenu.Menu.Item("nabbactivator.menu.potions.on_health_percent").GetValue<Slider>().Value)
            {
                Items.UseItem(2003);
            }
        }

        public static void UseBiscuits()
        {
            if (ObjectManager.Player.HealthPercent <= InitializeMenu.Menu.Item("nabbactivator.menu.potions.on_health_percent").GetValue<Slider>().Value)
            {
                Items.UseItem(2010);
            }
        }

        public static void UseManaPotions()
        {
            if (ObjectManager.Player.ManaPercent <= InitializeMenu.Menu.Item("nabbactivator.menu.potions.on_mana_percent").GetValue<Slider>().Value)
            {
                Items.UseItem(2004);
            }
        }

        public static void UseMuramana()
        {
            if (!ObjectManager.Player.HasBuff("muramana") && ObjectManager.Player.IsWindingUp
                || ObjectManager.Player.HasBuff("muramana") && !ObjectManager.Player.IsWindingUp)
            {
                Items.UseItem(3042, ObjectManager.Player);
            }
        }

        public static void UseGuardiansHorn()
        {
            // Engage / Disengage
            if ((target.CountEnemiesInRange(700) < ObjectManager.Player.CountAlliesInRange(700)) && (ObjectManager.Player.IsFacing(target) && !target.IsFacing(ObjectManager.Player))
                || (target.CountEnemiesInRange(700) > ObjectManager.Player.CountAlliesInRange(700)) && (!ObjectManager.Player.IsFacing(target) && target.IsFacing(ObjectManager.Player)))
            {
                Items.UseItem(2051, ObjectManager.Player);
            }
        }

        public static void UseTwinShadows()
        {
            // Engage / Disengage
            if ((target.CountEnemiesInRange(700) < ObjectManager.Player.CountAlliesInRange(700)) && (ObjectManager.Player.IsFacing(target) && !target.IsFacing(ObjectManager.Player))
                || (target.CountEnemiesInRange(700) > ObjectManager.Player.CountAlliesInRange(700)) && (!ObjectManager.Player.IsFacing(target) && target.IsFacing(ObjectManager.Player)))
            {
                Items.UseItem(3023, ObjectManager.Player);
            }
        }

        public static void UseSeraphsEmbrace()
        {
            if (ObjectManager.Player.HealthPercent <= 55 && (target.CountEnemiesInRange(700) < ObjectManager.Player.CountAlliesInRange(700)))
            {
                Items.UseItem(3040, ObjectManager.Player);
            }
        }

        public static void UseBannerofCommand()
        {
            if (banTarget != null)
            {
                Items.UseItem(3060, banTarget);
            }
        }

        public static void UseTalismanofAscension()
        {
            // Engage / Disengage
            if ((target.CountEnemiesInRange(600) < ObjectManager.Player.CountAlliesInRange(600)) && (ObjectManager.Player.IsFacing(target) && !target.IsFacing(ObjectManager.Player))
                || (target.CountEnemiesInRange(600) > ObjectManager.Player.CountAlliesInRange(600)) && (!ObjectManager.Player.IsFacing(target) && target.IsFacing(ObjectManager.Player)))
            {
                Items.UseItem(3069, ObjectManager.Player);
            }
        }

        public static void UseWoogletsWitchcap()
        {
            if (dHandler.GetPredictedDamage(ObjectManager.Player) <= 0)
            {
                return;
            }

            if (dHandler.GetPredictedDamage(ObjectManager.Player) > ObjectManager.Player.Health)
            {
                if (dHandler.GetFirstAttackTime() <= (150 + Game.Ping))
                {
                    Items.UseItem(3090, ObjectManager.Player);
                }
            }
        }

        public static void UseRanduinsOmen()
        {
            // Engage / Disengage
            if (((target.CountEnemiesInRange(700) < ObjectManager.Player.CountAlliesInRange(700))
                || (target.CountEnemiesInRange(700) > ObjectManager.Player.CountAlliesInRange(700)))
                && ObjectManager.Player.CountEnemiesInRange(400f) > 1)
            {
                Items.UseItem(3143, ObjectManager.Player);
            }
        }

        public static void UseZhonyasHourglass()
        {
            if (dHandler.GetPredictedDamage(ObjectManager.Player) <= 0)
            {
                return;
            }

            if (dHandler.GetPredictedDamage(ObjectManager.Player) > ObjectManager.Player.Health)
            {
                if (dHandler.GetFirstAttackTime() <= (150 + Game.Ping))
                {
                    Items.UseItem(3157, ObjectManager.Player);
                }
            }
        }

        public static void UseLocketoftheIronSolari()
        {
            if (ObjectManager.Player.CountAlliesInRange(600) > 2 && ObjectManager.Player.CountEnemiesInRange(600) > 3)
            {
                Items.UseItem(3190, ObjectManager.Player);
            }
        }

        public static void UseRighteousGlory()
        {
            // Engage / Disengage
            if ((target.CountEnemiesInRange(500) < ObjectManager.Player.CountAlliesInRange(500)) && (ObjectManager.Player.IsFacing(target) && !target.IsFacing(ObjectManager.Player))
                || (target.CountEnemiesInRange(500) > ObjectManager.Player.CountAlliesInRange(500)) && (!ObjectManager.Player.IsFacing(target) && target.IsFacing(ObjectManager.Player)))
            {
                Items.UseItem(3800, ObjectManager.Player);
            }
        }

        public static void UseFrostQueensClaim()
        {
            if (target.IsValidTarget())
            {
                // Engage / Disengage
                if ((target.CountEnemiesInRange(700) < ObjectManager.Player.CountAlliesInRange(700)) && (ObjectManager.Player.IsFacing(target) && !target.IsFacing(ObjectManager.Player))
                    || (target.CountEnemiesInRange(700) > ObjectManager.Player.CountAlliesInRange(700)) && (!ObjectManager.Player.IsFacing(target) && target.IsFacing(ObjectManager.Player)))
                {
                    Items.UseItem(3092, target);
                }
            }
        }

        public static void UseYoumuusGhostblade()
        {
            if (target.IsValidTarget() && ObjectManager.Player.IsWindingUp)
            {
                Items.UseItem(3142, target);
            }
        }

        public static void UseBilgewatersCutlass()
        {
            if (target.IsValidTarget())
            {
                Items.UseItem(3144, target);
            }
        }

        public static void UseHextechGunblade()
        {
            if (target.IsValidTarget())
            {
                Items.UseItem(3146, target);
            }
        }

        public static void UseBladeoftheRuinedKing()
        {
            if (target.IsValidTarget()
                && (ObjectManager.Player.HealthPercent <= 90 || (ObjectManager.Player.IsFacing(target) && !target.IsFacing(ObjectManager.Player))))
            {
                Items.UseItem(3153, target);
            }
        }

        public static void UseEntropy()
        {
            if (ObjectManager.Player.IsWindingUp)
            {
                Items.UseItem(3184, ObjectManager.Player);
            }
        }

        public static void UseOdynsVeil()
        {
            if (dHandler.GetPredictedDamage(ObjectManager.Player) <= 0)
            {
                return;
            }

            if (dHandler.GetPredictedDamage(ObjectManager.Player) > 200)
            {
                if (dHandler.GetFirstAttackTime() <= (150 + Game.Ping))
                {
                    Items.UseItem(3040, ObjectManager.Player);
                }
            }
        }

        #endregion

        private static void Obj_AI_Base_OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }

            if ((sender is Obj_AI_Turret) && (args.Target is Obj_AI_Hero) && args.Target.IsAlly)
            {
                StartOnDoCastLogic(sender);
            }
        }

        private static void StartOnDoCastLogic(Obj_AI_Base sender)
        {
            if (Items.HasItem(3056) && Items.CanUseItem(3056) && target.IsValidTarget())
            {
                Items.UseItem(3056, target);
            }
        }

        private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            StartAfterAttackLogic();
        }

        private static void StartAfterAttackLogic()
        {
            if (Items.HasItem(3074) && Items.CanUseItem(3074) && ObjectManager.Player.CountEnemiesInRange(400f) > 0)
            {
                Items.UseItem(3074, ObjectManager.Player);
            }

            if (Items.HasItem(3077) && Items.CanUseItem(3077) && ObjectManager.Player.CountEnemiesInRange(400f) > 0)
            {
                Items.UseItem(3077, ObjectManager.Player);
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }

            StartDefensiveLogic();
            StartCleanserLogic();
            StartSpellLogic();

            if (!(ObjectManager.Player.IsRecalling() || ObjectManager.Player.InFountain()))
            {
                StartPotionLogic();
            }

            if (InitializeMenu.Menu.Item("nabbactivator.menu.combo_button").GetValue<KeyBind>().Active)
            {
                StartOffensiveLogic();
            }
        }

        private static void StartOffensiveLogic()
        {
            if (Items.HasItem(3042) && Items.CanUseItem(3042))
            {
                UseMuramana();
            }

            if (Items.HasItem(3092) && Items.CanUseItem(3092))
            {
                UseFrostQueensClaim();
            }

            if (Items.HasItem(3142) && Items.CanUseItem(3142))
            {
                UseYoumuusGhostblade();
            }

            if (Items.HasItem(3144) && Items.CanUseItem(3144))
            {
                UseBilgewatersCutlass();
            }

            if (Items.HasItem(3146) && Items.CanUseItem(3146))
            {
                UseHextechGunblade();
            }

            if (Items.HasItem(3153) && Items.CanUseItem(3153))
            {
                UseBladeoftheRuinedKing();
            }

            if (Items.HasItem(3184) && Items.CanUseItem(3184))
            {
                UseEntropy();
            }

            if (Items.HasItem(3180) && Items.CanUseItem(3180))
            {
                UseOdynsVeil();
            }
        }

        private static void StartPotionLogic()
        {
            if (!IsHealthPotRunning())
            {
                if (Items.HasItem(2041) && Items.CanUseItem(2041))
                {
                    UseFlask();
                }

                if (Items.HasItem(2003) && Items.CanUseItem(2003))
                {
                    UseHealthPotions();
                }

                if (Items.HasItem(2010) && Items.CanUseItem(2010))
                {
                    UseBiscuits();
                }
            }

            if (!IsManaPotRunning())
            {
                if (Items.HasItem(2004) && Items.CanUseItem(2004))
                {
                    UseManaPotions();
                }
            }

        }

        private static void StartSpellLogic()
        {
            // Cleanse
            if (IsCleanseAvailable()
                && ShouldUseCleanse(ObjectManager.Player))
            {
                UseCleanse();
            }

            // Clarity
            if (IsClarityAvailable()
                && ObjectManager.Player.ManaPercent <= 40)
            {
                UseClarity();
            }

            // Ghost
            if (IsGhostAvailable())
            {
                UseGhost();
            }

            // Ignite
            if (IsIgniteAvailable())
            {
                if (target.Health < (float)ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite))
                {
                    UseIgnite();
                }
            }
        }

        private static void StartCleanserLogic()
        {
            if (ShouldUseCleanse(ObjectManager.Player))
            {
                if (!IsCleanseAvailable()
                    || ShouldUseCleanser())
                {
                    if (Items.HasItem(3222) && Items.CanUseItem(3222))
                    {
                        UseMikaelsCrucible();
                    }

                    if (Items.HasItem(3137) && Items.CanUseItem(3137))
                    {
                        UseDervishBlade();
                    }

                    if (Items.HasItem(3139) && Items.CanUseItem(3139))
                    {
                        UseMercurialScimitar();
                    }

                    if (Items.HasItem(3140) && Items.CanUseItem(3140))
                    {
                        UseQuicksilverSash();
                    }
                }
            }

        }

        private static void StartDefensiveLogic()
        {
            if (Items.HasItem(2051) && Items.CanUseItem(2051))
            {
                UseGuardiansHorn();
            }

            if ((Items.HasItem(3023) && Items.CanUseItem(3023))
                || (Items.HasItem(3290) && Items.CanUseItem(3290)))
            {
                UseTwinShadows();
            }

            if ((Items.HasItem(3040) && Items.CanUseItem(3040))
                || (Items.HasItem(3048) && Items.CanUseItem(3048)))
            {
                UseSeraphsEmbrace();
            }

            if (Items.HasItem(3060) && Items.CanUseItem(3060))
            {
                UseBannerofCommand();
            }

            if (Items.HasItem(3069) && Items.CanUseItem(3069))
            {
                UseTalismanofAscension();
            }

            if (Items.HasItem(3090) && Items.CanUseItem(3090))
            {
                UseWoogletsWitchcap();
            }

            if (Items.HasItem(3143) && Items.CanUseItem(3143))
            {
                UseRanduinsOmen();
            }

            if (Items.HasItem(3157) && Items.CanUseItem(3157))
            {
                UseZhonyasHourglass();
            }

            if (Items.HasItem(3190) && Items.CanUseItem(3190))
            {
                UseLocketoftheIronSolari();
            }

            if (Items.HasItem(3800) && Items.CanUseItem(3800))
            {
                UseRighteousGlory();
            }
        }
    }
}
