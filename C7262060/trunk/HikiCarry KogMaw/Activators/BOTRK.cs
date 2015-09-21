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
    public static class BOTRK
    {
        private static Obj_AI_Base kogmaw = null;
        public static bool hikiBOTRK { get; set; }
        public const string championName = "KogMaw";

        public static Obj_AI_Base kogmawBOTRK
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
        static BOTRK()
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
            var useBtork = Program.Config.Item("useBOTRK").GetValue<bool>();
            var theirhp = Program.Config.Item("theirhp").GetValue<Slider>().Value;
            var myhp = Program.Config.Item("myhp").GetValue<Slider>().Value;

            if (useBtork && ((tar.Health / tar.MaxHealth) < theirhp) && ((ObjectManager.Player.Health / ObjectManager.Player.MaxHealth) < myhp))
            {
                if (Items.CanUseItem(3144))
                {
                    Items.UseItem(3144, tar);
                }
            }
        }

    }
}