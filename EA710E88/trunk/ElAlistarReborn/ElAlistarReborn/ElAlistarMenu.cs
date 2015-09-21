﻿namespace ElAlistarReborn
{
    using System;

    using LeagueSharp.Common;

    public class ElAlistarMenu
    {
        #region Static Fields

        public static Menu Menu;

        #endregion

        #region Public Methods and Operators

        public static void Initialize()
        {
            Menu = new Menu("ElAlistar:Reborn", "menu", true);

            var orbwalkerMenu = new Menu("Orbwalker", "orbwalker");
            Alistar.Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            Menu.AddSubMenu(orbwalkerMenu);

            var targetSelector = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);

            Menu.AddSubMenu(targetSelector);

            var comboMenu = new Menu("Combo", "Combo");
            comboMenu.AddItem(new MenuItem("ElAlistar.Combo.Q", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("ElAlistar.Combo.W", "Use W").SetValue(true));
            comboMenu.AddItem(new MenuItem("ElAlistar.Combo.E", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("ElAlistar.Combo.R", "Use R").SetValue(true));
            comboMenu.AddItem(
                new MenuItem("ElAlistar.Combo.Count.Enemies", "Enemies in range for R").SetValue(new Slider(2, 1, 5)));
            comboMenu.AddItem(new MenuItem("ElAlistar.Combo.HP.Enemies", "R when HP").SetValue(new Slider(55)));
            comboMenu.AddItem(new MenuItem("ElAlistar.Combo.Ignite", "Use Ignite").SetValue(true));

            comboMenu.AddItem(
                new MenuItem("ElAlistar.Combo.FlashQ", "Flash Q").SetValue(
                    new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));

            /*comboMenu.AddItem(
                new MenuItem("ElAlistar.Combo.Flash", "Flash Q and W to ally").SetValue(
                    new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));*/
            comboMenu.AddItem(new MenuItem("ElAlistar.Combo.Click", "Left Click [on] TS [off]").SetValue(true));

            Menu.AddSubMenu(comboMenu);

            var harassMenu = new Menu("Harass", "Harass");
            harassMenu.AddItem(new MenuItem("ElAlistar.Harass.Q", "Use Q").SetValue(true));

            Menu.AddSubMenu(harassMenu);

            var healMenu = new Menu("Heal", "Heal");
            healMenu.AddItem(new MenuItem("ElAlistar.Heal.Activated", "Use heal").SetValue(true));
            healMenu.AddItem(new MenuItem("ElAlistar.Heal.Player.HP", "Player HP percentage").SetValue(new Slider(55)));

            //heal ally
            healMenu.AddItem(new MenuItem("ElAlistar.Heal.Ally.Activated", "Heal ally").SetValue(true));
            healMenu.AddItem(new MenuItem("ElAlistar.Heal.Ally.HP", "Ally HP percentage").SetValue(new Slider(55)));
            healMenu.AddItem(new MenuItem("ElAlistar.Heal.Player.Mana", "Mana for heal").SetValue(new Slider(55)));

            Menu.AddSubMenu(healMenu);

            var miscMenu = new Menu("Misc", "Misc");
            miscMenu.AddItem(new MenuItem("ElAlistar.Draw.off", "Turn drawings off").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElAlistar.Draw.Q", "Draw Q").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElAlistar.Draw.W", "Draw W").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElAlistar.Draw.E", "Draw E").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("xxx", ""));
            miscMenu.AddItem(new MenuItem("ElAlistar.Interrupt", "Interrupt spells").SetValue(true));

            Menu.AddSubMenu(miscMenu);

            //Here comes the moneyyy, money, money, moneyyyy
            var credits = Menu.AddSubMenu(new Menu("Credits", "jQuery"));
            credits.AddItem(new MenuItem("ElZilean.Paypal", "if you would like to donate via paypal:"));
            credits.AddItem(new MenuItem("ElZilean.Email", "info@zavox.nl"));

            Menu.AddItem(new MenuItem("422442fsaafs4242f", ""));
            Menu.AddItem(new MenuItem("422442fsaafsf", "Version: 1.0.0.3"));
            Menu.AddItem(new MenuItem("fsasfafsfsafsa", "Made By jQuery"));

            Menu.AddToMainMenu();

            Console.WriteLine("Menu Loaded");
        }

        #endregion
    }
}