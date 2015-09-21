﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.IO;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace JeonJunglePlay
{
    class Readini:CaptureLib
    {
        public static AutoLevel autoLevel;
        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static void Setini(string path)
        {
            var str = ObjectManager.Player.ChampionName;
            string[] supportnames = { "NUNU", "WARWICK", "MASTERYI", "CHOGATH", "MAOKAI", "NASUS" };

            
            Game.PrintChat("Your champion play on first time. - set ini file");

            if(str.ToUpper() =="NUNU")
            {
                SetSettingValue("ItemTreeType(AP,AD,TANK,AS)", "Type", "AP", path);
                SetSettingValue("SpellTree", "Value", "1, 3, 2, 1, 1, 4, 1, 3, 1, 3, 4, 2, 2, 2, 2, 4, 3, 3", path);
            }
            else if (str.ToUpper() == "WARWICK")
            {
                SetSettingValue("ItemTreeType(AP,AD,TANK,AS)", "Type", "AS", path);
                SetSettingValue("SpellTree", "Value", "1, 2, 3, 1, 1, 4, 1, 3, 1, 3, 4, 2, 2, 2, 2, 4, 3, 3", path);
            }
            else if (str.ToUpper() == "MASTERYI")
            {
                SetSettingValue("ItemTreeType(AP,AD,TANK,AS)", "Type", "AD", path);
                SetSettingValue("SpellTree", "Value", "1, 2, 3, 1, 1, 4, 1, 3, 1, 3, 4, 2, 2, 2, 2, 4, 3, 3", path);
            }
            else if (str.ToUpper() == "CHOGATH")
            {
                SetSettingValue("ItemTreeType(AP,AD,TANK,AS)", "Type", "AP", path);
                SetSettingValue("SpellTree", "Value", "3, 2, 1, 3, 3, 4, 3, 1, 3, 1, 4, 2, 2, 2, 2, 4, 1, 1", path);
            }
            else if (str.ToUpper() == "MAOKAI")
            {
                SetSettingValue("ItemTreeType(AP,AD,TANK,AS)", "Type", "AP", path);
                SetSettingValue("SpellTree", "Value", "1, 2, 3, 1, 1, 4, 1, 3, 1, 3, 4, 2, 2, 2, 2, 4, 3, 3", path);
            }
            else if (str.ToUpper() == "NASUS")
            {
                SetSettingValue("ItemTreeType(AP,AD,TANK,AS)", "Type", "TANK", path);
                SetSettingValue("SpellTree", "Value", "1, 3, 3, 2, 3, 4, 3, 1, 3, 1, 4, 1, 1, 2, 2, 4, 2, 2", path);
            }
            else
            {
                SetSettingValue("ItemTreeType(AP,AD,TANK,AS)", "Type", "AD", path);
                SetSettingValue("SpellTree", "Value", "1, 3, 2, 1, 1, 4, 1, 3, 1, 3, 4, 2, 2, 2, 2, 4, 3, 3", path);

                SetSettingValue("Cast(Q,W,E,R)", "Mob", "Q,W,E", path);
                SetSettingValue("Cast(Q,W,E,R)", "Hero", "Q,W,E,R", path);
                SetSettingValue("Cast(Q,W,E,R)", "LaneClear", "Q,W,E", path);
            }



        }
        public static void GetSpelltree(int[] defTree)
        {
            int[] tree = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            
            for (var i = 0; i < 18; i++)
            {
                tree[i] = defTree[i] - 1;
            }

            autoLevel = new AutoLevel(tree);
            AutoLevel.Enabled(true);
        }

        public static void UpdateLvl()
        {
            int[] tree = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int[] defTree = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            if (Player.ChampionName.ToUpper() == "NUNU")
            {
                tree = new int[] { 1, 3, 2, 1, 1, 4, 1, 3, 1, 3, 4, 2, 2, 2, 2, 4, 3, 3 };
            }
            else if (Player.ChampionName.ToUpper() == "WARWICK")
            {
                tree = new int[] { 1, 2, 3, 1, 1, 4, 1, 3, 1, 3, 4, 2, 2, 2, 2, 4, 3, 3 };
            }
            else if (Player.ChampionName.ToUpper() == "MASTERYI")
            {
                tree = new int[]  { 1, 2, 3, 1, 1, 4, 1, 3, 1, 3, 4, 2, 2, 2, 2, 4, 3, 3 };
            }
            else if (Player.ChampionName.ToUpper() == "CHOGATH")
            {
               tree = new int[] { 3, 2, 1, 3, 3, 4, 3, 1, 3, 1, 4, 2, 2, 2, 2, 4, 1, 1 };
            }
            else if (Player.ChampionName.ToUpper() == "MAOKAI")
            {
                tree = new int[]  { 1, 2, 3, 1, 1, 4, 1, 3, 1, 3, 4, 2, 2, 2, 2, 4, 3, 3 };
            }
            else if (Player.ChampionName.ToUpper() == "NASUS")
            {
                tree = new int[] { 1, 3, 3, 2, 3, 4, 3, 1, 3, 1, 4, 1, 1, 2, 2, 4, 2, 2 };
            }
            else
            {
               tree = new int[] { 1, 3, 2, 1, 1, 4, 1, 3, 1, 3, 4, 2, 2, 2, 2, 4, 3, 3 };
            }
            for (var i = 0; i < 18; i++)
            {
                defTree[i] = tree[i] - 1;
            }
            AutoLevel.UpdateSequence(defTree);
            AutoLevel.Enabled(true);
        }
        public static string GetItemTreetype(string path)
        {
            string[] s = { "AP", "AD", "TANK", "AS" };
            var str = GetSettingValue_String("ItemTreeType(AP,AD,TANK,AS)", "Type", path);

            if (!s.Contains(str.ToUpper()))
                return "X";

            return str.ToUpper();
        }

        public static void GetSpells(string path, ref List<Spell> mob, ref List<Spell> hero, ref List<Spell> laneclear)
        {
            try
            {
                var str = GetSettingValue_String("Cast(Q,W,E,R)", "Mob", path);

                foreach (var s in str.Replace(" ","").Split(','))
                {
                    if (s.ToUpper() == "Q")
                        mob.Add(Program.Q);
                    if (s.ToUpper() == "W")
                        mob.Add(Program.W);
                    if (s.ToUpper() == "E")
                        mob.Add(Program.E);
                    if (s.ToUpper() == "R")
                        mob.Add(Program.R);
                }

                str = GetSettingValue_String("Cast(Q,W,E,R)", "Hero", path);

                foreach (var s in str.Split(','))
                {
                    if (s.ToUpper() == "Q")
                        hero.Add(Program.Q);
                    if (s.ToUpper() == "W")
                        hero.Add(Program.W);
                    if (s.ToUpper() == "E")
                        hero.Add(Program.E);
                    if (s.ToUpper() == "R")
                        hero.Add(Program.R);
                }

                str = GetSettingValue_String("Cast(Q,W,E,R)", "LaneClear", path);

                foreach (var s in str.Split(','))
                {
                    if (s.ToUpper() == "Q")
                        laneclear.Add(Program.Q);
                    if (s.ToUpper() == "W")
                        laneclear.Add(Program.W);
                    if (s.ToUpper() == "E")
                        laneclear.Add(Program.E);
                    if (s.ToUpper() == "R")
                        laneclear.Add(Program.R);
                }

                Game.PrintChat("Get Spell data - Finished");
            }
            catch
            {
                Game.PrintChat("Get Spell data - ERROR");
            }
        }
    }
}
