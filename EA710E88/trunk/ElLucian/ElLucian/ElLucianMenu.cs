using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;

namespace ElLucian
{
    public class ElLucianMenu
    {
        public static Menu Menu;

        public static void Initialize()
        {
            Menu = new Menu("ElLucian", "menu", true);

            var orbwalkerMenu = new Menu("Orbwalker", "orbwalker");
            Lucian.Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            Menu.AddSubMenu(orbwalkerMenu);

            var targetSelector = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);
            Menu.AddSubMenu(targetSelector);

            var cMenu = new Menu("Combo", "Combo");
            {
                cMenu.AddItem(new MenuItem("ElLucian.Combo.Q", "Use Q").SetValue(true));
                cMenu.AddItem(new MenuItem("ElLucian.Combo.W", "Use W").SetValue(true));
            }

            Menu.AddSubMenu(cMenu);

            var hMenu = new Menu("Harass", "Harass");
  
        
            Menu.AddSubMenu(hMenu);

            var lMenu = new Menu("Lane clear", "Clear");
  

            Menu.AddSubMenu(lMenu);


            var itemMenu = new Menu("Items", "Items");
            {
                itemMenu.AddItem(new MenuItem("ElLucian.Items.Youmuu", "Use Youmuu's Ghostblade").SetValue(true));
                itemMenu.AddItem(new MenuItem("ElLucian.Items.Cutlass", "Use Cutlass").SetValue(true));
                itemMenu.AddItem(new MenuItem("ElLucian.Items.Blade", "Use Blade of the Ruined King").SetValue(true));
                itemMenu.AddItem(new MenuItem("ElLucian.Harasssfsddass.E", ""));
                itemMenu.AddItem(new MenuItem("ElLucian.Items.Blade.EnemyEHP", "Enemy HP Percentage").SetValue(new Slider(80, 100, 0)));
                itemMenu.AddItem(new MenuItem("ElLucian.Items.Blade.EnemyMHP", "My HP Percentage").SetValue(new Slider(80, 100, 0)));
            }
            Menu.AddSubMenu(itemMenu);


            var setMenu = new Menu("Misc", "Misc");
  

            Menu.AddSubMenu(setMenu);

            //ElKalista.Misc
            var miscMenu = new Menu("Drawings", "Misc");
            miscMenu.AddItem(new MenuItem("ElLucian.Draw.off", "Turn drawings off").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElLucian.Draw.Q", "Draw Q").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElLucian.Draw.W", "Draw W").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElLucian.Draw.E", "Draw E").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElLucian.Draw.R", "Draw R").SetValue(new Circle()));

            Menu.AddSubMenu(miscMenu);

            //Here comes the moneyyy, money, money, moneyyyy
            var credits = new Menu("Credits", "jQuery");
            credits.AddItem(new MenuItem("ElKalista.Paypal", "if you would like to donate via paypal:"));
            credits.AddItem(new MenuItem("ElKalista.Email", "info@zavox.nl"));
            Menu.AddSubMenu(credits);

            Menu.AddItem(new MenuItem("422442fsaafs4242f", ""));
            Menu.AddItem(new MenuItem("422442fsaafsf", (string.Format("ElKalista by jQuery v{0}", Lucian.ScriptVersion))));
            Menu.AddItem(new MenuItem("fsasfafsfsafsa", "Made By jQuery"));

            Menu.AddToMainMenu();

            Console.WriteLine("Menu Loaded");
        }
    }
}