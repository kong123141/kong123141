using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace PewPewQuinn.Helpers
{
    internal class Drawings : Statics
    {
        public static void DrawEvent()
        {
            Drawing.OnDraw += OnDraw;
            Drawing.OnDraw += DamageIndicator;
            Drawing.OnDraw += HuDisplay;
            Drawing.OnDraw += Indicator;
        }

        private static void Indicator(EventArgs args)
        {
            var enemies1 = HeroManager.Enemies.ToList();
            var enemies2 = HeroManager.Enemies.Where(e => e.Position.Distance(player.Position) < 3000).ToList();

            foreach (var enemy in enemies1.Where(enemy => enemy.Team != player.Team))
            {
                if (enemy.IsVisible && !enemy.IsDead)
                {
                    if (Config.Item("indicator").GetValue<bool>())
                    {
                        var pos = player.Position +
                                  Vector3.Normalize(enemy.Position - player.Position)*200;
                        var myPos = Drawing.WorldToScreen(pos);
                        pos = player.Position + Vector3.Normalize(enemy.Position - player.Position)*450;
                        var ePos = Drawing.WorldToScreen(pos);

                        var linecolor = Color.LawnGreen;
                        var linecolor2 = Color.LawnGreen;
                        if (enemy.Position.Distance(player.Position) > 3000)
                        {
                            linecolor = Color.LawnGreen;
                        }
                        else if (enemy.Position.Distance(player.Position) < 3000) 
                            linecolor = Color.Red;
                        if (enemies2.Count > 1)
                            linecolor2 = Color.Red;

                        if (Config.Item("indicator").GetValue<bool>())
                            Drawing.DrawLine(myPos.X, myPos.Y, ePos.X, ePos.Y, 2, linecolor);
                            Render.Circle.DrawCircle(player.Position, 200, linecolor2);
                        }
                    }
                }
            }
        

        private static void HuDisplay(EventArgs args)
        {
            if (Config.Item("Draw_Disabled").GetValue<bool>())
                return;

            var target = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Physical);
            var epos = Drawing.WorldToScreen(target.Position);
            if (PewPewQuinn.DiveDmgCalc(target) > target.Health && Config.Item("UseRDE").GetValue<bool>()
                && E.IsReady() && R.IsReady() && player.CountEnemiesInRange(800) <= Config.Item("enear").GetValue<Slider>().Value
                && target.CountEnemiesInRange(1000) <= Config.Item("enear").GetValue<Slider>().Value)
            {
                Drawing.DrawText(epos.X, epos.Y, Color.Yellow, "R-E-IGNITE-R-AA Possible");
            }




            if (Config.Item("HUD").GetValue<bool>())
            {
                if (Config.Item("AutoHarass").GetValue<KeyBind>().Active)
                    Drawing.DrawText(Drawing.Width*0.90f, Drawing.Height*0.68f, System.Drawing.Color.Yellow,
                        "Auto Harass : On");
                else
                    Drawing.DrawText(Drawing.Width*0.90f, Drawing.Height*0.68f, System.Drawing.Color.Tomato,
                        "Auto Harass : Off");


                if (Config.Item("HUD").GetValue<bool>())
                    Drawing.DrawText(Drawing.Width * 0.90f, Drawing.Height * 0.72f, System.Drawing.Color.Yellow, "Minions Killed: " + player.MinionsKilled);

                if (Config.Item("HUD").GetValue<bool>())
                    Drawing.DrawText(Drawing.Width * 0.90f, Drawing.Height * 0.70f, System.Drawing.Color.Yellow, "Total Kills: " + player.ChampionsKilled);

                if (Config.Item("HUD").GetValue<bool>())
                    Drawing.DrawText(Drawing.Width * 0.90f, Drawing.Height * 0.74f, System.Drawing.Color.Yellow, "Total Gold: " + player.GoldTotal);


                if (Config.Item("emgapcloser").GetValue<bool>())
                    Drawing.DrawText(Drawing.Width*0.90f, Drawing.Height*0.66f, System.Drawing.Color.Yellow,
                        "Minion Gapcloser: On");
                else
                    Drawing.DrawText(Drawing.Width*0.90f, Drawing.Height*0.66f, System.Drawing.Color.Tomato,
                        "Minion Gapcloser : Off");
            }

        }

        private static void DamageIndicator(EventArgs args)
        {
            if (Config.Item("Draw_Disabled").GetValue<bool>())
                return;

            int mode = Config.Item("dmgdrawer", true).GetValue<StringList>().SelectedIndex;
            if (mode == 0)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != player.Team))
                {
                    if (enemy.IsVisible && !enemy.IsDead)                                     
                        {

                            var combodamage = (PewPewQuinn.CalcDamage(enemy));

                            var PercentHPleftAfterCombo = (enemy.Health - combodamage)/enemy.MaxHealth;
                            var PercentHPleft = enemy.Health/enemy.MaxHealth;
                            if (PercentHPleftAfterCombo < 0)
                                PercentHPleftAfterCombo = 0;

                            var hpBarPos = enemy.HPBarPosition;
                            hpBarPos.X += 45;
                            hpBarPos.Y += 18;
                            double comboXPos = hpBarPos.X - 36 + (107*PercentHPleftAfterCombo);
                            double currentHpxPos = hpBarPos.X - 36 + (107*PercentHPleft);
                            var diff = currentHpxPos - comboXPos;
                            for (var i = 0; i < diff; i++)
                            {
                                Drawing.DrawLine(
                                    (float) comboXPos + i, hpBarPos.Y + 2, (float) comboXPos + i,
                                    hpBarPos.Y + 10, 1, Config.Item("dmgcolor").GetValue<Circle>().Color);
                                Utility.HpBarDamageIndicator.Enabled = false;
                            }

                        }
                    }
                }
            if (mode == 1)
            {
                    Utility.HpBarDamageIndicator.DamageToUnit = PewPewQuinn.CalcDamage;
                    Utility.HpBarDamageIndicator.Color = Config.Item("dmgcolor").GetValue<Circle>().Color;
                    Utility.HpBarDamageIndicator.Enabled = true;                
            }
            }
        

        private static void OnDraw(EventArgs args)
        {
            if (Config.Item("Draw_Disabled").GetValue<bool>())
                return;

            var orbwalktarget = Orbwalker.GetTarget();
            if (orbwalktarget.IsValidTarget())
                Render.Circle.DrawCircle(orbwalktarget.Position, 80, System.Drawing.Color.Orange);

            if (Config.Item("Draw_Disabled").GetValue<bool>())
                return;

            if (Config.Item("Qdraw").GetValue<Circle>().Active)
                if (Q.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range,
                        Q.IsReady() ? Config.Item("Qdraw").GetValue<Circle>().Color : Color.Red,
                        Config.Item("CircleThickness").GetValue<Slider>().Value);

            if (Config.Item("Edraw").GetValue<Circle>().Active)
                if (E.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range - 1,
                        E.IsReady() ? Config.Item("Edraw").GetValue<Circle>().Color : Color.Red,
                        Config.Item("CircleThickness").GetValue<Slider>().Value);

            if (Config.Item("Rdraw").GetValue<Circle>().Active)
                if (R.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, 1600,
                        R.IsReady() ? Config.Item("Rdraw").GetValue<Circle>().Color : Color.Red,
                        Config.Item("CircleThickness").GetValue<Slider>().Value);


            if (Config.Item("ECdraw").GetValue<Circle>().Active)
                if (E.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position,
                        Config.Item("UseECs").GetValue<Slider>().Value,
                        R.IsReady() ? Config.Item("ECdraw").GetValue<Circle>().Color : Color.Red,
                        Config.Item("CircleThickness").GetValue<Slider>().Value);


            var pos = Drawing.WorldToScreen(ObjectManager.Player.Position);
            if (Config.Item("AutoHarass").GetValue<KeyBind>().Active)
                Drawing.DrawText(pos.X - 50, pos.Y + 30, System.Drawing.Color.Plum, "AutoHarass Enabled");

            foreach (var minion in
                ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget() && minion.IsEnemy &&
                                                                   minion.Distance(player.ServerPosition) <=
                                                                   E.Range))
            {
                var ecastpos = player.ServerPosition.Extend(minion.Position,
                    player.ServerPosition.Distance(minion.Position) -
                    (Orbwalking.GetRealAutoAttackRange(player) + 35 - player.Distance(minion.Position)));

                if (Config.Item("minionespots").GetValue<bool>())
                    Render.Circle.DrawCircle(ecastpos, 15, System.Drawing.Color.Green, 20);
            }
        }
    }
}


