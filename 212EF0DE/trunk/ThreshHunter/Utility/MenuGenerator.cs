using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;

namespace ThreshHunter.Utility
{
    class MenuGenerator
    {
        public static void Init(Menu RootMenu)
        {
            var owMenu = new Menu("[TH] Orbwalker","dz191.thresh.orbwalker");
            Thresh.Orbwalker = new Orbwalking.Orbwalker(owMenu);
            RootMenu.AddSubMenu(owMenu);

            var TSMenu = new Menu("[TH] Target Selector", "dz191.thresh.ts");
            TargetSelector.AddToMenu(TSMenu);

            var ComboMenu = new Menu("[TH] Combo", "dz191.thresh.combo");
            {
                ComboMenu.AddItem(new MenuItem("dz191.thresh.combo.useQ", "Use Q").SetValue(true));
                ComboMenu.AddItem(new MenuItem("dz191.thresh.combo.useW", "Use W").SetValue(true));
                ComboMenu.AddItem(new MenuItem("dz191.thresh.combo.useE", "Use E").SetValue(true));
                ComboMenu.AddItem(new MenuItem("dz191.thresh.combo.useR", "Use R").SetValue(true));

                var ComboOptionsMenu = new Menu("Combo - Options","dz191.thresh.combo.settings");
                {
                    ComboOptionsMenu.AddItem(new MenuItem("dz191.thresh.combo.settings.minr", "Min Enemies for R").SetValue(new Slider(2, 1, 5)));
                    ComboOptionsMenu.AddItem(new MenuItem("dz191.thresh.combo.settings.pullebox", "Use E to pull inside box").SetValue(true));
                    ComboOptionsMenu.AddItem(new MenuItem("dz191.thresh.combo.settings.q2", "Use Q2 (Q to go in)").SetValue(false));

                    ComboMenu.AddSubMenu(ComboOptionsMenu);
                }
                RootMenu.AddSubMenu(ComboMenu);
            }

            var HarassMenu = new Menu("[TH] Harass", "dz191.thresh.harass");
            {
                HarassMenu.AddItem(new MenuItem("dz191.thresh.harass.useQ", "Use Q").SetValue(true));
                HarassMenu.AddItem(new MenuItem("dz191.thresh.harass.useE", "Use E").SetValue(true));
                HarassMenu.AddItem(new MenuItem("dz191.thresh.harass.mm", "Min Mana % for Harass").SetValue(new Slider(50)));

                RootMenu.AddSubMenu(HarassMenu);
            }

            var LanternMenu = new Menu("[TH] Lantern Options", "dz191.thresh.lantern");
            {
                var LanternAllies = new Menu("Use Lantern for", "dz191.thresh.lantern.allies");
                {
                    if (HeroManager.Allies.Any())
                    {
                        foreach (var ally in HeroManager.Allies)
                        {
                            LanternAllies.AddItem(
                                new MenuItem(
                                    "dz191.thresh.lantern.allies." + ally.ChampionName.ToLowerInvariant(),
                                    ally.ChampionName).SetValue(true));
                        }

                        LanternMenu.AddSubMenu(LanternAllies);

                    }
                }
                var LanternOptions = new Menu("Lantern Ally Settings","dz191.thresh.lantern.settings");
                {
                    LanternOptions.AddItem(new MenuItem("dz191.thresh.lantern.settings.lanternhealth", "Use Lantern if Ally Health < %").SetValue(new Slider(20)));
                    LanternOptions.AddItem(new MenuItem("dz191.thresh.lantern.settings.minene", "And Enemies Around >=").SetValue(new Slider(2, 1, 5)));
                    LanternOptions.AddItem(new MenuItem("dz191.thresh.lantern.settings.separator", " "));
                    LanternOptions.AddItem(new MenuItem("dz191.thresh.lantern.settings.cc", "Use Lantern on CC").SetValue(true));
                    LanternOptions.AddItem(new MenuItem("dz191.thresh.lantern.settings.minene", "If CC Numbers >=").SetValue(new Slider(2, 1, 7)));
                    LanternMenu.AddSubMenu(LanternOptions);
                }

                LanternMenu.AddItem(new MenuItem("dz191.thresh.lantern.autosave", "Auto Save Allies with Lantern").SetValue(true));
                LanternMenu.AddItem(new MenuItem("dz191.thresh.lantern.throw", "Throw Lantern").SetValue(new KeyBind("S".ToCharArray()[0], KeyBindType.Press)));
                LanternMenu.AddItem(new MenuItem("dz191.thresh.lantern.priority", "Priority").SetValue(new StringList(new []{"Closest", "Lowest Health"}, 1)));

                RootMenu.AddSubMenu(LanternMenu);
            }

            var MiscMenu = new Menu("[TH] Miscellaneous", "dz191.thresh.misc");
            {
                MiscMenu.AddItem(new MenuItem("dz191.thresh.misc.antigp", "AntiGapcloser").SetValue(true));
                MiscMenu.AddItem(new MenuItem("dz191.thresh.misc.interrupter.q", "Interrupter Q").SetValue(true));
                MiscMenu.AddItem(new MenuItem("dz191.thresh.misc.interrupter.e", "Interrupter E").SetValue(true));
                MiscMenu.AddItem(new MenuItem("dz191.thresh.misc.emode", "E Mode").SetValue(new StringList(new[] { "Push", "Pull" }, 1)));

                RootMenu.AddSubMenu(MiscMenu);
            }

            RootMenu.AddToMainMenu();
        }
    }
}
