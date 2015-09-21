using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;
using Font = SharpDX.Direct3D9.Font;
using Rectangle = SharpDX.Rectangle;
using Color = System.Drawing.Color;
using SharpColor = SharpDX.Color;



namespace VisionSharp
{
    class Program
    {
        private static Obj_AI_Hero Player = ObjectManager.Player;
        public static Menu Config;
        private static Font hikiFont;


        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        
        private static void Game_OnGameLoad(EventArgs args)
        {
            hikiFont = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Tahoma", Height = 15, Weight = FontWeight.Bold, Quality = FontQuality.Antialiased});

            Config = new Menu("Vision#", "Vision#", true);
            {
                Config.AddItem(new MenuItem("vision.status", "Vision# Status").SetValue(new StringList(new[] { "Inactive ", "Active"}, 1)));
                Config.AddItem(new MenuItem("vision.range", "Gang Range").SetValue(new Slider(1500, 0, 3000)));
                Config.AddItem(new MenuItem("waypoint.settings", "Waypoint Settings").SetValue(new StringList(new[] { "Disabled ", "Just Minion", "Minion + Siege Minion", "Minion + Siege Minion + Jungle Mobs" }, 1)));
            }

            var drawMenu = new Menu("Draw Settings", "Draw Settings");
            {
                drawMenu.AddItem(new MenuItem("mColor", "Minion Range Color").SetValue(new Circle(true, Color.White)));
                drawMenu.AddItem(new MenuItem("sColor", "Siege Minion Range Color").SetValue(new Circle(true, Color.Yellow)));
                drawMenu.AddItem(new MenuItem("wColor", "Wizard Minion Range Color").SetValue(new Circle(true, Color.Orange)));
                Config.AddSubMenu(drawMenu);
            }
            Config.AddToMainMenu();
            Drawing.OnDraw += OnDraw;
        }
        public static void DrawText(Font font, String text, int posX, int posY, Color color)
        {
            Rectangle rec = font.MeasureText(null, text, FontDrawFlags.Center);
            font.DrawText(null, text, posX + 1 + rec.X, posY + 1, SharpColor.White);
        }
        private static void wayPoint()
        {
            switch (Config.Item("waypoint.settings").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                   
                    break;
                case 1: // Just Minion
                    foreach (Obj_AI_Minion hero in
               ObjectManager.Get<Obj_AI_Minion>()
                   .Where(minion => minion.IsEnemy && minion.IsVisible && !minion.IsDead && minion.IsValid && minion.IsMoving && minion.CharData.BaseSkinName.Contains("Minion")))
                    {
                        List<Vector2> wpoints = hero.GetWaypoints();
                        int nPoint = wpoints.Count - 1;
                        int lPoint = wpoints.Count - 2;
                        float timer = 0;
                        for (int i = 0; i < nPoint; i++)
                        {
                            Vector3 bPoint = wpoints[i].To3D();
                            Vector3 ePoint = wpoints[i + 1].To3D();
                            timer += bPoint.Distance(ePoint) / ObjectManager.Player.MoveSpeed;
                            Vector2 p1 = Drawing.WorldToScreen(bPoint);
                            Vector2 p2 = Drawing.WorldToScreen(ePoint);
                            if (i != lPoint)
                            {
                                Drawing.DrawLine(p1[0], p1[1], p2[0], p2[1], 2, Color.White);
                            }
                            else
                            {
                                float r = 25 / p2.Distance(p1);
                                var enp = new Vector2(r * p1.X + (1 - r) * p2.X, r * p1.Y + (1 - r) * p2.Y);
                                Drawing.DrawLine(p1[0], p1[1], enp[0], enp[1], 2, Color.White);
                                Render.Circle.DrawCircle(ePoint, 50, Color.Black);
                                Render.Circle.DrawCircle(ePoint, 50, Color.FromArgb(50, Color.Gold), -2);
                                DrawText(hikiFont, timer.ToString("F"), (int)p2[0], (int)p2[1] - 10, Color.White);
                            }
                        }
                    }
                    break;
                case 2: // Minion + Siege Minion
                    foreach (Obj_AI_Minion hero in
               ObjectManager.Get<Obj_AI_Minion>()
                   .Where(minion => minion.IsEnemy && minion.IsVisible && !minion.IsDead && minion.IsValid && minion.IsMoving && minion.CharData.BaseSkinName.Contains("Minion") || minion.CharData.BaseSkinName.Contains("MinionSiege")))
                    {
                        List<Vector2> wpoints = hero.GetWaypoints();
                        int nPoint = wpoints.Count - 1;
                        int lPoint = wpoints.Count - 2;
                        float timer = 0;
                        for (int i = 0; i < nPoint; i++)
                        {
                            Vector3 bPoint = wpoints[i].To3D();
                            Vector3 ePoint = wpoints[i + 1].To3D();
                            timer += bPoint.Distance(ePoint) / ObjectManager.Player.MoveSpeed;
                            Vector2 p1 = Drawing.WorldToScreen(bPoint);
                            Vector2 p2 = Drawing.WorldToScreen(ePoint);
                            if (i != lPoint)
                            {
                                Drawing.DrawLine(p1[0], p1[1], p2[0], p2[1], 2, Color.White);
                            }
                            else
                            {
                                float r = 25 / p2.Distance(p1);
                                var enp = new Vector2(r * p1.X + (1 - r) * p2.X, r * p1.Y + (1 - r) * p2.Y);
                                Drawing.DrawLine(p1[0], p1[1], enp[0], enp[1], 2, Color.White);
                                Render.Circle.DrawCircle(ePoint, 50, Color.Black);
                                Render.Circle.DrawCircle(ePoint, 50, Color.FromArgb(50, Color.Gold), -2);
                                DrawText(hikiFont, timer.ToString("F"), (int)p2[0], (int)p2[1] - 10, Color.White);
                            }
                        }
                    }
                    break;
                case 3:
                    foreach (Obj_AI_Minion hero in
              ObjectManager.Get<Obj_AI_Minion>() // Minion + Siege + Red & Blue & Dragon
                  .Where(minion => minion.IsEnemy && !minion.IsAlly && minion.IsVisible && !minion.IsDead && minion.IsValid && minion.IsMoving
                      && minion.CharData.BaseSkinName.Contains("Minion") || minion.CharData.BaseSkinName.Contains("MinionSiege")
                      || minion.CharData.BaseSkinName.Contains("Dragon") || minion.CharData.BaseSkinName.Contains("SRU_Blue") 
                      || minion.CharData.BaseSkinName.Contains("SRU_Red")))
                    {
                        List<Vector2> wpoints = hero.GetWaypoints();
                        int nPoint = wpoints.Count - 1;
                        int lPoint = wpoints.Count - 2;
                        float timer = 0;
                        for (int i = 0; i < nPoint; i++)
                        {
                            Vector3 bPoint = wpoints[i].To3D();
                            Vector3 ePoint = wpoints[i + 1].To3D();
                            timer += bPoint.Distance(ePoint) / ObjectManager.Player.MoveSpeed;
                            Vector2 p1 = Drawing.WorldToScreen(bPoint);
                            Vector2 p2 = Drawing.WorldToScreen(ePoint);
                            if (i != lPoint)
                            {
                                Drawing.DrawLine(p1[0], p1[1], p2[0], p2[1], 2, Color.White);
                            }
                            else
                            {
                                float r = 25 / p2.Distance(p1);
                                var enp = new Vector2(r * p1.X + (1 - r) * p2.X, r * p1.Y + (1 - r) * p2.Y);
                                Drawing.DrawLine(p1[0], p1[1], enp[0], enp[1], 2, Color.White);
                                Render.Circle.DrawCircle(ePoint, 50, Color.Black);
                                Render.Circle.DrawCircle(ePoint, 50, Color.FromArgb(50, Color.Gold), -2);
                                DrawText(hikiFont, timer.ToString("F"), (int)p2[0], (int)p2[1] - 10, Color.White);
                            }
                        }
                    }
                    break;
            }
        }
        private static void OnDraw(EventArgs args)
        {
            if (Config.Item("vision.status").GetValue<StringList>().SelectedIndex != 0)
            {
                var visionRange = Config.Item("vision.range").GetValue<Slider>().Value;

                var xMinion = MinionManager.GetMinions(visionRange, MinionTypes.All, MinionTeam.Enemy).OrderBy(o => o.Distance(Player.Position)).FirstOrDefault(); 


                if (xMinion.CharData.BaseSkinName.Contains("Minion") && !xMinion.CharData.BaseSkinName.Contains("MinionSiege"))
                {
                    var mColor = Config.Item("mColor").GetValue<Circle>();
                    Render.Circle.DrawCircle(new Vector3(xMinion.Position.X, xMinion.Position.Y, xMinion.Position.Z), 100, mColor.Color, 5, false);
                }
                if (xMinion.CharData.BaseSkinName.Contains("MinionSiege"))
                {
                    var sColor = Config.Item("sColor").GetValue<Circle>();
                    Render.Circle.DrawCircle(new Vector3(xMinion.Position.X, xMinion.Position.Y, xMinion.Position.Z), 450, sColor.Color, 5, false);
                }
                if (xMinion.CharData.BaseSkinName.Contains("Ranged"))
                {
                    var wColor = Config.Item("wColor").GetValue<Circle>();
                    Render.Circle.DrawCircle(new Vector3(xMinion.Position.X, xMinion.Position.Y, xMinion.Position.Z), 600, wColor.Color, 5, false);
                }
                
            }
            if (Config.Item("waypoint.settings").GetValue<StringList>().SelectedIndex != 0)
	        {
                wayPoint();
	        }
        }

    }
}
