namespace Pluging
{
    using LeagueSharp;
    using LeagueSharp.Common;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ItemData = LeagueSharp.Common.Data.ItemData;

    public class Activator
    {
        public static Menu Menu;
        public delegate Items.Item GetHealthItemDelegate();
        public static Obj_AI_Hero Player;
        public static List<HealthItem> Items { get; set; }

        public Activator(Menu mainMenu)
        {
            Menu = mainMenu;

            Menu ActivatorMenu = new Menu("[FL] 活化剂", "Activator");

            ActivatorMenu.SubMenu("药水设置").AddItem(new MenuItem("PlayerHealth", "嗑药百分比").SetValue(new Slider(20)));
            ActivatorMenu.SubMenu("药水设置").AddItem(new MenuItem("Activate", "嗑药开关").SetValue(true));

            Menu.AddSubMenu(ActivatorMenu);

            Player = ObjectManager.Player;

            Items = new List<HealthItem>
                             {
                                 new HealthItem { GetItem = () => ItemData.Health_Potion.GetItem() },
                                 new HealthItem { GetItem = () => ItemData.Total_Biscuit_of_Rejuvenation2.GetItem() },
                                 new HealthItem { GetItem = () => ItemData.Refillable_Potion.GetItem() }, 
                                 new HealthItem { GetItem = () => ItemData.Hunters_Potion.GetItem() },
                                 new HealthItem { GetItem = () => ItemData.Corrupting_Potion.GetItem() }
                             };

            Game.OnUpdate += OnUpdate;
        }

        private void OnUpdate(EventArgs args)
        {
            try
            {
                if (Player.IsDead)
                    return;

                if (!Menu.Item("Activate").GetValue<bool>())
                    return;

                if (Player.IsRecalling())
                    return;

                if (Player.InFountain())
                    return;

                if(Player.HealthPercent < Menu.Item("PlayerHealth").GetValue<Slider>().Value)
                {
                    if(Player.HasBuff("RegenerationPotion") || Player.HasBuff("RegenerationPotion") || Player.HasBuff("ItemMiniRegenPotion") || Player.HasBuff("ItemCrystalFlask") || Player.HasBuff("ItemCrystalFlaskJungle") || Player.HasBuff("ItemDarkCrystalFlask"))
                    {
                        return;
                    }

                    var item = Items.Select(x => x.Item).FirstOrDefault(x => x.IsReady() && x.IsOwned());

                    if (item != null)
                    {
                        item.Cast();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Activator.OnUpdate + " + ex);
            }
        }

        public class HealthItem
        {
            public Activator.GetHealthItemDelegate GetItem { get; set; }
            public Items.Item Item => GetItem();
        }

    }
}