using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX.Multimedia;
using WaifuSharp.ResourceClasses;
using WaifuSharp.WaifuHelper;

namespace WaifuSharp
{
    class WaifuSharp
    {
        public static Menu Menu { get; set; }

        private static SoundPlayer sPlayer = new SoundPlayer();
        public static void OnLoad()
        {
            Game.OnNotify += Game_OnNotify;
        }

        static void Game_OnNotify(GameNotifyEventArgs args)
        {
            switch (args.EventId)
            {
                case GameEventId.OnKill:
                    var currentWaifu = GetCurrentWaifu();
                    if (currentWaifu != null)
                    {
                        var currentSprite =
                            GetCurrentWaifu()
                                .OnKillPics.Where(m => m.MinWaifuLevel <= GetCurrentWaifu().CurrentLevel)
                                .ToArray()[new Random().Next(0, GetCurrentWaifu().OnKillPics.Count())];
                        var currentSound = GetCurrentWaifu().OnKillSounds.Where(m => m.MinWaifuLevel <= GetCurrentWaifu().CurrentLevel)
                            .ToArray()[new Random().Next(0, GetCurrentWaifu().OnKillSounds.Count())];

                        WaifuSelector.WaifuSelector.InitKillSprite(currentSprite);
                        if (currentSound != null)
                        {
                            var sSound = currentSound;
                            sPlayer.Stream = new MemoryStream(currentSound.SoundStream, true);
                            sPlayer.Load();
                            if (sPlayer.IsLoadCompleted)
                            {
                                sPlayer.PlaySync();
                            }
                            sPlayer.Dispose();
                            
                            var s = GetCurrentWaifu()
                                .OnKillSounds.FirstOrDefault(m => m.SoundStream == sSound.SoundStream);

                            if (s != null)
                            {
                                s.SoundStream = sSound.SoundStream;

                            }
                            
                        }
                    }
                    break;
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
