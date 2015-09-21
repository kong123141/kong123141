using System;
using System.Linq;
using EndifsCollections.Controller;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace EndifsCollections.SummonerSpells
{
    class mySmiter
    {
        private static SpellSlot SmiteSlot;
        private static SpellDataInst Slot1;
        private static SpellDataInst Slot2;
        private static Menu tools;
        private static float smiterange;
        private static bool SmiteChampion;
        private static string SmiteType;
        private static readonly string[] Supported =
        {
            "khazix", "rengar", "masteryi", "evelynn", "shyvana", "vi"
        };

        public mySmiter()
        {
            Game.OnUpdate += OnUpdate;
        }

        public static void AddToMenu(Menu Tools)
        {            
            if (MapSupported)
            {
                tools = Tools;
                var subs = new Menu("Smite", "mySmiter");
                {
                    if (Supported.Contains(ObjectManager.Player.ChampionName.ToLower()))
                    {
                        subs.SubMenu("Champions").AddItem(new MenuItem("mst_cbool", "Enable").SetValue(false));
                        foreach (var enemy in HeroManager.Enemies)
                        {
                            subs.SubMenu("Champions").AddItem(new MenuItem("mst_target" + enemy.Name, enemy.ChampionName).SetValue(true));
                        }
                    }                    
                    var msMobs = new Menu("Monsters List", "MonstersList");
                    {
                        if (Game.MapId == GameMapId.SummonersRift)
                        {
                            foreach (var x in myUtility.LargeNeutral.Where(i => i.Contains("SRU_")))
                            {
                                var msMobsSub = new Menu(x, x);
                                {
                                    msMobsSub.AddItem(new MenuItem("bool_" + x, "Enable").SetValue(false));
                                    msMobsSub.AddItem(new MenuItem("index_" + x, "").SetValue(new StringList(new[] { "First", "Last" })));
                                }
                                msMobs.AddSubMenu(msMobsSub);
                            }
                        }
                        if (Game.MapId == GameMapId.TwistedTreeline)
                        {
                            foreach (var x in myUtility.LargeNeutral.Where(i => i.Contains("TT_")))
                            {
                                var msMobsSub = new Menu(x, x);
                                {
                                    msMobsSub.AddItem(new MenuItem("bool_" + x, "Enable").SetValue(false));
                                    msMobsSub.AddItem(new MenuItem("index_" + x, "").SetValue(new StringList(new[] { "First", "Last" })));
                                }
                                msMobs.AddSubMenu(msMobsSub);
                            }
                        }
                        subs.SubMenu("Jungle").AddSubMenu(msMobs);
                        subs.SubMenu("Jungle").AddItem(new MenuItem("mst_mbool", "Enable").SetValue(false));
                    }
                    subs.AddItem(new MenuItem("mst_sbool", "Seige/Super Minions").SetValue(false));
                }
                Tools.AddSubMenu(subs);
            }
        }
        private void SetSmite()
        {
            Int16[] BlastingSmite = { 3713, 3726, 3725, 3724, 3723 };
            Int16[] ChallengingSmite = { 3715, 3718, 3717, 3716, 3714 };
            Int16[] ChillingSmite = { 3706, 3710, 3709, 3708, 3707 };
            Int16[] ScavengingSmite = { 3711, 3722, 3721, 3720, 3719 };            
            if (BlastingSmite.Any(x => Items.HasItem(x)))
            {
                SmiteSlot = ObjectManager.Player.GetSpellSlot("itemsmiteaoe");
                return;
            }
            if (ChallengingSmite.Any(x => Items.HasItem(x)))
            {
                SmiteSlot = ObjectManager.Player.GetSpellSlot("s5_summonersmiteduel");
                SmiteChampion = true;
                SmiteType = "Red";
                return;
            }
            if (ChillingSmite.Any(x => Items.HasItem(x)))
            {
                SmiteSlot = ObjectManager.Player.GetSpellSlot("s5_summonersmiteplayerganker");
                SmiteChampion = true;
                SmiteType = "Blue";
                return;
            }
            if (ScavengingSmite.Any(x => Items.HasItem(x)))
            {
                SmiteSlot = ObjectManager.Player.GetSpellSlot("s5_summonersmitequick");
                return;
            }
            SmiteSlot = ObjectManager.Player.GetSpellSlot("summonersmite");            
        }
        
        public static void Smites(Obj_AI_Base target)
        {
            if (MapSupported)
            {
                if (target != null && target.IsValidTarget() && IsReady)
                {
                    smiterange = ObjectManager.Player.BoundingRadius + target.BoundingRadius + 500;
                    if (target is Obj_AI_Hero && CanSmiteChampions(target))
                    {
                        if (Vector3.Distance(ObjectManager.Player.ServerPosition, target.ServerPosition) <= 500)
                        {
                            ObjectManager.Player.Spellbook.CastSpell(SmiteSlot, target);
                        }
                    }
                    if (target is Obj_AI_Minion && target.Team == GameObjectTeam.Neutral && CanSmiteMonster)
                    {
                        if (tools.Item("bool_" + target.BaseSkinName).GetValue<bool>() && Vector3.Distance(ObjectManager.Player.ServerPosition, target.ServerPosition) <= smiterange)
                        {
                            switch (tools.Item("index_" + target.BaseSkinName).GetValue<StringList>().SelectedIndex)
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
                    if (target is Obj_AI_Minion && target.Team != ObjectManager.Player.Team && target.Team != GameObjectTeam.Neutral && CanSmiteMinions)
                    {
                        if (SmiteDamageMinions >= target.Health && Vector3.Distance(ObjectManager.Player.ServerPosition, target.ServerPosition) <= smiterange)
                        {
                            ObjectManager.Player.Spellbook.CastSpell(SmiteSlot, target);
                        }
                    }
                }
            }
        }
        private static bool IsReady
        {
            get
            {
                return HaveSmite && ObjectManager.Player.Spellbook.CanUseSpell(SmiteSlot) == SpellState.Ready;
            }
        }
        public static bool HaveSmite
        {
            get
            {
                return SmiteSlot == SpellSlot.Summoner1 || SmiteSlot == SpellSlot.Summoner2; 
            }
        }
        public static bool CanSmiteChampions(Obj_AI_Base target)
        {

            return HaveSmite && 
                MapSupported && 
                IsReady && 
                tools.Item("mst_cbool").GetValue<bool>() && 
                tools.Item("mst_target" + target.Name).GetValue<bool>() &&
                Supported.Contains(ObjectManager.Player.ChampionName.ToLower()) && 
                SmiteChampion;

        }
        public static bool CanSmiteMinions
        {
            get
            {
                return HaveSmite && tools.Item("mst_sbool").GetValue<bool>() && IsReady;
            }
        }
        public static bool CanSmiteMonster
        {
            get
            {
                return HaveSmite && tools.Item("mst_mbool").GetValue<bool>() && IsReady;
            }
        }
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

        public static int SmiteDamageChampions
        {
            get
            {
                if (SmiteType == "Red")
                {
                    return (54 + 6 * ObjectManager.Player.Level) / 3;
                }
                if (SmiteType == "Blue")
                {
                    return (20 + 8 * ObjectManager.Player.Level) / 2;
                }
                return 0;
            }
        }

        public static bool MapSupported
        {
            get { return Game.MapId == GameMapId.SummonersRift || Game.MapId == GameMapId.TwistedTreeline; }
        }

        void OnUpdate(EventArgs args)
        {
            SetSmite();
        }
    }
}
