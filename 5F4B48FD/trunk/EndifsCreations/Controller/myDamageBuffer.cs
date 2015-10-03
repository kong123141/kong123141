#region Todo
    //      SummonerSpell - Barrier, Heal
#endregion Todo

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace EndifsCreations.Controller
{
    internal static class myDamageBuffer
    {
        static myDamageBuffer()
        {
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
        }
        public delegate void DamageBuffer(Obj_AI_Base sender, Obj_AI_Hero target, SpellData spell, float damage, DamageTriggers type);
        public static event DamageBuffer ProcessDamageBuffer;
        private static List<float> BufferDamage = new List<float>();
        private static int LastOrder { get; set; }
        public enum DamageTriggers
        {
            None,
            SummonerSpells,
            Items,
            Killable,
            TonsOfDamage
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender == null || sender.IsAlly || sender.IsMe ||
                args.Target == null || !args.Target.IsMe ||
                ObjectManager.Player.IsDead || ObjectManager.Player.IsZombie ||
                myUtility.IsInvulnerable(ObjectManager.Player))
            {
                return;
            }
            if (BufferDamage.Count < 1)
            {
                LastOrder = myUtility.TickCount;
            }
            if (sender.GetSpellDamage(ObjectManager.Player, args.SData.Name) > 0)
            {
                BufferDamage.Add((float)sender.GetSpellDamage(ObjectManager.Player, args.SData.Name));
            }
            //myDevTools.DebugMode("BD Add: " + sender.GetSpellDamage(ObjectManager.Player, args.SData.Name));
            var bdtotal = Math.Abs(BufferDamage.Aggregate<float, float>(0, (a, b) => a + b));
            //myDevTools.DebugMode("Count: " + BufferDamage.Count + " BD Total: " + bdtotal);
            if (myUtility.TickCount - LastOrder < 1000) return;
            if (mySummonerSpell.CanUseHeal || mySummonerSpell.CanUseBarrier)
            {
                if (ProcessDamageBuffer != null)
                {
                    ProcessDamageBuffer(sender, (Obj_AI_Hero)args.Target, args.SData, bdtotal, DamageTriggers.SummonerSpells);
                }
            }
            if (bdtotal > ObjectManager.Player.MaxHealth * 0.05 && !(bdtotal > ObjectManager.Player.Health))
            {
                if (ProcessDamageBuffer != null)
                {
                    ProcessDamageBuffer(sender, (Obj_AI_Hero)args.Target, args.SData, bdtotal, DamageTriggers.TonsOfDamage);
                }
            }
            if (bdtotal > ObjectManager.Player.Health)
            {
                if (myUtility.PlayerHealthPercentage <= 30)
                {
                    if (ProcessDamageBuffer != null)
                    {
                        ProcessDamageBuffer(sender, (Obj_AI_Hero)args.Target, args.SData, bdtotal, DamageTriggers.Killable);
                    }
                }
            }
            BufferDamage.Clear();
        }
    }
}