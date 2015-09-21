namespace ElUtilitySuite
{
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    public class InitializeMenu
    {
        #region Static Fields

        public static Menu Menu, _mainMenu;

        #endregion


        public static void Load()
        {

            Menu = new Menu("ElUtilitySuite", "ElUtilitySuite", true);

            if (Smite.smiteSlot != SpellSlot.Unknown)
            {
                var smiteMenu = Menu.AddSubMenu(new Menu("Smite", "Smite"));
                {
                    //Important smites
                    smiteMenu.AddItem(
                        new MenuItem("ElSmite.Activated", "Activated").SetValue(
                            new KeyBind("M".ToCharArray()[0], KeyBindType.Toggle, true)));
                    if (Entry.IsSummonersRift)
                    {
                        smiteMenu.AddItem(new MenuItem("SRU_Dragon", "Dragon").SetValue(true));
                        smiteMenu.AddItem(new MenuItem("SRU_Baron", "Baron").SetValue(true));
                        smiteMenu.AddItem(new MenuItem("SRU_Red", "Red buff").SetValue(true));
                        smiteMenu.AddItem(new MenuItem("SRU_Blue", "Blue buff").SetValue(true));

                        //Bullshit smites
                        smiteMenu.AddItem(new MenuItem("SRU_Gromp", "Gromp").SetValue(false));
                        smiteMenu.AddItem(new MenuItem("SRU_Murkwolf", "Wolves").SetValue(false));
                        smiteMenu.AddItem(new MenuItem("SRU_Krug", "Krug").SetValue(false));
                        smiteMenu.AddItem(new MenuItem("SRU_Razorbeak", "Chicken camp").SetValue(false));
                        smiteMenu.AddItem(new MenuItem("Sru_Crab", "Crab").SetValue(false));
                    }

                    if (Entry.IsTwistedTreeline)
                    {
                        smiteMenu.AddItem(new MenuItem("TT_Spiderboss", "Vilemaw Enabled").SetValue(true));
                        smiteMenu.AddItem(new MenuItem("TT_NGolem", "Golem Enabled").SetValue(true));
                        smiteMenu.AddItem(new MenuItem("TT_NWolf", "Wolf Enabled").SetValue(true));
                        smiteMenu.AddItem(new MenuItem("TT_NWraith", "Wraith Enabled").SetValue(true));
                    }

                    //Killsteal submenu
                    smiteMenu.SubMenu("Killsteal")
                        .AddItem(new MenuItem("ElSmite.KS.Activated", "Use smite to killsteal").SetValue(true));

                    //Drawings
                    smiteMenu.SubMenu("Drawings")
                        .AddItem(new MenuItem("ElSmite.Draw.Range", "Draw smite Range").SetValue(new Circle()));
                    smiteMenu.SubMenu("Drawings")
                        .AddItem(new MenuItem("ElSmite.Draw.Text", "Draw smite text").SetValue(true));
                    smiteMenu.SubMenu("Drawings")
                        .AddItem(new MenuItem("ElSmite.Draw.Damage", "Draw smite Damage").SetValue(true));
                }
            }

            if (Entry.Player.GetSpellSlot("summonerheal") != SpellSlot.Unknown)
            {
                var healMenu = Menu.AddSubMenu(new Menu("Heal", "Heal"));
                {
                    healMenu.AddItem(new MenuItem("Heal.Activated", "Heal").SetValue(true));
                    healMenu.AddItem(new MenuItem("Heal.Predicted", "Predict damage").SetValue(true));
                    healMenu.AddItem(new MenuItem("Heal.HP", "Health percentage").SetValue(new Slider(20, 1)));
                }
            }

            if (Entry.Player.GetSpellSlot("summonerdot") != SpellSlot.Unknown)
            {
                var igniteMenu = Menu.AddSubMenu(new Menu("Ignite", "Ignite"));
                {
                    igniteMenu.AddItem(new MenuItem("Ignite.Activated", "Ignite").SetValue(true));
                    igniteMenu.AddItem(new MenuItem("Ignite.shieldCheck", "Check for shields").SetValue(true));
                }
            }

            var potionsMenu = Menu.AddSubMenu(new Menu("Potions", "Potions"));
            {
                potionsMenu.AddItem(new MenuItem("Potions.Activated", "Potions activated").SetValue(true));
                potionsMenu.AddItem(new MenuItem("Potions.Health", "Health potions").SetValue(true));
                potionsMenu.AddItem(new MenuItem("Potions.Biscuit", "Biscuits").SetValue(true));
                potionsMenu.AddItem(new MenuItem("Potions.Mana", "Mana potions").SetValue(true));
                potionsMenu.AddItem(new MenuItem("Potions.Flask", "Crystalline Flask").SetValue(true));
                potionsMenu.AddItem(new MenuItem("seperator.Potions", ""));
                potionsMenu.AddItem(new MenuItem("Potions.Player.Health", "Health percentage").SetValue(new Slider(20)));
                potionsMenu.AddItem(new MenuItem("Potions.Player.Mana", "Mana percentage").SetValue(new Slider(20)));
            }

            var protectMenu = Menu.AddSubMenu(new Menu("Protect yourself", "ProtectYourself"));
            {
                protectMenu.SubMenu("Rengar")
                    .AddItem(new MenuItem("Protect.Rengar", "Rengar antigapcloser").SetValue(true));
                protectMenu.SubMenu("Rengar")
                    .AddItem(new MenuItem("Protect.Rengar.Lens", "Oracle's Lens").SetValue(true));
                protectMenu.SubMenu("Akali").AddItem(new MenuItem("Protect.Akali", "Autopink Akali W").SetValue(true));
                protectMenu.SubMenu("Akali").AddItem(new MenuItem("Protect.Akali.PinkWard", "Pinkward").SetValue(true));
                protectMenu.SubMenu("Akali").AddItem(new MenuItem("Protect.Akali.Trinket", "Trinket").SetValue(true));
                protectMenu.SubMenu("Akali")
                    .AddItem(new MenuItem("Protect.Akali.Sweeping", "Oracle's Lens").SetValue(true));
                protectMenu.SubMenu("Akali")
                    .AddItem(new MenuItem("Protect.Akali.HP", "Pink when Akali's HP:").SetValue(new Slider(50)));
            }

            var cleanseMenu = Menu.AddSubMenu(new Menu("Cleanse", "Cleanse"));
            {
                cleanseMenu.AddItem(new MenuItem("Cleanse.Activated", "Activated").SetValue(true));
                cleanseMenu.AddItem(new MenuItem("Cleanse.Delay", "Cleanse delay ")).SetValue(new Slider(0, 0, 25));

                cleanseMenu.SubMenu("Cleanse settings").SubMenu("Activated").AddItem(new MenuItem("Cleanse.Activated", "Activate Cleanse").SetValue(true));
                cleanseMenu.SubMenu("Cleanse settings").SubMenu("Buffs").AddItem(new MenuItem("Protect.Cleanse.Stun", "Stuns").SetValue(true));
                cleanseMenu.SubMenu("Cleanse settings").SubMenu("Buffs").AddItem(new MenuItem("Protect.Cleanse.Charm", "Charms").SetValue(true));
                cleanseMenu.SubMenu("Cleanse settings").SubMenu("Buffs").AddItem(new MenuItem("Protect.Cleanse.Taunt", "Taunts").SetValue(false));
                cleanseMenu.SubMenu("Cleanse settings").SubMenu("Buffs").AddItem(new MenuItem("Protect.Cleanse.Fear", "Fears").SetValue(false));
                cleanseMenu.SubMenu("Cleanse settings").SubMenu("Buffs").AddItem(new MenuItem("Protect.Cleanse.Snare", "Snares").SetValue(false));
                cleanseMenu.SubMenu("Cleanse settings").SubMenu("Buffs").AddItem(new MenuItem("Protect.Cleanse.Silence", "Silences").SetValue(true));
                cleanseMenu.SubMenu("Cleanse settings").SubMenu("Buffs").AddItem(new MenuItem("Protect.Cleanse.Suppression", "Suppressions").SetValue(true));
                cleanseMenu.SubMenu("Cleanse settings").SubMenu("Buffs").AddItem(new MenuItem("Protect.Cleanse.Polymorph", "Polymorphs").SetValue(false));
                cleanseMenu.SubMenu("Cleanse settings").SubMenu("Buffs").AddItem(new MenuItem("Protect.Cleanse.Blind", "Blinds").SetValue(false));
                cleanseMenu.SubMenu("Cleanse settings").SubMenu("Buffs").AddItem(new MenuItem("Protect.Cleanse.Slow", "Slows").SetValue(false));
                cleanseMenu.SubMenu("Cleanse settings").SubMenu("Buffs").AddItem(new MenuItem("Protect.Cleanse.Posion", "Posion").SetValue(false));

                foreach (var a in ObjectManager.Get<Obj_AI_Hero>().Where(ally => ally.Team == Entry.Player.Team))
                {
                    cleanseMenu.SubMenu("Mikaels settings").AddItem(new MenuItem("Protect.Cleanse.Kappa" + a.SkinName, "Use for " + a.SkinName)).SetValue(true);
                }

                cleanseMenu.SubMenu("Mikaels settings").SubMenu("Activated").AddItem(new MenuItem("Protect.Cleanse.Mikeals.Activated", "Activate Mikaels").SetValue(true));

                cleanseMenu.SubMenu("Mikaels settings").SubMenu("Buffs").AddItem(new MenuItem("Protect.Cleanse.Stun.Ally", "Stuns").SetValue(true));
                cleanseMenu.SubMenu("Mikaels settings").SubMenu("Buffs").AddItem(new MenuItem("Protect.Cleanse.Charm.Ally", "Charms").SetValue(true));
                cleanseMenu.SubMenu("Mikaels settings").SubMenu("Buffs").AddItem(new MenuItem("Protect.Cleanse.Taunt.Ally", "Taunts").SetValue(false));
                cleanseMenu.SubMenu("Mikaels settings").SubMenu("Buffs").AddItem(new MenuItem("Protect.Cleanse.Fear.Ally", "Fears").SetValue(false));
                cleanseMenu.SubMenu("Mikaels settings").SubMenu("Buffs").AddItem(new MenuItem("Protect.Cleanse.Snare.Ally", "Snares").SetValue(false));
                cleanseMenu.SubMenu("Mikaels settings").SubMenu("Buffs").AddItem(new MenuItem("Protect.Cleanse.Silence.Ally", "Silences").SetValue(true));
                cleanseMenu.SubMenu("Mikaels settings").SubMenu("Buffs").AddItem(new MenuItem("Protect.Cleanse.Suppression.Ally", "Suppressions").SetValue(true));
                cleanseMenu.SubMenu("Mikaels settings").SubMenu("Buffs").AddItem(new MenuItem("Protect.Cleanse.Polymorph.Ally", "Polymorphs").SetValue(false));
                cleanseMenu.SubMenu("Mikaels settings").SubMenu("Buffs").AddItem(new MenuItem("Protect.Cleanse.Blind.Ally", "Blinds").SetValue(false));
                cleanseMenu.SubMenu("Mikaels settings").SubMenu("Buffs").AddItem(new MenuItem("Protect.Cleanse.Slow.Ally", "Slows").SetValue(false));
                cleanseMenu.SubMenu("Mikaels settings").SubMenu("Buffs").AddItem(new MenuItem("Protect.Cleanse.Posion.Ally", "Posion").SetValue(false));

                cleanseMenu.AddItem(new MenuItem("cmode", "Mode: ")).SetValue(new StringList(new[] { "Always", "Combo" }, 1));
            }


            #region Credits to Oracle

            _mainMenu = Menu.AddSubMenu(new Menu("Offensives", "omenu"));

            foreach (var x in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy))
            {
                _mainMenu.SubMenu("Champion Settings").AddItem(new MenuItem("ouseOn" + x.SkinName, "Use for " + x.SkinName)).SetValue(true);
            }

            CreateMenuItem("Muramana", "Muramana", 90, 30, true);
            CreateMenuItem("Tiamat/Hydra", "Hydra", 90, 30);
            CreateMenuItem("Hextech Gunblade", "Hextech", 90, 30);
            CreateMenuItem("Youmuu's Ghostblade", "Youmuus", 90, 30);
            CreateMenuItem("Bilgewater's Cutlass", "Cutlass", 90, 30);
            CreateMenuItem("Blade of the Ruined King", "Botrk", 70, 70);
            CreateMenuItem("Frost Queen's Claim", "Frostclaim", 100, 30);

            #endregion

            var credits = Menu.AddSubMenu(new Menu("Credits", "jQuery"));
            {
                credits.AddItem(new MenuItem("Paypal", "if you would like to donate via paypal:"));
                credits.AddItem(new MenuItem("Email", "info@zavox.nl"));
            }

            Menu.AddItem(new MenuItem("seperator1", ""));

            Menu.AddItem(new MenuItem("usecombo", "Combo (Active)").SetValue(new KeyBind(32, KeyBindType.Press)));

            Menu.AddItem(new MenuItem("seperator", ""));
            Menu.AddItem(new MenuItem("Versionnumber", string.Format("Version: {0}", Entry.ScriptVersion)));
            Menu.AddItem(new MenuItem("by.jQuery", "Made By jQuery"));

            Menu.AddToMainMenu();
        }

        private static void CreateMenuItem(string displayname, string name, int evalue, int avalue, bool usemana = false)
        {
            var menuName = new Menu(name, name.ToLower());

            menuName.AddItem(new MenuItem("use" + name, "Use " + displayname)).SetValue(true);
            menuName.AddItem(new MenuItem("use" + name + "Pct", "Use on enemy HP %")).SetValue(new Slider(evalue));

            if (!usemana)
                menuName.AddItem(new MenuItem("use" + name + "Me", "Use on my HP %")).SetValue(new Slider(avalue));

            if (usemana)
                menuName.AddItem(new MenuItem("use" + name + "Mana", "Minimum mana % to use")).SetValue(new Slider(35));

            if (name == "Muramana")
                menuName.AddItem(
                    new MenuItem("muraMode", " Muramana Mode: ").SetValue(new StringList(new[] { "Always", "Combo" }, 1)));

            _mainMenu.AddSubMenu(menuName);
        }
    }
}