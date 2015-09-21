using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using TheCassiopeia.Commons;
using TheCassiopeia.Commons.ComboSystem;
using TheCassiopeia.Commons.Debug;
using Color = System.Drawing.Color;

namespace TheCassiopeia
{
    class Cassiopeia
    {
        public void Load(EventArgs eArgs)
        {
            if (ObjectManager.Player.ChampionName != "Cassiopeia") return;

            //ItemSeraphsEmbrace
            var mainMenu = new Menu("The Cassiopeia", "TheCassiopeia", true);
            var orbwalkerMenu = mainMenu.CreateSubmenu("Orbwalker");
            var targetselectorMenu = mainMenu.CreateSubmenu("Target Selector");
            var comboMenu = mainMenu.CreateSubmenu("Combo");
            var ultMenu = mainMenu.CreateSubmenu("Ultimate Settings");
            var harassMenu = mainMenu.CreateSubmenu("Harass");
            var laneclearMenu = mainMenu.CreateSubmenu("Lane Clear");
            var lasthitMenu = mainMenu.CreateSubmenu("Last Hit");
            var burstmodeMenu = mainMenu.CreateSubmenu("Mode: Burst");
            var lanepressureMenu = mainMenu.CreateSubmenu("Mode: Lane Pressure");

            var gapcloserMenu = mainMenu.CreateSubmenu("Gapcloser");
            // var interrupterMenu = mainMenu.CreateSubmenu("Interrupter");
            var manamanagerMenu = mainMenu.CreateSubmenu("Mana Manager");
            var miscMenu = mainMenu.CreateSubmenu("Miscellaneous");
            var summonerMenu = mainMenu.CreateSubmenu("Summoners");
            var itemMenu = mainMenu.CreateSubmenu("Items");
            var drawingMenu = mainMenu.CreateSubmenu("Drawing");
            var autolevelMenu = mainMenu.CreateSubmenu("Auto-Level Spells");
            var infoMenu = mainMenu.CreateSubmenu("Info");

            var orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            TargetSelector.AddToMenu(targetselectorMenu);

            var provider = new CassioCombo(1000, orbwalker, new CassQ(SpellSlot.Q), new CassW(SpellSlot.W), new CassE(SpellSlot.E), new CassR(SpellSlot.R));

            provider.CreateBasicMenu(comboMenu, harassMenu, laneclearMenu, gapcloserMenu, null, manamanagerMenu, summonerMenu, itemMenu, drawingMenu, false);
            provider.CreateAutoLevelMenu(autolevelMenu, ComboProvider.SpellOrder.RQEEW, ComboProvider.SpellOrder.REQW);

            ultMenu.AddMItem("NOTE: Uses R if ANY conditions apply.");

            if (HeroManager.Enemies.Count >= 3)
            {
                ultMenu.AddMItem("Min Enemies (Facing)", new Slider(2, 1, HeroManager.Enemies.Count), (sender, args) => provider.GetSkill<CassR>().MinTargetsFacing = args.GetNewValue<Slider>().Value);
                ultMenu.AddMItem("Min Enemies (Not Facing)", new Slider(HeroManager.Enemies.Count - 1, 1, HeroManager.Enemies.Count), (sender, args) => provider.GetSkill<CassR>().MinTargetsNotFacing = args.GetNewValue<Slider>().Value);

            }
            else
            {
                ultMenu.AddMItem("Min Enemies (Facing)", new Slider(1, 1, HeroManager.Enemies.Count), (sender, args) => provider.GetSkill<CassR>().MinTargetsFacing = args.GetNewValue<Slider>().Value);
                ultMenu.AddMItem("Min Enemies (Not Facing)", new Slider(1, 1, HeroManager.Enemies.Count), (sender, args) => provider.GetSkill<CassR>().MinTargetsNotFacing = args.GetNewValue<Slider>().Value);
            }

            ultMenu.AddMItem("Do Above Only in Combo", true, (sender, args) => provider.GetSkill<CassR>().MinEnemiesOnlyInCombo = args.GetNewValue<bool>());
            ultMenu.AddMItem("Ult if Target Killable with Combo", true, (sender, args) => provider.GetSkill<CassR>().UltOnKillable = args.GetNewValue<bool>());
            ultMenu.AddMItem("Only R if Target has Health % > Than", new Slider(30), (sender, args) => provider.GetSkill<CassR>().MinHealth = args.GetNewValue<Slider>().Value);
            ultMenu.AddMItem("Block R that Won't Hit", false, (sender, args) => provider.BlockBadUlts = args.GetNewValue<bool>());
            ultMenu.AddMItem("Range", new Slider(700, 400, 825), (sender, args) => provider.GetSkill<CassR>().Range = args.GetNewValue<Slider>().Value);
            provider.AssistedUltMenu = ultMenu.AddMItem("Assisted Ult", new KeyBind(82, KeyBindType.Press));

            ultMenu.ProcStoredValueChanged<Slider>();
            ultMenu.ProcStoredValueChanged<bool>();

            harassMenu.AddMItem("Auto Harass", false, (sender, args) => provider.GetSkill<CassQ>().AutoHarass = args.GetNewValue<bool>());
            harassMenu.AddMItem("Min Mana %", new Slider(60), (sender, args) => provider.GetSkill<CassQ>().AutoHarassMana = args.GetNewValue<Slider>().Value);
            harassMenu.ProcStoredValueChanged<bool>();
            harassMenu.ProcStoredValueChanged<Slider>();


            burstmodeMenu.AddMItem("[Burst Mode = Full DPS mode, replaces Combo when Enabled]");
            var burstMode = provider.BurstMode = burstmodeMenu.AddMItem("Burst Mode Enabled", new KeyBind(78, KeyBindType.Toggle));
            provider.GetSkill<CassR>().BurstMode = burstMode;
            burstMode.Permashow(customdisplayname: "Burst Mode");
            burstmodeMenu.AddMItem("Auto-Burst Mode if My Health % < Than ", new Slider(25), (sender, args) => provider.GetSkill<CassR>().PanicModeHealth = args.GetNewValue<Slider>().Value);
            burstmodeMenu.AddMItem("Use Ignite in Burst Mode", false, (sender, args) => provider.IgniteInBurstMode = args.GetNewValue<bool>());
            burstmodeMenu.AddMItem("Ignite Only if E on Cooldown", false, (sender, args) => provider.OnlyIgniteWhenNoE = args.GetNewValue<bool>());
            burstmodeMenu.ProcStoredValueChanged<Slider>();
            burstmodeMenu.ProcStoredValueChanged<bool>();

            comboMenu.AddMItem("Only Kill non-Poisoned with E if No Other Enemies Nearby", false, (sender, args) => provider.GetSkill<CassE>().OnlyKillNonPIn1V1 = args.GetNewValue<bool>());
            comboMenu.AddMItem("Fast Combo (Small Chance to E non-Poisoned)", true, (sender, args) => provider.GetSkill<CassQ>().FastCombo = args.GetNewValue<bool>());
            //comboMenu.AddMItem("Risky mode (uses fast combo often, but more fails)", false, (sender, args) => provider.GetSkill<CassQ>().RiskyCombo = args.GetNewValue<bool>());
            comboMenu.AddMItem("AA in Combo (Disable for Better Kiting)", true, (sender, args) => provider.AutoInCombo = args.GetNewValue<bool>());
            comboMenu.ProcStoredValueChanged<bool>();

            var stackTearItem = miscMenu.AddMItem("Stack Tear", new KeyBind(77, KeyBindType.Toggle, true));
            miscMenu.AddMItem("NOTE: Will only stack when no enemies nearby.");
            provider.GetSkill<CassQ>().StackTear = stackTearItem;
            stackTearItem.Permashow();
            miscMenu.AddMItem("Min Mana % for Tear Stacking", new Slider(90), (sender, args) => provider.GetSkill<CassQ>().MinTearStackMana = args.GetNewValue<Slider>().Value);
            miscMenu.AddMItem("Make Poison Influence Target Selection", true, (sender, args) => provider.EnablePoisonTargetSelection = args.GetNewValue<bool>());
            miscMenu.ProcStoredValueChanged<Slider>();
            miscMenu.ProcStoredValueChanged<bool>();

            miscMenu.AddMItem("Enable this if you are Hawk", false, (sender, args) =>
            {
                ObjectManager.Player.SetSkin(args.GetNewValue<bool>() ? "Trundle" : "Cassiopeia", 0);
                Utility.DelayAction.Add(1000, () => Game.Say("/laugh"));
            });

            Circle q = new Circle(true, Color.GreenYellow), e = new Circle(false, Color.Red);

            drawingMenu.AddMItem("Q Range", q, (sender, args) => q = args.GetNewValue<Circle>());
            drawingMenu.AddMItem("E Range", e, (sender, args) => e = args.GetNewValue<Circle>());
            drawingMenu.ProcStoredValueChanged<Circle>();
            drawingMenu.ProcStoredValueChanged<bool>();

            gapcloserMenu.AddMItem("Use R if My Health % < Than", new Slider(40), (sender, args) => provider.GetSkill<CassR>().GapcloserUltHp = args.GetNewValue<Slider>().Value);
            gapcloserMenu.AddMItem("Otherwise Use W Instead", true, (sender, args) => provider.GetSkill<CassW>().UseOnGapcloser = args.GetNewValue<bool>());

            lasthitMenu.AddMItem("Use E on Poisoned", true, (sender, args) => provider.GetSkill<CassE>().Farm = args.GetNewValue<bool>());
            //lasthitMenu.AddMItem("Lasthit assist", true, (sender, args) => provider.GetSkill<CassE>().FarmAssist = args.GetNewValue<bool>());
            lasthitMenu.AddMItem("Use E on non-Poisoned if Mana % < Than", new Slider(50), (sender, args) => provider.GetSkill<CassE>().FarmNonPoisonedPercent = args.GetNewValue<Slider>().Value);
            lasthitMenu.ProcStoredValueChanged<bool>();
            lasthitMenu.ProcStoredValueChanged<Slider>();


            var lanepressureEnabled = lanepressureMenu.AddMItem("Enabled", new KeyBind(84, KeyBindType.Toggle));
            provider.LanepressureMenu = lanepressureEnabled;
            provider.GetSkill<CassQ>().LanepressureMenu = lanepressureEnabled;
            lanepressureEnabled.Permashow(customdisplayname: "Lane Pressure Mode");
            lanepressureMenu.AddMItem("NOTE: Overrides Lane Clear when active.");
            lanepressureMenu.AddMItem("NOTE: Uses Harass & Last Hit while pushing with AA.");
            lanepressureMenu.AddMItem("NOTE: All Harass & Last Hit settings apply to it.");
            lanepressureMenu.AddMItem("Use Q on Minions if E Ready", true, (sender, args) => provider.GetSkill<CassQ>().Farm = args.GetNewValue<bool>());
            lanepressureMenu.AddMItem("Only Q if Mana % > Than", new Slider(60), (sender, args) => provider.GetSkill<CassQ>().FarmIfHigherThan = args.GetNewValue<Slider>().Value);
            lanepressureMenu.AddMItem("Only Q if Min. Minions: ", new Slider(3, maxValue: 6), (sender, args) => provider.GetSkill<CassQ>().FarmIfMoreOrEqual = args.GetNewValue<Slider>().Value);


            infoMenu.AddMItem("TheCassiopeia - by TheNinow");
            infoMenu.AddMItem("Please give me feedback (on joduska.me) so I can improve this assembly!");
            infoMenu.AddMItem("Also, if you like this assembly, feel free to reward me with an upvote :)");

            mainMenu.AddToMainMenu();
            provider.Initialize();
            provider.GetSkill<CassQ>().GetPrediction(ObjectManager.Player); // Initializing the new prediction settings

            //   DevAssistant.Init();

            Game.OnUpdate += (args) => provider.Update();

            Drawing.OnDraw += (args) =>
            {
                if (q.Active)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, 850, q.Color);
                if (e.Active)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, 700, e.Color);


             //   Drawing.DrawText(200, 600, Color.Green, "Valid target: " + HeroManager.Enemies.FirstOrDefault().IsValidTarget().ToString() + " invulnerable: " + TargetSelector.IsInvulnerable(HeroManager.Enemies.FirstOrDefault(), TargetSelector.DamageType.Magical));
            };
        }



    }
}
