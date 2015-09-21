using System.Reflection;

namespace VHR_SDK.Utility
{
    using System.Windows.Forms;
    using LeagueSharp.SDK.Core.Enumerations;
    using LeagueSharp.SDK.Core.UI.IMenu.Values;
    using Menu = LeagueSharp.SDK.Core.UI.IMenu.Menu;

    class MenuGenerator
    {
        public static void SetUp()
        {
            VHR.VHRMenu = new Menu("VayneHunter Reborn", "VHR", true);

            #region Modes Menu
            var comboMenu = new Menu("dz191.vhr.orbwalk", "[VHR] Combo"); //Done
            {
                comboMenu.Add(new MenuBool("UseQ", "Use Q", true));
                comboMenu.Add(new MenuBool("UseE", "Use E", true));
                comboMenu.Add(new MenuBool("UseR", "Use R", true));
                comboMenu.Add(new MenuSlider("rMinEnemies", "Min R Enemies", 2, 1, 5));
                comboMenu.Add(new MenuSeparator("sepCombo", "Mana Manager"));
                comboMenu.Add(new MenuSlider("QMana", "Q Mana"));
                comboMenu.Add(new MenuSlider("EMana", "E Mana"));
                comboMenu.Add(new MenuSlider("RMana", "R Mana"));
            }
            VHR.VHRMenu.Add(comboMenu);

            var harassMenu = new Menu("dz191.vhr.hybrid", "[VHR] Harass"); //Done
            {
                harassMenu.Add(new MenuBool("UseQ", "Use Q", true));
                harassMenu.Add(new MenuBool("UseE", "Use E", true));
                harassMenu.Add(new MenuBool("eThird", "E for 3rd proc"));

                harassMenu.Add(new MenuSeparator("sepHarass", "Mana Manager"));
                harassMenu.Add(new MenuSlider("QMana", "Q Mana", 25));
                harassMenu.Add(new MenuSlider("EMana", "E Mana", 20));
            }
            VHR.VHRMenu.Add(harassMenu);

            var farmMenu = new Menu("dz191.vhr.laneclear", "[VHR] Farm"); //Done, sort of.
            {
                farmMenu.Add(new MenuBool("UseQ", "Use Q", true));
                farmMenu.Add(new MenuSeparator("sepFarm", "Mana Manager"));
                farmMenu.Add(new MenuSlider("QMana", "Q Mana", 40));
            }
            VHR.VHRMenu.Add(farmMenu);
            #endregion

            #region Misc Menu
            var miscMenu = new Menu("dz191.vhr.misc", "[VHR] Misc");
            var miscQMenu = new Menu("dz191.vhr.misc.tumble", "Tumble");
            {
                miscQMenu.Add(new MenuSeparator("sepQ1", "Q Mode Settings"));
                miscQMenu.Add(new MenuList<string>("qlogic", "Q Logic", new[] { "Normal", "Kite Melees when Possible" })); //Done

                miscQMenu.Add(new MenuSeparator("sepQ2", "Q Safety Checks")); //Done
                miscQMenu.Add(new MenuBool("noqenemies", "Don't Q into enemies", true)); //Done
                miscQMenu.Add(new MenuBool("dynamicqsafety", "Dynamic Q Safety Distance")); //Done
                miscQMenu.Add(new MenuBool("limitQ", "Limit Q")); //Done
                miscQMenu.Add(new MenuBool("qspam", "Ignore Q checks")); //Done

                miscQMenu.Add(new MenuSeparator("sepQ3", "Q Integration"));
                miscQMenu.Add(new MenuBool("smartq", "Try to QE First")); //Done
                miscQMenu.Add(new MenuBool("noaastealth", "Don't AA while stealthed")); //TODO Disabled?

                miscQMenu.Add(new MenuSeparator("sepQ4", "Q Miscellaneous"));
                miscQMenu.Add(new MenuBool("qinrange", "Q To KS if Enemy Killable", true)); //Done
                miscQMenu.Add(new MenuBool("qburst", "Q Wall Burst (Mirin Mode!)", true)); //Done
                miscQMenu.Add(new MenuKeyBind("walltumble", "Tumble Over Wall (WallTumble)", Keys.Y, KeyBindType.Press));
            }

            var miscEMenu = new Menu("dz191.vhr.misc.condemn", "Condemn"); //All Done
            {
                miscEMenu.Add(new MenuSeparator("sepE1", "E Mode Settings"));
                miscEMenu.Add(new MenuList<string>("condemnmethod", "Condemn Method", new[] { "VHR SDK", "Marksman/Gosu"}));
                
                miscEMenu.Add(new MenuSeparator("sepE2", "E Prediction Settings (Only VHR Method)"));
                miscEMenu.Add(new MenuSlider("predictionNumber", "Number of Predictions (Higher = Laggier)", 10, 1, 15));
                miscEMenu.Add(new MenuSlider("accuracy", "Condemn Accuracy", 50));
                miscEMenu.Add(new MenuSlider("nextprediction", "Last Prediction Time (Don't touch unless you know what you're doing)", 500, 0, 1000));
                ////miscEMenu.Add(new MenuList<string>("enemymode", "Enemy Bounding Box", new[] { "Circle", "Point"}));

                miscEMenu.Add(new MenuSeparator("sepE3", "E Skill Settings (Both)"));
                miscEMenu.Add(new MenuSlider("pushdistance", "E Push Distance", 460, 350, 470)); //Done
                miscEMenu.Add(new MenuBool("onlystuncurrent", "Only stun current target")); //Done
                miscEMenu.Add(new MenuSlider("noeaa", "Don't E if Target can be killed in X AA (0 = Always E)", 1, 0, 4)); //Done
                miscEMenu.Add(new MenuBool("condemnflag", "Condemn to J4 flag", true)); //Done
                miscEMenu.Add(new MenuBool("noeturret", "No E Under enemy turret", true)); //Done

                miscEMenu.Add(new MenuSeparator("sepE4", "E Miscellaneous (Both)"));
                miscEMenu.Add(new MenuKeyBind("enextauto", "E Next Auto", Keys.T, KeyBindType.Toggle)); //Done
                miscEMenu.Add(new MenuBool("autoe", "Auto E")); //Done
                miscEMenu.Add(new MenuBool("eks", "Smart E Ks")); //Done
                miscEMenu.Add(new MenuBool("trinketbush", "Trinket Bush on Condemn", true)); //Done
                miscEMenu.Add(new MenuBool("lowlifepeel", "Peel with E when low")); //Done

            }

            var miscGeneralSubMenu = new Menu("dz191.vhr.misc.general", "General");
            {
                miscGeneralSubMenu.Add(new MenuSeparator("sepGeneral1", "AntiGP & Interrupter"));
                miscGeneralSubMenu.Add(new MenuBool("antigp", "Anti Gapcloser", true)); //Done
                miscGeneralSubMenu.Add(new MenuBool("interrupt", "Interrupter", true)); //Done
                miscGeneralSubMenu.Add(new MenuSlider("antigpdelay", "Anti Gapcloser Delay (ms)", 0, 0, 1000)); //Done

                miscGeneralSubMenu.Add(new MenuSeparator("sepGeneral2", "Various"));
                miscGeneralSubMenu.Add(new MenuBool("specialfocus", "Focus targets with 2 W marks")); //Done
                miscGeneralSubMenu.Add(new MenuBool("reveal", "Stealth Reveal (Pink Ward)")); //Done
                miscGeneralSubMenu.Add(new MenuBool("disablemovement", "Disable Orbwalker Movement")); //Done
                miscGeneralSubMenu.Add(new MenuBool("disableaa", "Disable Orbwalker AA")); //Done

                miscGeneralSubMenu.Add(new MenuSeparator("sepGeneral3", "Performance"));
                miscGeneralSubMenu.Add(new MenuBool("lightweight", "Lightweight mode")); //Semi-Done
                ////TODO When Lightweight is enabled then double all the intervals in the TickLimiters.
            }

            miscMenu.Add(miscQMenu);
            miscMenu.Add(miscEMenu);
            miscMenu.Add(miscGeneralSubMenu);
            VHR.VHRMenu.Add(miscMenu);

            VHR.VHRMenu.Add(
                new MenuSeparator(
                    "sepVersion", "VH Revolution by Asuna v." + Assembly.GetExecutingAssembly().GetName().Version));
            #endregion

            VHR.VHRMenu.Attach();
        }
    }
}
