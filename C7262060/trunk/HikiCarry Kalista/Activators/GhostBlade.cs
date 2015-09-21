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
    public static class Ghostblade
    {
        private static Obj_AI_Base kalista = null;
        public static bool hikiGhostBlade { get; set; }
        public const string championName = "Kalista";

        public static Obj_AI_Base KalistaGhostblade
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
        static Ghostblade()
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

            var usebilge = Program.Config.Item("gBlade").GetValue<bool>();
            if (Items.CanUseItem(3142))
            {
                Items.UseItem(3142);
            }
        }

    }
}
