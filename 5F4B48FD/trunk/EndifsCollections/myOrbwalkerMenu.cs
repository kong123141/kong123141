using System;
using EndifsCollections.Controller;
using EndifsCollections.SummonerSpells;
using EndifsCollections.Tools;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace EndifsCollections
{
    class myOrbwalkerMenu
    {
        private static Menu config;
        private static Menu plugins;
        public myOrbwalkerMenu()
        {
            InitializeTools();
            config = new Menu("Endif's myOrbwalker", "", true);
            var myorb = new Menu("myOrbwalker", "myOrbwalker");
            {
                myOrbwalker.AddToMenu(myorb);
                config.AddSubMenu(myorb);
            }
            var ts = new Menu("Target Selector", "Target Selector");
            {
                TargetSelector.AddToMenu(ts);
                config.AddSubMenu(ts);
            }
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("UseItemCombo", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            config.AddToMainMenu();

            plugins = new Menu("Endif's Plugins", "EndifsPlugins", true);
            var ss = new Menu("Summoner Spells", "Summoner Spells");
            {
                mySmiter.AddToMenu(ss);
                myMarkDash.AddToMenu(ss);
                myIgniter.AddToMenu(ss);
                plugins.AddSubMenu(ss);
            }
            plugins.AddToMainMenu();
            Game.OnUpdate += OnUpdate;
        }
        private static void InitializeTools()
        {
            new myRePriority();
            new mySmiter();
            new myMarkDash();
            new myIgniter();
        }
        private static void OnUpdate(EventArgs args)
        {
            switch (myOrbwalker.ActiveMode)
            {
                case myOrbwalker.OrbwalkingMode.Combo:
                    Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(ObjectManager.Player.AttackRange, TargetSelector.DamageType.Physical);
                    if (target.IsValidTarget() && config.Item("UseItemCombo").GetValue<bool>())
                    {
                        myUtility.UseItems(0, target);
                        if (Vector3.Distance(ObjectManager.Player.ServerPosition, target.ServerPosition) <= 450f)
                        {
                            myUtility.UseItems(1, target);
                        }
                        if (Vector3.Distance(ObjectManager.Player.ServerPosition, target.ServerPosition) <= 250)
                        {
                            myUtility.UseItems(2, null);
                        }
                        if (Vector3.Distance(ObjectManager.Player.ServerPosition, target.ServerPosition) < 500f)
                        {
                            myUtility.UseItems(3, null);
                        }
                    }
                    break;
            }
        }
    }
}