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
    public static class BOTRK
    {
        private static Obj_AI_Base kalista = null;
        public static bool hikiBOTRK { get; set; }
        public const string championName = "Kalista";

        public static Obj_AI_Base KalistaBOTRK
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
        static BOTRK()
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
            var useBtork = Program.Config.Item("useBOTRK").GetValue<bool>();
            var theirhp = Program.Config.Item("theirhp").GetValue<Slider>().Value;
            var myhp = Program.Config.Item("myhp").GetValue<Slider>().Value;

            if ( useBtork && ((tar.Health / tar.MaxHealth) < theirhp) && ((ObjectManager.Player.Health / ObjectManager.Player.MaxHealth) < myhp))
            {
                if (Items.CanUseItem(3144))
                {
                    Items.UseItem(3144, tar);
                }
            }
        }

    }
}
