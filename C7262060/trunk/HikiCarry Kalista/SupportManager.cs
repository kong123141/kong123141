using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace HikiCarry_Kalista
{
    public static class SupportManager
    {
        private static Obj_AI_Base kalistaSupport = null;
        public static bool drawSupport { get; set; }

        public const string allyBuffName = "kalistacoopstrikeally";

        public static Obj_AI_Base Support
        {
            get
            {
                if (kalistaSupport != null && kalistaSupport.IsValid)
                {
                    return kalistaSupport;
                }
                return null;
            }
        }
        static SupportManager()
        {
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;

            kalistaSupport = ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(x => x.IsAlly && x.HasBuff(allyBuffName));
            if (kalistaSupport != null)
            {
                Console.Write(kalistaSupport.Name);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (drawSupport)
            {
                if (kalistaSupport != null)
                {
                    var signal = Program.Config.Item("signal").GetValue<bool>();
                    var heroPos = Drawing.WorldToScreen(ObjectManager.Player.Position);
                    var circleSupport = Program.Config.Item("circleSupport").GetValue<bool>();
                    if (signal)
                    {
                        if (kalistaSupport.Distance(ObjectManager.Player.Position) < 500 && !ObjectManager.Player.IsDead)
                        {
                            Drawing.DrawText(heroPos.X-100, heroPos.Y, Color.Gold, "Support Connection Signal: Good");
                        }
                        if (kalistaSupport.Distance(ObjectManager.Player.Position) > 500 && kalistaSupport.Distance(ObjectManager.Player.Position) < 1000
                            && !ObjectManager.Player.IsDead)
                        {
                            Drawing.DrawText(heroPos.X-100, heroPos.Y, Color.Gold, "Support Connection Signal: Medium");
                        }
                        if (kalistaSupport.Distance(ObjectManager.Player.Position) > 1000 && kalistaSupport.Distance(ObjectManager.Player.Position) < 1500
                            && !ObjectManager.Player.IsDead)
                        {
                            Drawing.DrawText(heroPos.X-100, heroPos.Y+25, Color.Gold, "Support Connection Signal: Low");
                        }
                        if (kalistaSupport.Distance(ObjectManager.Player.Position) > 1500 && !ObjectManager.Player.IsDead)
                        {
                            Drawing.DrawText(heroPos.X-100, heroPos.Y+25, Color.Gold, "Support Connection Signal: Missed");
                        }
                    }
                    if (circleSupport)
                    {
                        Render.Circle.DrawCircle(kalistaSupport.Position + new Vector3(0, 0, 15), 100, Color.Gold, 5, true);
                    }
                }
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            var saveSupport = Program.Config.Item("saveSupport").GetValue<bool>();
            var supportPercent = Program.Config.Item("savePercent").GetValue<Slider>().Value;

            if (kalistaSupport == null)
            {
                kalistaSupport = ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(x => x.IsAlly && x.HasBuff(allyBuffName));
            }
            if (kalistaSupport != null)
            {
                if (saveSupport && Program.R.IsReady()) // Support Save
                {
                    if (kalistaSupport.HealthPercent < supportPercent)
                    {
                        Program.R.Cast();
                    }
                }
            }
        }

    }
}