using System;
using LeagueSharp;
using LeagueSharp.Common;
using System.Collections.Generic;
using System.Diagnostics;
using SharpDX;

using Color = System.Drawing.Color;

namespace RoyalAssistant
{
    class Program
    {
        static Menu menu;
        static int[] SRExpCumulative = { 0, 280, 660, 1140, 1720, 2400, 3180, 4060, 5040, 6120, 7300, 8580, 9960, 11440, 13020, 14700, 16480, 18360 };
        static bool bought = false;
        static System.Timers.Timer globalCooldown = new System.Timers.Timer();

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        static void OnGameLoad(EventArgs args)
        {
            LoadMenu();

            if (Game.MapId != GameMapId.SummonersRift)
            {
                Game.PrintChat("RoyalAssistant: only SR support implemented!");
                return;
            }

            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnEnd += OnGameEnd;
            Obj_AI_Hero.OnProcessSpellCast += OnSpellCast;
            globalCooldown.Elapsed += new System.Timers.ElapsedEventHandler(OnTimerProcs);

            Game.PrintChat("RoyalAssistant Loaded!");
        }

        static void OnUpdate(EventArgs args)
        {
            if (Utility.InShop(ObjectManager.Player) && menu.Item("ward").GetValue<bool>() && !HasWard())
                if (menu.Item("buyward").GetValue<KeyBind>().Active && !bought)
                {
                    ObjectManager.Player.BuyItem(ItemId.Stealth_Ward);
                    bought = true;
                }
        }
         
        static void OnSpellCast(LeagueSharp.Obj_AI_Base sender, LeagueSharp.GameObjectProcessSpellCastEventArgs args)
        {
            if (args.SData.Name != "NocturneParanoia2" || !menu.Item("noct").GetValue<bool>()) return;
                Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(args.Target.Position.X, args.Target.Position.Y, 0, 0, Packet.PingType.Danger)).Process();
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Utility.InShop(ObjectManager.Player) && menu.Item("ward").GetValue<bool>() && !HasWard())
            {
                Drawing.DrawText(menu.Item("center").GetValue<bool>() ? Drawing.Width / 2 - 40 : 200,
                                  menu.Item("center").GetValue<bool>() ? Drawing.Height / 2 - 60 : 400, GetColor(),
                                  "Buy a ward, save a life! (" + (char)menu.Item("buyward").GetValue<KeyBind>().Key + ")");
                if(bought) Utility.DelayAction.Add(10, () => bought = false);
            }

            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
                if (hero.Level != 18 && hero.IsVisible && !hero.IsDead)
                {
                    int XOffset;
                    int YOffset;
                    int textXOffset;
                    int textYOffset;
                    int width;
                    if (hero.IsMe && menu.Item("showSelf").GetValue<bool>())
                    {
                        XOffset = 8;
                        YOffset = 2;
                        width = 132;
                        textXOffset = 6;
                        textYOffset = -14;
                    }
                    else if (hero.IsAlly && !hero.IsMe && menu.Item("showAllies").GetValue<bool>() || hero.IsEnemy && menu.Item("showEnemies").GetValue<bool>())
                    {
                        if ((hero.IsAlly && menu.Item("showAlliesT").GetValue<bool>() || hero.IsEnemy && menu.Item("showEnemiesT").GetValue<bool>()))
                        {
                            XOffset = 10;
                            YOffset = 42;
                            width = 130;
                            textXOffset = 6;
                            textYOffset = 3;
                        }
                        else
                        {
                            XOffset = 8;
                            YOffset = 13;
                            width = 132;
                            textXOffset = 6;
                            textYOffset = -2;
                        }
						if(hero.IsAlly)
						{
							YOffset -= 2;
							textYOffset -= 2;
						}
                    }
                    else return;
                    Drawing.DrawLine(
                        new Vector2(hero.HPBarPosition.X + XOffset, hero.HPBarPosition.Y + YOffset),
                        new Vector2(hero.HPBarPosition.X + XOffset + width * ((180 + 100 * hero.Level + hero.Experience - SRExpCumulative[hero.Level]) / (180 + 100 * hero.Level)),
                            hero.HPBarPosition.Y + YOffset), 3, Color.Gold);
                    if (menu.Item("text").GetValue<bool>()) Drawing.DrawText(hero.HPBarPosition.X + textXOffset, hero.HPBarPosition.Y + textYOffset, Color.PaleGoldenrod, (int)(180 + 100 * hero.Level + hero.Experience - SRExpCumulative[hero.Level]) + "/" + (180 + 100 * hero.Level));
                }

        }

        static void OnGameEnd(EventArgs args)
        {
            //Cause utility won't work after game ends ( -_-)
            globalCooldown.Interval = menu.Item("delay").GetValue<Slider>().Value;
            if (menu.SubMenu("util").Item("end").GetValue<bool>())
            {
                globalCooldown.Start();
            }
        }

        static void OnTimerProcs(object sender, System.Timers.ElapsedEventArgs e)
        {
            globalCooldown.Stop();
            globalCooldown.Dispose();
            Process.GetProcessesByName("League of Legends")[0].Close();
        }
       

        static bool HasWard()
        {
            foreach (InventorySlot slot in ObjectManager.Player.InventoryItems)
                if (slot.Name.ToLower().Contains("ward") && !slot.Name.ToLower().Contains("trinket"))
                    return true;
            return false;
        }
        
        static Color GetColor()
        {
            switch ((int)((Game.Time % 1) * 10))//SHITTY CODE! :D
            {
                case 0:
                    return Color.IndianRed;
                case 1:
                    return Color.LightGoldenrodYellow;
                case 2:
                    return Color.Goldenrod;
                case 3:
                    return Color.Green;
                case 4:
                    return Color.Blue;
                case 5:
                    return Color.Violet;
                case 6:
                    return Color.DeepPink;
                case 7:
                    return Color.DeepSkyBlue;
                case 8:
                    return Color.White;
                case 9:
                    return Color.Cyan;
                default:
                    return Color.ForestGreen;
            }
        }
        static void LoadMenu()
        {
            // Initialize the menu
            menu = new Menu("RoyalAssistant", "RoyalAssistant", true);

            menu.AddSubMenu(new Menu("Exp tracker", "track"));
            menu.SubMenu("track").AddItem(new MenuItem("showSelf", "Show your XP bar").SetValue(false));
            menu.SubMenu("track").AddItem(new MenuItem("showAllies", "Show allies XP bar").SetValue(true));
            menu.SubMenu("track").AddItem(new MenuItem("showEnemies", "Show enemies XP bar").SetValue(true));
            menu.SubMenu("track").AddItem(new MenuItem("text", "Draw XP count").SetValue(true));
            menu.SubMenu("track").AddItem(new MenuItem("1", "                  Tracker settings:"));
            //menu.AddItem(new MenuItem("showSelfT", "Tracker showing self").SetValue(false));
            menu.SubMenu("track").AddItem(new MenuItem("showAlliesT", "Tracker showing allies").SetValue(true));
            menu.SubMenu("track").AddItem(new MenuItem("showEnemiesT", "Tracker showing enemies").SetValue(true));

            menu.AddSubMenu(new Menu("Utilities", "util"));
            menu.SubMenu("util").AddItem(new MenuItem("end", "Quit game on end").SetValue(true));
            menu.SubMenu("util").AddItem(new MenuItem("delay", "Custom delay to closing LoL").SetValue(new Slider(500, 0, 3000)));
            menu.SubMenu("util").AddItem(new MenuItem("ward", "Show \"Buy ward\" reminder").SetValue(true));
            menu.SubMenu("util").AddItem(new MenuItem("center", "^ Place this message on center ^").SetValue(false));
            menu.SubMenu("util").AddItem(new MenuItem("buyward", "Buy ward key").SetValue(new KeyBind('U', KeyBindType.Press)));
            menu.SubMenu("util").AddItem(new MenuItem("noct", "Show Nocturne's ulti target").SetValue(true));

            menu.AddToMainMenu();
            Console.WriteLine("Menu finalized");
        }
    }
}
