#region Todo
    //      Smart menu (checks available items, adds to menu)
    //      More items, add toggles
#endregion Todo

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using ItemData = LeagueSharp.Common.Data.ItemData;

namespace EndifsCreations.Controller
{
    internal class myItemManager : PluginData
    {
        private static readonly Items.Item Ghostblade = ItemData.Youmuus_Ghostblade.GetItem();
        private static readonly Items.Item Tiamat = ItemData.Tiamat_Melee_Only.GetItem();
        private static readonly Items.Item Zhonya = ItemData.Zhonyas_Hourglass.GetItem();

        public static void UseGhostblade()
        {
            if (Ghostblade.IsReady()) Ghostblade.Cast();
        }
        public static void UseZhonya()
        {
            if (Zhonya.IsReady()) Zhonya.Cast();
        }
        public static void UseTiamat()
        {
            if (Tiamat.IsReady()) Tiamat.Cast();
        }

        public static void UseItems(int index, Obj_AI_Base target)
        {
            Int16[] SelfBuffItems =
            {
                3142, //Ghostblade
                3184, //Entropy
            };
            Int16[] TargettedItems =
            {
                3144, //Bilgewater Cutlass
                3153, //Blade of the Ruined King      
                3146, //Hextech Gunblade                
            };
            Int16[] AoeOffenseItems =
            {
                3748, //Titanic Hydra
                3074, //Ravenous Hydra
                3077, //Tiamat
            };
            Int16[] AoeDefenseItems =
            {
                3143, //Randuin
            };
            switch (index)
            {
                case 0:
                    foreach (var itemId in SelfBuffItems.Where(itemId => Items.HasItem(itemId) && Items.CanUseItem(itemId)))
                    {
                        if (target != null)
                        {
                            if (Orbwalking.InAutoAttackRange(target)) Items.UseItem(itemId);
                        }
                        else
                        {
                            Items.UseItem(itemId);
                        }
                    }
                    break;
                case 1:
                    foreach (var itemId in TargettedItems.Where(itemId => Items.HasItem(itemId) && Items.CanUseItem(itemId)))
                    {
                        if (target != null)
                        {
                            Items.UseItem(itemId, target);
                        }
                    }
                    break;
                case 2:
                    foreach (var itemId in AoeOffenseItems.Where(itemId => Items.HasItem(itemId) && Items.CanUseItem(itemId)))
                    {
                        Items.UseItem(itemId);
                    }
                    break;
                case 3:
                    foreach (var itemId in AoeDefenseItems.Where(itemId => Items.HasItem(itemId) && Items.CanUseItem(itemId)))
                    {
                        Items.UseItem(itemId);
                    }
                    break;
            }
        }
    }
}