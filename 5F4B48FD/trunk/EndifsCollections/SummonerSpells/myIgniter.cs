using System;
using System.Collections.Generic;
using System.Linq;
using EndifsCollections.Controller;
using EndifsCollections.Tools;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace EndifsCollections.SummonerSpells
{
    class myIgniter
    {
        private static SpellSlot IgniteSlot;
        private static Spell Ignite;
        private static Menu tools;
        private static IEnumerable<Obj_AI_Hero> AllEnemies;
        private static Obj_AI_Hero target;

        public myIgniter()
        {
            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
            if (HaveIgnite)
            {
                Ignite = new Spell(IgniteSlot, 600);
                Ignite.SetTargetted(float.MaxValue, float.MaxValue);
            }
            AllEnemies = ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy);
            Game.OnUpdate += OnUpdate;
        }

        public static void AddToMenu(Menu Tools)
        {
            tools = Tools;
            var subs = new Menu("Ignite", "myIgniter");
            {
                subs.AddItem(new MenuItem("mig_auto", "Auto Ignite").SetValue(false));
                foreach (var enemy in HeroManager.Enemies)
                {
                    subs.SubMenu("Champions").AddItem(new MenuItem("mig_target" + enemy.Name, enemy.ChampionName).SetValue(true));
                }
            }
            Tools.AddSubMenu(subs);
        }

        public static void Ignites(Obj_AI_Base target)
        {
            if (target != null && target.IsValidTarget() && IsReady)
            {
                if (target is Obj_AI_Hero && Vector3.Distance(ObjectManager.Player.ServerPosition, target.ServerPosition) <= Ignite.Range)
                {
                    if (target.Health + myUtility.TargetShields(target) <= IgniteDamage)
                    {
                        Ignite.Cast(target);
                    }
                }
            }
        }

        private static bool IsReady
        {
            get
            {
                return HaveIgnite && ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready;
            }
        }

        public static bool HaveIgnite
        {
            get
            {
                return IgniteSlot == SpellSlot.Summoner1 || IgniteSlot == SpellSlot.Summoner2; 
            }
        }

        public static int IgniteDamage
        {
            get
            {
                return 50 + 20 * ObjectManager.Player.Level;
            }
        }
        void OnUpdate(EventArgs args)
        {
            if (HaveIgnite)
            {
                if (tools.Item("mig_auto").GetValue<bool>() && IsReady)
                {
                    target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget(Ignite.Range)
                        ? TargetSelector.GetSelectedTarget()
                        : AllEnemies
                        .Where(x => x.IsValid<Obj_AI_Hero>() && x.IsVisible && Vector3.Distance(ObjectManager.Player.ServerPosition, x.ServerPosition) <= Ignite.Range)
                        .OrderByDescending(i => i.Health)
                        .ThenByDescending(x => myRePriority.ResortDB(x.ChampionName))
                        .FirstOrDefault();
                    if (target != null && target.IsValidTarget() && target.Health + myUtility.TargetShields(target) <= IgniteDamage && tools.Item("mig_target" + target.Name).GetValue<bool>())
                    {
                        Ignite.Cast(target);
                    }
                }
            }
        }
    }
}
