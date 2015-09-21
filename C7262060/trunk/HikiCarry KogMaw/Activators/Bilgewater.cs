using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;


namespace HikiCarry_KogMaw.Activators
{
    public static class Bilgewater
    {
        private static Obj_AI_Base kogmaw = null;
        public static bool hikiBilgewater { get; set; }
        public const string championName = "KogMaw";

        public static Obj_AI_Base kogmawBilgewater
        {
            get
            {
                if (kogmaw != null && kogmaw.IsValid)
                {
                    return kogmaw;
                }
                return null;
            }
        }
        static Bilgewater()
        {
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            kogmaw = ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(x => x.IsMe && x.CharData.BaseSkinName == championName);
            if (kogmaw != null)
            {
                Console.Write(kogmaw.CharData.BaseSkinName);
            }
        }

        private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var tar = (Obj_AI_Hero)target;
            var usebilge = Program.Config.Item("useBilge").GetValue<bool>();
            var theirhpbilge = Program.Config.Item("theirhpbilge").GetValue<Slider>().Value;
            var myhpbilge = Program.Config.Item("myhpbilge").GetValue<Slider>().Value;

            if (usebilge && ((tar.Health / tar.MaxHealth) < theirhpbilge) && ((ObjectManager.Player.Health / ObjectManager.Player.MaxHealth) < myhpbilge))
            {
                if (Items.CanUseItem(3153))
                {
                    Items.UseItem(3153, tar);
                }
            }
        }

    }
}