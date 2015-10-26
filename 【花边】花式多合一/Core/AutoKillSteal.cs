using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace 花边_花式多合一.Core
{
    internal static class AutoKillSteal
    {
        public static Spell Ignite;
        public static Spell Smite;
        public static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        internal static void Game_OnGameLoad(EventArgs args)
        {
            try
            {
                if (!InitializeMenu.Menu.Item("AutoKs").GetValue<bool>()) return;

                Game.OnUpdate += Game_OnUpdate;
                var igniteSlot = Player.GetSpellSlot("summonerdot");

                if (igniteSlot.IsValid())
                {
                    Ignite = new Spell(igniteSlot, 600);
                    Ignite.SetTargetted(.172f, 20);
                }

                var bluesmiteSlot = Player.GetSpellSlot("s5_summonersmiteplayerganker");

                if (bluesmiteSlot.IsValid())
                {
                    Smite = new Spell(bluesmiteSlot, 760);
                    Smite.SetTargetted(.333f, 20);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("AutoKillsteal error occurred: '{0}'", ex);
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            if (InitializeMenu.Menu.Item("AA").GetValue<bool>() && AutoAttack())
            {
                return;
            }

            if (InitializeMenu.Menu.Item("Smite").GetValue<bool>())
            {
                CastSmite();
            }

            if (InitializeMenu.Menu.Item("Ignite").GetValue<bool>() && CastIgnite())
            {
                return;
            }
        }

        private static bool AutoAttack()
        {
            if (!Player.CanAttack || Player.IsChannelingImportantSpell() || Player.Spellbook.IsAutoAttacking)
            {
                return false;
            }

            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        h =>
                            h.IsValidTarget(Player.GetAutoAttackRange()) &&
                            h.Health < Player.GetAutoAttackDamage(h, true))
                    .Any(enemy => Player.IssueOrder(GameObjectOrder.AutoAttack, enemy));
        }

        private static double SmiteChampDamage()
        {
            if (AutoSmite.smite.Slot == Player.GetSpellSlot("s5_summonersmiteduel"))
            {
                var damage = new[] { 54 + 6 * Player.Level };
                return Player.Spellbook.CanUseSpell(AutoSmite.smite.Slot) == SpellState.Ready ? damage.Max() : 0;
            }

            if (AutoSmite.smite.Slot == Player.GetSpellSlot("s5_summonersmiteplayerganker"))
            {
                var damage = new[] { 20 + 8 * Player.Level };
                return Player.Spellbook.CanUseSpell(AutoSmite.smite.Slot) == SpellState.Ready ? damage.Max() : 0;
            }

            return 0;
        }

        private static void CastSmite()
        {

            var kSableEnemy =
                HeroManager.Enemies.FirstOrDefault(hero => hero.IsValidTarget(550) && SmiteChampDamage() >= hero.Health);
            if (kSableEnemy != null)
            {
                Player.Spellbook.CastSpell(AutoSmite.smite.Slot, kSableEnemy);
            }
        }

        private static bool CastIgnite()
        {
            if (Ignite == null || !Ignite.IsReady())
            {
                return false;
            }

            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        h =>
                            h.IsValidTarget(Ignite.Range) &&
                            h.Health < Player.GetSummonerSpellDamage(h, Damage.SummonerSpell.Ignite))
                    .Any(enemy => Ignite.Cast(enemy).IsCasted());
        }

        public static bool IsValid(this SpellSlot spell)
        {
            return spell != SpellSlot.Unknown;
        }

        public static bool IsCastableOnChampion(this Spell spell)
        {
            var name = spell.Instance.Name.ToLower();
            return name != "summonersmite" && name != "s5_summonersmitequick" && name != "itemsmiteaoe";
        }

        public static float GetAutoAttackRange(this Obj_AI_Hero hero)
        {
            return Orbwalking.GetRealAutoAttackRange(hero);
        }
    }
}