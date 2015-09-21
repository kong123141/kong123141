using System;
using System.Media;


namespace PewPewQuinn.Helpers
{
    public class Soundplayer : Statics
    {
        public static void PlaySound(SoundPlayer sound = null)
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
        }
    }
}
