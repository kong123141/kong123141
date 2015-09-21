using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;
using SharpDX.Multimedia;
using WaifuSharp.ResourceClasses;

namespace WaifuSharp.WaifuHelper
{
    class Waifu
    {
        public String Name { get; set; }

        public List<OnKillSprite> OnKillPics = new List<OnKillSprite>();

        public List<OnKillSound> OnKillSounds = new List<OnKillSound>();

        public List<OnDeathSprite> OnDeathPics = new List<OnDeathSprite>();

        public List<OnDeathSound> OnDeathSounds = new List<OnDeathSound>();

        public int CurrentLevel = 1;

        public int CurrentExp = 0;

        public bool IsDrawing { get; set; }
    }
}
