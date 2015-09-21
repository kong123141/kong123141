using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;


namespace HikiCarry_Viktor.Activator
{
    public static class Zhonya
    {
        private static Obj_AI_Base viktor = null;
        public static bool hikiZhonya { get; set; }
        public const string championName = "Viktor";

        public static Obj_AI_Base viktorPotion
        {
            get
            {
                if (viktor != null && viktor.IsValid)
                {
                    return viktor;
                }
                return null;
            }
        }
        static Zhonya()
        {
            Game.OnUpdate += Game_OnGameUpdate;
            viktor = ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(x => x.IsMe && x.CharData.BaseSkinName == championName);
            if (viktor != null)
            {
                Console.Write(viktor.CharData.BaseSkinName);
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            var useZ = Program.Config.Item("useZhonya").GetValue<bool>();
            var zhonyaHP = Program.Config.Item("zhonyaHP").GetValue<Slider>().Value;

            if (useZ && Items.HasItem(3157))
            {
                if (ObjectManager.Player.HealthPercent <= zhonyaHP)
                {
                    if (Items.CanUseItem(3157))
                    {
                        Items.UseItem(3157);
                    }
                }
            }
        }
    }
}