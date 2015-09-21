using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using System.Media;


namespace GagongSyndra
{
    public static class PlaySound
    {
        public static SoundPlayer welcome = new SoundPlayer(Properties.Resources.Welcome);
        public static SoundPlayer ballstotheface = new SoundPlayer(Properties.Resources.BallsToTheFace);
        public static SoundPlayer imkillingthebitch = new SoundPlayer(Properties.Resources.ImKillingTheBitch);
        public static SoundPlayer ohdontyoudare = new SoundPlayer(Properties.Resources.OhDontYouDare);
        public static SoundPlayer ohidiot = new SoundPlayer(Properties.Resources.OhIdiot);
        public static SoundPlayer whosthebitchnow = new SoundPlayer(Properties.Resources.WhosTheBitchNow);
        public static SoundPlayer yourdeadmeatasshole = new SoundPlayer(Properties.Resources.YourDeadMeatAsshole);
        public static SoundPlayer diefucker = new SoundPlayer(Properties.Resources.DieFucker);
        public static SoundPlayer goingsomewhereasshole = new SoundPlayer(Properties.Resources.GoingSomewhereAsshole);
        public static SoundPlayer ilovethisgame = new SoundPlayer(Properties.Resources.ILoveThisGame); 
        public static int LastPlayedSound = 0;

        public static void PlatSounds(SoundPlayer sound = null)
        {
            if (sound != null)
            {
                try
                {
                    sound.Play();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else if (Environment.TickCount - LastPlayedSound > 45000 && Program.Menu.Item("Sound2").GetValue<bool>())
            {
                var rnd = new Random();
                switch (rnd.Next(1, 11))
                {
                    case 1:
                        PlatSounds(imkillingthebitch);
                        break;
                    case 2:
                        PlatSounds(ballstotheface);
                        break;
                    case 3:
                        PlatSounds(diefucker);
                        break;
                    case 4:
                        PlatSounds(goingsomewhereasshole);
                        break;
                    case 5:
                        PlatSounds(ilovethisgame);
                        break;
                    case 6:
                        PlatSounds(imkillingthebitch);
                        break;
                    case 7:
                        PlatSounds(ohdontyoudare);
                        break;
                    case 8:
                        PlatSounds(ohidiot);
                        break;
                    case 9:
                        PlatSounds(whosthebitchnow);
                        break;
                    case 10:
                        PlatSounds(yourdeadmeatasshole);
                        break;
                }
                LastPlayedSound = Environment.TickCount;
            }
        }
    }
}
