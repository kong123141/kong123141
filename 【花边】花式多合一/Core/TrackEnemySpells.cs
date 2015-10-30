using System;
using System.Collections.Generic;
using System.Linq;
using Color = System.Drawing.Color;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using 花边_花式多合一.Properties;
using System.Resources;
using System.Globalization;
using System.Collections;

namespace 花边_花式多合一.Core
{
    class SpellTrackerInfo
    {
        public SpellSlot spellSlot;
        public float cooldownExpires = 0;
        public float totalCooldown = 0;
        public SpellCooldownInfo info;

        public SpellTrackerInfo(SpellCooldownInfo info, SpellSlot spellSlot, float cooldownExpires)
        {
            this.info = info;
            this.spellSlot = spellSlot;
            this.cooldownExpires = cooldownExpires;
        }
    }

    class SpellCooldownInfo
    {
        public string charName;
        public string spellName;
        public SpellSlot spellSlot;
        public float[] cooldownArray;

        public SpellCooldownInfo()
        {

        }
    }

    class SpellCooldownDatabase
    {
        public static List<SpellCooldownInfo> spellCDDatabase = new List<SpellCooldownInfo>();

        static SpellCooldownDatabase()
        {
            spellCDDatabase.Add(new SpellCooldownInfo
            {
                charName = "AllChampions",
                spellName = "summonerteleport",
                spellSlot = SpellSlot.Summoner1,
                cooldownArray = new float[] { 300, 300, 300, 300, 300 },
            });
        }
    }

    class TeleportInfo
    {
        public float startTime = 0;
        public float endTime = 0;
        public Vector3 position = Vector3.Zero;
        public bool isTurretTeleport = false;
        public bool isTeleporting = false;
        public bool isRecalling = false;
        public Obj_AI_Hero hero;

        public TeleportInfo(Obj_AI_Hero hero)
        {
            this.hero = hero;
        }
    }

    class TrackEnemySpells
    {
        public static SpellSlot[] summonerSpellSlots = { SpellSlot.Summoner1, SpellSlot.Summoner2 };
        public static SpellSlot[] spellSlots = { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };

        public static Color backgroundColor = Color.FromArgb(255, 15, 130, 0);

        public static float allyOffsetY = 14;
        public static float enemyOffsetY = 16;

        static float recallBarX = Drawing.Width * 0.425f;
        static float recallBarY = Drawing.Height * 0.80f;
        static float recalBarWidth = Drawing.Width - 2 * recallBarX;

        public static Dictionary<int, TeleportInfo> teleportInfos = new Dictionary<int, TeleportInfo>();
        public static Dictionary<int, Color> heroS1Color = new Dictionary<int, Color>();
        public static Dictionary<int, Color> heroS2Color = new Dictionary<int, Color>();
        public static Dictionary<int, SummonerSpellTracker> heroS1Tracker = new Dictionary<int, SummonerSpellTracker>();
        public static Dictionary<int, SummonerSpellTracker> heroS2Tracker = new Dictionary<int, SummonerSpellTracker>();
        public static Dictionary<int, Dictionary<string, SpellTrackerInfo>> spellCooldowns =
            new Dictionary<int, Dictionary<string, SpellTrackerInfo>>();
        internal static void Game_OnGameLoad(EventArgs args)
        {
            try
            {
                if (!InitializeMenu.Menu.Item("TrackEnemySpells").GetValue<bool>()) return;

                LoadSummonerSpell();
                LoadSpecialSpells();
                Obj_AI_Base.OnProcessSpellCast += Game_OnProcessSpell;
                Drawing.OnDraw += Drawing_OnDraw;
                Drawing.OnEndScene += Drawing_OnEndScene;
                Obj_AI_Base.OnTeleport += Game_OnTeleport;
                GameObject.OnCreate += Game_OnCreateObject;
            }
            catch (Exception ex)
            {
                Console.WriteLine("TrackEnemySpells error occurred: '{0}'", ex);
            }
        }

        private static void LoadSpecialSpells()
        {
            foreach (var hero in HeroManager.AllHeroes)
            {
                teleportInfos.Add(hero.NetworkId, new TeleportInfo(hero));

                Dictionary<string, SpellTrackerInfo> spellDictioniary = new Dictionary<string, SpellTrackerInfo>();
                spellCooldowns.Add(hero.NetworkId, spellDictioniary);

                foreach (var spell in SpellCooldownDatabase.spellCDDatabase)
                {
                    if (spell.spellSlot == SpellSlot.Summoner1)
                    {
                        var spellSlot = GetSummonerSlot(hero, spell.spellName);
                        if (spellSlot != SpellSlot.Unknown)
                        {
                            spellDictioniary.Add(spell.spellName.ToLower(),
                                new SpellTrackerInfo(spell, spellSlot, 0));
                        }
                    }
                    else
                    {
                        if (spell.charName == hero.ChampionName || spell.charName == "AllChampions")
                        {
                            var tSpell = hero.Spellbook.GetSpell(spell.spellSlot);
                            if (tSpell.Name.ToLower() == spell.spellName.ToLower())
                            {
                                spellDictioniary.Add(spell.spellName.ToLower(),
                                    new SpellTrackerInfo(spell, spell.spellSlot, 0));
                            }
                        }
                    }
                }
            }
        }

        private static SpellSlot GetSummonerSlot(Obj_AI_Hero hero, string SpellName)
        {
            var sum1 = hero.Spellbook.GetSpell(SpellSlot.Summoner1);
            if (sum1.Name.ToLower() == SpellName.ToLower())
            {
                return SpellSlot.Summoner1;
            }

            var sum2 = hero.Spellbook.GetSpell(SpellSlot.Summoner2);
            if (sum2.Name.ToLower() == SpellName.ToLower())
            {
                return SpellSlot.Summoner2;
            }

            return SpellSlot.Unknown;
        }

        private static void LoadSummonerSpell()
        {
            foreach (var hero in HeroManager.AllHeroes)
            {
                var spell1 = hero.Spellbook.GetSpell(SpellSlot.Summoner1);
                var spell2 = hero.Spellbook.GetSpell(SpellSlot.Summoner2);

                heroS1Color.Add(hero.NetworkId, GetSummonerColor(spell1.Name));
                heroS2Color.Add(hero.NetworkId, GetSummonerColor(spell2.Name));

                if (!hero.IsMe)
                {
                    heroS1Tracker.Add(hero.NetworkId, new SummonerSpellTracker(hero, spell1.Name));
                    heroS2Tracker.Add(hero.NetworkId, new SummonerSpellTracker(hero, spell2.Name, false));
                }
            }

        }

        public static Color GetSummonerColor(string name)
        {
            Color color;

            //Game.PrintChat(name);

            switch (name.ToLower())
            {
                case "summonerbarrier":
                    color = Color.SandyBrown;
                    break;
                case "summonersnowball":
                    color = Color.White;
                    break;
                case "summonerodingarrison":
                    color = Color.Green;
                    break;
                case "summonerclairvoyance":
                    color = Color.Blue;
                    break;
                case "summonerboost": //cleanse
                    color = Color.LightBlue;
                    break;
                case "summonermana":
                    color = Color.Blue;
                    break;
                case "summonerteleport":
                    color = Color.Purple;
                    break;
                case "summonerheal":
                    color = Color.GreenYellow;
                    break;
                case "summonerexhaust":
                    color = Color.Brown;
                    break;
                case "summonersmite":
                    color = Color.Orange;
                    break;
                case "summonerdot":
                    color = Color.Red;
                    break;
                case "summonerhaste":
                    color = Color.SkyBlue;
                    break;
                case "summonerflash":
                    color = Color.Yellow;
                    break;
                case "s5_summonersmiteduel":
                    color = Color.Orange;
                    break;
                case "s5_summonersmiteplayerganker":
                    color = Color.Orange;
                    break;
                case "s5_summonersmitequick":
                    color = Color.Orange;
                    break;
                case "itemsmiteaoe":
                    color = Color.Orange;
                    break;
                default:
                    color = Color.White;
                    break;
            }

            return color;
        }

        private static void Drawing_OnEndScene(EventArgs args)
        {
            foreach (var hero in HeroManager.AllHeroes)
            {
                if (hero.IsMe ||
                    !hero.IsHPBarRendered
                    || (hero.IsAlly && !InitializeMenu.Menu.Item("TrackAllyCooldown").GetValue<bool>())
                    || (hero.IsEnemy && !InitializeMenu.Menu.Item("TrackEnemyCooldown").GetValue<bool>())
                    )
                {
                    continue;
                }

                var startX = hero.HPBarPosition.X - 9;
                var startY = hero.HPBarPosition.Y +
                    (hero.IsAlly ? allyOffsetY : enemyOffsetY);

                foreach (var slot in summonerSpellSlots)
                {
                    var spell = hero.Spellbook.GetSpell(slot);
                    var time = spell.CooldownExpires - Game.Time;
                    var totalCooldown = spell.Cooldown;

                    SpellTrackerInfo spellInfo;
                    if (spellCooldowns[hero.NetworkId].TryGetValue(spell.Name.ToLower(), out spellInfo))
                    {
                        time = (spellInfo.cooldownExpires - DelayAction.TickCount) / 1000;
                        totalCooldown = spellInfo.totalCooldown > 0 ? spellInfo.totalCooldown : totalCooldown;
                    }

                    var percent = (time > 0 && Math.Abs(totalCooldown) > float.Epsilon)
                            ? 1f - (time / totalCooldown)
                            : 1f;

                    var spellState = hero.Spellbook.CanUseSpell(slot);

                    if (spellState != SpellState.NotLearned)
                    {
                        var color = Color.Green;

                        var summonerTracker = (slot.ToString() == "Summoner1") ?
                            heroS1Tracker[hero.NetworkId] : heroS2Tracker[hero.NetworkId];

                        if (percent != 1f)
                        {

                            Drawing.DrawLine(new Vector2(startX, startY),
                                            new Vector2(startX + ImageLoader.imageWidth * percent, startY),
                                            3, color);

                            summonerTracker.SetGreyScale();
                        }
                        else
                        {
                            summonerTracker.SetNormalScale();
                        }
                    }

                    startY += ImageLoader.imageHeight;
                }
            }

        }

        private static void Game_OnProcessSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var hero = sender as Obj_AI_Hero;
            if (hero != null)
            {
                SpellTrackerInfo spellInfo;
                if (spellCooldowns[sender.NetworkId].TryGetValue(args.SData.Name.ToLower(), out spellInfo))
                {
                    var spellInst = hero.GetSpell(spellInfo.spellSlot);
                    spellInfo.cooldownExpires = DelayAction.TickCount
                        + spellInfo.info.cooldownArray[spellInst.Level - 1] * 1000; // + 250
                }
            }
        }

        private static void Game_OnCreateObject(GameObject sender, EventArgs args)
        {
            if (sender.Type == GameObjectType.obj_GeneralParticleEmitter &&
                sender.Name.Contains("global_ss_teleport"))
            {
                foreach (var info in teleportInfos.Values)
                {
                    if (info.isTeleporting)
                    {
                        if (sender.Name.Contains("turret"))
                        {
                            info.isTurretTeleport = true;
                        }

                        info.position = sender.Position;

                        RenderObjects.Add(
                            new RenderText(info.hero.ChampionName, sender.Position.To2D(), 3500,
                            (sender.Name.Contains("blue") ? Color.SkyBlue : Color.Red)));

                        RenderObjects.Add(
                            new CooldownBar(sender.Position.To2D(), 3500, 20));
                    }
                }
            }

        }

        private static void Game_OnTeleport(Obj_AI_Base sender, GameObjectTeleportEventArgs args)
        {
            var hero = sender as Obj_AI_Hero;
            if (hero != null)
            {
                var packet = Packet.S2C.Teleport.Decoded(sender, args);
                if (packet.Type == Packet.S2C.Teleport.Type.Recall)
                {
                    if (packet.Status == Packet.S2C.Teleport.Status.Start)
                    {
                        var info = teleportInfos[sender.NetworkId];
                        info.isRecalling = true;
                        info.startTime = DelayAction.TickCount;

                        var totalRecallTime = 8000;

                        if (Game.MapId == GameMapId.CrystalScar)
                        {
                            totalRecallTime = 4500;
                        }

                        info.endTime = DelayAction.TickCount + totalRecallTime;
                    }
                    else
                    {
                        var info = teleportInfos[sender.NetworkId];
                        info.isRecalling = false;
                    }
                }

                if (packet.Type == Packet.S2C.Teleport.Type.Teleport)
                {
                    var duration = 0;

                    if (packet.Status == Packet.S2C.Teleport.Status.Finish)
                    {
                        duration = 300;

                        var info = teleportInfos[sender.NetworkId];
                        if (info.isTurretTeleport)
                        {
                            duration = 240;
                        }
                    }
                    else if (packet.Status == Packet.S2C.Teleport.Status.Abort)
                    {
                        duration = 200;
                    }

                    if (packet.Status == Packet.S2C.Teleport.Status.Start)
                    {
                        var info = teleportInfos[sender.NetworkId];
                        info.isTeleporting = true;
                        info.isTurretTeleport = false;
                        info.isRecalling = false;
                        info.startTime = DelayAction.TickCount;
                    }
                    else
                    {
                        var info = teleportInfos[sender.NetworkId];
                        info.isTeleporting = false;
                        info.isTurretTeleport = false;
                    }

                    if (duration > 0)
                    {
                        SpellTrackerInfo spellInfo;
                        if (spellCooldowns[sender.NetworkId].TryGetValue("summonerteleport", out spellInfo))
                        {
                            spellInfo.cooldownExpires = DelayAction.TickCount + duration * 1000;
                            spellInfo.totalCooldown = duration;
                        }
                    }
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {

        }
        private static void DrawRecallBars()
        {
            if (!InitializeMenu.Menu.Item("TrackEnemyRecalls").GetValue<bool>())
            {
                return;
            }
            bool recallBarDrawn = false;

            foreach (var info in teleportInfos.Values.OrderBy(i => i.endTime))
            {
                if (info.isRecalling
                    && info.hero.IsEnemy
                    )
                {
                    var hero = info.hero;

                    var timeLeft = (DelayAction.TickCount - info.startTime);
                    var totalRecallTime = 8000;

                    if (Game.MapId == GameMapId.CrystalScar)
                    {
                        totalRecallTime = 4500;
                    }

                    var percent = timeLeft < totalRecallTime ? 1f - (timeLeft / totalRecallTime) : 1f;

                    if (percent < 1f)
                    {
                        if (recallBarDrawn == false)
                        {
                            Drawing.DrawLine(new Vector2(recallBarX - 1, recallBarY - 1),
                            new Vector2(recallBarX + recalBarWidth + 1, recallBarY - 1), 12, Color.Black);

                            Drawing.DrawLine(new Vector2(recallBarX, recallBarY),
                            new Vector2(recallBarX + recalBarWidth, recallBarY), 10, Color.LightGray);

                            recallBarDrawn = true;
                        }

                        var textDimension = TextUtils.GetTextExtent(hero.ChampionName);
                        TextUtils.DrawText(recallBarX + recalBarWidth * percent - textDimension.Width / 2,
                            recallBarY - 20, Color.White, hero.ChampionName);

                        Drawing.DrawLine(new Vector2(recallBarX, recallBarY),
                            new Vector2(recallBarX + recalBarWidth * percent + 2, recallBarY), 10,
                            Color.FromArgb((int)(255 * (1f - percent)), Color.Black));
                    }
                }
            }
        }

    }

    class SummonerSpellTracker
    {
        private Obj_AI_Hero hero;
        private bool isSummoner1;
        private string spellName;

        private Render.Sprite sImage;

        public Vector2 heroHPBarPosition;

        private bool isGreyScale = false;

        public SummonerSpellTracker(Obj_AI_Hero hero, string spellName, bool isSummoner1 = true)
        {
            this.hero = hero;
            this.isSummoner1 = isSummoner1;
            this.spellName = spellName;

            InitSummonerImage(GetSummonerBitmap(spellName));
        }

        public static Bitmap GetSummonerBitmap(string spellName)
        {
            return ImageLoader.Load(spellName);
        }

        private void InitSummonerImage(Bitmap bmp)
        {
            Game.OnUpdate += Drawing_OnDraw;

            sImage = new Render.Sprite(bmp, new Vector2(0, 0));

            //sImage.Scale = new Vector2(0.5f, 0.5f);
            sImage.VisibleCondition = sender => !(!hero.IsHPBarRendered
                    || (hero.IsAlly && !InitializeMenu.Menu.Item("TrackAllyCooldown").GetValue<bool>())
                    || (hero.IsEnemy && !InitializeMenu.Menu.Item("TrackEnemyCooldown").GetValue<bool>()));

            sImage.PositionUpdate = delegate
            {
                var summoner2OffSet = isSummoner1 ? 0 : sImage.Height;

                var startX = heroHPBarPosition.X - 9;
                var startY = heroHPBarPosition.Y + summoner2OffSet +
                    (hero.IsAlly ? TrackEnemySpells.allyOffsetY : TrackEnemySpells.enemyOffsetY);

                return new Vector2(startX, startY);
            };
            sImage.Add(0);

        }

        private void Drawing_OnDraw(EventArgs args)
        {
            heroHPBarPosition = new Vector2(hero.HPBarPosition.X, hero.HPBarPosition.Y);
        }

        public void SetGreyScale()
        {
            if (!isGreyScale)
            {
                isGreyScale = true;
                sImage.SetSaturation(0.0f);
            }
        }

        public void SetNormalScale()
        {
            if (isGreyScale)
            {
                isGreyScale = false;
                sImage.Reset();
            }
        }
    }

    public class ImageLoader
    {
        private static float iconOpacity = 1f;
        public static int imageHeight = 16;
        public static int imageWidth = 16;

        static ImageLoader()
        {
            Resources.ResourceManager.IgnoreCase = true;

            ResourceSet resourceSet = Resources.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            foreach (DictionaryEntry entry in resourceSet)
            {
                string resourceKey = entry.Key as string;
                object resource = entry.Value;

                if (resource.GetType() == typeof(Bitmap)
                    && resourceKey != null)
                {
                    Load(resourceKey);
                }
            }

        }

        public static Bitmap Load(string spellName)
        {
            string cachedPath = GetCachedPath(spellName);
            if (File.Exists(cachedPath))
            {
                return new Bitmap(cachedPath);
            }

            var bitmap = Resources.ResourceManager.GetObject(spellName) as Bitmap;
            if (bitmap == null)
            {
                Console.WriteLine(spellName + ".png not found.");
                return CreateFinalImage(Resources.Default);
            }
            Bitmap finalBitmap = CreateFinalImage(bitmap);
            //finalBitmap = ChangeOpacity(finalBitmap);
            finalBitmap.Save(cachedPath);

            return finalBitmap;
        }

        private static string GetCachedPath(string championName)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UtilityPlusCache");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path = Path.Combine(path, Game.Version);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return Path.Combine(path, championName + ".png");
        }

        private static Bitmap CreateFinalImage(Bitmap srcBitmap)
        {
            float scale = 0.25f;
            int scaledSize = (int)(scale * srcBitmap.Width);

            Bitmap resized = new Bitmap(srcBitmap, new Size(imageWidth, imageHeight));

            srcBitmap.Dispose();
            return resized;
        }

        private static Bitmap ChangeOpacity(Bitmap img)
        {
            var bmp = new Bitmap(img.Width, img.Height);
            Graphics graphics = Graphics.FromImage(bmp);
            var colormatrix = new ColorMatrix { Matrix33 = iconOpacity };
            var imgAttribute = new ImageAttributes();
            imgAttribute.SetColorMatrix(colormatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            graphics.DrawImage(
                img, new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel,
                imgAttribute);
            graphics.Dispose();
            img.Dispose();
            return bmp;
        }

    }
}
