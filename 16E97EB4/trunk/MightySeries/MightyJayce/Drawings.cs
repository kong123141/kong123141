using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using SharpDX;

namespace MightyJayce
{
    class Drawings : ConfigMenu
    {
        public static void DrawEvent() //Call OnDraws
        {
            Drawing.OnDraw += Cooldowns;

        }

        private static void Cooldowns(EventArgs args)
        {
            var pos = Drawing.WorldToScreen(ObjectManager.Player.Position);

            //Cannon
            if (Mechanics.Ready_Cannon_Q() == 0 && CannonQ.Level >= 1)
                Drawing.DrawText(pos[0] + 105, pos[1] + 20, System.Drawing.Color.White, "Cannon Q: Ready");
            else if (CannonQ.Level < 1)
                Drawing.DrawText(pos[0] + 105, pos[1] + 20, System.Drawing.Color.White, "Cannon Q: Not Learned Yet");
            else Drawing.DrawText(pos[0] + 105, pos[1] + 20, System.Drawing.Color.White, "Cannon Q: " + Mechanics.Ready_Cannon_Q().ToString("#.##"));

            if (Mechanics.Ready_Cannon_E() == 0 && CannonE.Level >= 1)
                Drawing.DrawText(pos[0] + 105, pos[1] + 35, System.Drawing.Color.White, "Cannon E: Ready");
            else if (CannonE.Level < 1)
                Drawing.DrawText(pos[0] + 105, pos[1] + 35, System.Drawing.Color.White, "Cannon E: Not Learned Yet");
            else Drawing.DrawText(pos[0] + 105, pos[1] + 35, System.Drawing.Color.White, "Cannon E: " + Mechanics.Ready_Cannon_E().ToString("#.##"));

            if (Mechanics.Ready_Cannon_W() == 0 && CannonW.Level >= 1)
                Drawing.DrawText(pos[0] + 105, pos[1] + 50, System.Drawing.Color.White, "Cannon W: Ready");
            else if (CannonW.Level < 1)
                Drawing.DrawText(pos[0] + 105, pos[1] + 50, System.Drawing.Color.White, "Cannon W: Not Learned Yet");
            else Drawing.DrawText(pos[0] + 105, pos[1] + 50, System.Drawing.Color.White, "Cannon W: " + Mechanics.Ready_Cannon_W().ToString("#.##"));

            //Hammer
            if (Mechanics.Ready_Hammer_Q() == 0 && HammerQ.Level >= 1)
                Drawing.DrawText(pos[0] - 210, pos[1] + 20, System.Drawing.Color.White, "Hammer Q: Ready");
            else if (HammerQ.Level < 1)
                Drawing.DrawText(pos[0] - 210, pos[1] + 20, System.Drawing.Color.White, "Hammer Q: Not Learned Yet");
            else Drawing.DrawText(pos[0] - 210, pos[1] + 20, System.Drawing.Color.White, "Hammer Q: " + Mechanics.Ready_Hammer_Q().ToString("#.##"));

            if (Mechanics.Ready_Hammer_E() == 0 && HammerE.Level >= 1)
                Drawing.DrawText(pos[0] - 210, pos[1] + 35, System.Drawing.Color.White, "Hammer E: Ready");
            else if (HammerE.Level < 1)
                Drawing.DrawText(pos[0] - 210, pos[1] + 35, System.Drawing.Color.White, "Hammer E: Not Learned Yet");
            else Drawing.DrawText(pos[0] - 210, pos[1] + 35, System.Drawing.Color.White, "Hammer E: " + Mechanics.Ready_Hammer_E().ToString("#.##"));

            if (Mechanics.Ready_Hammer_W() == 0 && HammerW.Level >= 1)
                Drawing.DrawText(pos[0] - 210, pos[1] + 50, System.Drawing.Color.White, "Hammer W: Ready");
            else if (HammerW.Level < 1)
                Drawing.DrawText(pos[0] - 210, pos[1] + 50, System.Drawing.Color.White, "Hammer W: Not Learned Yet");
            else Drawing.DrawText(pos[0] - 210, pos[1] + 50, System.Drawing.Color.White, "Hammer W: " + Mechanics.Ready_Hammer_W().ToString("#.##"));

            //R

            if (Mechanics.Ready_R() == 0 && R.Level >= 1)
                Drawing.DrawText(pos[0], pos[1] + 50, System.Drawing.Color.White, "R: Ready");
            else Drawing.DrawText(pos[0], pos[1] + 50, System.Drawing.Color.White, "R: " + Mechanics.Ready_R().ToString("#.##"));

        }
    }
}
