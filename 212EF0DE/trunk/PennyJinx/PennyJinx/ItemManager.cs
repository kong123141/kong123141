﻿using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace PennyJinx
{
    class ItemManager
    {
        private static float _lastCheckTick;
        //TODO: List of Activator Features here:

        //TODO: Shield Module
        //TODO: Summoners Spells Implementation

        private static readonly List<DzItem> ItemList = new List<DzItem>
        {
            new DzItem
            {
                Id=3144,
                Name = "Bilgewater Cutlass",
                Range = 600f,
                Class = ItemClass.Offensive
            },
            new DzItem
            {
                Id= 3153,
                Name = "Blade of the Ruined King",
                Range = 600f,
                Class = ItemClass.Offensive
            },
            new DzItem
            {
                Id= 3142,
                Name = "Youmuu",
                Range = float.MaxValue,
                Class = ItemClass.Offensive
            }
        };

        public static void OnLoad(Menu menu)
        {
            //Create the menu here.
            var cName = ObjectManager.Player.ChampionName;
            var activatorMenu = new Menu("[PJ] Activator", "pennyjinx.activator");

            //Offensive Menu
            var offensiveMenu = new Menu("Activator - Offensive", "pennyjinx.activator.offensive");
            var offensiveItems = ItemList.FindAll(item => item.Class == ItemClass.Offensive);
            foreach (var item in offensiveItems)
            {
                var itemMenu = new Menu(item.Name, cName + item.Id);
                itemMenu.AddItem(new MenuItem("pennyjinx.activator." + item.Id + ".always", "Always").SetValue(true));
                itemMenu.AddItem(new MenuItem("pennyjinx.activator." + item.Id + ".onmyhp", "On my HP < then %").SetValue(new Slider(30)));
                itemMenu.AddItem(new MenuItem("pennyjinx.activator." + item.Id + ".ontghpgreater", "On Target HP > then %").SetValue(new Slider(40)));
                itemMenu.AddItem(new MenuItem("pennyjinx.activator." + item.Id + ".ontghplesser", "On Target HP < then %").SetValue(new Slider(40)));
                itemMenu.AddItem(new MenuItem("pennyjinx.activator." + item.Id + ".ontgkill", "On Target Killable").SetValue(true));
                itemMenu.AddItem(new MenuItem("pennyjinx.activator." + item.Id + ".displaydmg", "Display Damage").SetValue(true));
                offensiveMenu.AddSubMenu(itemMenu);
            }
            activatorMenu.AddSubMenu(offensiveMenu);

            //Defensive Menu
            AddHitChanceSelector(activatorMenu);

            activatorMenu.AddItem(new MenuItem("pennyjinx.activator.activatordelay", "Global Activator Delay").SetValue(new Slider(80, 0, 300)));
            activatorMenu.AddItem(new MenuItem("pennyjinx.activator.enabledalways", "Enabled Always?").SetValue(false));
            activatorMenu.AddItem(new MenuItem("pennyjinx.activator.enabledcombo", "Enabled On Press?").SetValue(new KeyBind(32, KeyBindType.Press)));
            menu.AddSubMenu(activatorMenu);
            Game.OnUpdate += Game_OnGameUpdate;
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            if (!PennyJinx.IsMenuEnabled("pennyjinx.activator.enabledalways") ||
                !PennyJinx.GetKeyBindValue("pennyjinx.activator.enabledcombo"))
            {
                return;
            }
            if (Environment.TickCount - _lastCheckTick < PennyJinx.GetSliderValue("pennyjinx.activator.activatordelay"))
            {
                return;
            }
            _lastCheckTick = Environment.TickCount;
            UseOffensive();
        }

        static void UseOffensive()
        {
            var offensiveItems = ItemList.FindAll(item => item.Class == ItemClass.Offensive);
            foreach (var item in offensiveItems)
            {
                var selectedTarget = Hud.SelectedUnit as Obj_AI_Base ?? TargetSelector.GetTarget(item.Range, TargetSelector.DamageType.True);
                if (!selectedTarget.IsValidTarget())
                    return;
                if (PennyJinx.IsMenuEnabled("pennyjinx.activator." + item.Id + ".always"))
                {
                    UseItem(selectedTarget, item);
                }
                if (ObjectManager.Player.HealthPercentage() < PennyJinx.GetSliderValue("pennyjinx.activator." + item.Id + ".onmyhp"))
                {
                    UseItem(selectedTarget, item);
                }
                if (selectedTarget.HealthPercentage() < PennyJinx.GetSliderValue("pennyjinx.activator." + item.Id + ".ontghplesser") && !PennyJinx.IsMenuEnabled("pennyjinx.activator." + item.Id + ".ontgkill"))
                {
                    UseItem(selectedTarget, item);
                }
                if (selectedTarget.HealthPercentage() > PennyJinx.GetSliderValue("pennyjinx.activator." + item.Id + ".ontghpgreater"))
                {
                    UseItem(selectedTarget, item);
                }
                if (selectedTarget.Health < ObjectManager.Player.GetSpellDamage(selectedTarget, GetItemSpellSlot(item)) && PennyJinx.IsMenuEnabled("pennyjinx.activator." + item.Id + ".ontgkill"))
                {
                    UseItem(selectedTarget, item);
                }
            }
        }


        static void UseItem(Obj_AI_Base target, DzItem item)
        {
            if (!Items.HasItem(item.Id) || !Items.CanUseItem(item.Id))
            {
                return;
            }
            switch (item.Mode)
            {
                case ItemMode.Targeted:
                    Items.UseItem(item.Id, target);
                    break;
                case ItemMode.Skillshot:
                    var customPred = Prediction.GetPrediction(item.CustomInput);
                    if (customPred.Hitchance >= GetHitchance())
                    {
                        Items.UseItem(item.Id, customPred.CastPosition);
                    }
                    break;
            }
        }

        static SpellSlot GetItemSpellSlot(DzItem item)
        {
            foreach (var it in ObjectManager.Player.InventoryItems.Where(it => (int)it.Id == item.Id))
            {
                return it.SpellSlot != SpellSlot.Unknown ? it.SpellSlot : SpellSlot.Unknown;
            }
            return SpellSlot.Unknown;
        }

        public static HitChance GetHitchance()
        {
            switch (PennyJinx.Menu.Item("pennyjinx.activator.customhitchance").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }

        public static void AddHitChanceSelector(Menu menu)
        {
            menu.AddItem(new MenuItem("pennyjinx.activator.customhitchance", "Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 2)));
        }

        internal static float GetItemsDamage(Obj_AI_Hero target)
        {
            var items = ItemList.Where(item => Items.HasItem(item.Id) && Items.CanUseItem(item.Id) && PennyJinx.IsMenuEnabled("pennyjinx.activator." + item.Id + ".displaydmg"));
            return items.Sum(item => (float)ObjectManager.Player.GetSpellDamage(target, GetItemSpellSlot(item)));
        }
    }

    internal class DzItem
    {
        public String Name { get; set; }
        public int Id { get; set; }
        public float Range { get; set; }
        public ItemClass Class { get; set; }
        public ItemMode Mode { get; set; }
        public PredictionInput CustomInput { get; set; }
    }

    enum ItemMode
    {
        Targeted, Skillshot
    }

    enum ItemClass
    {
        Offensive, Defensive
    }
}
