using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Newtonsoft.Json;
using WaifuSharp.Enums;

namespace WaifuSharp.Levelmanager
{
    class LevelManager
    {
        public delegate void OnWaifuLevelChange(int previousLevel, int currentLevel);

        public static int baseExp = 350;

        public static float Step = 2.0f;

        public static int KillExp = 50;

        public static int MultiKillAward = 25;

        public static float DeathPenalty = KillExp * 0.75f;

        public static int extraPenalty = 10;

        public static List<WaifuExpWrapper> Exps = new List<WaifuExpWrapper>();

        private static String FilePath
        {
            get { return Path.Combine(WaifuSelector.WaifuSelector.AssemblyDir, "Levels.json"); }
        }

        public static void Onload()
        {
            LoadWaifuLevels();

            LoadMenu(WaifuSharp.Menu);
            UpdateWaifuStatistics();
        }

        private static void LoadMenu(Menu RootMenu)
        {
            var currentWaifu = WaifuSelector.WaifuSelector.GetCurrentWaifu();

            //var LevelMenu = new Menu("Waifu# Level","dz191.waifusharp.level");
            //{
                RootMenu.AddItem(
                    new MenuItem(
                        "dz191.waifusharp.level.currentlevel",
                        string.Format("Level: {0}", currentWaifu != null ? currentWaifu.CurrentLevel : 0)));
                RootMenu.AddItem(
                    new MenuItem(
                        "dz191.waifusharp.level.exp",
                        string.Format("Exp: {0} / {1}", currentWaifu != null ? currentWaifu.CurrentExp : 0, baseExp * (currentWaifu != null ? currentWaifu.CurrentLevel : 1) * Step)));
                RootMenu.AddItem(
                    new MenuItem(
                        "dz191.waifusharp.level.progress",
                        string.Format("Progress: {0} %", currentWaifu != null ? currentWaifu.CurrentExp / (baseExp * currentWaifu.CurrentLevel * Step) : 0)));
               // RootMenu.AddSubMenu(LevelMenu);

            //}
        }

        public static void UpdateWaifuStatistics(String WaifuName = "", bool save = true)
        {
            var RootMenu = WaifuSharp.Menu;
            var currentWaifu = WaifuName != "" ? WaifuSelector.WaifuSelector.GetWaifuByName(WaifuName) : WaifuSelector.WaifuSelector.GetCurrentWaifu();
            if (currentWaifu != null)
            {
                RootMenu.Item("dz191.waifusharp.level.currentlevel").DisplayName = string.Format(
                    "Level: {0}", currentWaifu.CurrentLevel);
                RootMenu.Item("dz191.waifusharp.level.exp").DisplayName = string.Format(
                    "Exp: {0} / {1}", currentWaifu.CurrentExp,
                    baseExp * currentWaifu.CurrentLevel * Step);
                RootMenu.Item("dz191.waifusharp.level.progress").DisplayName = string.Format(
                    "Progress: {0} %",(float)
                            Math.Round(currentWaifu.CurrentExp / (baseExp * currentWaifu.CurrentLevel * Step) * 100, 2));
                if (save)
                {
                    SaveWaifuLevels();
                }
            }

        }

        public static void RaiseWaifuEXP(ResourcePriority Kill)
        {
            var currentWaifu = WaifuSelector.WaifuSelector.GetCurrentWaifu();
            if (currentWaifu != null)
            {
                var currentExperience = currentWaifu.CurrentExp;
                var currentLevel = currentWaifu.CurrentLevel;
                var killNumbers = (int) Kill;
                var killExp = KillExp + (MultiKillAward * killNumbers - MultiKillAward);
                if (Game.Time - WaifuSharp.LastEventTick > 15000)
                {
                    //To Award correct EXP to insta multikills
                    killExp = KillExp * killNumbers + (MultiKillAward * killNumbers - MultiKillAward);
                }
                var nextLevelExp = baseExp * currentLevel * Step;
                if (currentExperience + killExp >= nextLevelExp)
                {
                    var remainingExp = currentWaifu.CurrentExp + killExp - nextLevelExp;
                    currentWaifu.CurrentExp = (int) remainingExp;
                    currentWaifu.CurrentLevel += 1;
                }
                else
                {
                    currentWaifu.CurrentExp += killExp;
                }

                UpdateWaifuStatistics();

            }
        }
        public static void DecreaseWaifuExp()
        {
            var currentWaifu = WaifuSelector.WaifuSelector.GetCurrentWaifu();
            if (currentWaifu != null)
            {
                var currentExperience = currentWaifu.CurrentExp;
                var currentLevel = currentWaifu.CurrentLevel;
                var deathExp = DeathPenalty;
                var lostExp = deathExp + (ObjectManager.Player.Deaths * extraPenalty);
                var prevLevelExp = baseExp * (currentLevel - 1) * Step;

                if (currentExperience - lostExp < 0)
                {
                    if (prevLevelExp > 0)
                    {
                        var remainingExp = prevLevelExp + (currentExperience - lostExp) + 1;
                        currentWaifu.CurrentExp = (int) remainingExp;
                        currentWaifu.CurrentLevel -= 1;
                    }
                    else
                    {
                        currentWaifu.CurrentExp = 0;
                    }
                }
                else
                {
                    currentWaifu.CurrentExp -= (int)deathExp;
                }

                UpdateWaifuStatistics();
            }
        }

        //fromBehind(); xdxd
        [SecurityPermission(SecurityAction.Assert, Unrestricted = true)]
        public static void SaveWaifuLevels()
        {
            var wList = WaifuSelector.WaifuSelector.Waifus.Select(waifu => new WaifuExpWrapper { CurrentLevel = waifu.CurrentLevel, CurrentExp = waifu.CurrentExp, WaifuName = waifu.Name }).ToList();
            var serializedObject = JsonConvert.SerializeObject(wList, Formatting.Indented);
            using (StreamWriter sw = new StreamWriter(@FilePath))
            {
                sw.Write(serializedObject);
            }
        }

        [SecurityPermission(SecurityAction.Assert, Unrestricted = true)]
        public static void LoadWaifuLevels()
        {
            if (!File.Exists(@FilePath))
            {
                SaveWaifuLevels();
                return;
            }

            var text = File.ReadAllText(@FilePath);
            var WaifusExps = JsonConvert.DeserializeObject<List<WaifuExpWrapper>>(text);
            foreach (var waifuExp in WaifusExps)
            {
                var waifu = WaifuSelector.WaifuSelector.GetWaifuByName(waifuExp.WaifuName);
                if (waifu != null)
                {
                    waifu.CurrentExp = waifuExp.CurrentExp;
                    waifu.CurrentLevel = waifuExp.CurrentLevel;
                }
            }
        }
            
    }

    internal class WaifuExpWrapper
    {
        [JsonProperty(PropertyName = "WaifuName")]
        public string WaifuName { get; set; }

        [JsonProperty(PropertyName = "CurrentLevel")]
        public int CurrentLevel { get; set; }

        [JsonProperty(PropertyName = "CurrentExp")]
        public int CurrentExp { get; set; }

    }
}
