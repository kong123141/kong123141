﻿#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
#endregion

namespace Humanizer
{
    public class Program
    {

        public static Menu Config;
        public static float lastmovement;

        public class LatestCast
        {
            public static float Tick = 0;
            public static float Timepass;
            public static double X;
            public static double Y;
            public static double Distance;
            public static double Delay;

        }

        private static Obj_AI_Hero Player;
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Console.WriteLine("Humanizer LOADED");
            LeagueSharp.Game.OnGameSendPacket += PacketHandler;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            //Create the menu
            Config = new Menu("Humanizer", "Humanizer", true);

            Config.AddSubMenu(new Menu("Casts delay", "Castsdelay"));
            Config.AddSubMenu(new Menu("Movements delay", "Movementdelay"));
            Config.SubMenu("Castsdelay").AddItem(new MenuItem("delaytime", "Delay time for distance")).SetValue(new Slider(0, 100, 0));
            Config.SubMenu("Castsdelay").AddItem(new MenuItem("delaytimecasts", "Delay time between casts")).SetValue(new Slider(0, 100, 0));
            Config.SubMenu("Movementdelay").AddItem(new MenuItem("delaytimem", "Delay time")).SetValue(new Slider(0, 100, 0));
            Config.AddToMainMenu();
        }
        private static void PacketHandler(GamePacketEventArgs args)
        {
            var Packetc = new GamePacket(args.PacketData);
            if (Packetc.Header == Packet.C2S.Cast.Header)
            {
                
                var decodedpacket = Packet.C2S.Cast.Decoded(args.PacketData);
                LatestCast.Timepass = Environment.TickCount - LatestCast.Tick;
                LatestCast.Distance = Math.Sqrt(Math.Pow(decodedpacket.ToX - LatestCast.X, 2) + Math.Pow(decodedpacket.ToY - LatestCast.Y, 2));
                LatestCast.Delay = (LatestCast.Distance * 0.01 * Config.Item("delaytime").GetValue<Slider>().Value + Config.Item("delaytimecasts").GetValue<Slider>().Value);
                if (Environment.TickCount < LatestCast.Tick + LatestCast.Delay)
                {
                    args.Process = false;
                }
                if (args.Process == true)
                {
                    LatestCast.Tick = Environment.TickCount;
                    LatestCast.X = decodedpacket.ToX;
                    LatestCast.Y = decodedpacket.ToY;
                }
            }

            else if (Packetc.Header == Packet.C2S.Move.Header)
            {
                //Console.WriteLine("Last movement : " + lastmovement.ToString() + "\n DelayTime : " + (Config.Item("delaytimem").GetValue<Slider>().Value * 25).ToString() + "\n Tick : " + Environment.TickCount.ToString());
                var decodedpacket = Packet.C2S.Move.Decoded(args.PacketData);
                if (decodedpacket.MoveType != 2)
                {
                    return;
                }
                if (lastmovement + Config.Item("delaytimem").GetValue<Slider>().Value * 5 > Environment.TickCount)
                {
                    args.Process = false;
                    Console.WriteLine("delayed");
                }
                else
                {
                    args.Process = true;
                    lastmovement = Environment.TickCount;

                }
            }
        }
    }
}