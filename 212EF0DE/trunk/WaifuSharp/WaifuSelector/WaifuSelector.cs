using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Multimedia;
using WaifuSharp.Enums;
using WaifuSharp.ResourceClasses;
using WaifuSharp.WaifuHelper;

namespace WaifuSharp.WaifuSelector
{
    class WaifuSelector
    {
        private static string LeagueSharpAppData
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "LS" + Environment.UserName.GetHashCode().ToString("X"));
            }
        }

        public static String AssemblyDir
        {
            get { return Path.Combine(LeagueSharpAppData, "WaifuSharp"); }
        }

        private static String WaifusDir
        {
            get { return Path.Combine(AssemblyDir, "WaifusDir"); }
        }

        public static List<Waifu> Waifus = new List<Waifu>();

        private static bool IsDrawing = false;

        private static int X
        {
            get { return WaifuSharp.Menu.Item("waifusharp.options.x").GetValue<Slider>().Value; }
        }

        private static int Y
        {
            get { return WaifuSharp.Menu.Item("waifusharp.options.y").GetValue<Slider>().Value; }
        }

        private static float Scale
        {
            get { return WaifuSharp.Menu.Item("waifusharp.options.scale").GetValue<Slider>().Value / 100f; }
        }

        public static Render.Sprite CurrentSprite;

        private static SoundPlayer sPlayer = new SoundPlayer();

        
        public static void OnLoad()
        {
            CheckAndCreateDirectories();
            SaveDefaultWaifus();
            LoadWaifus();
            LoadMenu();
            Game.OnInput += Game_OnInput;
        }


        private static void Game_OnInput(GameInputEventArgs args)
        {
            if (args.Input.StartsWith(".w"))
            {
                args.Process = false;
                if (IsDrawing)
                {
                    return;
                }
                var waifu = GetCurrentWaifu();
                if (waifu != null)
                {
                    var soundList =
                            GetCurrentWaifu()
                                .OnKillSounds.Where(m => m.MinWaifuLevel <= GetCurrentWaifu().CurrentLevel)
                                .ToArray();
                    if (soundList.Any())

                    {
                        var currentSound = soundList[new Random().Next(0, soundList.Count())];

                        if (currentSound != null)
                        {
                            sPlayer.Stream = new MemoryStream(currentSound.SoundStream, true);
                            sPlayer.Load();
                            if (sPlayer.IsLoadCompleted)
                            {
                                sPlayer.Play();
                            }
                            sPlayer.Dispose();
                        }
                    }
                    

                    var spriteList = GetCurrentWaifu()
                                .OnKillPics.Where(m => m.MinWaifuLevel <= GetCurrentWaifu().CurrentLevel)
                                .ToArray();
                    if (spriteList.Any())
                    {
                        var sprite = spriteList[new Random().Next(0, spriteList.Count())];
                        if (sprite != null)
                        {
                            IsDrawing = true;
                            sprite.IsDrawing = true;
                            sprite.Sprite.Visible = true;
                            sprite.Sprite.Scale = new Vector2(Scale, Scale);
                            sprite.Sprite.VisibleCondition = delegate
                            {
                                return sprite.IsDrawing;
                            };
                            sprite.Sprite.Position = new Vector2(X, Y);
                            sprite.Sprite.PositionUpdate += () => new Vector2(X, Y);
                            sprite.Sprite.Add();
                            Utility.DelayAction.Add(
                                2000, () =>
                                {
                                    IsDrawing = false;
                                    sprite.IsDrawing = true;
                                    sprite.Sprite.Visible = false;
                                    sprite.Sprite.Remove();
                                });
                        }
                    }
                    
                }
            }
            if (args.Input.StartsWith(".l"))
            {
                args.Process = false;
                GetCurrentWaifu().CurrentLevel += 1;
            }
            if (args.Input.StartsWith(".e"))
            {
                args.Process = false;
                Levelmanager.LevelManager.RaiseWaifuEXP(ResourcePriority.SingleKill);
            }
            if (args.Input.StartsWith(".d"))
            {
                args.Process = false;
                Levelmanager.LevelManager.DecreaseWaifuExp();
            }
        }

        public static Waifu GetCurrentWaifu()
        {
            var menuItem = WaifuSharp.Menu.Item("waifusharp.options.waifus");

            if (menuItem != null)
            {
                return GetWaifuByName(menuItem.GetValue<StringList>().SelectedValue);
            }

            return Waifus.FirstOrDefault();
        }

        public static Waifu GetWaifuByName(String name)
        {
            return Waifus.FirstOrDefault(w => w.Name.ToLower() == name.ToLower());
        }

        #region Waifus Loading
        private static void LoadWaifus()
        {
            foreach (var directory in Directory.GetDirectories(WaifusDir))
            {
                var array = directory.Split(Path.DirectorySeparatorChar);
                var waifuName = array.Last();
                var currentWaifu = new Waifu
                {
                    Name = waifuName
                };

                foreach (var d2 in Directory.GetDirectories(directory))
                {
                    var a2 = d2.Split(Path.DirectorySeparatorChar);
                    var d2Name = a2.Last();

                    if (!IsInt(d2Name))
                    {
                        continue;
                    }

                    string[] content = Directory.GetFiles(d2);

                    foreach (var file in content)
                    {
                        LoadContentToWaifu(file, d2Name, currentWaifu);
                    }
                }
                //Game.PrintChat(string.Format("<b><font color='#FF0000'>Waifu#:</font></b> Loaded <b><font color='#7A6EFF'>{0}</font></b>", currentWaifu.Name));

                Waifus.Add(currentWaifu);
            }
            Game.PrintChat(string.Format("<b><font color='#FF0000'>Waifu#:</font></b> Loaded <b><font color='#FF0000'>{0}</font></b> waifus", Waifus.Count));
        }

        private static void LoadContentToWaifu(String FilePath, String DirName, Waifu currentWaifu)
        {
            var array = FilePath.Split(Path.DirectorySeparatorChar);
            var fileName = array.Last();
            if (fileName.ToLower().Contains("onkill"))
            {
                OnKillLoad(fileName, FilePath, DirName, currentWaifu);
            }else if (fileName.ToLower().Contains("ondeath"))
            {
                OnDeathLoad(fileName, FilePath, DirName, currentWaifu);
            }
        }

        #region OnKill / OnDeath Load
        private static void OnKillLoad(String fileName, String filePath, String DirName, Waifu currentWaifu)
        {
            var MinWaifuLevel = Int32.Parse(DirName);


            var priority = fileName.Replace("onkill", "");

            switch (fileName.GetLast(4))
            {
                case ".png":
                case ".jpg":
                case "jpeg":
                        var currentSprite = GetSpriteFromFile(filePath);
                        if (currentSprite != null)
                        {
                            currentWaifu.OnKillPics.Add(
                                new OnKillSprite
                                {
                                    PicPriority = GetResourcePriority(priority),
                                    MinWaifuLevel = MinWaifuLevel,
                                    Sprite = currentSprite
                                });
                        }
                    break;
                case ".wav":
                        var currentSound = GetSoundStreamFromFile(filePath);
                        if (currentSound.Any())
                        {
                            currentWaifu.OnKillSounds.Add(new OnKillSound
                            {
                                SoundPriority = GetResourcePriority(priority),
                                MinWaifuLevel = MinWaifuLevel,
                                SoundStream = currentSound
                            });
                        }
                    break;
            }
            
        }

        private static void OnDeathLoad(String fileName, String filePath, String DirName, Waifu currentWaifu)
        {
            var MinWaifuLevel = Int32.Parse(DirName);


            var priority = fileName.Replace("onkill", "");

            switch (fileName.GetLast(4))
            {
                case ".png":
                case ".jpg":
                case "jpeg":
                    var currentSprite = GetSpriteFromFile(filePath);
                    if (currentSprite != null)
                    {
                        currentWaifu.OnDeathPics.Add(
                            new OnDeathSprite()
                            {
                                PicPriority = GetResourcePriority(priority),
                                MinWaifuLevel = MinWaifuLevel,
                                Sprite = currentSprite
                            });
                    }
                    break;
                case ".wav":
                    var currentSound = GetSoundStreamFromFile(filePath);
                    if (currentSound.Any())
                    {
                        currentWaifu.OnDeathSounds.Add(new OnDeathSound()
                        {
                            SoundPriority = GetResourcePriority(priority),
                            MinWaifuLevel = MinWaifuLevel,
                            SoundStream = currentSound
                        });
                    }
                    break;
            }
        }

        #endregion

        
        #endregion

        #region Menu Creation
        private static void LoadMenu()
        {

            var OptionsMenu = new Menu("Waifu# Options","waifusharp.options");
            {
                //var stringListContainer = new MenuItem("waifusharp.options.waifus", "Current Waifu: ");
                var WaifuNames = Waifus.Select(w => w.Name).ToArray();
                //stringListContainer.SetValue(new StringList(WaifuNames.ToArray()));
                //OptionsMenu.AddItem(stringListContainer);

                OptionsMenu.AddItem(
                    new MenuItem("waifusharp.options.waifus", "Current Waifu: ").SetValue(
                        new StringList(WaifuNames, 0))).ValueChanged += (sender, args) =>
                        {
                            var newWaifu = args.GetNewValue<StringList>().SelectedValue;
                            Levelmanager.LevelManager.UpdateWaifuStatistics(newWaifu);
                        };
                OptionsMenu.AddItem(
                    new MenuItem("waifusharp.options.x", "X Coordinate").SetValue(
                        new Slider(200, 0, Drawing.Direct3DDevice.Viewport.Width)));
                OptionsMenu.AddItem(
                    new MenuItem("waifusharp.options.y", "Y Coordinate").SetValue(
                        new Slider(200, 0, Drawing.Direct3DDevice.Viewport.Height)));
                OptionsMenu.AddItem(new MenuItem("waifusharp.options.scale", "Image Scale %").SetValue(new Slider(50))).ValueChanged +=
                    (sender, args) =>
                    {
                        if (CurrentSprite != null)
                        {
                            CurrentSprite.Scale = new Vector2(Scale, Scale);
                        } 
                    };

                OptionsMenu.AddItem(new MenuItem("waifusharp.options.testwaifu", "Show Test Waifu").SetValue(false))
                    .ValueChanged += (sender, args) =>
                    {
                        var waifu = GetCurrentWaifu();
                        if (waifu != null)
                        {
                            var sprite = waifu.OnKillPics[new Random().Next(0, waifu.OnKillPics.Count())];
                            if (sprite != null)
                            {
                                if (args.GetNewValue<bool>())
                                {
                                    InitKillSprite(sprite);
                                }
                                else
                                {
                                    if (CurrentSprite != null)
                                    {
                                        IsDrawing = false;
                                        CurrentSprite.Visible = false;
                                        CurrentSprite.Remove();
                                    }
                                    
                                }
                            }
                        }
                    };
                OptionsMenu.AddItem(
                    new MenuItem("waifusharp.options.duration", "Duration").SetValue(new Slider(3500, 0, 10000)));
                


                WaifuSharp.Menu.AddSubMenu(OptionsMenu);
            }
            
            WaifuSharp.Menu.Item("waifusharp.options.testwaifu").SetValue(false);

            WaifuSharp.Menu.AddToMainMenu();
        }
        #endregion

        #region Utility Methods

        public static void InitKillSprite(OnKillSprite sprite)
        {
            if (IsDrawing)
            {
                CurrentSprite = null;

                IsDrawing = false;
                sprite.IsDrawing = true;
                sprite.Sprite.Visible = false;
                sprite.Sprite.Remove();
            }

            IsDrawing = true;
            sprite.IsDrawing = true;
            sprite.Sprite.Visible = true;
            sprite.Sprite.Scale = new Vector2(Scale, Scale);
            sprite.Sprite.VisibleCondition = delegate
            { return sprite.IsDrawing; };
            sprite.Sprite.Position = new Vector2(X, Y);
            sprite.Sprite.PositionUpdate += () => new Vector2(X, Y);
            CurrentSprite = sprite.Sprite;
            sprite.Sprite.Add();

            Utility.DelayAction.Add(
                               WaifuSharp.Menu.Item("waifusharp.options.duration").GetValue<Slider>().Value, () =>
                               {
                                   CurrentSprite = null;

                                   IsDrawing = false;
                                   sprite.IsDrawing = true;
                                   sprite.Sprite.Visible = false;
                                   sprite.Sprite.Remove();
                               });
        }

        public static void InitDeathSprite(OnDeathSprite sprite)
        {
            if (IsDrawing)
            {
                CurrentSprite = null;

                IsDrawing = false;
                sprite.IsDrawing = true;
                sprite.Sprite.Visible = false;
                sprite.Sprite.Remove();
            }

            IsDrawing = true;
            sprite.IsDrawing = true;
            sprite.Sprite.Visible = true;
            sprite.Sprite.Scale = new Vector2(Scale, Scale);
            sprite.Sprite.VisibleCondition = delegate
            { return sprite.IsDrawing; };
            sprite.Sprite.Position = new Vector2(X, Y);
            sprite.Sprite.PositionUpdate += () => new Vector2(X, Y);
            CurrentSprite = sprite.Sprite;
            sprite.Sprite.Add();

            Utility.DelayAction.Add(
                               WaifuSharp.Menu.Item("waifusharp.options.duration").GetValue<Slider>().Value, () =>
                               {
                                   CurrentSprite = null;

                                   IsDrawing = false;
                                   sprite.IsDrawing = true;
                                   sprite.Sprite.Visible = false;
                                   sprite.Sprite.Remove();
                               });
        }
        public static ResourcePriority GetResourcePriority(string priority)
        {
            if (priority.Contains("single"))
            {
                return ResourcePriority.SingleKill;
            }
            
            if (priority.Contains("double"))
            {
                return ResourcePriority.DoubleKill;
            }

            if (priority.Contains("triple"))
            {
                return ResourcePriority.TripleKill;
            }

            if (priority.Contains("quadra"))
            {
                return ResourcePriority.QuadraKill;
            }

            if (priority.Contains("penta"))
            {
                return ResourcePriority.PentaKill;   
            }
            return ResourcePriority.Random;
        }
        private static void SaveDefaultWaifus()
        {

        }
        private static byte[] GetSoundStreamFromFile(string filePath)
        {
            //var soundStream = new SoundStream(fs);
            return File.ReadAllBytes(filePath);
        }

        private static Render.Sprite GetSpriteFromFile(string filePath)
        {
            var sprite = new Render.Sprite(filePath, Vector2.Zero);
            return sprite;
        }

        private static bool IsInt(string sVal)
        {
            return sVal.Select(c => (int)c).All(iN => (iN <= 57) && (iN >= 48));
        }

        private static void CheckAndCreateDirectories()
        {
            if (!Directory.Exists(AssemblyDir))
            {
                Directory.CreateDirectory(AssemblyDir);
            }

            if (!Directory.Exists(WaifusDir))
            {
                Directory.CreateDirectory(WaifusDir);
            }
        }
        #endregion
    }
}
