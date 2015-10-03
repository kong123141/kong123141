#region Todo
    //      Formatting menu to 'standard'
    //      summonermana, 600 range, SelfAoe
    //      summonerhaste, 
    //      summonerexhaust, 650 range, Unit
    //      summonerboost, cleanse
    //      summonerclairvoyance
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
        private static SpellDataInst Slot1, Slot2;
        private static Spell IgniteSpell;
        private static String Menu1, Menu2;
        private static Menu Spellmenu;
        private static SpellSlot SmiteSlot, IgniteSlot, BarrierSlot, HealSlot, SnowballSlot, ClaritySlot, GhostSlot, ExhaustSlot;
        public static void AddToMenu(Menu menu)
        {
            Spellmenu = menu;
            #region Menu1
            if (Menu1 != null)
            {
                var slotmenu1 = new Menu(Menu1, Menu1);
                {
                    if (Menu1 == "Snowball" || Menu1 == "Clarity" || Menu1 == "Ghost" || Menu1 == "Exhaust")
                    {
                        slotmenu1.AddItem(new MenuItem("Not Supported", "Not Supported"));
                    }
                    if (Menu1 == "Smite" || Menu1 == "Ignite")
                    {
                        var Combo = new Menu("Combo Mode", "ComboMode");
                        {
                            Combo.AddItem(new MenuItem("EC." + Menu1 + "." + ObjectManager.Player.ChampionName + ".Combo.Bool", "Enable").SetValue(false));
                            foreach (var enemy in HeroManager.Enemies)
                            {
                                Combo.SubMenu("Whitelist").AddItem(new MenuItem("EC." + Menu1 + ".Whitelist." + enemy.NetworkId, enemy.CharData.BaseSkinName).SetValue(false));
                            }
                        }
                        slotmenu1.AddSubMenu(Combo);
                    }
                    if (Menu1 == "Barrier" || Menu1 == "Heal")
                    {
                        slotmenu1.AddItem(new MenuItem("EC." + Menu1 + "." + ObjectManager.Player.ChampionName + ".Bool", "Enable").SetValue(false));
                        slotmenu1.AddItem(new MenuItem("EC." + Menu1 + ".Bool.Killable", "Only on Killable").SetValue(false));
                        slotmenu1.AddItem(new MenuItem("EC." + Menu1 + ".Slider.HP", "Triggers when % hp <").SetValue(new Slider(30, 5, 99)));
                        slotmenu1.AddItem(new MenuItem("EC." + Menu1 + ".Slider.Damage", "Damage exceeds % hp").SetValue(new Slider(5,5, 99)));
                    }
                    if (Menu1 == "Smite")
                    {
                        slotmenu1.SubMenu("Jungle Clear").AddItem(new MenuItem("EC.Smite." + ObjectManager.Player.ChampionName + ".Jungle.Bool", "Enable").SetValue(false));
                        var MonsterList = new Menu("Whitelist", "Whitelist");
                        {
                            foreach (var x in myFarmManager.NeutralCampList
                                .Where(
                                i =>
                                    (Game.MapId == GameMapId.SummonersRift && (i.Contains("SRU_") || i.Contains("Sru_"))) ||
                                    (Game.MapId == GameMapId.TwistedTreeline && i.Contains("TT_"))))
                            {
                                MonsterList.SubMenu(x).AddItem(new MenuItem("EC.Smite." + x + ".Bool", "Enable").SetValue(false));
                                MonsterList.SubMenu(x).AddItem(new MenuItem("EC.Smite." + x + ".Index", "").SetValue(new StringList(new[] { "First", "Last" })));
                            }
                            slotmenu1.SubMenu("Jungle Clear").AddSubMenu(MonsterList);
                        }
                        slotmenu1.SubMenu("Lane Clear").AddItem(new MenuItem("EC.Smite." + ObjectManager.Player.ChampionName + ".Lane.Bool", "Enable").SetValue(false));
                    }
                }
                Spellmenu.AddSubMenu(slotmenu1);
            }
            #endregion Menu1
            #region Menu2
            if (Menu2 != null)
            {
                var slotmenu2 = new Menu(Menu2, Menu2);
                {
                    if (Menu2 == "Snowball" || Menu2 == "Clarity" || Menu2 == "Ghost" || Menu2 == "Exhaust")
                    {
                        slotmenu2.AddItem(new MenuItem("Not Supported", "Not Supported"));
                    }
                    if (Menu2 == "Smite" || Menu2 == "Ignite")
                    {
                        var Combo = new Menu("Combo Mode", "ComboMode");
                        {
                            Combo.AddItem(new MenuItem("EC." + Menu2 + "." + ObjectManager.Player.ChampionName + ".Combo.Bool", "Enable").SetValue(false));
                            foreach (var enemy in HeroManager.Enemies)
                            {
                                Combo.SubMenu("Whitelist").AddItem(new MenuItem("EC." + Menu2 + ".Whitelist." + enemy.NetworkId, enemy.CharData.BaseSkinName).SetValue(false));
                            }
                        }
                        slotmenu2.AddSubMenu(Combo);
                    }
                    if (Menu2 == "Barrier" || Menu2 == "Heal")
                    {
                        slotmenu2.AddItem(new MenuItem("EC." + Menu2 + "." + ObjectManager.Player.ChampionName + ".Bool", "Enable").SetValue(false));
                        slotmenu2.AddItem(new MenuItem("EC." + Menu2 + ".Bool.Killable", "Only on Killable").SetValue(false));
                        slotmenu2.AddItem(new MenuItem("EC." + Menu2 + ".Slider.HP", "Triggers when % hp <").SetValue(new Slider(30, 5, 99)));
                        slotmenu2.AddItem(new MenuItem("EC." + Menu2 + ".Slider.Damage", "Damage exceeds % hp").SetValue(new Slider(5, 5, 99)));
                    }
                    if (Menu2 == "Smite")
                    {
                        slotmenu2.SubMenu("Jungle Clear").AddItem(new MenuItem("EC.Smite." + ObjectManager.Player.ChampionName + ".Jungle.Bool", "Enable").SetValue(false));
                        var MonsterList = new Menu("Whitelist", "Whitelist");
                        {
                            foreach (var x in myFarmManager.NeutralCampList
                               .Where(
                               i =>
                                   (Game.MapId == GameMapId.SummonersRift && (i.Contains("SRU_") || i.Contains("Sru_"))) ||
                                   (Game.MapId == GameMapId.TwistedTreeline && i.Contains("TT_"))))
                            {
                                MonsterList.SubMenu(x).AddItem(new MenuItem("EC.Smite." + x + ".Bool", "Enable").SetValue(false));
                                MonsterList.SubMenu(x).AddItem(new MenuItem("EC.Smite." + x + ".Index", "").SetValue(new StringList(new[] { "First", "Last" })));
                            }
                            slotmenu2.SubMenu("Jungle Clear").AddSubMenu(MonsterList);
                        }
                        slotmenu2.SubMenu("Lane Clear").AddItem(new MenuItem("EC.Smite." + ObjectManager.Player.ChampionName + ".Lane.Bool", "Enable").SetValue(false));
                    }
                }
                Spellmenu.AddSubMenu(slotmenu2);
            }
            #endregion Menu2
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
                case "summonersnowball":
                    SnowballSlot = SpellSlot.Summoner1;
                    Menu1 = "Snowball";
                    break;
                case "summonermana":
                    ClaritySlot = SpellSlot.Summoner1;
                    Menu1 = "Clarity";
                    break;
                case "summonerhaste":
                    GhostSlot = SpellSlot.Summoner1;
                    Menu1 = "Ghost";
                    break;
                case "summonerexhaust":
                    ExhaustSlot = SpellSlot.Summoner1;
                    Menu1 = "Exhaust";
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
                    IgniteSlot = SpellSlot.Summoner2;
                    Menu2 = "Ignite";
                    break;
                case "summonerbarrier":
                    BarrierSlot = SpellSlot.Summoner2;
                    Menu2 = "Barrier";
                    break;
                case "summonerheal":
                    HealSlot = SpellSlot.Summoner2;
                    Menu2 = "Heal";
                    break;
                case "summonersnowball":
                    //SnowballSlot = SpellSlot.Summoner2;
                    Menu2 = "Snowball";
                    break;
                case "summonermana":
                    //ClaritySlot = SpellSlot.Summoner2;
                    Menu2 = "Clarity";
                    break;
                case "summonerhaste":
                    //GhostSlot = SpellSlot.Summoner2;
                    Menu2 = "Ghost";
                    break;
                case "summonerexhaust":
                    //ExhaustSlot = SpellSlot.Summoner2;
                    Menu2 = "Exhaust";
                    break;
            }
            if (Menu1 != null || Menu2 != null)
            {
                Game.OnUpdate += OnUpdate;
                myDamageBuffer.ProcessDamageBuffer += ProcessDamageBuffer;
                Obj_AI_Base.OnBuffAdd += OnBuffAdd;
            }
        }

        #region Smite
        private static bool CanSmiteChampion
        {
            get
            {
                return (SmiteSlot == SpellSlot.Summoner1 && (Slot1.Name.Contains("s5_summonersmiteplayerganker") || Slot1.Name.Contains("s5_summonersmiteduel"))) ||
                       (SmiteSlot == SpellSlot.Summoner2 && (Slot2.Name.Contains("s5_summonersmiteplayerganker") || Slot2.Name.Contains("s5_summonersmiteduel")));
            }
        }
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
                    if (target is Obj_AI_Hero && Spellmenu.Item("EC.Smite." + ObjectManager.Player.ChampionName + ".Combo.Bool").GetValue<bool>() && CanSmiteChampion)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(SmiteSlot, target);
                    }
                    if (target is Obj_AI_Minion && target.Team == GameObjectTeam.Neutral && Spellmenu.Item("EC.Smite." + ObjectManager.Player.ChampionName + ".Jungle.Bool").GetValue<bool>())
                    {
                        if (Spellmenu.Item("EC.Smite." + target.CharData.BaseSkinName + ".Bool").GetValue<bool>() && inrange)
                        {
                            switch (Spellmenu.Item("EC.Smite." + target.CharData.BaseSkinName + ".Index").GetValue<StringList>().SelectedIndex)
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
                    if (target is Obj_AI_Minion && target.Team != ObjectManager.Player.Team && target.Team != GameObjectTeam.Neutral && Spellmenu.Item("EC.Smite." + ObjectManager.Player.ChampionName + ".Lane.Bool").GetValue<bool>())
                    {
                        if (SmiteDamageMinions >= target.Health && inrange && (target.NetworkId == myOrbwalker.LastHitID || target.NetworkId == myOrbwalker.LaneClearID))
                        {
                            ObjectManager.Player.Spellbook.CastSpell(SmiteSlot, target);
                        }
                    }
                }
            }
        }
        #endregion Smite

        #region Ignite
        private static bool CanUseIgnite
        {
            get
            {
                return
                    HaveIgnite &&
                    Spellmenu.Item("EC.Ignite." + ObjectManager.Player.ChampionName + ".Combo.Bool").GetValue<bool>() &&
                    IgniteSpell.IsReady();
                //ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready;
            }
        }
        private static bool HaveIgnite
        {
            get { return (Menu1 == "Ignite" || Menu2 == "Ignite"); }
        }
        private static void Ignites(Obj_AI_Hero target)
        {
            if (IgniteSpell.IsReady()) IgniteSpell.Cast(target);
        }
        #endregion

        #region Barrier
        public static bool CanUseBarrier
        {
            get 
            { 
                return 
                    HaveBarrier && 
                    Spellmenu.Item("EC.Barrier." + ObjectManager.Player.ChampionName + ".Bool").GetValue<bool>() && 
                    ObjectManager.Player.Spellbook.CanUseSpell(BarrierSlot) == SpellState.Ready; 
            }
        }
        private static bool HaveBarrier
        {
            get { return (Menu1 == "Barrier" || Menu2 == "Barrier"); }
        }
        private static void Barriers(Obj_AI_Hero target)
        {
            if (ObjectManager.Player.Spellbook.CanUseSpell(BarrierSlot) == SpellState.Ready) ObjectManager.Player.Spellbook.CastSpell(BarrierSlot, target);
        } 
        #endregion

        #region Heal
        public static bool CanUseHeal
        {
            get
            {
                return 
                    HaveHeal && 
                    Spellmenu.Item("EC.Heal." + ObjectManager.Player.ChampionName + ".Bool").GetValue<bool>() &&
                    ObjectManager.Player.Spellbook.CanUseSpell(HealSlot) == SpellState.Ready;
            }
        }
        private static bool HaveHeal
        {
            get { return (Menu1 == "Heal" || Menu2 == "Heal"); }
        }
        private static void Heals(Obj_AI_Hero target)
        {
            if (ObjectManager.Player.Spellbook.CanUseSpell(HealSlot) == SpellState.Ready) ObjectManager.Player.Spellbook.CastSpell(HealSlot, target);
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
            if (SmiteSlot == SpellSlot.Summoner1 || SmiteSlot == SpellSlot.Summoner2)
            {
                if (Spellmenu.Item("EC.Smite." + ObjectManager.Player.ChampionName + ".Combo.Bool").GetValue<bool>())
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
                                    Spellmenu.Item("EC.Smite.Whitelist." + x.NetworkId).GetValue<bool>() &&
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
            }
            #endregion
            #region Ignite
            if (HaveIgnite)
            {
                IgniteSpell = new Spell(IgniteSlot, 600);
                IgniteSpell.SetTargetted(float.MaxValue, float.MaxValue);
            }
            if (CanUseIgnite)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                {
                    var EnemyList = HeroManager.Enemies.Where(
                            x =>
                                x.IsVisible && !x.IsDead && !x.IsZombie && x.IsValidTarget() &&
                                Spellmenu.Item("EC.Ignite.Whitelist." + x.NetworkId).GetValue<bool>() &&
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
        private static void ProcessDamageBuffer(Obj_AI_Base sender, Obj_AI_Hero target, SpellData spell, float damage, myDamageBuffer.DamageTriggers type)
        {
            if (sender != null && target.IsMe && type == myDamageBuffer.DamageTriggers.SummonerSpells)
            {
                #region Barrier
                if (CanUseBarrier)
                {
                    if (damage > ObjectManager.Player.Health && Spellmenu.Item("EC.Barrier.Bool.Killable").GetValue<bool>())
                    {
                        //Do check existing shields, shield strenght, if shield + remaining hp > damage, cast                            
                        //test, ObjectManager.Player.AllShield
                        //include invulerability
                    }
                    else if (!(damage > ObjectManager.Player.Health) && myUtility.PlayerHealthPercentage <= Spellmenu.Item("EC.Barrier.Slider.HP").GetValue<Slider>().Value)
                    {
                        if (damage > Spellmenu.Item("EC.Barrier.Slider.Damage").GetValue<Slider>().Value)
                        {

                        }
                    }
                }
                #endregion Barrier
                #region Heal
                if (CanUseHeal)
                {
                    if (damage > ObjectManager.Player.Health && Spellmenu.Item("EC.Heal.Bool.Killable").GetValue<bool>())
                    {
                        //Do check existing shields, shield strenght, if shield + remaining hp > damage, cast                            
                        //test, ObjectManager.Player.AllShield
                        //include invulerability
                    }
                    else if (!(damage > ObjectManager.Player.Health) && myUtility.PlayerHealthPercentage <= Spellmenu.Item("EC.Heal.Slider.HP").GetValue<Slider>().Value)
                    {
                        if (damage > Spellmenu.Item("EC.Heal.Slider.Damage").GetValue<Slider>().Value)
                        {

                        }
                    }
                }
                #endregion Heal
            }
        }
        private static void OnBuffAdd(Obj_AI_Base sender, Obj_AI_BaseBuffAddEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.Buff.Name == "")
                {

                }
            }
            #region Exhaust
            #endregion Exhaust
            #region Cleanse
            #endregion Cleanse
           
        }
    }
}