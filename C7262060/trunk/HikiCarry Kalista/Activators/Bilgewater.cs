using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;


namespace HikiCarry_Kalista.Activators
{
    public static class Bilgewater
    {
        private static Obj_AI_Base kalista = null;
        public static bool hikiBilgewater { get; set; }
        public const string championName = "Kalista";

        public static Obj_AI_Base KalistaBilgewater
        {
            get
            {
                if (kalista != null && kalista.IsValid)
                {
                    return kalista;
                }
                return null;
            }
        }
        static Bilgewater()
        {
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            kalista = ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(x => x.IsMe && x.CharData.BaseSkinName == championName);
            if (kalista != null)
            {
                Console.Write(kalista.CharData.BaseSkinName);
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
