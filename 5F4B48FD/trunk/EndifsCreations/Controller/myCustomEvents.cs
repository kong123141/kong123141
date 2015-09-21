#region Todo
    //      OnProcessSpellCast damage buffering
    //      ItemManager - item zhonya, wooglet, archangel staff
    //      Plugin - kayle, tryndamere, lissandra
    //      SummonerSpell - Barrier, Heal
#endregion Todo

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace EndifsCreations.Controller
{
    internal static class myCustomEvents
    {       
        static myCustomEvents()
        {
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
        }
        //delegate
        public delegate void DamageBuffer(Obj_AI_Base sender, Obj_AI_Hero target, SpellData spell, DamageTriggerType type);
        //public event
        public static event DamageBuffer ProcessDamageBuffer;
        //variables
        private static List<double> BufferDamage = new List<double>();
        private static int LastOrder { get; set; }  
        //enums
        public enum DamageTriggerType
        {
            Killable,
            TonsOfDamage
        }
        //process
        private static readonly string[] HaveSpells = { "Kayle, Lissandra, Tryndamere,  Zilean" };
        private static readonly Int16[] HaveZhonya = { 3157  }; //todo wooglet id
        private static readonly Int16[] HaveShieldItems = {  }; //todo 
        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {            
            if (sender == null ||
                args.Target == null || 
                ObjectManager.Player.IsDead || ObjectManager.Player.IsZombie ||
                myUtility.IsInvulnerable(ObjectManager.Player))
            {
                return;
            }
            if (!sender.IsMe && !sender.IsAlly && args.Target.IsMe)
            {
                if (myUtility.PlayerHealthPercentage <= 50) // trigger threshold, todo individual assembly value
                {
                    if (BufferDamage.Count < 1 && myUtility.TickCount - LastOrder > 2000) //test
                    {
                        LastOrder = myUtility.TickCount;
                    }
                    BufferDamage.Add(sender.GetSpellDamage(ObjectManager.Player, args.SData.Name));                                     
                    myDevTools.DebugMode("BD Add: " + sender.GetSpellDamage(ObjectManager.Player, args.SData.Name));
                    myDevTools.DebugMode("Count: " + BufferDamage.Count + " BD Total: " + BufferDamage.Aggregate<double, double>(0, (a, b) => a + b));
                    if (myUtility.TickCount - LastOrder < 2000) return;
                    if (BufferDamage.Aggregate<double, double>(0, (a, b) => a + b) > ObjectManager.Player.MaxHealth * 0.03) //total damage more than player's 3% maxhp
                    {
                        if (ProcessDamageBuffer != null)
                        {
                            /*
                            if (mySummonerSpell.HaveBarrier || mySummonerSpell.HaveHeal)
                            {
                                ProcessDamageBuffer(sender, ObjectManager.Player, args.SData, DamageTriggerType.TonsOfDamage);
                            }
                            else
                            {*/
                                ProcessDamageBuffer(sender, ObjectManager.Player, args.SData, DamageTriggerType.TonsOfDamage);
                            /*
                             * Works for kayle peanut heal. 
                             */
                            //}
                        }
                        BufferDamage.Clear();
                    }
                    if (BufferDamage.Aggregate<double, double>(0, (a, b) => a + b) > ObjectManager.Player.Health) //triggers on killable
                    {
                        if (HaveZhonya.Any(itemId => Items.HasItem(itemId) && Items.CanUseItem(itemId)))
                        {
                            Items.UseItem(HaveZhonya.FirstOrDefault(itemId => Items.HasItem(itemId) && Items.CanUseItem(itemId)));
                        }
                        else
                        {
                            if (ProcessDamageBuffer != null)
                            {
                                ProcessDamageBuffer(sender, ObjectManager.Player, args.SData, DamageTriggerType.Killable);
                                //doesn't work with kayle's ult.
                            }
                        }
                        BufferDamage.Clear();
                    }
                }
            }
        }
    }    
}