using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace PewPewQuinn.Helpers
{
    public class Statics
    {
        public const string ChampName = "Quinn";
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q;
        public static Spell E;
        public static Spell W;
        public static Spell R;
        public static SpellSlot Ignite;
        public static readonly Obj_AI_Hero player = ObjectManager.Player;
        public static SoundPlayer welcome = new SoundPlayer(Properties.Resources.Welcome);
    }
}
