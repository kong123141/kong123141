using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ElZilean
{
    public class ZileanMenu
    {
        public static Menu _menu;

        public static void Initialize()
        {
            _menu = new Menu("ElZilean", "menu", true);

            //ElZilean.Orbwalker
            var orbwalkerMenu = new Menu("Orbwalker", "orbwalker");
            Zilean._orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            _menu.AddSubMenu(orbwalkerMenu);

            //ElZilean.TargetSelector
            var targetSelector = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);
            _menu.AddSubMenu(targetSelector);

            //ElZilean.Menu
            var comboMenu = _menu.AddSubMenu(new Menu("Combo", "Combo"));
            comboMenu.AddItem(new MenuItem("ElZilean.Combo.Q", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("ElZilean.Combo.E", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("ElZilean.Combo.W", "Use W to reset Q when target is marked").SetValue(true));
            comboMenu.AddItem(new MenuItem("ElZilean.Combo.Ignite", "Use Ignite").SetValue(true));
            comboMenu.AddItem(new MenuItem("ElZilean.hitChance", "Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 3)));
            comboMenu.AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            //ElZilean.Menu
            var harassMenu = _menu.AddSubMenu(new Menu("Harass", "Harass"));
            harassMenu.AddItem(new MenuItem("ElZilean.Harass.Q", "Use Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("ElZilean.Harass.E", "Use E").SetValue(true));
            harassMenu.AddItem(new MenuItem("ElZilean.hitChance", "Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High", "Very High" }, 3)));

            harassMenu.SubMenu("AutoHarass").AddItem(new MenuItem("ElZilean.AutoHarass", "[Toggle] Auto harass", false).SetValue(new KeyBind("U".ToCharArray()[0], KeyBindType.Toggle)));
            harassMenu.SubMenu("AutoHarass").AddItem(new MenuItem("spacespacespace", ""));
            harassMenu.SubMenu("AutoHarass").AddItem(new MenuItem("ElZilean.UseQAutoHarass", "Use Q").SetValue(true));
            harassMenu.SubMenu("AutoHarass").AddItem(new MenuItem("ElZilean.UseEAutoHarass", "Use E").SetValue(false));
            harassMenu.SubMenu("AutoHarass").AddItem(new MenuItem("spacespacespassce", ""));
            harassMenu.SubMenu("AutoHarass").AddItem(new MenuItem("ElZilean.harass.mana", "Min % mana for autoharass")).SetValue(new Slider(55));

            //ElZilean.Lanclear
            var clearMenu = _menu.AddSubMenu(new Menu("Laneclear", "LC"));
            clearMenu.AddItem(new MenuItem("ElZilean.Clear.Q", "Use Q").SetValue(true));
            clearMenu.AddItem(new MenuItem("ElZilean.Clear.W", "Use W to reset bomb").SetValue(true));

            //ElZilean.Ult
            var castUltMenu = _menu.AddSubMenu(new Menu("Ult settings", "ElZilean.Ally.Ult"));
            castUltMenu.AddItem(new MenuItem("ElZilean.useult", "Use ult on ally").SetValue(true));
            castUltMenu.AddItem(new MenuItem("ElZilean.Ally.HP", "Ally Health %")).SetValue(new Slider(25, 1, 100));
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly && !hero.IsMe))
                castUltMenu.AddItem(new MenuItem("ElZilean.Cast.Ult.Ally" + hero.BaseSkinName, hero.BaseSkinName).SetValue(true));

            castUltMenu.AddItem(new MenuItem("422442fsaasssfs4242f", ""));
            castUltMenu.AddItem(new MenuItem("ElZilean.R", "Cast R")).SetValue(true);
            castUltMenu.AddItem(new MenuItem("ElZilean.HP", "Self Health %")).SetValue(new Slider(25, 1, 100));

            //ElZilean.Misc
            var miscMenu = _menu.AddSubMenu(new Menu("Misc", "Misc"));
            miscMenu.AddItem(new MenuItem("ElZilean.Draw.off", "[Drawing] Drawings off").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElZilean.Draw.Q", "Draw Q").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElZilean.Draw.W", "Draw W").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElZilean.Draw.E", "Draw E").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElZilean.Draw.R", "Draw R").SetValue(new Circle()));

            //copied from esk0r Syndra // Beaving ahri :D
            var dmgAfterComboItem = new MenuItem("ElZilean.DrawComboDamage", "Draw combo damage").SetValue(true); 
            miscMenu.AddItem(dmgAfterComboItem);

            Utility.HpBarDamageIndicator.DamageToUnit = Zilean.GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                };
            //end copy

            miscMenu.AddItem(new MenuItem("ElZilean.SupportMode", "Support mode").SetValue(false));


            //ElZilean.SuperSecretSettings
            var SSSMenu = _menu.AddSubMenu(new Menu("Super Secret Settings", "SSS"));
            SSSMenu.AddItem(new MenuItem("FleeActive", "Flee").SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));

            //Here comes the moneyyy, money, money, moneyyyy
            var credits = _menu.AddSubMenu(new Menu("Credits", "jQuery"));
            credits.AddItem(new MenuItem("ElZilean.Paypal", "if you would like to donate via paypal:"));
            credits.AddItem(new MenuItem("ElZilean.Email", "info@zavox.nl"));

            _menu.AddItem(new MenuItem("422442fsaafs4242f", ""));
            _menu.AddItem(new MenuItem("422442fsaafsf", "Version: 1.0.1.5"));
            _menu.AddItem(new MenuItem("fsasfafsfsafsa", "Made By jQuery"));

            _menu.AddToMainMenu();

            Console.WriteLine("Menu Loaded");
        }
    }
}