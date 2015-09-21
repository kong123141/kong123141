﻿using System;
using LeagueSharp.Common;
using PRADA_Vayne.MyUtils;
using Activator = PRADA_Vayne.MyUtils.Activator;

namespace PRADA_Vayne
{
    public class Program
    {
        #region Fields and Objects

        public static Activator Activator;
        public static MyOrbwalker.Orbwalker Orbwalker;
        public static PotionManager PotionManager;

        #region Menu

        public static Menu MainMenu;
        public static Menu ComboMenu;
        public static Menu LaneClearMenu;
        public static Menu EscapeMenu;
        public static Menu ActivatorMenu;
        public static Menu DrawingsMenu;
        public static Menu SkinhackMenu;
        public static Menu OrbwalkerMenu;

        #endregion Menu

        #region Spells

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        #endregion Spells

        #endregion

        private static void Main(string[] args)
        {
            MyInitializer.PRADALoader.Init();
        }
    }
}
