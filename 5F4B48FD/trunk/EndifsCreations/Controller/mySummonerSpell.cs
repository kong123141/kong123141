#region Todo
    //      Everything Kappa
#endregion Todo

using System;
using System.Collections.Generic;
using System.Linq;
using EndifsCreations.Tools;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace EndifsCreations.Controller
{
    internal static class mySummonerSpell
    {
        public static SpellDataInst Slot1, Slot2;
        private static Spell IgniteSpell;
        public static String Menu1, Menu2;
        private static Menu spellmenu;
        private static SpellSlot SmiteSlot, IgniteSlot, BarrierSlot, HealSlot;        
        public static void AddToMenu(Menu menu)
        {
            spellmenu = menu;
            var subs = new Menu("Summoner Spells", "SummonerSpell");
            {                
                if (Menu1 != null)
                {
                    var slotmenu1 = new Menu(Menu1, Menu1);
                    {                        
                        if (Menu1 == "Smite" || Menu1 == "Ignite")
                        {
                            var Combo = new Menu("Combo Mode", "ComboMode");
                            {
                                Combo.AddItem(new MenuItem(ObjectManager.Player.ChampionName + ".EC." + Menu1 + ".Combo.Bool", "Enable").SetValue(false));
                                foreach (var enemy in HeroManager.Enemies)
                                {
                                    Combo.SubMenu("Whitelist").AddItem(new MenuItem("EC." + Menu1 + ".Whitelist." + enemy.NetworkId, enemy.CharData.BaseSkinName).SetValue(false));
                                }
                            }                            
                            slotmenu1.AddSubMenu(Combo);
                        }
                        if (Menu1 == "Barrier" || Menu1 == "Heal")
                        {
                            slotmenu1.AddItem(new MenuItem("EC." + Menu2 + ".Bool", "Enable").SetValue(false));
                        }
                        if (Menu1 == "Smite")
                        {
                            slotmenu1.SubMenu("Jungle Clear").AddItem(new MenuItem(ObjectManager.Player.ChampionName + ".EC.Smite.Jungle.Bool", "Enable").SetValue(false));
                            var MonsterList = new Menu("Whitelist", "Whitelist");
                            {
                                if (Game.MapId == GameMapId.SummonersRift)
                                {
                                    foreach (var x in myFarmManager.NeutralCampList.Where(i => i.Contains("SRU_")))
                                    {
                                        MonsterList.SubMenu(x).AddItem(new MenuItem("EC.Smite." + x + ".Bool", "Enable").SetValue(false));
                                        MonsterList.SubMenu(x).AddItem(new MenuItem("EC.Smite." + x + ".Index", "").SetValue(new StringList(new[] { "First", "Last" })));
                                    }
                                }
                                if (Game.MapId == GameMapId.TwistedTreeline)
                                {
                                    foreach (var x in myFarmManager.NeutralCampList.Where(i => i.Contains("TT_")))
                                    {
                                        MonsterList.SubMenu(x).AddItem(new MenuItem("EC.Smite." + x + ".Bool", "Enable").SetValue(false));
                                        MonsterList.SubMenu(x).AddItem(new MenuItem("EC.Smite." + x + ".Index", "").SetValue(new StringList(new[] { "First", "Last" })));
                                    }
                                }
                                slotmenu1.SubMenu("Jungle Clear").AddSubMenu(MonsterList);                                
                            }
                            slotmenu1.SubMenu("Lane Clear").AddItem(new MenuItem(ObjectManager.Player.ChampionName + ".EC.Smite.Lane.Bool", "Enable").SetValue(false));
                        }
                    }
                    subs.AddSubMenu(slotmenu1);
                }
                if (Menu2 != null)
                {
                    var slotmenu2 = new Menu(Menu2, Menu2);
                    {
                        if (Menu2 == "Smite" || Menu2 == "Ignite")
                        {
                            var Combo = new Menu("Combo Mode", "ComboMode");
                            {
                                Combo.AddItem(new MenuItem(ObjectManager.Player.ChampionName + ".EC." + Menu2 + ".Combo.Bool", "Enable").SetValue(false));
                                foreach (var enemy in HeroManager.Enemies)
                                {
                                    Combo.SubMenu("Whitelist").AddItem(new MenuItem("EC." + Menu2 + ".Whitelist." + enemy.NetworkId, enemy.CharData.BaseSkinName).SetValue(false));
                                }
                            }
                            slotmenu2.AddSubMenu(Combo);
                        }
                        if (Menu2 == "Barrier" || Menu2 == "Heal")
                        {
                            slotmenu2.AddItem(new MenuItem("EC." + Menu2 + ".Bool", "Enable").SetValue(false));
                        }
                        if (Menu2 == "Smite")
                        {
                            slotmenu2.SubMenu("Jungle Clear").AddItem(new MenuItem(ObjectManager.Player.ChampionName + ".EC.Smite.Jungle.Bool", "Enable").SetValue(false));
                            var MonsterList = new Menu("Whitelist", "Whitelist");
                            {
                                if (Game.MapId == GameMapId.SummonersRift)
                                {
                                    foreach (var x in myFarmManager.NeutralCampList.Where(i => i.Contains("SRU_")))
                                    {
                                        MonsterList.SubMenu(x).AddItem(new MenuItem("EC.Smite." + x + ".Bool", "Enable").SetValue(false));
                                        MonsterList.SubMenu(x).AddItem(new MenuItem("EC.Smite." + x + ".Index", "").SetValue(new StringList(new[] { "First", "Last" })));
                                    }
                                }
                                if (Game.MapId == GameMapId.TwistedTreeline)
                                {
                                    foreach (var x in myFarmManager.NeutralCampList.Where(i => i.Contains("TT_")))
                                    {
                                        MonsterList.SubMenu(x).AddItem(new MenuItem("EC.Smite.TT." + x + ".Bool", "Enable").SetValue(false));
                                        MonsterList.SubMenu(x).AddItem(new MenuItem("EC.Smite.TT." + x + ".Index", "").SetValue(new StringList(new[] { "First", "Last" })));
                                    }
                                }
                                slotmenu2.SubMenu("Jungle Clear").AddSubMenu(MonsterList);
                            }
                            slotmenu2.SubMenu("Lane Clear").AddItem(new MenuItem(ObjectManager.Player.ChampionName + ".EC.Smite.Lane.Bool", "Enable").SetValue(false));
                        }
                    }
                    subs.AddSubMenu(slotmenu2);
                }
            }
            menu.AddSubMenu(subs);
        }

        static mySummonerSpell()
        {
            Slot1 = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Summoner1);
            Slot2 = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Summoner2);

            switch (Slot1.Name.ToLower())
            {
                case "itemsmiteaoe":
                case "s5_summonersmiteduel":
                case "s5_summonersmiteplayerganker":
                case "s5_summonersmitequick":
                case "summonersmite":
                    SmiteSlot = SpellSlot.Summoner1;
                    Menu1 = "Smite";
                    break;
                case "summonerdot":
                    IgniteSlot = SpellSlot.Summoner1;
                    Menu1 = "Ignite";
                    break;
                case "summonerbarrier":
                    BarrierSlot = SpellSlot.Summoner1;
                    Menu1 = "Barrier";
                    break;
                case "summonerheal":
                    HealSlot = SpellSlot.Summoner1;
                    Menu1 = "Heal";
                    break;
            }
            switch (Slot2.Name.ToLower())
            {
                case "itemsmiteaoe":
                case "s5_summonersmiteduel":
                case "s5_summonersmiteplayerganker":
                case "s5_summonersmitequick":
                case "summonersmite":
                    SmiteSlot = SpellSlot.Summoner2;
                    Menu2 = "Smite";
                    break;
                case "summonerdot":
                    IgniteSlot = SpellSlot.Summoner1;
                    Menu2 = "Ignite";
                    break;
                case "summonerbarrier":
                    BarrierSlot = SpellSlot.Summoner1;
                    Menu2 = "Barrier";
                    break;
                case "summonerheal":
                    HealSlot = SpellSlot.Summoner1;
                    Menu2 = "Heal";
                    break;
            }

            Game.OnUpdate += OnUpdate;
            myCustomEvents.ProcessDamageBuffer += ProcessDamageBuffer;
        }

        #region Smite
        private static readonly string[] SmiteNames =
        {
            "s5_summonersmiteplayerganker", "itemsmiteaoe", "s5_summonersmitequick","s5_summonersmiteduel", "summonersmite"
        };
        private static int SmiteDamageMinions
        {
            get
            {
                if (ObjectManager.Player.Level <= 4)
                {
                    return 370 + 20 * ObjectManager.Player.Level;
                }
                if (ObjectManager.Player.Level > 4 && ObjectManager.Player.Level <= 9)
                {
                    return 450 + 30 * (ObjectManager.Player.Level - 4);
                }
                if (ObjectManager.Player.Level > 9 && ObjectManager.Player.Level <= 14)
                {
                    return 600 + 40 * (ObjectManager.Player.Level - 9);
                }
                if (ObjectManager.Player.Level > 14)
                {
                    return 800 + 50 * (ObjectManager.Player.Level - 14);
                }
                return 0;
            }
        }
        public static void Smites(Obj_AI_Base target)
        {
            if (Menu1 == "Smite" || Menu2 == "Smite")
            {
                if (target != null && target.IsValidTarget() && ObjectManager.Player.Spellbook.CanUseSpell(SmiteSlot) == SpellState.Ready)
                {
                    var inrange = Vector3.Distance(ObjectManager.Player.ServerPosition, target.ServerPosition) <= ObjectManager.Player.BoundingRadius + target.BoundingRadius + 500;
                    if (target is Obj_AI_Hero && spellmenu.Item(ObjectManager.Player.ChampionName + ".EC.Smite.Combo.Bool").GetValue<bool>())
                    {
                        ObjectManager.Player.Spellbook.CastSpell(SmiteSlot, target);
                    }
                    if (target is Obj_AI_Minion && target.Team == GameObjectTeam.Neutral && spellmenu.Item(ObjectManager.Player.ChampionName + ".EC.Smite.Jungle.Bool").GetValue<bool>())
                    {
                        if (spellmenu.Item("EC.Smite." + target.CharData.BaseSkinName + ".Bool").GetValue<bool>() && inrange)
                        {
                            switch (spellmenu.Item("EC.Smite." + target.CharData.BaseSkinName + ".Index").GetValue<StringList>().SelectedIndex)
                            {
                                case 0:
                                    ObjectManager.Player.Spellbook.CastSpell(SmiteSlot, target);
                                    break;
                                case 1:
                                    if (SmiteDamageMinions >= target.Health)
                                    {
                                        ObjectManager.Player.Spellbook.CastSpell(SmiteSlot, target);
                                    }
                                    break;
                            }
                        }
                    }
                    if (target is Obj_AI_Minion && target.Team != ObjectManager.Player.Team && target.Team != GameObjectTeam.Neutral &&
                        spellmenu.Item(ObjectManager.Player.ChampionName + ".EC.Smite.Lane.Bool").GetValue<bool>())
                    {
                        if (SmiteDamageMinions >= target.Health && inrange)
                        {
                            ObjectManager.Player.Spellbook.CastSpell(SmiteSlot, target);
                        }
                    }
                }
            }
        }
        #endregion Smite
        #region Ignite
        public static void Ignites(Obj_AI_Hero target)
        {
            if (Menu1 == "Ignite" || Menu2 == "Ignite")
            {
                if (ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                {
                    IgniteSpell.Cast(target);
                }
            }
        } 
        #endregion
        #region Barrier
        public static bool HaveBarrier
        {
            get { return (Menu1 == "Barrier" || Menu2 == "Barrier"); }
        }
        #endregion
        #region Heal
        public static bool HaveHeal
        {
            get { return (Menu1 == "Heal" || Menu2 == "Heal"); }
        }
        #endregion
        private static void OnUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead || ObjectManager.Player.IsZombie) return;
            #region Smite
            if (SmiteNames.Contains(Slot1.Name))
            {
                SmiteSlot = SpellSlot.Summoner1;
            }
            else if (SmiteNames.Contains(Slot2.Name))
            {
                SmiteSlot = SpellSlot.Summoner2;
            }
            if (spellmenu.Item(ObjectManager.Player.ChampionName + ".EC.Smite.Combo.Bool").GetValue<bool>())
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                {
                    if (ObjectManager.Player.Spellbook.CanUseSpell(SmiteSlot) == SpellState.Ready)
                    {
                        var target =
                            TargetSelector.GetSelectedTarget() != null &&
                            TargetSelector.GetSelectedTarget().IsValidTarget() ?
                            TargetSelector.GetSelectedTarget() :
                            HeroManager.Enemies.Where(
                            x =>
                                x.IsVisible && !x.IsDead && !x.IsZombie && x.IsValidTarget() &&
                                spellmenu.Item("EC.Smite.Whitelist." + x.NetworkId).GetValue<bool>() &&
                                Vector3.Distance(ObjectManager.Player.ServerPosition, x.ServerPosition) <= 550)
                                .OrderBy(i => i.Health)
                                .FirstOrDefault();
                        if (target != null && !myUtility.ImmuneToDeath(target) && !myUtility.ImmuneToMagic(target))
                        {
                            ObjectManager.Player.Spellbook.CastSpell(SmiteSlot, target);
                        }
                    }
                }
            }
            #endregion
            #region Ignite

            if (Menu1 == "Ignite" || Menu2 == "Ignite")
            {
                IgniteSpell = new Spell(IgniteSlot, 600);
                IgniteSpell.SetTargetted(Single.MaxValue, Single.MaxValue);
            }

            if (spellmenu.Item(ObjectManager.Player.ChampionName + ".EC.Ignite.Combo.Bool").GetValue<bool>())
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                {
                    var EnemyList = HeroManager.Enemies.Where(
                            x =>
                                x.IsVisible && !x.IsDead && !x.IsZombie && x.IsValidTarget() &&
                                spellmenu.Item("EC.Ignite.Whitelist." + x.NetworkId).GetValue<bool>() &&
                                !myUtility.ImmuneToDeath(x) &&
                                !myUtility.ImmuneToMagic(x) &&
                                Vector3.Distance(ObjectManager.Player.ServerPosition, x.ServerPosition) <= IgniteSpell.Range &&
                                ObjectManager.Player.GetSummonerSpellDamage(x, Damage.SummonerSpell.Ignite) >= x.Health)
                                .OrderByDescending(i => myRePriority.ResortDB(i.ChampionName));

                    foreach (var x in EnemyList)
                    {
                        Ignites(x);
                    }
                }
            }
            #endregion
        }

        private static void ProcessDamageBuffer(Obj_AI_Base sender, Obj_AI_Hero target, SpellData spell, myCustomEvents.DamageTriggerType type)
        {
            if (sender != null && target.IsMe)
            {
                switch (type)
                {
                    case myCustomEvents.DamageTriggerType.Killable:
                        break;
                    case myCustomEvents.DamageTriggerType.TonsOfDamage:
                        break;
                }
            }
        }
    }
}