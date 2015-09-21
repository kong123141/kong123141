#region Credits
    //      DamageIndicator  https://github.com/LeagueSharp/LeagueSharp.Common/blob/master/Utility.cs#L691
#endregion Credits
#region Template
/*  
 DamageIndicator.DamageToUnit = GetDamage;
 private float GetDamage(Obj_AI_Hero target)
        {
            var damage = 0d;
            if (Q.IsReady())
            {
                damage += Player.GetSpellDamage(target, SpellSlot.Q);
            }
            return (float)damage;
        }
*/
#endregion Template

using System;
using System.Globalization;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace EndifsCreations.Controller
{    
    internal static class myDamageIndicator
    {
        static myDamageIndicator()
        {
            Drawing.OnDraw += OnDraw;
        }       
        public delegate float DamageToUnitDelegate(Obj_AI_Hero hero);

        private const int XOffset = 10;
        private const int YOffset = 20;
        private const int Width = 103;
        private const int Height = 8;

        private static readonly Render.Text Text = new Render.Text(0, 0, "", 14, new ColorBGRA(255, 0, 0, 255), "Verdana");
        private static readonly Render.Rectangle DamageBar = new Render.Rectangle(0, 0, 1, 8, Color.White);
        private static readonly Render.Line HealthLine = new Render.Line(Vector2.Zero, Vector2.Zero, 1, Color.White);
        public static DamageToUnitDelegate DamageToUnit { get; set; }
        private static bool Enable
        {
            get { return Menu.Item("EC." + ObjectManager.Player.ChampionName + ".Draw").GetValue<bool>(); }
        }
        private static Color LineColor
        {
            get { return Color.Cyan; }
        }
        private static Color FillColor
        {
            get { return Color.LimeGreen; }
        }       
        private static Menu Menu;

        public static void AddToMenu(Menu menu)
        {
            Menu = menu;
            Menu.AddItem(new MenuItem("EC." + ObjectManager.Player.ChampionName + ".Draw", "Enable").SetValue(true));
            Menu.AddItem(new MenuItem("EC." + ObjectManager.Player.ChampionName + ".PredictedHealth", "Predicted Health").SetValue(true));
            Menu.AddItem(new MenuItem("EC." + ObjectManager.Player.ChampionName + ".Fill", "Fill Bar").SetValue(true));
        }

        private static void OnDraw(EventArgs args)
        {
            if (Enable && DamageToUnit != null)
            {
                foreach (var unit in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValid && h.IsHPBarRendered && h.IsEnemy))
                {
                    var HPBarPosition = unit.HPBarPosition;
                    var damage = DamageToUnit(unit);
                    var HypoteticalDamagePercent = Math.Max(0, unit.Health - damage) / unit.MaxHealth;
                    var yPos = HPBarPosition.Y + YOffset;
                    var xPosDamage = HPBarPosition.X + XOffset + Width * HypoteticalDamagePercent;
                    var xPosCurrentHp = HPBarPosition.X + XOffset + Width * unit.Health / unit.MaxHealth;

                    if (damage > unit.Health)
                    {
                        Text.X = (int)HPBarPosition.X + XOffset;
                        Text.Y = (int)HPBarPosition.Y + YOffset;
                        Text.text = "Overkill";
                        Text.OnEndScene();
                    }

                    if (Menu.Item("EC." + ObjectManager.Player.ChampionName + ".PredictedHealth").GetValue<bool>())
                    {
                        HealthLine.Start = new Vector2(xPosDamage, yPos);
                        HealthLine.End = new Vector2(xPosDamage, yPos + Height);
                        HealthLine.Width = 2;
                        HealthLine.Color = LineColor;
                        HealthLine.OnEndScene();
                    }

                    if (Menu.Item("EC." + ObjectManager.Player.ChampionName + ".Fill").GetValue<bool>())
                    {
                        var differenceInHp = xPosCurrentHp - xPosDamage;
                        DamageBar.Color = FillColor;
                        DamageBar.X = (int)(HPBarPosition.X + 9 + (107 * HypoteticalDamagePercent));
                        DamageBar.Y = (int)yPos;
                        DamageBar.Width = (int)Math.Round(differenceInHp);
                        DamageBar.Height = Height;
                        DamageBar.OnEndScene();
                    }
                }
            }
        }
    }
}