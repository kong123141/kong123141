using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace LickyLicky
{
    internal class IncomingDamage
    {
        public static int WarmingUpStacks = 0, HeatedUpStacks = 0, HeatStacks = 0;

        public static bool StackResetDelay = false;

        public static void Init()
        {
            Obj_AI_Base.OnProcessSpellCast += MinionSpellCast;
            Obj_AI_Base.OnProcessSpellCast += HeroSpellCast;
            Obj_AI_Base.OnProcessSpellCast += TowerSpellCast;
            Obj_AI_Base.OnProcessSpellCast += ChargeOnTowerSpellCast;
        }

        public static bool MinionIsLethal(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            //Console.WriteLine("Damage from Minion: " + sender.GetAutoAttackDamage(ObjectManager.Player));
            return ObjectManager.Player.Health
                   <= sender.CalcDamage(ObjectManager.Player, Damage.DamageType.Physical, sender.BaseAttackDamage);
        }

        public static bool TowerIsLethal(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            //Console.WriteLine("Damage from Tower: " + GetTowerDamage(sender));
            return ObjectManager.Player.Health <= GetTowerDamage(sender);
        }

        public static bool TargetedHeroIsLethal(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            double incDmg;
            var attackerHero = (Obj_AI_Hero)sender;
            SpellSlot spellSlot = attackerHero.GetSpellSlot(args.SData.Name);
            SpellSlot igniteSlot = attackerHero.GetSpellSlot("SummonerDot");

            if (igniteSlot != SpellSlot.Unknown && spellSlot == igniteSlot) incDmg = attackerHero.GetSummonerSpellDamage(ObjectManager.Player, Damage.SummonerSpell.Ignite);
            else if (spellSlot == SpellSlot.Item1 || spellSlot == SpellSlot.Item2 || spellSlot == SpellSlot.Item3
                     || spellSlot == SpellSlot.Item4 || spellSlot == SpellSlot.Item5 || spellSlot == SpellSlot.Item6)
            {
                incDmg = 200f;
                if (args.SData.Name.Contains("Bilgewater")) incDmg = attackerHero.GetItemDamage(ObjectManager.Player, Damage.DamageItems.Bilgewater);
                if (args.SData.Name.Contains("Ruined")) incDmg = attackerHero.GetItemDamage(ObjectManager.Player, Damage.DamageItems.Botrk);
                if (args.SData.Name.Contains("Deathfire")) incDmg = attackerHero.GetItemDamage(ObjectManager.Player, Damage.DamageItems.Dfg);
                if (args.SData.Name.Contains("Hextech")) incDmg = attackerHero.GetItemDamage(ObjectManager.Player, Damage.DamageItems.Hexgun);
                if (args.SData.Name.Contains("Hydra")) incDmg = attackerHero.GetItemDamage(ObjectManager.Player, Damage.DamageItems.Hydra);
                if (args.SData.Name.Contains("Tiamat")) incDmg = attackerHero.GetItemDamage(ObjectManager.Player, Damage.DamageItems.Tiamat);
            }
            else if (spellSlot == SpellSlot.Unknown) incDmg = attackerHero.GetAutoAttackDamage(ObjectManager.Player);
            else incDmg = attackerHero.GetSpellDamage(ObjectManager.Player, spellSlot);

            return ObjectManager.Player.Health <= incDmg;
        }

        public static bool SkillshotHeroIsLethal(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            double incDmg = 200f;
            var attackerHero = (Obj_AI_Hero)sender;
            SpellSlot spellSlot = attackerHero.GetSpellSlot(args.SData.Name);
            incDmg = attackerHero.GetSpellDamage(ObjectManager.Player, spellSlot);
            //if (SkillshotDetector.IsAboutToHit(ObjectManager.Player, 150))
            //{
            //    incDmg = attackerHero.GetSpellDamage(ObjectManager.Player, spellSlot);

            //}
            return ObjectManager.Player.Health <= incDmg;
        }

        public static bool TowerIsOuter(Obj_AI_Base sender)
        {
            return sender.InventoryItems.Any(t => t.DisplayName == "Penetrating Bullets");
        }

        public static bool TowerIsInhib(Obj_AI_Base sender)
        {
            return sender.InventoryItems.Any(t => t.DisplayName == "Lightning Rod");
        }

        public static double GetTowerDamage(Obj_AI_Base sender)
        {
            var towerDamage = sender.CalcDamage(
                ObjectManager.Player,
                Damage.DamageType.Physical,
                sender.BaseAttackDamage);
            if (TowerIsOuter(sender))
            {
                towerDamage = towerDamage * (1 + 0.375f * WarmingUpStacks + 0.25f * HeatedUpStacks);
            }
            else if (TowerIsInhib(sender))
            {
                towerDamage = towerDamage * (1 + 0.0105f * HeatStacks);
            }
            return towerDamage;
        }

        public static void ResetTowerStacks()
        {
            HeatStacks = 0;
            HeatedUpStacks = 0;
            WarmingUpStacks = 0;
        }

        public static void ResetTowerWarming()
        {
            WarmingUpStacks = 0;
            HeatStacks = 0;
        }

        public static void ChargeOnTowerSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (ObjectManager.Player.IsDead || ObjectManager.Player.InFountain())
            {
                return;
            }

            if (sender.IsEnemy && sender.Type == GameObjectType.obj_AI_Turret
                && sender.Distance(ObjectManager.Player) < 2000f)
            {

                if (args.Target.IsMe)
                {
                    if (TowerIsOuter(sender))
                    {
                        if (WarmingUpStacks < 2)
                        {
                            WarmingUpStacks++;
                            //Console.WriteLine("Warming: " + WarmingUpStacks);
                        }
                        else if (HeatedUpStacks < 2)
                        {
                            HeatedUpStacks++;
                            //Console.WriteLine("Heated: " + HeatedUpStacks);
                        }
                    }
                    if (TowerIsInhib(sender))
                    {
                        if (HeatStacks < 120)
                        {
                            HeatStacks = HeatStacks + 6;
                            //Console.WriteLine("Heat: " + HeatStacks);
                        }
                    }

                }
                else if (args.Target.IsAlly && args.Target.Type == GameObjectType.obj_AI_Hero)
                {
                    ResetTowerWarming();
                }
                else
                {
                    ResetTowerStacks();
                }

            }
            //            if (!IncomingDamage.StackResetDelay)
            //            {
            //                Utility.DelayAction.ActionList.Clear();
            //                Utility.DelayAction.Add(1500, () => IncomingDamage.StackResetDelay = true);
            //            }

            //        else if (IncomingDamage.StackResetDelay)
            //        {
            //            IncomingDamage.ResetTowerStacks(); 
            //            IncomingDamage.StackResetDelay = false
            //        }
            //    }
            //}

        }


        private static void MinionSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (ObjectManager.Player.IsDead || ObjectManager.Player.InFountain())
            {
                return;
            }
            if (sender.IsEnemy && sender.Type == GameObjectType.obj_AI_Minion)
            {
                if (args.Target.IsMe)
                {
                    if (IncomingDamage.MinionIsLethal(sender, args))
                    {
                        Program.E.Cast();
                    }
                }
            }
        }

        private static void HeroSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (ObjectManager.Player.IsDead || ObjectManager.Player.InFountain())
            {
                return;
            }

            if (sender.IsEnemy && sender.Type == GameObjectType.obj_AI_Hero)
            {
                if (args.Target == null)
                {
                    if (IncomingDamage.SkillshotHeroIsLethal(sender, args))
                    {
                        Program.E.Cast();
                    }
                }
                else if (args.Target.IsMe && TargetedHeroIsLethal(sender, args))
                {
                    Program.E.Cast();
                }
                else if (args.Target.IsAlly && IncomingDamage.TargetedHeroIsLethal(sender, args) && args.Target.Position.Distance(Program.Player.Position)<=300) Program.W.CastOnUnit((Obj_AI_Base)args.Target);
            }
        }

        private static void TowerSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (ObjectManager.Player.IsDead || ObjectManager.Player.InFountain())
            {
                return;
            }

            if (sender.IsEnemy && sender.Type == GameObjectType.obj_AI_Turret
                && sender.Distance(ObjectManager.Player) < 2000f)
            {
                if (args.Target.IsAlly)
                {
                    if (args.Target.IsMe && IncomingDamage.TowerIsLethal(sender, args))
                    {
                        Program.E.Cast();
                    }
                    else if (args.Target.IsAlly && Program.Player.Distance(args.Target.Position) <= 300
                             && IncomingDamage.TowerIsLethal(sender, args)) 
                        Program.W.CastOnUnit((Obj_AI_Base)args.Target);
                }

            }

        }

    }
}