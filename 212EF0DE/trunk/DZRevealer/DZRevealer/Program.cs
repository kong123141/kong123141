﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Color = System.Drawing.Color;

namespace DZRevealer
{
    //Inspired from the discontinued BoL script Disclosures. Many thanks to the Autor for his Trinket\Ward place calcs.

    class Program
    {
        public static Dictionary<String, String> dict;
        public static Obj_AI_Base player = ObjectManager.Player;
        public static Spell E;
        public static Menu menu;
        public static int VISION_WARD = 2043;
        public static int TRINKET_RED = 3364;
        public static float wardrange = 600f;
        public static float trinket_range = 600f;
        public static bool debug = false;
        static void Main(string[] args)
        {
            try
            {
                CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return;
            }
            
        }
        static void Game_OnGameLoad(EventArgs args)
        {
            menu = new Menu("DZReveal!", "DZReveal", true);
            menu.AddItem(new MenuItem("doRev", "Reveal").SetValue(true));
            menu.AddItem(new MenuItem("revDesc1", "Priority:"));
            menu.AddItem(new MenuItem("prior", "ON: Pink OFF: Trinket").SetValue(true));
            if(player.BaseSkinName =="LeeSin")
            {
                menu.AddItem(new MenuItem("leeE", "Lee Sin: Use E").SetValue(true));
                E = new Spell(SpellSlot.E, 350f);
            }
            Game.PrintChat("DZReveal Loaded");
            menu.AddToMainMenu();
            fillDict();
            Game.PrintChat(player.BaseSkinName);
            Game.OnGameProcessPacket += Game_OnGameProcessPacket;
            Game.OnGameUpdate += Game_GameUpdate;
        }

        private static void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
        }
        public static void OnGainBuff(Obj_AI_Hero unit,String buff)
        {

        }
        private static void Game_GameUpdate(EventArgs args)
        {
            if (!isEn("doRev")) return;
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                if(enemy.HasBuffOfType(BuffType.Invisibility) && !(enemy.BaseSkinName =="Evelynn"))
                {
                    Reveal(enemy);
                }
            }

        }

        static void Reveal(Obj_AI_Hero enemy)
        {
            if(player.BaseSkinName == "LeeSin" && E.IsReady() && player.Distance(enemy)<= E.Range && isEn("leeE"))
            {
                E.Cast();
            }
            else
            {
            if (isEn("prior"))
            {
                //W
                if (player.Distance(enemy) <= wardrange+300f)
                {
                    if(player.Distance(enemy) <= wardrange)
                    {
                        useItem(VISION_WARD, enemy.Position);
                    }
                    else
                    {       
                        Vector3 pos1 = Vector3.Lerp(player.Position, enemy.Position, wardrange / player.Distance(enemy));
                        useItem(VISION_WARD, pos1);
                    }
                    
                }
            }
            else
            {
                //Trink
                if (player.Distance(enemy) <= trinket_range+300f)
                {
                    if (player.Distance(enemy) <= trinket_range)
                    {
                        useItem(TRINKET_RED, enemy.Position);
                    }
                    else
                    {
                        Vector3 pos1 = Vector3.Lerp(player.Position, enemy.Position, trinket_range / player.Distance(enemy));
                        useItem(TRINKET_RED, pos1);
                    }
                    
                }
            }
            }
            
        }
        public static bool isEn(String item)
        {
            return menu.Item(item).GetValue<bool>();
        }
        
        static void fillDict()
        {
            dict = new Dictionary<String, String>();
            dict.Add("Vayne", "VayneTumbleFade");
            dict.Add("Twitch", "TwitchHideInShadows");
            dict.Add("Rengar", "RengarR");
            dict.Add("MonkeyKing", "monkeykingdecoystealth");
            dict.Add("Khazix", "khazixrstealth");
            dict.Add("Talon", "talonshadowassaultbuff");
            dict.Add("Akali", "akaliwstealth");
        }
        public static void useItem(int id, Vector3 position)
        {
            if (Items.HasItem(id) && Items.CanUseItem(id))
            {
                Items.UseItem(id,position);
            }
        }
    }
}
