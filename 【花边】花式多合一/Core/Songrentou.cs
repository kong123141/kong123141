using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using System.Text.RegularExpressions;

namespace 花边_花式多合一.Core
{
    public class Songrentou
    {
        #region SpellList

        public static List<ChampWrapper> ListChamp = new List<ChampWrapper>()
                                                         {
                                                             new ChampWrapper
                                                                 {
                                                                     Name = "Blitzcrank",
                                                                     SpellSlots = new List<SpellSlot>() { SpellSlot.W }
                                                                 },
                                                             new ChampWrapper
                                                                 {
                                                                     Name = "Bard",
                                                                     SpellSlots = new List<SpellSlot>() { SpellSlot.W }
                                                                 },
                                                             new ChampWrapper
                                                                 {
                                                                     Name = "DrMundo",
                                                                     SpellSlots = new List<SpellSlot>() { SpellSlot.R }
                                                                 },
                                                             new ChampWrapper
                                                                 {
                                                                     Name = "Draven",
                                                                     SpellSlots = new List<SpellSlot>() { SpellSlot.W }
                                                                 },
                                                             new ChampWrapper
                                                                 {
                                                                     Name = "Evelynn",
                                                                     SpellSlots = new List<SpellSlot>() { SpellSlot.W }
                                                                 },
                                                             new ChampWrapper
                                                                 {
                                                                     Name = "Garen",
                                                                     SpellSlots = new List<SpellSlot>() { SpellSlot.Q }
                                                                 },
                                                             new ChampWrapper
                                                                 {
                                                                     Name = "Hecarim",
                                                                     SpellSlots = new List<SpellSlot>() { SpellSlot.E }
                                                                 },
                                                             new ChampWrapper
                                                                 {
                                                                     Name = "Karma",
                                                                     SpellSlots = new List<SpellSlot>() { SpellSlot.E }
                                                                 },
                                                             new ChampWrapper
                                                                 {
                                                                     Name = "Kayle",
                                                                     SpellSlots = new List<SpellSlot>() { SpellSlot.W }
                                                                 },
                                                             new ChampWrapper
                                                                 {
                                                                     Name = "Kennen",
                                                                     SpellSlots = new List<SpellSlot>() { SpellSlot.E }
                                                                 },
                                                             new ChampWrapper
                                                                 {
                                                                     Name = "Lulu",
                                                                     SpellSlots = new List<SpellSlot>() { SpellSlot.W }
                                                                 },
                                                             new ChampWrapper
                                                                 {
                                                                     Name = "MasterYi",
                                                                     SpellSlots = new List<SpellSlot>() { SpellSlot.R }
                                                                 },
                                                             new ChampWrapper
                                                                 {
                                                                     Name = "Nunu",
                                                                     SpellSlots = new List<SpellSlot>() { SpellSlot.W }
                                                                 },
                                                             new ChampWrapper
                                                                 {
                                                                     Name = "Olaf",
                                                                     SpellSlots = new List<SpellSlot>() { SpellSlot.R }
                                                                 },
                                                             new ChampWrapper
                                                                 {
                                                                     Name = "Orianna",
                                                                     SpellSlots = new List<SpellSlot>() { SpellSlot.W }
                                                                 },
                                                             new ChampWrapper
                                                                 {
                                                                     Name = "Poppy",
                                                                     SpellSlots = new List<SpellSlot>() { SpellSlot.W }
                                                                 },
                                                             new ChampWrapper
                                                                 {
                                                                     Name = "Quinn",
                                                                     SpellSlots = new List<SpellSlot>() { SpellSlot.R }
                                                                 },
                                                             new ChampWrapper
                                                                 {
                                                                     Name = "Rammus",
                                                                     SpellSlots = new List<SpellSlot>() { SpellSlot.Q }
                                                                 },
                                                             new ChampWrapper
                                                                 {
                                                                     Name = "Rumble",
                                                                     SpellSlots = new List<SpellSlot>() { SpellSlot.W }
                                                                 },
                                                             new ChampWrapper
                                                                 {
                                                                     Name = "Ryze",
                                                                     SpellSlots = new List<SpellSlot>() { SpellSlot.R }
                                                                 },
                                                             new ChampWrapper
                                                                 {
                                                                     Name = "Shyvana",
                                                                     SpellSlots = new List<SpellSlot>() { SpellSlot.W }
                                                                 },
                                                             new ChampWrapper
                                                                 {
                                                                     Name = "Singed",
                                                                     SpellSlots = new List<SpellSlot>() { SpellSlot.R }
                                                                 },
                                                             new ChampWrapper
                                                                 {
                                                                     Name = "Sivir",
                                                                     SpellSlots = new List<SpellSlot>() { SpellSlot.R }
                                                                 },
                                                             new ChampWrapper
                                                                 {
                                                                     Name = "Skarner",
                                                                     SpellSlots = new List<SpellSlot>() { SpellSlot.W }
                                                                 },
                                                             new ChampWrapper
                                                                 {
                                                                     Name = "Sona",
                                                                     SpellSlots = new List<SpellSlot>() { SpellSlot.E }
                                                                 },
                                                             new ChampWrapper
                                                                 {
                                                                     Name = "Teemo",
                                                                     SpellSlots = new List<SpellSlot>() { SpellSlot.W }
                                                                 },
                                                             new ChampWrapper
                                                                 {
                                                                     Name = "Trundle",
                                                                     SpellSlots = new List<SpellSlot>() { SpellSlot.W }
                                                                 },
                                                             new ChampWrapper
                                                                 {
                                                                     Name = "Twitch",
                                                                     SpellSlots = new List<SpellSlot>() { SpellSlot.Q }
                                                                 },
                                                             new ChampWrapper
                                                                 {
                                                                     Name = "Udyr",
                                                                     SpellSlots = new List<SpellSlot>() { SpellSlot.E }
                                                                 },
                                                             new ChampWrapper
                                                                 {
                                                                     Name = "Volibear",
                                                                     SpellSlots = new List<SpellSlot>() { SpellSlot.Q }
                                                                 },
                                                             new ChampWrapper
                                                                 {
                                                                     Name = "Zilean",
                                                                     SpellSlots = new List<SpellSlot>() { SpellSlot.W, SpellSlot.E }
                                                                 }
                                                         };
        #endregion

        private static Item _lastItem;
        private static int _priceAddup;
        private static readonly List<Item> ItemList = new List<Item>();
        private static Obj_AI_Hero player;
        private static SpellSlot ghostSlot, healSlot;
        private static readonly Vector3 TopVector3 = new Vector3(2122, 12558, 53);
        private static readonly Vector3 BotVector3 = new Vector3(12608, 2380, 52);
        private static readonly Vector3 PurpleSpawn = new Vector3(14286f, 14382f, 172f);
        private static readonly Vector3 BlueSpawn = new Vector3(416f, 468f, 182f);
        public static bool TopVectorReached;
        public static bool BotVectorReached;
        private static int globalRand;
        private static bool surrenderActive;
        private static int surrenderTime;
        private static float realTime;
        private static int lastLaugh;
        private static double lastTouchdown;
        private static double timeDead;

        internal static void Game_OnGameLoad(EventArgs args)
        {
            try
            {
                if (!InitializeMenu.Menu.Item("SongrentoupEnable").GetValue<bool>())
                {
                    return;
                }

                ghostSlot = player.GetSpellSlot("SummonerHaste");
                healSlot = player.GetSpellSlot("SummonerHeal");

                if (player.Gold >= 0)
                {
                    realTime = Game.ClockTime;
                }

                Load();
                Init(); //Credits to Insensitivity for his amazing "ARAMShopAI" assembly
                Game.OnUpdate += OnUpdate;
                Game.OnEnd += OnEnd;
                Obj_AI_Base.OnIssueOrder += OnIssueOrder;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Songrentou error occurred: '{0}'", ex);
            }
        }

        private static void OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (!InitializeMenu.Menu.Item("Attacks.Disabled").GetValue<bool>())
            {
                return;
            }

            if (sender.IsMe && args.Order == GameObjectOrder.AttackTo)
            {
                args.Process = false;
            }
        }

        private static void OnEnd(GameEndEventArgs args)
        {
            if (InitializeMenu.Menu.Item("Surrender.Activated.Say").GetValue<bool>())
            {
                Game.Say("/all 瀵归潰鐨勮寰楀嚭鍘荤粰鎴戜釜璧炲晩.");
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            if (InitializeMenu.Menu.Item("Feeding.Activated").GetValue<bool>())
            {
                Feed();
            }

            if (InitializeMenu.Menu.Item("Surrender.Activated").GetValue<bool>())
            {
                Surrender();
            }

            if (player.IsDead || player.InFountain())
            {
                TopVectorReached = false;
                BotVectorReached = false;
            }
            else
            {
                if (player.Distance(BotVector3) <= 300)
                {
                    BotVectorReached = true;
                }

                if (player.Distance(TopVector3) <= 300)
                {
                    TopVectorReached = true;
                }
            }
        }

        private static void Surrender()
        {
            if (Game.ClockTime - realTime >= 1200 && !surrenderActive)
            {
                Game.Say("/ff");
                surrenderActive = true;
                surrenderTime = Environment.TickCount;
            }

            if (surrenderActive)
            {
                if (Environment.TickCount - surrenderTime > 180000)
                {
                    surrenderActive = false;
                }
            }
        }

        private static void Feed()
        {
            var feedingMode = InitializeMenu.Menu.Item("Feeding.FeedMode").GetValue<StringList>().SelectedIndex;

            if (feedingMode == 3 && globalRand == -1)
            {
                var rnd = new Random();
                globalRand = rnd.Next(0, 3);
            }

            if (feedingMode != 3)
            {
                globalRand = -1;
            }

            if (player.IsDead)
            {
                globalRand = -1;
            }

            if (globalRand != -1)
            {
                feedingMode = globalRand;
            }

            switch (feedingMode)
            {
                case 0:
                    {
                        if (player.Team == GameObjectTeam.Order)
                        {
                            player.IssueOrder(GameObjectOrder.MoveTo, PurpleSpawn);
                        }
                        else
                        {
                            player.IssueOrder(GameObjectOrder.MoveTo, BlueSpawn);
                        }
                    }
                    break;
                case 1:
                    {
                        if (player.Team == GameObjectTeam.Order)
                        {
                            if (!BotVectorReached)
                                player.IssueOrder(GameObjectOrder.MoveTo, BotVector3);
                            else if (BotVectorReached)
                                player.IssueOrder(GameObjectOrder.MoveTo, PurpleSpawn);
                        }
                        else
                        {
                            if (!BotVectorReached)
                                player.IssueOrder(GameObjectOrder.MoveTo, BotVector3);
                            else if (BotVectorReached)
                                player.IssueOrder(GameObjectOrder.MoveTo, BlueSpawn);
                        }
                    }
                    break;
                case 2:
                    {
                        if (player.Team == GameObjectTeam.Order)
                        {
                            if (!TopVectorReached)
                                player.IssueOrder(GameObjectOrder.MoveTo, TopVector3);
                            else if (TopVectorReached)
                                player.IssueOrder(GameObjectOrder.MoveTo, PurpleSpawn);
                        }
                        else
                        {
                            if (!TopVectorReached)
                                player.IssueOrder(GameObjectOrder.MoveTo, TopVector3);
                            else if (TopVectorReached)
                                player.IssueOrder(GameObjectOrder.MoveTo, BlueSpawn);
                        }
                    }
                    break;
            }

            if (InitializeMenu.Menu.Item("Spells.Activated").GetValue<bool>())
            {
                Spells();
            }
        }

        private static void Spells()
        {
            if (player.Distance(PurpleSpawn) < 600 | player.Distance(BlueSpawn) < 600)
            {
                return;
            }

            if (ghostSlot != SpellSlot.Unknown && player.Spellbook.CanUseSpell(ghostSlot) == SpellState.Ready)
            {
                player.Spellbook.CastSpell(ghostSlot);
            }

            if (healSlot != SpellSlot.Unknown && player.Spellbook.CanUseSpell(healSlot) == SpellState.Ready)
            {
                player.Spellbook.CastSpell(healSlot);
            }

            var entry = ListChamp.FirstOrDefault(h => h.Name == ObjectManager.Player.ChampionName);

            if (entry == null)
            {
                return;
            }

            var slots = entry.SpellSlots;

            foreach (var slot in slots)
            {
                player.Spellbook.LevelSpell(slot);
                if (player.Spellbook.CanUseSpell(slot) == SpellState.Ready)
                {
                    player.Spellbook.CastSpell(slot, player);
                }
            }
        }

        private static void Init()
        {
            throw new NotImplementedException();
        }

        private static void Load()
        {
            throw new NotImplementedException();
        }
    }

    public class ChampWrapper
    {
        public string Name { get; set; }

        public List<SpellSlot> SpellSlots { get; set; }
    }

    internal static class BuyItemEvent
    {
        public delegate void OnBoughtItem(InventorySlot item);

        public static short BuyItemAns = 0x97;
        private static List<ItemId> Inventory = new List<ItemId>();

        static BuyItemEvent()
        {
            LeagueSharp.Common.CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        public static event OnBoughtItem OnBuyItem;

        private static void Game_OnGameLoad(EventArgs args)
        {
            UpdateInventory();
            Game.OnProcessPacket += Game_OnProcessPacket;
        }

        private static void Game_OnProcessPacket(GamePacketEventArgs args)
        {
            if (args.PacketData.GetPacketId().Equals(BuyItemAns))
            {
                Utility.DelayAction.Add(300, UpdateInventory);
            }
        }

        private static void UpdateInventory()
        {
            if (Inventory.Count == 0)
            {
                Inventory = GetInventoryItems();
                return;
            }

            if (Inventory.Count == ObjectManager.Player.InventoryItems.Length)
            {
                return;
            }

            var items = new List<ItemId>().Concat(Inventory).ToList();

            foreach (var item in ObjectManager.Player.InventoryItems)
            {
                if (items.Contains(item.Id))
                {
                    items.Remove(item.Id);
                    continue;
                }

                if (OnBuyItem != null)
                {
                    OnBuyItem(item);
                }
            }

            Inventory.Clear();
            Inventory = GetInventoryItems();
        }

        private static List<ItemId> GetInventoryItems()
        {
            return ObjectManager.Player.InventoryItems.Select(item => item.Id).ToList();
        }

        private static short GetPacketId(this IReadOnlyList<byte> data)
        {
            return (short)((data[1] << 8) | (data[0] << 0));
        }
    }

    public class CustomEvents
    {
        public delegate void OnSpawned(Obj_AI_Hero sender, EventArgs args);
        private static readonly Dictionary<int, bool> Heroes = new Dictionary<int, bool>();

        static CustomEvents()
        {
            Game.OnUpdate += OnGameUpdate;
        }

        public static event OnSpawned OnSpawn;

        private static void OnGameUpdate(EventArgs args)
        {
            var spawnHandler = OnSpawn;
            if (spawnHandler == null)
                return;

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (Heroes.ContainsKey(hero.NetworkId))
                {
                    bool death;
                    if (Heroes.TryGetValue(hero.NetworkId, out death))
                    {
                        if (death && !hero.IsDead)
                            spawnHandler(hero, new EventArgs());

                        Heroes[hero.NetworkId] = hero.IsDead;
                    }
                }
                else
                    Heroes.Add(hero.NetworkId, hero.IsDead);
            }
        }
    }

    class Item
    {
        public string Name;
        public int Id;
        public int[] Into;
        public int[] From;
        public int[] Maps;
        public int Goldbase;
        public bool Ispurchasable;
        public int Goldtotal;
        public int Goldsell;

        public static string GetNameFromBlock(string block)
        {
            return Regex.Matches(block, "(?<=\"name\":\").*?(?=\")")[0].Groups[0].ToString();
        }

        public static int GetIdFromBlock(string block)
        {
            return int.Parse(block.Split(':')[0].Replace("\"", ""));
        }

        public static int[] GetIntoFromBlock(string block)
        {
            var stringItemArray = Regex.Match(block, "(?<=\"into\":\\[).*?(?=])").ToString().Split(',');
            if (stringItemArray[0].Length < 4)
                return null;
            var intItemArray = new int[stringItemArray.Length];
            for (int i = 0; i < stringItemArray.Length; i++)
                intItemArray[i] = int.Parse(stringItemArray[i].Replace("\"", ""));
            return intItemArray;
        }

        public static int[] GetFromFromBlock(string block)
        {
            var stringItemArray = Regex.Match(block, "(?<=\"from\":\\[).*?(?=])").ToString().Split(',');
            if (stringItemArray[0].Length < 4)
                return null;
            var intItemArray = new int[stringItemArray.Length];
            for (int i = 0; i < stringItemArray.Length; i++)
                intItemArray[i] = int.Parse(stringItemArray[i].Replace("\"", ""));
            return intItemArray;
        }

        public static int[] GetMapsFromBlock(string block)
        {
            var stringItemArray = Regex.Match(block, "(?<=\"maps\":\\{).*?(?=})").ToString().Split(',');
            if (stringItemArray[0].Length < 1)
                return null;
            var intItemArray = new int[stringItemArray.Length];
            for (int i = 0; i < stringItemArray.Length; i++)
                intItemArray[i] = int.Parse(stringItemArray[i].Replace("\"", "").Split(':')[0]);
            return intItemArray;
        }

        public static int GetGoldBaseFromBlock(string block)
        {
            var stringPriceArray = Regex.Match(block, "(?<=\"gold\":\\{).*?(?=})").ToString().Split(',');
            return int.Parse(stringPriceArray[0].Split(':')[1].Replace("\"", ""));
        }

        public static bool GetPurchasableBaseFromBlock(string block)
        {
            var stringPriceArray = Regex.Match(block, "(?<=\"gold\":\\{).*?(?=})").ToString().Split(',');
            return bool.Parse(stringPriceArray[1].Split(':')[1].Replace("\"", ""));
        }

        public static int GetGoldTotalFromBlock(string block)
        {
            var stringPriceArray = Regex.Match(block, "(?<=\"gold\":\\{).*?(?=})").ToString().Split(',');
            return int.Parse(stringPriceArray[2].Split(':')[1].Replace("\"", ""));
        }
        public static int GetGoldSellFromBlock(string block)
        {
            var stringPriceArray = Regex.Match(block, "(?<=\"gold\":\\{).*?(?=})").ToString().Split(',');
            return int.Parse(stringPriceArray[3].Split(':')[1].Replace("\"", ""));
        }

        public Item(string inputBlock)
        {
            Name = GetNameFromBlock(inputBlock);
            Id = GetIdFromBlock(inputBlock);
            if (GetIntoFromBlock(inputBlock) != null)
                Into = GetIntoFromBlock(inputBlock);
            if (GetFromFromBlock(inputBlock) != null)
                From = GetFromFromBlock(inputBlock);
            if (GetMapsFromBlock(inputBlock) != null)
                Maps = GetMapsFromBlock(inputBlock);
            Goldbase = GetGoldBaseFromBlock(inputBlock);
            Ispurchasable = GetPurchasableBaseFromBlock(inputBlock);
            Goldtotal = GetGoldTotalFromBlock(inputBlock);
            Goldsell = GetGoldSellFromBlock(inputBlock);
        }

        public override string ToString()
        {
            var info =
            "ITEM NAME: " + Name + "\n" + "ITEM ID: " + Id;
            return info;
        }
    }

}
