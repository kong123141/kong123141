using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Multimedia;
using WaifuSharp.Enums;
using WaifuSharp.ResourceClasses;
using WaifuSharp.WaifuHelper;

namespace WaifuSharp
{
    class WaifuSharp
    {
        public static Menu Menu { get; set; }

        public static float LastEventTick;

        private static ResourcePriority KillPriority = ResourcePriority.SingleKill;

        private static SoundPlayer sPlayer = new SoundPlayer();

        private static float Kills;

        public static void OnLoad()
        {
            Game.OnNotify += Game_OnNotify;
            Game.OnUpdate += Game_OnUpdate;
            //Obj_AI_Base.OnDamage += Obj_AI_Base_OnDamage;
        }

        static void Game_OnUpdate(EventArgs args)
        {
           // Kills = ObjectManager.Player.Score;

        }

        private static int delay = 125;
        private static void Game_OnNotify(GameNotifyEventArgs args)
        {
            
                switch (args.EventId)
                {
                    case GameEventId.OnChampionKill:
                        if (ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(args.NetworkId).IsEnemy)
                        {
                            Levelmanager.LevelManager.RaiseWaifuEXP(ResourcePriority.SingleKill);
                            LastEventTick = Game.Time;
                            Utility.DelayAction.Add(delay, ShowOnKillWaifu);
                        }
                        else if (ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(args.NetworkId).IsMe)
                        {
                            Levelmanager.LevelManager.DecreaseWaifuExp();
                            Utility.DelayAction.Add(delay, ShowOnDeathWaifu);
                        }
                        break;
                    case GameEventId.OnChampionDoubleKill:
                        if (ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(args.NetworkId).IsEnemy)
                        {
                        Levelmanager.LevelManager.RaiseWaifuEXP(ResourcePriority.DoubleKill);
                        LastEventTick = Game.Time;
                        Utility.DelayAction.Add(delay, ShowOnKillWaifu);
                        }
                        break;
                    case GameEventId.OnChampionTripleKill:
                        if (ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(args.NetworkId).IsEnemy)
                        {
                            Levelmanager.LevelManager.RaiseWaifuEXP(ResourcePriority.TripleKill);
                            LastEventTick = Game.Time;
                            Utility.DelayAction.Add(delay, ShowOnKillWaifu);
                        }
                        break;
                    case GameEventId.OnChampionQuadraKill:
                        if (ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(args.NetworkId).IsEnemy)
                        {
                            Levelmanager.LevelManager.RaiseWaifuEXP(ResourcePriority.QuadraKill);
                            LastEventTick = Game.Time;
                            Utility.DelayAction.Add(delay, ShowOnKillWaifu);
                        }
                        break;

                    case GameEventId.OnChampionPentaKill:
                        if (ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(args.NetworkId).IsEnemy)
                        {
                            Levelmanager.LevelManager.RaiseWaifuEXP(ResourcePriority.PentaKill);
                            LastEventTick = Game.Time;
                            Utility.DelayAction.Add(delay, ShowOnKillWaifu);
                        }
                        break;
            }
        }

         [SecurityPermission(SecurityAction.Assert, Unrestricted = true)]
        private static void ShowOnKillWaifu()
        {
            if (WaifuSelector.WaifuSelector.CurrentSprite != null)
            {
                WaifuSelector.WaifuSelector.CurrentSprite.Visible = false;
                WaifuSelector.WaifuSelector.CurrentSprite.Remove();
            }
            sPlayer.Stop();

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
                        WaifuSelector.WaifuSelector.InitKillSprite(sprite);
                    }
                }

            }
        }

         [SecurityPermission(SecurityAction.Assert, Unrestricted = true)]
         private static void ShowOnDeathWaifu()
         {
             if (WaifuSelector.WaifuSelector.CurrentSprite != null)
             {
                 WaifuSelector.WaifuSelector.CurrentSprite.Visible = false;
                 WaifuSelector.WaifuSelector.CurrentSprite.Remove();
             }
             sPlayer.Stop();

             var waifu = GetCurrentWaifu();
             if (waifu != null)
             {
                 var soundList =
                         GetCurrentWaifu()
                             .OnDeathSounds.Where(m => m.MinWaifuLevel <= GetCurrentWaifu().CurrentLevel)
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
                             .OnDeathPics.Where(m => m.MinWaifuLevel <= GetCurrentWaifu().CurrentLevel)
                             .ToArray();
                 if (spriteList.Any())
                 {
                     var sprite = spriteList[new Random().Next(0, spriteList.Count())];
                     if (sprite != null)
                     {
                         WaifuSelector.WaifuSelector.InitDeathSprite(sprite);
                     }
                 }

             }
         }

        private static Waifu GetCurrentWaifu()
        {
            var menuItem = Menu.Item("waifusharp.options.waifus");

            if (menuItem != null)
            {
                return GetWaifuByName(menuItem.GetValue<StringList>().SelectedValue);
            }

            return WaifuSelector.WaifuSelector.Waifus.FirstOrDefault();
        }

        private static Waifu GetWaifuByName(String name)
        {
            return WaifuSelector.WaifuSelector.Waifus.FirstOrDefault(w => w.Name.ToLower() == name.ToLower());
        }
    }
}
